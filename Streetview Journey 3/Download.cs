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
        /// Download a screenshot from the streetview website for each point in a location data array. 
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <param name="bearings">An array of bearing values from 0 to 360.</param>
        /// <param name="resX">The width of each output image.</param>
        /// <param name="resY">The height of each output image.</param>
        /// <param name="pitch">The pitch of each output image.</param>
        /// <param name="folderPath">The folder into which every screenshot will be saved.</param>
        /// <param name="maxWindows">The maximum amount of Firefox windows to be opened at once. Higher values mean faster overall downloads but slower machines may suffer. Resolution also affects performance.</param>
        /// <param name="format">The format in which to save every image.</param>
        /// <returns>An array of basic road/place names with one for each point given from the location data array.</returns>
        public static string[] AllScreenshots((double Lat, double Lon)[] locData, double[] bearings, int resX, int resY, double pitch, string folderPath, int maxWindows, ScreenshotImageFormat format = ScreenshotImageFormat.Jpeg)
        {
            FirefoxDriver[] drivers = new FirefoxDriver[maxWindows];
            double scaling = Get.DisplayScalingFactor();
            Size windowSize = new Size(
                Convert.ToInt32(Math.Round(Convert.ToDouble(resX) / scaling)) + 12,
                Convert.ToInt32(Math.Round(Convert.ToDouble(resY) / scaling)) + 80
            );
            string[] placesNames = new string[locData.Length];

            Parallel.For(0, maxWindows, a =>
            {
                FirefoxOptions options = new FirefoxOptions();
                options.LogLevel = FirefoxDriverLogLevel.Fatal;
                FirefoxDriverService service = FirefoxDriverService.CreateDefaultService(geckoDriverPath.Replace(@"\geckodriver.exe", ""), "geckodriver.exe");
                drivers[a] = new FirefoxDriver(service, options);
                drivers[a].Manage().Window.Size = windowSize;

                for (int b = a; b < locData.Length; b += maxWindows)
                {
                    if (Web.GetPanoID(locData[b]).StartsWith("CAosSLEF"))
                        locData[b] = Web.GetExact(Web.GetGooglePanoID(locData[b], 100));

                    drivers[a].Navigate().GoToUrl(Get.StreetviewURL(locData[b], bearings[b], pitch));

                    Wait(drivers[a]);

                    placesNames[b] = drivers[a].Title.Replace("Google Maps", "");
                    if (placesNames[b].EndsWith(" - "))
                        placesNames[b] = placesNames[b].Remove(placesNames[b].Length - 3);

                    RemoveElementsByClassName(drivers[a], new string[] {
                        "widget-titlecard widget-titlecard-show-spotlight-link widget-titlecard-show-settings-menu",
                        "widget-image-header",
                        "scene-footer-container noprint",
                        "widget-minimap",
                        "app-vertical-widget-holder noprint",
                        "app-horizontal-widget-holder noprint",
                        "watermark watermark-imagery"
                    });

                    drivers[a].GetScreenshot().SaveAsFile(folderPath + @"\image" + b + "." + format.ToString().ToLower(), format);
                }
                drivers[a].Quit();
            });

            return placesNames;
        }

        /// <summary>
        /// Downloads a panorama for every point in a location data array.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <param name="folderPath">The folder into which every panorama will be downloaded.</param>
        /// <param name="format">The image format of the output panoramas.</param>
        /// <param name="width">The width of each output panorama. Panoramas have an aspect ratio of 2:1</param>
        /// <param name="height">The height of each output panorama. Panoramas have an aspect ratio of 2:1</param>
        /// /// <param name="runInParallel">Whether to download images in parallel or not. Will result in significant memory usage if true.</param>
        public static void AllPanoramas((double Lat, double Lon)[] locData, string folderPath, ImageFormat format, int width, int height, bool runInParallel = true)
        {
            string[] panoIDs = Get.GooglePanoIDs(locData);

            if (runInParallel)
                Parallel.For(0, panoIDs.Length, i => {
                    Modify.ResizeImage(Panorama(panoIDs[i]), width, height).Save(folderPath + @"\image" + i + "." + format.ToString().ToLower(), format);
                });
            else
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
        /// <param name="runInParallel">Whether to download images in parallel or not. Will result in significant memory usage if true.</param>
        public static void AllPanoramas((double Lat, double Lon)[] locData, string folderPath, ImageFormat format, bool runInParallel = true)
        {
            string[] panoIDs = Get.GooglePanoIDs(locData);

            if (runInParallel)
                Parallel.For(0, panoIDs.Length, i => {
                    Panorama(panoIDs[i]).Save(folderPath + @"\image" + i + "." + format.ToString().ToLower(), format);
                });
            else
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
        /// <param name="pitch"> </param>
        /// <param name="folderPath">The The folder into which every screenshot will be saved.</param>
        /// <param name="format">The format in which to save every image.</param>
        /// <returns>An array of basic road/place names with one for each point given from the location data array.</returns>
        public static string[] AllScreenshots((double Lat, double Lon)[] locData, double[] bearings, int resX, int resY, double pitch, string folderPath, ScreenshotImageFormat format = ScreenshotImageFormat.Jpeg)
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
                if (Web.GetPanoID(locData[i]).StartsWith("CAosSLEF"))
                    locData[i] = Web.GetExact(Web.GetGooglePanoID(locData[i], 100));

                driver.Navigate().GoToUrl(Get.StreetviewURL(locData[i], bearings[i], pitch));

                Wait(driver);

                placesNames[i] = driver.Title.Replace("Google Maps", "");
                if (placesNames[i].EndsWith(" - "))
                    placesNames[i] = placesNames[i].Remove(placesNames[i].Length - 3);

                RemoveElementsByClassName(driver, new string[] {
                    "widget-titlecard widget-titlecard-show-spotlight-link widget-titlecard-show-settings-menu",
                    "widget-image-header",
                    "scene-footer-container noprint",
                    "widget-minimap",
                    "app-vertical-widget-holder noprint",
                    "app-horizontal-widget-holder noprint",
                    "watermark watermark-imagery"
                });

                driver.GetScreenshot().SaveAsFile(folderPath + @"\image" + i + "." + format.ToString().ToLower(), format);
            }
            driver.Quit();

            return placesNames;
        }

        private static void Wait(FirefoxDriver driver)
        {
            int tries = 0;
            while (tries < 30)
            {
                try {
                    if (driver.FindElementByClassName("widget-minimap-shim").Displayed)
                        break;
                } catch { tries++; }
                Thread.Sleep(500);
            }
        }

        private static void RemoveElementsByClassName(FirefoxDriver driver, string[] elements)
        {
            foreach (string element in elements)
            {
                bool finished = false;
                int tries = 0;
                while (!finished && tries < 3)
                    try
                    {
                        driver.ExecuteScript("return document.getElementsByClassName('" + element + "')[0].remove();");
                        finished = true;
                    }
                    catch
                    {
                        Thread.Sleep(500);
                        tries++;
                    }
            }
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
