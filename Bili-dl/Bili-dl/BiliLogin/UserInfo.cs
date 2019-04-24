using Bili;
using Json;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BiliLogin
{
    /// <summary>
    /// Class <c>UserInfo</c> models a UserInfo.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    class UserInfo
    {
        public uint CurrentLevel;
        public int CurrentMin;
        public int CurrentExp;
        public int NextExp;
        public int BCoins;
        public double Coins;
        public string Face;
        public string NameplateCurrent;
        public string PendantCurrent;
        public string Uname;
        public string UserStatus;
        public uint VipType;
        public uint VipStatus;
        public int OfficialVerify;
        public uint PointBalance;

        public UserInfo(IJson json)
        {
            CurrentLevel = (uint)json.GetValue("data").GetValue("level_info").GetValue("current_level").ToLong();
            CurrentMin = (int)json.GetValue("data").GetValue("level_info").GetValue("current_min").ToLong();
            CurrentExp = (int)json.GetValue("data").GetValue("level_info").GetValue("current_exp").ToLong();
            NextExp = (int)json.GetValue("data").GetValue("level_info").GetValue("next_exp").ToLong();
            BCoins = (int)json.GetValue("data").GetValue("bCoins").ToLong();
            Coins = json.GetValue("data").GetValue("coins").ToDouble();
            Face = Regex.Unescape(json.GetValue("data").GetValue("face").ToString());
            NameplateCurrent = Regex.Unescape(json.GetValue("data").GetValue("nameplate_current").ToString());
            PendantCurrent = Regex.Unescape(json.GetValue("data").GetValue("pendant_current").ToString());
            Uname = Regex.Unescape(json.GetValue("data").GetValue("uname").ToString());
            UserStatus = Regex.Unescape(json.GetValue("data").GetValue("userStatus").ToString());
            VipType = (uint)json.GetValue("data").GetValue("vipType").ToLong();
            VipStatus = (uint)json.GetValue("data").GetValue("vipStatus").ToLong();
            OfficialVerify = (int)json.GetValue("data").GetValue("official_verify").ToLong();
            PointBalance = (uint)json.GetValue("data").GetValue("pointBalance").ToLong();
        }

        public static UserInfo GetUserInfo(CookieCollection cookies)
        {
            try
            {
                IJson json = BiliApi.GetJsonResult("https://account.bilibili.com/home/userInfo", null, false);
                if (json.GetValue("code").ToLong() == 0)
                    return new UserInfo(json);
                else
                    return null;
            }
            catch (WebException)
            {
                return null;
            }
            
        }

        public async static Task<UserInfo> GetUserInfoAsync(CookieCollection cookies)
        {
            try
            {
                IJson json = await BiliApi.GetJsonResultAsync("https://account.bilibili.com/home/userInfo", null, false);
                if (json.GetValue("code").ToLong() == 0)
                    return new UserInfo(json);
                else
                    return null;
            }
            catch (WebException)
            {
                return null;
            }
            
        }

        public Bitmap GetFaceBitmap()
        {
            return BiliApi.GetImage(Face);
        }

        public async Task<Bitmap> GetFaceBitmapAsync()
        {
            return await BiliApi.GetImageAsync(Face);
        }

        public Bitmap GetNamePlateBitmap()
        {
            return BiliApi.GetImage(NameplateCurrent);
        }

        public async Task<Bitmap> GetNamePlateBitmapAsync()
        {
            return await BiliApi.GetImageAsync(NameplateCurrent);
        }

        public Bitmap GetPendantBitmap()
        {
            return BiliApi.GetImage(PendantCurrent);
        }

        public async Task<Bitmap> GetPendantBitmapAsync()
        {
            return await BiliApi.GetImageAsync(PendantCurrent);
        }
    }
}
