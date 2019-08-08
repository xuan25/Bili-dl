using JsonUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Bili_dl
{
    public class UpdateUtil
    {
        /// <summary>
        /// NewVersionFound delegate.
        /// </summary>
        public delegate void NewVersionFoundDel(string description);
        /// <summary>
        /// Occurs when a new version has been recieved.
        /// </summary>
        public static event NewVersionFoundDel NewVersionFound;

        /// <summary>
        /// NewVersionFound delegate.
        /// </summary>
        public delegate void QuitHandler();
        /// <summary>
        /// Occurs when a new version has been recieved.
        /// </summary>
        public static event QuitHandler Quit;

        public static string UpdaterPath { get; private set; }
        public static FileStream CheckingFileStream { get; private set; }
        public static Thread CheckVersionThread { get; private set; }

        static UpdateUtil()
        {
            UpdaterPath = AppDomain.CurrentDomain.BaseDirectory + "Bili-dl-updater.exe";
        }

        public static void StopCheckVersion()
        {
            if (CheckVersionThread != null)
                CheckVersionThread.Abort();
            if (CheckingFileStream != null)
                CheckingFileStream.Close();
        }

        public static void StartCheckVersion()
        {
            CheckVersionThread = new Thread(delegate ()
            {
                if (File.Exists(UpdaterPath))
                {
                    while (IsFileInUse(UpdaterPath))
                    {
                        Thread.Sleep(1000);
                    }
                    File.Delete(UpdaterPath);
                }
                if (!IsLatestVersion(out string description))
                {
                    NewVersionFound?.Invoke(description);
                }
            });
            CheckVersionThread.Start();
        }

        public static bool IsFileInUse(string fileName)
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

        public static bool IsLatestVersion(out string description)
        {
            description = null;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/xuan525/Bili-dl/releases/latest");
            request.Accept = "application/vnd.github.v3+json";
            request.UserAgent = "Bili-dl";
            try
            {
                string result;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                    result = reader.ReadToEnd();

                Json.Value json = Json.Parser.Parse(result);
                string latestTag = json["tag_name"];
                if (Application.Current.FindResource("Version").ToString() == latestTag)
                    return true;
                description = ((string)json["body"]).Replace("\\r", "\r").Replace("\\n", "\n");
                return false;
            }
            catch
            {
                return true;
            }
        }

        public static void RunUpdate()
        {
            try
            {
                using (Stream stream = Application.GetResourceStream(new Uri("/Updater/Bili-dl-updater.exe", UriKind.Relative)).Stream)
                {
                    using (FileStream fileStream = new FileStream(UpdaterPath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
                Process.Start(UpdaterPath, string.Format("\"{0}\"", Process.GetCurrentProcess().MainModule.FileName));
            }
            catch (UnauthorizedAccessException)
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName, "-update")
                {
                    Verb = "runas"
                };
                Process.Start(processStartInfo);
            }
            Quit?.Invoke();
        }
    }
}
