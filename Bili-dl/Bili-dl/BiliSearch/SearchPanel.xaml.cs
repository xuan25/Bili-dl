using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        public delegate void SelectedDel(string title, object id, string type);
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
            ResultBox.SearchAsync(text, 1);
        }

        private void ResultBox_HistorySelected(string text)
        {
            SearchBox.Text = text;
        }

        private void ResultBox_VideoSelected(string title, object id, string type)
        {
            VideoSelected?.Invoke(title, id, type);
        }

        private void ResultBox_SeasonSelected(string title, object id, string type)
        {
            SeasonSelected?.Invoke(title, id, type);
        }

        private void ResultBox_UserSelected(string title, object id, string type)
        {
            UserVideoListBox.Visibility = Visibility.Visible;
            UserVideoListBox.LoadAsync((int)(long)id, 1, true);
        }

        private void UserVideoGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UserVideoListBox.Visibility = Visibility.Hidden;
        }

        private void UserVideoListBox_VideoSelected(string title, long id)
        {
            VideoSelected?.Invoke(title, id, "aid");
        }
    }
}
