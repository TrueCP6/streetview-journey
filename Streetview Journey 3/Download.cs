using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using OpenQA.Selenium.Firefox;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using OpenQA.Selenium;
using System.IO;

namespace Streetview_Journey_3
{
    /// <summary>
    /// Downloading of Street View Images.
    /// </summary>
    class Download
    {
        /// <summary>
        /// Downloads a panorama for every point in a location data array.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <param name="folderPath">The folder into which every panorama will be downloaded.</param>
        /// <param name="format">The image format of the output panoramas.</param>
        /// <param name="width">The width of each output panorama. Panoramas have an aspect ratio of 2:1</param>
        /// <param name="height">The height of each output panorama. Panoramas have an aspect ratio of 2:1</param>
        public static void AllPanoramas((double Lat, double Lon)[] locData, string folderPath, ImageFormat format, int width, int height)
        {
            string[] panoIDs = Get.GooglePanoIDs(locData);

            for (int i = 0; i < panoIDs.Length; i++)
                Modify.ResizeImage(Panorama(panoIDs[i]), width, height).Save(folderPath + @"\image" + i + "." + format.ToString().ToLower(), format);
        }

        /// <summary>
        /// Downloads a 1st party panorama from a panorama ID.
        /// </summary>
        /// <param name="panoID">A 1st party panorama ID.</param>
        /// <returns>A bitmap image equirectangular panorama with an aspect ratio of 2:1</returns>
        public static Bitmap Panorama(string panoID)
        {
            Image[,] images = new Image[26, 13];
            Parallel.For(0, 26, x =>
            {
                Parallel.For(0, 13, y =>
                {
                    using (WebClient client = new WebClient())
                        images[x, y] = Image.FromStream(new MemoryStream(client.DownloadData(Get.TileURL(panoID, x, y))));
                });
            });

            Bitmap result = new Bitmap(26 * 512, 13 * 512);
            for (int x = 0; x < 26; x++)
                 for (int y = 0; y < 13; y++)
                     using (Graphics g = Graphics.FromImage(result))
                         g.DrawImage(images[x, y], x * 512, y * 512);
            return result;
        }

        /// <summary>
        /// Downloads a panorama for every point in a location data array.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <param name="folderPath">The folder into which every panorama will be downloaded.</param>
        /// <param name="format">The image format of the output panoramas.</param>
        public static void AllPanoramas((double Lat, double Lon)[] locData, string folderPath, ImageFormat format)
        {
            string[] panoIDs = Get.GooglePanoIDs(locData);

            for (int i = 0; i < panoIDs.Length; i++)
                Panorama(panoIDs[i]).Save(folderPath + @"\image" + i + "." + format.ToString().ToLower(), format);
        }

        /// <summary>
        /// The path to geckodriver.exe. The default is in the base directory of the executable.
        /// </summary>
        public static string geckoDriverPath = AppDomain.CurrentDomain.BaseDirectory + "geckodriver.exe";
        /// <summary>
        /// Download a screenshot from the streetview website for each point in a location data array.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <param name="bearings">An array of bearing values from 0 to 360.</param>
        /// <param name="resX">The width of each output image.</param>
        /// <param name="resY">The height of each output image.</param>
        /// <param name="pitch">The pitch of each output image.</param>
        /// <param name="wait">The time in seconds after an image has been opened to wait to take a screenshot for it to fully load. This has to be tweaked based on the speed of your internet and PC.</param>
        /// <param name="folderPath">The The folder into which every screenshot will be saved.</param>
        /// <param name="format">The format in which to save every image.</param>
        /// <returns>An array of basic road/place names with one for each point given from the location data array.</returns>
        public static string[] AllScreenshots((double Lat, double Lon)[] locData, double[] bearings, int resX, int resY, double pitch, double wait, string folderPath, ScreenshotImageFormat format = ScreenshotImageFormat.Jpeg)
        {
            FirefoxDriverService service = FirefoxDriverService.CreateDefaultService(geckoDriverPath.Replace(@"\geckodriver.exe", ""), "geckodriver.exe");
            var options = new FirefoxOptions();
            options.LogLevel = FirefoxDriverLogLevel.Fatal;
            var driver = new FirefoxDriver(service, options);
            double scaling = Get.DisplayScalingFactor();
            driver.Manage().Window.Size = new Size(
                Convert.ToInt32(Math.Round(Convert.ToDouble(resX) / scaling)) + 12,
                Convert.ToInt32(Math.Round(Convert.ToDouble(resY) / scaling)) + 80
            );

            string[] placesNames = new string[locData.Length];
            for (int i = 0; i < locData.Length; i++)
            {
                driver.Navigate().GoToUrl(Get.StreetviewURL(locData[i], bearings[i], pitch));
                Thread.Sleep(Convert.ToInt32(wait * 1000));

                placesNames[i] = driver.Title.Replace("Google Maps", "");
                if (placesNames[i].EndsWith(" - "))
                    placesNames[i] = placesNames[i].Remove(placesNames[i].Length - 3);

                RemoveElementsByClassName(driver, new string[] {
                    "widget-titlecard widget-titlecard-show-spotlight-link widget-titlecard-show-settings-menu",
                    "widget-image-header",
                    "scene-footer-container noprint",
                    "widget-minimap",
                    "app-vertical-widget-holder noprint",
                    "app-horizontal-widget-holder noprint"
                });

                driver.GetScreenshot().SaveAsFile(folderPath + @"\image" + i + "." + format.ToString().ToLower(), format);
            }
            driver.Quit();

            return placesNames;
        }

        private static void RemoveElementsByClassName(FirefoxDriver driver, string[] elements)
        {
            var js = (IJavaScriptExecutor)driver;
            foreach (string element in elements)
                js.ExecuteScript("return document.getElementsByClassName('" + element + "')[0].remove();");
        }

        /// <summary>
        /// Download a static streetview image for each point in a location data array
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <param name="bearings">An array of bearing values from 0 to 360.</param>
        /// <param name="pitch">The pitch of each output image.</param>
        /// <param name="resX">The width of each output image. Maximum is dependent on your Static Streetview API plan.</param>
        /// <param name="resY">The height of each output image. Maximum is dependent on your Static Streetviw API plan.</param>
        /// <param name="fov">Field of view of each output image. Maximum of 120.</param>
        /// <param name="folder">The format in which to save every image.</param>
        public static void AllImages((double Lat, double Lon)[] locData, double[] bearings, double pitch, int resX, int resY, int fov, string folder)
        {
            Parallel.For(0, locData.Length, i =>
            {
                using (WebClient client = new WebClient())
                    client.DownloadFile(Get.ImageURL(locData[i], bearings[i], pitch, resX, resY, fov), folder + @"\image" + i + ".png");
            });
        }
    }
}
