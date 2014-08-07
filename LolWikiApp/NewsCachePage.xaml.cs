using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Coding4Fun.Toolkit.Controls;
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

        private async void CacheNewsList()
        {
            App.NewsViewModel.NewsRepository.NewsListCacheProgreessChangedEventHandler += (s, e) =>
            {
                InfoTextBlock.Text = "缓存资讯列表中: " + e.Value.ToString();
            };

            App.NewsViewModel.NewsRepository.NewsListCacheCompletedEventHandler += (s, e) =>
            {
                CachingProgressBar.Maximum = e.Value;
                ListReadingTipStackPanel.Visibility = Visibility.Collapsed;
                ContentReadingTipStackPanel.Visibility = Visibility.Visible;
            };


            App.NewsViewModel.NewsRepository.NewsContentCacheProgressChangedEventHandler += (s, e) =>
            {
                  CachingProgressBar.Value = e.Value;
                  InfoTextBlock2.Text = "资讯内容缓存中 " + string.Format("{0:F2}%", e.Value / CachingProgressBar.Maximum * 100);
            };

            App.NewsViewModel.NewsRepository.NewsContentCacheCompletedEventHandler += (s, e) =>
            {
                InfoTextBlock2.Text = "资讯内容缓存完成";
                //StartButton.IsEnabled = true;
                var tost = ToastPromt.GetToastWithImgAndTitle("资讯内容缓存完成!");
                tost.Show();
            };

            await App.NewsViewModel.NewsRepository.CacheNews();

            //内容缓存完成后，缓存列表信息

            await App.NewsViewModel.NewsRepository.SaveNewsCacheList(App.NewsViewModel.NewsCacheListInfo);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            CacheNewsList();

            ListReadingTipStackPanel.Visibility = Visibility.Visible;
            StartButton.Visibility = Visibility.Collapsed;
        }

        private async void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            await App.NewsViewModel.FileRepository.ClearNewsCache();

            var tost = ToastPromt.GetToastWithImgAndTitle("清除成功!");
            tost.Show();
        }


    }
}