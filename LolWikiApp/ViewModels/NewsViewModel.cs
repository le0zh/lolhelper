using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly NewsRepository newsRepository;

        private NewsType oldNewsType;
        public ObservableCollection<NewsListInfo> NewsListInfObservableCollection { get; private set; }

        private List<NewsTypeWrapper> newsTypeList;
        public List<NewsTypeWrapper> NewsTypeList
        {
            get
            {
                if (newsTypeList != null && newsTypeList.Count != 0)
                    return newsTypeList;

                return newsTypeList = newsRepository.GetNewsTypeList();
            }
        }

        public int CurrentPage { get; set; }

        public int TotalPage { get; set; }

        public NewsViewModel()
        {
            NewsListInfObservableCollection = new ObservableCollection<NewsListInfo>();
            oldNewsType = NewsType.Latest;

            newsRepository = new NewsRepository();
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

        public async Task LoadNewsListInfosByTypeAndPageAsync(NewsType type = NewsType.Latest, int page = 1, bool refresh = false)
        {
            List<NewsListInfo> newsList = await this.newsRepository.GetPagedNewsList(type, page);

            if (refresh || (type != oldNewsType))
            {
                oldNewsType = type;
                NewsListInfObservableCollection.Clear();
            }

            foreach (var n in newsList)
            {
                NewsListInfObservableCollection.Add(n);
            }
        }

        public async Task<NewsDetail> GetNewsDetailAsync(string artId)
        {
            NewsDetail newsDetail = await newsRepository.GetNewsDetailAsync(artId);
            return newsDetail;
        }

    }
}
