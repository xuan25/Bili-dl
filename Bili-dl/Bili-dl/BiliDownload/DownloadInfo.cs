using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliDownload
{
    [Serializable]
    public class DownloadInfo
    {
        public string Title;
        public uint Aid;
        public uint Num;
        public uint Cid;
        public string Part;
        public uint Qn;
        public string Description;
        public int Threads;

        public DownloadInfo(VideoInfo.Page.Quality quality, int threads)
        {
            Title = quality.Title;
            Aid = quality.Aid;
            Num = quality.Num;
            Cid = quality.Cid;
            Part = quality.Part;
            Qn = quality.Qn;
            Description = quality.Description;
            Threads = threads;
        }
    }
}
