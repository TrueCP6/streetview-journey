using Newtonsoft.Json;
using System;
using System.Net;
using System.Drawing;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace StreetviewJourney
{
    /// <summary>
    /// Used for modifying and storing of panorama IDs
    /// </summary>
    public class PanoID
    {
        /// <summary>
        /// The ID of the panorama
        /// </summary>
        public string ID;

        /// <summary>
        /// Creates a new PanoID using a pano ID string
        /// </summary>
        /// <param name="id"></param>
        public PanoID(string id)
        {
            ID = id;
        }

        /// <summary>
        /// Creates a new PanoID with the ID of null
        /// </summary>
        public PanoID() { }

        /// <summary>
        /// Whether the panorama was uploaded by a third party user
        /// </summary>
        public bool isThirdParty { get =>
            ID.Length == 64;
        }

        public static bool operator ==(PanoID left, PanoID right) => left.ID == right.ID;
        public static bool operator !=(PanoID left, PanoID right) => left.ID != right.ID;

        /// <summary>
        /// Gets the position of the PanoID
        /// </summary>
        /// <param name="searchRadius">The radius in meters to search</param>
        /// <returns>The position of the PanoID</returns>
        public Point Exact(int searchRadius = 50)
        {
            dynamic data;
            using (WebClient client = new WebClient())
                data = JsonConvert.DeserializeObject(client.DownloadString(URL.Sign("https://maps.googleapis.com/maps/api/streetview/metadata?pano=" + ID + "&key=" + Setup.APIKey + "&radius=" + searchRadius, Setup.URLSigningSecret)));
            if (data.status == "OK")
                return new Point(Convert.ToDouble(data.location.lat), Convert.ToDouble(data.location.lng));
            if (data.status == "ZERO_RESULTS")
                throw new ZeroResultsException("No matching panoramas could be found within the search radius.");
            throw new MetadataQueryException(Convert.ToString(data.status) + ", " + Convert.ToString(data.error_message));
        }

        /// <summary>
        /// Whether the panorama is usable or not
        /// </summary>
        /// <param name="searchRadius">The radius in meters to search</param>
        /// <returns></returns>
        public bool IsUsable(int searchRadius = 50)
        {
            dynamic data;
            using (WebClient client = new WebClient())
                data = JsonConvert.DeserializeObject(client.DownloadString(URL.Sign("https://maps.googleapis.com/maps/api/streetview/metadata?pano=" + ID + "&key=" + Setup.APIKey + "&radius=" + searchRadius, Setup.URLSigningSecret)));
            return data.status == "OK";
        }

        /// <summary>
        /// Gets the URL to a low resolution image of the URL
        /// </summary>
        /// <param name="res">The desired resolution of the image</param>
        /// <returns>The URL to the thumbnail image</returns>
        public string ThumbnailURL(Resolution res) =>
            "http://maps.google.com/cbk?output=thumbnail&w=" + res.Width + "&h=" + res.Height + "&panoid=" + ID;

        /// <summary>
        /// Gets the URL to a specific tile for a panorama
        /// </summary>
        /// <param name="x">The x coordinate of the tile</param>
        /// <param name="y">The y coordinate of the tile</param>
        /// <param name="zoomLevel">The zoom level quality which determines the maximum amount of tiles</param>
        /// <returns>The URL to a specific tile of a panorama</returns>
        public string TileURL(int x, int y, int zoomLevel = 5) =>
            "http://maps.google.com/cbk?output=tile&panoid=" + ID + "&zoom=" + zoomLevel + "&x=" + x + "&y=" + y;

        /// <summary>
        /// Gets an equirectangular bitmap image of the current panorama
        /// </summary>
        /// <returns>An equirectangular bitmap image of the panorama</returns>
        public Bitmap DownloadPanorama()
        {
            if (isThirdParty)
                throw new ThirdPartyPanoramaException("DownloadPanorama can only be used with first party panoramas.");

            Image[,] images = new Image[26, 13];
            Parallel.For(0, 26, x =>
            {
                Parallel.For(0, 13, y =>
                {
                    using (WebClient client = new WebClient())
                        images[x, y] = Image.FromStream(new MemoryStream(client.DownloadData(TileURL(x, y))));
                });
            });

            Bitmap result = new Bitmap(Resolution.DefaultPanoramaResolution.Width, Resolution.DefaultPanoramaResolution.Height);
            for (int x = 0; x < 26; x++)
            {
                for (int y = 0; y < 13; y++)
                {
                    using (Graphics g = Graphics.FromImage(result))
                        g.DrawImage(images[x, y], x * 512, y * 512);
                    images[x, y].Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets an equirectangular bitmap image of the current panorama at a specific resolution
        /// </summary>
        /// <param name="res">The desired output resolution</param>
        /// <returns>An equirectangular bitmap image of the panorama</returns>
        public Bitmap DownloadPanorama(Resolution res)
        {
            var destRect = new Rectangle(0, 0, res.Width, res.Height);
            var destImage = new Bitmap(res.Width, res.Height);

            using (var image = DownloadPanorama())
            {
                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }
            }

            return destImage;
        }

        /// <summary>
        /// Gets a random usable panorama from a random spot on Earth
        /// </summary>
        /// <param name="firstParty">Whether the panorama must be first party</param>
        /// <returns>A random PanoID</returns>
        public static PanoID RandomUsable(bool firstParty = false)
        {
            Random rng = new Random();
            bool success = false;
            PanoID id = new PanoID();
            while (!success)
            {
                Point pt = new Point(0, rng.NextDouble() * 360 - 180).Destination(rng.NextDouble() * 10018750, new Bearing((rng.NextDouble() <= 0.5) ? 0 : 180));
                try
                {
                    id = pt.PanoID(firstParty, 500000);
                    success = true;
                }
                catch (ZeroResultsException) { }
            }

            return id;
        }

        /// <summary>
        /// The ID of the panorama
        /// </summary>
        public override string ToString() => ID;

        public static implicit operator string(PanoID id) => id.ToString();
    }
}
