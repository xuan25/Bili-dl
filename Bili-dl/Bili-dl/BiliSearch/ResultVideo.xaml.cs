using Bili;
using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BiliSearch
{
    /// <summary>
    /// SearchResultVideo.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
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

            ImageBox.Source = new BitmapImage(new Uri(video.Pic));
        }


    }
}
