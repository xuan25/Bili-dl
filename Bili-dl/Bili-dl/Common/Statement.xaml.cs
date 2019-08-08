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

namespace Bili_dl
{
    /// <summary>
    /// Interaction logic for Statement.xaml
    /// </summary>
    public partial class Statement : UserControl
    {
        public Statement()
        {
            InitializeComponent();
        }

        private void StatementConfirmChk_Checked(object sender, RoutedEventArgs e)
        {
            ConfigUtil.ConfigManager.ConfirmStatement();
            ((Grid)this.Parent).Children.Remove(this);
        }
    }
}
