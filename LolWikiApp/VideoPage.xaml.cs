using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace LolWikiApp
{
    public partial class VideoPage : PhoneApplicationPage
    {
        public VideoPage()
        {
            InitializeComponent();
        }

        public void PlayVideo()
        {
            //MediaPlayerLauncher player = new MediaPlayerLauncher();
            //player.Media = new Uri("http://115.28.43.55/lol/index.php/YoukuM3u8/getplaylist/type/mp4/youkuid/XNzUzMzgxMDgw");
            //player.Show();
            string html = @"<html>
<head>
<meta charset='UTF-8'>
<meta http-equiv='X-UA-Compatible' content='IE=edge' />
<meta name='viewport' content='width=device-width, initial-scale=1.0, user-scalable=no, minimum-scale=1.0, maximum-scale=1.0'>
<title>video play</title>
</head>
<body>
<h3>video start</h3>
<video id='youku-html5-player-video' width='300' height='300' x-webkit-airplay='allow' controls='controls' autoplay='true' src='http://v.youku.com/player/getrealM3U8/vid/XNzUzNDYwNDgw/type/mp4/v.m3u8'></video>
<h3>video end</h3>
</body>
<html>";
            VideoWebBrowser.NavigateToString(html);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PlayVideo();

            base.OnNavigatedTo(e);
        }
    }
}