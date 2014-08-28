using System;
using System.Globalization;
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

            ReadNewsCachedSize();
        }

        private async void CacheNewsList()
        {
            App.NewsViewModel.NewsRepository.NewsListCacheProgreessChangedEventHandler += (s, e) =>
            {
                InfoTextBlock.Text = "缓存资讯列表中: " + e.Value.ToString(CultureInfo.InvariantCulture);
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
                if (Math.Abs(e.Value - CachingProgressBar.Maximum) < 0.000001)
                {
                    InfoTextBlock2.Text = "资讯内容缓存完成";
                }
                else
                {
                    InfoTextBlock2.Text = "资讯内容缓存中 " + string.Format("{0:F2}%", e.Value / CachingProgressBar.Maximum * 100);
                }
            };

            App.NewsViewModel.NewsRepository.NewsContentCacheCompletedEventHandler += (s, e) =>
            {
                
                var tost = ToastPromts.GetToastWithImgAndTitle("资讯内容缓存完成!");
                tost.Show();

                //var sbShow = this.Resources["ShowCacheStoryboard"] as Storyboard;
                //if (sbShow != null) sbShow.Begin();
                ReadNewsCachedSize();
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
                sizeDisplay = string.Format("{0:F} KB", size/ 1024.0);
            }
            else
            {
                sizeDisplay = string.Format("{0:F} MB", size / 1048576.0);
            }

            CachedSizeTextBlock.Text = string.Format("已缓存内容：{0}", sizeDisplay);

            var sbShow = this.Resources["ShowCacheStoryboard"] as Storyboard;
            if (sbShow != null) sbShow.Begin();
        }
    }
}