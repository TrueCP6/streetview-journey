using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static StreetviewJourney.Enums;

namespace StreetviewJourney
{
    /// <summary>
    /// Used for storing, saving and modifying point sequences
    /// </summary>
    public class Route
    {
        /// <summary>
        /// Creates a new Route object
        /// </summary>
        public Route() { }

        /// <summary>
        /// Creates a new Route object from a file path
        /// </summary>
        /// <param name="filePath">The path of the gpx or svj file</param>
        public Route(string filePath)
        {
            Points = FromFile(filePath).Points;
        }

        /// <summary>
        /// Creates a new Route object from a Point array
        /// </summary>
        /// <param name="points">The point array to create the Route from</param>
        public Route(Point[] points)
        {
            Points = points;
        }

        /// <summary>
        /// The points making up the Route
        /// </summary>
        public Point[] Points;

        /// <summary>
        /// Creates a Route from an svj file
        /// </summary>
        /// <param name="path">The svj file path</param>
        /// <returns>A Route created from an svj file</returns>
        public static Route FromSVJ(string path)
        {
            string[] svj = File.ReadAllLines(path);

            Route rt = new Route();
            rt.Points = new Point[svj.Length / 2];

            for (int i = 0; i < svj.Length; i += 2)
                rt.Points[i / 2] = new Point(Convert.ToDouble(svj[i]), Convert.ToDouble(svj[i + 1]));

            return rt;
        }

        /// <summary>
        /// Creates a Route from a gpx file
        /// </summary>
        /// <param name="path">The path to the gpx file</param>
        /// <returns>A route created from a gpx file</returns>
        public static Route FromGPX(string path)
        {
            string[] gpx = File.ReadAllLines(path);

            List<string> unsorted = new List<string>();

            foreach (string line in gpx)
                if (line.Contains("trkpt "))
                    foreach (Match m in Regex.Matches(line, @"[-+]?\d+(?:\.\d+)?"))
                        unsorted.Add(Convert.ToString(m));

            Route rt = new Route();
            rt.Points = new Point[unsorted.Count / 2];

            for (int i = 0; i < unsorted.Count; i += 2)
                rt.Points[i / 2] = new Point(Convert.ToDouble(unsorted[i]), Convert.ToDouble(unsorted[i + 1]));

            return rt;
        }

        /// <summary>
        /// Creates a Route from a gpx or svj file
        /// </summary>
        /// <param name="path">The file path</param>
        /// <returns>A Route created from a gpx or svj file</returns>
        public static Route FromFile(string path)
        {
            if (Path.GetExtension(path) == ".gpx")
                return FromGPX(path);
            else if (Path.GetExtension(path) == ".svj")
                return FromSVJ(path);
            else
                throw new FileLoadException("Unrecognised file type. Use a .gpx or .svj file instead.");
        }

        /// <summary>
        /// A string containing every point in the Route
        /// </summary>
        /// <returns>A string containing every point in the Route</returns>
        public override string ToString()
        {
            string points = "";
            foreach (Point pt in Points)
                points += pt.ToString() + Environment.NewLine;
            return points;
        }

        public static implicit operator string(Route rt) => rt.ToString();

        /// <summary>
        /// Gets the bearings for every Point in the Route
        /// </summary>
        /// <returns>A new Route</returns>
        public Route GetBearings()
        {
            Point[] pts = Points;
            for (int i = 0; i < pts.Length - 1; i++)
                pts[i].Bearing = pts[i].BearingTo(pts[i+1]);
            pts[pts.Length - 1] = pts[pts.Length - 2];
            return new Route(pts);
        }

        /// <summary>
        /// Gets the position of the nearest panorama within the search radius for every point
        /// </summary>
        /// <param name="firstParty">Whether each point must be first party</param>
        /// <param name="searchRadius">The radius in metres to search</param>
        /// <returns>A new Route</returns>
        public Route Exact(bool firstParty = false, int searchRadius = 50)
        {
            Point[] pts = Points;
            Parallel.For(0, pts.Length, i =>
            {
                try
                {
                    pts[i] = pts[i].Exact(firstParty, searchRadius);
                }
                catch (ZeroResultsException)
                {
                    pts[i].usable = false;
                }
            });

            return new Route(RemoveUnusablePoints(pts));
        }

