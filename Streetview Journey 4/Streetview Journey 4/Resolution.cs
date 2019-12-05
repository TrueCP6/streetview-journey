using System;

namespace StreetviewJourney
{
    /// <summary>
    /// Used for determining the size of output images
    /// </summary>
    public class Resolution
    {
        /// <summary>
        /// The width in pixels
        /// </summary>
        public int Width;
        /// <summary>
        /// The height in pixels
        /// </summary>
        public int Height;

        /// <summary>
        /// Creates a new instance of the Resolution class
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        public Resolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Creates a default instance of the Resolution class with a resolution of 1920 by 1080
        /// </summary>
        public Resolution() : this(1920, 1080) { }

        /// <summary>
        /// The default resolution of an equirectangular panorama image
        /// </summary>
        public static Resolution DefaultPanoramaResolution { get; } = new Resolution(26 * 512, 13 * 512);
    }
}
