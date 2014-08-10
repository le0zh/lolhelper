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
            MediaPlayerLauncher player = new MediaPlayerLauncher();
            player.Media = new Uri("http://115.28.43.55/lol/index.php/YoukuM3u8/getplaylist/type/mp4/youkuid/XNzUzMzgxMDgw");
            player.Show();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            PlayVideo();
        }
    }
}