        private static Point[] RemoveUnusablePoints(Point[] pts) =>
            pts.Where(pt => pt.usable).ToArray();

        /// <summary>
        /// Removes all duplicate points
        /// </summary>
        /// <returns>A new Route</returns>
        public Route RemoveDuplicates() =>
            new Route(Points.Distinct().ToArray());

        /// <summary>
        /// Trims the points to a new length
        /// </summary>
        /// <param name="trimTo">The new length</param>
        /// <returns></returns>
        public Route Trim(int trimTo)
        {
            Point[] trimmed = new Point[trimTo];
            double modifier = (double)Length / (double)trimTo;
            for (int i = 0; i < trimTo; i++)
                trimmed[i] = Points[(int)Math.Round(i * modifier)];
            return new Route(trimmed);
        }

        /// <summary>
        /// The total distance in meters the length of the Route covers
        /// </summary>
        [JsonIgnore]
        public double TotalDistance
        {
            get
            {
                double dist = 0;
                for (int i = 0; i < Points.Length - 1; i++)
                    dist += Points[i].DistanceTo(Points[i + 1]);
                return dist;
            }
        }

        /// <summary>
        /// The average distance in meters between each point
        /// </summary>
        [JsonIgnore]
        public double AverageDistance =>
            TotalDistance / Points.Length;

        /// <summary>
        /// Saves the Route's points to an svj file 
        /// </summary>
        /// <param name="filePath">The path of the svj file to save as</param>
        public void Save(string filePath)
        {
            List<string> str = new List<string>();
            foreach (Point pt in Points)
            {
                str.Add(pt.Latitude.ToString());
                str.Add(pt.Longitude.ToString());
            }
            File.WriteAllLines(filePath, str.ToArray());
        }

        /// <summary>
        /// The amount of points the Route contains
        /// </summary>
        [JsonIgnore]
        public int Length => Points.Length;

        /// <summary>
        /// Smoothes out the bearings of a Route according to an int value
        /// </summary>
        /// <param name="smoothValue">The amount of bearings to smooth together</param>
        /// <returns>A new Route</returns>
        public Route SmoothBearings(int smoothValue)
        {
            Point[] points = Points;
            for (int i = 0; i < points.Length - smoothValue; i++)
                points[i].Bearing = Bearing.Average(new ArraySegment<Point>(points, i, smoothValue).ToArray());
            return new Route(points);
        }

        /// <summary>
        /// The maximum amount of bearings that can be smoothed into eachother. Has a default of 10.
        /// </summary>
        public static int MaximumSmooth = 10;

        /// <summary>
        /// Determines a smooth value and smoothes out the bearings of the Route according to it
        /// </summary>
        /// <returns>A new Route</returns>
        public Route SmoothBearings()
        {
            double[] difference = new double[Length];
            for (int i = 0; i < difference.Length - 1; i++)
                difference[i] = (Points[i].Bearing - Points[i + 1].Bearing).Value;
            difference[difference.Length - 1] = 0;

            int[] maxSmooths = new int[Length - MaximumSmooth];
            for (int a = 0; a < maxSmooths.Length; a++)
            {
                double sum = 0;
                maxSmooths[a] = MaximumSmooth;
                for (int b = 0; b < MaximumSmooth; b++)
                {
                    sum += difference[a + b];
                    if (sum >= 90)
                    {
                        maxSmooths[a] = b;
                        break;
                    }
                }
            }

            return SmoothBearings((int)Math.Round(maxSmooths.Average()));
        }

        /// <summary>
        /// Calculates the bearings for an entire Route to face a certain point
        /// </summary>
        /// <param name="trackedPoint">The point to track</param>
        /// <returns>A new Route</returns>
        public Route TrackPoint(Point trackedPoint)
        {
            Point[] pts = Points;
            for (int i = 0; i < pts.Length; i++)
                pts[i].Bearing = pts[i].BearingTo(trackedPoint);
            return new Route(pts);
        }

