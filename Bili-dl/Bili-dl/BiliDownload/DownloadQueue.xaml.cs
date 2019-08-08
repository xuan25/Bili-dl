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

            DownloadManager.Appended += DownloadManager_Appended;
            foreach(DownloadTask downloadTask in DownloadManager.TaskList)
            {
                Append(downloadTask);
            }
        }

        private void DownloadManager_Appended(DownloadTask downloadTask)
        {
            Append(downloadTask);
        }

        private void Append(DownloadTask downloadTask)
        {
            foreach (ListBoxItem i in QueueList.Items)
            {
                DownloadQueueItem dqi = (DownloadQueueItem)i.Content;
                if (dqi.downloadTask.Aid == downloadTask.Aid &&
                    dqi.downloadTask.Cid == downloadTask.Cid &&
                    dqi.downloadTask.Qn == downloadTask.Qn)
                    return;
            }
            DownloadQueueItem downloadQueueItem = new DownloadQueueItem(downloadTask);
            downloadQueueItem.Finished += DownloadQueueItem_Finished;
            downloadQueueItem.Remove += DownloadQueueItem_Remove;
            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = downloadQueueItem;
            QueueList.Items.Add(listBoxItem);
            ItemMap.Add(downloadQueueItem, listBoxItem);
        }

        private void DownloadQueueItem_Remove(DownloadQueueItem downloadQueueItem)
        {
            DownloadManager.Remove(downloadQueueItem.downloadTask);
            QueueList.Items.Remove(ItemMap[downloadQueueItem]);
            ItemMap.Remove(downloadQueueItem);
        }

        private void DownloadQueueItem_Finished(DownloadQueueItem downloadQueueItem)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                QueueList.Items.Remove(ItemMap[downloadQueueItem]);
                ItemMap.Remove(downloadQueueItem);
            }));
        }

        private void OpenDownloadDirectoryBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(string.Format("\"{0}\"", Bili_dl.SettingPanel.settings.DownloadPath));
        }

        private void QueuePanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void DownloadQueueGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((Grid)this.Parent).Children.Remove(this);
        }
    }
}
