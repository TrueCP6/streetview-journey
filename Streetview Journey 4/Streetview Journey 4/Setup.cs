using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using Xabe.FFmpeg;

namespace StreetviewJourney
{
    public class Setup
    {
        static Setup()
        {
            ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount * 12; //stops timeout crashes
            FFmpegExecutablesFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ffmpeg"; //workaround to initialize FFmpegExecutablesFolder
        }

        public static void SetStaticStreetviewAPIInfo(bool dontBillMe, string apiKey, string urlSigningSecret)
        {
            DontBillMe = dontBillMe;
            APIKey = apiKey;
            URLSigningSecret = urlSigningSecret;
        }

        public static bool DontBillMe;

        public static string APIKey;

        public static string URLSigningSecret;

        public static string FFmpegExecutablesFolder
        {
            get => FFmpeg.ExecutablesPath;
            set => FFmpeg.ExecutablesPath = value;
        }

        public static string GeckodriverPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\geckodriver.exe";

        public static void DownloadFFmpeg()
        {
            FFmpeg.GetLatestVersion(true).Wait();
        }

        public static void DownloadGeckodriver()
        {
            //find the latest version of geckodriver
            HttpWebRequest req = WebRequest.Create("https://api.github.com/repos/mozilla/geckodriver/releases") as HttpWebRequest;
            req.UserAgent = Environment.UserName;
            dynamic data;
            using (WebResponse resp = req.GetResponse())
            using (Stream stream = resp.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
                data = JsonConvert.DeserializeObject(reader.ReadToEnd());

            //get the url to the download
            string version = data[0].tag_name;
            string architecture = Environment.Is64BitOperatingSystem ? "64" : "32";
            string downloadURL = "https://github.com/mozilla/geckodriver/releases/download/" + version + "/geckodriver-" + version + "-win" + architecture + ".zip";

            //download and extract
            if (File.Exists(GeckodriverPath))
                File.Delete(GeckodriverPath);
            using (WebClient client = new WebClient())
            using (MemoryStream stream = new MemoryStream(client.DownloadData(downloadURL)))
            using (ZipArchive archive = new ZipArchive(stream))
                archive.ExtractToDirectory(Path.GetDirectoryName(GeckodriverPath));

            //update path
            GeckodriverPath = Path.GetDirectoryName(GeckodriverPath) + @"\geckodriver.exe";
        }
    }
}
