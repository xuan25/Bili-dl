using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BiliDownload
{
    /// <summary>
    /// DownloadQueue.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public partial class DownloadQueue : UserControl
    {
        private Dictionary<DownloadQueueItem, ListBoxItem> ItemMap;
        public DownloadQueue()
        {
            InitializeComponent();
            ItemMap = new Dictionary<DownloadQueueItem, ListBoxItem>();
        }

        /// <summary>
        /// Append a DownloadTask into the queue.
        /// </summary>
        /// <param name="downloadTask">DownloadTask</param>
        /// <returns>Successful</returns>
        public bool Append(DownloadTask downloadTask)
        {
            foreach (ListBoxItem i in QueueList.Items)
            {
                DownloadQueueItem dqi = (DownloadQueueItem)i.Content;
                if (dqi.downloadTask.Aid == downloadTask.Aid &&
                    dqi.downloadTask.Cid == downloadTask.Cid &&
                    dqi.downloadTask.Qn == downloadTask.Qn)
                {
                    return false;
                }

            }
            DownloadQueueItem downloadQueueItem = new DownloadQueueItem(downloadTask);
            downloadQueueItem.Finished += DownloadQueueItem_Finished;
            downloadQueueItem.Remove += DownloadQueueItem_Remove;
            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = downloadQueueItem;
            QueueList.Items.Add(listBoxItem);
            ItemMap.Add(downloadQueueItem, listBoxItem);
            if (!((DownloadQueueItem)((ListBoxItem)QueueList.Items[0]).Content).IsRunning)
                ((DownloadQueueItem)((ListBoxItem)QueueList.Items[0]).Content).Start();

            ConfigManager.ConfigManager.AppendDownloadInfo(downloadTask.Info);
            return true;
        }

        private void DownloadQueueItem_Remove(DownloadQueueItem downloadQueueItem)
        {
            QueueList.Items.Remove(ItemMap[downloadQueueItem]);
            ItemMap.Remove(downloadQueueItem);
            if (QueueList.Items.Count > 0 && !((DownloadQueueItem)((ListBoxItem)QueueList.Items[0]).Content).IsRunning)
                ((DownloadQueueItem)((ListBoxItem)QueueList.Items[0]).Content).Start();
            ConfigManager.ConfigManager.RemoveDownloadInfo(downloadQueueItem.downloadTask.Info);
        }

        private void DownloadQueueItem_Finished(DownloadQueueItem downloadQueueItem)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                QueueList.Items.Remove(ItemMap[downloadQueueItem]);
                ItemMap.Remove(downloadQueueItem);
                if (QueueList.Items.Count > 0)
                    ((DownloadQueueItem)((ListBoxItem)QueueList.Items[0]).Content).Start();
            }));
            ConfigManager.ConfigManager.RemoveDownloadInfo(downloadQueueItem.downloadTask.Info);
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        public void StopAll()
        {
            foreach (ListBoxItem listBoxItem in QueueList.Items)
                if (((DownloadQueueItem)listBoxItem.Content).downloadTask.IsRunning)
                    ((DownloadQueueItem)listBoxItem.Content).downloadTask.Stop();
        }

        private void OpenDownloadDirectoryBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", Bili_dl.SettingPanel.settings.DownloadPath);
        }
    }
}
