using System;
using System.Collections.Generic;
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
    public partial class LetvVideoPage : PhoneApplicationPage
    {
        protected readonly FullScreenPopup ActionPopup;

        public LetvVideoPage()
        {
            InitializeComponent();

            ActionPopup = new FullScreenPopup();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BindLatestVideoList();

            base.OnNavigatedTo(e);
        }

        protected async void BindLatestVideoList()
        {
            if (LatestVideoLongListSelector.ItemsSource != null) return;

            LateastLoadingBar.Visibility = Visibility.Visible;
            LateastRetryNetPanel.Visibility = Visibility.Collapsed;

            try
            {
                var videoList = await App.ViewModel.VideoRepository.GetLetvVideoListTest();
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

        private void LatestVideoLongListSelector_OnTap(object sender, GestureEventArgs e)
        {
            if (LatestVideoLongListSelector.SelectedItem != null)
            {
                var videoListInfo = LatestVideoLongListSelector.SelectedItem as LetvVideoListInfo;
                if (videoListInfo != null)
                {
                    LatestVideoLongListSelector.SelectedItem = null;//reset selected item
                    App.ViewModel.VideoRepository.PrepareLetvVideoActionPopup(ActionPopup, videoListInfo, NavigationService);
                    ActionPopup.Show();
                }
            }
        }
    }
}