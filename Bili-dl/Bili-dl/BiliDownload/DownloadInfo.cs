using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BiliDownload
{
    [Serializable]
    public class DownloadInfo
    {
        public string Title;
        public string Index;
        public uint Aid;
        public uint Num;
        public uint Cid;
        public string Part;
        public uint Qn;
        public string Description;
        public int Threads;

        public DownloadInfo(VideoInfo.Page.Quality quality, int threads)
        {
            Title = Regex.Unescape(quality.Title);
            Index = Regex.Unescape(quality.Index);
            Aid = quality.Aid;
            Num = quality.Num;
            Cid = quality.Cid;
            Part = Regex.Unescape(quality.Part);
            Qn = quality.Qn;
            Description = Regex.Unescape(quality.Description);
            Threads = threads;
        }
    }
}
