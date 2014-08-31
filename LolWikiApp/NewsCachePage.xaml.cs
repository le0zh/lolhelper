using System;
using System.Diagnostics;
using System.Globalization;
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
                Debug.WriteLine("------------------------------CACHING--------------------");
                HideCachedSizeLoadingIndicator();
                DeleteCacheStackPanel.Visibility = Visibility.Collapsed;
                ContentReadingTipStackPanel.Visibility = Visibility.Visible;
                StartButton.Visibility = Visibility.Collapsed;
                BindNewsCacheEvent();
            }
        }

        private void BindNewsCacheEvent()
        {            
            App.NewsViewModel.NewsRepository.ReadNewsListToCacheProgreessChangedEventHandler += (s, e) =>
            {
                InfoTextBlock.Text = "缓存资讯列表中: " + e.Value.ToString(CultureInfo.InvariantCulture);
            };

            App.NewsViewModel.NewsRepository.ReadNewsListToCacheCompletedEventHandler += (s, e) =>
            {
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
                    var message = "资讯内容缓存中 " + string.Format("{0:F2}%    {1}/{2}", e.Value / CachingProgressBar.Maximum * 100, e.Value, CachingProgressBar.Maximum);
                    InfoTextBlock2.Text = message;
                    Debug.WriteLine("[caching]: " + message);
                }
            };

            App.NewsViewModel.NewsRepository.NewsContentCacheCompletedEventHandler += (s, e) =>
            {

                var tost = ToastPromts.GetToastWithImgAndTitle("资讯内容缓存完成!");
                tost.Show();
                App.NewsViewModel.IsNewsCaching = false;

                ShowCachedSizeLoadingIndicator();

                ReadNewsCachedSize();
            };
        }

        private async void CacheNewsList()
        {
            BindNewsCacheEvent();

            await App.NewsViewModel.NewsRepository.CacheNews();

            //内容缓存完成后，缓存列表信息

            await App.NewsViewModel.NewsRepository.SaveNewsCacheList(App.NewsViewModel.NewsCacheListInfo);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            HideCachedSizeLoadingIndicator();

            App.NewsViewModel.IsNewsCaching = true;
            CacheNewsList();

            ListReadingTipStackPanel.Visibility = Visibility.Visible;
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
    }
}