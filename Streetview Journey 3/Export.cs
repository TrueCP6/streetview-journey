using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Enums;

namespace Streetview_Journey_3
{
    class Export
    {
        /// <summary>
        /// The path to the folder containing ffmpeg.exe, ffprobe.exe, etc
        /// </summary>
        public static string ffmpegExecutablesPath {
            get {
                return FFmpeg.ExecutablesPath;
            }
            set {
                FFmpeg.ExecutablesPath = value;
            }
        }

        /// <summary>
        /// Converts a sequence of images in a folder to a video.
        /// </summary>
        /// <param name="framerate">The output framerate of the video.</param>
        /// <param name="inputFolder">The folder to in which to look for the image sequence.</param>
        /// <param name="outputFileName">The name of the output file inluding the file extension.</param>
        /// <param name="preset">Custom preset to be used for conversion.</param>
        /// <param name="multithread">Whether to multithread or not.</param>
        public static void ToVideo(int framerate, string inputFolder, string outputFileName, IConversion preset, bool multithread = true)
        {
            string[] files = Directory.GetFiles(inputFolder + @"\");
            string fileType = "jpg";
            foreach (string file in files)
                if (file.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries).Last().StartsWith("image"))
                {
                    fileType = file.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries).Last().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Last();
                    break;
                }

            var conv = preset
                .SetFrameRate(framerate)
                .SetOverwriteOutput(true)
                .AddParameter("-i \"" + inputFolder + @"\image%d." + fileType + "\"")
                .AddParameter("-start_number 0")
                .SetOutput(inputFolder + @"\" + outputFileName);

            if (multithread)
                conv = conv.UseMultiThread(Environment.ProcessorCount);

            var task = conv.Start();
            task.Wait();
        }

        /// <summary>
        /// Converts a sequence of images in a folder to a video.
        /// </summary>
        /// <param name="framerate">The output framerate of the video.</param>
        /// <param name="inputFolder">The folder to in which to look for the image sequence.</param>
        /// <param name="outputFileName">The name of the output file inluding the file extension.</param>
        /// <param name="multithread">Whether to multithread or not.</param>
        public static void ToVideo(int framerate, string inputFolder, string outputFileName, bool multithread = true)
        {
            string[] files = Directory.GetFiles(inputFolder + @"\");
            string fileType = "jpg";
            foreach (string file in files)
                if (file.Split(new string[] {@"\"}, StringSplitOptions.RemoveEmptyEntries).Last().StartsWith("image")) {
                    fileType = file.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries).Last().Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries).Last();
                    break;
                }

            var conv = Conversion.New()
                .SetFrameRate(framerate)
                .SetOverwriteOutput(true)
                .AddParameter("-i \"" + inputFolder + @"\image%d." + fileType + "\"")
                .AddParameter("-start_number 0")
                .SetOutput(inputFolder + @"\" + outputFileName);

            if (multithread)
                conv = conv.UseMultiThread(Environment.ProcessorCount);

            var task = conv.Start();
            task.Wait();
        }

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
