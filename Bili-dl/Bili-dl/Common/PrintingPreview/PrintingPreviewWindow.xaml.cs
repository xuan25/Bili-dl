using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace PrintingPreview
{
    /// <summary>
    /// Interaction logic for PrintingPreviewWindow.xaml
    /// </summary>
    public partial class PrintingPreviewWindow : Window
    {
        public FlowDocument FlowDocument { get; private set; }

        public PrintingPreviewWindow(FlowDocument flowDocument)
        {
            InitializeComponent();

            FlowDocument = flowDocument;
            PreviewViewer.Document = FlowDocument;

            this.Loaded += PrintingPreviewWindow_Loaded;
            this.PaddingSettingBox.Loaded += PaddingSettingBox_Loaded;
        }

        private class Printer
        {
            public PrintQueue PrintQueue { get; private set; }

            public string Name
            {
                get
                {
                    return PrintQueue.Name;
                }
            }

            public string FullName
            {
                get
                {
                    return PrintQueue.FullName;
                }
            }

            public IReadOnlyCollection<PageMediaSize> PageMediaSizeCapability
            {
                get
                {
                    return PrintQueue.GetPrintCapabilities().PageMediaSizeCapability;
                }
            }

            public Printer(PrintQueue printQueue)
            {
                PrintQueue = printQueue;
            }

            public override string ToString()
            {
                return FullName;
            }
        }

        private void PrintingPreviewWindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<Printer> printers = new List<Printer>();

            PrintServer printServer = new PrintServer();
            PrintQueueCollection printQueues = printServer.GetPrintQueues();
            string defaultFullName = LocalPrintServer.GetDefaultPrintQueue().FullName;
            Printer defaultPrinter = null;

            foreach (PrintQueue printQueue in printQueues)
            {
                Printer printer = new Printer(printQueue);
                printers.Add(printer);
                if (printer.FullName == defaultFullName)
                    defaultPrinter = printer;
            }

            printers.Reverse();

            PrintersCombo.ItemsSource = printers;

            PrintersCombo.SelectionChanged -= PrintersCombo_SelectionChanged;
            PrintersCombo.SelectionChanged += PrintersCombo_SelectionChanged;

            if (defaultPrinter != null)
                PrintersCombo.SelectedItem = defaultPrinter;
            else
                PrintersCombo.SelectedIndex = 0;
        }

        private void PrintersCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePageSizes((Printer)((ComboBox)sender).SelectedItem);
        }

        private void UpdatePageSizes(Printer printer)
        {
            List<PageSize> pageSizes = new List<PageSize>();

            PageMediaSizeName? selectedName = ((PageSize)PageSizeCombo.SelectedItem)?.PageMediaSizeName;
            PageSize selectedPageSize = null;
            PageMediaSizeName defaultName = PageMediaSizeName.ISOA4;
            PageSize defaultPageSize = null;

            foreach (PageMediaSize pageMediaSize in printer.PageMediaSizeCapability)
            {
                PageSize pageSize = new PageSize(pageMediaSize);
                pageSizes.Add(pageSize);
                if (pageSize.PageMediaSizeName == selectedName)
                    selectedPageSize = pageSize;
                if (pageSize.PageMediaSizeName == defaultName)
                    defaultPageSize = pageSize;
            }

            pageSizes.Sort();

            PageSizeCombo.ItemsSource = pageSizes;

            PageSizeCombo.SelectionChanged -= PageSizeCombo_SelectionChanged;
            PageSizeCombo.SelectionChanged += PageSizeCombo_SelectionChanged;

            if (selectedPageSize != null)
                PageSizeCombo.SelectedItem = selectedPageSize;
            else if (defaultPageSize != null)
                PageSizeCombo.SelectedItem = defaultPageSize;
            else
                PageSizeCombo.SelectedIndex = 0;
        }

        private class PageSize : IComparable
        {
            public static ResourceDictionary NameDictionary { get; private set; }

            static PageSize()
            {
                NameDictionary = (ResourceDictionary)Application.LoadComponent(new Uri("Common/PrintingPreview/PageSizes_en_US.xaml", UriKind.Relative));
            }
            
            public PageMediaSizeName PageMediaSizeName { get; private set; }

            public Size Size { get; private set; }

            public string FriendlyName
            {
                get
                {
                    string name = PageMediaSizeName.ToString();
                    if (NameDictionary.Contains(name))
                        return (string)NameDictionary[name];
                    else
                        return name;
                }
            }

            public PageSize(PageMediaSize pageMediaSize)
            {
                PageMediaSizeName = (PageMediaSizeName)pageMediaSize.PageMediaSizeName;
                Size = new Size((double)pageMediaSize.Width, (double)pageMediaSize.Height);
            }

            public override string ToString()
            {
                return string.Format("{0}\n{1:0.##}cm x {2:0.##}cm", FriendlyName, PixelToLength(Size.Width), PixelToLength(Size.Height) / 96 * 2.54);
            }

            public int CompareTo(object obj)
            {
                return this.PageMediaSizeName.CompareTo(((PageSize)obj).PageMediaSizeName);
            }
        }

        private void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PageSize pageSize = (PageSize)((ComboBox)sender).SelectedItem;
            if (pageSize == null)
                return;
            UpdateView(pageSize);
        }

        private void UpdateView(PageSize pageSize)
        {
            FlowDocument.PageWidth = pageSize.Size.Width;
            FlowDocument.PageHeight = pageSize.Size.Height;
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            PrintBtn.IsEnabled = false;
            Printer printer = (Printer)PrintersCombo.SelectedItem;
            System.Windows.Xps.XpsDocumentWriter xpsDocumentWriter = PrintQueue.CreateXpsDocumentWriter(printer.PrintQueue);
            xpsDocumentWriter.WritingCompleted += XpsDocumentWriter_WritingCompleted;
            xpsDocumentWriter.WriteAsync(PreviewViewer.Document.DocumentPaginator);
        }

        private void XpsDocumentWriter_WritingCompleted(object sender, System.Windows.Documents.Serialization.WritingCompletedEventArgs e)
        {
            Close();
        }

        private void PaddingSettingBox_Loaded(object sender, RoutedEventArgs e)
        {
            UniformPaddingBox.Value = 2.54;

            VerticalPaddingBox.Value = 2.54;
            HorizontalPaddingBox.Value = 3.18;

            TopPaddingBox.Value = 2.54;
            BottomPaddingBox.Value = 2.54;
            LeftPaddingBox.Value = 3.18;
            RightPaddingBox.Value = 3.18;

            UniformPaddingBox.ValueChanged += (NumberBox numberBox, double newValue) => UpdatePadding(PaddingMode.Uniform);

            VerticalPaddingBox.ValueChanged += (NumberBox numberBox, double newValue) => UpdatePadding(PaddingMode.Symmrtry);
            HorizontalPaddingBox.ValueChanged += (NumberBox numberBox, double newValue) => UpdatePadding(PaddingMode.Symmrtry);

            TopPaddingBox.ValueChanged += (NumberBox numberBox, double newValue) => UpdatePadding(PaddingMode.Independent);
            BottomPaddingBox.ValueChanged += (NumberBox numberBox, double newValue) => UpdatePadding(PaddingMode.Independent);
            LeftPaddingBox.ValueChanged += (NumberBox numberBox, double newValue) => UpdatePadding(PaddingMode.Independent);
            RightPaddingBox.ValueChanged += (NumberBox numberBox, double newValue) => UpdatePadding(PaddingMode.Independent);

            PaddingModeCombo.SelectionChanged += PaddingModeCombo_SelectionChanged;
            PaddingModeCombo.SelectedIndex = 0;
        }

        public enum PaddingMode { Uniform, Symmrtry, Independent }

        private void UpdatePadding(PaddingMode paddingMode)
        {
            switch (paddingMode)
            {
                case PaddingMode.Uniform:
                    FlowDocument.PagePadding = new Thickness(LengthToPixel(UniformPaddingBox.Value));
                    break;
                case PaddingMode.Symmrtry:
                    double h = LengthToPixel(HorizontalPaddingBox.Value);
                    double v = LengthToPixel(VerticalPaddingBox.Value);
                    FlowDocument.PagePadding = new Thickness(h, v, h, v);
                    break;
                case PaddingMode.Independent:
                    FlowDocument.PagePadding = new Thickness(LengthToPixel(LeftPaddingBox.Value), LengthToPixel(TopPaddingBox.Value), LengthToPixel(RightPaddingBox.Value), LengthToPixel(BottomPaddingBox.Value));
                    break;
            }
        }

        private void PaddingModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem comboBoxItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            if (comboBoxItem == null)
                return;
            PaddingMode paddingMode = (PaddingMode)comboBoxItem.Tag;
            switch (paddingMode)
            {
                case PaddingMode.Uniform:
                    UniformPaddingBox.Visibility = Visibility.Visible;
                    SymmrtryPaddingBox.Visibility = Visibility.Collapsed;
                    IndependentPaddingBox.Visibility = Visibility.Collapsed;
                    break;
                case PaddingMode.Symmrtry:
                    UniformPaddingBox.Visibility = Visibility.Collapsed;
                    SymmrtryPaddingBox.Visibility = Visibility.Visible;
                    IndependentPaddingBox.Visibility = Visibility.Collapsed;
                    break;
                case PaddingMode.Independent:
                    UniformPaddingBox.Visibility = Visibility.Collapsed;
                    SymmrtryPaddingBox.Visibility = Visibility.Collapsed;
                    IndependentPaddingBox.Visibility = Visibility.Visible;
                    break;
            }
            UpdatePadding(paddingMode);
        }

        private static double PixelToLength(double pixels)
        {
            return pixels / 96 * 2.54;
        }

        private static double LengthToPixel(double pixels)
        {
            return pixels / 2.54 * 96;
        }
    }
}
