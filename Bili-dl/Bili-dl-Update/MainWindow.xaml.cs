using Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace Bili_dl_Update
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string VerTag = "v0.1.2-alpha";
        public MainWindow()
        {
            InitializeComponent();
        }

        public void DownLoadLatest()
        {
            Console.WriteLine("123");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
