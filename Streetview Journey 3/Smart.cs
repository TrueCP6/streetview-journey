using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;

namespace Streetview_Journey_3
{
    class Smart
    {
        public enum Type { Drive, Hike }
        public static void ToSVJ(string path, Type type, int searchRadius = 50)
        {
            var locData = Import.Auto(path);
            double distance = type == Type.Drive ? 5 : 1;
            if (Get.AverageDistance(locData) > distance)
                locData = Modify.Interpolate(locData, distance, searchRadius);
            Export.ToSVJ(locData, path.Replace(".gpx", ".svj"));
        }

        public static void ToSVJ(string path, Type type, int trimTo, bool maintainSpeed, bool keepDupes, int searchRadius = 50)
        {
            var locData = Import.Auto(path);
            double distance = type == Type.Drive ? 5 : 1;
            if (Get.AverageDistance(locData) > distance)
                locData = Modify.Interpolate(locData, distance, searchRadius);
            if (maintainSpeed || trimTo > locData.Length)
                locData = Modify.SmoothTrim(locData, trimTo);
            else
                locData = Modify.Trim(locData, trimTo);
            if (keepDupes == false)
                locData = Remove.Dupes(locData);
            Export.ToSVJ(locData, path.Replace(".gpx", ".svj"));
        }

        public static Bitmap RandomImageFromRandomPanorama(int resX, int resY)
        {
            Random rng = new Random();
            var image = Bearing.OffsetPanorama(Download.Panorama(Get.RandomGooglePanoID()), rng.Next(0, 360));
            image = Modify.CropImage(image, new Rectangle(image.Width / 2 - resX / 2, image.Height / 2 - resY / 2, resX, resY));
            return image;
        }

        public static void ScreenshotSequence(string inputFile, string outputFolder, Type type, int resX, int resY, int searchRadius = 50)
        {
            var locData = Import.Auto(inputFile);
            double distance = type == Type.Drive ? 5 : 1;
            if (Get.AverageDistance(locData) > distance)
                locData = Modify.Interpolate(locData, distance, searchRadius);
            var bearings = Bearing.Get(locData);
            double wait = 1920 * 1080 >= resX * resY ? 5 : 10;
            Download.AllScreenshots(locData, bearings, resX, resY, 0, wait, outputFolder);
        }

        public static void PanoramaSequence(string inputFile, string outputFolder, Type type, int resX, int resY, int searchRadius = 50)
        {
            var locData = Import.Auto(inputFile);
            double distance = type == Type.Drive ? 5 : 1;
            if (Get.AverageDistance(locData) > distance)
                locData = Modify.Interpolate(locData, distance, searchRadius);
            Download.AllPanoramas(locData, outputFolder, ImageFormat.Jpeg, resX, resY);
        }
    }
}
