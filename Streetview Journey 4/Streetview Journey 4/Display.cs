using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace StreetviewJourney
{
    public sealed class Display //yoinked from stackoverflow
    {
        Display() { }

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }

        public static void SetWallpaper(string path, Style style)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            if (style == Style.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            else if (style == Style.Centered)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            else if (style == Style.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                path,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE
            );
        }

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        private enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117
        }

        /// <summary>
        /// Gets the windows display (DPI) scaling factor set. 1.5 = 150%
        /// </summary>
        /// <returns>The windows display (DPI) scaling factor set. 1.5 = 150%</returns>
        public static double ScalingFactor()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            double ScreenScalingFactor = (double)PhysicalScreenHeight / (double)LogicalScreenHeight;

            return ScreenScalingFactor;
        }
    }
}
