using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace LolWikiApp
{
    public partial class NewsCachePage : PhoneApplicationPage
    {
        public NewsCachePage()
        {
            InitializeComponent();
        }

        private async void cacheNewsList()
        {
            App.NewsViewModel.NewsRepository.NewsListCacheProgreessChangedEventHandler += (s, e) =>
            {
                InfoTextBlock.Text = "缓存资讯列表中: " + e.Value.ToString();
            };

            App.NewsViewModel.NewsRepository.NewsListCacheCompletedEventHandler += (s, e) =>
            {
                InfoTextBlock2.Text = "资讯列表缓存完成: " + e.Value.ToString();
            };


            App.NewsViewModel.NewsRepository.NewsContentCacheProgressChangedEventHandler += (s, e) =>
            {
                InfoTextBlock.Text = "缓存资讯内容中: " + e.Value.ToString();
            };

            App.NewsViewModel.NewsRepository.NewsContentCacheCompletedEventHandler += (s, e) =>
            {
                InfoTextBlock2.Text = "资讯内容缓存完成: " + e.Value.ToString();
            };

            await App.NewsViewModel.NewsRepository.CacheNews();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            cacheNewsList();
        }
    }
}