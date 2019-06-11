using Bili;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace BiliUser
{
    /// <summary>
    /// FavFolder.xaml 的交互逻辑
    /// </summary>
    public partial class FavItem : UserControl
    {
        public bool IsFolder;
        public long Id;
        public string Title;
        public FavItem(string title, string cover, long info, long id, bool isFolder)
        {
            InitializeComponent();

            IsFolder = isFolder;
            Id = id;
            Title = title;
            TitleBox.Text = title;
            if (isFolder)
                InfoBox.Text = string.Format("{0}个视频", info);
            else
                InfoBox.Text = string.Format("收藏于: {0}", TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(info).ToString("yyyy-MM-dd"));

            if (cover != null)
                this.Loaded += async delegate (object senderD, RoutedEventArgs eD)
                {
                    System.Drawing.Bitmap bitmap = await BiliApi.GetImageAsync(cover);
                    ImageBox.Source = BiliApi.BitmapToImageSource(bitmap);
                };
        }
    }
}
