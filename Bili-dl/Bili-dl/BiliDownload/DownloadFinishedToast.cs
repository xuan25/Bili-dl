using Microsoft.Toolkit.Uwp.Notifications;
using Notifications;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using Windows.UI.Notifications;

namespace BiliDownload
{
    public static class DownloadFinishedToast
    {
        public static ToastNotifier SendToast(DownloadTask downloadTask, string filepath)
        {
            string imagefolder = Path.Combine(Bili_dl.SettingPanel.settings.TempPath, "Image");
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

            ToastContent toastContent = new ToastContent()
            {
                Launch = "ignore=",

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Bili-dl下载完成",
                                HintMaxLines = 1
                            },
                            new AdaptiveText()
                            {
                                Text = downloadTask.Title
                            },
                            new AdaptiveText()
                            {
                                Text = string.Format("{0}-{1}    {2}", downloadTask.Index, downloadTask.Part, downloadTask.Description)
                            }
                        },
                        HeroImage = new ToastGenericHeroImage()
                        {
                            Source = imagepath
                        }
                    },
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("打开视频", string.Format("open=\"{0}\"", filepath))
                        {
                            ActivationType = ToastActivationType.Background
                        },

                        new ToastButton("打开文件夹", string.Format("openfolder=\"{0}\"", filepath))
                        {
                            ActivationType = ToastActivationType.Background
                        },

                        new ToastButton("移动至", string.Format("move=\"{0}\"", filepath))
                        {
                            ActivationType = ToastActivationType.Background
                        }
                    }
                },
                Audio = new ToastAudio()
                {
                    Src = new Uri("ms-winsoundevent:Notification.Looping.Call6")
                }
            };

            // Send Toast

            ToastNotifier toastNotifier = NotificationManager.Show(toastContent.GetContent());

            return toastNotifier;
        }
    }
}