        /// <summary>
        /// Reverses the order of the points in the Route
        /// </summary>
        /// <returns>A new Route</returns>
        public Route Reverse() =>
            new Route(Points.Reverse().ToArray());

        /// <summary>
        /// Whether all the points in the Route have a panorama within the defined search radius
        /// </summary>
        /// <param name="searchRadius">The radius to search in meters for each point</param>
        /// <param name="firstParty">Whether each point must be first party</param>
        /// <returns>A new Route</returns>
        public bool AllUsable(bool firstParty = false, int searchRadius = 50)
        {
            bool usable = true;
            Parallel.ForEach(Points, (pt, state) => {
                if (!pt.IsUsable(firstParty, searchRadius))
                {
                    usable = false;
                    state.Break();
                }
            });
            return usable;
        }

        /// <summary>
        /// Interpolates points between each point to attempt to reach the desired metres per point
        /// </summary>
        /// <param name="desiredMperPoint">The desired metre distance between each point</param>
        /// <param name="firstParty">Whether each point must be third party</param>
        /// <param name="searchRadius">The radius in metres to search for each point</param>
        /// <returns>A new Route</returns>
        public Route Interpolate(double desiredMperPoint, bool firstParty = false, int searchRadius = 50) =>
            new Route(InterpolateArray(Exact(firstParty, searchRadius).Points, searchRadius, desiredMperPoint, firstParty));

        private static Point[] InterpolateArray(Point[] arr, int searchRadius, double mpp, bool firstParty)
        {
            Point[][] jPts = new Point[arr.Length][];
            Parallel.For(0, jPts.Length - 1, a =>
            {
                if (arr[a].DistanceTo(arr[a + 1]) > mpp) //if the desired distance per point has been reached
                {
                    Point exactMid = new Point(); //this 0, 0 point wont be used (only to stop an error)
                    try
                    {
                        exactMid = arr[a].Midpoint(arr[a + 1]).Exact(firstParty, searchRadius);
                    }
                    catch (ZeroResultsException) //if there are no panoramas found between in the search radius
                    {
                        jPts[a] = new Point[] { arr[a], arr[a + 1] };
                        return; //same as continue in parallel
                    }

                    if (exactMid == arr[a] || exactMid == arr[a + 1]) //if there are no panoramas inbetween (checking for dupes)
                    {
                        jPts[a] = new Point[] { arr[a], arr[a + 1] };
                        return;
                    }

                    jPts[a] = InterpolateArray(new Point[] { arr[a], exactMid, arr[a + 1] }, searchRadius, mpp, firstParty);
                }
                else
                    jPts[a] = new Point[] { arr[a], arr[a + 1] };
            });
            jPts[jPts.Length - 1] = new Point[] {arr.Last()};

            List<Point> flat = new List<Point>();
            foreach (Point[] pts in jPts)
                foreach (Point pt in pts)
                    flat.Add(pt);

            return flat.Distinct().ToArray();
        }

        /// <summary>
        /// Smoothly trims the Route to a determined length
        /// </summary>
        /// <param name="trimTo">The desired new length of the Route</param>
        /// <returns>A new Route</returns>
        public Route SmoothTrim(int trimTo)
        {
            double[] distances = new double[Length];
            distances[0] = 0;
            for (int i = 1; i < Length; i++)
                distances[i] = Points[i - 1].DistanceTo(Points[i]) + distances[i - 1];

            double mpp = TotalDistance / trimTo;
            Point[] pts = new Point[trimTo];
            Parallel.For(0, trimTo, a => {
                pts[a] = Points[ClosestIndex(distances, a * mpp)];
            });

            return new Route(pts);
        }

        private static int ClosestIndex(double[] distances, double value)
        {
            int closest = 0;
            for (int i = 0; i < distances.Length; i++)
                if (Math.Abs(distances[closest] - value) > Math.Abs(distances[i] - value))
                    closest = i;
            return closest;
        }

        /// <summary>
        /// Attempts to bring the distance between points in the Route closer to the average. May result in more duplicate points
        /// </summary>
        /// <returns>A new Route</returns>
        public Route Smoothen() => SmoothTrim(Length);

