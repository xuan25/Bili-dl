using Bili;
using JsonUtil;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BiliSearch
{
    /// <summary>
    /// SearchResultBox.xaml 的交互逻辑
    /// Author: Xuan525
    /// Date: 24/04/2019
    /// </summary>
    public partial class ResultBox : UserControl
    {
        /// <summary>
        /// Selected delegate.
        /// </summary>
        /// <param name="title">Title of the selected item</param>
        /// <param name="id">Aid/Season-id of the selected item</param>
        public delegate void SelectedDel(string title, object id, string type);
        /// <summary>
        /// Occurs when a Video has been selected.
        /// </summary>
        public event SelectedDel VideoSelected;
        /// <summary>
        /// Occurs when a Season has been selected.
        /// </summary>
        public event SelectedDel SeasonSelected;
        /// <summary>
        /// Occurs when a User has been selected.
        /// </summary>
        public event SelectedDel UserSelected;


        /// <summary>
        /// History Selected delegate.
        /// </summary>
        /// <param name="text">History</param>
        public delegate void HistorySelectedDel(string text);
        /// <summary>
        /// Occurs when a History has been selected.
        /// </summary>
        public event HistorySelectedDel HistorySelected;

        /// <summary>
        /// Class <c>Video</c> models the info of a Video.
        /// Author: Xuan525
        /// Date: 24/04/2019
        /// </summary>
        public class Video
        {
            public string Pic;
            public string Title;
            public long Play;
            public long Pubdate;
            public string Author;
            public long Aid;

            public Video(Json.Value json)
            {
                Pic = "https:" + json["pic"];
                Title = WebUtility.HtmlDecode(json["title"]);
                Play = json["play"];
                if (json.Contains("pubdate"))
                    Pubdate = json["pubdate"];
                else
                    Pubdate = json["created"];
                Author = Regex.Unescape(json["author"]);
                Aid = json["aid"];
            }
        }

        /// <summary>
        /// Class <c>Season</c> models the info of a Season.
        /// Author: Xuan525
        /// Date: 24/04/2019
        /// </summary>
        public class Season
        {
            public string Cover;
            public string Title;
            public string Styles;
            public string Areas;
            public long Pubtime;
            public string Cv;
            public string Description;
            public long SeasonId;
            public string SeasonTypeName;
            public string OrgTitle;

            public Season(Json.Value json, Json.Value cardsJson)
            {
                Cover = "https:" + Regex.Unescape(json["cover"]);
                Title = WebUtility.HtmlDecode(json["title"]);
                Styles = json["styles"];
                Areas = json["areas"];
                Pubtime = json["pubtime"];
                Cv = json["cv"];
                Description = json["desc"];
                SeasonId = json["season_id"];
                SeasonTypeName = cardsJson["result"][SeasonId.ToString()]["season_type_name"];
                OrgTitle = WebUtility.HtmlDecode(json["org_title"]);
            }
        }

        /// <summary>
        /// Class <c>User</c> models the info of a User.
        /// Author: Xuan525
        /// Date: 24/04/2019
        /// </summary>
        public class User
        {
            public long Mid;
            public string Upic;
            public string Uname;
            public long Videos;
            public long Fans;
            public string Usign;

            public User(Json.Value json)
            {
                Mid = json["mid"];
                Upic = "https:" + json["upic"];
                Uname = json["uname"];
                Videos = json["videos"];
                Fans = json["fans"];
                Usign = json["usign"];
            }
        }

        public ResultBox()
        {
            InitializeComponent();
        }

        public string SearchText;
        public string NavType;
        public RadioButton TypeBtn;

        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Search a text asynchronously.
        /// </summary>
        /// <param name="text">text</param>
        public void SearchAsync(string text, int pagenum)
        {
            SearchText = text;

            long aid = FindAid(text);
            if (aid >= 0)
            {
                HistoryBox.Insert(text);
                VideoSelected?.Invoke("Av" + aid.ToString(), aid, "aid");
                return;
            }

            string bvid = FindBvid(text);
            if (bvid != null)
            {
                HistoryBox.Insert(text);
                VideoSelected?.Invoke("Bv" + bvid, bvid, "bvid");
                return;
            }

            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
            ContentViewer.ScrollToHome();
            ContentPanel.Children.Clear();
            PagesBox.Visibility = Visibility.Hidden;
            if (text != null && text.Trim() != string.Empty)
            {
                HistoryBox.Insert(text);
                HistoryBox.Visibility = Visibility.Hidden;
                TypeBtn.IsChecked = true;

                cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = cancellationTokenSource.Token;

                LoadingPrompt.Visibility = Visibility.Visible;
                NoMoreGrid.Visibility = Visibility.Collapsed;
                ContentViewer.Visibility = Visibility.Collapsed;
                Task task = new Task(() =>
                {
                    string type = NavType;
                    Json.Value json = GetResult(text, type, pagenum);
                    if (json != null)
                        Dispatcher.Invoke(new Action(() =>
                        {
                            if (cancellationToken.IsCancellationRequested)
                                return;
                            ShowResult(json, type);
                            PagesBox.SetPage((int)json["data"]["numPages"], (int)json["data"]["page"], false);
                            PagesBox.Visibility = Visibility.Visible;
                            LoadingPrompt.Visibility = Visibility.Hidden;
                        }));
                });
                task.Start();
            }
            else
            {
                HistoryBox.Visibility = Visibility.Visible;
            }
        }

        private long FindAid(string text)
        {
            Match match = Regex.Match(text, "[Aa][Vv](?<Aid>[0-9]+)");
            if (match.Success)
                return long.Parse(match.Groups["Aid"].Value);
            return -1;
        }

        private string FindBvid(string text)
        {
            Match match = Regex.Match(text, "[Bb][Vv](?<Bvid>[0-9a-zA-Z]+)");
            if (match.Success)
                return match.Groups["Bvid"].Value;
            return null;
        }

        private Json.Value GetResult(string text, string type, int pagenum)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("jsonp", "jsonp");
            dic.Add("highlight", "1");
            dic.Add("search_type", type);
            dic.Add("keyword", text);
            dic.Add("page", pagenum.ToString());
            try
            {
                Json.Value json = BiliApi.GetJsonResult("https://api.bilibili.com/x/web-interface/search/type", dic, true);
                return json;
            }
            catch (System.Net.WebException)
            {
                return null;
            }

        }

        private async void ShowResult(Json.Value json, string type)
        {
            if (json["code"] == 0 && json["data"]["numResults"] > 0)
            {
                switch (type)
                {
                    case "video":
                        foreach (Json.Value v in json["data"]["result"])
                        {
                            Video video = new Video(v);
                            ResultVideo resultVideo = new ResultVideo(video);
                            resultVideo.PreviewMouseLeftButtonDown += ResultVideo_PreviewMouseLeftButtonDown;
                            ContentPanel.Children.Add(resultVideo);
                        }
                        break;
                    case "media_bangumi":
                    case "media_ft":
                        StringBuilder stringBuilderBangumi = new StringBuilder();
                        foreach (Json.Value v in json["data"]["result"])
                        {
                            stringBuilderBangumi.Append(',');
                            stringBuilderBangumi.Append(((uint)v["season_id"]).ToString());
                        }
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add("season_ids", stringBuilderBangumi.ToString().Substring(1));
                        try
                        {
                            Json.Value cardsJson = await BiliApi.GetJsonResultAsync("https://api.bilibili.com/pgc/web/season/cards", dic, true);
                            foreach (Json.Value v in json["data"]["result"])
                            {
                                Season season = new Season(v, cardsJson);
                                ResultSeason resultSeason = new ResultSeason(season);
                                resultSeason.PreviewMouseLeftButtonDown += ResultSeason_PreviewMouseLeftButtonDown;
                                ContentPanel.Children.Add(resultSeason);
                            }
                        }
                        catch (WebException)
                        {

                        }
                        break;
                    case "bili_user":
                        foreach (Json.Value v in json["data"]["result"])
                        {
                            User user = new User(v);
                            ResultUser resultUser = new ResultUser(user);
                            resultUser.PreviewMouseLeftButtonDown += ResultUser_PreviewMouseLeftButtonDown;
                            ContentPanel.Children.Add(resultUser);
                        }
                        break;
                }

                ContentViewer.Visibility = Visibility.Visible;
            }
            else
            {
                NoMoreGrid.Visibility = Visibility.Visible;
            }
        }

        private void ResultUser_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UserSelected?.Invoke(null, ((ResultUser)sender).Mid, "mid");
        }

        private void ResultSeason_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SeasonSelected?.Invoke(((ResultSeason)sender).Title, ((ResultSeason)sender).SeasonId, "sid");
        }

        private void ResultVideo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            VideoSelected?.Invoke(((ResultVideo)sender).Title, ((ResultVideo)sender).Aid, "aid");
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            TypeBtn = (RadioButton)sender;
            NavType = ((RadioButton)sender).Tag.ToString();
            if (SearchText != null && SearchText != "")
            {
                SearchAsync(SearchText, 1);
            }
        }

        public void SetHistory(List<string> history)
        {
            HistoryBox.SetHistory(history);
        }

        private void HistoryList_Search(string text)
        {
            HistorySelected?.Invoke(text);
            SearchAsync(text, 1);
        }

        private void PagesBox_JumpTo(int pagenum)
        {
            SearchAsync(SearchText, pagenum);
        }
    }
}
