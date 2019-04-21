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

namespace BiliSearch
{
    /// <summary>
    /// ResultUser.xaml 的交互逻辑
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
