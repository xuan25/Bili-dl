using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BiliUser
{
    /// <summary>
    /// FavFolder.xaml 的交互逻辑
    /// </summary>
    public partial class FavItem : UserControl
    {
        public long Id;
        public string Title;
        public FavItem(string title, string cover, long info, long id)
        {
            InitializeComponent();

            Id = id;
            Title = title;
            TitleBox.Text = title;
            InfoBox.Text = string.Format("收藏于: {0}", TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(info).ToString("yyyy-MM-dd"));

            if (cover != null)
                ImageBox.Source = new BitmapImage(new Uri(cover));
        }
    }
}
