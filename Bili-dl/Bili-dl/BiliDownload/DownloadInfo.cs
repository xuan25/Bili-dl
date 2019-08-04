using System;

namespace BiliDownload
{
    /// <summary>
    /// Class <c>DownloadInfo</c> models the info of a download task.
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
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
        public string Pic;
        public bool MergeRequired;

        public DownloadInfo(VideoInfo.Page.Quality quality, int threads)
        {
            Title = quality.Title;
            Index = quality.Index;
            Aid = quality.Aid;
            Num = quality.Num;
            Cid = quality.Cid;
            Part = quality.Part;
            Qn = quality.Qn;
            Description = quality.Description;
            Threads = threads;
            Pic = quality.Pic;
            MergeRequired = quality.MergeRequired;
        }
    }
}
