using JsonUtil;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Bili_dl
{
    /// <summary>
    /// UpdatePrompt.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public partial class UpdatePrompt : UserControl
    {
        /// <summary>
        /// NewVersionFound delegate.
        /// </summary>
        public delegate void NewVersionFoundDel();
        /// <summary>
        /// Occurs when a new version has been recieved.
        /// </summary>
        public event NewVersionFoundDel NewVersionFound;

        /// <summary>
        /// Confirmed delegate.
        /// </summary>
        /// <param name="IsUpdate">Is update</param>
        public delegate void ConfirmedDel(bool IsUpdate);
        /// <summary>
        /// Occurs when user comfirmd a sellection.
        /// </summary>
        public event ConfirmedDel Confirmed;

        public static string UpdaterPath;
        public FileStream CheckingFileStream;
        public Thread CheckVersionThread;

        static UpdatePrompt()
        {
            UpdaterPath = AppDomain.CurrentDomain.BaseDirectory + "Bili-dl-updater.exe";
        }

        public UpdatePrompt()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            StartCheckVersion();
        }

        public void StopCheckVersion()
        {
            if (CheckVersionThread != null)
                CheckVersionThread.Abort();
            if (CheckingFileStream != null)
                CheckingFileStream.Close();
        }

        public void StartCheckVersion()
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
                if (!IsLatestVersion())
                {
                    NewVersionFound?.Invoke();
                    Dispatcher.Invoke(new Action(() =>
                    {
                        this.Visibility = Visibility.Visible;
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ((Grid)this.Parent).Children.Remove(this);
                    }));
                }
            });
            CheckVersionThread.Start();
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

        public bool IsLatestVersion()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/xuan525/Bili-dl/releases/latest");
            request.Accept = "application/vnd.github.v3+json";
            request.UserAgent = "Bili-dl";
            try
            {
                string result;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using(Stream stream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                            result = reader.ReadToEnd();

                Json.Value json = Json.Parser.Parse(result);
                string latestTag = json["tag_name"];
                if (Application.Current.FindResource("Version").ToString() == latestTag)
                    return true;
                Dispatcher.Invoke(new Action(() =>
                {
                    InfoBox.Text = ((string)json["body"]).Replace("\\r", "\r").Replace("\\n", "\n");
                }));
                return false;
            }
            catch
            {
                return true;
            }
        }

        private void LaterBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            Confirmed?.Invoke(false);
        }

        private void NowBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RunUpdate();
            }
            catch (UnauthorizedAccessException)
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName, "-update")
                {
                    Verb = "runas"
                };
                Process.Start(processStartInfo);
            }
            Confirmed?.Invoke(true);
        }

        public static void RunUpdate()
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
    }
}
