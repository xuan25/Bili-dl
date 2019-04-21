using Bili;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BiliSearch
{
    /// <summary>
    /// SeasonSuggest.xaml 的交互逻辑
    /// </summary>
    public partial class SuggestItemSeason : UserControl
    {
        public SuggestItemSeason(SearchBox.SeasonSuggest seasonSuggest)
        {
            InitializeComponent();

            if(TitleInline.Text != null)
                TitleInline.Text = seasonSuggest.Title;

            InfoInline.Text = string.Format("{0} | {1} | {2}", TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(seasonSuggest.Ptime).Year, seasonSuggest.SeasonTypeName, seasonSuggest.Area);

            if (seasonSuggest.Label != null)
                LabelInline.Text = seasonSuggest.Label;

            this.Loaded += async delegate (object senderD, RoutedEventArgs eD)
            {
                System.Drawing.Bitmap bitmap = await seasonSuggest.GetCoverAsync();
                ImageBox.Source = BiliApi.BitmapToImageSource(bitmap);
            };
        }
    }
}
