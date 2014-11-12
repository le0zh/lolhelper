using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.PlayerFramework;

namespace LolWikiApp
{
    public partial class VideoPlay : PhoneApplicationPage
    {
        public VideoPlay()
        {
            InitializeComponent();
        }

        private bool _isNavigated;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_isNavigated == false)
            {
                _isNavigated = true;

                string source;
                string videoName;
                if (!NavigationContext.QueryString.TryGetValue("source", out source))
                {
                    MessageBox.Show("错误的视频地址。");
                    NavigationService.GoBack();
                }

                if (NavigationContext.QueryString.TryGetValue("name", out videoName))
                {
                    Debug.WriteLine(videoName.Length);
                    var msg = videoName.Length > 25 ? videoName.Substring(0, 25) + "..." : videoName;
                    LoadingIndicator.Content = "即将播放: " + msg;
                }
                else
                {
                    LoadingIndicator.Content = "";
                }

                Debug.WriteLine("--->VIDEO PLAY PAGE");

                VideoMediaPlayer.Source = new Uri(source, UriKind.RelativeOrAbsolute);
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ((SolidColorBrush)Application.Current.Resources["PhoneBackgroundBrush"]).Color = Colors.White;
            ((SolidColorBrush)Application.Current.Resources["PhoneForegroundBrush"]).Color = Colors.Black;

            base.OnNavigatedFrom(e);
        }

        private void VideoMediaPlayer_OnMediaLoading(object sender, MediaPlayerDeferrableEventArgs e)
        {
            LoadingIndicator.IsRunning = true;
        }

        private void VideoMediaPlayer_OnMediaOpened(object sender, RoutedEventArgs e)
        {
            LoadingIndicator.IsRunning = false;

            ((SolidColorBrush)Application.Current.Resources["PhoneBackgroundBrush"]).Color = Colors.Black;
            ((SolidColorBrush)Application.Current.Resources["PhoneForegroundBrush"]).Color = Colors.White;
        }
    }
}