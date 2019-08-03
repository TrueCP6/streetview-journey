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
using OpenQA;

namespace Streetview_Journey_3
{
    class Download
    {
        public static void AllPanoramas((double Lat, double Lon)[] locData, string folderPath, ImageFormat format, int width, int height)
        {
            string[] panoIDs = Get.GooglePanoIDs(locData);

            for (int i = 0; i < panoIDs.Length; i++)
                Modify.ResizeImage(Panorama(panoIDs[i]), width, height).Save(folderPath + @"\image" + i + "." + format.ToString().ToLower(), format);
        }

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

        public static void AllPanoramas((double Lat, double Lon)[] locData, string folderPath, ImageFormat format)
        {
            string[] panoIDs = Get.GooglePanoIDs(locData);

            for (int i = 0; i < panoIDs.Length; i++)
                Panorama(panoIDs[i]).Save(folderPath + @"\image" + i + "." + format.ToString().ToLower(), format);
        }

        public static string geckoDriverPath = AppDomain.CurrentDomain.BaseDirectory + "geckodriver.exe";
        public static string[] AllScreenshots((double Lat, double Lon)[] locData, double[] bearings, int resX, int resY, double pitch, double wait, string folderPath, ScreenshotImageFormat format = ScreenshotImageFormat.Jpeg)
        {
            FirefoxDriverService service = FirefoxDriverService.CreateDefaultService(geckoDriverPath.Replace(@"\geckodriver.exe", ""), "geckodriver.exe");
            var options = new FirefoxOptions();
            options.LogLevel = FirefoxDriverLogLevel.Fatal;
            var driver = new FirefoxDriver(service, options);
            double scaling = Get.DisplayScalingFactor();
            driver.Manage().Window.Size = new Size(
                Convert.ToInt32(Math.Round(Convert.ToDouble(resX + 12) / scaling)),
                Convert.ToInt32(Math.Round(Convert.ToDouble(resY + 80) / scaling))
            );

            string[] placesNames = new string[locData.Length];
            for (int i = 0; i < locData.Length; i++)
            {
                driver.Navigate().GoToUrl(Get.StreetviewURL(locData[i], bearings[i], pitch));
                Thread.Sleep(Convert.ToInt32(wait * 1000));
                placesNames[i] = driver.Title.Replace("Google Maps", "");
                if (placesNames[i].EndsWith(" - "))
                    placesNames[i] = placesNames[i].Replace(" - ", "");
                driver.GetScreenshot().SaveAsFile(folderPath + @"\image" + i + "." + format.ToString().ToLower(), format);
            }
            driver.Quit();

            return placesNames;
        }

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
