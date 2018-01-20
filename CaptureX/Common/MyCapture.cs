using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace CaptureX
{
    class MyCapture
    {

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOZORDER = 0x0004;
        const UInt32 SWP_NOREDRAW = 0x0008;
        const UInt32 SWP_NOACTIVATE = 0x0010;
        const UInt32 SWP_FRAMECHANGED = 0x0020;
        const UInt32 SWP_SHOWWINDOW = 0x0040;
        const UInt32 SWP_HIDEWINDOW = 0x0080;
        const UInt32 SWP_NOCOPYBITS = 0x0100;
        const UInt32 SWP_NOOWNERZORDER = 0x0200;
        const UInt32 SWP_NOSENDCHANGING = 0x0400;
        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;


        public static Image GetScreenSnapshot(IntPtr hwnd)
        {
            Rectangle rc = new Rectangle();
            Win32API.GetWindowRect(hwnd, out rc);

            Point p = new Point(rc.X, rc.Y);
            if (isOut(rc))
            {
                Win32API.SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_SHOWWINDOW);

                Win32API.GetWindowRect(hwnd, out rc);
            }
            else
            {
                Win32API.SetWindowPos(hwnd, HWND_TOPMOST, p.X + 2, p.Y + 2, 0, 0, SWP_NOSIZE | SWP_SHOWWINDOW | SWP_FRAMECHANGED);
                Thread.Sleep(50);
                Win32API.SetWindowPos(hwnd, HWND_TOPMOST, p.X, p.Y, 0, 0, SWP_NOSIZE | SWP_SHOWWINDOW | SWP_FRAMECHANGED);
            }

            Thread.Sleep(100);
            Image bitmap = new Bitmap(rc.Width - rc.X, rc.Height - rc.Y, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(rc.X, rc.Y, 0, 0, new Size(rc.Width - rc.X, rc.Height - rc.Y), CopyPixelOperation.SourceCopy);
            }

            Win32API.SetWindowPos(hwnd, HWND_NOTOPMOST, p.X, p.Y, 0, 0, SWP_NOSIZE);
            return bitmap;
        }

        private static bool isOut(Rectangle rc)
        {

            return (rc.Width > Screen.PrimaryScreen.Bounds.Width
                    || rc.Height > Screen.PrimaryScreen.Bounds.Height
                    || rc.X < 0
                    || rc.Y < 0);
        }

    }
}
