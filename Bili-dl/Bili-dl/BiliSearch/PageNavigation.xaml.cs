using System.Windows;
using System.Windows.Controls;

namespace BiliSearch
{
    /// <summary>
    /// Interaction logic for PageNavigation.xaml
    /// </summary>
    public partial class PageNavigation : UserControl
    {
        public delegate void JumpToDel(int pagenum);
        public event JumpToDel JumpTo;

        private int PageCount;
        private int CurrentPage;
        private bool Auto;

        public PageNavigation()
        {
            InitializeComponent();
        }

        public void SetPage(int pageCount, int currentPage, bool auto)
        {
            PageCount = pageCount;
            CurrentPage = currentPage;
            Auto = auto;

            Wrapper.Children.Clear();

            if (currentPage > 1)
                Wrapper.Children.Add(new Button
                {
                    Style = (Style)Resources["TextButtonStyle"],
                    Content = "上一页",
                    Tag = currentPage - 1
                });

            if (currentPage < 5)
                for (int i = 1; i < currentPage; i++)
                    Wrapper.Children.Add(new Button
                    {
                        Style = (Style)Resources["NumberButtonStyle"],
                        Content = i.ToString(),
                        Tag = i
                    });
            else
            {
                Wrapper.Children.Add(new Button
                {
                    Style = (Style)Resources["NumberButtonStyle"],
                    Content = 1.ToString(),
                    Tag = 1
                });
                Wrapper.Children.Add(new Button
                {
                    Style = (Style)Resources["EllipsisButtonStyle"]
                });
                for (int i = currentPage - 3; i < currentPage; i++)
                    Wrapper.Children.Add(new Button
                    {
                        Style = (Style)Resources["NumberButtonStyle"],
                        Content = i.ToString(),
                        Tag = i
                    });
            }

            Wrapper.Children.Add(new Button
            {
                Style = (Style)Resources["HighlightedNumberButtonStyle"],
                Content = currentPage.ToString(),
                Tag = currentPage
            });

            if (currentPage > pageCount - 5)
                for (int i = currentPage + 1; i < pageCount + 1; i++)
                    Wrapper.Children.Add(new Button
                    {
                        Style = (Style)Resources["NumberButtonStyle"],
                        Content = i.ToString(),
                        Tag = i
                    });
            else
            {

                for (int i = currentPage + 1; i < currentPage + 4; i++)
                    Wrapper.Children.Add(new Button
                    {
                        Style = (Style)Resources["NumberButtonStyle"],
                        Content = i.ToString(),
                        Tag = i
                    });
                Wrapper.Children.Add(new Button
                {
                    Style = (Style)Resources["EllipsisButtonStyle"]
                });
                Wrapper.Children.Add(new Button
                {
                    Style = (Style)Resources["NumberButtonStyle"],
                    Content = pageCount.ToString(),
                    Tag = pageCount
                });
            }

            if (currentPage < pageCount)
                Wrapper.Children.Add(new Button
                {
                    Style = (Style)Resources["TextButtonStyle"],
                    Content = "下一页",
                    Tag = currentPage + 1
                });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int page = (int)((Button)sender).Tag;
            if (Auto)
                SetPage(PageCount, page, Auto);
            JumpTo?.Invoke(page);
        }
    }
}
