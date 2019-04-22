using Bili;
using Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliDownload
{
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
                pages = new List<Page>();
                foreach (IJson p in json.GetValue("pages"))
                {
                    pages.Add(new Page(Title, Aid, p, isSeason));
                }
            }
            else
            {
                Aid = (uint)json.GetValue("season_id").ToLong();
                Title = json.GetValue("series_title").ToString();
                pages = new List<Page>();
                foreach (IJson p in json.GetValue("episodes"))
                {
                    pages.Add(new Page(Title, Aid, p, isSeason));
                }
            }
            
        }

        public static VideoInfo GetInfo(uint id, bool isSeason)
        {
            if (!isSeason)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("aid", id.ToString());
                try
                {
                    IJson json = BiliApi.GetJsonResult("https://api.bilibili.com/x/web-interface/view", dic, false);
                    return new VideoInfo(json.GetValue("data"), isSeason);
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
                    if(json.GetValue("code").ToLong() == 0)
                        return new VideoInfo(json.GetValue("result"), isSeason);
                    return null;
                }
                catch (System.Net.WebException)
                {
                    return null;
                }
                
            }
            
        }

        public static Task<VideoInfo> GetInfoAsync(uint id, bool isSeason)
        {
            Task<VideoInfo> task = new Task<VideoInfo>(() =>
            {
                return GetInfo(id, isSeason);
            });
            task.Start();
            return task;
        }

        public class Page
        {
            public string Title;
            public uint Aid;
            public uint Num;
            public uint Cid;
            public string Part;
            public uint Duration;
            public List<Quality> Qualities;

            public Page(string title, uint aid, IJson json, bool isSeason)
            {
                if (!isSeason)
                {
                    Title = title;
                    Aid = aid;
                    Num = (uint)json.GetValue("page").ToLong();
                    Cid = (uint)json.GetValue("cid").ToLong();
                    Part = json.GetValue("part").ToString();
                    Duration = (uint)json.GetValue("duration").ToLong();
                }
                else
                {
                    Title = title;
                    Aid = (uint)json.GetValue("aid").ToLong();
                    Num = (uint)json.GetValue("page").ToLong();
                    Cid = (uint)json.GetValue("cid").ToLong();
                    Part = json.GetValue("index_title").ToString();
                    Duration = (uint)json.GetValue("duration").ToLong();
                }
                
            }

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
                        {
                            Qualities.Add(new Quality(Title, Num, Part, Aid, Cid, (uint)json.GetValue("data").GetValue("accept_quality").GetValue(i).ToLong(), json.GetValue("data").GetValue("accept_description").GetValue(i).ToString()));
                        }
                    return Qualities;
                }
                catch (System.Net.WebException)
                {
                    return null;
                }
                
            }

            public Task<List<Quality>> GetQualitiesAsync()
            {
                Task<List<Quality>> task = new Task<List<Quality>>(() =>
                {
                    return GetQualities();
                });
                task.Start();
                return task;
            }

            public class Quality
            {
                public string Title;
                public uint Num;
                public string Part;
                public uint Aid;
                public uint Cid;
                public uint Qn;
                public string Description;
                public bool IsAvaliable;

                public Quality(string title, uint num, string part, uint aid, uint cid, uint qn, string description)
                {
                    Title = title;
                    Num = num;
                    Part = part;
                    Aid = aid;
                    Cid = cid;
                    Qn = qn;
                    Description = description;

                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("avid", Aid.ToString());
                    dic.Add("cid", Cid.ToString());
                    dic.Add("qn", Qn.ToString());
                    //dic.Add("fnval", "16");
                    try
                    {
                        IJson json = BiliApi.GetJsonResult("https://api.bilibili.com/x/player/playurl", dic, false);
                        IsAvaliable = json.GetValue("data").GetValue("quality").ToLong() == Qn;
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
