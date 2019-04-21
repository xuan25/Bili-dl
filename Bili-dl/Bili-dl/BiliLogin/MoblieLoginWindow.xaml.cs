using Bili;
using System;
using System.Drawing;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BiliLogin
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MoblieLoginWindow : Window
    {
        public delegate void LoggedInDel(MoblieLoginWindow sender, CookieCollection cookies, uint uid);
        public event LoggedInDel LoggedIn;

        public delegate void ConnectionFailedDel(MoblieLoginWindow sender, WebException ex);
        public event ConnectionFailedDel ConnectionFailed;

        public delegate void CanceledDel(MoblieLoginWindow sender);
        public event CanceledDel Canceled;

        public MoblieLoginWindow(Window parent)
        {
            InitializeComponent();
            if(parent != null)
                parent.Closed += Parent_Closed;
        }

        private void Parent_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshQRCode();
        }

        private void ContentGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Canceled?.Invoke(this);
            this.Close();
        }

        private void ReloadBtn_Click(object sender, RoutedEventArgs e)
        {
            QrImageBox.Source = null;
            ReloadGrid.Visibility = Visibility.Hidden;
            RefreshQRCode();
        }

        public void RefreshQRCode()
        {
            BiliLoginQR biliLoginQR = new BiliLoginQR(this);
            biliLoginQR.QRImageLoaded += BiliLoginQR_QRImageLoaded;
            biliLoginQR.LoggedIn += BiliLoginQR_LoggedIn;
            biliLoginQR.Timeout += BiliLoginQR_Timeout;
            biliLoginQR.Updated += BiliLoginQR_Updated;
            biliLoginQR.ConnectionFailed += BiliLoginQR_ConnectionFailed;
            biliLoginQR.Begin();
        }

        private void BiliLoginQR_QRImageLoaded(BiliLoginQR sender, Bitmap qrImage)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                QrImageBox.Source = BiliApi.BitmapToImageSource(qrImage);
            }));
        }

        private void BiliLoginQR_LoggedIn(BiliLoginQR sender, CookieCollection cookies, uint uid)
        {
            LoggedIn?.Invoke(this, cookies, uid);
        }

        private void BiliLoginQR_Timeout(BiliLoginQR sender)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                ReloadGrid.Visibility = Visibility.Visible;
            }));
        }

        private void BiliLoginQR_Updated(BiliLoginQR sender)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                QrImageBox.Visibility = Visibility.Visible;
            }));
        }

        private void BiliLoginQR_ConnectionFailed(BiliLoginQR sender, WebException ex)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                QrImageBox.Visibility = Visibility.Hidden;
            }));
            ConnectionFailed?.Invoke(this, ex);
        }
    }
}
