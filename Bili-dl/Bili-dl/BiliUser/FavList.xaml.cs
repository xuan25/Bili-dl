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
            PagesBox.JumpTo += PagesBox_JumpTo;
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

            LoadingPromptList.Visibility = Visibility.Visible;
            PagesBox.Visibility = Visibility.Hidden;

            Task task = new Task(() =>
            {
                Json.Value userinfo = BiliApi.RequestJsonResult("https://api.bilibili.com/x/web-interface/nav", null, false);
                if (cancellationToken.IsCancellationRequested)
                    return;
                if (userinfo["code"] == 0)
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("pn", "1");
                    dic.Add("ps", "100");
                    dic.Add("up_mid", ((uint)userinfo["data"]["mid"]).ToString());
                    dic.Add("is_space", "0");
                    Json.Value json = BiliApi.RequestJsonResult("https://api.bilibili.com/medialist/gateway/base/created", dic, false);
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    if (json["code"] == 0)
                        Dispatcher.Invoke(new Action(() =>
                        {
                            foreach (Json.Value folder in json["data"]["list"])
                            {
                                AddFavFolder(new FavFolder(folder["title"], folder["media_count"], folder["id"]));
                            }
                            AddFavFolder(new FavFolder(FavFolder.SpecialFolder.ToView));
                        }));
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    LoadingPromptList.Visibility = Visibility.Hidden;
                }));
            });
            task.Start();
        }

        private readonly List<FavFolder> FavFolders = new List<FavFolder>();
        private void AddFavFolder(FavFolder favFolder)
        {
            favFolder.Selected += FavFolder_Selected; ;
            FavFolders.Add(favFolder);
            FavFoldersStack.Children.Add(favFolder);
        }

        private void FavFolder_Selected(FavFolder sender)
        {
            foreach (FavFolder f in FavFolders)
            {
                f.InActive();
            }

            sender.Active();
            ShowFolder(sender.Mid, 1, true);
        }

        private void FavItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FavItem favItemSender = (FavItem)sender;
            VideoSelected?.Invoke(favItemSender.Title, favItemSender.Id);
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

            LoadingPromptFolder.Visibility = Visibility.Visible;
            PagesBox.Visibility = Visibility.Hidden;

            if(mediaId >= 0)
            {
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
                    Json.Value json = BiliApi.RequestJsonResult("https://api.bilibili.com/medialist/gateway/base/spaceDetail", dic, false);
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    if (json["code"] == 0)
                    {
                        if(json["data"]["info"]["media_count"] > 0)
                        {
                            Dispatcher.Invoke(new Action(() =>
                            {
                                foreach (Json.Value media in json["data"]["medias"])
                                {
                                    FavItem favItem = new FavItem(media["title"], media["cover"], media["fav_time"], media["id"]);
                                    favItem.PreviewMouseLeftButtonDown += FavItem_PreviewMouseLeftButtonDown;
                                    ContentPanel.Children.Add(favItem);
                                }
                                if (init)
                                    PagesBox.SetPage((int)Math.Ceiling((double)json["data"]["info"]["media_count"] / 20), 1, true);
                                PagesBox.Visibility = Visibility.Visible;
                            }));
                        }
                    }
                    Dispatcher.Invoke(new Action(() =>
                    {
                        LoadingPromptFolder.Visibility = Visibility.Hidden;
                    }));
                });
                task.Start();
            }
            else
            {
                Task task = new Task(() =>
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("pn", pagenum.ToString());
                    dic.Add("ps", "20");
                    Json.Value json = BiliApi.RequestJsonResult("https://api.bilibili.com/x/v2/history/toview/web", dic, false);
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    if (json["code"] == 0)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            if(json["data"]["count"] > 0)
                            {
                                foreach (Json.Value media in json["data"]["list"])
                                {
                                    FavItem favItem = new FavItem(media["title"], media["pic"], media["add_at"], media["aid"]);
                                    favItem.PreviewMouseLeftButtonDown += FavItem_PreviewMouseLeftButtonDown;
                                    ContentPanel.Children.Add(favItem);
                                }
                                if (init)
                                    PagesBox.SetPage((int)Math.Ceiling((double)json["data"]["count"] / 20), 1, true);
                                PagesBox.Visibility = Visibility.Visible;
                            }
                            
                        }));
                    }
                    Dispatcher.Invoke(new Action(() =>
                    {
                        LoadingPromptFolder.Visibility = Visibility.Hidden;
                    }));
                });
                task.Start();
            }
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
