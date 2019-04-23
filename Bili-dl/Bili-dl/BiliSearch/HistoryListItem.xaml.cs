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
    /// HistoryListItem.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryListItem : UserControl
    {
        public delegate void RemoveDel(HistoryListItem historyListItem);
        public event RemoveDel Remove;

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
