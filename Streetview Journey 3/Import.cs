using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Streetview_Journey_3
{
    class Import
    {
        public static (double Lat, double Lon)[] Auto(string filePath)
        {
            if (filePath.EndsWith(".gpx"))
                return GPX(filePath);
            if (filePath.EndsWith(".svj"))
                return SVJ(filePath);
            throw new FileLoadException();
        }

        public static (double Lat, double Lon)[] SVJ(string filePath)
        {
            string[] svj = File.ReadAllLines(filePath);

            (double Lat, double Lon)[] tuples = new (double Lat, double Lon)[svj.Length / 2];
            for (int i = 0; i < svj.Length; i += 2)
                tuples[i / 2] = (Convert.ToDouble(svj[i]), Convert.ToDouble(svj[i + 1]));

            return tuples;
        }

        public static (double Lat, double Lon)[] GPX(string filePath)
        {
            string[] gpx = File.ReadAllLines(filePath);

            List<string> unsorted = new List<string>();
            foreach (string line in gpx)
                if (line.Contains("trkpt "))
                    foreach (Match m in Regex.Matches(line, @"[-+]?\d+(?:\.\d+)?"))
                        unsorted.Add(Convert.ToString(m));

            (double Lat, double Lon)[] tuples = new (double Lat, double Lon)[unsorted.Count / 2];
            for (int i = 0; i < unsorted.Count; i += 2)
                tuples[i / 2] = (Convert.ToDouble(unsorted[i]), Convert.ToDouble(unsorted[i+1]));

            return tuples;
        }
    }
}
