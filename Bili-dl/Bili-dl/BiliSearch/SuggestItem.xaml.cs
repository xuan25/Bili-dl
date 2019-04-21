using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BiliSearch
{
    /// <summary>
    /// SuggestItem.xaml 的交互逻辑
    /// </summary>
    public partial class SuggestItem : UserControl
    {
        public SuggestItem(SearchBox.Suggest suggest)
        {
            InitializeComponent();

            MatchCollection mc = Regex.Matches(Regex.Unescape(suggest.Title), "(\\<em.*?\\>(?<Word>.*?)\\</em\\>|.)");
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
                SuggestBox.Inlines.Add(inline);
            }

            if (suggest.Type != null)
                TypeBox.Text = suggest.Type;
        }
    }
}
