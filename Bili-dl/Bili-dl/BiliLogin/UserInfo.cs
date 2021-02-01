using Bili;
using JsonUtil;
using System.Net;
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

        public UserInfo(Json.Value json)
        {
            Face = json["data"]["face"];
            Uname = json["data"]["uname"];
            VipType = json["data"]["vipType"];
            VipStatus = json["data"]["vipStatus"];
        }

        public static UserInfo GetUserInfo(CookieCollection cookies)
        {
            try
            {
                Json.Value json = BiliApi.GetJsonResult("http://api.bilibili.com/nav", null, false);
                if (json["code"] == 0)
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
                Json.Value json = await BiliApi.GetJsonResultAsync("http://api.bilibili.com/nav", null, false);
                if (json["code"] == 0)
                    return new UserInfo(json);
                else
                    return null;
            }
            catch (WebException)
            {
                return null;
            }

        }
    }
}
