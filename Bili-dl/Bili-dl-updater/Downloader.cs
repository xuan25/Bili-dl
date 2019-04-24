using Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bili_dl_updater
{
    class Downloader
    {
        public enum Status { Initializing, Waiting, Downloading, Finishing, Finished };
        public delegate void ProgressUpdatedDel(Status status, double progress, long bps);
        public event ProgressUpdatedDel ProgressUpdated;

        public delegate void FinishedDel();
        public event FinishedDel Finished;

        private FileStream CheckingFileStream;

        public string Filepath;
        public bool IsRunning;
        public bool IsBackuped;
        public Thread DownloadThread;
        public Thread ProgressMonitorThread;
        public FileStream OutputFileStream;
        public long Position;
        public long Length;

        public Downloader(string filepath)
        {
            Filepath = filepath;
            IsRunning = false;
            IsBackuped = false;
            Position = 0;
            Length = 0;
        }

        public void CancelDownload()
        {
            AbortDownload();
            if (IsBackuped)
            {
                File.Delete(Filepath);
                File.Move(Filepath + ".backup", Filepath);
                IsBackuped = false;
            }
        }

        public void AbortDownload()
        {
            if (ProgressMonitorThread != null)
                ProgressMonitorThread.Abort();
            if (DownloadThread != null)
                DownloadThread.Abort();
            if (OutputFileStream != null)
                OutputFileStream.Close();
            if (CheckingFileStream != null)
                CheckingFileStream.Close();
            IsRunning = false;
        }

        public void StartDownloadLatest()
        {
            AbortDownload();
            IsRunning = true;
            DownloadThread = new Thread(delegate ()
            {
                while (IsFileInUse(Filepath))
                {
                    Thread.Sleep(1000);
                }
                DownloadLatest();
            });
            DownloadThread.Start();
        }

        public bool IsFileInUse(string fileName)
        {
            bool inUse = true;
            try
            {
                CheckingFileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                inUse = false;
            }
            catch
            {

            }
            finally
            {
                if (CheckingFileStream != null)
                    CheckingFileStream.Close();
            }
            return inUse;
        }

        public void DownloadLatest()
        {
            ProgressUpdated?.Invoke(Status.Initializing, 0, 0);
            File.Move(Filepath, Filepath + ".backup");
            IsBackuped = true;
            string downloadUrl = GetLatestDownloadUrl();

            ProgressUpdated?.Invoke(Status.Downloading, 0, 0);
            StartProgressMonitor();
            Download(Filepath, downloadUrl);
            AbortProgressMonitor();

            ProgressUpdated?.Invoke(Status.Finishing, 100, 0);
            File.Delete(Filepath + ".backup");

            ProgressUpdated?.Invoke(Status.Finished, 100, 0);
            IsRunning = false;
            Finished?.Invoke();
            
        }

        private void Download(string filepath, string downloadUrl)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            OutputFileStream = new FileStream(filepath, FileMode.Append);
            Position = OutputFileStream.Position;
            do
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadUrl);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.119 Safari/537.36";
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Length = response.ContentLength;
                    Stream dataStream = response.GetResponseStream();

                    long copied = 0;
                    byte[] buffer = new byte[1024 * 1024 * 10];
                    while (copied != response.ContentLength)
                    {
                        int size = dataStream.Read(buffer, 0, (int)buffer.Length);
                        OutputFileStream.Write(buffer, 0, size);
                        copied += size;
                        Position += size;
                    }
                    response.Close();
                    dataStream.Close();
                }
                catch (WebException)
                {
                    Thread.Sleep(5000);
                }
                catch (IOException)
                {

                }
            } while (Position != Length);
            OutputFileStream.Close();
        }

        public string GetLatestDownloadUrl()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/xuan525/Bili-dl/releases/latest");
            request.Accept = "application/vnd.github.v3+json";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string result = reader.ReadToEnd();
            reader.Close();
            response.Close();
            dataStream.Close();

            IJson json = JsonParser.Parse(result);
            string url = json.GetValue("assets").GetValue(0).GetValue("browser_download_url").ToString();
            return url;
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
                ProgressMonitorThread.Abort();
        }

        private void ProgressMonitor()
        {
            long lastPosition = Position;
            while (true)
            {
                if(Position != 0)
                    ProgressUpdated?.Invoke(Status.Downloading, (double)Position / Length * 100, Position - lastPosition);
                lastPosition = Position;
                Thread.Sleep(1000);
            }
        }
    }
}
