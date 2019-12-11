using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Net;

namespace StreetviewJourney
{
    /// <summary>
    /// Used for storing latitude and longitude values
    /// </summary>
    public class Point
    {
        /// <summary>
        /// Creates a new point at 0, 0
        /// </summary>
        public Point() : this(0, 0) { }

        /// <summary>
        /// Creates a new point from a latitude and longitude value
        /// </summary>
        /// <param name="lat">Latitude</param>
        /// <param name="lon">Longitude</param>
        public Point(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
            Bearing = new Bearing();
        }

        /// <summary>
        /// Creates a new point from a latitude, longitude and bearing
        /// </summary>
        /// <param name="lat">Latitude</param>
        /// <param name="lon">Longitude</param>
        /// <param name="bearing">Bearing</param>
        public Point(double lat, double lon, Bearing bearing)
        {
            Latitude = lat;
            Longitude = lon;
            Bearing = bearing;
        }

        /// <summary>
        /// The Latitude value of the point
        /// </summary>
        public double Latitude;
        /// <summary>
        /// The Longitude value of the point
        /// </summary>
        public double Longitude;
        /// <summary>
        /// The bearing of the point
        /// </summary>
        public Bearing Bearing;
        internal bool usable = true;

        /// <summary>
        /// The Earth's radius in metres
        /// </summary>
        private const double Radius = 6371000;

        /// <summary>
        /// Whether 2 points have the same position
        /// </summary>
        public static bool operator ==(Point left, Point right) =>
            (left.Latitude == right.Latitude) && (left.Longitude == right.Longitude);

        /// <summary>
        /// Whether 2 points do not have the same position
        /// </summary>
        public static bool operator !=(Point left, Point right) =>
            (left.Latitude != right.Latitude) || (left.Longitude != right.Longitude);

        /// <summary>
        /// Gets a point a fraction along a straight line drawn from the point to the end point
        /// </summary>
        /// <param name="endPoint">The end point where the fraction is 1</param>
        /// <param name="fraction">A value from 0 to 1</param>
        /// <returns>A new point</returns>
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

        /// <summary>
        /// Calculates the resultant point if given a starting point, distance and bearing 
        /// </summary>
        /// <param name="distance">The distance in metres</param>
        /// <param name="bearing">The direction</param>
        /// <returns>A new point</returns>
        public Point Destination(double distance, Bearing bearing)
        {
            double φ1 = Latitude * (Math.PI / 180);
            double λ1 = Longitude * (Math.PI / 180);
            double brng = bearing.Value * (Math.PI / 180);
            double φ2 = Math.Asin(Math.Sin(φ1) * Math.Cos(distance / Radius) + Math.Cos(φ1) * Math.Sin(distance / Radius) * Math.Cos(brng));
            double λ2 = λ1 + Math.Atan2(Math.Sin(brng) * Math.Sin(distance / Radius) * Math.Cos(φ1), Math.Cos(distance / Radius) - Math.Sin(φ1) * Math.Sin(φ2));
            return new Point(φ2 * (180 / Math.PI), λ2 * (180 / Math.PI));
        }

        /// <summary>
        /// Calculates the distance in metres between 2 points
        /// </summary>
        /// <param name="point">The end point</param>
        /// <returns>The distance in metres</returns>
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

        /// <summary>
        /// Gets the position of the nearest panorama within the search radius
        /// </summary>
        /// <param name="firstParty">Whether the point must be a first party panorama</param>
        /// <param name="searchRadius">The radius in metres to search for a panorama</param>
        /// <returns>A new Point</returns>
        public Point Exact(bool firstParty = false, int searchRadius = 50)
        {
            dynamic data = JsonConvert.DeserializeObject(JsonMetadata(firstParty, searchRadius));
            if (data.status == "OK")
                return new Point(Convert.ToDouble(data.location.lat), Convert.ToDouble(data.location.lng));
            if (data.status == "ZERO_RESULTS")
                throw new ZeroResultsException("No matching panoramas could be found within the search radius.");
            throw new MetadataQueryException(Convert.ToString(data.status) + ", " + Convert.ToString(data.error_message));
        }

        /// <summary>
        /// Returns the PanoID of the nearest panorama within the search radius
        /// </summary>
        /// <param name="firstParty">Whether the panorama must be a first party panorama</param>
        /// <param name="searchRadius">The radius in metres to search for a panorama</param>
        /// <returns>A new PanoID</returns>
        public PanoID PanoID(bool firstParty = false, int searchRadius = 50)
        {
            dynamic data = JsonConvert.DeserializeObject(JsonMetadata(firstParty, searchRadius));
            if (data.status == "OK")
                return new PanoID(Convert.ToString(data.pano_id));
            if (data.status == "ZERO_RESULTS")
                throw new ZeroResultsException("No matching panoramas could be found within the search radius.");
            throw new MetadataQueryException(Convert.ToString(data.status) + ", " + Convert.ToString(data.error_message));
        }

