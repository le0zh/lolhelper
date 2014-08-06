using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using LolWikiApp.Repository;

namespace LolWikiApp.ViewModels
{
    public class NewsViewModel
    {
        public readonly NewsRepository NewsRepository;

        public NewsCacheListInfo NewsCacheListInfo { get; private set; }

        private NewsType _oldNewsType;
        public ObservableCollection<NewsListInfo> NewsListInfObservableCollection { get; private set; }

        private List<NewsTypeWrapper> _newsTypeList;
        public List<NewsTypeWrapper> NewsTypeList
        {
            get
            {
                if (_newsTypeList != null && _newsTypeList.Count != 0)
                    return _newsTypeList;

                return _newsTypeList = NewsRepository.GetNewsTypeList();
            }
        }

        public async void LoadCachedNewsList()
        {
           var count =  await NewsRepository.LoadNewsCachedListInfo(NewsCacheListInfo);
            Debug.WriteLine("-------LOADED CACHED NEWS LIST COUNT " + count);
        }

        public int CurrentPage { get; set; }

        public int TotalPage { get; set; }

        public NewsViewModel()
        {
            NewsListInfObservableCollection = new ObservableCollection<NewsListInfo>();
            _oldNewsType = NewsType.Latest;

            NewsRepository = new NewsRepository();
            NewsCacheListInfo = new NewsCacheListInfo();
        }

        //public async Task LoadHeadLineListForHomePageAsync(int size = 10)
        //{
        //    List<News> list = await this.newsRepository.GetRecentNewsListAsync(size);

        //    HeadLineListForHomePage.Clear();
        //    foreach (var n in list)
        //    {
        //        HeadLineListForHomePage.Add(n);
        //    }
        //}

        //public async Task LoadHeaderLinesListForListPageAsync()
        //{
        //    List<News> list = await this.newsRepository.GetTopicNewsListAsync();

        //    HeaderLinesListForListPage.Clear();
        //    foreach (var n in list)
        //    {
        //        HeaderLinesListForListPage.Add(n);
        //    }
        //}

        public async Task<List<NewsListInfo>> LoadNewsListInfoListAsync(NewsType type, int page = 1)
        {
            List<NewsListInfo> newsList = await this.NewsRepository.GetPagedNewsList(type, page);
            return newsList;
        }

        public async Task LoadNewsListInfosByTypeAndPageAsync(NewsType type = NewsType.Latest, int page = 1, bool refresh = false)
        {
            List<NewsListInfo> newsList = await this.NewsRepository.GetPagedNewsList(type, page);

            if (refresh || (type != _oldNewsType))
            {
                _oldNewsType = type;
                NewsListInfObservableCollection.Clear();
            }

            foreach (var n in newsList)
            {
                NewsListInfObservableCollection.Add(n);
            }
        }

        public async Task<NewsDetail> GetNewsDetailAsync(string artId)
        {
            NewsDetail newsDetail = await NewsRepository.GetNewsDetailAsync(artId);
            return newsDetail;
        }

    }
}