        /// <summary>
        /// Gets the PanoID for each point of the Route
        /// </summary>
        /// <param name="firstParty">Whether the PanoIDs must be first party</param>
        /// <param name="searchRadius">The radius in meters to search for each point</param>
        /// <returns>A new Route</returns>
        public PanoID[] PanoIDs(bool firstParty = false, int searchRadius = 50)
        {
            PanoID[] ids = new PanoID[Length];
            Parallel.For(0, Length, i => {
                try
                {
                    ids[i] = Points[i].PanoID(firstParty, searchRadius);
                }
                catch (ZeroResultsException)
                {
                    ids[i] = new PanoID();
                }
            });
            return ids.Where(id => id.ID != null).ToArray();
        }

        /// <summary>
        /// Creates a Route from an array of PanoIDs
        /// </summary>
        /// <param name="panoIDs">The PanoIDs to create the Route from</param>
        public Route(PanoID[] panoIDs)
        {
            Point[] pts = new Point[panoIDs.Length];
            Parallel.For(0, pts.Length, i => {
                try
                {
                    pts[i] = panoIDs[i].Position;
                }
                catch (ZeroResultsException)
                {
                    pts[i] = new Point();
                    pts[i].usable = false;
                }
            });
            Points = RemoveUnusablePoints(pts);
        }

        /// <summary>
        /// Downloads and saves a 360 equirectangular image for every point
        /// </summary>
        /// <param name="folder">The ImageFolder where the images should be saved to</param>
        /// <param name="format">The image format to save the images as</param>
        /// <param name="res">The resolution to save the images as with an aspect ratio of 2:1</param>
        /// <param name="searchRadius">The radius in meters to search for each poin</param>
        /// <param name="parallel">Whether to run in parallel. Setting this as true will use significantly more memory but take less time to complete</param>
        /// <returns>The ImageFolder containing all the saved images</returns>
        public ImageFolder DownloadAllPanoramas(ImageFolder folder, ImageFileFormat format, Resolution res, int searchRadius = 50, bool parallel = true)
        {
            PanoID[] ids = PanoIDs(true, searchRadius);
            ImageFormat frmt = GetFormat(format);

            if (parallel)
            {
                Parallel.For(0, ids.Length, i => {
                    using (Bitmap pano = ids[i].DownloadPanorama(res))
                        pano.Save(Path.Combine(folder.Path, "image" + i + "." + format.ToString().ToLower()), frmt);
                });
            }
            else
            {
                for (int i = 0; i < ids.Length; i++)
                    using (Bitmap pano = ids[i].DownloadPanorama(res))
                        pano.Save(Path.Combine(folder.Path, "image" + i + "." + format.ToString().ToLower()), frmt);
            }

            return folder;
        }

        /// <summary>
        /// Downloads an image for every point in the Route using the Static Streetview API
        /// </summary>
        /// <param name="folder">The folder to save the images to</param>
        /// <param name="res">The desired output resolution of each image</param>
        /// <param name="fov">The field of view of each image</param>
        /// <param name="pitch">The pitch of each image</param>
        /// <param name="parallel">Whether to run the process in parallel</param>
        /// <returns>The folder where all the images were saved</returns>
        public ImageFolder DownloadAllImages(ImageFolder folder, Resolution res, int fov, double pitch, bool parallel = true)
        {
            if (Setup.DontBillMe)
                throw new DontBillMeException("You attempted to use a function that will cause you to be billed by Google. Change Setup.DontBillMe to false to stop this exception.");

            if (parallel)
            {
                Parallel.For(0, Length, i=> {
                    using (WebClient client = new WebClient())
                        client.DownloadFile(Points[i].ImageURL(pitch, res, fov), Path.Combine(folder.Path, "image" + i + ".png"));
                });
            }
            else
            {
                for (int i = 0; i < Length; i++)
                    using (WebClient client = new WebClient())
                        client.DownloadFile(Points[i].ImageURL(pitch, res, fov), Path.Combine(folder.Path, "image" + i + ".png"));
            }
            return folder;
        }

