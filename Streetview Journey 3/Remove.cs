using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetview_Journey_3
{
    class Remove
    {
        public static (double Lat, double Lon)[] Dupes((double Lat, double Lon)[] locData)
        {
            return locData.Distinct().ToArray();
        }

        public static string[] Nulls(string[] array)
        {
            List<string> points = new List<string>();
            foreach (string strng in array)
                if (strng != null)
                    points.Add(strng);
            return points.ToArray();
        }

        public static (double Lat, double Lon)[] Zeroes((double Lat, double Lon)[] locData)
        {
            List<(double Lat, double Lon)> points = new List<(double Lat, double Lon)>();
            foreach ((double Lat, double Lon) point in locData)
                if (point != (0, 0))
                    points.Add(point);
            return points.ToArray();
        }
    }
}
