using Bili;
using System.Windows;
using System.Windows.Controls;

namespace BiliSearch
{
    /// <summary>
    /// UserSuggestItem.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public partial class SuggestItemUser : UserControl
    {
        public SuggestItemUser(SearchBox.UserSuggest userSuggest)
        {
            InitializeComponent();

            if (TitleInline.Text != null)
                TitleInline.Text = userSuggest.Title;

            FansInline.Text = string.Format("{0:0}粉丝", BiliApi.FormatNum(userSuggest.Fans, 1)).PadRight(10, ' ');

            ArchivesInline.Text = string.Format("{0:0}个视频", BiliApi.FormatNum(userSuggest.Archives, 1));

            this.Loaded += async delegate (object senderD, RoutedEventArgs eD)
            {
                System.Drawing.Bitmap bitmap = await userSuggest.GetCoverAsync();
                ImageBox.Source = BiliApi.BitmapToImageSource(bitmap);
            };
        }
    }
}
