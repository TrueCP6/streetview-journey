using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Xabe.FFmpeg;

namespace Streetview_Journey_3
{
    class Export
    {
        public static string ffmpegExecutablesPath;

        //public static void ToVideo()
        //{
        //    FFmpeg.ExecutablesPath = ffmpegExecutablesPath;
        //}

        public static void ToSVJ((double Lat, double Lon)[] locData, string destinationFile)
        {
            List<string> pointList = new List<string>();
            foreach ((double Lat, double Lon) point in locData)
            {
                pointList.Add(Convert.ToString(point.Lat));
                pointList.Add(Convert.ToString(point.Lon));
            }
            File.WriteAllLines(destinationFile, pointList);
        }
    }
}