        /// <summary>
        /// Whether there is a panorama in the search radius
        /// </summary>
        /// <param name="firstParty">Whether the panoramas to search for must be first party</param>
        /// <param name="searchRadius">The radius in metres to search</param>
        public bool IsUsable(bool firstParty = false, int searchRadius = 50)
        {
            dynamic data = JsonConvert.DeserializeObject(JsonMetadata(firstParty, searchRadius));
            if (data.status == "OK")
                return true;
            if (data.status == "ZERO_RESULTS")
                return false;
            throw new MetadataQueryException(Convert.ToString(data.status) + ", " + Convert.ToString(data.error_message));
        }

        /// <summary>
        /// Gets the position of a random panorama
        /// </summary>
        /// <param name="firstParty">Whether the panorama must be first party</param>
        /// <returns>A new Point</returns>
        public static Point RandomUsable(bool firstParty = false)
        {
            bool success = false;
            Point pt = new Point();
            while (!success)
            {
                pt = Random();
                try
                {
                    pt = pt.Exact(firstParty, 500000);
                    success = true;
                }
                catch (ZeroResultsException) { }
            }

            return pt;
        }

        /// <summary>
        /// Gets a random point on Earth
        /// </summary>
        /// <returns>A new point</returns>
        public static Point Random()
        {
            Random rng = new Random();
            return new Point(0, rng.NextDouble() * 360 - 180).Destination(rng.NextDouble() * 10018750, new Bearing((rng.NextDouble() <= 0.5) ? 0 : 180));
        }

        /// <summary>
        /// Gets the URL to the point's Streetview page
        /// </summary>
        /// <param name="pitch">The starting pitch</param>
        /// <returns>The URL to the page</returns>
        public string StreetviewURL(double pitch) => 
            "http://maps.google.com/maps?q=&layer=c&cbll=" + Latitude + "," + Longitude + "&cbp=11," + Bearing + ",0,0," + pitch;

        /// <summary>
        /// The Streetview static API image URL
        /// </summary>
        /// <param name="pitch">The pitch of the image</param>
        /// <param name="res">The resolution of the image</param>
        /// <param name="fov">The field of view of the image</param>
        /// <returns>The Streetview static API image URL</returns>
        public string ImageURL(double pitch, Resolution res, int fov, bool firstParty = false, int radius = 50) =>
            URL.Sign("https://maps.googleapis.com/maps/api/streetview?size=" + res.Width + "x" + res.Height + "&location=" + Latitude + "," + Longitude + "&heading=" + Bearing + "&pitch=" + pitch + "&fov=" + fov + "&radius=" + radius + (firstParty ? "&source=outdoor" : ""));

        /// <summary>
        /// Calculates the direction from one point to another
        /// </summary>
        /// <param name="point2">The end point</param>
        /// <returns>A new bearing</returns>
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

        /// <summary>
        /// Writes out the point as "Latitude, Longitude"
        /// </summary>
        public override string ToString() => Latitude + ", " + Longitude;

        public static implicit operator string(Point pt) => pt.ToString();

        /// <summary>
        /// The midpoint between 2 points
        /// </summary>
        /// <param name="point2">The second point</param>
        /// <returns>A new point</returns>
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

        /// <summary>
        /// Whether the point is third party
        /// </summary>
        public bool isThirdParty =>
            PanoID(searchRadius: 1).isThirdParty;

        /// <summary>
        /// Gets the URL to the point's metadata
        /// </summary>
        /// <param name="firstParty">Whether to search for first party panoramas</param>
        /// <param name="searchRadius">The search radius in metres</param>
        /// <returns>The URL of the metadata</returns>
        public string MetadataURL(bool firstParty = false, int searchRadius = 50)
        {
            string url = "https://maps.googleapis.com/maps/api/streetview/metadata?location=" + Latitude + "," + Longitude + "&radius=" + searchRadius;
            if (firstParty)
                url += "&source=outdoor";
            return URL.Sign(url);
        }

        /// <summary>
        /// Gets the Json metadata of the point as a string
        /// </summary>
        /// <param name="firstParty">Whether the point must be first party</param>
        /// <param name="searchRadius">The radius in metres to search</param>
        /// <returns>The Json metadata as a string</returns>
        public string JsonMetadata(bool firstParty = false, int searchRadius = 50)
        {
            string data;
            using (WebClient client = new WebClient())
                data = client.DownloadString(MetadataURL(firstParty, searchRadius));
            return data;
        }
    }
}
