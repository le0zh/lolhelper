using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LolWikiApp.Repository
{
    public class NewsRepository : Repository
    {
        private const string NewsContentRequestUrl = "http://lolbox.oss.aliyuncs.com/json/v3/news/content/{0}.json?r={1}   "; //{0}: artId, {1}: random
        private LocalFileRepository localFileRepository = new LocalFileRepository();
        /// <summary>
        /// 获取缓存中的咨询列表
        /// </summary>
        /// <returns></returns>
        //public async Task<List<NewsListInfo>> GetNewsCachedListAsync()
        //{
            
        //}

        public async Task<NewsDetail> GetNewsDetailAsync(string artId)
        {
            var r = new Random(DateTime.Now.Millisecond);
            var random = r.Next();
            var url = string.Format(NewsContentRequestUrl, artId, random);
            var json = await GetJsonStringViaHTTPAsync(url);

            var newsDetail = JsonConvert.DeserializeObject<NewsDetail>(json);

            return newsDetail;
        }

        /// <summary>
        /// 从本地文件夹中读取缓存的新闻列表信息
        /// </summary>
        /// <param name="cacheListInfo"></param>
        /// <returns></returns>
        public async Task<int> LoadNewsCachedListInfo(NewsCacheListInfo cacheListInfo)
        {
            var count = 0;
            foreach (var fileName in cacheListInfo.FileNameAndListDcit.Keys)
            {
                var content = await localFileRepository.GetNewsCacheListInfoStringAsync(fileName);
                if (!string.IsNullOrEmpty(content))
                {
                    var list = JsonConvert.DeserializeObject<List<NewsListInfo>>(content);
                    foreach (var listInfo in list)
                    {
                        cacheListInfo.FileNameAndListDcit[fileName].Add(listInfo);
                        count++;
                    }
                }
            }
            cacheListInfo.IsDataLoaded = true;
            return count;
        }

        public async Task<int> SaveNewsCacheList(NewsCacheListInfo cacheListInfo)
        {
            var count = 0;
            foreach (var fileName in cacheListInfo.FileNameAndListDcit.Keys)
            {
                var content = JsonConvert.SerializeObject(cacheListInfo.FileNameAndListDcit[fileName]);
                await localFileRepository.SaveNewsCacheListInfoStringAsync(fileName,content);
                count++;
            }
            cacheListInfo.IsDataLoaded = true;
            return count;
        }

        /// <summary>
        /// 根据类型和页数，获取新闻列表信息的请求地址
        /// </summary>
        /// <param name="type">新闻类型</param>
        /// <param name="page">页数</param>
        /// <returns></returns>
        private string getNewsListRequestUrl(NewsType type, int page)
        {
            const string latestNewsListRequestUrl = "http://lolbox.oss.aliyuncs.com/json/v3/news/newslist_99_{0}.json?r={1}";
            const string topCommentedNewsListRequestUrl = "http://lolbox.oss.aliyuncs.com/json/v3/news/newslist_88_{0}.json?r={1}";
            const string officalNewsListRequestUrl = "http://lolbox.oss.aliyuncs.com/json/v3/news/newslist_1_{0}.json?r={1}";
            const string outSideSeverNewsListRequestUrl = "http://lolbox.oss.aliyuncs.com/json/v3/news/newslist_3_{0}.json?r={1}";
            const string matchNewsListRequestUrl = "http://lolbox.oss.aliyuncs.com/json/v3/news/newslist_2_{0}.json?r={1}";
            const string guideNewsListRequestUrl = "http://lolbox.oss.aliyuncs.com/json/v3/news/newslist_4_{0}.json?r={1}";

            string requestedUrl;
            var r = new Random(DateTime.Now.Millisecond);
            var random = r.Next();

            switch (type)
            {
                case NewsType.Latest:
                    requestedUrl = string.Format(latestNewsListRequestUrl, page, random);
                    break;
                case NewsType.Offical:
                    requestedUrl = string.Format(officalNewsListRequestUrl, page, random);
                    break;
                case NewsType.MostCommented: requestedUrl = string.Format(topCommentedNewsListRequestUrl, page, random);
                    break;
                case NewsType.OutsideServer: requestedUrl = string.Format(outSideSeverNewsListRequestUrl, page, random);
                    break;
                case NewsType.Match: requestedUrl = string.Format(matchNewsListRequestUrl, page, random);
                    break;
                case NewsType.Guide: requestedUrl = string.Format(guideNewsListRequestUrl, page, random);
                    break;
                default:
                    requestedUrl = string.Format(latestNewsListRequestUrl, page, random);
                    break;
            }

            return requestedUrl;
        }


        public async Task<List<NewsListInfo>> GetPagedNewsList(NewsType type = NewsType.Latest, int page = 1)
        {
            string url = getNewsListRequestUrl(type, page);
            string json = await GetJsonStringViaHTTPAsync(url);

            var newsList = JsonConvert.DeserializeObject<List<NewsListInfo>>(json);

            return newsList;
        }

        public List<NewsTypeWrapper> GetNewsTypeList()
        {
            var t1 = new NewsTypeWrapper() {Type = NewsType.Latest, DisplayName = "最新资讯"};
            var t2 = new NewsTypeWrapper() { Type = NewsType.MostCommented, DisplayName = "热评资讯" };
            var t3 = new NewsTypeWrapper() { Type = NewsType.Offical, DisplayName = "官方资讯" };
            var t4 = new NewsTypeWrapper() { Type = NewsType.OutsideServer, DisplayName = "外服资讯" };
            var t5 = new NewsTypeWrapper() { Type = NewsType.Match, DisplayName = "赛事资讯" };
            var t6 = new NewsTypeWrapper() { Type = NewsType.Guide, DisplayName = "攻略资讯" };

            var newsTypeList = new List<NewsTypeWrapper>(){t1,t2,t3,t4,t5,t6};
            return newsTypeList;
        }


        public void DownloadNews(NewsType type, int page = 1)
        {
               
        }
    }
}
