﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
                    JieshuoWrapPanel.Children.Add(GetSubCategoryHubTile(videoSubcategory, "jieshuo"));
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

        protected async void BindFunnyVideoList()
        {
            if (FunnyWrapPanel.Children.Count > 0)
                return;

            FunnyLoadingBar.Visibility = Visibility.Visible;
            FunnyRetryNetPanel.Visibility = Visibility.Collapsed;

            FunnyWrapPanel.Children.Clear();
            try
            {
                if (_letvVideoTypeList == null)
                {
                    _letvVideoTypeList = await App.ViewModel.VideoRepository.GetLetvVideoTypeList();
                }

                var funnyList = _letvVideoTypeList.First(l => l.Group == "gaoxiao").SubCategory;

                foreach (var videoSubcategory in funnyList)
                {
                    FunnyWrapPanel.Children.Add(GetSubCategoryHubTile(videoSubcategory, "gaoxiao"));
                }
            }
            catch (Exception)
            {
                FunnyRetryNetPanel.Visibility = Visibility.Visible;
            }
            finally
            {
                FunnyLoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        protected async void BindZhongheVideoList()
        {
            if (ZhongheWrapPanel.Children.Count > 0)
                return;

            ZhongheLoadingBar.Visibility = Visibility.Visible;
            ZhongheRetryNetPanel.Visibility = Visibility.Collapsed;

            ZhongheWrapPanel.Children.Clear();
            try
            {
                if (_letvVideoTypeList == null)
                {
                    _letvVideoTypeList = await App.ViewModel.VideoRepository.GetLetvVideoTypeList();
                }

                var zhongheList = _letvVideoTypeList.First(l => l.Group == "zhonghe").SubCategory;

                foreach (var videoSubcategory in zhongheList)
                {
                    ZhongheWrapPanel.Children.Add(GetSubCategoryHubTile(videoSubcategory, "zhonghe"));
                }
            }
            catch (Exception)
            {
                ZhongheRetryNetPanel.Visibility = Visibility.Visible;
            }
            finally
            {
                ZhongheLoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        protected async void BindMatchVideoList()
        {
            if (MatchWrapPanel.Children.Count > 0)
                return;

            MatchLoadingBar.Visibility = Visibility.Visible;
            MatchRetryNetPanel.Visibility = Visibility.Collapsed;

            MatchWrapPanel.Children.Clear();
            try
            {
                if (_letvVideoTypeList == null)
                {
                    _letvVideoTypeList = await App.ViewModel.VideoRepository.GetLetvVideoTypeList();
                }

                var matchList = _letvVideoTypeList.First(l => l.Group == "newmatch").SubCategory;

                foreach (var videoSubcategory in matchList)
                {
                    MatchWrapPanel.Children.Add(GetSubCategoryHubTile(videoSubcategory, "match"));
                }
            }
            catch (Exception)
            {
                MatchRetryNetPanel.Visibility = Visibility.Visible;
            }
            finally
            {
                MatchLoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private HubTile GetSubCategoryHubTile(LetvVideoSubcategory videoSubcategory, string groupTag)
        {
            var bitmap = new BitmapImage(new Uri(videoSubcategory.Icon));
            var hubTitle = new HubTile()
            {
                IsFrozen = false,
                Title = videoSubcategory.Name.Length > 4 ? videoSubcategory.Name.Substring(0, 4) : videoSubcategory.Name,
                Margin = new Thickness(0, 0, 5, 10),
                GroupTag = groupTag,
                Style = (Style)Application.Current.Resources["MyHubTileStyle"],
                Source = bitmap,
                Tag = videoSubcategory
            };

            hubTitle.Tap += hubTitle_OnTap;

            if (videoSubcategory.DailyUpdate > 0)
            {
                hubTitle.Notification = string.Format("{0} 个更新", videoSubcategory.DailyUpdate);
                hubTitle.DisplayNotification = true;
            }

            return hubTitle;
        }

        private void hubTitle_OnTap(object sender, EventArgs e)
        {
            var hubTitle = sender as HubTile;
            if (hubTitle != null)
            {
                var videoSubcategory = hubTitle.Tag as LetvVideoSubcategory;
                if (videoSubcategory != null)
                {
                    var url = string.Format("/VideoTypeListPage.xaml?tag={0}&img={1}&name={2}", videoSubcategory.Tag, videoSubcategory.Icon, videoSubcategory.Name);
                    NavigationService.Navigate(new Uri(url, UriKind.Relative));
                }
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
                    HubTileService.FreezeGroup("gaoxiao");
                    HubTileService.FreezeGroup("zhonghe");
                    HubTileService.FreezeGroup("match");

                    HubTileService.UnfreezeGroup("jieshuo");
                    BindJieshuoVideoList();
                    break;
                case 2:
                    //搞笑
                    HubTileService.FreezeGroup("jieshuo");
                    HubTileService.FreezeGroup("zhonghe");
                    HubTileService.FreezeGroup("match");

                    HubTileService.UnfreezeGroup("gaoxiao");
                    BindFunnyVideoList();
                    break;
                case 3:
                    //综合
                    HubTileService.FreezeGroup("gaoxiao");
                    HubTileService.FreezeGroup("jieshuo");
                    HubTileService.FreezeGroup("match");

                    HubTileService.UnfreezeGroup("zhonghe");
                    BindZhongheVideoList();
                    break;
                case 4:
                    //比赛
                    HubTileService.FreezeGroup("gaoxiao");
                    HubTileService.FreezeGroup("zhonghe");
                    HubTileService.FreezeGroup("jieshuo");

                    HubTileService.UnfreezeGroup("match");
                    BindMatchVideoList();
                    break;
                case 5:
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

        private void LetvVideoPage_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (ActionPopup.IsOpen)
            {
                ActionPopup.Hide();
                e.Cancel = true;
            }
        }
    }
}