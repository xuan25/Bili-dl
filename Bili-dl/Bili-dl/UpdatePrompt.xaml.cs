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
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string result = reader.ReadToEnd();
                reader.Close();
                response.Close();
                dataStream.Close();

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
            ExportResource(UpdaterPath, "Updater.Bili-dl-updater.exe");
            Process.Start(UpdaterPath, string.Format("\"{0}\"", Process.GetCurrentProcess().MainModule.FileName));
        }

        private static void ExportResource(string path, string source)
        {
            string projectName = Assembly.GetExecutingAssembly().GetName().Name.ToString().Replace("-", "_");
            Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(projectName + "." + source);
            FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            resourceStream.CopyTo(fileStream);
            fileStream.Close();
            resourceStream.Close();
        }
    }
}
