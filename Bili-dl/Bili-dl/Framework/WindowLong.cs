using System;
using System.Runtime.InteropServices;

namespace Framework
{
    /// <summary>
    /// Class <c>WindowLong</c> packaged Win32 apis for Get/Set WindowLong.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    class WindowLong
    {
        [DllImport("USER32.DLL")]
        public static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("USER32.DLL")]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        public static int GWL_EXSTYLE = -20;
        public static int GWL_HINSTANCE = -6;
        public static int GWL_ID = -12;
        public static int GWL_STYLE = -16;
        public static int GWL_USERDATA = -21;
        public static int GWL_WNDPROC = -4;

        public static uint WS_BORDER = 0x00800000;
        public static uint WS_CAPTION = 0x00C00000;
        public static uint WS_CHILD = 0x40000000;
        public static uint WS_CHILDWINDOW = 0x40000000;
        public static uint WS_CLIPCHILDREN = 0x02000000;
        public static uint WS_CLIPSIBLINGS = 0x04000000;
        public static uint WS_DISABLED = 0x08000000;
        public static uint WS_DLGFRAME = 0x00400000;
        public static uint WS_GROUP = 0x00020000;
        public static uint WS_HSCROLL = 0x00100000;
        public static uint WS_ICONIC = 0x20000000;
        public static uint WS_MAXIMIZE = 0x01000000;
        public static uint WS_MAXIMIZEBOX = 0x00010000;
        public static uint WS_MINIMIZE = 0x20000000;
        public static uint WS_MINIMIZEBOX = 0x00020000;
        public static uint WS_OVERLAPPED = 0x00000000;
        public static uint WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        public static uint WS_POPUP = 0x80000000;
        public static uint WS_POPUPWINDOW = (WS_POPUP | WS_BORDER | WS_SYSMENU);
        public static uint WS_SIZEBOX = 0x00040000;
        public static uint WS_SYSMENU = 0x00080000;
        public static uint WS_TABSTOP = 0x00010000;
        public static uint WS_THICKFRAME = 0x00040000;
        public static uint WS_TILED = 0x00000000;
        public static uint WS_TILEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        public static uint WS_VISIBLE = 0x10000000;
        public static uint WS_VSCROLL = 0x00200000;
        
        public static uint WS_EX_ACCEPTFILES = 0x00000010;
        public static uint WS_EX_APPWINDOW = 0x00040000;
        public static uint WS_EX_CLIENTEDGE = 0x00000200;
        public static uint WS_EX_COMPOSITED = 0x02000000;
        public static uint WS_EX_CONTEXTHELP = 0x00000400;
        public static uint WS_EX_CONTROLPARENT = 0x00010000;
        public static uint WS_EX_DLGMODALFRAME = 0x00000001;
        public static uint WS_EX_LAYERED = 0x00080000;
        public static uint WS_EX_LAYOUTRTL = 0x00400000;
        public static uint WS_EX_LEFT = 0x00000000;
        public static uint WS_EX_LEFTSCROLLBAR = 0x00004000;
        public static uint WS_EX_LTRREADING = 0x00000000;
        public static uint WS_EX_MDICHILD = 0x00000040;
        public static uint WS_EX_NOACTIVATE = 0x08000000;
        public static uint WS_EX_NOINHERITLAYOUT = 0x00100000;
        public static uint WS_EX_NOPARENTNOTIFY = 0x00000004;
        public static uint WS_EX_NOREDIRECTIONBITMAP = 0x00200000;
        public static uint WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
        public static uint WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
        public static uint WS_EX_RIGHT = 0x00001000;
        public static uint WS_EX_RIGHTSCROLLBAR = 0x00000000;
        public static uint WS_EX_RTLREADING = 0x00002000;
        public static uint WS_EX_STATICEDGE = 0x00020000;
        public static uint WS_EX_TOOLWINDOW = 0x00000080;
        public static uint WS_EX_TOPMOST = 0x00000008;
        public static uint WS_EX_TRANSPARENT = 0x00000020;
        public static uint WS_EX_WINDOWEDGE = 0x00000100;
    }
}
