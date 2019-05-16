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
    /// SearchPanel.xaml 的交互逻辑
    /// </summary>
    public partial class SearchPanel : UserControl
    {
        /// <summary>
        /// Selected delegate.
        /// </summary>
        /// <param name="title">Title of the selected item</param>
        /// <param name="id">Aid/Season-id of the selected item</param>
        public delegate void SelectedDel(string title, long id);
        /// <summary>
        /// Occurs when a Video has been selected.
        /// </summary>
        public event SelectedDel VideoSelected;
        /// <summary>
        /// Occurs when a Season has been selected.
        /// </summary>
        public event SelectedDel SeasonSelected;

        public SearchPanel()
        {
            InitializeComponent();
        }

        public void SetHistory(List<string> history)
        {
            ResultBox.SetHistory(history);
        }

        private void SearchBox_Search(SearchBox sender, string text)
        {
            ResultBox.SearchAsync(text);
        }

        private void ResultBox_HistorySelected(string text)
        {
            SearchBox.Text = text;
        }

        private void ResultBox_VideoSelected(string title, long id)
        {
            VideoSelected?.Invoke(title, id);
        }

        private void ResultBox_SeasonSelected(string title, long id)
        {
            SeasonSelected?.Invoke(title, id);
        }
    }
}
