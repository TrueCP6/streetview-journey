using System;
using System.Security.Cryptography;
using System.Text;

namespace StreetviewJourney
{
    public class URL
    {
        /// <summary>
        /// Signs a url with a URL signing secret
        /// </summary>
        /// <param name="url">The url containing the API Key to sign</param>
        /// <param name="urlSigningSecret">The URL Signing Secret to sign the URL with</param>
        /// <returns>The resultant URL</returns>
        public static string Sign(string url, string urlSigningSecret) //code by google
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            string usablePrivateKey = urlSigningSecret.Replace("-", "+").Replace("_", "/");
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
