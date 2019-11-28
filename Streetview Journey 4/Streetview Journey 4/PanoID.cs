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
    public class PanoID
    {
        public string ID;
        public PanoID(string id)
        {
            ID = id;
        }

        public PanoID() { }

        public bool isThirdParty { get =>
            ID.Length == 64;
        }

        public static bool operator ==(PanoID left, PanoID right) => left.ID == right.ID;
        public static bool operator !=(PanoID left, PanoID right) => left.ID != right.ID;

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

        public bool IsUsable(int searchRadius = 50)
        {
            dynamic data;
            using (WebClient client = new WebClient())
                data = JsonConvert.DeserializeObject(client.DownloadString(URL.Sign("https://maps.googleapis.com/maps/api/streetview/metadata?pano=" + ID + "&key=" + Setup.APIKey + "&radius=" + searchRadius, Setup.URLSigningSecret)));
            return data.status == "OK";
        }

        public string ThumbnailURL(int width, int height) =>
            "http://maps.google.com/cbk?output=thumbnail&w=" + width + "&h=" + height + "&panoid=" + ID;

        public string TileURL(int x, int y, int zoomLevel = 5) =>
            "http://maps.google.com/cbk?output=tile&panoid=" + ID + "&zoom=" + zoomLevel + "&x=" + x + "&y=" + y;

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

            Bitmap result = new Bitmap(26 * 512, 13 * 512);
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

        public override string ToString() => ID;

        public static implicit operator string(PanoID id) => id.ToString();
    }
}
