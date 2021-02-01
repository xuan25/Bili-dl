using ConfigUtil;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace BiliDownload
{
    /// <summary>
    /// DownloadOptionPannel.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public partial class DownloadOption : UserControl
    {
        /// <summary>
        /// TaskCreated delegate.
        /// </summary>
        /// <param name="downloadTask">DownloadTask</param>
        public delegate void TaskCreatedDel(DownloadTask downloadTask);
        /// <summary>
        /// Occurs when a DownloadTask has been generated.
        /// </summary>
        public event TaskCreatedDel TaskCreated;

        private Thread showQualitiesThread;

        public DownloadOption()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Show parts/episodes of a video/season.
        /// </summary>
        /// <param name="title">Title of the video/season</param>
        /// <param name="id">Aid/Season-id of the video/season</param>
        /// <param name="isSeason">IsSeason</param>
        public async void ShowParts(string title, uint id, bool isSeason)
        {
            SetTitle(title);
            PageList.Items.Clear();
            QualityList.Items.Clear();
            PartsLoadingPrompt.Visibility = Visibility.Visible;
            VideoInfo videoInfo = await VideoInfo.GetInfoAsync(id, isSeason);
            if (videoInfo != null)
            {
                SetTitle(videoInfo.Title);
                foreach (VideoInfo.Page page in videoInfo.pages)
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.TextTrimming = TextTrimming.WordEllipsis;
                    textBlock.Text = string.Format("{0}-{1}", page.Index, page.Part);

                    ListBoxItem listBoxItem = new ListBoxItem();
                    listBoxItem.Tag = page;
                    listBoxItem.Content = textBlock;
                    PageList.Items.Add(listBoxItem);
                }
            }
            PartsLoadingPrompt.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Show parts/episodes of a video/season.
        /// </summary>
        /// <param name="title">Title of the video/season</param>
        /// <param name="bvid">Bvid of the video/season</param>
        public async void ShowPartsBv(string title, string bvid)
        {
            SetTitle(title);
            PageList.Items.Clear();
            QualityList.Items.Clear();
            PartsLoadingPrompt.Visibility = Visibility.Visible;
            VideoInfo videoInfo = await VideoInfo.GetInfoBvAsync(bvid);
            if (videoInfo != null)
            {
                SetTitle(videoInfo.Title);
                foreach (VideoInfo.Page page in videoInfo.pages)
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.TextTrimming = TextTrimming.WordEllipsis;
                    textBlock.Text = string.Format("{0}-{1}", page.Index, page.Part);

                    ListBoxItem listBoxItem = new ListBoxItem();
                    listBoxItem.Tag = page;
                    listBoxItem.Content = textBlock;
                    PageList.Items.Add(listBoxItem);
                }
            }
            PartsLoadingPrompt.Visibility = Visibility.Hidden;
        }

        private void SetTitle(string title)
        {
            TitleBox.Inlines.Clear();
            MatchCollection mc = Regex.Matches(title, "(\\<em.*?\\>(?<Word>.*?)\\</em\\>|.)");
            foreach (Match m in mc)
            {
                Inline inline = new Run(m.Value);
                if (m.Value.StartsWith("<"))
                {
                    inline = new Run(m.Groups["Word"].Value);
                }
                else
                {
                    inline = new Run(m.Value);
                }
                TitleBox.Inlines.Add(inline);
            }
        }

        private void PageListItem_Selected(object sender, RoutedEventArgs e)
        {
            QualitiesLoadingPrompt.Visibility = Visibility.Visible;
            QualityList.Items.Clear();
            VideoInfo.Page page = (VideoInfo.Page)((ListBoxItem)sender).Tag;
            if (page != null)
            {
                if (showQualitiesThread != null)
                    showQualitiesThread.Abort();
                showQualitiesThread = new Thread(delegate ()
                {
                    ShowQualies(page);
                });
                showQualitiesThread.Start();
            }
        }

        private void ShowQualies(VideoInfo.Page page)
        {
            List<VideoInfo.Page.Quality> qualities = page.GetQualities();
            if (qualities != null)
                Dispatcher.Invoke(new Action(() =>
                {
                    foreach (VideoInfo.Page.Quality quality in qualities)
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.TextTrimming = TextTrimming.WordEllipsis;
                        textBlock.Text = quality.Description;

                        ListBoxItem listBoxItem = new ListBoxItem();
                        listBoxItem.Tag = quality;
                        listBoxItem.Content = textBlock;
                        listBoxItem.IsEnabled = quality.IsAvaliable;
                        QualityList.Items.Add(listBoxItem);
                    }
                    QualitiesLoadingPrompt.Visibility = Visibility.Hidden;
                }));

        }

        private void QualityListItem_Selected(object sender, RoutedEventArgs e)
        {
            DownloadTask downloadTask = new DownloadTask(new DownloadInfo(((VideoInfo.Page.Quality)((ListBoxItem)sender).Tag), ConfigManager.GetSettings().DownloadThreads));
            TaskCreated?.Invoke(downloadTask);
        }

        private void DownloadPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void DownloadOptionGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((Grid)this.Parent).Children.Remove(this);
        }
    }
}
