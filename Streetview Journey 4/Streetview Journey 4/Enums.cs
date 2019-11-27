using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Text;

namespace StreetviewJourney
{
    public class Enums
    {
        public enum ImageFileFormat
        {
            Bmp = 4,
            Gif = 2,
            Jpeg = 1,
            Png = 0,
            Tiff = 3
        }

        public static ImageFormat GetFormat(ImageFileFormat format)
        {
            if (format == ImageFileFormat.Jpeg)
                return ImageFormat.Jpeg;
            else if (format == ImageFileFormat.Png)
                return ImageFormat.Png;
            else if (format == ImageFileFormat.Bmp)
                return ImageFormat.Bmp;
            else if (format == ImageFileFormat.Gif)
                return ImageFormat.Gif;
            else if (format == ImageFileFormat.Tiff)
                return ImageFormat.Tiff;
            else
                throw new BadImageFormatException();
        }
    }
}
