﻿using Bili;
using ConfigUtil;
using FlvMerge;
using JsonUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace BiliDownload
{
    /// <summary>
    /// Class <c>DownloadTask</c> models a download task.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public class DownloadTask
    {
        public DownloadInfo Info;
        public string Title;
        public string Index;
        public uint Aid;
        public uint Num;
        public uint Cid;
        public string Part;
        public uint Qn;
        public string Description;
        public List<Segment> Segments;
        public enum SegmentType { Video, Audio, Mixed };
        public int Threads;
        public string Pic;
        public bool MergeRequired;
        public bool IsFinished;
        public int CurrentSegment;
        public bool IsRunning;
        public double ProgressPercentage;
        public Thread ProgressMonitorThread;
        private Thread startThread;

        public delegate void FinishedHandler(DownloadTask downloadTask, string filepath);
        public event FinishedHandler Finished;

        public enum Status { Analyzing, Downloading, Merging, Finished, AnalysisFailed };
        public delegate void StatusUpdateDel(DownloadTask downloadTask, double progressPercentage, long bps, Status statues);
        public event StatusUpdateDel StatusUpdate;

        const int MAX_PATH = 260;

        public DownloadTask(DownloadInfo downloadInfo)
        {
            Info = downloadInfo;
            ProgressPercentage = 0;
            IsRunning = false;
            IsFinished = false;

            Title = downloadInfo.Title;
            Index = downloadInfo.Index;
            Aid = downloadInfo.Aid;
            Num = downloadInfo.Num;
            Cid = downloadInfo.Cid;
            Part = downloadInfo.Part;
            Qn = downloadInfo.Qn;
            Description = downloadInfo.Description;
            Threads = downloadInfo.Threads;
            Pic = downloadInfo.Pic;
            MergeRequired = downloadInfo.MergeRequired;
        }

        private bool Analysis()
        {
            StatusUpdate?.Invoke(this, ProgressPercentage, 0, Status.Analyzing);
            Segments = new List<Segment>();
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                { "avid", Aid.ToString() },
                { "cid", Cid.ToString() },
                { "qn", Qn.ToString() }
            };
            try
            {
                Json.Value json = BiliApi.RequestJsonResult("https://api.bilibili.com/x/player/playurl", dic, false);
                if (json["code"] == 0)
                    if (json["data"]["quality"] == Qn)
                        foreach (Json.Value v in json["data"]["durl"])
                        {
                            Segment segment = new Segment(Aid, v["url"], SegmentType.Mixed, v["size"], Threads);
                            segment.Finished += Segment_Finished;
                            Segments.Add(segment);
                        }
                    else
                        return false;
                else
                {
                    json = BiliApi.RequestJsonResult("http://api.bilibili.com/pgc/player/web/playurl", dic, false);
                    if (json["code"] == 0)
                        if (json["result"]["quality"] == Qn)
                            foreach (Json.Value v in json["result"]["durl"])
                            {
                                Segment segment = new Segment(Aid, v["url"], SegmentType.Mixed, v["size"], Threads);
                                segment.Finished += Segment_Finished;
                                Segments.Add(segment);
                            }
                        else
                            return false;
                    else
                        return false;
                }
                return true;
            }
            catch (WebException)
            {
                return false;
            }
        }

        

        private void Segment_Finished()
        {
            if (CurrentSegment < Segments.Count - 1)
            {
                CurrentSegment++;
                Segments[CurrentSegment].Start();
            }
            else
            {
                AbortProgressMonitor();
                ProgressPercentage = 100;
                StatusUpdate?.Invoke(this, ProgressPercentage, 0, Status.Merging);
                string directory = Path.Combine(ConfigManager.GetSettings().DownloadPath, FilenameValidation(string.Format("[{0}]{1}", Description, Title)));
                Directory.CreateDirectory(directory);
                string filename = FilenameValidation($"{Index}-{Part}");
                string filepath = Path.Combine(directory, $"{filename}.{Segments[0].Extention}");

                // valid path length
                if (filepath.Length > MAX_PATH - 1)     // Reserve 1 char for <NULL>
                {
                    int truncateCount = filepath.Length - (MAX_PATH - 1);
                    if(truncateCount > filename.Length - 1)     // Least 1 char for filename
                    {
                        // Should never occur
                        throw new Exception($"无法创建合法文件名，截断失败: {filename} (截断计数: {truncateCount})");
                    }
                    filename = filename.Substring(0, filename.Length - truncateCount);
                    filepath = directory + $"{filename}.{Segments[0].Extention}";
                }

                int count = Segments.Count;
                string[] paths = new string[count];
                for (int i = 0; i < count; i++)
                    paths[i] = Segments[i].Filepath;

                if (MergeRequired)
                    FlvUtil.FlvMerge(paths, filepath);
                else
                    File.Copy(Segments[0].Filepath, filepath, true);


                foreach (string path in paths)
                {
                    File.Delete(path);
                }
                IsFinished = true;
                IsRunning = false;
                StatusUpdate?.Invoke(this, 100, 0, Status.Finished);

                DownloadFinishedNotification.SendNotification(this, filepath);

                Finished?.Invoke(this, filepath);
            }
        }

        private string FilenameValidation(string path)
        {
            StringBuilder stringBuilder = new StringBuilder(path);
            foreach (char c in Path.GetInvalidFileNameChars())
                stringBuilder.Replace(c.ToString(), string.Empty);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Start a task.
        /// </summary>
        public void Start()
        {
            if (IsRunning || IsFinished)
                return;
            IsRunning = true;

            CurrentSegment = -1;
            if (startThread != null)
                startThread.Abort();
            startThread = new Thread(delegate ()
            {
                while (!Analysis())
                {
                    for(int i = 5; i > 0; i--)
                    {
                        StatusUpdate?.Invoke(this, 0, i, Status.AnalysisFailed);
                        Thread.Sleep(1000);
                    }
                }
                CurrentSegment = 0;
                Segments[CurrentSegment].Start();
                StartProgressMonitor();
                startThread = null;
            });
            startThread.Start();
        }

        /// <summary>
        /// Stop a task.
        /// </summary>
        public void Stop()
        {
            if (startThread != null)
            {
                startThread.Abort();
                startThread = null;
            } 
            if (!IsFinished && IsRunning)
            {
                if (CurrentSegment != -1)
                    Segments[CurrentSegment].Abort();
                AbortProgressMonitor();
                IsRunning = false;
            }
        }

        /// <summary>
        /// Clean up temp files for a task.
        /// </summary>
        public void Clean()
        {
            if (!IsFinished)
            {
                Stop();
                if (Segments != null)
                    foreach (Segment segment in Segments)
                        segment.Clean();
            }
        }

        private void StartProgressMonitor()
        {
            AbortProgressMonitor();
            ProgressMonitorThread = new Thread(delegate ()
            {
                ProgressMonitor();
            });
            ProgressMonitorThread.Start();
        }

        private void AbortProgressMonitor()
        {
            if (ProgressMonitorThread != null)
            {
                ProgressMonitorThread.Abort();
                ProgressMonitorThread = null;
            }
                
        }

        private void ProgressMonitor()
        {
            long total = 0;
            foreach (Segment segment in Segments)
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
                StatusUpdate?.Invoke(this, (double)downloaded / total * 100, downloaded - downloadedLast, Status.Downloading);
                downloadedLast = downloaded;
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Class <c>Segment</c> models a download task segment.
        /// Author: Xuan525
        /// Date: 24/04/2019
        /// </summary>
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
            public string Filename
            {
                get
                {
                    string[] urlcomp = Url.Split('?')[0].Split('/');
                    return urlcomp[urlcomp.Length - 1];
                }
            }
            public string Extention
            {
                get
                {
                    string[] namecomp = Filename.Split('.');
                    return namecomp[namecomp.Length - 1];
                }
            }

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
                string directory = ConfigManager.GetSettings().TempPath + "\\";
                Directory.CreateDirectory(directory);
                Filepath = string.Format("{0}{1}", directory, Url.Substring(Url.LastIndexOf('/') + 1, Url.IndexOf('?') - Url.LastIndexOf('/') - 1));
                Threads = threads;
                DownloadThreads = new List<DownloadThread>();
                for (int i = 0; i < threads; i++)
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

            /// <summary>
            /// Start the download.
            /// </summary>
            public void Start()
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
                    downloadThread.Start();
                }
            }

            /// <summary>
            /// Abort the download.
            /// </summary>
            public void Abort()
            {
                foreach (DownloadThread downloadThread in DownloadThreads)
                {
                    downloadThread.Abort();
                }
            }

            /// <summary>
            /// Clean up the temp files of the segment.
            /// </summary>
            public void Clean()
            {
                if (IsFinished)
                    File.Delete(Filepath);
                else
                {
                    Abort();
                    foreach (DownloadThread downloadThread in DownloadThreads)
                        downloadThread.Clean();
                }
            }

            private void DownloadThread_Finished()
            {
                FinishedThreadCount++;
                if (FinishedThreadCount == DownloadThreads.Count)
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
                        using (FileStream fileStream = new FileStream(Filepath, FileMode.Create))
                        {
                            foreach (DownloadThread downloadThread in DownloadThreads)
                            {
                                using (FileStream fileStreamFrom = new FileStream(downloadThread.Filepath, FileMode.Open))
                                {
                                    fileStreamFrom.CopyTo(fileStream);
                                    fileStreamFrom.Flush();
                                }
                                File.Delete(downloadThread.Filepath);
                            }
                        }
                        IsFinished = true;
                        Finished();
                    }
                }
            }

            /// <summary>
            /// Class <c>DownloadThread</c> models a thread of a download task segment.
            /// Author: Xuan525
            /// Date: 24/04/2019
            /// </summary>
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

                /// <summary>
                /// Start the thread.
                /// </summary>
                public void Start()
                {
                    Abort();
                    downloadThread = new Thread(delegate ()
                    {
                        Download();
                    });
                    downloadThread.Start();
                }

                /// <summary>
                /// Abort the thread.
                /// </summary>
                public void Abort()
                {
                    if (downloadThread != null)
                    {
                        downloadThread.Abort();
                        downloadThread = null;
                    }
                    if (fileStream != null)
                    {
                        fileStream.Close();
                        fileStream = null;
                    }
                }

                /// <summary>
                /// Clean up the temp file of the thread.
                /// </summary>
                public void Clean()
                {
                    if (!IsFinished)
                    {
                        Abort();
                        File.Delete(Filepath);
                    }
                }

                private void Download()
                {
                    fileStream = new FileStream(Filepath, FileMode.Append);
                    Position = fileStream.Position;
                    while (Position != Length)
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                        request.ServicePoint.ConnectionLimit = ConnectionLimit;
                        request.Method = "GET";
                        if (BiliApi.Cookies != null)
                        {
                            request.CookieContainer = new CookieContainer();
                            request.CookieContainer.Add(BiliApi.Cookies);
                        }
                        request.Referer = string.Format("https://www.bilibili.com/video/av{0}", Aid);
                        request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.119 Safari/537.36";
                        request.Accept = "*/*";
                        request.Headers.Add("Accept-Encoding", "gzip, deflate");

                        request.AddRange(From + Position, To);

                        try
                        {
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
                        catch (WebException)
                        {
                            Thread.Sleep(Bili_dl.SettingPanel.settings.RetryInterval);
                        }
                        catch (IOException)
                        {

                        }
                    }
                    fileStream.Close();
                    IsFinished = true;
                    Finished();
                }
            }
        }
    }
}
