using System;

namespace Framework
{
    class HitTest
    {
        public const int WM_NCHITTEST = 0x0084;
        private enum HitTestResult : int
        {
            HTERROR = -2,
            HTTRANSPARENT = -1,
            HTNOWHERE = 0,
            HTCLIENT = 1,
            HTCAPTION = 2,
            HTSYSMENU = 3,
            HTGROWBOX = 4,
            HTSIZE = HTGROWBOX,
            HTMENU = 5,
            HTHSCROLL = 6,
            HTVSCROLL = 7,
            HTMINBUTTON = 8,
            HTMAXBUTTON = 9,
            HTLEFT = 10,
            HTRIGHT = 11,
            HTTOP = 12,
            HTTOPLEFT = 13,
            HTTOPRIGHT = 14,
            HTBOTTOM = 15,
            HTBOTTOMLEFT = 16,
            HTBOTTOMRIGHT = 17,
            HTBORDER = 18,
            HTREDUCE = HTMINBUTTON,
            HTZOOM = HTMAXBUTTON,
            HTSIZEFIRST = HTLEFT,
            HTSIZELAST = HTBOTTOMRIGHT,
            HTOBJECT = 19,
            HTCLOSE = 20,
            HTHELP = 21,
        }
        private static double borderThickness = 16;
        private static double borderOffset = 8;

        public static void ChangeBorderSize(double borderThickness, double borderOffset)
        {
            HitTest.borderThickness = borderThickness;
            HitTest.borderOffset = borderOffset;
        }

        public static IntPtr Hit(IntPtr lParam, double top, double left, double height, double width)
        {
            double mousePointX = (Int16)(lParam.ToInt32() & 0xFFFF) / ((float)Screen.DpiY / Screen.UNSCALED_DPI);
            double mousePointY = (Int16)(lParam.ToInt32() >> 16) / ((float)Screen.DpiY / Screen.UNSCALED_DPI);
            // Empty
            if (mousePointY - top <= borderOffset && mousePointX - left <= borderOffset
                || height + top - mousePointY <= borderOffset && mousePointX - left <= borderOffset
                || mousePointY - top <= borderOffset && width + left - mousePointX <= borderOffset
                || width + left - mousePointX <= borderOffset && height + top - mousePointY <= borderOffset
                || mousePointX - left <= borderOffset
                || width + left - mousePointX <= borderOffset
                || mousePointY - top <= borderOffset
                || height + top - mousePointY <= borderOffset)
                return new IntPtr((int)HitTestResult.HTTRANSPARENT);
            // TopLeft
            if (mousePointY - top <= borderThickness && mousePointX - left <= borderThickness)
                return new IntPtr((int)HitTestResult.HTTOPLEFT);
            // BottomLeft    
            else if (height + top - mousePointY <= borderThickness && mousePointX - left <= borderThickness)
                return new IntPtr((int)HitTestResult.HTBOTTOMLEFT);
            // TopRight
            else if (mousePointY - top <= borderThickness && width + left - mousePointX <= borderThickness)
                return new IntPtr((int)HitTestResult.HTTOPRIGHT);
            // BottomRight
            else if (width + left - mousePointX <= borderThickness && height + top - mousePointY <= borderThickness)
                return new IntPtr((int)HitTestResult.HTBOTTOMRIGHT);
            // Left
            else if (mousePointX - left <= borderThickness)
                return new IntPtr((int)HitTestResult.HTLEFT);
            // Right
            else if (width + left - mousePointX <= borderThickness)
                return new IntPtr((int)HitTestResult.HTRIGHT);
            // Top
            else if (mousePointY - top <= borderThickness)
                return new IntPtr((int)HitTestResult.HTTOP);
            // Bottom
            else if (height + top - mousePointY <= borderThickness)
                return new IntPtr((int)HitTestResult.HTBOTTOM);
            // Inside
            else
                return new IntPtr((int)HitTestResult.HTCLIENT);
        }
    }
}
