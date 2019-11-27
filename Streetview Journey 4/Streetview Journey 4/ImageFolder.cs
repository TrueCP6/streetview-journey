using System;
using System.IO;
using Xabe.FFmpeg;
using System.Linq;

namespace StreetviewJourney
{
    public class ImageFolder
    {
        public string Path;

        public ImageFolder() : this(System.IO.Path.GetTempPath() + @"Streetview Journey Temporary Image Folder\") { }

        public ImageFolder(string path)
        {
            Path = path;
            if (!Path.EndsWith(@"\"))
                Path += @"\";
            if (!Directory.Exists(Path.Remove(Path.Length - 1)))
                Directory.CreateDirectory(Path.Remove(Path.Length - 1));
        }

        public string[] Files
        {
            get => Directory.GetFiles(Path);
        }

        public string[] ImagePaths
        {
            get => Files.Where(str => str.Split(new string[] {@"\"}, StringSplitOptions.RemoveEmptyEntries).Last().StartsWith("image")).ToArray();
        }

        public void SaveVideo(IConversion preset, double framerate, string outputVideoPath, bool multithread = true)
        {
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

        public void SaveVideo(double framerate, bool multithread = true) =>
            SaveVideo(framerate, Path + "output.mp4", multithread);

        public void SaveVideo(double framerate, string outputVideoPath, bool multithread = true) =>
            SaveVideo(Conversion.New(), framerate, outputVideoPath, multithread);

        public void DeleteImages()
        {
            foreach (string path in ImagePaths)
                File.Delete(path);
        }

        public string OutputVideoPath;

        public void DeleteVideo() => File.Delete(OutputVideoPath);
    }
}
