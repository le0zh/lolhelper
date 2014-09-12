using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using Windows.Storage.FileProperties;
using Newtonsoft.Json;

namespace LolWikiApp.Repository
{
    public class VideoRepository : Repository
    {
        private const string LatestVideoListRequestUrl = "http://lolbox.oss.aliyuncs.com/json/videolist_99.json?r={0}";

        private const string SeriesVideoListRequestUrl = "http://lolbox.oss.aliyuncs.com/json/v4/videotype_3.json?r={0}";
        private const string SeriesVideoesRequesttUrl = "http://lolbox.oss.aliyuncs.com/json/v4/video/videolist_3_{0}_{1}.json?r={2}";//0:id, 1:page, 2:random

        private const string GameVideoListRequestUrl = "http://lolbox.oss.aliyuncs.com/json/v4/videotype_4.json?r={0}";
        private const string GameVideoesRequestUrl = "http://lolbox.oss.aliyuncs.com/json/v4/video/videolist_4_{0}_{1}.json?r={2}";

        private const string TalkerVideoListRequestUrl = "http://lolbox.oss.aliyuncs.com/json/v4/videotype_2.json?r={0}";
        private const string TalkerVideoesRequestUrl = "http://lolbox.oss.aliyuncs.com/json/v4/video/videolist_2_{0}_{1}.json?r={2}";

        public async Task<List<VideoListInfo>> GetLatestVideoListyAsync()
        {
            var random = new Random();
            var url = string.Format(LatestVideoListRequestUrl, random.Next(1, 99999));
            var json = await GetJsonStringViaHttpAsync(url);

            var videoList = JsonConvert.DeserializeObject<List<VideoListInfo>>(json);
            var orderedList = from v in videoList
                              orderby v.Time descending
                              select v;
            return orderedList.ToList();
        }

        public async Task<List<VideoTypeListInfo>> GetSeriesVideoListyAsync()
        {
            var random = new Random();
            var url = string.Format(SeriesVideoListRequestUrl, random.Next(1, 99999));
            var json = await GetJsonStringViaHttpAsync(url);

            var videoList = JsonConvert.DeserializeObject<List<VideoTypeListInfo>>(json);
            var orderedList = from v in videoList
                              orderby v.Time descending
                              select v;
            return orderedList.ToList();
        }

        public async Task<List<VideoTypeListInfo>> GetGameVideoListyAsync()
        {
            var random = new Random();
            var url = string.Format(GameVideoListRequestUrl, random.Next(1, 99999));
            var json = await GetJsonStringViaHttpAsync(url);

            var videoList = JsonConvert.DeserializeObject<List<VideoTypeListInfo>>(json);
            var orderedList = from v in videoList
                              orderby v.Time descending
                              select v;
            return orderedList.ToList();
        }

        public async Task<List<VideoTypeListInfo>> GetTalkerVideoListyAsync()
        {
            var random = new Random();
            var url = string.Format(TalkerVideoListRequestUrl, random.Next(1, 99999));
            var json = await GetJsonStringViaHttpAsync(url);

            var videoList = JsonConvert.DeserializeObject<List<VideoTypeListInfo>>(json);
            var orderedList = from v in videoList
                              orderby v.Time descending
                              select v;
            return orderedList.ToList();
        }

        public async Task<List<VideoListInfo>> GetTypedVideoListyAsync(VideoType type, string id, int page = 1)
        {
            var random = new Random();
            var url = string.Empty;

            switch (type)
            {
                case VideoType.Game:
                    url = string.Format(GameVideoesRequestUrl, id, page, random.Next(1, 99999));
                    break;
                case VideoType.Series:
                    url = string.Format(SeriesVideoesRequesttUrl, id, page, random.Next(1, 99999));
                    break;
                case VideoType.Talker:
                    url = string.Format(TalkerVideoesRequestUrl, id, page, random.Next(1, 99999));
                    break;
            }

            var json = await GetJsonStringViaHttpAsync(url);

            var videoList = JsonConvert.DeserializeObject<List<VideoListInfo >>(json);
            var orderedList = from v in videoList
                              orderby v.Time descending
                              select v;

            return orderedList.ToList();
        }
        

