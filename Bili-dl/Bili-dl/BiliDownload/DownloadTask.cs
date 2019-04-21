using Bili;
using Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BiliDownload
{
    public class DownloadTask
    {
        public DownloadInfo Info;
        public string Title;
        public uint Aid;
        public uint Num;
        public uint Cid;
        public string Part;
        public uint Qn;
        public string Description;
        public List<Segment> Segments;
        public enum SegmentType { Video, Audio, Mixed };
        public int Threads;
        public bool IsFinished;
        public int CurrentSegment;
        public bool IsRunning;
        public double ProgressPercentage;
        public Thread ProgressMonitorThread;

        public delegate void FinishedDel();
        public event FinishedDel Finished;

        public enum Statues { Analyzing, DownLoading, Merging, Finished };
        public delegate void StatusUpdateDel(double progressPercentage, long bps, Statues statues);
        public event StatusUpdateDel StatusUpdate;

        public DownloadTask(DownloadInfo downloadInfo)
        {
            Info = downloadInfo;
            ProgressPercentage = 0;
            IsRunning = false;
            IsFinished = false;

            Title = downloadInfo.Title;
            Aid = downloadInfo.Aid;
            Num = downloadInfo.Num;
            Cid = downloadInfo.Cid;
            Part = downloadInfo.Part;
            Qn = downloadInfo.Qn;
            Description = downloadInfo.Description;
            Threads = downloadInfo.Threads;
        }

        public bool Analysis()
        {
            StatusUpdate?.Invoke(ProgressPercentage, 0, Statues.Analyzing);
            Segments = new List<Segment>();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("avid", Aid.ToString());
            dic.Add("cid", Cid.ToString());
            dic.Add("qn", Qn.ToString());
            IJson json = BiliApi.GetJsonResult("https://api.bilibili.com/x/player/playurl", dic, false);
            if(json.GetValue("data").GetValue("quality").ToLong() == Qn)
            {
                if (json.GetValue("data").Contains("durl"))
                {
                    foreach (IJson v in json.GetValue("data").GetValue("durl"))
                    {
                        Segment segment = new Segment(Aid, Regex.Unescape(v.GetValue("url").ToString()), SegmentType.Mixed, v.GetValue("size").ToLong(), Threads);
                        segment.Finished += Segment_Finished;
                        Segments.Add(segment);
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        private void Segment_Finished()
        {
            if(CurrentSegment < Segments.Count-1)
            {
                CurrentSegment++;
                Segments[CurrentSegment].Download();
            }
            else
            {
                AbortProgressMonitor();
                ProgressPercentage = 100;
                StatusUpdate?.Invoke(ProgressPercentage, 0, Statues.Merging);
                string directory = string.Format("{0}Download\\", AppDomain.CurrentDomain.BaseDirectory);
                Directory.CreateDirectory(directory);
                if (Segments.Count > 1)
                {
                    List<string> paths = new List<string>();
                    foreach (Segment segment in Segments)
                    {
                        paths.Add(segment.Filepath);
                    }
                    Flv.Merge(paths, directory + FilenameValidation(string.Format("[{0}]{1}_{2}-{3}.flv", Description, Title, Num, Part)));
                    foreach (string path in paths)
                    {
                        File.Delete(path);
                    }
                }
                else
                {
                    FileStream fileStreamInput = new FileStream(Segments[0].Filepath, FileMode.Open);
                    FileStream fileStreamOutput = new FileStream(directory + FilenameValidation(string.Format("[{0}]{1}_{2}-{3}.flv", Description, Title, Num, Part)), FileMode.Create);
                    fileStreamInput.CopyTo(fileStreamOutput);
                    fileStreamInput.Close();
                    fileStreamOutput.Close();
                    File.Delete(Segments[0].Filepath);
                }
                IsFinished = true;
                IsRunning = false;
                StatusUpdate?.Invoke(100, 0, Statues.Finished);
                Finished?.Invoke();
            }
        }

        private string FilenameValidation(string path)
        {
            StringBuilder stringBuilder = new StringBuilder(path);
            foreach (char c in Path.GetInvalidFileNameChars())
                stringBuilder.Replace(c.ToString(), string.Empty);
            return stringBuilder.ToString();
        }

        public void Run()
        {
            if (!IsFinished && !IsRunning)
            {
                Analysis();
                CurrentSegment = 0;
                Segments[CurrentSegment].Download();
                StartProgressMonitor();
                IsRunning = true;
            }
        }

        public void Stop()
        {
            if (!IsFinished && IsRunning)
            {
                Segments[CurrentSegment].AbortDownload();
                AbortProgressMonitor();
                IsRunning = false;
            }
        }

        public void Clean()
        {
            if (!IsFinished)
            {
                Stop();
                foreach (Segment segment in Segments)
                    segment.Clean();
            }
                
        }

        public void StartProgressMonitor()
        {
            AbortProgressMonitor();
            ProgressMonitorThread = new Thread(delegate ()
            {
                ProgressMonitor();
            });
            ProgressMonitorThread.Start();
        }

        public void AbortProgressMonitor()
        {
            if (ProgressMonitorThread != null)
                ProgressMonitorThread.Abort();
        }

        public void ProgressMonitor()
        {
            long total = 0;
            foreach(Segment segment in Segments)
            {
                total += segment.Length;
            }
            long downloadedLast = 0;
            foreach (Segment segment in Segments)
            {
                foreach (Segment.DownloadThread thread in segment.DownloadThreads)
                {
                    downloadedLast += thread.Position;
                }
            }
            while (true)
            {
                long downloaded = 0;
                foreach (Segment segment in Segments)
                {
                    if (segment.IsFinished)
                        downloaded += segment.Length;
                    else
                        foreach (Segment.DownloadThread thread in segment.DownloadThreads)
                        {
                            downloaded += thread.Position;
                        }
                }
                StatusUpdate?.Invoke((double)downloaded / total * 100, downloaded - downloadedLast, Statues.DownLoading);
                downloadedLast = downloaded;
                Thread.Sleep(1000);
            }
        }

        public class Segment
        {
            public uint Aid;
            public string Url;
            public SegmentType Type;
            public long Length;
            public string Filepath;
            public int Threads;
            public List<DownloadThread> DownloadThreads;
            public int FinishedThreadCount;
            public bool IsFinished;

            public delegate void FinishedDel();
            public event FinishedDel Finished;

            public Segment(uint aid, string url, SegmentType segmentType, long contentLength, int threads)
            {
                Aid = aid;
                FinishedThreadCount = 0;
                IsFinished = false;
                Url = url;
                Type = segmentType;
                Length = contentLength;
                string directory = string.Format("{0}Temp\\", AppDomain.CurrentDomain.BaseDirectory);
                Directory.CreateDirectory(directory);
                Filepath = string.Format("{0}{1}", directory, Url.Substring(Url.LastIndexOf('/') + 1, Url.IndexOf('?') - Url.LastIndexOf('/') - 1));
                Threads = threads;
                DownloadThreads = new List<DownloadThread>();
                for (int i=0; i < threads; i++)
                {
                    DownloadThread downloadThread;
                    if (i != threads - 1)
                        downloadThread = new DownloadThread(Aid, Filepath + "_" + (i + 1), Url, i * (Length / threads), (i + 1) * (Length / threads) - 1, Threads);
                    else
                        downloadThread = new DownloadThread(Aid, Filepath + "_" + threads, Url, (threads - 1) * (Length / threads), Length - 1, Threads);
                    downloadThread.Finished += DownloadThread_Finished;
                    DownloadThreads.Add(downloadThread);
                }
            }

            public void Download()
            {
                if (File.Exists(Filepath))
                {
                    bool flag = true;
                    foreach (DownloadThread thread in DownloadThreads)
                    {
                        if (File.Exists(thread.Filepath))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        IsFinished = true;
                        Finished();
                        return;
                    }
                }

                FinishedThreadCount = 0;
                foreach (DownloadThread downloadThread in DownloadThreads)
                {
                    downloadThread.StartDownloadThread();
                } 
            }

            public void AbortDownload()
            {
                foreach (DownloadThread downloadThread in DownloadThreads)
                {
                    downloadThread.AbortDownloadThread();
                }
            }

            public void Clean()
            {
                if (IsFinished)
                    File.Delete(Filepath);
                else
                {
                    AbortDownload();
                    foreach (DownloadThread downloadThread in DownloadThreads)
                        downloadThread.Clean();
                }
            }

            private void DownloadThread_Finished()
            {
                FinishedThreadCount++;
                if(FinishedThreadCount == DownloadThreads.Count)
                {
                    bool flag = true;
                    foreach (DownloadThread downloadThread in DownloadThreads)
                    {
                        if (!downloadThread.IsFinished)
                            flag = false;
                        break;
                    }
                    if (flag)
                    {
                        FileStream fileStream = new FileStream(Filepath, FileMode.Create);
                        foreach (DownloadThread downloadThread in DownloadThreads)
                        {
                            FileStream fileStreamFrom = new FileStream(downloadThread.Filepath, FileMode.Open);
                            fileStreamFrom.CopyTo(fileStream);
                            fileStreamFrom.Flush();
                            fileStreamFrom.Close();
                            fileStreamFrom.Dispose();
                            File.Delete(downloadThread.Filepath);
                        }
                        fileStream.Close();
                        IsFinished = true;
                        Finished();
                    }
                }
            }

            public class DownloadThread
            {
                public uint Aid;
                public string Filepath;
                public string Url;
                public long From;
                public long To;
                public long Length;
                public long Position;
                public bool IsFinished;
                public Thread downloadThread;
                public int ConnectionLimit;
                private FileStream fileStream;

                public delegate void FinishedDel();
                public event FinishedDel Finished;

                public DownloadThread(uint aid, string filepath, string url, long from, long to, int connectionLimit)
                {
                    Aid = aid;
                    Filepath = filepath;
                    Url = url;
                    From = from;
                    To = to;
                    Length = To - From + 1;
                    Position = 0;
                    IsFinished = false;
                    ConnectionLimit = connectionLimit;
                }

                public void StartDownloadThread()
                {
                    AbortDownloadThread();
                    downloadThread = new Thread(delegate ()
                    {
                        Download();
                    });
                    downloadThread.Start();
                }

                public void AbortDownloadThread()
                {
                    if (downloadThread != null)
                        downloadThread.Abort();
                    if (fileStream != null)
                        fileStream.Close();
                }

                public void Clean()
                {
                    if (!IsFinished)
                    {
                        AbortDownloadThread();
                        File.Delete(Filepath);
                    }
                }

                public void Download()
                {
                    fileStream = new FileStream(Filepath, FileMode.Append);
                    Position = fileStream.Position;
                    while (Position != Length)
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                        request.ServicePoint.ConnectionLimit = ConnectionLimit;
                        request.Method = "GET";
                        if (BiliApi.CookieCollection != null)
                        {
                            request.CookieContainer = new CookieContainer();
                            request.CookieContainer.Add(BiliApi.CookieCollection);
                        }
                        request.Referer = string.Format("https://www.bilibili.com/video/av{0}", Aid);
                        request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.119 Safari/537.36";
                        request.Accept = "*/*";
                        request.Headers.Add("Accept-Encoding", "gzip, deflate");

                        request.AddRange(From + Position, To);

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        Stream dataStream = response.GetResponseStream();

                        long copied = 0;
                        byte[] buffer = new byte[1024 * 1024 * 10];
                        while (copied != response.ContentLength)
                        {
                            int size = dataStream.Read(buffer, 0, (int)buffer.Length);
                            fileStream.Write(buffer, 0, size);
                            copied += size;
                            Position += size;
                            //Console.WriteLine("{0} / {1} ({2}-{3})", Position, Length, From, To);
                        }
                        response.Close();
                        dataStream.Close();
                    }
                    fileStream.Close();
                    IsFinished = true;
                    Finished();
                }
            }
        }
    }
}