        /// <summary>
        /// Saves an image for every point in the Route using a Firefox window
        /// </summary>
        /// <param name="folder">The folder to save the images to</param>
        /// <param name="res">The output resolution of each image</param>
        /// <param name="maxWindows">The maximum amount of windows to open at once. Higher uses more memory but will complete faster</param>
        /// <param name="pitch">The pitch in angles for each image to face</param>
        /// <param name="format">The format of the output images</param>
        /// <returns>The folder where all the images were saved</returns>
        public ImageFolder DownloadAllScreenshots(ImageFolder folder, Resolution res, int maxWindows = 1, double pitch = 0, ImageFileFormat format = ImageFileFormat.Jpeg)
        {
            if (!File.Exists(Setup.GeckodriverPath))
                Setup.DownloadGeckodriver();

            string geckoExe = Path.GetFileName(Setup.GeckodriverPath);
            string geckoDir = Path.GetDirectoryName(Setup.GeckodriverPath);

            FirefoxDriver[] drivers = new FirefoxDriver[maxWindows];
            double scaling = Display.ScalingFactor();
            Size windowSize = new Size(
                (int)Math.Round(res.Width / scaling) + 12,
                (int)Math.Round(res.Height / scaling) + 80
            );

            Route rt = RemoveThirdPartyPanoramas();

            Parallel.For(0, maxWindows, a =>
            {
                FirefoxOptions options = new FirefoxOptions();
                options.LogLevel = FirefoxDriverLogLevel.Fatal;
                FirefoxDriverService service = FirefoxDriverService.CreateDefaultService(geckoDir, geckoExe);
                drivers[a] = new FirefoxDriver(service, options);
                drivers[a].Manage().Window.Size = windowSize;

                for (int b = a; b < rt.Length; b += maxWindows)
                {
                    drivers[a].Navigate().GoToUrl(rt.Points[b].StreetviewURL(pitch));

                    Wait(drivers[a]);

                    RemoveElementsByClassName(drivers[a], new string[] {
                        "widget-titlecard widget-titlecard-show-spotlight-link widget-titlecard-show-settings-menu",
                        "widget-image-header",
                        "scene-footer-container noprint",
                        "widget-minimap",
                        "app-vertical-widget-holder noprint",
                        "app-horizontal-widget-holder noprint",
                        "watermark watermark-imagery"
                    });

                    drivers[a].GetScreenshot().SaveAsFile(Path.Combine(folder.Path, "image"+b+"."+format.ToString().ToLower()), (ScreenshotImageFormat)format);
                }

                drivers[a].Quit();
            });

            return folder;
        }

        private static void RemoveElementsByClassName(FirefoxDriver driver, string[] elements)
        {
            foreach (string element in elements)
            {
                for (int tries = 0; tries < 3; tries++)
                {
                    try
                    {
                        driver.ExecuteScript("return document.getElementsByClassName('" + element + "')[0].remove();");
                        break;
                    }
                    catch (WebDriverException) { }
                    Thread.Sleep(500);
                }
            }
        }

        private static void Wait(FirefoxDriver driver)
        {
            for (int tries = 0; tries < 30; tries++)
            {
                try
                {
                    if (driver.FindElementByClassName("widget-minimap-shim").Displayed)
                        break;
                }
                catch (NoSuchElementException) { }
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Removes all third party panoramas from the Route
        /// </summary>
        /// <returns>A new Route</returns>
        public Route RemoveThirdPartyPanoramas()
        {
            Point[] pts = Points;
            Parallel.For(0, pts.Length, i =>
            {
                try
                {
                    if (pts[i].isThirdParty)
                        pts[i].usable = false;
                }
                catch (ZeroResultsException)
                {
                    pts[i].usable = false;
                }
            });
            return new Route(RemoveUnusablePoints(pts));
        }

        /// <summary>
        /// The bearings of each point in the route
        /// </summary>
        [JsonIgnore]
        public Bearing[] Bearings =>
            Points.Select(pt => pt.Bearing).ToArray();
    }
}
