using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BiliUser
{
    /// <summary>
    /// Interaction logic for FavFolder.xaml
    /// </summary>
    public partial class FavFolder : UserControl
    {
        public delegate void SelectedHandler(FavFolder sender);
        public event SelectedHandler Selected;

        public int Mid { get; set; }

        private SolidColorBrush TitleDefaultBrush = new SolidColorBrush(Colors.Black);

        public FavFolder(string title, int count, int mid)
        {
            InitializeComponent();

            Mid = mid;

            TitleBox.Text = title;
            if(count >= 0)
                CountBox.Text = count.ToString();
        }

        public enum SpecialFolder { ToView = -1 };

        public FavFolder(SpecialFolder specialFolder)
        {
            InitializeComponent();

            switch (specialFolder)
            {
                case SpecialFolder.ToView:
                    Mid = (int)SpecialFolder.ToView;
                    TitleBox.Text = "稍后再看";
                    TitleDefaultBrush = new SolidColorBrush(Color.FromRgb(153, 162, 170));
                    TitleBox.Foreground = TitleDefaultBrush;
                    break;
            }
        }

        public void SetTitleColor()
        {

        }

        public void Active()
        {
            MainGrid.Background = new SolidColorBrush(Color.FromRgb(0, 161, 214));
            TitleBox.Foreground = new SolidColorBrush(Colors.White);
            CountBox.Foreground = new SolidColorBrush(Colors.White);
        }

        public void InActive()
        {
            MainGrid.Background = new SolidColorBrush(Colors.Transparent);
            TitleBox.Foreground = TitleDefaultBrush;
            CountBox.Foreground = new SolidColorBrush(Color.FromRgb(153, 162, 170));
        }

        private void MainGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Selected?.Invoke(this);
        }
    }
}
