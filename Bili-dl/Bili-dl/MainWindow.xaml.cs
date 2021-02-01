using Bili;
using BiliDownload;
using BiliLogin;
using BiliUser;
using Framework;
using Notifications;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Bili_dl
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SearchBox.VideoSelected += SearchBox_VideoSelected;
            SearchBox.SeasonSelected += SearchBox_SeasonSelected;
        }

        #region Window

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Enable the minimize animation
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;
            WindowLong.SetWindowLong(windowHandle, WindowLong.GWL_STYLE, (WindowLong.GetWindowLong(windowHandle, WindowLong.GWL_STYLE) | WindowLong.WS_CAPTION));

            NotificationManager.Activated += ToastHandler.HandleToast;
            NotificationManager.Install();

            LoadConfig();

            UpdateUtil.NewVersionFound += UpdateUtil_NewVersionFound;
            UpdateUtil.StartCheckVersion();
        }

        private async void LoadConfig()
        {
            ConfigUtil.ConfigManager.Init();

            if (!ConfigUtil.ConfigManager.GetStatementConfirmed())
                StatementGrid.Children.Add(new Statement());

            BiliApi.CookieCollection = ConfigUtil.ConfigManager.GetCookieCollection();

            List<DownloadInfo> infos = ConfigUtil.ConfigManager.GetDownloadInfos();
            foreach (DownloadInfo info in infos)
                DownloadManager.Append(new DownloadTask(info));

            SearchBox.SetHistory(ConfigUtil.ConfigManager.GetSearchHistory());

            if (BiliApi.CookieCollection != null)
            {
                UserInfo userInfo = await UserInfo.GetUserInfoAsync(BiliApi.CookieCollection);
                if (userInfo != null)
                {
                    ShowUserInfo(userInfo);
                    LoginBtn.Content = "登出";
                }
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            // Add an event handler for window messages
            base.OnSourceInitialized(e);
            HwndSource hwndSource = (HwndSource)PresentationSource.FromVisual(this);
            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(this.WndProc));
            }
        }

        protected IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                // handle resize
                case HitTest.WM_NCHITTEST:
                    handled = true;
                    return HitTest.Hit(lParam, this.Top, this.Left, this.ActualHeight, this.ActualWidth);
            }
            return IntPtr.Zero;
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ResizeMode = ResizeMode.NoResize;
            this.DragMove();
            this.ResizeMode = ResizeMode.CanResize;
        }

        #endregion

        #region Login

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (LoginBtn.Content.ToString() == "登录")
            {
                MoblieLoginWindow moblieLoginWindow = new MoblieLoginWindow(this);
                moblieLoginWindow.LoggedIn += MoblieLoginWindow_LoggedIn;
                moblieLoginWindow.Canceled += MoblieLoginWindow_Canceled;
                moblieLoginWindow.Show();
                LoginBtn.Content = "登录中...";
            }
            else if (LoginBtn.Content.ToString() == "登出")
            {
                BiliApi.CookieCollection = null;
                ConfigUtil.ConfigManager.SetCookieCollection(null);
                UserInfoBox.Text = string.Empty;
                UserFaceImage.Source = null;
                ShowFavoritesBtn.Visibility = Visibility.Collapsed;
                LoginBtn.Content = "登录";
            }

        }

        private void MoblieLoginWindow_Canceled(MoblieLoginWindow sender)
        {
            LoginBtn.Content = "登录";
        }

        private void MoblieLoginWindow_LoggedIn(MoblieLoginWindow sender, System.Net.CookieCollection cookies, uint uid)
        {
            Dispatcher.Invoke(new Action(async () =>
            {
                sender.Topmost = false;
                sender.Hide();

                BiliApi.CookieCollection = cookies;
                ConfigUtil.ConfigManager.SetCookieCollection(cookies);

                UserInfo userInfo = await UserInfo.GetUserInfoAsync(BiliApi.CookieCollection);

                if (userInfo != null)
                {
                    ShowUserInfo(userInfo);
                    LoginBtn.Content = "登出";
                }
                sender.Close();

            }));
        }

        private void ShowUserInfo(UserInfo userInfo)
        {
            ShowFavoritesBtn.Visibility = Visibility.Visible;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(userInfo.Uname);
            stringBuilder.Append(string.Format(" [Lv.{0}]", userInfo.CurrentLevel));
            if (userInfo.VipStatus > 0)
                stringBuilder.Append(" [大会员]");
            UserInfoBox.Text = stringBuilder.ToString();

            UserFaceImage.Source = new BitmapImage(new Uri(userInfo.Face));
        }

        #endregion

        #region DownloadOption

        private DownloadOption ShowDownloadOption()
        {
            PopupGrid.Children.Clear();
            DownloadOption downloadOption = new DownloadOption();
            downloadOption.TaskCreated += DownloadOption_TaskCreated;
            PopupGrid.Children.Add(downloadOption);
            return downloadOption;
        }

        private void DownloadOption_TaskCreated(DownloadTask downloadTask)
        {
            if (DownloadManager.Append(downloadTask))
            {
                Prompt.Text = "已添加到下载队列";
                ((System.Windows.Media.Animation.Storyboard)Resources["ShowPrompt"]).Begin();
            }
            else
            {
                Prompt.Text = "已存在于下载队列";
                ((System.Windows.Media.Animation.Storyboard)Resources["ShowPrompt"]).Begin();
            }
        }

        private void SearchBox_VideoSelected(string title, object id, string type)
        {
            DownloadOption downloadOption = ShowDownloadOption();
            switch (type)
            {
                case "aid":
                    downloadOption.ShowParts(title, (uint)(long)id, false);
                    break;
                case "bvid":
                    downloadOption.ShowPartsBv(title, (string)id);
                    break;
            }
            
        }

        private void SearchBox_SeasonSelected(string title, object id, string type)
        {
            DownloadOption downloadOption = ShowDownloadOption();
            downloadOption.ShowParts(title, (uint)(long)id, true);
        }

        #endregion

        #region DownloadQueue

        private void ShowQueueBtn_Click(object sender, RoutedEventArgs e)
        {
            PannelGrid.Children.Clear();
            PannelGrid.Children.Add(new DownloadQueue());
        }

        #endregion

        #region Settings

        private void ShowSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            PannelGrid.Children.Clear();
            PannelGrid.Children.Add(new SettingPanel());
        }

        #endregion

        #region Closing

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DownloadFinishedNotification.DisposeNotifyIcon();
            NotificationManager.Close();
            UpdateUtil.StopCheckVersion();
            DownloadManager.StopAll();
            SettingPanel.Settings settings = ConfigUtil.ConfigManager.GetSettings();
            if (settings.MovedTempPath != null && settings.MovedTempPath != settings.TempPath)
            {
                CopyDirectory(settings.TempPath, settings.MovedTempPath);
                settings.TempPath = settings.MovedTempPath;
                settings.MovedTempPath = null;
                ConfigUtil.ConfigManager.SetSettings(settings);
            }
        }

        public static void CopyDirectory(string sourcePath, string destinationPath)
        {
            System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(sourcePath);
            System.IO.Directory.CreateDirectory(destinationPath);
            foreach (System.IO.FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                string destName = System.IO.Path.Combine(destinationPath, fsi.Name);

                if (fsi is System.IO.FileInfo)
                {
                    System.IO.File.Copy(fsi.FullName, destName, true);
                    System.IO.File.Delete(fsi.FullName);
                }
                else
                {
                    System.IO.Directory.CreateDirectory(destName);
                    CopyDirectory(fsi.FullName, destName);
                }
            }
            System.IO.Directory.Delete(sourcePath);
        }

        #endregion

        #region Statement

        private void StatementConfirmChk_Checked(object sender, RoutedEventArgs e)
        {
            ConfigUtil.ConfigManager.ConfirmStatement();
            StatementGrid.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Update

        private void UpdateUtil_NewVersionFound(string description)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                UpdatePrompt updatePrompt = new UpdatePrompt(description);
                updatePrompt.Confirmed += UpdatePrompt_Confirmed;
                UpdateGrid.Children.Add(updatePrompt);
            }));
        }

        private void UpdatePrompt_Confirmed(bool IsUpdate)
        {
            if (IsUpdate)
                this.Close();
        }

        #endregion

        #region Favourite

        private void ShowFavoritesBtn_Click(object sender, RoutedEventArgs e)
        {
            PannelGrid.Children.Clear();
            FavList favList = new FavList();
            favList.VideoSelected += FavList_VideoSelected;
            favList.LoadAsync();
            PannelGrid.Children.Add(favList);
        }

        private void FavList_VideoSelected(string title, long id)
        {
            DownloadOption downloadOption = ShowDownloadOption();
            downloadOption.ShowParts(title, (uint)id, false);
        }

        #endregion
    }
}
