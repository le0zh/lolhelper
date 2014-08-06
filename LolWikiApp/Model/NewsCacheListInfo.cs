using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LolWikiApp
{
    public class NewsCacheListInfo
    {
        // Latest, //最新新闻
        //MostCommented, //热评新闻 
        //Offical, //官方新闻
        //OutsideServer, //外服新闻
        //Match, //赛事新闻
        //Guide //攻略

        public NewsCacheListInfo()
        {
            LatestNewsCacheList = new List<NewsListInfo>();
            MostCommentedNewsCacheList = new List<NewsListInfo>();
            OfficalNewsCacheList = new List<NewsListInfo>();
            OutsideServerNewsCacheList = new List<NewsListInfo>();
            GuideNewsCacheList = new List<NewsListInfo>();

            FileNameAndListDcit = new Dictionary<string, List<NewsListInfo>>
            {
                {"Latest.json", LatestNewsCacheList},
                {"MostCommented.json", LatestNewsCacheList},
                {"Offical.json", LatestNewsCacheList},
                {"OutsideServer.json", LatestNewsCacheList},
                {"Match.json", LatestNewsCacheList},
                {"Guide.json", LatestNewsCacheList}
            };
        }

        public bool IsDataLoaded { get; set; }

        public Dictionary<string,List<NewsListInfo>> FileNameAndListDcit { get; private set; } 

        public List<NewsListInfo> LatestNewsCacheList { get; private set; }
        public List<NewsListInfo> MostCommentedNewsCacheList { get; private set; }
        public List<NewsListInfo> OfficalNewsCacheList { get; private set; }
        public List<NewsListInfo> OutsideServerNewsCacheList { get; private set; }
        public List<NewsListInfo> GuideNewsCacheList { get; private set; }
    }
}
