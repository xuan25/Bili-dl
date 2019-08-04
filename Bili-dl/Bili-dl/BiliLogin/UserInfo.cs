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
            CurrentLevel = json["data"]["level_info"]["current_level"];
            CurrentMin = json["data"]["level_info"]["current_min"];
            CurrentExp = json["data"]["level_info"]["current_exp"];
            NextExp = json["data"]["level_info"]["next_exp"];
            BCoins = json["data"]["bCoins"];
            Coins = json["data"]["coins"];
            Face = json["data"]["face"];
            NameplateCurrent = json["data"]["nameplate_current"];
            PendantCurrent = json["data"]["pendant_current"];
            Uname = json["data"]["uname"];
            UserStatus = json["data"]["userStatus"];
            VipType = json["data"]["vipType"];
            VipStatus = json["data"]["vipStatus"];
            OfficialVerify = json["data"]["official_verify"];
            PointBalance = json["data"]["pointBalance"];
        }

        public static UserInfo GetUserInfo(CookieCollection cookies)
        {
            try
            {
                Json.Value json = BiliApi.GetJsonResult("https://account.bilibili.com/home/userInfo", null, false);
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
                Json.Value json = await BiliApi.GetJsonResultAsync("https://account.bilibili.com/home/userInfo", null, false);
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
