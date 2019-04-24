using Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

namespace Bili_dl_updater
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Downloader downloader;
        public string Filepath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] args = (string[])Application.Current.Properties["Args"];
            if (args.Length != 1)
            {
                Close();
                return;
            }
            Filepath = args[0];

            InfoBox.Text = "等待主程序关闭...";

            Update();
        }

        private void Update()
        {
            downloader = new Downloader(Filepath);
            downloader.ProgressUpdated += Downloader_ProgressUpdated;
            downloader.Finished += Downloader_Finished;
            downloader.StartDownloadLatest();
        }

        private void Downloader_Finished()
        {
            Process.Start(Filepath);
            Dispatcher.Invoke(new Action(() =>
            {
                this.Close();
            }));
        }

        private void Downloader_ProgressUpdated(Downloader.Status status, double progress, long bps)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                InfoBox.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
                switch (status)
                {
                    case Downloader.Status.Downloading:
                        InfoBox.Text = string.Format("{0:0.0}%    {1}    下载中...", progress, FormatBps(bps));
                        break;
                    case Downloader.Status.Initializing:
                        InfoBox.Text = "正在初始化...";
                        break;
                    case Downloader.Status.Finishing:
                        InfoBox.Text = "正在完成...";
                        break;
                    case Downloader.Status.Finished:
                        InfoBox.Text = "下载完成!!!";
                        break;
                }
                PBar.Value = progress;
            }));
        }

        private string FormatBps(long bps)
        {
            if (bps < 1024)
                return string.Format("{0:0.0} Byte/s", bps);
            else if (bps < 1024 * 1024)
                return string.Format("{0:0.0} KB/s", (double)bps / 1024);
            else
                return string.Format("{0:0.0} MB/s", (double)bps / (1024 * 1024));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (downloader != null && downloader.IsRunning)
            {
                downloader.CancelDownload();
            }
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
            this.DragMove();
        }
    }

    /// <summary>
    /// A MultiValueConverter for converting width & height to Rect instance.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public class RectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double w = (double)values[0];
            double h = (double)values[1];
            Rect rect;
            if (w > 0 && h > 0)
                rect = new Rect(0, 0, w, h);
            else
                rect = new Rect(0, 0, 1, 1);
            return rect;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// A MultiValueConverter for converting width & height to Rect instance with a 1px offset.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public class BorderRectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double borderThickness = 1;
            double w = (double)values[0] - 2 * borderThickness;
            double h = (double)values[1] - 2 * borderThickness;
            Rect rect;
            if (w > 2 && h > 2)
                rect = new Rect(borderThickness, borderThickness, w, h);
            else
                rect = new Rect(borderThickness, borderThickness, 2, 2);
            return rect;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
