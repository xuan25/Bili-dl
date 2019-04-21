using Json;
using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BiliLogin
{
    class BiliLoginQR
    {
        public delegate void LoginUrlRecievedDel(BiliLoginQR sender, string url);
        public event LoginUrlRecievedDel LoginUrlRecieved;

        public delegate void QRImageLoadedDel(BiliLoginQR sender, Bitmap qrImage);
        public event QRImageLoadedDel QRImageLoaded;

        public delegate void LoggedInDel(BiliLoginQR sender, CookieCollection cookies, uint uid);
        public event LoggedInDel LoggedIn;

        public delegate void ConnectionFailedDel(BiliLoginQR sender, WebException ex);
        public event ConnectionFailedDel ConnectionFailed;

        public delegate void TimeoutDel(BiliLoginQR sender);
        public event TimeoutDel Timeout;

        private Thread loginListenerThread;
        private string oauthKey;
        private bool isTimeout;

        public BiliLoginQR(Window parent)
        {
            isTimeout = false;
            if(parent != null)
                parent.Closed += Parent_Closed;
        }

        private void Parent_Closed(object sender, EventArgs e)
        {
            Stop();
        }

        public void Begin()
        {
            Stop();
            loginListenerThread = new Thread(LoginListener);
            loginListenerThread.Start();
        }

        public void Stop()
        {
            if (loginListenerThread != null)
            {
                loginListenerThread.Abort();
                loginListenerThread.Join();
            }
        }

        public void Init()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://passport.bilibili.com/qrcode/getLoginUrl");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string result = reader.ReadToEnd();
            reader.Close();
            response.Close();
            dataStream.Close();

            IJson getLoginUrl = JsonParser.Parse(result);
            LoginUrlRecieved?.Invoke(this, getLoginUrl.GetValue("data").GetValue("url").ToString());
            Bitmap qrBitmap = RenderQrCode(getLoginUrl.GetValue("data").GetValue("url").ToString());
            QRImageLoaded?.Invoke(this, qrBitmap);
            oauthKey = getLoginUrl.GetValue("data").GetValue("oauthKey").ToString();
        }

        private void LoginListener()
        {
            try
            {
                Init();
                while (true)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://passport.bilibili.com/qrcode/getLoginInfo");
                    byte[] data = Encoding.UTF8.GetBytes("oauthKey=" + oauthKey);
                    request.Method = "POST";
                    request.ContentLength = data.Length;
                    request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    request.CookieContainer = new CookieContainer();
                    Stream postStream = request.GetRequestStream();
                    postStream.Write(data, 0, data.Length);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string result = reader.ReadToEnd();
                    CookieCollection cookieCollection = response.Cookies;
                    reader.Close();
                    response.Close();
                    dataStream.Close();
                    postStream.Close();

                    IJson loginInfo = JsonParser.Parse(result);
                    if (loginInfo.GetValue("status").ToBool())
                    {
                        uint uid = 0;
                        foreach(Cookie cookie in cookieCollection)
                        {
                            if(cookie.Name == "DedeUserID")
                            {
                                uid = uint.Parse(cookie.Value);
                            }
                        }
                        LoggedIn?.Invoke(this, cookieCollection, uid);
                        break;
                    }
                    switch ((int)loginInfo.GetValue("data").ToLong())
                    {
                        case -2:
                            if (!isTimeout)
                            {
                                isTimeout = true;
                                Timeout?.Invoke(this);
                                Stop();
                            }
                            break;
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (WebException ex)
            {
                ConnectionFailed?.Invoke(this, ex);
            }

        }

        private Bitmap RenderQrCode(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, false);
            return qrCodeImage;
        }
    }
}
