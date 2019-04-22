using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
    /// DownloadOptionPannel.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadOption : UserControl
    {
        public delegate void TaskCreatedDel(DownloadTask downloadTask);
        public event TaskCreatedDel TaskCreated;

        private Thread showQualitiesThread;

        public DownloadOption()
        {
            InitializeComponent();
        }

        public async void ShowParts(string title, uint id, bool isSeason)
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

            PageList.Items.Clear();
            QualityList.Items.Clear();
            PartsLoadingPrompt.Visibility = Visibility.Visible;
            VideoInfo videoInfo = await VideoInfo.GetInfoAsync(id, isSeason);
            if (videoInfo != null)
            {
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
                PartsLoadingPrompt.Visibility = Visibility.Hidden;
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
            if(qualities != null)
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
            DownloadTask downloadTask = new DownloadTask(new DownloadInfo(((VideoInfo.Page.Quality)((ListBoxItem)sender).Tag), 5));
            TaskCreated?.Invoke(downloadTask);
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
