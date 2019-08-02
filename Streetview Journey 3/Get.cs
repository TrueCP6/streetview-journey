using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetview_Journey_3
{
    class Get
    {
        public static string[] GooglePanoIDs((double Lat, double Lon)[] locData, int searchRadius = 50)
        {
            string[] outStrings = new string[locData.Length];
            Parallel.For(0, outStrings.Length, i =>
            {
                outStrings[i] = Web.GetGooglePanoID(locData[i], searchRadius);
            });
            return Remove.Nulls(outStrings);
        }

        public static string[] PanoIDs((double Lat, double Lon)[] locData, int searchRadius = 50)
        {
            string[] outStrings = new string[locData.Length];
            Parallel.For(0, outStrings.Length, i =>
            {
                outStrings[i] = Web.GetPanoID(locData[i], searchRadius);
            });
            return Remove.Nulls(outStrings);
        }

        public static string RandomGooglePanoID()
        {
            Random rng = new Random();
            return Web.GetGooglePanoID((rng.NextDouble() * 180 + -90, rng.NextDouble() * 360 + -180), 20000000);
        }

        public static string ThumbnailURL(string panoID, int width, int height)
        {
            return "http://maps.google.com/cbk?output=thumbnail&w=" + width+"&h="+height+"&panoid="+panoID;
        }

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

        public static (double Lat, double Lon) RandomUsablePoint()
        {
            Random rng = new Random();
            return Web.GetExact((rng.NextDouble() * 180 + -90, rng.NextDouble() * 360 + -180), 20000000);
        }

        public static string TileURL(string panoID, int x, int y, int zoomLevel = 5)
        {
            return "http://maps.google.com/cbk?output=tile&panoid=" + panoID + "&zoom=" + zoomLevel + "&x=" + x + "&y=" + y;
        }

        public static string StreetviewURL((double Lat, double Lon) point, double bearing, double pitch)
        {
            return "http://maps.google.com/maps?q=&layer=c&cbll=" + point.Lat + "," + point.Lon + "&cbp=11," + bearing + ",0,0," + pitch;
        }

        public static string ImageURL((double Lat, double Lon) point, double bearing, double pitch, int resX, int resY, int fov)
        {
            return Web.Sign("https://maps.googleapis.com/maps/api/streetview?size=" + resX + "x" + resY + "&location=" + point.Lat + "," + point.Lon + "&heading=" + bearing + "&pitch=" + pitch + "&fov=" + fov + "&key=" + Web.apiKey, Web.signingKey);
        }

        public static string DistancesString((double Lat, double Lon)[] locData)
        {
            double[] distances = Distances(locData);
            string output = "";
            foreach (double distance in distances)
                output += distance + Environment.NewLine;
            return output;
        }

        public static double[] Distances((double Lat, double Lon)[] locData)
        {
            double[] distances = new double[locData.Length - 1];
            for (int i = 0; i < distances.Length; i++)
                distances[i] = Calculate.Distance(locData[i], locData[i + 1]);
            return distances;
        }

        public static double AverageDistance((double Lat, double Lon)[] locData)
        {
            return TotalDistance(locData) / Convert.ToDouble(locData.Length);
        }

        public static double TotalDistance((double Lat, double Lon)[] locData)
        {
            return Distances(locData).Sum();
        }

        public static string String((double Lat, double Lon)[] locData)
        {
            string outstring = "";
            foreach ((double Lat, double Lon) point in locData)
                outstring += point.Lat + ", " + point.Lon + Environment.NewLine;
            return outstring;
        }

        public static (double Lat, double Lon)[] ExactCoords((double Lat, double Lon)[] locData, int searchRadius = 50)
        {
            (double Lat, double Lon)[] points = new (double Lat, double Lon)[locData.Length];
            Parallel.For(0, locData.Length, i =>
            {
                points[i] = Web.GetExact(locData[i], searchRadius);
            });

            return Remove.Zeroes(points);
        }

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
