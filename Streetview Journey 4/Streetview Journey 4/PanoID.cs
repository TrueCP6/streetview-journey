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
        [JsonIgnore]
        public bool isThirdParty =>
            ID.Length == 64;

        public static bool operator ==(PanoID left, PanoID right) => left.ID == right.ID;
        public static bool operator !=(PanoID left, PanoID right) => left.ID != right.ID;

        /// <summary>
        /// The position of the panorama
        /// </summary>
        [JsonIgnore]
        public Point Position
        {
            get
            {
                dynamic data = JsonConvert.DeserializeObject(JsonMetadata);
                if (data.status == "OK")
                    return new Point(Convert.ToDouble(data.location.lat), Convert.ToDouble(data.location.lng));
                if (data.status == "ZERO_RESULTS")
                    throw new ZeroResultsException("Panorama could not be found.");
                throw new MetadataQueryException(Convert.ToString(data.status) + ", " + Convert.ToString(data.error_message));
            }
        }

        /// <summary>
        /// Whether the panorama is valid
        /// </summary>
        [JsonIgnore]
        public bool IsUsable
        {
            get
            {
                dynamic data = JsonConvert.DeserializeObject(JsonMetadata);
                if (data.status == "OK")
                    return true;
                if (data.status == "ZERO_RESULTS")
                    return false;
                throw new MetadataQueryException(Convert.ToString(data.status) + ", " + Convert.ToString(data.error_message));
            }
        }

        /// <summary>
        /// Gets the URL to a low resolution image of the URL
        /// </summary>
        /// <param name="res">The desired resolution of the image</param>
        /// <returns>The URL to the thumbnail image</returns>
        public string ThumbnailURL(Resolution res)
        {
            if (isThirdParty)
                throw new ThirdPartyPanoramaException("This method can only be used with first party panoramas.");
            return string.Format("http://maps.google.com/cbk?output=thumbnail&w={0}&h={1}&panoid={2}", res.Width, res.Height, ID);
        }

        /// <summary>
        /// Gets the URL to a specific tile for a panorama
        /// </summary>
        /// <param name="x">The x coordinate of the tile</param>
        /// <param name="y">The y coordinate of the tile</param>
        /// <param name="zoomLevel">The zoom level quality which determines the maximum amount of tiles</param>
        /// <returns>The URL to a specific tile of a panorama</returns>
        public string TileURL(int x, int y, int zoomLevel = 5)
        {
            if (isThirdParty)
                throw new ThirdPartyPanoramaException("This method can only be used with first party panoramas.");
            if (x < 0 || x > 25)
                throw new ArgumentOutOfRangeException("x", "Must be from 0 to 25");
            if (y < 0 || y > 12)
                throw new ArgumentOutOfRangeException("y", "Must be from 0 to 12");
            if (zoomLevel < 0 || zoomLevel > 5)
                throw new ArgumentOutOfRangeException("zoomLevel", "Must be from 0 to 5");
            return string.Format("http://maps.google.com/cbk?output=tile&panoid={0}&zoom={1}&x={2}&y={3}", ID, zoomLevel, x, y);
        }

        /// <summary>
        /// Gets an equirectangular bitmap image of the current panorama
        /// </summary>
        /// <returns>An equirectangular bitmap image of the panorama</returns>
        public Bitmap DownloadPanorama()
        {
            if (isThirdParty)
                throw new ThirdPartyPanoramaException("This method can only be used with first party panoramas.");

            Image[,] images = new Image[26, 13];
            Parallel.For(0, 26, x =>
            {
                Parallel.For(0, 13, y =>
                {
                    using (WebClient client = new WebClient())
                    using (MemoryStream stream = new MemoryStream(client.DownloadData(TileURL(x, y))))
                        images[x, y] = Image.FromStream(stream);
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
            if (res.Width == Resolution.DefaultPanoramaResolution.Width && res.Height == Resolution.DefaultPanoramaResolution.Height)
                return DownloadPanorama();

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
            bool success = false;
            PanoID id = new PanoID();
            while (!success)
            {
                Point pt = Point.Random();
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

        /// <summary>
        /// Gets the URL to the panorama's metadata
        /// </summary>
        [JsonIgnore]
        public string MetadataURL =>
            URL.Sign("https://maps.googleapis.com/maps/api/streetview/metadata?pano=" + ID);

        /// <summary>
        /// The json metadata of the panorama as a string
        /// </summary>
        [JsonIgnore]
        public string JsonMetadata
        {
            get
            {
                string data;
                using (WebClient client = new WebClient())
                    data = client.DownloadString(MetadataURL);
                return data;
            }
        }

        /// <summary>
        /// Gets the URL to the image of the PanoID
        /// </summary>
        /// <param name="bearing">The bearing of the image</param>
        /// <param name="pitch">The pitch of the image</param>
        /// <param name="res">The resolution of the image</param>
        /// <param name="fov">The field of view of the image</param>
        /// <returns>The Streetview Static API image URL</returns>
        public string ImageURL(Bearing bearing, double pitch, Resolution res, int fov) =>
            URL.Sign(string.Join("https://maps.googleapis.com/maps/api/streetview?size={0}x{1}&pano={2}&heading={3}&pitch={4}&fov={5}", res.Width, res.Height, ID, bearing, pitch, fov));
    }
}
