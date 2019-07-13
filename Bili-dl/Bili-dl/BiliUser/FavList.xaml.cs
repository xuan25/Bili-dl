using Bili;
using Json;
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
                IJson userinfo = BiliApi.GetJsonResult("https://api.bilibili.com/x/web-interface/nav", null, false);
                if (cancellationToken.IsCancellationRequested)
                    return;
                if (userinfo.GetValue("code").ToLong() == 0)
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("mid", userinfo.GetValue("data").GetValue("mid").ToLong().ToString());
                    IJson json = BiliApi.GetJsonResult("https://api.bilibili.com/x/space/fav/nav", dic, false);
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    if (json.GetValue("code").ToLong() == 0)
                        Dispatcher.Invoke(new Action(() =>
                        {
                            foreach (IJson folder in json.GetValue("data").GetValue("archive"))
                            {
                                FavItem favItem;
                                if (folder.GetValue("Cover").Contains(0))
                                    favItem = new FavItem(folder.GetValue("name").ToString(), folder.GetValue("cover").GetValue(0).GetValue("pic").ToString(), folder.GetValue("cur_count").ToLong(), folder.GetValue("media_id").ToLong(), true);
                                else
                                    favItem = new FavItem(folder.GetValue("name").ToString(), null, folder.GetValue("cur_count").ToLong(), folder.GetValue("media_id").ToLong(), true);
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
                IJson json = BiliApi.GetJsonResult("https://api.bilibili.com/medialist/gateway/base/spaceDetail", dic, false);
                if (cancellationToken.IsCancellationRequested)
                    return;
                if (json.GetValue("code").ToLong() == 0)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        foreach (IJson media in json.GetValue("data").GetValue("medias"))
                        {
                            FavItem favItem = new FavItem(media.GetValue("title").ToString(), media.GetValue("cover").ToString(), media.GetValue("fav_time").ToLong(), media.GetValue("id").ToLong(), false);
                            favItem.PreviewMouseLeftButtonDown += FavItem_PreviewMouseLeftButtonDown;
                            ContentPanel.Children.Add(favItem);
                        }
                        if (init)
                            PagesBox.SetPage((int)Math.Ceiling((double)json.GetValue("data").GetValue("info").GetValue("media_count").ToLong() / 20), 1, true);
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
    }
}
