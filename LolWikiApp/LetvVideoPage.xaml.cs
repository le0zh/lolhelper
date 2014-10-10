using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace LolWikiApp
{
    public partial class LetvVideoPage : PhoneApplicationPage
    {
        protected readonly FullScreenPopup ActionPopup;
        private List<LetvVideoTypeListInfo> _letvVideoTypeList;
        private int currentLateastPage = 1;

        private ObservableCollection<LetvVideoListInfo> _letvLatestVideoListInfos;


        public LetvVideoPage()
        {
            InitializeComponent();

            _letvLatestVideoListInfos = new ObservableCollection<LetvVideoListInfo>();
            ActionPopup = new FullScreenPopup();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LatestVideoLongListSelector.ItemsSource = _letvLatestVideoListInfos;

            base.OnNavigatedFrom(e);
        }

        protected void BindDataCommon(StackPanel loadingBar, HttpRequest404Control retryBar, Action bindAction)
        {
            loadingBar.Visibility = Visibility.Visible;
            retryBar.Visibility = Visibility.Collapsed;

            try
            {
                bindAction();
            }
            catch (Exception)
            {
                retryBar.Visibility = Visibility.Visible;
            }
            finally
            {
                loadingBar.Visibility = Visibility.Collapsed;
            }
        }

        protected async void BindLateastVideoList()
        {
            LateastLoadingBar.Visibility = Visibility.Visible;
            LateastRetryNetPanel.Visibility = Visibility.Collapsed;

            try
            {
                if (currentLateastPage == 1)
                {
                    _letvLatestVideoListInfos.Clear();
                }
                var lateastVideoList = await App.ViewModel.VideoRepository.GetLetvLateastVideoList(currentLateastPage);
                foreach (var info in lateastVideoList)
                {
                    _letvLatestVideoListInfos.Add(info);
                }
            }
            catch (Exception)
            {
                LateastRetryNetPanel.Visibility = Visibility.Visible;
            }
            finally
            {
                LateastLoadingBar.Visibility = Visibility.Collapsed;
            }
        }



        protected async void BindJieshuoVideoList()
        {
            if (JieshuoWrapPanel.Children.Count > 0)
                return;

            JieshuoLoadingBar.Visibility = Visibility.Visible;
            JieshuoRetryNetPanel.Visibility = Visibility.Collapsed;

            JieshuoWrapPanel.Children.Clear();
            try
            {
                if (_letvVideoTypeList == null)
                {
                    _letvVideoTypeList = await App.ViewModel.VideoRepository.GetLetvVideoTypeList();
                }

                var jieshuoList = _letvVideoTypeList.First(l => l.Group == "jieshuo").SubCategory;

                foreach (var videoSubcategory in jieshuoList)
                {
                    var bitmap = new BitmapImage(new Uri(videoSubcategory.Icon));

                    var hubTitle = new HubTile()
                    {
                        IsFrozen = false,
                        Title = videoSubcategory.Name,
                        Margin = new Thickness(0, 0, 12, 12),
                        GroupTag = "jieshuo",
                        Style = (Style)Application.Current.Resources["MyHubTileStyle"],
                        Source = bitmap
                    };

                    if (videoSubcategory.DailyUpdate > 0)
                    {
                        hubTitle.Notification = string.Format("{0} 个更新", videoSubcategory.DailyUpdate);
                        hubTitle.DisplayNotification = true;
                    }
                    JieshuoWrapPanel.Children.Add(hubTitle);
                }
            }
            catch (Exception)
            {
                JieshuoRetryNetPanel.Visibility = Visibility.Visible;
            }
            finally
            {
                JieshuoLoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private void VideoListLongListSelector_OnTap(object sender, GestureEventArgs e)
        {
            var longListSelector = sender as RefreshableListBox;
            if (longListSelector != null && longListSelector.SelectedItem != null)
            {
                var videoListInfo = longListSelector.SelectedItem as LetvVideoListInfo;
                if (videoListInfo != null)
                {
                    longListSelector.SelectedItem = null;//reset selected item
                    App.ViewModel.VideoRepository.PrepareLetvVideoActionPopup(ActionPopup, videoListInfo, NavigationService);
                    ActionPopup.Show();
                }
            }
        }
        private void MainPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (MainPivot.SelectedIndex)
            {
                case 0:
                    //最新
                    BindLateastVideoList();
                    break;
                case 1:
                    //解说
                    BindJieshuoVideoList();
                    break;
                case 2:
                    //搞笑
                    break;
                case 3:
                    //综合
                    break;
                case 4:
                    //本地
                    break;
            }
        }

        private async void LatestVideoLongListSelector_OnGettingMoreTriggered(object sender, EventArgs e)
        {
            LetvVideoListInfo lastVideoListInfo = null;

            try
            {
                currentLateastPage += 1;
                lastVideoListInfo = _letvLatestVideoListInfos.Last();

                var lateastVideoList = await App.ViewModel.VideoRepository.GetLetvLateastVideoList(currentLateastPage);
                foreach (var info in lateastVideoList)
                {
                    _letvLatestVideoListInfos.Add(info);
                }
            }
            catch (Exception)
            {
                ToastPromts.GetToastWithImgAndTitle("网络不太稳定，加载获取失败.").Show();
                return;
            }
            finally
            {
                LatestVideoLongListSelector.HideGettingMorePanel();
            }

            if (lastVideoListInfo != null)
            {
                LatestVideoLongListSelector.ScrollTo(lastVideoListInfo);
            }

        }
    }
}