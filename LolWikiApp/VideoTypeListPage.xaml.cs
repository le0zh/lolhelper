using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace LolWikiApp
{
    public class VideoTypeListPageHeaderInfo
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public string Img { get; set; }
    }
    public partial class VideoTypeListPage : PhoneApplicationPage
    {
        private int _currentPage;

        protected readonly FullScreenPopup ActionPopup;

        public ObservableCollection<LetvVideoListInfo> VideoList;

        public VideoTypeListPageHeaderInfo HeaderInfo { get; set; }

        public VideoTypeListPage()
        {
            InitializeComponent();

            ActionPopup = new FullScreenPopup();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var tag = string.Empty;
            var name = string.Empty;
            var img = string.Empty;

            var flag = NavigationContext.QueryString.TryGetValue("tag", out tag) &&
                       NavigationContext.QueryString.TryGetValue("name", out name);
                       NavigationContext.QueryString.TryGetValue("img", out img);

            if (flag == false)
            {
                NavigationService.GoBack();
                return;
            }

            HeaderInfo = new VideoTypeListPageHeaderInfo() { Name = name, Tag = tag, Img = img };
            DataContext = HeaderInfo;

            VideoList = new ObservableCollection<LetvVideoListInfo>();
            _currentPage = 1;

            if (VideoLongListSelector.ItemsSource == null)
            {
                VideoLongListSelector.ItemsSource = VideoList;
            }

            LoadVideoList();

            base.OnNavigatedTo(e);
        }

        private async void LoadVideoList()
        {
            LetvVideoListInfo lastVideoListInfo = null;
            if (_currentPage == 1)
            {
                LoadingBar.Visibility = Visibility.Visible;
            }
            else
            {
                lastVideoListInfo = VideoList.Last();
            }

            try
            {
                var list = await App.ViewModel.VideoRepository.GetTypedLetvVideoListyAsync(HeaderInfo.Tag, _currentPage);
                foreach (var videoListInfo in list)
                {
                    VideoList.Add(videoListInfo);
                }
            }
            catch (Exception)
            {
                if (_currentPage == 1)
                {
                    RetryNetPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    ToastPromts.GetToastWithImgAndTitle("没有更多视频了。").Show();
                }
            }
            finally
            {
                VideoLongListSelector.HideGettingMorePanel();
                LoadingBar.Visibility = Visibility.Collapsed;
            }

            if (lastVideoListInfo != null)
            {
                try
                {
                    VideoLongListSelector.ScrollTo(lastVideoListInfo);
                }
                catch
                {
                    Debug.WriteLine("VideoLongListSelector.ScrollTo(lastVideoListInfo) failed.");
                }
            }
        }

        private void VideoLongListSelector_OnGettingMoreTriggered(object sender, EventArgs e)
        {
            _currentPage++;
            LoadVideoList();
        }

        private void VideoLongListSelector_OnTap(object sender, GestureEventArgs e)
        {
            var longListSelector = sender as RefreshableListBox;
            if (longListSelector != null && longListSelector.SelectedItem != null)
            {
                var videoListInfo = longListSelector.SelectedItem as LetvVideoListInfo;
                if (videoListInfo != null)
                {
                    longListSelector.SelectedItem = null;//reset selected item
                    App.ViewModel.VideoRepository.PrepareLetvVideoActionPopup(ActionPopup, videoListInfo);
                    ActionPopup.Show();
                }
            }
        }

        private void VideoTypeListPage_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (ActionPopup.IsOpen)
            {
                ActionPopup.Hide();
                e.Cancel = true;
            }
        }
    }
}