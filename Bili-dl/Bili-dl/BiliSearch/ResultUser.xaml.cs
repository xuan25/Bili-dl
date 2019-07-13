using Bili;
using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

            ImageBox.Source = new BitmapImage(new Uri(user.Upic));
        }
    }
}
