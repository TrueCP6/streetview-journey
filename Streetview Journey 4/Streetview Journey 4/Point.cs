using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Net;

namespace StreetviewJourney
{
    public class Point
    {
        public Point() : this(0, 0) { }

        public Point(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
            Bearing = new Bearing();
        }

        public Point(double lat, double lon, Bearing bearing)
        {
            Latitude = lat;
            Longitude = lon;
            Bearing = bearing;
        }

        public double Latitude;
        public double Longitude;
        public Bearing Bearing;
        internal bool usable = true;

        private const double Radius = 6371000;

        public static bool operator ==(Point left, Point right)
        {
            return (left.Latitude == right.Latitude) && (left.Longitude == right.Longitude);
        }

        public static bool operator !=(Point left, Point right)
        {
            return (left.Latitude != right.Latitude) || (left.Longitude != right.Longitude);
        }

        public Point LinearInterpolate(Point endPoint, double fraction)
        {
            if (fraction < 0 || fraction > 1)
                throw new ArgumentOutOfRangeException("fraction", "Must be between 0 and 1");
            double angDist = DistanceTo(endPoint) / Radius;
            double lat1 = Latitude * (Math.PI / 180);
            double lon1 = Longitude * (Math.PI / 180);
            double lat2 = endPoint.Latitude * (Math.PI / 180);
            double lon2 = endPoint.Longitude * (Math.PI / 180);
            double a = Math.Sin((1 - fraction) * angDist) / Math.Sin(angDist);
            double b = Math.Sin(fraction * angDist) / Math.Sin(angDist);
            double x = a * Math.Cos(lat1) * Math.Cos(lon1) + b * Math.Cos(lat2) * Math.Cos(lon2);
            double y = a * Math.Cos(lat1) * Math.Sin(lon1) + b * Math.Cos(lat2) * Math.Sin(lon2);
            double z = a * Math.Sin(lat1) + b * Math.Sin(lat2);
            double lat3 = Math.Atan2(z, Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)));
            double lon3 = Math.Atan2(y, x);
            return new Point(lat3 * (180 / Math.PI), lon3 * (180 / Math.PI));
        }

        public Point Destination(double distance, Bearing bearing)
        {
            double φ1 = Latitude * (Math.PI / 180);
            double λ1 = Longitude * (Math.PI / 180);
            double brng = bearing.Value * (Math.PI / 180);
            double φ2 = Math.Asin(Math.Sin(φ1) * Math.Cos(distance / Radius) + Math.Cos(φ1) * Math.Sin(distance / Radius) * Math.Cos(brng));
            double λ2 = λ1 + Math.Atan2(Math.Sin(brng) * Math.Sin(distance / Radius) * Math.Cos(φ1), Math.Cos(distance / Radius) - Math.Sin(φ1) * Math.Sin(φ2));
            return new Point(φ2 * (180 / Math.PI), λ2 * (180 / Math.PI));
        }

        public double DistanceTo(Point point)
        {
            double φ1 = Latitude * (Math.PI / 180.0);
            double φ2 = point.Latitude * (Math.PI / 180.0);
            double Δφ = (point.Latitude - Latitude) * (Math.PI / 180.0);
            double Δλ = (point.Longitude - Longitude) * (Math.PI / 180.0);
            double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) + Math.Cos(φ1) * Math.Cos(φ2) * Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return Radius * c;
        }

        public Point Exact(int searchRadius = 50)
        {
            dynamic data;
            using (WebClient client = new WebClient())
                data = JsonConvert.DeserializeObject(client.DownloadString(URL.Sign("https://maps.googleapis.com/maps/api/streetview/metadata?location=" + Latitude + "," + Longitude + "&key=" + Setup.APIKey + "&radius=" + searchRadius, Setup.URLSigningSecret)));
            if (data.status == "OK")
                return new Point(Convert.ToDouble(data.location.lat), Convert.ToDouble(data.location.lng));
            if (data.status == "ZERO_RESULTS")
                throw new ZeroResultsException("No matching panoramas could be found within the search radius.");
            throw new MetadataQueryException(Convert.ToString(data.status) + ", " + Convert.ToString(data.error_message));
        }

        public PanoID PanoID(bool firstParty = false, int searchRadius = 50)
        {
            string url = "https://maps.googleapis.com/maps/api/streetview/metadata?location=" + Latitude + "," + Longitude + "&key=" + Setup.APIKey + "&radius=" + searchRadius;
            if (firstParty)
                url += "&source=outdoor";
            dynamic data;
            using (WebClient client = new WebClient())
                data = JsonConvert.DeserializeObject(client.DownloadString(URL.Sign(url, Setup.URLSigningSecret)));
            if (data.status == "OK")
                return new PanoID(Convert.ToString(data.pano_id));
            if (data.status == "ZERO_RESULTS")
                throw new ZeroResultsException("No matching panoramas could be found within the search radius.");
            throw new MetadataQueryException(Convert.ToString(data.status) + ", " + Convert.ToString(data.error_message));
        }

        public bool IsUsable(int searchRadius = 50)
        {
            dynamic data;
            using (WebClient client = new WebClient())
                data = JsonConvert.DeserializeObject(client.DownloadString(URL.Sign("https://maps.googleapis.com/maps/api/streetview/metadata?location=" + Latitude + "," + Longitude + "&key=" + Setup.APIKey + "&radius=" + searchRadius, Setup.URLSigningSecret)));
            return data.status == "OK";
        }

        public static Point RandomUsable()
        {
            bool success = false;
            Point pt = new Point();
            while (!success)
            {
                pt = Random();
                try
                {
                    pt = pt.Exact(500000);
                    success = true;
                }
                catch (ZeroResultsException) { }
            }

            return pt;
        }

        public static Point Random()
        {
            Random rng = new Random();
            return new Point(0, rng.NextDouble() * 360 - 180).Destination(rng.NextDouble() * 10018750, new Bearing((rng.NextDouble() <= 0.5) ? 0 : 180));
        }

        public string StreetviewURL(Bearing bearing, double pitch) => 
            "http://maps.google.com/maps?q=&layer=c&cbll=" + Latitude + "," + Longitude + "&cbp=11," + bearing.Value + ",0,0," + pitch;

        public string ImageURL(Bearing bearing, double pitch, Resolution res, int fov) =>
            URL.Sign("https://maps.googleapis.com/maps/api/streetview?size=" + res.Width + "x" + res.Height + "&location=" + Latitude + "," + Longitude + "&heading=" + bearing + "&pitch=" + pitch + "&fov=" + fov + "&key=" + Setup.APIKey, Setup.URLSigningSecret);

        public Bearing BearingTo(Point point2)
        {
            double lat1 = Latitude * (Math.PI / 180.0);
            double lon1 = Longitude * (Math.PI / 180.0);
            double lat2 = point2.Latitude * (Math.PI / 180.0);
            double lon2 = point2.Longitude * (Math.PI / 180.0);

            return new Bearing(
                Math.Atan2(Math.Sin(lon2 - lon1) * Math.Cos(lat2), Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1)) * (180 / Math.PI)
            );
        }

        public override string ToString() => Latitude + ", " + Longitude;

        public static implicit operator string(Point pt) => pt.ToString();

        public Point Midpoint(Point point2)
        {
            double lat1 = Latitude * (Math.PI / 180.0);
            double lon1 = Longitude * (Math.PI / 180.0);
            double lat2 = point2.Latitude * (Math.PI / 180.0);
            double lon2 = point2.Longitude * (Math.PI / 180.0);
            double Bx = Math.Cos(lat2) * Math.Cos(lon2 - lon1);
            double By = Math.Cos(lat2) * Math.Sin(lon2 - lon1);
            double lat3 = Math.Atan2(Math.Sin(lat1) + Math.Sin(lat2), Math.Sqrt((Math.Cos(lat1) + Bx) * (Math.Cos(lat1) + Bx) + By * By));
            double lon3 = lon1 + Math.Atan2(By, Math.Cos(lat1) + Bx);
            return new Point(lat3 * (180.0 / Math.PI), lon3 * (180.0 / Math.PI));
        }

        public bool isThirdParty
        {
            get => PanoID(searchRadius: 1).isThirdParty;
        }
    }
}
