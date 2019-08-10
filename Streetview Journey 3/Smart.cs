using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.IO;

namespace Streetview_Journey_3
{
    /// <summary>
    /// Application uses of this library.
    /// </summary>
    class Smart
    {
        public enum Type { Drive, Hike }

        /// <summary>
        /// Takes an SVJ or GPX file and automatically interpolates and fully details itself and saves a .svj file with the same name.
        /// </summary>
        /// <param name="path">The path of the input file.</param>
        /// <param name="type">The type of travel that was used in the route. Can be Drive or Hike.</param>
        /// <param name="searchRadius">The search radius in meters for each new point.</param>
        public static void ToSVJ(string path, Type type, int searchRadius = 50)
        {
            var locData = Import.Auto(path);
            double distance = type == Type.Drive ? 5 : 1;
            if (Get.AverageDistance(locData) > distance)
                locData = Modify.Interpolate(locData, distance, searchRadius);
            Export.ToSVJ(locData, path.Replace(".gpx", ".svj"));
        }

        /// <summary>
        /// Takes an SVJ or GPX file and automatically interpolates and fully details itself and saves a .svj file with the same name. The desired length can be chosen.
        /// </summary>
        /// <param name="path">The path of the input file.</param>
        /// <param name="type">The type of travel that was used in the route. Can be Drive or Hike.</param>
        /// <param name="trimTo">The desired amount of points that it will return.</param>
        /// <param name="maintainSpeed">Whether to try to keep a constant speed when trimming. Takes slightly longer when true and may result in duplicates.</param>
        /// <param name="keepDupes">Whether to keep duplicate points in the output file. Length may not be equal to <c>trimTo</c> if true.</param>
        /// <param name="searchRadius">The search radius in meters for each new point.</param>
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

        /// <summary>
        /// Gets a random image facing a random direction from a random panorama on Earth. The lower the resolution the higher the zoom.
        /// </summary>
        /// <param name="resX">The width of the output image.</param>
        /// <param name="resY">The height of the output image.</param>
        /// <returns>A bitmap image.</returns>
        public static Bitmap RandomImageFromRandomPanorama(int resX, int resY)
        {
            Random rng = new Random();
            var image = Bearing.OffsetPanorama(Download.Panorama(Get.RandomGooglePanoID()), rng.Next(0, 360));
            image = Modify.CropImage(image, new Rectangle(image.Width / 2 - resX / 2, image.Height / 2 - resY / 2, resX, resY));
            return image;
        }

        /// <summary>
        /// Downloads a sequence of screenshots from a .svj or .gpx file to a folder.
        /// </summary>
        /// <param name="inputFile">The path to the input .gpx or .svj file.</param>
        /// <param name="outputFolder">The path to the folder into which all images will be saved.</param>
        /// <param name="type">The type of travel that was used in the route. Can be Drive or Hike.</param>
        /// <param name="resX">The width of the output images.</param>
        /// <param name="resY">The height of the output images.</param>
        /// <param name="searchRadius">The search radius in meters for each new point.</param>
        public static void ScreenshotSequence(string inputFile, string outputFolder, Type type, int resX, int resY, int searchRadius = 50)
        {
            var locData = Import.Auto(inputFile);
            double distance = type == Type.Drive ? 5 : 1;
            if (Get.AverageDistance(locData) > distance)
                locData = Modify.Interpolate(locData, distance, searchRadius);
            var bearings = Bearing.Smooth(Bearing.Get(locData));
            Download.AllScreenshots(locData, bearings, resX, resY, 0, outputFolder, Environment.ProcessorCount / 2);
        }

        /// <summary>
        /// Downloads a sequence of panoramas from a .svj or .gpx file to a folder.
        /// </summary>
        /// <param name="inputFile">The path to the input .gpx or .svj file.</param>
        /// <param name="outputFolder">The path to the folder into which all images will be saved.</param>
        /// <param name="type">The type of travel that was used in the route. Can be Drive or Hike.</param>
        /// <param name="resX">The width of the output images. Panoramas have an aspect ratio of 2:1.</param>
        /// <param name="resY">The height of the output images. Panoramas have an aspect ratio of 2:1.</param>
        /// <param name="searchRadius">The search radius in meters for each new point.</param>
        public static void PanoramaSequence(string inputFile, string outputFolder, Type type, int resX, int resY, int searchRadius = 50)
        {
            var locData = Import.Auto(inputFile);
            double distance = type == Type.Drive ? 5 : 1;
            if (Get.AverageDistance(locData) > distance)
                locData = Modify.Interpolate(locData, distance, searchRadius);
            Download.AllPanoramas(locData, outputFolder, ImageFormat.Jpeg, resX, resY);
        }

