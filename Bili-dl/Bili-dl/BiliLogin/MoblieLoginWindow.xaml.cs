using Bili;
using System;
using System.Drawing;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace BiliLogin
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public partial class MoblieLoginWindow : Window
    {
        /// <summary>
        /// LoggedIn delegate.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="cookies">Identity cookie</param>
        /// <param name="uid">Loged in uid</param>
        public delegate void LoggedInDel(MoblieLoginWindow sender, CookieCollection cookies, uint uid);
        /// <summary>
        /// Occurs when user logged in.
        /// </summary>
        public event LoggedInDel LoggedIn;

        /// <summary>
        /// ConnectionFailed delegate.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="ex">Exception</param>
        public delegate void ConnectionFailedDel(MoblieLoginWindow sender, WebException ex);
        /// <summary>
        /// Occurs when connection failed.
        /// </summary>
        public event ConnectionFailedDel ConnectionFailed;

        /// <summary>
        /// Canceled delegate.
        /// </summary>
        /// <param name="sender">Sender</param>
        public delegate void CanceledDel(MoblieLoginWindow sender);
        /// <summary>
        /// Occurs when login has been canceled.
        /// </summary>
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
