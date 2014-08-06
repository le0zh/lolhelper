using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LolWikiApp.Repository
{
    public class ProgressChangedArgs : EventArgs
    {
        public int Value;
    }

    public class NewsRepository : Repository
    {
        public EventHandler<ProgressChangedArgs> NewsListCacheProgreessChangedEventHandler;
        public EventHandler<ProgressChangedArgs> NewsListCacheCompletedEventHandler;

        public EventHandler<ProgressChangedArgs> NewsContentCacheProgressChangedEventHandler;
        public EventHandler<ProgressChangedArgs> NewsContentCacheCompletedEventHandler;


        private const string NewsContentRequestUrl = "http://lolbox.oss.aliyuncs.com/json/v3/news/content/{0}.json?r={1}   "; //{0}: artId, {1}: random
        private readonly LocalFileRepository _localFileRepository = new LocalFileRepository();

        public string RenderNewsHtmlContent(NewsDetail detail)
        {
            #region HtmlTemplate
            const string htmlTemplate = @"
<html>
<head>
<meta charset='UTF-8'>
<title>$title$</title>
<meta http-equiv='X-UA-Compatible' content='IE=edge' />
<meta name='viewport' content='width=device-width, initial-scale=1.0, user-scalable=no, minimum-scale=1.0, maximum-scale=1.0'>
<style>
/* Reset */
html,body,div,span,object,iframe,h1,h2,h3,h4,h5,h6,p,blockquote,pre,a,abbr,acronym,address,code,del,dfn,em,img,q,dl,dt,dd,ol,ul,li,fieldset,form,label,legend,table,caption,tbody,tfoot,thead,tr,th,td{border:0;font-weight:inherit;font-style:inherit;font-size:100%;font-family:inherit;vertical-align:baseline;margin:0;padding:0;}
table{border-collapse:separate;border-spacing:0;margin-bottom:1.4em;}
caption,th,td{text-align:left;font-weight:400;}
blockquote:before,blockquote:after,q:before,q:after{content:'';}
blockquote,q{quotes:;}
a img{border:none;}

/* Layout */
@-webkit-viewport{width:device-width}
@-moz-viewport{width:device-width}
@-ms-viewport{width:device-width}
@-o-viewport{width:device-width}
@viewport{width:device-width}
img,video{ max-width: 100%; }
.container{width:90%;margin-left:auto;margin-right:auto;}
.container div{ font-size: 1.3em;}
body{
	font-size: 100%;
	line-height: 1.5;	
}

h1{
	font-size: 2.0em;
	text-align: left;
	
}

span.info{
    color: #555555;
}

p{
    font-size: 1.3em;    
	margin-bottom: 0.5em;
}

</style>

<script lang='javascript'> 

    var isNotify = false;
    function initialize() { 
        if(document.body.clientHeight){
            window.external.notify('clientHeight=' + document.body.clientHeight.toString());
            isNotify = true;
        }
        if(document.body.scrollHeight){
            window.external.notify('scrollHeight=' + document.body.scrollHeight.toString()); 
        }
      window.onscroll = onScroll; 
    }
     
    function onScroll(e) {
        if (isNotify == false) {
            window.external.notify('clientHeight=' + document.body.clientHeight.toString());
            isNotify = true;
        }
        var top = (document.documentElement && document.documentElement.scrollTop) ||  document.body.scrollTop;
        window.external.notify('scrollTop=' + top.toString()); 
    }

    window.onload = initialize;
</script>
</head>

<body>

 <div class='container'>
        <h1>$title$</h1>
        <span class='info'>发表时间：$postTime$   来源： $site$</span>     
        <hr />   
        $content$
 </div>
 
</body>
</html>";
            #endregion

            return htmlTemplate.Replace("$title$", detail.Title)
                .Replace("$postTime$", detail.Posttime)
                .Replace("$site$", detail.Site)
                .Replace("$content$", detail.Content.Replace("<div", "<p").Replace("</div", "</p"));
        }

        /// <summary>
        /// 根据新闻ID获取新闻内容
        /// </summary>
        /// <param name="artId"></param>
        /// <returns></returns>
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
                Debug.WriteLine("-----going to load cached file:" + fileName);
                var content = await _localFileRepository.GetNewsCacheListInfoStringAsync(fileName);
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

        /// <summary>
        /// 缓存列表，仅仅是列表，没有内容
        /// </summary>
        /// <param name="cacheListInfo"></param>
        /// <returns></returns>
        public async Task<int> SaveNewsCacheList(NewsCacheListInfo cacheListInfo)
        {
            var count = 0;
            foreach (var fileName in cacheListInfo.FileNameAndListDcit.Keys)
            {
                var content = JsonConvert.SerializeObject(cacheListInfo.FileNameAndListDcit[fileName]);
                await _localFileRepository.SaveNewsCacheListInfoStringAsync(fileName, content);
                count++;
            }
            cacheListInfo.IsDataLoaded = true;
            return count;
        }

        private int _newsToCacheCount;
        private int _nesCachedCount;

        /// <summary>
        /// 缓存新闻内容
        /// </summary>
        public async Task CacheNews()
        {
            var latestNewsList = await GetPagedNewsList(NewsType.Latest);
            _newsToCacheCount += latestNewsList.Count;
            NewsListCacheProgreessChanged();

            NewsListCacheCompleted();

            foreach (var newsListInfo in latestNewsList)
            {
                var isCached = await _localFileRepository.CheckNewsIsCachedOrNot(newsListInfo.Id);
                if (isCached)
                {
                    Debug.WriteLine(newsListInfo.Id + " is cached.");
                }
                else
                {
                    Debug.WriteLine("going to cache:" + newsListInfo.Id + ".html");
                    //TODO: EXCEPTION HANDER HERE
                    NewsDetail detail;
                    try
                    {
                        detail = await GetNewsDetailAsync(newsListInfo.Id);
                    }
                    catch (HttpRequestException)
                    {
                        Debug.WriteLine("4O4:" + newsListInfo.Id + ".html");
                    }
                   
                    var content

                    var path = await _localFileRepository.SaveNewsContentToCacheFolder(newsListInfo.Id, content);
                    _nesCachedCount++;
                    Debug.WriteLine("##Cached: " + path);
                    NewsContentCacheProgreessChanged();
                    App.NewsViewModel.NewsCacheListInfo.LatestNewsCacheList.Add(newsListInfo);
                }
            }

            NewsContentCacheCompleted();
        }

        #region Event hook
        private void NewsListCacheProgreessChanged()
        {
            if (NewsListCacheProgreessChangedEventHandler != null)
            {
                var progressChangedArgs = new ProgressChangedArgs() { Value = _newsToCacheCount };
                NewsListCacheProgreessChangedEventHandler(this, progressChangedArgs);
            }
        }

        private void NewsListCacheCompleted()
        {
            if (NewsListCacheCompletedEventHandler != null)
            {
                var progressChangedArgs = new ProgressChangedArgs() { Value = _newsToCacheCount };
                NewsListCacheCompletedEventHandler(this, progressChangedArgs);
            }
        }

        private void NewsContentCacheProgreessChanged()
        {
            if (NewsContentCacheProgressChangedEventHandler != null)
            {
                var progressChangedArgs = new ProgressChangedArgs() { Value = _nesCachedCount };
                NewsContentCacheProgressChangedEventHandler(this, progressChangedArgs);
            }
        }

        private void NewsContentCacheCompleted()
        {
            if (NewsContentCacheCompletedEventHandler != null)
            {
                var progressChangedArgs = new ProgressChangedArgs() { Value = _nesCachedCount };
                NewsContentCacheCompletedEventHandler(this, progressChangedArgs);
            }
        }
        #endregion

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
            var t1 = new NewsTypeWrapper() { Type = NewsType.Latest, DisplayName = "最新资讯" };
            var t2 = new NewsTypeWrapper() { Type = NewsType.MostCommented, DisplayName = "热评资讯" };
            var t3 = new NewsTypeWrapper() { Type = NewsType.Offical, DisplayName = "官方资讯" };
            var t4 = new NewsTypeWrapper() { Type = NewsType.OutsideServer, DisplayName = "外服资讯" };
            var t5 = new NewsTypeWrapper() { Type = NewsType.Match, DisplayName = "赛事资讯" };
            var t6 = new NewsTypeWrapper() { Type = NewsType.Guide, DisplayName = "攻略资讯" };

            var newsTypeList = new List<NewsTypeWrapper>() { t1, t2, t3, t4, t5, t6 };
            return newsTypeList;
        }

        public void DownloadNews(NewsType type, int page = 1)
        {

        }
    }
}
