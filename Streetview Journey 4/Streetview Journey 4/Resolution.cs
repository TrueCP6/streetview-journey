using System;

namespace StreetviewJourney
{
    public class Resolution
    {
        public int Width;
        public int Height;

        public Resolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Resolution() : this(1920, 1080) { }

        public static Resolution DefaultPanoramaResolution { get; } = new Resolution(26 * 512, 13 * 512);
    }
}
