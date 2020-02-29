using System;
using System.Drawing.Imaging;

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

        /// <summary>
        /// Gets the System.Drawing.Imaging equivalent ImageFormat
        /// </summary>
        /// <param name="format">The format to convert</param>
        /// <returns>The System.Drawing.Imaging equivalent ImageFormat</returns>
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
