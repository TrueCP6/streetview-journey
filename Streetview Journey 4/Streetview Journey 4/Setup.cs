using System;
using System.Net;
using Xabe.FFmpeg;

namespace StreetviewJourney
{
    public class Setup
    {
        public static void Set(bool dontBillMe, string apiKey, string urlSigningSecret, string ffmpegExecutablesFolder, string geckodriverPath)
        {
            ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount * 12;
            DontBillMe = dontBillMe;
            APIKey = apiKey;
            URLSigningSecret = urlSigningSecret;
            FFmpegExecutablesFolder = ffmpegExecutablesFolder;
            GeckodriverPath = geckodriverPath;
        }

        public static bool DontBillMe { get; private set; }
        public static string APIKey { get; private set; }
        public static string URLSigningSecret { get; private set; }
        public static string FFmpegExecutablesFolder {
            get => FFmpeg.ExecutablesPath;
            private set => FFmpeg.ExecutablesPath = value;
        }
        public static string GeckodriverPath { get; private set; }
    }
}
