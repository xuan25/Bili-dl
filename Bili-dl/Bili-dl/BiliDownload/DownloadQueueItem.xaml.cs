using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BiliDownload
{
    /// <summary>
    /// DownloadQueueItem.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public partial class DownloadQueueItem : UserControl
    {
        /// <summary>
        /// Item delegate.
        /// </summary>
        /// <param name="downloadQueueItem">DownloadQueueItem</param>
        public delegate void ItemDel(DownloadQueueItem downloadQueueItem);
        /// <summary>
        /// Occurs when a DownloadQueueItem has been finished.
        /// </summary>
        public event ItemDel Finished;
        /// <summary>
        /// Occurs when a DownloadQueueItem need to be removed.
        /// </summary>
        public event ItemDel Remove;

        public DownloadTask downloadTask;
        public DownloadQueueItem(DownloadTask downloadTask)
        {
            InitializeComponent();

            this.downloadTask = downloadTask;
            Title.Text = downloadTask.Title;
            SubTitle.Text = string.Format("{0}-{1}", downloadTask.Index, downloadTask.Part);
            Quality.Text = downloadTask.Description;
            InfoBox.Text = "等待中...";

            downloadTask.StatusUpdate += DownloadTask_StatusUpdate;
            downloadTask.Finished += DownloadTask_Finished;
        }

        private void DownloadTask_StatusUpdate(DownloadTask downloadTask, double progressPercentage, long bps, DownloadTask.Status status)
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    switch (status)
                    {
                        case DownloadTask.Status.Downloading:
                            InfoBox.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
                            InfoBox.Text = string.Format("{0:0.0}%    {1}    下载中...", progressPercentage, FormatBps(bps));
                            PBar.Value = progressPercentage;
                            break;
                        case DownloadTask.Status.Analyzing:
                            InfoBox.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
                            InfoBox.Text = "正在获取下载地址...";
                            PBar.Value = progressPercentage;
                            break;
                        case DownloadTask.Status.Merging:
                            InfoBox.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
                            InfoBox.Text = "正在完成...";
                            PBar.Value = progressPercentage;
                            break;
                        case DownloadTask.Status.Finished:
                            InfoBox.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
                            InfoBox.Text = "下载完成!!!";
                            PBar.Value = progressPercentage;
                            break;
                        case DownloadTask.Status.AnalysisFailed:
                            InfoBox.Foreground = new SolidColorBrush(Color.FromRgb(0xf2, 0x5d, 0x8e));
                            InfoBox.Text = string.Format("获取下载地址失败，将在{0}秒后重试", bps);
                            break;
                    }
                    
                }));
            }
            catch (TaskCanceledException)
            {

            }
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

        private void DownloadTask_Finished(DownloadTask downloadTask, string filepath)
        {
            Finished?.Invoke(this);
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            Remove?.Invoke(this);
        }
    }
}
