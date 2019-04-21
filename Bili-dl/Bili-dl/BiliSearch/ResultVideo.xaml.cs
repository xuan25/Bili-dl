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
    /// SearchResultVideo.xaml 的交互逻辑
    /// </summary>
    public partial class ResultVideo : UserControl
    {
        public long Aid;
        public string Title;

        public ResultVideo(ResultBox.Video video)
        {
            InitializeComponent();

            Aid = video.Aid;
            Title = video.Title;

            TitleBox.Inlines.Clear();
            MatchCollection mc = Regex.Matches(video.Title, "(\\<em.*?\\>(?<Word>.*?)\\</em\\>|.)");
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

            PlayBox.Text = BiliApi.FormatNum(video.Play, 1);
            PostdateBox.Text = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(video.Pubdate).ToString("yyyy-MM-dd");
            AuthorBox.Text = video.Author;

            this.Loaded += async delegate (object senderD, RoutedEventArgs eD)
            {
                System.Drawing.Bitmap bitmap = await video.GetPicAsync();
                ImageBox.Source = BiliApi.BitmapToImageSource(bitmap);
            };
        }

        
    }
}