        public void PrepareVideoActionPopup(FullScreenPopup actionPopup, VideoListInfo videoListInfo, NavigationService ns)
        {
            var mainStackPanel = new StackPanel
            {
                Background = new SolidColorBrush(Colors.Gray),
                Orientation = System.Windows.Controls.Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            mainStackPanel.Tap += (s, e) => e.Handled = true;

            var titleBorder = new Border()
            {
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = new SolidColorBrush(Colors.DarkGray)
            };

            var titleTextBlock = new TextBlock()
            {
                Text = videoListInfo.Name,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Application.Current.Resources["PhoneTextLowContrastBrush"] as SolidColorBrush,
                FontSize = (double)Application.Current.Resources["PhoneFontSizeMedium"],
                Margin = new Thickness(12, 12, 12, 12),
            };

            titleBorder.Child = titleTextBlock;

            var grid = new Grid()
            {
                Width = 480
            };

            var actionPanel = new StackPanel()
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,

                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 8, 0, 20)
            };

            const string xamlBtn1 = @"<Button xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Width='100' Margin='0' BorderThickness='0' HorizontalAlignment='Center'>
                            <Grid>
                                <Grid.RowDefinitions>
                                    <RowDefinition Height='30'></RowDefinition>
                                    <RowDefinition Height='*'></RowDefinition>
                                </Grid.RowDefinitions>
                               <Path Grid.Row='0' Width='38' Height='26'  Stretch='Fill' Fill='#FFFFFF' Data='F1 M 21,25L 55,25C 56.1045,25 57,25.8955 57,27L 57,49C 57,50.1046 56.1045,51 55,51L 21,51C 19.8954,51 19,50.1046 19,49L 19,27C 19,25.8955 19.8954,25 21,25 Z M 26.7692,29C 25.2398,29 24,30.5111 24,32.375L 24,43.625C 24,45.489 25.2398,47 26.7692,47L 42.2307,47C 43.7601,47 45,45.489 45,43.625L 45,41L 51,46L 52,46L 52,30L 51,30L 45,35L 45,32.375C 45,30.5111 43.7601,29 42.2307,29L 26.7692,29 Z '/>
                                <TextBlock Grid.Row='1' Text='标清' Foreground='WhiteSmoke' HorizontalAlignment='Center'></TextBlock>
                            </Grid>
                        </Button>";

            var btn1 = XamlReader.Load(xamlBtn1) as Button;
            if (btn1 != null)
            {
                btn1.Tap += (s, e) =>
                {
                    actionPopup.Hide();
                    ns.Navigate(new Uri("/VideoPlay.xaml?source=" + videoListInfo.Video_addr + "&name=" + videoListInfo.Name + "[标清]", UriKind.Relative));
                };
            }

            const string xamlBtn3 = @"<Button xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Width='100'  Margin='12 0 0 0' BorderThickness='0' HorizontalAlignment='Center'>
                            <Grid>
                                <Grid.RowDefinitions>
                                    <RowDefinition Height='30'></RowDefinition>
                                    <RowDefinition Height='*'></RowDefinition>
                                </Grid.RowDefinitions>
                                <Path Grid.Row='0' Width='38' Height='26'  Stretch='Fill' Fill='#FFFFFF' Data='F1 M 21,25L 55,25C 56.1045,25 57,25.8955 57,27L 57,49C 57,50.1046 56.1045,51 55,51L 21,51C 19.8954,51 19,50.1046 19,49L 19,27C 19,25.8955 19.8954,25 21,25 Z M 26.7692,29C 25.2398,29 24,30.5111 24,32.375L 24,43.625C 24,45.489 25.2398,47 26.7692,47L 42.2307,47C 43.7601,47 45,45.489 45,43.625L 45,41L 51,46L 52,46L 52,30L 51,30L 45,35L 45,32.375C 45,30.5111 43.7601,29 42.2307,29L 26.7692,29 Z '/>
                                <TextBlock Grid.Row='1' Text='高清' Foreground='WhiteSmoke' HorizontalAlignment='Center'></TextBlock>
                            </Grid>
                        </Button>";

            var btn3 = XamlReader.Load(xamlBtn3) as Button;
            if (btn3 != null)
            {
                btn3.Tap += (s, e) =>
                {
                    actionPopup.Hide();
                    ns.Navigate(new Uri("/VideoPlay.xaml?source=" + videoListInfo.Video_addr_super + "&name=" + videoListInfo.Name + "[高清]", UriKind.Relative));
                };
            }

            actionPanel.Children.Add(btn1);
            actionPanel.Children.Add(btn3);

            grid.Children.Add(actionPanel);

            mainStackPanel.Children.Add(titleBorder);
            mainStackPanel.Children.Add(grid);

            actionPopup.Child = mainStackPanel;
        }
    }
}
