using System.Windows;
using System.Windows.Controls;

namespace BiliSearch
{
    /// <summary>
    /// HistoryListItem.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public partial class HistoryListItem : UserControl
    {
        /// <summary>
        /// Remove delegate.
        /// </summary>
        /// <param name="historyListItem">HistoryListItem</param>
        public delegate void RemoveDel(HistoryListItem historyListItem);
        /// <summary>
        /// Occurs when a search history need to be remove.
        /// </summary>
        public event RemoveDel Remove;

        // The text of the history
        public string Text;

        public HistoryListItem()
        {
            InitializeComponent();
        }

        public HistoryListItem(string text)
        {
            InitializeComponent();
            Text = text;
            HistoryTextBox.Text = text;
        }

        /// <summary>
        /// Set the text of the history
        /// </summary>
        /// <param name="text">text</param>
        public void SetText(string text)
        {
            Text = text;
            HistoryTextBox.Text = text;
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            Remove?.Invoke(this);
        }
    }
}
