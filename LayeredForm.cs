using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RobotInterfaceUI
{
    public class LayeredForm : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst,
            ref Size psize, IntPtr hdcSrc, ref Point pptSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        private const int ULW_ALPHA = 0x00000002;
        private const byte AC_SRC_OVER = 0x00;
        private const byte AC_SRC_ALPHA = 0x01;
        public const uint DIB_RGB_COLORS = 0;

        private IntPtr _memDC;
        private IntPtr _hBitmap;
        private IntPtr _oldBitmap;
        private Size _layerSize;

        public void InitLayer(int width, int height)
        {
            _layerSize = new Size(width, height);
            IntPtr screenDC = GetDC(IntPtr.Zero);

            _memDC = Gdi32.CreateCompatibleDC(screenDC);

            BITMAPINFO bmi = new BITMAPINFO();
            bmi.bmiHeader.biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER));
            bmi.bmiHeader.biWidth = width;
            bmi.bmiHeader.biHeight = -height;
            bmi.bmiHeader.biPlanes = 1;
            bmi.bmiHeader.biBitCount = 32;
            bmi.bmiHeader.biCompression = 0;

            _hBitmap = Gdi32.CreateDIBSection(_memDC, ref bmi, 0, out _, IntPtr.Zero, 0);
            _oldBitmap = Gdi32.SelectObject(_memDC, _hBitmap);

            ReleaseDC(IntPtr.Zero, screenDC);
        }

        public Graphics GetGraphics()
        {
            return Graphics.FromHdc(_memDC);
        }

        public void UpdateLayer()
        {
            Point topPos = new Point(Left, Top);
            Point srcPos = new Point(0, 0);
            BLENDFUNCTION blend = new BLENDFUNCTION
            {
                BlendOp = 0,
                BlendFlags = 0,
                SourceConstantAlpha = 255,
                AlphaFormat = 1
            };

            IntPtr screenDC = GetDC(IntPtr.Zero);
            UpdateLayeredWindow(Handle, screenDC, ref topPos, ref _layerSize, _memDC, ref srcPos, 0, ref blend, 2);
            ReleaseDC(IntPtr.Zero, screenDC);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int exStyle = (int)Win32.GetWindowLong(Handle, Win32.GWL_EXSTYLE);
            Win32.SetWindowLong(Handle, Win32.GWL_EXSTYLE, exStyle | Win32.WS_EX_LAYERED);
        }

        protected override void Dispose(bool disposing)
        {
            if (_memDC != IntPtr.Zero && _oldBitmap != IntPtr.Zero)
            {
                Gdi32.SelectObject(_memDC, _oldBitmap);
            }

            if (_hBitmap != IntPtr.Zero)
            {
                Gdi32.DeleteObject(_hBitmap);
                _hBitmap = IntPtr.Zero;
            }

            if (_memDC != IntPtr.Zero)
            {
                Gdi32.DeleteDC(_memDC);
                _memDC = IntPtr.Zero;
            }

            base.Dispose(disposing);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            public uint bmiColors;
        }

        private static class Gdi32
        {
            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern bool DeleteDC(IntPtr hdc);

            [DllImport("gdi32.dll", ExactSpelling = true)]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern bool DeleteObject(IntPtr hObject);

            [DllImport("gdi32.dll", SetLastError = true)]
            public static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BITMAPINFO pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);
        }

        private static class Win32
        {
            public const int GWL_EXSTYLE = -20;
            public const int WS_EX_LAYERED = 0x80000;
            public const int WS_EX_TRANSPARENT = 0x00000020;

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        }
    }
}
