using System;
using System.Security.Cryptography;
using System.Text;

namespace StreetviewJourney
{
    public class URL
    {
        /// <summary>
        /// Signs a url with a URL signing secret and adds the API Key
        /// </summary>
        /// <param name="url">The url to sign without the API Key</param>
        /// <returns>The resultant URL</returns>
        public static string Sign(string url) //modified code by google
        {
            if (Setup.APIKey.Length != 39)
                throw new InvalidAuthenticationException("Invalid API Key.");
            if (Setup.URLSigningSecret.Length != 28 || !Setup.URLSigningSecret.EndsWith("="))
                throw new InvalidAuthenticationException("Invalid URL Signing Secret");

            url = url.Replace("&key=" + Setup.APIKey, "");
            url += "&key=" + Setup.APIKey;
            ASCIIEncoding encoding = new ASCIIEncoding();
            string usablePrivateKey = Setup.URLSigningSecret.Replace("-", "+").Replace("_", "/");
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
