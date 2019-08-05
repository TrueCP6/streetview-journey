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
        /// <summary>
        /// The path to the folder containing ffmpeg.exe, ffprobe.exe, etc
        /// </summary>
        public static string ffmpegExecutablesPath;

        //public static void ToVideo()
        //{
        //    FFmpeg.ExecutablesPath = ffmpegExecutablesPath;
        //}

        /// <summary>
        /// Saves a location data array as a .svj file.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <param name="destinationFile">The path of the destination file.</param>
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
