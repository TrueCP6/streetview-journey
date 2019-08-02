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
        public static int smoothMax = 10;
        public static double[] Smooth(double[] bearings)
        {
            double[] difference = new double[bearings.Length];
            for (int i = 0; i < difference.Length - 1; i++)
                difference[i] = Calculate.BearingDifference(bearings[i], bearings[i + 1]);
            difference[difference.Length - 1] = 0;

            double[] final = new double[bearings.Length];
            for (int a = 0; a < bearings.Length - smoothMax; a++)
            {
                double sum = 0;
                int smoothTo = smoothMax;
                for (int b = 0; b < smoothMax; b++)
                {
                    sum += difference[a + b];
                    if (sum >= 90)
                    {
                        smoothTo = b;
                        break;
                    }
                }

                double[] forSmooth = new double[smoothTo];
                for (int b = 0; b < smoothTo; b++)
                    forSmooth[b] = bearings[a + b];
                if (forSmooth.Length != 0)
                    final[a] = Calculate.AverageBearing(forSmooth);
                else
                    final[a] = bearings[a];
            }

            for (int a = bearings.Length - smoothMax; a < bearings.Length; a++)
                final[a] = bearings[a];

            return final;
        }

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
