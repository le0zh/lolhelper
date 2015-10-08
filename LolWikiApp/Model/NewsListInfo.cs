using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Interop;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json;

namespace LolWikiApp
{
    public class NewsListBaseInfo
    {
        public NewsTypeWrapper NewsType { get; set; }
    }

    public class NewsListInfo : NewsListBaseInfo
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "img")]
        public string Img { get; set; }

        [JsonProperty("thumb_img")]
        public string Thumb_img { get; set; }

        public string DescDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(Desc))
                    return "暂无简介";

                if (Desc.Length > 27)
                {
                    return Desc.Substring(0, 27) + "...";
                }

                return Desc;
            }
        }

        [JsonProperty("desc")]
        public string Desc { get; set; }

        [JsonProperty("posttime")]
        public string PostTime { get; set; }

        [JsonProperty("site")]
        public string Site { get; set; }

        [JsonProperty("topic_id")]
        public string Topic_id { get; set; }

        [JsonProperty("thumb_ok")]
        public string Thumb_ok { get; set; }

        public bool IsCached { get; set; }

        public bool IsFlipNews { get; set; }

        private List<NewsListInfo> _bannerListInfos;
        public List<NewsListInfo> BannerListInfos
        {
            get { return _bannerListInfos ?? (_bannerListInfos = new List<NewsListInfo>()); }
        }
    }

    public class NewsTypeWrapper
    {
        public NewsType Type { get; set; }

        public string DisplayName { get; set; }

        public string Source { get; set; }
    }

    public enum NewsType
    {
        Latest, //最新新闻
        MostCommented, //热评新闻 
        Offical, //官方新闻
        OutsideServer, //外服新闻
        Match, //赛事新闻
        Guide, //攻略

        Funny, //搞笑
        Story, //连载
        Beatury //美女
    }

    public class TcNewsListInfo : NewsListBaseInfo
    {
        public string article_id { get; set; }

        public string article_url { get; set; }

        public string channel_desc { get; set; }

        public string chanel_id { get; set; }

        public string image_url_big { get; set; }

        public string image_url_small { get; set; }

        public string insert_date { get; set; }

        public string publication_date { get; set; }

        public string score { get; set; }

        public string summary { get; set; }

        public string title { get; set; }

        public string SummaryDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(summary))
                    return "暂无简介";

                if (summary.Length > 27)
                {
                    return summary.Substring(0, 27) + "...";
                }

                return summary;
            }
        }
    }
}
