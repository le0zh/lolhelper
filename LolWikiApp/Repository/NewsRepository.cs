using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using HtmlAgilityPack;
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
        public EventHandler<ProgressChangedArgs> ReadNewsListToCacheProgreessChangedEventHandler;
        public EventHandler<ProgressChangedArgs> ReadNewsListToCacheCompletedEventHandler;

        public EventHandler<ProgressChangedArgs> NewsContentCacheProgressChangedEventHandler;
        public EventHandler<ProgressChangedArgs> NewsContentCacheCompletedEventHandler;

        private const string NewsContentRequestUrl = "http://lolbox.oss.aliyuncs.com/json/v3/news/content/{0}.json?r={1}"; //{0}: artId, {1}: random
        private readonly LocalFileRepository _localFileRepository = new LocalFileRepository();
        
        #region Tecent News
        private const string TcNewsFunnyListtRequestUrl = "http://qt.qq.com/static/pages/news/phone/c18_list_{0}.shtml"; //{0}: page
        private const string TcNewsStoryListtRequestUrl = "http://qt.qq.com/static/pages/news/phone/c16_list_{0}.shtml"; //{0}: page
        private const string TcNewsMmListtRequestUrl = "http://qt.qq.com/static/pages/news/phone/c17_list_{0}.shtml"; //{0}: page

        public async Task<List<TcNewsListInfo>> GetTcPagedNewsList(NewsType type = NewsType.Funny, int page = 1)
        {
            var url = string.Empty;
            switch (type)
            {
                case NewsType.Funny:
                    url = string.Format(TcNewsFunnyListtRequestUrl, page);
                    break;
                case NewsType.Story:
                    url = string.Format(TcNewsStoryListtRequestUrl, page);
                    break;
                case NewsType.Beatury:
                    url = string.Format(TcNewsMmListtRequestUrl, page);
                    break;
            }

            var json = await GetJsonStringViaHttpAsync(url);
            var jObject = JObject.Parse(json);
            var newsList = JsonConvert.DeserializeObject<List<TcNewsListInfo>>(jObject["list"].ToString());

            return newsList.ToList();

        }
        #endregion

        public async Task<string> SaveHtmlToTempIsoFile(string html)
        {
            Debug.WriteLine(html);
            var path = await _localFileRepository.SaveStringToTempHtmlFile(html);
            return path;
        }

        public async Task<string> SaveHtmlToTempIsoFile(NewsDetail detail)
        {
            var html = RenderNewsHtmlContent(detail);
            Debug.WriteLine(html);
            var path = await _localFileRepository.SaveStringToTempHtmlFile(html);
            return path;
        }

        /// <summary>
        /// 拼写新闻内容的html格式
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public string RenderNewsHtmlContent(NewsDetail detail)
        {
            if (detail == null)
                return string.Empty;

            #region HtmlTemplate
            const string htmlTemplate = @"
<!Doctype html>
<html xmlns='http://www.w3.org/1999/xhtml'>
<meta charset='utf-8'>
<!-- Behavioral Meta Data -->
<meta name='apple-mobile-web-app-capable' content='yes'>
<meta name='viewport' content='width=device-width,initial-scale=1.0,maximum-scale=1.0,user-scalable=0' />
<!-- Styles -->

<style type='text/css'>
    html, body, div, span,applet, object, iframe, h1, h2, h3, h4, h5, h6, p, blockquote, pre, a, abbr, acronym, address, big, cite, code, del, dfn, em, img, ins, kbd, q, s, samp, small, strike, strong, sub, sup, tt, var, dd, dl, dt, li, ol, ul, fieldset, form, label, legend, table, caption, tbody, tfoot, thead, tr, th, td {margin:0;padding:0;}
    html,body{background:#fff;}
    body{font:normal normal normal 12px/1.5em Arial, Helvetica,STHeiti,Droid Sans Fallback;text-align: center;height:100%;word-break : break-all;}
    div {text-align:left;}
    h1,h2,h3,h4,h5,h6{font-size:100%;}
    a{text-decoration: none;color:#007bca;}
    a:hover{text-decoration: none;}
    a:active,a:focus{outline:none;}
    q:before, q:after {   content: '';   }    
    abbr, acronym {  border: 0; }
    ::-moz-selection {color:#FFFFFF;background-color:#209EEB;}
    li{list-style-type:none;}
    img{border: none;}
    fieldset{border-style: none }
    label,button{cursor: pointer;}
    select,input,textarea{font-size:12px;line-height:1.2em;}
    .clearfix:before,.clearfix:after{clear:both;content:'.';display:block;height:0;visibility:hidden;line-height:0;}
    .clearfix{*zoom:1;}
    .fix{word-break:keep-all;overflow:hidden;text-overflow:ellipsis;}
    .clear{font-size:0;line-height:0;height:0;clear:both;overflow: hidden;display: block;}
    .none{display:none;}

    html,body{background:#f2f2f2;}
    .article_box{color:#3f4140;}
    .article_box h1{padding:12px 12px 5px;font-size:18px;line-height:24px;background:#29282e;color:#fff}
    .article_author{padding:0px 12px;background:#29282e;color:#656565;}
    .article_box p{line-height:24px;}
    .article_meta{border-bottom:1px solid #b6b6b6;padding:0 12px 8px;height:18px;line-height:18px;color:#656565;background:#29282e;}
    .article_content{padding:5px 12px;border-top:1px solid #fff;font-size:16px;}
    .article_content img{margin:3px 0;max-width:100%;}
    .article_content iframe,
    .article_content embed{max-width:95%!important;margin:0 auto;}
    .video { position: relative; float:left ;}

    .video a {
        position: absolute;
        display: block;
        background: url(http://ossweb-img.qq.com/images/qqtalk/act/lol_app_bg/playIcon.png)  no-repeat center center;
        height: 60px;
        width: 60px;
        top: 44%;
        left: 44%;
    }
</style>
</head>

<body>
 <div class='article_box'>
	<h1>$title$</h1>
	<div class='article_author'>$site$</div>
	<div class='article_meta'>时间：$postTime$</div>
	<div class='article_content'>
		$content$

        <p><br/></p>
        <p style='text-align: right;'><span style='color: rgb(0, 176, 240); font-family: 微软雅黑, &#39;Microsoft YaHei&#39;; font-size: 14px;'>英雄联盟助手WP版反馈QQ群 49573963</span</span></p>
	</div>
</div>

</body>
</html>";
            #endregion

            var doc = new HtmlDocument();
            doc.LoadHtml("<div>" + detail.Content + "</div>");
            Debug.WriteLine("----Originial-----");
            Debug.WriteLine(detail.Content);

            //var pNodes = doc.DocumentNode.SelectNodes("div/p");

            //if (pNodes != null)
            //{
            //    foreach (var node in pNodes)
            //    {
            //        //var innerText = node.InnerText.Trim().ToLower();
            //        //if (innerText == "&nbsp;" || innerText == "")
            //        //{
            //        //    node.Remove();
            //        //    continue;
            //        //}
            //        var style = node.GetAttributeValue("style", "N/A");

            //        if (style != "N/A")
            //        {
            //            if (!style.ToLower().Contains("center") && !style.ToLower().Contains("text-indent"))
            //            {
            //                node.SetAttributeValue("style", "");
            //            }

            //            if (style.ToLower().Contains("center"))
            //            {
            //                node.SetAttributeValue("style", "text-align:center");
            //            }
            //            if (style.ToLower().Contains("text-indent"))
            //            {
            //                node.SetAttributeValue("style", "text-indent:2em");
            //            }
            //        }
            //    }
            //}

            detail.Content = doc.DocumentNode.OuterHtml;

            var html = htmlTemplate.Replace("$title$", detail.Title)
                .Replace("$postTime$", detail.Posttime)
                .Replace("$site$", detail.Site)
                .Replace("$content$", detail.Content);
            //.Replace("$content$", detail.Content.Replace("<div", "<p").Replace("</div", "</p"));

            return HelperRepository.Unicode2Html(html);
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
            var json = await GetJsonStringViaHttpAsync(url);

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

        private int _newsToCacheCount = 5;//初始化为5，因为是5种类型
        private int _newsCachedCount;

        private readonly Dictionary<NewsType, List<NewsListInfo>> _newsTypeAndListDict = new Dictionary<NewsType, List<NewsListInfo>>()
        {
            {NewsType.Latest, new List<NewsListInfo>()},
            {NewsType.MostCommented, new List<NewsListInfo>()},
            {NewsType.Offical, new List<NewsListInfo>()},
            {NewsType.OutsideServer, new List<NewsListInfo>()},
            {NewsType.Match, new List<NewsListInfo>()},
            {NewsType.Guide, new List<NewsListInfo>()}
        };

        /// <summary>
        /// 缓存新闻内容
        /// </summary>
        public async Task CacheNews()
        {
            _newsToCacheCount = 1;
            _newsCachedCount = 0;

            //Read news list to cache
            var listTmp = await GetPagedNewsList(NewsType.Latest);
            _newsToCacheCount += listTmp.Count;
            ReadNewsListToCacheProgreessChanged();

            ReadNewsListToCacheCompleted();

            await SaveNewsListContent(listTmp);

            NewsContentCacheCompleted();
        }

        /// <summary>
        /// 缓存所有类别的资讯内容//TODO:暂时不缓存所有的资讯内容
        /// </summary>
        /// <returns></returns>
        public async Task CacheAllTypesNews()
        {
            _newsToCacheCount = 6;
            _newsCachedCount = 0;

            //Read news list to cache
            foreach (var type in _newsTypeAndListDict.Keys)
            {
                var listTmp = await GetPagedNewsList(type);
                _newsTypeAndListDict[type].Clear();
                _newsTypeAndListDict[type].AddRange(listTmp);
                _newsToCacheCount += listTmp.Count;
                ReadNewsListToCacheProgreessChanged();
            }

            ReadNewsListToCacheCompleted();

            //Save news each by each
            foreach (var list in _newsTypeAndListDict.Values)
            {
                await SaveNewsListContent(list);
            }

            await SaveNewsListContent(_newsTypeAndListDict[NewsType.Latest]);
        }

        private async Task SaveNewsListContent(IEnumerable<NewsListInfo> listInfos)
        {
            foreach (var listInfo in listInfos)
            {
                var isCached = await _localFileRepository.CheckNewsIsCachedOrNot(listInfo.Id);
                if (isCached)
                {
                    Debug.WriteLine(listInfo.Id + " is cached.");
                }
                else
                {
                    Debug.WriteLine("going to cache:" + listInfo.Id + ".html//" + listInfo.Title);
                    //TODO: EXCEPTION HANDER HERE
                    var detail = await GetNewsDetailAsync(listInfo.Id);
                    var content = RenderNewsHtmlContent(detail);
                    var path = await _localFileRepository.SaveNewsContentToCacheFolder(listInfo.Id, content);
                    Debug.WriteLine("##Cached: " + path);
                    App.NewsViewModel.NewsCacheListInfo.LatestNewsCacheList.Add(listInfo);
                }

                _newsCachedCount++;
                NewsContentCacheProgreessChanged();
            }
        }

        //TODO:暂时仅缓存最新资讯内容，其他类型不缓存
        public async Task<int> SaveNewsCacheList(NewsCacheListInfo cacheListInfo)
        {
            const string latestJsonFile = "Latest.json";

            await _localFileRepository.SaveNewsListCacheAsync(latestJsonFile, cacheListInfo.FileNameAndListDcit[latestJsonFile]);
            _newsCachedCount++;

            NewsContentCacheProgreessChanged();
            NewsContentCacheCompleted();

            cacheListInfo.IsDataLoaded = true;
            return 1;
        }

        /// <summary>
        /// 缓存列表，仅仅是列表，没有内容 TODO:暂时仅缓存最新资讯内容，其他类型不缓存
        /// </summary>
        /// <param name="cacheListInfo"></param>
        /// <returns></returns>
        public async Task<int> SaveNewsAllCacheList(NewsCacheListInfo cacheListInfo)
        {
            var count = 0;

            const string latestJsonFile = "Latest.json";
            const string mostCommentedJsonFile = "MostCommented.json";
            const string officalJsonFile = "Offical.json";
            const string outsideServerJsonFile = "OutsideServer.json";
            const string matchJsonFile = "Match.json";
            const string guideJsonFile = "Guide.json";

            var jsonFileList = new List<string> { latestJsonFile, mostCommentedJsonFile, officalJsonFile, outsideServerJsonFile, matchJsonFile, guideJsonFile };

            foreach (var jsonFile in jsonFileList)
            {
                await _localFileRepository.SaveNewsListCacheAsync(jsonFile, cacheListInfo.FileNameAndListDcit[jsonFile]);
                count++;
            }

            foreach (var fileName in cacheListInfo.FileNameAndListDcit.Keys)
            {
                await _localFileRepository.SaveNewsListCacheAsync(fileName, cacheListInfo.FileNameAndListDcit[fileName]);
                count++;

                _newsCachedCount++;
                NewsContentCacheProgreessChanged();
            }

            cacheListInfo.IsDataLoaded = true;
            return count;
        }

        #region Event hook
        private void ReadNewsListToCacheProgreessChanged()
        {
            if (ReadNewsListToCacheProgreessChangedEventHandler != null)
            {
                var progressChangedArgs = new ProgressChangedArgs() { Value = _newsToCacheCount };
                ReadNewsListToCacheProgreessChangedEventHandler(this, progressChangedArgs);
            }
        }

        private void ReadNewsListToCacheCompleted()
        {
            if (ReadNewsListToCacheCompletedEventHandler != null)
            {
                var progressChangedArgs = new ProgressChangedArgs() { Value = _newsToCacheCount };
                ReadNewsListToCacheCompletedEventHandler(this, progressChangedArgs);
            }
        }

        private void NewsContentCacheProgreessChanged()
        {
            if (NewsContentCacheProgressChangedEventHandler != null)
            {
                var progressChangedArgs = new ProgressChangedArgs() { Value = _newsCachedCount };
                NewsContentCacheProgressChangedEventHandler(this, progressChangedArgs);
            }
        }

        private void NewsContentCacheCompleted()
        {
            if (NewsContentCacheCompletedEventHandler != null)
            {
                var progressChangedArgs = new ProgressChangedArgs() { Value = _newsCachedCount };
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

        /// <summary>
        /// 根据类型和页数，获取新闻列表信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<List<NewsListInfo>> GetPagedNewsList(NewsType type = NewsType.Latest, int page = 1)
        {
            var url = getNewsListRequestUrl(type, page);
            var json = await GetJsonStringViaHttpAsync(url);

            var newsList = JsonConvert.DeserializeObject<List<NewsListInfo>>(json);

            var selectedNewsList = from n in newsList
                                   where n.Site != "超级辅助"
                                   select n;

            return selectedNewsList.ToList();
        }

        /// <summary>
        /// 获取banner新闻列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<NewsListInfo>> GetBannerNewsList()
        {
            var r = new Random(DateTime.Now.Millisecond);
            var random = r.Next();
            var url = string.Format("http://lolbox.oss.aliyuncs.com/json/v3/news/banner.json?r={0}", random);

            string json = await GetJsonStringViaHttpAsync(url);

            var newsList = JsonConvert.DeserializeObject<List<NewsListInfo>>(json);

            return newsList;
        }

        /// <summary>
        /// 获取新闻类型列表
        /// </summary>
        /// <returns></returns>
        public List<NewsTypeWrapper> GetNewsTypeList()
        {
            var t1 = new NewsTypeWrapper() { Type = NewsType.Latest, DisplayName = "最新资讯", Source = "HELPER" };
            var t2 = new NewsTypeWrapper() { Type = NewsType.MostCommented, DisplayName = "热评资讯", Source = "HELPER" };
            var t3 = new NewsTypeWrapper() { Type = NewsType.Offical, DisplayName = "官方资讯", Source = "HELPER" };
            var t4 = new NewsTypeWrapper() { Type = NewsType.OutsideServer, DisplayName = "外服资讯", Source = "HELPER" };
            var t5 = new NewsTypeWrapper() { Type = NewsType.Match, DisplayName = "赛事资讯", Source = "HELPER" };
            var t6 = new NewsTypeWrapper() { Type = NewsType.Guide, DisplayName = "攻略资讯", Source = "HELPER" };

            var t7 = new NewsTypeWrapper() { Type = NewsType.Funny, DisplayName = "搞笑娱乐", Source = "TC" };
            var t8 = new NewsTypeWrapper() { Type = NewsType.Story, DisplayName = "漫画连载", Source = "TC" };
            var t9 = new NewsTypeWrapper() { Type = NewsType.Beatury, DisplayName = "美图COSPLAY", Source = "TC" };

            var newsTypeList = new List<NewsTypeWrapper>() { t1, t2, t3, t4, t5, t6, t7, t8, t9 };
            return newsTypeList;
        }
    }
}
