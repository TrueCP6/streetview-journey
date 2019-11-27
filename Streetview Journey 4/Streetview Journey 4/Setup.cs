using System;
using System.IO;
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

        public static void Set(bool dontBillMe, string apiKey, string urlSigningSecret)
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

        public static string GeckodriverPath;
    }
}
