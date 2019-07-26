using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetview_Journey_3
{
    class Modify
    {
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static (double Lat, double Lon)[] SmoothTrim((double Lat, double Lon)[] locData, int trimTo)
        {
            double distancePerPoint = Get.TotalDistance(locData) / Convert.ToDouble(trimTo);
            double[] distances = Get.Distances(locData);

            (double Lat, double Lon)[] pointarray = new (double Lat, double Lon)[trimTo];
            for (int i = 0; i < trimTo; i++)
                pointarray[i] = locData[ClosestIndex(distances, Convert.ToDouble(i) * distancePerPoint)];

            return pointarray;
        }

        private static int ClosestIndex(double[] array, double value)
        {
            int closestIndexFound = 0;
            for (int i = 0; i < array.Length; i++)
                if (Math.Abs(array[i] - value) < Math.Abs(array[closestIndexFound] - value))
                    closestIndexFound = i;
            return closestIndexFound;
        }

        public static (double Lat, double Lon)[] Trim((double Lat, double Lon)[] locData, int trimTo)
        {
            double multiplier = Convert.ToDouble(locData.Length) / Convert.ToDouble(trimTo);
            (double Lat, double Lon)[] output = new (double Lat, double Lon)[trimTo];
            for (int i = 0; i < trimTo; i++)
                output[i] = locData[Convert.ToInt32(Math.Round(Convert.ToDouble(i) * multiplier))];
            return output;
        }

        public static (double Lat, double Lon)[] Interpolate((double Lat, double Lon)[] locData, double desiredMperPoint, int searchRadius = 50)
        {
            locData = Get.ExactCoords(locData, searchRadius);

            List<(double Lat, double Lon)>[] pointlistarray = new List<(double Lat, double Lon)>[locData.Length];

            Parallel.For(0, pointlistarray.Length - 1, a =>
            {
                pointlistarray[a] = new List<(double Lat, double Lon)>() {locData[a], locData[a + 1]};
                for (int b = 0; b < InterpolationCalc(Calculate.Distance(locData[a], locData[a + 1]), desiredMperPoint); b++)
                    pointlistarray[a] = InterpolateList(pointlistarray[a], searchRadius);
            });

            List<(double Lat, double Lon)> sortedList = new List<(double Lat, double Lon)>();
            foreach (List<(double Lat, double Lon)> list in pointlistarray)
            {
                if (list == null)
                    continue;
                foreach ((double Lat, double Lon) point in list)
                    sortedList.Add(point);
            }

            return Remove.Zeroes(Remove.Dupes(sortedList.ToArray()));
        }

        private static List<(double Lat, double Lon)> InterpolateList(List<(double Lat, double Lon)> list, int searchRadius)
        {
            List<(double Lat, double Lon)> interpolatedList = new List<(double Lat, double Lon)>();
            for (int i = 0; i < list.Count - 1; i++)
            {
                interpolatedList.Add(list[i]);
                interpolatedList.Add(Web.GetExact(Calculate.Midpoint(list[i], list[i + 1]), searchRadius));
            }
            interpolatedList.Add(list[list.Count - 1]);
            return interpolatedList;
        }

        private static int InterpolationCalc(double avgDistance, double desiredMPerPoint)
        {
            if (avgDistance < desiredMPerPoint)
                return 0;
            return Convert.ToInt32(Math.Ceiling(Math.Log(avgDistance / desiredMPerPoint - 1.0, 2) + 1.0));
        }

        public static Bitmap CropImage(Bitmap image, Rectangle cropRectangle)
        {
            Bitmap nb = new Bitmap(cropRectangle.Width, cropRectangle.Height);
            Graphics g = Graphics.FromImage(nb);
            g.DrawImage(image, -cropRectangle.X, -cropRectangle.Y);
            return nb;
        }
    }
}
