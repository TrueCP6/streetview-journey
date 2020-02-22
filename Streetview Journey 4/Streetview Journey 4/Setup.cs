using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using Xabe.FFmpeg;

namespace StreetviewJourney
{
    /// <summary>
    /// Various settings that allows this library to function
    /// </summary>
    public class Setup
    {
        static Setup()
        {
            ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount * 12; //stops timeout exceptions
            FFmpegExecutablesFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ffmpeg"); //workaround to initialize FFmpegExecutablesFolder
        }

        /// <summary>
        /// Sets all the required information needed to make queries to the Static Streetview API
        /// </summary>
        /// <param name="dontBillMe">If true all methods that get billed by Google will throw an exception</param>
        /// <param name="apiKey">Your Static Streetview API Key</param>
        /// <param name="urlSigningSecret">Your Static Streetview API URL Signing Secret</param>
        public static void SetStaticStreetviewAPIInfo(string apiKey, string urlSigningSecret, bool dontBillMe = true)
        {
            DontBillMe = dontBillMe;
            APIKey = apiKey;
            URLSigningSecret = urlSigningSecret;
        }

        /// <summary>
        /// If true all methods that get billed by Google will throw an exception
        /// </summary>
        public static bool DontBillMe = true;

        /// <summary>
        /// Your Static Streetview API Key
        /// </summary>
        public static string APIKey;

        /// <summary>
        /// Your Static Streetview API URL Signing Secret
        /// </summary>
        public static string URLSigningSecret;

        /// <summary>
        /// The directory containing your FFmpeg executables
        /// </summary>
        public static string FFmpegExecutablesFolder
        {
            get => FFmpeg.ExecutablesPath;
            set => FFmpeg.ExecutablesPath = value;
        }

        /// <summary>
        /// The path to geckodriver.exe
        /// </summary>
        public static string GeckodriverPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\geckodriver.exe";

        /// <summary>
        /// Downloads the latest version of FFmpeg to your FFmpegExecutablesFolder
        /// </summary>
        public static void DownloadFFmpeg()
        {
            FFmpeg.GetLatestVersion(true).Wait();
        }

        /// <summary>
        /// Downloads the latest version of geckodriver.exe to your GeckodriverPath
        /// </summary>
        public static void DownloadGeckodriver()
        {
            //find the latest version of geckodriver using Github API
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
            GeckodriverPath = Path.Combine(Path.GetDirectoryName(GeckodriverPath), "geckodriver.exe");
        }

        /// <summary>
        /// Whether all the FFmpeg executables exist in their set location
        /// </summary>
        public static bool FFmpegExists =>
            File.Exists(Path.Combine(FFmpegExecutablesFolder, "ffmpeg.exe")) && File.Exists(Path.Combine(FFmpegExecutablesFolder, "ffprobe.exe")) && File.Exists(Path.Combine(FFmpegExecutablesFolder, "ffplay.exe"));
    }
}
