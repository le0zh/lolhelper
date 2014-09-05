using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace LolWikiApp
{
    public partial class NewsCachePage : PhoneApplicationPage
    {
        public NewsCachePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (App.NewsViewModel.IsNewsCaching == false)
            {
                ReadNewsCachedSize();
            }
            else
            {
                HideCachedSizeLoadingIndicator();
                DeleteCacheStackPanel.Visibility = Visibility.Collapsed;
                
                StartButton.Visibility = Visibility.Collapsed;
                BindNewsCacheEvent();
                CachingProgressBar.Value = App.NewsViewModel.CachedNewsCount;
                CachingProgressBar.Maximum = App.NewsViewModel.TotalToCacheCount;
                if (App.NewsViewModel.CachedNewsCount == 0)
                {
                    ListReadingTipStackPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    ContentReadingTipStackPanel.Visibility = Visibility.Visible;
                    //var message = "资讯内容缓存中 " + string.Format("{0:F2}%    {1}/{2}", CachingProgressBar.Value / CachingProgressBar.Maximum * 100, CachingProgressBar.Value, App.NewsViewModel.TotalToCacheCount);
                    var message = "资讯内容缓存中 " + string.Format("{0:F2}%", CachingProgressBar.Value / CachingProgressBar.Maximum * 100);
                    InfoTextBlock2.Text = message;
                }
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (App.NewsViewModel.IsNewsCaching)
            {
                App.NewsViewModel.CachedNewsCount = (int)CachingProgressBar.Value;
            }

            base.OnNavigatingFrom(e);
        }

        private void BindNewsCacheEvent()
        {
            App.NewsViewModel.NewsRepository.ReadNewsListToCacheProgreessChangedEventHandler += (s, e) =>
            {
                InfoTextBlock.Text = "缓存资讯列表中: " + e.Value.ToString(CultureInfo.InvariantCulture);
            };

            App.NewsViewModel.NewsRepository.ReadNewsListToCacheCompletedEventHandler += (s, e) =>
            {
                App.NewsViewModel.TotalToCacheCount = e.Value;
                CachingProgressBar.Maximum = e.Value;
                ListReadingTipStackPanel.Visibility = Visibility.Collapsed;
                ContentReadingTipStackPanel.Visibility = Visibility.Visible;
            };

            App.NewsViewModel.NewsRepository.NewsContentCacheProgressChangedEventHandler += (s, e) =>
            {
                CachingProgressBar.Value = e.Value;
                if (Math.Abs(e.Value - CachingProgressBar.Maximum) < 0.000001)
                {
                    InfoTextBlock2.Text = "资讯内容缓存完成";
                }
                else
                {
                    //var message = "资讯内容缓存中 " + string.Format("{0:F2}%    {1}/{2}", e.Value / CachingProgressBar.Maximum * 100, e.Value, App.NewsViewModel.TotalToCacheCount);
                    var message = "资讯内容缓存中 " + string.Format("{0:F2}%", e.Value / CachingProgressBar.Maximum * 100);
                    InfoTextBlock2.Text = message;
                    Debug.WriteLine("[caching]: " + message);
                }
            };

            App.NewsViewModel.NewsRepository.NewsContentCacheCompletedEventHandler += (s, e) =>
            {
                var tost = ToastPromts.GetToastWithImgAndTitle("资讯内容缓存完成!");
                tost.Show();
                StartButton.Visibility = Visibility.Collapsed;
                App.NewsViewModel.IsNewsCaching = false;
                App.NewsViewModel.CachedNewsCount = 0;
                ShowCachedSizeLoadingIndicator();

                ReadNewsCachedSize();
            };
        }

        private async void CacheNewsList()
        {
            BindNewsCacheEvent();

            try
            {
                await App.NewsViewModel.NewsRepository.CacheNews();
            }
            catch (Exception ex404)
            {
                ResetCacheProgress();
                ToastPromts.GetToastWithImgAndTitle("网络貌似有问题，稍后重试").Show();
                App.NewsViewModel.IsNewsCaching = false;
                
                return;
            }

            //内容缓存完成后，缓存列表信息

            await App.NewsViewModel.NewsRepository.SaveNewsCacheList(App.NewsViewModel.NewsCacheListInfo);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            HideCachedSizeLoadingIndicator();

            App.NewsViewModel.IsNewsCaching = true;
            CachingProgressBar.Value = 0;
            InfoTextBlock.Text = "缓存资讯列表中: ";
            CacheNewsList();

            ListReadingTipStackPanel.Visibility = Visibility.Visible;
            ContentReadingTipStackPanel.Visibility = Visibility.Collapsed;
            StartButton.Visibility = Visibility.Collapsed;

            var sbHide = this.Resources["HideCacheStoryboard"] as Storyboard;
            if (sbHide != null) sbHide.Begin();
        }

        private async void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            await App.NewsViewModel.FileRepository.ClearNewsCache();

            var tost = ToastPromts.GetToastWithImgAndTitle("资讯缓存内容清除完成!");
            ReadNewsCachedSize();
            tost.Show();

            ListReadingTipStackPanel.Visibility = Visibility.Collapsed;
            ContentReadingTipStackPanel.Visibility = Visibility.Collapsed;
            StartButton.Visibility = Visibility.Visible;
        }

        private async void ReadNewsCachedSize()
        {
            var size = await App.NewsViewModel.FileRepository.GetNewsCacheSizeInByte();
            string sizeDisplay;

            if (size < 1024)
            {
                sizeDisplay = string.Format("{0} bytes", size);
            }
            else if (size < 1048576)
            {
                sizeDisplay = string.Format("{0:F} KB", size / 1024.0);
            }
            else
            {
                sizeDisplay = string.Format("{0:F} MB", size / 1048576.0);
            }

            CachedSizeTextBlock.Text = string.Format("已缓存内容：{0}", sizeDisplay);

            HideCachedSizeLoadingIndicator();

            var sbShow = this.Resources["ShowCacheStoryboard"] as Storyboard;
            if (sbShow != null) sbShow.Begin();

            StartButton.Visibility = Visibility.Visible;
        }

        private void ShowCachedSizeLoadingIndicator()
        {
            CachedSizeLoadingIndicator.IsRunning = true;
            CachedSizeLoadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideCachedSizeLoadingIndicator()
        {
            CachedSizeLoadingIndicator.IsRunning = false;
            CachedSizeLoadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ResetCacheProgress()
        {
            ListReadingTipStackPanel.Visibility = Visibility.Collapsed;
            ContentReadingTipStackPanel.Visibility = Visibility.Collapsed;
            StartButton.Visibility = Visibility.Collapsed;

            ShowCachedSizeLoadingIndicator();
            ReadNewsCachedSize();
        }
    }
}