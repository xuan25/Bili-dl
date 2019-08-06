using JsonUtil;
using System.IO;
using System.Net;
using System.Threading;

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
        public Thread DownloadThread;
        public Thread ProgressMonitorThread;
        public FileStream OutputFileStream;
        public long Position;
        public long Length;

        public Downloader(string filepath)
        {
            Filepath = filepath;
            IsRunning = false;
            Position = 0;
            Length = 0;

            if (File.Exists(Filepath + ".temp"))
                File.Delete(Filepath + ".temp");
        }

        public void CancelDownload()
        {
            AbortDownload();
            if (File.Exists(Filepath + ".temp"))
                File.Delete(Filepath + ".temp");
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
            string downloadUrl = GetLatestDownloadUrl();

            ProgressUpdated?.Invoke(Status.Downloading, 0, 0);
            StartProgressMonitor();
            Download(Filepath + ".temp", downloadUrl);
            AbortProgressMonitor();

            ProgressUpdated?.Invoke(Status.Waiting, 100, 0);
            while (File.Exists(Filepath) && IsFileInUse(Filepath))
            {
                Thread.Sleep(1000);
            }

            ProgressUpdated?.Invoke(Status.Finishing, 100, 0);
            if (File.Exists(Filepath))
                File.Delete(Filepath);
            File.Move(Filepath + ".temp", Filepath);

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
                request.ReadWriteTimeout = 5000;
                request.Method = "GET";
                request.UserAgent = "Bili-dl-updater";
                if (Length != 0)
                    request.AddRange(Position);
                try
                {
                    using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (Length == 0)
                            Length = response.ContentLength;
                        using(Stream dataStream = response.GetResponseStream())
                        {
                            long copied = 0;
                            byte[] buffer = new byte[1024 * 1024 * 10];
                            while (copied != response.ContentLength)
                            {
                                int size = dataStream.Read(buffer, 0, (int)buffer.Length);
                                OutputFileStream.Write(buffer, 0, size);
                                copied += size;
                                Position += size;
                            }
                        }
                    }
                }
                catch (WebException)
                {

                }
                catch (IOException)
                {

                }
            } while (Length == 0 || (Length != 0 && Position != Length));
            OutputFileStream.Close();
        }

        public string GetLatestDownloadUrl()
        {
            while (true)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/xuan525/Bili-dl/releases/latest");
                request.Accept = "application/vnd.github.v3+json";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36";
                try
                {
                    string result;
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        using (Stream dataStream = response.GetResponseStream())
                            using (StreamReader reader = new StreamReader(dataStream))
                                result = reader.ReadToEnd();

                    Json.Value json = Json.Parser.Parse(result);
                    string url = json["assets"][0]["browser_download_url"];
                    return url;
                }
                catch (WebException)
                {
                    Thread.Sleep(5000);
                }
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
                ProgressMonitorThread.Abort();
        }

        private void ProgressMonitor()
        {
            long lastPosition = Position;
            while (true)
            {
                if (Position != 0)
                    ProgressUpdated?.Invoke(Status.Downloading, (double)Position / Length * 100, Position - lastPosition);
                lastPosition = Position;
                Thread.Sleep(1000);
            }
        }
    }
}
