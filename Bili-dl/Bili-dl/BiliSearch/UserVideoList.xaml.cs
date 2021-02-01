using Bili;
using JsonUtil;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BiliSearch
{
    /// <summary>
    /// FavList.xaml 的交互逻辑
    /// </summary>
    public partial class UserVideoList : UserControl
    {
        /// <summary>
        /// Selected delegate.
        /// </summary>
        /// <param name="title">Title of the selected item</param>
        /// <param name="id">Aid/Season-id of the selected item</param>
        public delegate void SelectedDel(string title, long id);
        /// <summary>
        /// Occurs when a Video has been selected.
        /// </summary>
        public event SelectedDel VideoSelected;

        private int Mid;

        public UserVideoList()
        {
            InitializeComponent();
        }

        private CancellationTokenSource cancellationTokenSource;
        public void LoadAsync(int mid, int page, bool init)
        {
            Mid = mid;
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
            ContentViewer.ScrollToHome();
            ContentPanel.Children.Clear();
            PagesBox.Visibility = Visibility.Hidden;

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            LoadingPrompt.Visibility = Visibility.Visible;
            Task task = new Task(() =>
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("mid", mid.ToString());
                dic.Add("ps", "30");
                dic.Add("pn", page.ToString());
                try
                {
                    Json.Value json = BiliApi.GetJsonResult("https://api.bilibili.com/x/space/arc/search", dic, true);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        foreach (Json.Value v in json["data"]["list"]["vlist"])
                        {
                            ResultBox.Video video = new ResultBox.Video(v);
                            ResultVideo resultVideo = new ResultVideo(video);
                            resultVideo.PreviewMouseLeftButtonDown += ResultVideo_PreviewMouseLeftButtonDown;
                            ContentPanel.Children.Add(resultVideo);
                        }
                        if (init)
                        {
                            int pages = (int)Math.Ceiling((double)json["data"]["page"]["count"] / json["data"]["page"]["ps"]);
                            PagesBox.SetPage(pages, 1, true);
                        }
                        PagesBox.Visibility = Visibility.Visible;
                        LoadingPrompt.Visibility = Visibility.Hidden;
                    }));
                }
                catch (Exception)
                {

                }
                Dispatcher.Invoke(new Action(() =>
                {
                    LoadingPrompt.Visibility = Visibility.Hidden;
                }));
            }, cancellationTokenSource.Token);
            task.Start();
        }

        private void ResultVideo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            VideoSelected?.Invoke(((ResultVideo)sender).Title, ((ResultVideo)sender).Aid);
        }

        private void PagesBox_JumpTo(int pagenum)
        {
            LoadAsync(Mid, pagenum, false);
        }
    }
}
