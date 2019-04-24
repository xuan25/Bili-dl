using Bili;
using System.Windows;
using System.Windows.Controls;

namespace BiliSearch
{
    /// <summary>
    /// ResultUser.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public partial class ResultUser : UserControl
    {
        public long Mid;
        public ResultUser(ResultBox.User user)
        {
            InitializeComponent();

            Mid = user.Mid;
            UnameBox.Text = user.Uname;
            VideosBox.Text = BiliApi.FormatNum(user.Videos, 1);
            FansBox.Text = BiliApi.FormatNum(user.Fans, 1);
            UsignBox.Text = user.Usign;

            this.Loaded += async delegate (object senderD, RoutedEventArgs eD)
            {
                System.Drawing.Bitmap bitmap = await user.GetPicAsync();
                ImageBox.Source = BiliApi.BitmapToImageSource(bitmap);
            };
        }
    }
}
