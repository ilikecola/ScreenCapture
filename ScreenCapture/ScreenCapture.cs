using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace WindowsFormsApplication1
{
    public partial class ScreenCapture : Form
    {
        [DllImport("user32.dll", EntryPoint = "WindowFromPoint")]
        public static extern IntPtr WindowFromPoint(Point point);
        [DllImport("gdi32.dll", EntryPoint = "GetPixel")]//取指定点颜色
        private static extern int GetPixel(IntPtr hdc, Point p);
        private class GDI32
        {
            public const int SRCCOPY = 0x00CC0020;
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }
        /// <summary>
        /// 辅助类 定义User32 API函数
        /// </summary>
        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
        }


        IntPtr GamehWnd = new IntPtr(0);
        DateTime currentTime = new DateTime();

        public ScreenCapture()
        {
            InitializeComponent();
        }

        private void button_MouseDown(object sender, MouseEventArgs e)
        {
            manual.Enabled = true;
        }

        private void button_MouseUp(object sender, MouseEventArgs e)
        {
            manual.Enabled = false;
        }

        private void manual_Tick(object sender, EventArgs e)
        {
            int x = Cursor.Position.X;
            int y = Cursor.Position.Y;
            Point p = new Point(x, y);
            GamehWnd = WindowFromPoint(p);
            label1.Text = GamehWnd.ToString();
        }

        private void ScreenCapture_Load(object sender, EventArgs e)
        {
            manual.Enabled = false;
            manual.Interval = 500;
        }

        public Image CaptureWindow(IntPtr handle)
        {
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            GDI32.SelectObject(hdcDest, hOld);
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            Image img = Image.FromHbitmap(hBitmap);
            GDI32.DeleteObject(hBitmap);
            return img;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Image img = CaptureWindow(GamehWnd);
            string dir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            String Timestr = currentTime.ToString("HHmmss");
            img.Save(dir + Timestr + ".jpg", ImageFormat.Jpeg);
            Process.Start(dir + Timestr + ".jpg");
        }

        //后台取色部分
        public string GetPixelColor(IntPtr hWnd, int xPos, int yPos)
        {
            string PixelColor = "";
            Point p = new Point(xPos, yPos + 6);
            IntPtr hdc = User32.GetWindowDC(hWnd);
            int c = GetPixel(hdc, p);
            int r = (c & 0xFF);//转换R
            int g = (c & 0xFF00) / 256;//转换G
            int b = (c & 0xFF0000) / 65536;//转换B
            PixelColor = b.ToString("X").PadLeft(2, '0') + g.ToString("X").PadLeft(2, '0') + r.ToString("X").PadLeft(2, '0');//输出16进制颜色
            return PixelColor;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string pixelcolor = GetPixelColor(GamehWnd, int.Parse(textBox1.Text), int.Parse(textBox2.Text));
            label2.Text = pixelcolor;
        }
    }
}
