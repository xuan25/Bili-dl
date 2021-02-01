using Bili;
using JsonUtil;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiliDownload
{
    /// <summary>
    /// Class <c>VideoInfo</c> models the info of a video/season on Bilibili.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public class VideoInfo
    {
        public uint Aid;
        public string Title;
        public List<Page> pages;

        public VideoInfo(Json.Value json, bool isSeason)
        {
            if (!isSeason)
            {
                Aid = json["aid"];
                Title = json["title"];
                string pic = json["pic"];
                pages = new List<Page>();
                foreach (Json.Value p in json["pages"])
                {
                    pages.Add(new Page(Title, Aid, p, isSeason, pic));
                }
            }
            else
            {
                Aid = json["season_id"];
                Title = json["title"];
                pages = new List<Page>();
                foreach (Json.Value p in json["episodes"])
                {
                    string cover = (string)p["cover"];
                    pages.Add(new Page(Title, Aid, p, isSeason, cover));
                }
            }

        }

        /// <summary>
        /// Get infos of a video/season.
        /// </summary>
        /// <param name="id">Aid/Season-id</param>
        /// <param name="isSeason">IsSeason</param>
        /// <returns>Video info</returns>
        public static VideoInfo GetInfo(uint id, bool isSeason)
        {
            if (!isSeason)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>
                {
                    { "aid", id.ToString() }
                };
                try
                {
                    Json.Value json = BiliApi.GetJsonResult("https://api.bilibili.com/x/web-interface/view", dic, false);
                    if (json["code"] == 0)
                        return new VideoInfo(json["data"], isSeason);
                    return null;
                }
                catch (System.Net.WebException)
                {
                    return null;
                }

            }
            else
            {
                Dictionary<string, string> dic = new Dictionary<string, string>
                {
                    { "season_id", id.ToString() }
                };
                try
                {
                    Json.Value json = BiliApi.GetJsonResult("https://bangumi.bilibili.com/view/web_api/season", dic, false);
                    if (json["code"] == 0)
                        return new VideoInfo(json["result"], isSeason);
                    return null;
                }
                catch (System.Net.WebException)
                {
                    return null;
                }

            }

        }

        /// <summary>
        /// Get infos of a video/season.
        /// </summary>
        /// <param name="bvid">Bvid</param>
        /// <returns>Video info</returns>
        public static VideoInfo GetInfoBv(string bvid)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>
                {
                    { "bvid", bvid }
                };
            try
            {
                Json.Value json = BiliApi.GetJsonResult("https://api.bilibili.com/x/web-interface/view", dic, false);
                if (json["code"] == 0)
                    return new VideoInfo(json["data"], false);
                return null;
            }
            catch (System.Net.WebException)
            {
                return null;
            }
        }

        /// <summary>
        /// Get infos of a video/season asynchronously.
        /// </summary>
        /// <param name="id">Aid/Season-id</param>
        /// <param name="isSeason">IsSeason</param>
        /// <returns>Video info</returns>
        public static Task<VideoInfo> GetInfoAsync(uint id, bool isSeason)
        {
            Task<VideoInfo> task = new Task<VideoInfo>(() =>
            {
                return GetInfo(id, isSeason);
            });
            task.Start();
            return task;
        }

        /// <summary>
        /// Get infos of a video/season asynchronously.
        /// </summary>
        /// <param name="bvid">Bvid</param>
        /// <returns>Video info</returns>
        public static Task<VideoInfo> GetInfoBvAsync(string bvid)
        {
            Task<VideoInfo> task = new Task<VideoInfo>(() =>
            {
                return GetInfoBv(bvid);
            });
            task.Start();
            return task;
        }

        /// <summary>
        /// Class <c>Page</c> models the info of a page/episode on Bilibili.
        /// Author: Xuan525
        /// Date: 24/04/2019
        /// </summary>
        public class Page
        {
            public bool IsSeason;
            public string Title;
            public string Index;
            public uint Aid;
            public uint Num;
            public uint Cid;
            public string Part;
            public uint Duration;
            public List<Quality> Qualities;
            public string Pic;

            public Page(string title, uint aid, Json.Value json, bool isSeason, string pic)
            {
                IsSeason = isSeason;
                if (!isSeason)
                {
                    Title = title;
                    Index = ((uint)json["page"]).ToString();
                    Aid = aid;
                    Num = json["page"];
                    Cid = json["cid"];
                    Part = json["part"];
                    Duration = json["duration"];
                    Pic = pic;
                }
                else
                {
                    Title = title;
                    Index = json["index"];
                    Aid = json["aid"];
                    Num = json["page"];
                    Cid = json["cid"];
                    Part = json["index_title"];
                    Duration = json["duration"];
                    Pic = pic;
                }
            }

            /// <summary>
            /// Get qualities of the page/episode.
            /// </summary>
            /// <returns>A list of qualities</returns>
            public List<Quality> GetQualities()
            {
                Dictionary<string, string> dic = new Dictionary<string, string>
                {
                    { "avid", Aid.ToString() },
                    { "cid", Cid.ToString() }
                };
                //dic.Add("fnval", "16");
                try
                {
                    Json.Value json = BiliApi.GetJsonResult("https://api.bilibili.com/x/player/playurl", dic, false);
                    Qualities = new List<Quality>();
                    if (json["code"] == 0)
                        for (int i = 0; i < json["data"]["accept_quality"].Count; i++)
                            Qualities.Add(new Quality(Title, Index, Num, Part, Aid, Cid, json["data"]["accept_quality"][i], json["data"]["accept_description"][i], false, Pic));
                    else if (IsSeason)
                    {
                        json = BiliApi.GetJsonResult("http://api.bilibili.com/pgc/player/web/playurl", dic, false);
                        if (json["code"] == 0)
                        {
                            for (int i = 0; i < json["result"]["accept_quality"].Count; i++)
                                Qualities.Add(new Quality(Title, Index, Num, Part, Aid, Cid, json["result"]["accept_quality"][i], json["result"]["accept_description"][i], true, Pic));
                        }
                    }
                    return Qualities;
                }
                catch (System.Net.WebException)
                {
                    return null;
                }

            }

            /// <summary>
            /// Get qualities of the page/episode asynchronously.
            /// </summary>
            /// <returns>A list of qualities</returns>
            public Task<List<Quality>> GetQualitiesAsync()
            {
                Task<List<Quality>> task = new Task<List<Quality>>(() =>
                {
                    return GetQualities();
                });
                task.Start();
                return task;
            }

            /// <summary>
            /// Class <c>Quality</c> models the info of a quality of a page/episode on Bilibili.
            /// Author: Xuan525
            /// Date: 24/04/2019
            /// </summary>
            public class Quality
            {
                public string Title;
                public string Index;
                public uint Num;
                public string Part;
                public uint Aid;
                public uint Cid;
                public uint Qn;
                public string Description;
                public bool IsAvaliable;
                public bool SeasonApiOnly;
                public string Pic;
                public bool MergeRequired;

                public Quality(string title, string index, uint num, string part, uint aid, uint cid, uint qn, string description, bool seasonApiOnly, string pic)
                {
                    SeasonApiOnly = seasonApiOnly;
                    Title = title;
                    Index = index;
                    Num = num;
                    Part = part;
                    Aid = aid;
                    Cid = cid;
                    Qn = qn;
                    Description = description;
                    Pic = pic;
                    MergeRequired = true;

                    Dictionary<string, string> dic = new Dictionary<string, string>
                    {
                        { "avid", Aid.ToString() },
                        { "cid", Cid.ToString() },
                        { "qn", Qn.ToString() }
                    };
                    //dic.Add("fnval", "16");
                    try
                    {
                        if (!seasonApiOnly)
                        {
                            Json.Value json = BiliApi.GetJsonResult("https://api.bilibili.com/x/player/playurl", dic, false);
                            IsAvaliable = json["data"]["quality"] == Qn;

                            if (IsAvaliable && !((string)json["data"]["format"]).Contains("flv"))
                            {
                                IsAvaliable = json["data"]["durl"].Count == 1;
                                MergeRequired = false;
                            }
                        }
                        else
                        {
                            Json.Value json = BiliApi.GetJsonResult("http://api.bilibili.com/pgc/player/web/playurl", dic, false);
                            IsAvaliable = json["result"]["format"] == Qn;
                        }

                    }
                    catch (System.Net.WebException)
                    {
                        IsAvaliable = false;
                    }

                }
            }
        }
    }
}
