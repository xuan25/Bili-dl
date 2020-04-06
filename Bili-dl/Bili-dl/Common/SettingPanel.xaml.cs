using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Bili_dl
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public partial class SettingPanel : UserControl
    {
        /// <summary>
        /// Class <c>Settings</c> models settings of the SettingPanel.
        /// Author: Xuan525
        /// Date: 24/04/2019
        /// </summary>
        [Serializable]
        public class Settings
        {
            public string DownloadPath;
            public string TempPath;
            public string MovedTempPath;
            public int RetryInterval;
            public int DownloadThreads;
            public bool ToMp4;
            public bool CreatFolder;

            public Settings()
            {
                DownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Bili-dl");
                TempPath = Path.Combine(Path.GetTempPath(), "Bili-dl");

                RetryInterval = 5;
                DownloadThreads = 5;

                ToMp4 = true;
                CreatFolder = true;
            }
        }

        // Settings instance.
        public static Settings settings;

        public SettingPanel()
        {
            InitializeComponent();
            SetSettings(ConfigUtil.ConfigManager.GetSettings());
        }

        public string CheckFFmpeg()
        {
            string sCom = "ffmpeg -V&exit";
            Process p = new Process();  //设置要启动的应用程序
            p.StartInfo.FileName = "cmd.exe";      //是否使用操作系统shell启动
            p.StartInfo.UseShellExecute = false;          //接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardInput = true;           //输出信息
            p.StartInfo.RedirectStandardOutput = true;           //输出错误
            p.StartInfo.RedirectStandardError = true;           //不显示程序窗口
            p.StartInfo.CreateNoWindow = true;           //启动程序
            p.Start();
            p.StandardInput.WriteLine(sCom);
            p.StandardInput.AutoFlush = true;

            StreamReader reader = p.StandardOutput;
            StreamReader error = p.StandardError;
            string sOutput = reader.ReadToEnd() + error.ReadToEnd();
            p.WaitForExit();
            p.Close();

            string sRet = "需要安装ffmpeg!";
            if (string.IsNullOrEmpty(sOutput))
                return sRet;
            sOutput = sOutput.Substring(sOutput.IndexOf(sCom) + sCom.Length).ToLower();
            if (sOutput.IndexOf("version") > 0 && sOutput.IndexOf("copyright") > 0)
                return "";
            return sRet;
        }

        /// <summary>
        /// Set settings.
        /// </summary>
        /// <param name="settings">settings</param>
        public void SetSettings(Settings settings)
        {
            SettingPanel.settings = settings;

            DownloadPathBox.Text = settings.DownloadPath;
            if (settings.MovedTempPath != null)
                TempPathBox.Text = settings.MovedTempPath;
            else
                TempPathBox.Text = settings.TempPath;
            RetryIntervalBox.Text = settings.RetryInterval.ToString();
            DownloadThreadsBox.Text = settings.DownloadThreads.ToString();

            ToMp4Box.IsChecked = settings.ToMp4;
            CheckFFmpegText.Text = CheckFFmpeg();

            CreatFolderBox.IsChecked = settings.CreatFolder;
        }

        private void SelectDownloadPathBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = settings.DownloadPath,
                Description = "请选择下载目录"
            };
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                settings.DownloadPath = folderBrowserDialog.SelectedPath.ToString();
                DownloadPathBox.Text = folderBrowserDialog.SelectedPath.ToString();
                ConfigUtil.ConfigManager.SetSettings(settings);
            }
        }

        private void SelectTempPathBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = settings.TempPath,
                Description = "请选择一个空白文件夹作为临时文件目录"
            };
            while (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (settings.MovedTempPath != null && folderBrowserDialog.SelectedPath == settings.MovedTempPath)
                    break;
                if (Directory.Exists(folderBrowserDialog.SelectedPath) && folderBrowserDialog.SelectedPath != settings.TempPath)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(folderBrowserDialog.SelectedPath);
                    if (directoryInfo.GetDirectories().Length == 0 && directoryInfo.GetFiles().Length == 0)
                    {
                        settings.MovedTempPath = folderBrowserDialog.SelectedPath.ToString();
                        TempPathBox.Text = folderBrowserDialog.SelectedPath.ToString();
                        ConfigUtil.ConfigManager.SetSettings(settings);
                        break;
                    }
                    MessageBox.Show("请选择一个空目录");
                }
                else
                {
                    settings.MovedTempPath = folderBrowserDialog.SelectedPath.ToString();
                    TempPathBox.Text = folderBrowserDialog.SelectedPath.ToString();
                    ConfigUtil.ConfigManager.SetSettings(settings);
                    break;
                }
            }
        }

        private void ReduceRetryIntervalBtn_Click(object sender, RoutedEventArgs e)
        {
            if (settings.RetryInterval == 3)
                return;
            settings.RetryInterval--;
            RetryIntervalBox.Text = settings.RetryInterval.ToString();
            ConfigUtil.ConfigManager.SetSettings(settings);
        }

        private void IncreaseRetryIntervalBtn_Click(object sender, RoutedEventArgs e)
        {
            settings.RetryInterval++;
            RetryIntervalBox.Text = settings.RetryInterval.ToString();
            ConfigUtil.ConfigManager.SetSettings(settings);
        }

        private void ReduceDownloadThreadsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (settings.DownloadThreads == 1)
                return;
            settings.DownloadThreads--;
            DownloadThreadsBox.Text = settings.DownloadThreads.ToString();
            ConfigUtil.ConfigManager.SetSettings(settings);
        }

        private void IncreaseDownloadThreadsBtn_Click(object sender, RoutedEventArgs e)
        {
            settings.DownloadThreads++;
            DownloadThreadsBox.Text = settings.DownloadThreads.ToString();
            ConfigUtil.ConfigManager.SetSettings(settings);
        }

        private void SettingsGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((Grid)this.Parent).Children.Remove(this);
        }

        private void SettingsPanel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void ToMp4Box_Click(object sender, RoutedEventArgs e)
        {
            settings.ToMp4 = ToMp4Box.IsChecked == true;
            ConfigUtil.ConfigManager.SetSettings(settings);
        }

        private void CreatFolderBox_Click(object sender, RoutedEventArgs e)
        {
            settings.CreatFolder = CreatFolderBox.IsChecked == true;
            ConfigUtil.ConfigManager.SetSettings(settings);
        }
    }
}
