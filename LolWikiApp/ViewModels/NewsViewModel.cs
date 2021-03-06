﻿using System;
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
        public readonly LocalFileRepository FileRepository;

        public NewsCacheListInfo NewsCacheListInfo { get; private set; }

        /// <summary>
        /// 标识离线是否在进行中
        /// </summary>
        public bool IsNewsCaching { get; set; }
        public int CachedNewsCount { get; set; }
        public int TotalToCacheCount { get; set; }

        private NewsType _oldNewsType;
        public ObservableCollection<NewsListBaseInfo> NewsListInfObservableCollection { get; private set; }

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
            var count = await NewsRepository.LoadNewsCachedListInfo(NewsCacheListInfo);
            Debug.WriteLine("-------LOADED CACHED NEWS LIST COUNT " + count);
        }

       public int CurrentPageForFunnyNews { get; set; }

        public int TotalPage { get; set; }

        public NewsViewModel()
        {
            NewsListInfObservableCollection = new ObservableCollection<NewsListBaseInfo>();

            _oldNewsType = NewsType.Latest;

            NewsRepository = new NewsRepository();
            FileRepository = new LocalFileRepository();
            NewsCacheListInfo = new NewsCacheListInfo();
        }

        public async Task<List<NewsListInfo>> LoadNewsListInfoListAsync(NewsType type, int page = 1)
        {
            var newsList = await this.NewsRepository.GetPagedNewsList(type, page);
            return newsList;
        }

        public async Task LoadNewsListInfosByTypeAndPageAsync(NewsTypeWrapper typeWrapper, int page = 1, bool refresh = false)
        {
            
            if (refresh || (typeWrapper.Type != _oldNewsType))
            {
                _oldNewsType = typeWrapper.Type;
                NewsListInfObservableCollection.Clear();
            }

            if (typeWrapper.Source == "HELPER")
            {
                var newsList = await NewsRepository.GetPagedNewsList(typeWrapper.Type, page);

                if (page == 1)
                {
                    if (typeWrapper.Type == NewsType.Latest)
                    {
                        try
                        {
                            var bannerList = await NewsRepository.GetBannerNewsList();
                            var bannerNewsInfo = new NewsListInfo() { IsFlipNews = true };
                            bannerNewsInfo.BannerListInfos.AddRange(bannerList);
                            NewsListInfObservableCollection.Add(bannerNewsInfo);
                        }
                        catch
                        {
                            Debug.WriteLine("banner news got failed.");
                        }
                    }
                    //隐藏其他类型资讯的banner图片
                    //else
                    //{
                    //    var bannerNewsInfo = new NewsListInfo() { IsFlipNews = true };
                    //    bannerNewsInfo.BannerListInfos.Add(new NewsListInfo()
                    //    {
                    //        Img = "/Data/banner3.png"
                    //    });
                    //    NewsListInfObservableCollection.Add(bannerNewsInfo);
                    //}
                }

                foreach (var n in newsList)
                {
                    n.NewsType = typeWrapper;
                    NewsListInfObservableCollection.Add(n);
                }
            }
            else if (typeWrapper.Source == "TC")
            {
                var newsList = await NewsRepository.GetTcPagedNewsList(typeWrapper.Type, page);
                foreach (var n in newsList)
                {
                    n.NewsType = typeWrapper;
                    NewsListInfObservableCollection.Add(n);
                }
            }
        }

        public async void LoadeNewsListInfoFromCache(NewsType type = NewsType.Latest)
        {
            if (!App.NewsViewModel.NewsCacheListInfo.IsDataLoaded)
            {
                await NewsRepository.LoadNewsCachedListInfo(App.NewsViewModel.NewsCacheListInfo);
            }

            NewsListInfObservableCollection.Clear();
            foreach (var n in (App.NewsViewModel.NewsCacheListInfo.GetListByNewsType(type)))
            {
                NewsListInfObservableCollection.Add(n);
            }
        }

        public async Task<NewsDetail> GetNewsDetailAsync(string artId)
        {
            var newsDetail = await NewsRepository.GetNewsDetailAsync(artId);
            return newsDetail;
        }

    }
}
