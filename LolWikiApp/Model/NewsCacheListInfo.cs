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
            MatchNewsCacheList = new List<NewsListInfo>();

            FileNameAndListDcit = new Dictionary<string, List<NewsListInfo>>
            {
                {"Latest.json", LatestNewsCacheList},
                {"MostCommented.json", MostCommentedNewsCacheList},
                {"Offical.json", OfficalNewsCacheList},
                {"OutsideServer.json", OutsideServerNewsCacheList},
                {"Match.json", MatchNewsCacheList},
                {"Guide.json", GuideNewsCacheList}
            };
        }


        public void Clear()
        {
            LatestNewsCacheList.Clear();
            MostCommentedNewsCacheList.Clear();
            OfficalNewsCacheList.Clear();
            OutsideServerNewsCacheList.Clear();
            MatchNewsCacheList.Clear();
            GuideNewsCacheList.Clear();
        }

        public List<NewsListInfo> GetListByNewsType(NewsType type)
        {
            List<NewsListInfo> result;
            switch (type)
            {
                case NewsType.Latest:
                    result = LatestNewsCacheList;
                    break;
                case NewsType.MostCommented:
                    result = MostCommentedNewsCacheList;
                    break;
                case NewsType.Offical:
                    result = OfficalNewsCacheList;
                    break;
                case NewsType.Match:
                    result = MatchNewsCacheList;
                    break;
                case NewsType.Guide:
                    result = GuideNewsCacheList;
                    break;
                default:
                       result = LatestNewsCacheList;
                    break;
            }

            return result;
        }

        public bool IsDataLoaded { get; set; }

        public Dictionary<string, List<NewsListInfo>> FileNameAndListDcit { get; private set; }

        public List<NewsListInfo> LatestNewsCacheList { get; private set; }
        public List<NewsListInfo> MostCommentedNewsCacheList { get; private set; }
        public List<NewsListInfo> OfficalNewsCacheList { get; private set; }
        public List<NewsListInfo> OutsideServerNewsCacheList { get; private set; }
        public List<NewsListInfo> MatchNewsCacheList { get; private set; }
        public List<NewsListInfo> GuideNewsCacheList { get; private set; }
    }
}
