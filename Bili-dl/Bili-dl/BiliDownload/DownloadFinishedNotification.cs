using ConfigUtil;
using Notifications;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security;
using System.Windows;
using Windows.UI.Notifications;

namespace BiliDownload
{
    public static class DownloadFinishedNotification
    {

        public static void SendNotification(DownloadTask downloadTask, string filepath)
        {
            if (NotificationManager.Available)
                ShowToast(downloadTask, filepath);
            else
                ShowBalloonTip(downloadTask);
        }

        public static void DisposeNotifyIcon()
        {
            if (!NotificationManager.Available && Application.Current.Resources.Contains("NotifyIcon"))
                ((System.Windows.Forms.NotifyIcon)Application.Current.Resources["NotifyIcon"]).Dispose();
        }

        private static void ShowBalloonTip(DownloadTask downloadTask)
        {
            if (!Application.Current.Resources.Contains("NotifyIcon"))
                Application.Current.Resources.Add("NotifyIcon", new System.Windows.Forms.NotifyIcon
                {
                    Icon = Icon.ExtractAssociatedIcon(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName),
                    Visible = true
                });
            System.Windows.Forms.NotifyIcon notifyIcon = (System.Windows.Forms.NotifyIcon)Application.Current.Resources["NotifyIcon"];
            notifyIcon.ShowBalloonTip(5000, "Bili-dl下载完成", string.Format("{0}\n{1}-{2}    {3}", downloadTask.Title, downloadTask.Index, downloadTask.Part, downloadTask.Description), System.Windows.Forms.ToolTipIcon.Info);
        }

        private static void ShowToast(DownloadTask downloadTask, string filepath)
        {
            SendToast(downloadTask, filepath);
        }

        private static ToastNotifier SendToast(DownloadTask downloadTask, string filepath)
        {
            string imagefolder = Path.Combine(ConfigManager.GetSettings().TempPath, "Image");
            string imagename = downloadTask.Pic.Substring(downloadTask.Pic.LastIndexOf('/') + 1);
            string imagepath = Path.Combine(imagefolder, imagename);

            // Clean workspace

            if (Directory.Exists(imagefolder))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(imagefolder);
                DateTime datetime = DateTime.UtcNow.AddDays(-3);
                foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                {
                    if (fileInfo.CreationTime.CompareTo(datetime) < 0)
                    {
                        File.Delete(fileInfo.FullName);
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(imagefolder);
            }

            // Download Image

            Image image = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadTask.Pic);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream dataStream = response.GetResponseStream())
                {
                    image = Image.FromStream(dataStream);
                }
            }

            // Resize Image

            int width = 364;
            int height = 180;
            double scale = (double)height / image.Height;
            int resizedWidth = (int)(image.Width * scale);
            int xOffset = (width - resizedWidth) / 2;

            Image resizedImage = null;
            using (Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.DrawImage(image, new Rectangle(xOffset, 0, resizedWidth, height));
                }
                resizedImage = new Bitmap(bitmap);
            }
            resizedImage.Save(imagepath);

            // Create Toast

            string toastXml;
            using (StreamReader streamReader = new StreamReader(Application.GetResourceStream(new Uri("/BiliDownload/DownloadFinishedToast.xml", UriKind.Relative)).Stream))
            {
                toastXml = string.Format(streamReader.ReadToEnd(), SecurityElement.Escape(downloadTask.Title), SecurityElement.Escape(string.Format("{0}-{1}    {2}", downloadTask.Index, downloadTask.Part, downloadTask.Description)), SecurityElement.Escape(imagepath), SecurityElement.Escape(filepath));
            }

            // Send Toast

            ToastNotifier toastNotifier = NotificationManager.Show(toastXml);

            return toastNotifier;
        }
    }
}
