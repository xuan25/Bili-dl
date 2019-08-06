using Bili;
using JsonUtil;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BiliUser
{
    /// <summary>
    /// FavList.xaml 的交互逻辑
    /// </summary>
    public partial class FavList : UserControl
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

        public FavList()
        {
            InitializeComponent();
        }

        private CancellationTokenSource cancellationTokenSource;
        public void LoadAsync()
        {
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
            ContentViewer.ScrollToHome();
            ContentPanel.Children.Clear();
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            LoadingPrompt.Visibility = Visibility.Visible;
            PagesBox.Visibility = Visibility.Hidden;

            Task task = new Task(() =>
            {
                Json.Value userinfo = BiliApi.GetJsonResult("https://api.bilibili.com/x/web-interface/nav", null, false);
                if (cancellationToken.IsCancellationRequested)
                    return;
                if (userinfo["code"] == 0)
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("mid", ((uint)userinfo["data"]["mid"]).ToString());
                    Json.Value json = BiliApi.GetJsonResult("https://api.bilibili.com/x/space/fav/nav", dic, false);
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    if (json["code"] == 0)
                        Dispatcher.Invoke(new Action(() =>
                        {
                            foreach (Json.Value folder in json["data"]["archive"])
                            {
                                FavItem favItem;
                                if (folder["cover"].Count > 0)
                                    favItem = new FavItem(folder["name"], folder["cover"][0]["pic"], folder["cur_count"], folder["media_id"], true);
                                else
                                    favItem = new FavItem(folder["name"], null, folder["cur_count"], folder["media_id"], true);
                                favItem.PreviewMouseLeftButtonDown += FavItem_PreviewMouseLeftButtonDown;
                                ContentPanel.Children.Add(favItem);
                            }
                        }));
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    LoadingPrompt.Visibility = Visibility.Hidden;
                }));
            });
            task.Start();
        }

        private void FavItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FavItem favItemSender = (FavItem)sender;
            if (favItemSender.IsFolder)
            {
                ShowFolder((int)favItemSender.Id, 1, true);
            }
            else
            {
                VideoSelected?.Invoke(favItemSender.Title, favItemSender.Id);
            }
        }

        private int MediaId;
        private void ShowFolder(int mediaId, int pagenum, bool init)
        {
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
            ContentViewer.ScrollToHome();
            ContentPanel.Children.Clear();
            MediaId = mediaId;
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            LoadingPrompt.Visibility = Visibility.Visible;
            PagesBox.Visibility = Visibility.Hidden;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("media_id", mediaId.ToString());
            dic.Add("pn", pagenum.ToString());
            dic.Add("ps", "20");
            dic.Add("keyword", "");
            dic.Add("order", "mtime");
            dic.Add("type", "0");
            dic.Add("tid", "0");
            dic.Add("jsonp", "jsonp");
            Task task = new Task(() =>
            {
                Json.Value json = BiliApi.GetJsonResult("https://api.bilibili.com/medialist/gateway/base/spaceDetail", dic, false);
                if (cancellationToken.IsCancellationRequested)
                    return;
                if (json["code"] == 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        foreach (Json.Value media in json["data"]["medias"])
                        {
                            FavItem favItem = new FavItem(media["title"], media["cover"], media["fav_time"], media["id"], false);
                            favItem.PreviewMouseLeftButtonDown += FavItem_PreviewMouseLeftButtonDown;
                            ContentPanel.Children.Add(favItem);
                        }
                        if (init)
                            PagesBox.SetPage((int)Math.Ceiling((double)json["data"]["info"]["media_count"] / 20), 1, true);
                        PagesBox.Visibility = Visibility.Visible;
                    }));
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    LoadingPrompt.Visibility = Visibility.Hidden;
                }));
            });
            task.Start();
        }

        private void PagesBox_JumpTo(int pagenum)
        {
            ShowFolder(MediaId, pagenum, false);
        }

        private void FavGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((Grid)this.Parent).Children.Remove(this);
        }

        private void FavPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
