using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenQA.Selenium.Firefox;
using System.Drawing;
using OpenQA.Selenium;
using System.Net;
using System.Drawing.Imaging;

namespace Streetview_Journey_3
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount * 12;
            Web.apiKey = File.ReadAllLines(@"C:\Users\minec\Google Drive\Programming\C#\Streetview journey\keys.txt")[0];
            Web.signingKey = File.ReadAllLines(@"C:\Users\minec\Google Drive\Programming\C#\Streetview journey\keys.txt")[1];
            Export.ffmpegExecutablesPath = @"D:\Programs\ffmpeg\ffmpeg-20190601-4158865-win64-static\bin";

            var locData = Import.Auto(@"C:\Users\minec\Google Drive\Programming\C#\Streetview journey\gpxes\glacierviedma.svj");
            locData = Modify.Interpolate(locData, 5, 1000);
            Export.ToSVJ(locData, @"C:\Users\minec\Google Drive\Programming\C#\Streetview journey\gpxes\glacierviedma.svj");
            Console.WriteLine(locData.Length);

            Console.ReadLine();
        }
    }
}
