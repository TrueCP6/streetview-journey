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
        /// <summary>
        /// The maximum automatic smooth value. Default is 10.
        /// </summary>
        public static int smoothMax = 10;
        /// <summary>
        /// Automatically smoothes an array of bearings.
        /// </summary>
        /// <param name="bearings">An array of bearing values from 0 to 360.</param>
        /// <returns>A smoothed array of bearing values from 0 to 360.</returns>
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

        /// <summary>
        /// Rotates an equirectangular panorama in a circle by a certain angle.
        /// </summary>
        /// <param name="panorama">Equirectangular panorama image.</param>
        /// <param name="angleToOffset">Bearing angle from 0 to 360.</param>
        /// <returns>An equirectangular panorama bitmap image.</returns>
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

        /// <summary>
        /// Trims an array of bearings down to a specific length. The pair of Modify.Trim.
        /// </summary>
        /// <param name="bearings">An array of bearing values from 0 to 360.</param>
        /// <param name="trimTo">The desired length of the output array.</param>
        /// <returns>An array of bearing values from 0 to 360.</returns>
        public static double[] Trim(double[] bearings, int trimTo)
        {
            double multiplier = Convert.ToDouble(bearings.Length) / Convert.ToDouble(trimTo);
            double[] output = new double[trimTo];
            for (int i = 0; i < trimTo; i++)
                output[i] = bearings[Convert.ToInt32(Math.Round(Convert.ToDouble(i) * multiplier))];
            return output;
        }

        /// <summary>
        /// Smoothes an array of bearing values consistently.
        /// </summary>
        /// <param name="bearings">An array of bearing values from 0 to 360.</param>
        /// <param name="smoothValue">The amount of bearings to smooth into each other.</param>
        /// <returns>An array of bearing values from 0 to 360.</returns>
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

        /// <summary>
        /// Calculates the bearings for a location data array so that they track a point.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <param name="point">A latitude-longitude point to be tracked.</param>
        /// <returns>An array of bearing values from 0 to 360.</returns>
        public static double[] TrackPoint((double Lat, double Lon)[] locData, (double Lat, double Lon) point)
        {
            double[] bearings = new double[locData.Length];
            for (int i = 0; i < bearings.Length; i++)
                bearings[i] = Calculate.Bearing(locData[i], point);
            return bearings;
        }

        /// <summary>
        /// Calculates the bearings for a location data array where each point faces the direction of the next point in the array.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <returns>An array of bearing values from 0 to 360.</returns>
        public static double[] Get((double Lat, double Lon)[] locData)
        {
            double[] bearings = new double[locData.Length];
            for (int i = 0; i < bearings.Length - 1; i++)
                bearings[i] = Calculate.Bearing(locData[i], locData[i + 1]);
            bearings[bearings.Length - 1] = bearings[bearings.Length - 2];
            return bearings;
        }

        /// <summary>
        /// Gets a multilined string of each bearing in an array.
        /// </summary>
        /// <param name="bearings">An array of bearing values from 0 to 360.</param>
        /// <returns>A multilined string.</returns>
        public static string GetString(double[] bearings)
        {
            string outstring = "";
            foreach (double bearing in bearings)
                outstring += bearing + Environment.NewLine;
            return outstring;
        }
    }
}