        /// <summary>
        /// Downloads a sequence of static streetview images from a .svj or .gpx file to a folder.
        /// </summary>
        /// <param name="inputFile">The path to the input .gpx or .svj file.</param>
        /// <param name="outputFolder">The path to the folder into which all images will be saved.</param>
        /// <param name="type">The type of travel that was used in the route. Can be Drive or Hike.</param>
        /// <param name="resX">The width of the output images. Max resolution is determined by your API plan.</param>
        /// <param name="resY">The height of the output images. Max resolution is determined by your API plan.</param>
        /// <param name="fieldOfView">The output field of view of all the images.</param>
        /// <param name="searchRadius">The search radius in meters for each new point.</param>
        public static void ImageSequence(string inputFile, string outputFolder, Type type, int resX, int resY, int fieldOfView, int searchRadius = 50)
        {
            var locData = Import.Auto(inputFile);
            double distance = type == Type.Drive ? 5 : 1;
            if (Get.AverageDistance(locData) > distance)
                locData = Modify.Interpolate(locData, distance, searchRadius);
            var bearings = Bearing.Smooth(Bearing.Get(locData));
            Download.AllImages(locData, bearings, 0, resX, resY, fieldOfView, outputFolder);
        }

        /// <summary>
        /// Creates a video from a .svj or .gpx file
        /// </summary>
        /// <param name="inputFile">The path to the input .svj/.gpx file.</param>
        /// <param name="outputFolder">The folder into which all temporary images and the output video will be saved.</param>
        /// <param name="framerate">The framerate of the output video.</param>
        /// <param name="type">The type of travel used in the input file.</param>
        /// <param name="resX">The width of the output video.</param>
        /// <param name="resY">The height of the output video.</param>
        /// <param name="searchRadius">The radius in meters to search for each image.</param>
        public static void ScreenshotVideo(string inputFile, string outputFolder, int framerate, Type type, int resX, int resY, int searchRadius = 50)
        {
            var locData = Import.Auto(inputFile);
            double distance = type == Type.Drive ? 5 : 1;
            if (Get.AverageDistance(locData) > distance)
                locData = Modify.Interpolate(locData, distance, searchRadius);
            var bearings = Bearing.Smooth(Bearing.Get(locData));

            Download.AllScreenshots(locData, bearings, resX, resY, 0, outputFolder, Environment.ProcessorCount / 2);

            Export.ToVideo(framerate, outputFolder, "output.mp4");

            string[] files = Directory.GetFiles(outputFolder);
            foreach (string file in files)
            {
                string tempname = file.Split(new string[] {"\""}, StringSplitOptions.RemoveEmptyEntries).Last();
                if (tempname.StartsWith("image") && tempname.Contains("."))
                    File.Delete(file);
            }
        }

        /// <summary>
        /// Creates a video from a .svj or .gpx file.
        /// </summary>
        /// <param name="inputFile">The path to the input .svj/.gpx file.</param>
        /// <param name="outputFolder">The folder into which all temporary images and the output video will be saved.</param>
        /// <param name="framerate">The framerate of the output video.</param>
        /// <param name="type">The type of travel used in the input file.</param>
        /// <param name="resX">The width of the output video.</param>
        /// <param name="resY">The height of the output video.</param>
        /// <param name="fieldOfView">The field of view in degrees of the output video. Maximum of 120.</param>
        /// <param name="searchRadius">The radius in meters to search for each image.</param>
        public static void ImageVideo(string inputFile, string outputFolder, int framerate, Type type, int resX, int resY, int fieldOfView, int searchRadius = 50)
        {
            var locData = Import.Auto(inputFile);
            double distance = type == Type.Drive ? 5 : 1;
            if (Get.AverageDistance(locData) > distance)
                locData = Modify.Interpolate(locData, distance, searchRadius);
            var bearings = Bearing.Smooth(Bearing.Get(locData));

            Download.AllImages(locData, bearings, 0, resX, resY, fieldOfView, outputFolder);

            Export.ToVideo(framerate, outputFolder, "output.mp4");

            string[] files = Directory.GetFiles(outputFolder);
            foreach (string file in files)
            {
                string tempname = file.Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries).Last();
                if (tempname.StartsWith("image") && tempname.Contains("."))
                    File.Delete(file);
            }
        }
    }
}
