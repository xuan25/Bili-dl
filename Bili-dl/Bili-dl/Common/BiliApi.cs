using JsonUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bili
{
    /// <summary>
    /// Class <c>BiliApi</c> packaged a number of Apis for Bilibili.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    class BiliApi
    {
        // Api infos
        public const string APP_KEY = "4409e2ce8ffd12b8";
        public const string APP_SECRET = "59b43e04ad6965f34319062b478f83dd";
        public const string BUILD = "5520400";

        // Cookies for identification
        public static CookieCollection Cookies;

        private static string MD5Hash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private static List<KeyValuePair<string, string>> SignPayload(List<KeyValuePair<string, string>> payload)
        {
            List<KeyValuePair<string, string>> signedPayload = new List<KeyValuePair<string, string>>();
            signedPayload.Add(new KeyValuePair<string, string>("appkey", APP_KEY));
            signedPayload.Add(new KeyValuePair<string, string>("build", BUILD));
            signedPayload.AddRange(payload);
            signedPayload.Sort((a, b) => a.Key.CompareTo(b.Key));

            string query = BuildQuery(signedPayload, false);
            string hash = MD5Hash($"{query}{APP_SECRET}");

            signedPayload.Add(new KeyValuePair<string, string>("sign", hash));

            return signedPayload;
        }

        /// <summary>
        /// Build Query from a dictionary
        /// </summary>
        /// <param name="payload">Query payload</param>
        /// <param name="sign">Add sign into query</param>
        /// <returns>Query string</returns>
        private static string BuildQuery(Dictionary<string, string> payload, bool sign)
        {
            if (payload == null)
            {
                payload = new Dictionary<string, string>();
            }
            return BuildQuery(payload.ToList(), sign);
        }

        /// <summary>
        /// Build Query from a list of key-value pairs
        /// </summary>
        /// <param name="payload">Query payload</param>
        /// <param name="sign">Add sign into query</param>
        /// <returns>Query string</returns>
        private static string BuildQuery(List<KeyValuePair<string, string>> payload, bool sign)
        {
            if (payload == null)
            {
                payload = new List<KeyValuePair<string, string>>();
            }

            if (sign)
            {
                payload = SignPayload(payload);
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<string, string> item in payload)
            {
                stringBuilder.Append("&");
                stringBuilder.Append(item.Key);
                stringBuilder.Append("=");
                stringBuilder.Append(WebUtility.UrlEncode(item.Value));
            }
            if (stringBuilder.Length == 0)
            {
                return string.Empty;
            }
            return stringBuilder.ToString().Substring(1);
        }

        /// <summary>
        /// Get the result of the request in the form of a string.
        /// </summary>
        /// <param name="url">Request url</param>
        /// <param name="query">Parameter dictionary</param>
        /// <param name="sign">Add verification sign for parameters</param>
        /// <returns>Text result</returns>
        public static string RequestTextResult(string url, Dictionary<string, string> query, bool sign, string method = "GET")
        {
            string queryStr = BuildQuery(query, sign);

            HttpWebRequest request;
            switch (method)
            {
                case "GET":
                    request = (HttpWebRequest)WebRequest.Create($"{url}?{queryStr}");
                    break;
                case "POST":
                    request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    break;
                default:
                    throw new InvalidDataException();
            }

            request.Referer = System.Text.RegularExpressions.Regex.Match(url, "https?://[^/]+").Value;
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(Cookies);

            if (method == "POST")
            {
                byte[] payloadBuffer = Encoding.UTF8.GetBytes(queryStr);
                request.ContentLength = payloadBuffer.Length;
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(payloadBuffer, 0, payloadBuffer.Length);
                }
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        /// <summary>
        /// Get the result of the request in the form of a string asynchronously.
        /// </summary>
        /// <param name="url">Request url</param>
        /// <param name="query">Parameter dictionary</param>
        /// <param name="sign">Add verification sign for parameters</param>
        /// <returns>Text result</returns>
        public static Task<string> RequestTextResultAsync(string url, Dictionary<string, string> query, bool sign, string method = "GET")
        {
            Task<string> task = new Task<string>(() =>
            {
                return RequestTextResult(url, query, sign, method);
            });
            task.Start();
            return task;
        }

        /// <summary>
        /// Get the result of the request in the form of IJson.
        /// </summary>
        /// <param name="url">Request url</param>
        /// <param name="query">Parameter dictionary</param>
        /// <param name="sign">Add verification sign for parameters</param>
        /// <returns>IJson result</returns>
        public static Json.Value RequestJsonResult(string url, Dictionary<string, string> query, bool sign, string method = "GET")
        {
            string result = RequestTextResult(url, query, sign, method);
            Json.Value json = Json.Parser.Parse(result);
            return json;
        }

        /// <summary>
        /// Get the result of the request in the form of IJson asynchronously.
        /// </summary>
        /// <param name="url">Request url</param>
        /// <param name="query">Parameter dictionary</param>
        /// <param name="sign">Add verification sign for parameters</param>
        /// <returns>IJson result</returns>
        public static Task<Json.Value> RequestJsonResultAsync(string url, Dictionary<string, string> query, bool sign, string method = "GET")
        {
            Task<Json.Value> task = new Task<Json.Value>(() =>
            {
                return RequestJsonResult(url, query, sign, method);
            });
            task.Start();
            return task;
        }

        /// <summary>
        /// Formatting numbers.
        /// </summary>
        /// <param name="number">Original number</param>
        /// <param name="decimalPlaces">Reserved decimal places</param>
        /// <returns>Formatted numeric string</returns>
        public static string FormatNum(long number, int decimalPlaces)
        {
            if (number < 10000)
            {
                return number.ToString();
            }
            else if (number < 100000000)
            {
                if (decimalPlaces == -1)
                    return ((double)number / 10000) + "万";
                else if (decimalPlaces == 0)
                    return (number / 10000) + "万";
                else
                    return ((double)(number / (10000 / (long)Math.Pow(10, decimalPlaces)))) / (long)Math.Pow(10, decimalPlaces) + "万";
            }
            else
            {
                if (decimalPlaces == -1)
                    return ((double)number / 100000000) + "亿";
                else if (decimalPlaces == 0)
                    return (number / 100000000) + "亿";
                else
                    return (double)(number / (100000000 / (long)Math.Pow(10, decimalPlaces))) / (long)Math.Pow(10, decimalPlaces) + "亿";
            }
        }
    }
}
