using Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Bili
{
    class BiliApi
    {
        public const string APP_KEY = "iVGUTjsxvpLeuDCf";
        public const string SECRET_KEY = "aHRmhWMLkdeMuILqORnYZocwMBpMEOdt";
        public const string BUILD = "8430";

        public static CookieCollection CookieCollection;

        public static string DicToParams(Dictionary<string, string> dic, bool addVerification)
        {
            if(dic != null)
            {
                if (addVerification)
                    dic = AddVerification(dic);
                StringBuilder stringBuilder = new StringBuilder();
                foreach (KeyValuePair<string, string> item in dic)
                {
                    stringBuilder.Append("&");
                    stringBuilder.Append(item.Key);
                    stringBuilder.Append("=");
                    stringBuilder.Append(item.Value.Replace(" ", "%20").Replace("&", "%26").Replace("=", "%3D"));
                }
                return stringBuilder.ToString().Substring(1);
            }
            return string.Empty;
            
        }

        public static Dictionary<string, string> AddVerification(Dictionary<string, string> dic)
        {
            dic.Add("appkey", APP_KEY);
            dic.Add("build", BUILD);
            string baseParams = DicToParams(dic.OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.Value), false);
            string sign = CreateMD5Hash(baseParams + SECRET_KEY);
            dic.Add("sign", sign);
            return dic;
        }

        private static string CreateMD5Hash(string input)
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

        public static string GetTextResult(string url, Dictionary<string, string> paramsDic, bool addVerification)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("{0}?{1}", url, DicToParams(paramsDic, addVerification)));
            if(CookieCollection != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(CookieCollection);
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string result = reader.ReadToEnd();
            reader.Close();
            response.Close();
            dataStream.Close();

            Console.WriteLine(result);
            return result;
        }

        public static Task<string> GetTextResultAsync(string url, Dictionary<string, string> paramsDic, bool addVerification)
        {
            Task<string> task = new Task<string>(() =>
            {
                return GetTextResult(url, paramsDic, addVerification);
            });
            task.Start();
            return task;
        }

        public static IJson GetJsonResult(string url, Dictionary<string, string> paramsDic, bool addVerification)
        {
            string result = GetTextResult(url, paramsDic, addVerification);
            IJson json = JsonParser.Parse(result);
            return json;
        }

        public static Task<IJson> GetJsonResultAsync(string url, Dictionary<string, string> paramsDic, bool addVerification)
        {
            Task<IJson> task = new Task<IJson>(() =>
            {
                return GetJsonResult(url, paramsDic, addVerification);
            });
            task.Start();
            return task;
        }

        public static Task<System.Drawing.Bitmap> GetImageAsync(string url)
        {
            Task<System.Drawing.Bitmap> task = new Task<System.Drawing.Bitmap>(() =>
            {
                return GetImage(url);
            });
            task.Start();
            return task;
        }

        public static System.Drawing.Bitmap GetImage(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(dataStream);
                response.Close();
                dataStream.Close();
                return bitmap;
            }
            catch (Exception)
            {
                return new System.Drawing.Bitmap(1,1);
            }
            
        }

        public static BitmapSource BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            IntPtr ip = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return bitmapSource;
        }

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
