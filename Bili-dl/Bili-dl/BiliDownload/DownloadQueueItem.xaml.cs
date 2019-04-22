using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BiliDownload
{
    /// <summary>
    /// DownloadQueueItem.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadQueueItem : UserControl
    {
        public delegate void ItemDel(DownloadQueueItem downloadQueueItem);
        public event ItemDel Finished;
        public event ItemDel Remove;

        public bool IsRunning;

        public DownloadTask downloadTask;
        public DownloadQueueItem(DownloadTask downloadTask)
        {
            InitializeComponent();

            IsRunning = false;
            this.downloadTask = downloadTask;
            Title.Text = downloadTask.Title;
            SubTitle.Text = string.Format("{0}-{1}", downloadTask.Index, downloadTask.Part);
            Quality.Text = downloadTask.Description;
            InfoBox.Text = "等待中...";
        }

        public void Start()
        {
            downloadTask.StatusUpdate += DownloadTask_StatusUpdate;
            downloadTask.Finished += DownloadTask_Finished;
            downloadTask.AnalysisFailed += DownloadTask_AnalysisFailed;
            downloadTask.Run();
            IsRunning = true;
        }

        private void DownloadTask_AnalysisFailed(DownloadTask downloadTask)
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    InfoBox.Foreground = new SolidColorBrush(Color.FromRgb(0xf2, 0x5d, 0x8e));
                    InfoBox.Text = "获取下载地址失败";
                }));
                for (int i = Bili_dl.SettingPanel.settings.RetryInterval; i > 0; i--)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        InfoBox.Text = string.Format("获取下载地址失败，将在{0}秒后重试", i);
                    }));
                    System.Threading.Thread.Sleep(1000);
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    downloadTask.Run();
                }));
            }
            catch (TaskCanceledException)
            {

            }
            
        }

        private void DownloadTask_StatusUpdate(double progressPercentage, long bps, DownloadTask.Statues statues)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                InfoBox.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
                switch (statues)
                {
                    case DownloadTask.Statues.DownLoading:
                        InfoBox.Text = string.Format("{0:0.0}%    {1}    下载中...", progressPercentage, FormatBps(bps));
                        break;
                    case DownloadTask.Statues.Analyzing:
                        InfoBox.Text = "正在获取下载地址...";
                        break;
                    case DownloadTask.Statues.Merging:
                        InfoBox.Text = "正在完成...";
                        break;
                    case DownloadTask.Statues.Finished:
                        InfoBox.Text = "下载完成!!!";
                        break;
                }
                PBar.Value = progressPercentage;
            }));
        }

        private string FormatBps(long bps)
        {
            if (bps < 1024)
                return string.Format("{0:0.0} Byte/s", bps);
            else if (bps < 1024 * 1024)
                return string.Format("{0:0.0} KB/s", (double)bps / 1024);
            else
                return string.Format("{0:0.0} MB/s", (double)bps / (1024 * 1024));
        }

        private void DownloadTask_Finished(DownloadTask downloadTask)
        {
            Finished?.Invoke(this);
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            //downloadTask.Stop();
            downloadTask.Clean();
            Remove?.Invoke(this);
        }
    }
}
