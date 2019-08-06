using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BiliSearch
{
    /// <summary>
    /// HistoryList.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public partial class HistoryList : UserControl
    {
        /// <summary>
        /// Search delegate.
        /// </summary>
        /// <param name="text">search text</param>
        public delegate void SearchDel(string text);
        /// <summary>
        /// Occurs when a search history has been selected.
        /// </summary>
        public event SearchDel Search;

        private Dictionary<HistoryListItem, ListBoxItem> ItemMap;
        private List<string> History;

        public HistoryList()
        {
            InitializeComponent();
            ItemMap = new Dictionary<HistoryListItem, ListBoxItem>();
        }

        /// <summary>
        /// Set the history list.
        /// </summary>
        /// <param name="history">A list of history</param>
        public void SetHistory(List<string> history)
        {
            History = history;
            foreach (string text in history)
                HistoryListBox.Items.Add(CreateItem(text));
            if (History.Count == 0)
                ClearListBtn.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Insert a history
        /// </summary>
        /// <param name="text">history text</param>
        public void Insert(string text)
        {
            ClearListBtn.Visibility = Visibility.Visible;
            foreach (KeyValuePair<HistoryListItem, ListBoxItem> pair in ItemMap)
            {
                if (pair.Key.Text == text)
                {
                    HistoryListBox.Items.Remove(pair.Value);
                    History.Remove(pair.Key.Text);
                }
            }
            History.Insert(0, text);
            HistoryListBox.Items.Insert(0, CreateItem(text));
            ConfigUtil.ConfigManager.SetSearchHistory(History);
        }

        private ListBoxItem CreateItem(string text)
        {
            HistoryListItem historyListItem = new HistoryListItem(text);
            historyListItem.MouseLeftButtonDown += HistoryListItem_MouseLeftButtonDown;
            historyListItem.Remove += HistoryListItem_Remove;
            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = historyListItem;
            ItemMap.Add(historyListItem, listBoxItem);
            return listBoxItem;
        }

        private void HistoryListItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Search?.Invoke(((HistoryListItem)sender).Text);
        }

        private void HistoryListItem_Remove(HistoryListItem historyListItem)
        {
            HistoryListBox.Items.Remove(ItemMap[historyListItem]);
            History.Remove(historyListItem.Text);
            if (History.Count == 0)
                ClearListBtn.Visibility = Visibility.Hidden;
            ConfigUtil.ConfigManager.SetSearchHistory(History);
        }

        private void ClearListBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearListBtn.Visibility = Visibility.Hidden;
            ItemMap.Clear();
            History.Clear();
            HistoryListBox.Items.Clear();
            ConfigUtil.ConfigManager.SetSearchHistory(History);
        }
    }
}
