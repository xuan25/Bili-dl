using System;
using System.Collections.Generic;

namespace BiliDownload
{
    public static class DownloadManager
    {
        public static List<DownloadTask> TaskList { get; private set; }

        public delegate void AppendedHandler(DownloadTask downloadTask);
        public static event AppendedHandler Appended;

        static DownloadManager()
        {
            TaskList = new List<DownloadTask>();
        }

        public static bool Append(DownloadTask downloadTask)
        {
            foreach (DownloadTask i in TaskList)
            {
                if (i.Aid == downloadTask.Aid &&
                    i.Cid == downloadTask.Cid &&
                    i.Qn == downloadTask.Qn)
                {
                    return false;
                }
            }

            downloadTask.Finished += DownloadTask_Finished;
            downloadTask.StatusUpdate += DownloadTask_StatusUpdate;
            TaskList.Add(downloadTask);

            if (!TaskList[0].IsRunning)
                TaskList[0].Start();

            ConfigUtil.ConfigManager.AppendDownloadInfo(downloadTask.Info);
            Appended?.Invoke(downloadTask);
            return true;
        }

        private static void DownloadTask_StatusUpdate(DownloadTask downloadTask, double progressPercentage, long bps, DownloadTask.Status statues)
        {
            Console.WriteLine("{0} {1} {2}", downloadTask.Title, progressPercentage, statues);
        }

        private static void DownloadTask_Finished(DownloadTask downloadTask, string filepath)
        {
            Console.WriteLine("Finished");
            TaskList.RemoveAt(0);
            ConfigUtil.ConfigManager.RemoveDownloadInfo(downloadTask.Info);
            if (TaskList.Count > 0)
                TaskList[0].Start();
        }

        public static void Remove(DownloadTask downloadTask)
        {
            downloadTask.Clean();
            TaskList.Remove(downloadTask);
            ConfigUtil.ConfigManager.RemoveDownloadInfo(downloadTask.Info);
        }

        public static void StopAll()
        {
            foreach (DownloadTask downloadTask in TaskList)
                if (downloadTask.IsRunning)
                    downloadTask.Stop();
        }
    }
}
