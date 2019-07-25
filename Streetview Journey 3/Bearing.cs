using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetview_Journey_3
{
    class Bearing
    {
        public static Bitmap OffsetPanorama(Bitmap panorama, double angleToOffset)
        {
            Bitmap result = new Bitmap(panorama.Width, panorama.Height);
            int pixelsToOffset = Convert.ToInt32(Math.Round(angleToOffset / 360.0 * Convert.ToDouble(panorama.Width)));
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(panorama, pixelsToOffset, 0);
                g.DrawImage(panorama, pixelsToOffset - panorama.Width, 0);
            }
            return result;
        }

        public static double[] Trim(double[] bearings, int trimTo)
        {
            double multiplier = Convert.ToDouble(bearings.Length) / Convert.ToDouble(trimTo);
            double[] output = new double[trimTo];
            for (int i = 0; i < trimTo; i++)
                output[i] = bearings[Convert.ToInt32(Math.Round(Convert.ToDouble(i) * multiplier))];
            return output;
        }

        public static double[] Smooth(double[] bearings, int smoothValue)
        {
            for (int a = 0; a < bearings.Length - smoothValue; a++)
            {
                double[] temp = new double[smoothValue];
                for (int b = 0; b < smoothValue; b++)
                    temp[b] = bearings[a + b];
                bearings[a] = Calculate.AverageBearing(temp);
            }
            return bearings;
        }

        public static double[] TrackPoint((double Lat, double Lon)[] locData, (double Lat, double Lon) point)
        {
            double[] bearings = new double[locData.Length];
            for (int i = 0; i < bearings.Length; i++)
                bearings[i] = Calculate.Bearing(locData[i], point);
            return bearings;
        }

        public static double[] Get((double Lat, double Lon)[] locData)
        {
            double[] bearings = new double[locData.Length];
            for (int i = 0; i < bearings.Length - 1; i++)
                bearings[i] = Calculate.Bearing(locData[i], locData[i + 1]);
            bearings[bearings.Length - 1] = bearings[bearings.Length - 2];
            return bearings;
        }
    }
}
