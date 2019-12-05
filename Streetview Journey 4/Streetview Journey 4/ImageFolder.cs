using System;
using System.IO;
using Xabe.FFmpeg;
using System.Linq;

namespace StreetviewJourney
{
    /// <summary>
    /// Used for the storing of images and converting image sequences into videos
    /// </summary>
    public class ImageFolder
    {
        /// <summary>
        /// The path to the ImageFolder
        /// </summary>
        public string Path;

        /// <summary>
        /// Creates a new ImageFolder in a temporary location
        /// </summary>
        public ImageFolder() : this(System.IO.Path.GetTempPath() + @"Streetview Journey Temporary Image Folder\") { }

        /// <summary>
        /// Creates a new ImageFolder from the path given
        /// </summary>
        /// <param name="path">The path to the directory</param>
        public ImageFolder(string path)
        {
            Path = path;
            if (!Path.EndsWith(@"\"))
                Path += @"\";
            if (!Directory.Exists(Path.Remove(Path.Length - 1)))
                Directory.CreateDirectory(Path.Remove(Path.Length - 1));
        }

        /// <summary>
        /// An array of paths for every file in the ImageFolder
        /// </summary>
        public string[] Files
        {
            get => Directory.GetFiles(Path);
        }

        /// <summary>
        /// The paths to all the images contained
        /// </summary>
        public string[] ImagePaths
        {
            get => Files.Where(str => System.IO.Path.GetFileName(str).StartsWith("image")).ToArray();
        }

        /// <summary>
        /// Saves a video from the images in the ImageFolder
        /// </summary>
        /// <param name="preset">The Xabe.FFmpeg preset to use (some values will be overwritten)</param>
        /// <param name="framerate">The desired framerate of the output video</param>
        /// <param name="outputVideoPath">The path including the file name and extension to the desired output video. e.g C:\dir\output.mp4</param>
        /// <param name="multithread">Whether to multithread the process</param>
        public void SaveVideo(IConversion preset, double framerate, string outputVideoPath, bool multithread = true)
        {
            if (!File.Exists(Setup.FFmpegExecutablesFolder + @"\ffmpeg.exe") && !File.Exists(Setup.FFmpegExecutablesFolder + @"\ffprobe.exe") && !File.Exists(Setup.FFmpegExecutablesFolder + @"\ffplay.exe"))
                Setup.DownloadFFmpeg();

            string fileType = ImagePaths[0].Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Last();

            var conv = preset
                .SetFrameRate(framerate)
                .SetOverwriteOutput(true)
                .SetOutput(outputVideoPath)
                .AddParameter("-i \"" + Path + @"image%d." + fileType + "\"")
                .AddParameter("-start_number 0")
                .UseMultiThread(multithread);

            conv.Start().Wait();

            OutputVideoPath = outputVideoPath;
        }

        /// <summary>
        /// Saves a video from the images in the ImageFolder to the default OutputVideoPath
        /// </summary>
        /// <param name="framerate">The desired framerate of the output video</param>
        /// <param name="multithread">Whether to multithread the process</param>
        public void SaveVideo(double framerate, bool multithread = true) =>
            SaveVideo(framerate, Path + "output.mp4", multithread);

        /// <summary>
        /// Saves a video from the images in the ImageFolder using the default preset
        /// </summary>
        /// <param name="framerate">The desired framerate of the output video</param>
        /// <param name="outputVideoPath">The path including the file name and extension to the desired output video. e.g C:\dir\output.mp4</param>
        /// <param name="multithread">Whether to multithread the process</param>
        public void SaveVideo(double framerate, string outputVideoPath, bool multithread = true) =>
            SaveVideo(Conversion.New(), framerate, outputVideoPath, multithread);

        /// <summary>
        /// Deletes all images from the ImageFolder
        /// </summary>
        public void DeleteImages()
        {
            foreach (string path in ImagePaths)
                File.Delete(path);
        }

        /// <summary>
        /// The path of the outputted video
        /// </summary>
        public string OutputVideoPath;

        /// <summary>
        /// Deletes the output video
        /// </summary>
        public void DeleteVideo() => File.Delete(OutputVideoPath);

        /// <summary>
        /// Deletes all files in the ImageFolder
        /// </summary>
        public void DeleteFiles()
        {
            foreach (string path in Files)
                File.Delete(path);
        }

        /// <summary>
        /// Deletes the ImageFolder
        /// </summary>
        public void Delete()
        {
            DeleteVideo();
            DeleteFiles();
            Directory.Delete(System.IO.Path.GetDirectoryName(Path + "virus.exe"));
        }
    }
}
