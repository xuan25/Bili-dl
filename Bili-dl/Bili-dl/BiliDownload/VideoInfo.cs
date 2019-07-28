using Bili;
using Json;
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

        public VideoInfo(IJson json, bool isSeason)
        {
            if (!isSeason)
            {
                Aid = (uint)json.GetValue("aid").ToLong();
                Title = json.GetValue("title").ToString();
                string pic = json.GetValue("pic").ToString();
                pages = new List<Page>();
                foreach (IJson p in json.GetValue("pages"))
                {
                    pages.Add(new Page(Title, Aid, p, isSeason, pic));
                }
            }
            else
            {
                Aid = (uint)json.GetValue("season_id").ToLong();
                Title = json.GetValue("title").ToString();
                pages = new List<Page>();
                foreach (IJson p in json.GetValue("episodes"))
                {
                    string cover = p.GetValue("cover").ToString();
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
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("aid", id.ToString());
                try
                {
                    IJson json = BiliApi.GetJsonResult("https://api.bilibili.com/x/web-interface/view", dic, false);
                    if (json.GetValue("code").ToLong() == 0)
                        return new VideoInfo(json.GetValue("data"), isSeason);
                    return null;
                }
                catch (System.Net.WebException)
                {
                    return null;
                }

            }
            else
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("season_id", id.ToString());
                try
                {
                    IJson json = BiliApi.GetJsonResult("https://bangumi.bilibili.com/view/web_api/season", dic, false);
                    if (json.GetValue("code").ToLong() == 0)
                        return new VideoInfo(json.GetValue("result"), isSeason);
                    return null;
                }
                catch (System.Net.WebException)
                {
                    return null;
                }

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

            public Page(string title, uint aid, IJson json, bool isSeason, string pic)
            {
                IsSeason = isSeason;
                if (!isSeason)
                {
                    Title = title;
                    Index = json.GetValue("page").ToLong().ToString();
                    Aid = aid;
                    Num = (uint)json.GetValue("page").ToLong();
                    Cid = (uint)json.GetValue("cid").ToLong();
                    Part = json.GetValue("part").ToString();
                    Duration = (uint)json.GetValue("duration").ToLong();
                    Pic = pic;
                }
                else
                {
                    Title = title;
                    Index = json.GetValue("index").ToString();
                    Aid = (uint)json.GetValue("aid").ToLong();
                    Num = (uint)json.GetValue("page").ToLong();
                    Cid = (uint)json.GetValue("cid").ToLong();
                    Part = json.GetValue("index_title").ToString();
                    Duration = (uint)json.GetValue("duration").ToLong();
                    Pic = pic;
                }
            }

            /// <summary>
            /// Get qualities of the page/episode.
            /// </summary>
            /// <returns>A list of qualities</returns>
            public List<Quality> GetQualities()
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("avid", Aid.ToString());
                dic.Add("cid", Cid.ToString());
                //dic.Add("fnval", "16");
                try
                {
                    IJson json = BiliApi.GetJsonResult("https://api.bilibili.com/x/player/playurl", dic, false);
                    Qualities = new List<Quality>();
                    if (json.GetValue("code").ToLong() == 0)
                        for (int i = 0; i < ((JsonArray)json.GetValue("data").GetValue("accept_quality")).Count; i++)
                            Qualities.Add(new Quality(Title, Index, Num, Part, Aid, Cid, (uint)json.GetValue("data").GetValue("accept_quality").GetValue(i).ToLong(), json.GetValue("data").GetValue("accept_description").GetValue(i).ToString(), false, Pic));
                    else if (IsSeason)
                    {
                        json = BiliApi.GetJsonResult("http://api.bilibili.com/pgc/player/web/playurl", dic, false);
                        if (json.GetValue("code").ToLong() == 0)
                        {
                            for (int i = 0; i < ((JsonArray)json.GetValue("result").GetValue("accept_quality")).Count; i++)
                                Qualities.Add(new Quality(Title, Index, Num, Part, Aid, Cid, (uint)json.GetValue("result").GetValue("accept_quality").GetValue(i).ToLong(), json.GetValue("result").GetValue("accept_description").GetValue(i).ToString(), true, Pic));
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

                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("avid", Aid.ToString());
                    dic.Add("cid", Cid.ToString());
                    dic.Add("qn", Qn.ToString());
                    //dic.Add("fnval", "16");
                    try
                    {
                        if (!seasonApiOnly)
                        {
                            IJson json = BiliApi.GetJsonResult("https://api.bilibili.com/x/player/playurl", dic, false);
                            IsAvaliable = json.GetValue("data").GetValue("quality").ToLong() == Qn;
                        }
                        else
                        {
                            IJson json = BiliApi.GetJsonResult("http://api.bilibili.com/pgc/player/web/playurl", dic, false);
                            IsAvaliable = json.GetValue("result").GetValue("quality").ToLong() == Qn;
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
