using Bili;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// SearchResultBangumi.xaml 的交互逻辑
    /// </summary>
    public partial class ResultSeason : UserControl
    {
        public long SeasonId;
        public string Title;
        public ResultSeason(ResultBox.Season season)
        {
            InitializeComponent();

            SeasonId = season.SeasonId;
            Title = season.Title;

            TypeBox.Text = season.SeasonTypeName;

            TitleBox.Inlines.Clear();
            MatchCollection mc;
            if (season.Title != null && season.Title != "")
                mc = Regex.Matches(season.Title, "(\\<em.*?\\>(?<Word>.*?)\\</em\\>|.)");
            else
                mc = Regex.Matches(season.OrgTitle, "(\\<em.*?\\>(?<Word>.*?)\\</em\\>|.)");
            foreach (Match m in mc)
            {
                Inline inline = new Run(m.Value);
                if (m.Value.StartsWith("<"))
                {
                    inline = new Run(m.Groups["Word"].Value);
                    inline.Foreground = new SolidColorBrush(Color.FromRgb(0xf2, 0x5d, 0x8e));
                }
                else
                {
                    inline = new Run(m.Value);
                }
                TitleBox.Inlines.Add(inline);
            }

            StylesBox.Text = season.Styles;
            AreasBox.Text = season.Areas;
            PubtimeBox.Text = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(season.Pubtime).ToString("yyyy-MM-dd");
            CvBox.Text = season.Cv.Replace('\n', ' ');
            DescriptionBox.Text = season.Description.Replace('\n', ' ');

            this.Loaded += async delegate (object senderD, RoutedEventArgs eD)
            {
                System.Drawing.Bitmap bitmap = await season.GetCoverAsync();
                ImageBox.Source = BiliApi.BitmapToImageSource(bitmap);
            };
        }
    }
}
