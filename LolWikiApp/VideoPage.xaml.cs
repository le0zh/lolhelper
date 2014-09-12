using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace LolWikiApp
{
    public partial class VideoPage : PhoneApplicationPage
    {
        protected readonly FullScreenPopup ActionPopup;

        public VideoPage()
        {
            InitializeComponent();

            ActionPopup = new FullScreenPopup();
        }

        #region Pivot Latest video list
        protected async void BindLatestVideoList()
        {
            if (LatestVideoLongListSelector.ItemsSource != null) return;

            LateastLoadingBar.Visibility = Visibility.Visible;
            LateastRetryNetPanel.Visibility = Visibility.Collapsed;

            try
            {
                var videoList = await App.ViewModel.VideoRepository.GetLatestVideoListyAsync();
                LatestVideoLongListSelector.ItemsSource = videoList;
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

        private void LatestVideoLongListSelector_Tap(object sender, GestureEventArgs e)
        {
            if (LatestVideoLongListSelector.SelectedItem != null)
            {
                var videoListInfo = LatestVideoLongListSelector.SelectedItem as VideoListInfo;
                if (videoListInfo != null)
                {
                    App.ViewModel.VideoRepository.PrepareVideoActionPopup(ActionPopup, videoListInfo, NavigationService);
                    ActionPopup.Show();
                }
            }
        }
        #endregion

        #region Pivot Series video list
        protected async void BindSeriesVideoList()
        {
            if (SeriesVideoLongListSelector.ItemsSource != null) return;

            SeriesLoadingBar.Visibility = Visibility.Visible;
            SeriesRetryNetPanel.Visibility = Visibility.Collapsed;

            try
            {
                var videoList = await App.ViewModel.VideoRepository.GetSeriesVideoListyAsync();
                SeriesVideoLongListSelector.ItemsSource = videoList;
            }
            catch (Exception)
            {
                SeriesRetryNetPanel.Visibility = Visibility.Visible;
            }
            finally
            {
                SeriesLoadingBar.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Pivot Game video list
        protected async void BindGameVideoList()
        {
            if (GameVideoLongListSelector.ItemsSource != null) return;

            GameLoadingBar.Visibility = Visibility.Visible;
            GameRetryNetPanel.Visibility = Visibility.Collapsed;

            try
            {
                var videoList = await App.ViewModel.VideoRepository.GetGameVideoListyAsync();
                GameVideoLongListSelector.ItemsSource = videoList;
            }
            catch (Exception)
            {
                GameRetryNetPanel.Visibility = Visibility.Visible;
            }
            finally
            {
                GameLoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Pivot Talker video list
        protected async void BindTalkerVideoList()
        {
            if (TalkerVideoLongListSelector.ItemsSource != null) return;

            TalkerLoadingBar.Visibility = Visibility.Visible;
            TalkerRetryNetPanel.Visibility = Visibility.Collapsed;

            try
            {
                var videoList = await App.ViewModel.VideoRepository.GetTalkerVideoListyAsync();
                TalkerVideoLongListSelector.ItemsSource = videoList;
            }
            catch (Exception)
            {
                TalkerRetryNetPanel.Visibility = Visibility.Visible;
            }
            finally
            {
                TalkerLoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (MainPivot.SelectedIndex)
            {
                case 0:
                    //latest
                    BindLatestVideoList();
                    break;
                case 1:
                    //series
                    BindSeriesVideoList();
                    break;
                case 2:
                    //game
                    BindGameVideoList();
                    break;
                case 3:
                    //talker
                    BindTalkerVideoList();
                    break;
            }
        }

        private void VideoPage_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (ActionPopup.IsOpen)
            {
                e.Cancel = true;
                ActionPopup.Hide();
            }
        }

        private void VideoTypeListSelector_Tap(object sender, GestureEventArgs e)
        {
            var selector = sender as RefreshableListBox;

            if (selector != null && selector.SelectedItem != null)
            {
                var info = selector.SelectedItem as VideoTypeListInfo;
                if (info != null)
                {
                    var index = MainPivot.SelectedIndex;
                    var url = string.Format("/VideoTypeListPage.xaml?name={0}&id={1}&img={2}&type={3}", info.Name, info.Id, info.Img, index);
                    NavigationService.Navigate(new Uri(url, UriKind.Relative));
                }
            }
        }
    }
}