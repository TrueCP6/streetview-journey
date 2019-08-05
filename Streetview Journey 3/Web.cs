using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace Streetview_Journey_3
{
    class Web
    {
        /// <summary>
        /// Your Google Streetview Static API API key.
        /// </summary>
        public static string apiKey = "";
        /// <summary>
        /// Your Google Streetview Static API URL Signing Secret.
        /// </summary>
        public static string signingKey = "";

        /// <summary>
        /// Gets a panorama ID uploaded by Google and not a user from a point.
        /// </summary>
        /// <param name="point">A latitude-longitude point.</param>
        /// <param name="searchRadius">The search radius in meters for a panorama.</param>
        /// <returns>A 1st Party Panorama ID.</returns>
        public static string GetGooglePanoID((double Lat, double Lon) point, int searchRadius = 50)
        {
            dynamic data;
            using (WebClient client = new WebClient())
                data = JsonConvert.DeserializeObject(client.DownloadString(Sign("https://maps.googleapis.com/maps/api/streetview/metadata?location=" + point.Lat + "," + point.Lon + "&key=" + apiKey + "&radius=" + searchRadius + "&source=outdoor", signingKey)));
            if (data.status == "OK")
                return data.pano_id;
            return null;
        }

        /// <summary>
        /// Gets the position of the nearest panorama (1st or 3rd party).
        /// </summary>
        /// <param name="panoID">1st or 3rd party panorama ID.</param>
        /// <param name="searchRadius">The search radius in meters for a panorama.</param>
        /// <returns>A latitude-longitude point.</returns>
        public static (double Lat, double Lon) GetExact(string panoID, int searchRadius = 50)
        {
            dynamic data;
            using (WebClient client = new WebClient())
                data = JsonConvert.DeserializeObject(client.DownloadString(Sign("https://maps.googleapis.com/maps/api/streetview/metadata?pano=" + panoID + "&key=" + apiKey + "&radius=" + searchRadius, signingKey)));
            if (data.status == "OK")
                return (Convert.ToDouble(data.location.lat), Convert.ToDouble(data.location.lng));
            return (0, 0);
        }

        /// <summary>
        /// Whether every point in a location data array has a panorama (1st or 3rd party) within the search radius.
        /// </summary>
        /// <param name="locData">An array of latitude-longitude points.</param>
        /// <param name="searchRadius">The search radius in meters for a panorama.</param>
        /// <returns>A bool that is true if every point has a panorama within the search radius.</returns>
        public static bool AllUsable((double Lat, double Lon)[] locData, int searchRadius = 50)
        {
            foreach ((double Lat, double Lon) point in locData)
                if (IsUsable(point, searchRadius) == false)
                    return false;
            return true;
        }

        /// <summary>
        /// Whether a point has a panorama (1st or 3rd party) within the search radius.
        /// </summary>
        /// <param name="point">A latitude-longitude point.</param>
        /// <param name="searchRadius">The search radius in meters for a panorama.</param>
        /// <returns>A bool that is true if the point has a panorama within the search radius.</returns>
        public static bool IsUsable((double Lat, double Lon) point, int searchRadius = 50)
        {
            string data;
            using (WebClient client = new WebClient())
                data = client.DownloadString(Sign("https://maps.googleapis.com/maps/api/streetview/metadata?location=" + point.Lat + "," + point.Lon + "&key=" + apiKey + "&radius=" + searchRadius, signingKey));
            return data.Contains("OK");
        }

        /// <summary>
        /// Gets the position of the nearest panorama (1st or 3rd party).
        /// </summary>
        /// <param name="point">A latitude-longitude point.</param>
        /// <param name="searchRadius">The search radius in meters for a panorama.</param>
        /// <returns>A latitude-longitude point.</returns>
        public static (double Lat, double Lon) GetExact((double Lat, double Lon) point, int searchRadius = 50)
        {
            dynamic data;
            using (WebClient client = new WebClient())
                data = JsonConvert.DeserializeObject(client.DownloadString(Sign("https://maps.googleapis.com/maps/api/streetview/metadata?location=" + point.Lat + "," + point.Lon + "&key=" + apiKey + "&radius=" + searchRadius, signingKey)));
            if (data.status == "OK")
                return (Convert.ToDouble(data.location.lat), Convert.ToDouble(data.location.lng));
            return (0, 0);
        }

        /// <summary>
        /// Gets a 3rd or 1st party panorama ID from a point.
        /// </summary>
        /// <param name="point">A latitude-longitude point.</param>
        /// <param name="searchRadius">The search radius in meters for a panorama.</param>
        /// <returns>A 1st or 3rd Party Panorama ID.</returns>
        public static string GetPanoID((double Lat, double Lon) point, int searchRadius = 50)
        {
            dynamic data;
            using (WebClient client = new WebClient())
                data = JsonConvert.DeserializeObject(client.DownloadString(Sign("https://maps.googleapis.com/maps/api/streetview/metadata?location=" + point.Lat + "," + point.Lon + "&key=" + apiKey + "&radius=" + searchRadius, signingKey)));
            if (data.status == "OK")
                return data.pano_id;
            return null;
        }

        /// <summary>
        /// Encrypts (signs) a url with a signing secret for use with the Static Streetview API;
        /// </summary>
        /// <param name="url">The URL. Does not need <c>&signature=</c>.</param>
        /// <param name="keyString">Your URL signing secret.</param>
        /// <returns>The signed URL with the signature appended.</returns>
        public static string Sign(string url, string keyString) //code by google
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            string usablePrivateKey = keyString.Replace("-", "+").Replace("_", "/");
            byte[] privateKeyBytes = Convert.FromBase64String(usablePrivateKey);
            Uri uri = new Uri(url);
            byte[] encodedPathAndQueryBytes = encoding.GetBytes(uri.LocalPath + uri.Query);
            HMACSHA1 algorithm = new HMACSHA1(privateKeyBytes);
            byte[] hash = algorithm.ComputeHash(encodedPathAndQueryBytes);
            string signature = Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_");
            return uri.Scheme + "://" + uri.Host + uri.LocalPath + uri.Query + "&signature=" + signature;
        }
    }
}
