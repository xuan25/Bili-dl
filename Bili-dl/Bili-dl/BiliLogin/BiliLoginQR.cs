using JsonUtil;
using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;

namespace BiliLogin
{
    /// <summary>
    /// Class <c>BiliLoginQR</c> used to request and listen a login QR code for bilibili.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    class BiliLoginQR
    {
        /// <summary>
        /// LoginUrlRecieved delegate.
        /// </summary>
        /// <param name="sender">Seader</param>
        /// <param name="url">login url</param>
        public delegate void LoginUrlRecievedDel(BiliLoginQR sender, string url);
        /// <summary>
        /// Occurs when a login url has been recieved.
        /// </summary>
        public event LoginUrlRecievedDel LoginUrlRecieved;

        /// <summary>
        /// QRImageLoaded delegate.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="qrImage">QR code bitmap</param>
        public delegate void QRImageLoadedDel(BiliLoginQR sender, Bitmap qrImage);
        /// <summary>
        /// Occurs when a login QR code has been generated.
        /// </summary>
        public event QRImageLoadedDel QRImageLoaded;

        /// <summary>
        /// LoggedIn delegate.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="cookies">Identity cookie</param>
        /// <param name="uid">Loged in uid</param>
        public delegate void LoggedInDel(BiliLoginQR sender, CookieCollection cookies, uint uid);
        /// <summary>
        /// Occurs when user logged in.
        /// </summary>
        public event LoggedInDel LoggedIn;

        /// <summary>
        /// Updated delegate.
        /// </summary>
        /// <param name="sender">Sender</param>
        public delegate void UpdatedDel(BiliLoginQR sender);
        /// <summary>
        /// Occurs when a status info has been recieved.
        /// </summary>
        public event UpdatedDel Updated;

        /// <summary>
        /// ConnectionFailed delegate.
        /// </summary>
        /// <param name="sender">Seander</param>
        /// <param name="ex">Exception</param>
        public delegate void ConnectionFailedDel(BiliLoginQR sender, WebException ex);
        /// <summary>
        /// Occurs when connection failed.
        /// </summary>
        public event ConnectionFailedDel ConnectionFailed;

        /// <summary>
        /// Timeout delegate.
        /// </summary>
        /// <param name="sender">Sender</param>
        public delegate void TimeoutDel(BiliLoginQR sender);
        /// <summary>
        /// Occurs when the QR code is timeout.
        /// </summary>
        public event TimeoutDel Timeout;

        private Thread loginListenerThread;
        private string oauthKey;
        private bool isTimeout;

        public BiliLoginQR(Window parent)
        {
            isTimeout = false;
            if (parent != null)
                parent.Closed += Parent_Closed;
        }

        private void Parent_Closed(object sender, EventArgs e)
        {
            Stop();
        }

        /// <summary>
        /// Begin the login listener.
        /// </summary>
        public void Begin()
        {
            Stop();
            loginListenerThread = new Thread(LoginListener);
            loginListenerThread.Start();
        }

        /// <summary>
        /// Stop the login listener.
        /// </summary>
        public void Stop()
        {
            if (loginListenerThread != null)
            {
                loginListenerThread.Abort();
                loginListenerThread.Join();
            }
        }

        /// <summary>
        /// Initialize a new login.
        /// </summary>
        /// <returns>Successful</returns>
        public bool Init()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://passport.bilibili.com/qrcode/getLoginUrl");
                string result;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using(Stream stream = response.GetResponseStream())
                        using(StreamReader reader = new StreamReader(stream))
                            result = reader.ReadToEnd();


                Json.Value getLoginUrl = Json.Parser.Parse(result);
                LoginUrlRecieved?.Invoke(this, getLoginUrl["data"]["url"]);
                Bitmap qrBitmap = RenderQrCode(getLoginUrl["data"]["url"]);
                QRImageLoaded?.Invoke(this, qrBitmap);
                oauthKey = getLoginUrl["data"]["oauthKey"];
                return true;
            }
            catch (WebException ex)
            {
                ConnectionFailed?.Invoke(this, ex);
                return false;
            }

        }

        private void LoginListener()
        {
            while (!Init())
            {
                Thread.Sleep(5000);
            }
            while (true)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://passport.bilibili.com/qrcode/getLoginInfo");
                    byte[] data = Encoding.UTF8.GetBytes("oauthKey=" + oauthKey);
                    request.Method = "POST";
                    request.ContentLength = data.Length;
                    request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    request.CookieContainer = new CookieContainer();
                    using(Stream postStream = request.GetRequestStream())
                        postStream.Write(data, 0, data.Length);
                    string result;
                    CookieCollection cookieCollection;
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        cookieCollection = response.Cookies;
                        using(Stream stream = response.GetResponseStream())
                            using (StreamReader reader = new StreamReader(stream))
                                result = reader.ReadToEnd();
                    }

                    Json.Value loginInfo = Json.Parser.Parse(result);
                    if (loginInfo["status"])
                    {
                        uint uid = 0;
                        foreach (Cookie cookie in cookieCollection)
                        {
                            if (cookie.Name == "DedeUserID")
                            {
                                uid = uint.Parse(cookie.Value);
                            }
                        }
                        LoggedIn?.Invoke(this, cookieCollection, uid);
                        break;
                    }
                    switch ((int)loginInfo["data"])
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
                    Updated?.Invoke(this);
                }
                catch (WebException ex)
                {
                    ConnectionFailed?.Invoke(this, ex);
                }
                Thread.Sleep(1000);
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
