using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Bili_dl
{
    /// <summary>
    /// A MultiValueConverter for converting width & height to Rect instance.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public class RectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double w = (double)values[0];
            double h = (double)values[1];
            Rect rect;
            if (w > 0 && h > 0)
                rect = new Rect(0, 0, w, h);
            else
                rect = new Rect(0, 0, 1, 1);
            return rect;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// A MultiValueConverter for converting width & height to Rect instance with a 1px offset.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public class BorderRectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double borderThickness = 1;
            double w = (double)values[0] - 2 * borderThickness;
            double h = (double)values[1] - 2 * borderThickness;
            Rect rect;
            if (w > 2 && h > 2)
                rect = new Rect(borderThickness, borderThickness, w, h);
            else
                rect = new Rect(borderThickness, borderThickness, 2, 2);
            return rect;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
