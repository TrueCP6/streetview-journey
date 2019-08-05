using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Streetview_Journey_3
{
    class Get
    {
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        private enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117
        }

        /// <summary>
        /// Gets the windows display (DPI) scaling factor set. 1.5 = 150%
        /// </summary>
        /// <returns>The windows display (DPI) scaling factor set. 1.5 = 150%</returns>
        public static double DisplayScalingFactor()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            double ScreenScalingFactor = (double)PhysicalScreenHeight / (double)LogicalScreenHeight;

            return ScreenScalingFactor;
        }

        /// <summary>
        /// Gets an array of 1st Party panorama IDs from a location data array.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <param name="searchRadius">The search radius in meters for each point.</param>
        /// <returns>An array of 1st Party panorama IDs.</returns>
        public static string[] GooglePanoIDs((double Lat, double Lon)[] locData, int searchRadius = 50)
        {
            string[] outStrings = new string[locData.Length];
            Parallel.For(0, outStrings.Length, i =>
            {
                outStrings[i] = Web.GetGooglePanoID(locData[i], searchRadius);
            });
            return Remove.Nulls(outStrings);
        }

        /// <summary>
        /// Gets an array of 1st or 3rd Party panorama IDs from a location data array.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <param name="searchRadius">The search radius in meters for each point.</param>
        /// <returns>An array of panorama IDs.</returns>
        public static string[] PanoIDs((double Lat, double Lon)[] locData, int searchRadius = 50)
        {
            string[] outStrings = new string[locData.Length];
            Parallel.For(0, outStrings.Length, i =>
            {
                outStrings[i] = Web.GetPanoID(locData[i], searchRadius);
            });
            return Remove.Nulls(outStrings);
        }

        /// <summary>
        /// Gets a random 1st party panorama ID from a random point on Earth.
        /// </summary>
        /// <returns>A random 1st party panorama ID from a random point on Earth.</returns>
        public static string RandomGooglePanoID()
        {
            Random rng = new Random();
            return Web.GetGooglePanoID((rng.NextDouble() * 180 + -90, rng.NextDouble() * 360 + -180), 20000000);
        }

        /// <summary>
        /// Gets a URL to a low resolution thumbnail of a panorama.
        /// </summary>
        /// <param name="panoID">A 1st or 3rd party panorama ID.</param>
        /// <param name="width">The width of the desired image.</param>
        /// <param name="height">The height of the desired image.</param>
        /// <returns>Returns a URL to a low resolution thumbnail of a panorama.</returns>
        public static string ThumbnailURL(string panoID, int width, int height)
        {
            return "http://maps.google.com/cbk?output=thumbnail&w=" + width+"&h="+height+"&panoid="+panoID;
        }

        /// <summary>
        /// Gets an array of a desired length of random panorama IDs each from a random point on Earth.
        /// </summary>
        /// <param name="count">The desired length of the array.</param>
        /// <returns>An array of a desired length of random panorama IDs each from a random point on Earth.</returns>
        public static string[] UniquePanoIDs(int count)
        {
            var panoids = new List<string>();
            while (panoids.Count < count)
            {
                panoids.Add(Web.GetPanoID(RandomUsablePoint()));
                panoids = Remove.Nulls(panoids.ToArray()).Distinct().ToList();
            }
            return panoids.ToArray();
        }

        /// <summary>
        ///Gets a random point at the location of a 1st party panorama at a random place on Earth.
        /// </summary>
        /// <returns>A random point at the location of a 1st party panorama at a random place on Earth.</returns>
        public static (double Lat, double Lon) RandomUsablePoint()
        {
            Random rng = new Random();
            return Web.GetExact((rng.NextDouble() * 180 + -90, rng.NextDouble() * 360 + -180), 20000000);
        }

        /// <summary>
        /// Gets the URL to a 1st party panorama tile.
        /// </summary>
        /// <param name="panoID">1st party panorama ID.</param>
        /// <param name="x">X co-ordinate.</param>
        /// <param name="y">Y co-ordinate.</param>
        /// <param name="zoomLevel">Level of zoom. Maximum of 5 and minimum of 0.</param>
        /// <returns></returns>
        public static string TileURL(string panoID, int x, int y, int zoomLevel = 5)
        {
            return "http://maps.google.com/cbk?output=tile&panoid=" + panoID + "&zoom=" + zoomLevel + "&x=" + x + "&y=" + y;
        }

        /// <summary>
        /// Gets the URL to a street view page.
        /// </summary>
        /// <param name="point">A latitude-longitude point.</param>
        /// <param name="bearing">A bearing value from 0 to 360.</param>
        /// <param name="pitch">A pitch value from -90 to 90.</param>
        /// <returns>The URL to a street view page.</returns>
        public static string StreetviewURL((double Lat, double Lon) point, double bearing, double pitch)
        {
            return "http://maps.google.com/maps?q=&layer=c&cbll=" + point.Lat + "," + point.Lon + "&cbp=11," + bearing + ",0,0," + pitch;
        }

        /// <summary>
        /// Gets a URL to a static street view image.
        /// </summary>
        /// <param name="point">A latitude-longitude point.</param>
        /// <param name="bearing">A bearing value from 0 to 360.</param>
        /// <param name="pitch">A pitch value from -90 to 90.</param>
        /// <param name="resX">Width of output image.</param>
        /// <param name="resY">Height of output image.</param>
        /// <param name="fov">Field of view of output image. Maximum is 120.</param>
        /// <returns>A URL to a static street view image.</returns>
        public static string ImageURL((double Lat, double Lon) point, double bearing, double pitch, int resX, int resY, int fov)
        {
            return Web.Sign("https://maps.googleapis.com/maps/api/streetview?size=" + resX + "x" + resY + "&location=" + point.Lat + "," + point.Lon + "&heading=" + bearing + "&pitch=" + pitch + "&fov=" + fov + "&key=" + Web.apiKey, Web.signingKey);
        }

        /// <summary>
        /// Gets a multilined string of all distances of a location data array.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <returns>A multilined string of all distances of a location data array.</returns>
        public static string DistancesString((double Lat, double Lon)[] locData)
        {
            double[] distances = Distances(locData);
            string output = "";
            foreach (double distance in distances)
                output += distance + Environment.NewLine;
            return output;
        }

        /// <summary>
        /// Gets an array of the distances between each point in meters from a location data array.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <returns>An array of the distances between each point in meters.</returns>
        public static double[] Distances((double Lat, double Lon)[] locData)
        {
            double[] distances = new double[locData.Length - 1];
            for (int i = 0; i < distances.Length; i++)
                distances[i] = Calculate.Distance(locData[i], locData[i + 1]);
            return distances;
        }

        /// <summary>
        /// Calculates the average distance in meters between each point of a location data array.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <returns>The average distance in meters between each point.</returns>
        public static double AverageDistance((double Lat, double Lon)[] locData)
        {
            return TotalDistance(locData) / Convert.ToDouble(locData.Length);
        }

        /// <summary>
        /// Calculates the total distance in meters of a location data array.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <returns>The total distance in meters between each point.</returns>
        public static double TotalDistance((double Lat, double Lon)[] locData)
        {
            return Distances(locData).Sum();
        }

        /// <summary>
        /// Gets a multilined string of each point from a location data array.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <returns>A multilined string of each point from a location data array.</returns>
        public static string String((double Lat, double Lon)[] locData)
        {
            string outstring = "";
            foreach ((double Lat, double Lon) point in locData)
                outstring += point.Lat + ", " + point.Lon + Environment.NewLine;
            return outstring;
        }

        /// <summary>
        /// Snaps each point from a location data array to the position of the nearest panorama it can.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <param name="searchRadius">The search radius in meters for each point.</param>
        /// <returns>A location data array of snapped points.</returns>
        public static (double Lat, double Lon)[] ExactCoords((double Lat, double Lon)[] locData, int searchRadius = 50)
        {
            (double Lat, double Lon)[] points = new (double Lat, double Lon)[locData.Length];
            Parallel.For(0, locData.Length, i =>
            {
                points[i] = Web.GetExact(locData[i], searchRadius);
            });

            return Remove.Zeroes(points);
        }

        /// <summary>
        /// Gets a multilined string of panorama IDs from a location data array.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <param name="searchRadius">The search radius in meters for each panorama.</param>
        /// <returns>A multilined string of panorama IDs from a location data array.</returns>
        public static string PanoIDsString((double Lat, double Lon)[] locData, int searchRadius = 50)
        {
            string[] outStrings = PanoIDs(locData, searchRadius);

            string returnString = "";
            foreach (string id in outStrings)
                returnString += id + Environment.NewLine;
            return returnString;
        }
    }
}
