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
        public static string apiKey = "";
        public static string signingKey = "";

        public static string GetGooglePanoID((double Lat, double Lon) point, int searchRadius = 50)
        {
            dynamic data;
            using (WebClient client = new WebClient())
                data = JsonConvert.DeserializeObject(client.DownloadString(Sign("https://maps.googleapis.com/maps/api/streetview/metadata?location=" + point.Lat + "," + point.Lon + "&key=" + apiKey + "&radius=" + searchRadius + "&source=outdoor", signingKey)));
            if (data.status == "OK")
                return data.pano_id;
            return null;
        }

        public static (double Lat, double Lon) GetExact(string panoID, int searchRadius = 50)
        {
            dynamic data;
            using (WebClient client = new WebClient())
                data = JsonConvert.DeserializeObject(client.DownloadString(Sign("https://maps.googleapis.com/maps/api/streetview/metadata?pano=" + panoID + "&key=" + apiKey + "&radius=" + searchRadius, signingKey)));
            if (data.status == "OK")
                return (Convert.ToDouble(data.location.lat), Convert.ToDouble(data.location.lng));
            return (0, 0);
        }

        public static bool AllUsable((double Lat, double Lon)[] locData, int searchRadius = 50)
        {
            foreach ((double Lat, double Lon) point in locData)
                if (IsUsable(point, searchRadius) == false)
                    return false;
            return true;
        }

        public static bool IsUsable((double Lat, double Lon) point, int searchRadius = 50)
        {
            string data;
            using (WebClient client = new WebClient())
                data = client.DownloadString(Sign("https://maps.googleapis.com/maps/api/streetview/metadata?location=" + point.Lat + "," + point.Lon + "&key=" + apiKey + "&radius=" + searchRadius, signingKey));
            return data.Contains("OK");
        }

        public static (double Lat, double Lon) GetExact((double Lat, double Lon) point, int searchRadius = 50)
        {
            dynamic data;
            using (WebClient client = new WebClient())
                data = JsonConvert.DeserializeObject(client.DownloadString(Sign("https://maps.googleapis.com/maps/api/streetview/metadata?location=" + point.Lat + "," + point.Lon + "&key=" + apiKey + "&radius=" + searchRadius, signingKey)));
            if (data.status == "OK")
                return (Convert.ToDouble(data.location.lat), Convert.ToDouble(data.location.lng));
            return (0, 0);
        }

        public static string GetPanoID((double Lat, double Lon) point, int searchRadius = 50)
        {
            dynamic data;
            using (WebClient client = new WebClient())
                data = JsonConvert.DeserializeObject(client.DownloadString(Sign("https://maps.googleapis.com/maps/api/streetview/metadata?location=" + point.Lat + "," + point.Lon + "&key=" + apiKey + "&radius=" + searchRadius, signingKey)));
            if (data.status == "OK")
                return data.pano_id;
            return null;
        }

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
