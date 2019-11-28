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
    public class Route
    {
        public Route() { }

        public Route(string filePath)
        {
            Route rt = FromFile(filePath);
            Points = rt.Points;
        }

        public Route(Point[] points)
        {
            Points = points;
        }

        public Point[] Points;

        public static Route FromSVJ(string path)
        {
            string[] svj = File.ReadAllLines(path);

            Route rt = new Route();
            rt.Points = new Point[svj.Length / 2];

            for (int i = 0; i < svj.Length; i += 2)
                rt.Points[i / 2] = new Point(Convert.ToDouble(svj[i]), Convert.ToDouble(svj[i + 1]));

            return rt;
        }

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

        public static Route FromFile(string path)
        {
            if (path.EndsWith(".gpx"))
                return FromGPX(path);
            else if (path.EndsWith(".svj"))
                return FromSVJ(path);
            else
                throw new FileLoadException("Unrecognised file type. Use a .gpx or .svj file instead.");
        }

        public override string ToString()
        {
            string points = "";
            foreach (Point pt in Points)
                points += pt.ToString() + Environment.NewLine;
            return points;
        }

        public static implicit operator string(Route rt) => rt.ToString();

        public Route GetBearings()
        {
            Point[] pts = Points;
            for (int i = 0; i < pts.Length - 1; i++)
                pts[i].Bearing = pts[i].BearingTo(pts[i+1]);
            pts[pts.Length - 1] = pts[pts.Length - 2];
            return new Route(pts);
        }

        public Route Exact(int searchRadius = 50)
        {
            Point[] pts = Points;
            Parallel.For(0, pts.Length, i =>
            {
                try
                {
                    pts[i] = pts[i].Exact(searchRadius);
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

        public Route RemoveDuplicates() =>
            new Route(Points.Distinct().ToArray());

        public Route Trim(int trimTo)
        {
            Point[] trimmed = new Point[trimTo];
            double modifier = (double)Points.Length / (double)trimTo;
            for (int i = 0; i < trimTo; i++)
                trimmed[i] = Points[(int)Math.Round(i * modifier)];
            return new Route(trimmed);
        }

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

        public double AverageDistance
        {
            get => TotalDistance / Points.Length;
        }

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

        public int Length
        {
            get => Points.Length;
        }

        public Route SmoothBearings(int smoothValue)
        {
            Point[] points = Points;
            for (int i = 0; i < points.Length - smoothValue; i++)
                points[i].Bearing = Bearing.Average(new ArraySegment<Point>(points, i, smoothValue).ToArray());
            return new Route(points);
        }

        public static int MaximumSmooth = 10;

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

        public Route TrackPoint(Point trackedPoint)
        {
            Point[] pts = Points;
            for (int i = 0; i < pts.Length; i++)
                pts[i].Bearing = pts[i].BearingTo(trackedPoint);
            return new Route(pts);
        }

        public Route Reverse() =>
            new Route(Points.Reverse().ToArray());

        public bool AllUsable(int searchRadius = 50)
        {
            bool usable = true;
            Parallel.ForEach(Points, (pt, state) => {
                if (!pt.IsUsable(searchRadius))
                {
                    usable = false;
                    state.Break();
                }
            });
            return usable;
        }

        public Route Interpolate(double desiredMperPoint, int searchRadius = 50) =>
            new Route(InterpolateArray(Exact(searchRadius).Points, searchRadius, desiredMperPoint));

        private static Point[] InterpolateArray(Point[] arr, int searchRadius, double mpp)
        {
            Point[][] jPts = new Point[arr.Length][];
            Parallel.For(0, jPts.Length - 1, a =>
            {
                if (arr[a].DistanceTo(arr[a + 1]) > mpp) //if the desired distance per point has been reached
                {
                    Point exactMid = new Point(); //this 0, 0 point wont be used (only to stop an error)
                    try
                    {
                        exactMid = arr[a].Midpoint(arr[a + 1]).Exact(searchRadius);
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

                    jPts[a] = InterpolateArray(new Point[] { arr[a], exactMid, arr[a + 1] }, searchRadius, mpp);
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

        public Route Smoothen() => SmoothTrim(Length);

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

        public Route(PanoID[] panoIDs, int searchRadius = 50)
        {
            Point[] pts = new Point[panoIDs.Length];
            Parallel.For(0, pts.Length, i => {
                try
                {
                    pts[i] = panoIDs[i].Exact(searchRadius);
                }
                catch (ZeroResultsException)
                {
                    pts[i] = new Point();
                    pts[i].usable = false;
                }
            });
            Points = RemoveUnusablePoints(pts);
        }

        public ImageFolder DownloadAllPanoramas(ImageFolder folder, ImageFileFormat format, Size size, int searchRadius = 50, bool parallel = true)
        {
            PanoID[] ids = PanoIDs(true, searchRadius);
            ImageFormat frmt = GetFormat(format);

            if (parallel)
            {
                Parallel.For(0, ids.Length, i => {
                    using (Bitmap pano = ids[i].DownloadPanorama(size))
                        pano.Save(folder.Path + "image" + i + "." + format.ToString().ToLower(), frmt);
                });
            }
            else
            {
                for (int i = 0; i < ids.Length; i++)
                    using (Bitmap pano = ids[i].DownloadPanorama(size))
                        pano.Save(folder.Path + "image" + i + "." + format.ToString().ToLower(), frmt);
            }

            return folder;
        }

        public ImageFolder DownloadAllImages(ImageFolder folder, Size size, int fov, double pitch, bool parallel = true)
        {
            if (Setup.DontBillMe)
                throw new DontBillMeException("You attempted to use a function that will cause you to be billed by Google. Change Setup.DontBillMe to false to stop this exception.");

            if (parallel)
            {
                Parallel.For(0, Length, i=> {
                    using (WebClient client = new WebClient())
                        client.DownloadFile(Points[i].ImageURL(Points[i].Bearing, pitch, size, fov), folder.Path + "image" + i + ".png");
                });
            }
            else
            {
                for (int i = 0; i < Length; i++)
                    using (WebClient client = new WebClient())
                        client.DownloadFile(Points[i].ImageURL(Points[i].Bearing, pitch, size, fov), folder.Path + "image" + i + ".png");
            }
            return folder;
        }

        public ImageFolder DownloadAllScreenshots(ImageFolder folder, Size size, int maxWindows = 1, double pitch = 0, ImageFileFormat format = ImageFileFormat.Jpeg)
        {
            if (!File.Exists(Setup.GeckodriverPath))
                Setup.DownloadGeckodriver();

            string geckoExe = Path.GetFileName(Setup.GeckodriverPath);
            string geckoDir = Path.GetDirectoryName(Setup.GeckodriverPath);

            FirefoxDriver[] drivers = new FirefoxDriver[maxWindows];
            double scaling = Display.ScalingFactor();
            Size windowSize = new Size(
                (int)Math.Round(size.Width / scaling) + 12,
                (int)Math.Round(size.Height / scaling) + 80
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
                    drivers[a].Navigate().GoToUrl(rt.Points[b].StreetviewURL(rt.Points[b].Bearing, pitch));

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

                    drivers[a].GetScreenshot().SaveAsFile(folder.Path+"image"+b+"."+format.ToString().ToLower(), (ScreenshotImageFormat)format);
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
    }
}
