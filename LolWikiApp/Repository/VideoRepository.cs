using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using Windows.Storage.FileProperties;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LolWikiApp.Repository
{
    public class VideoRepository : Repository
    {
        #region m3u8 style
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

            var videoList = JsonConvert.DeserializeObject<List<VideoListInfo>>(json);
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

        #endregion

        #region letv style
        //LeTV:
        private const string LetvTypedVieoRequestUrl = "http://box.dwstatic.com/apiVideoesNormal.php?v=25&action=l&p={0}&OSType=iOS7.1.1&src=letv&tag={1}";//{0}:page;{1}:tag

        private const string LetvVideoRequestUrl = "http://api.letvcloud.com/gpc.php?cf=html5&sign=signxxxxx&ver=2.0&vu={0}&uu=20c3de8a2e&format=jsonp&callback="; //{0}:vu:videoid        
        private const string LetvLatestVideoListRequestUrl = "http://box.dwstatic.com/apiVideoesNormal.php?v=25&action=l&p={0}&OSType=iOS7.1.1&src=letv&tag=newest";///{0}p:pagenumber
        private const string LetvVideoTypeRequestUrl = "http://box.dwstatic.com/apiVideoesNormal.php?sn=%E7%BD%91%E9%80%9A%E5%9B%9B&action=c&pn=%E6%B5%AA%E6%BD%AE%E4%B9%8B%E5%B7%85&OSType=iOS7.1.1&v=25";


        public async Task<List<LetvVideoTypeListInfo>> GetLetvVideoTypeList()
        {
            const string url = LetvVideoTypeRequestUrl;
            var json = await GetJsonStringViaHttpAsync(url);

            var videoList = JsonConvert.DeserializeObject<List<LetvVideoTypeListInfo>>(json);
            var orderedList = from v in videoList
                              select v;

            return orderedList.ToList();
        }

        public async Task<List<LetvVideoListInfo>> GetLetvLateastVideoList(int page = 1)
        {
            var url = string.Format(LetvLatestVideoListRequestUrl, page);
            var json = await GetJsonStringViaHttpAsync(url);

            var videoList = JsonConvert.DeserializeObject<List<LetvVideoListInfo>>(json);
            var orderedList = from v in videoList
                              select v;

            return orderedList.ToList();
        }

        public async Task<List<LetvVideoListInfo>> GetTypedLetvVideoListyAsync(string tag, int page = 1)
        {
            var url = string.Format(LetvTypedVieoRequestUrl, page, tag);
            var json = await GetJsonStringViaHttpAsync(url);

            var videoList = JsonConvert.DeserializeObject<List<LetvVideoListInfo>>(json);
            var orderedList = from v in videoList
                              select v;

            return orderedList.ToList();
        }

        /// <summary>
        /// 查询并返回视频地址，注意，此处返回一个视频地址列表
        /// </summary>
        /// <param name="vu"></param>
        /// <returns>返回一个视频地址列表，0：标清，1：高清，2：超清， 3：原画</returns>
        public async Task<List<string>> GetNewsVideoUrlAsync(string vu)
        {
            var videoList = new List<string>();
            if (string.IsNullOrEmpty(vu))
                return videoList;

            string url = string.Format(LetvVideoRequestUrl, vu);
            string json = await GetJsonStringViaHttpAsync(url);

            if (json.StartsWith("(") && json.EndsWith(")"))
            {
                json = json.Substring(1, json.Length - 2);
            }

            var jObject = JObject.Parse(json);

            var myLetvSourceConverter = new LetvSourceConverter();

            var standardVideo = jObject["data"]["video_list"]["video_1"]["main_url"].ToString();
            var hdVideo = jObject["data"]["video_list"]["video_2"]["main_url"].ToString();
            var superHdVideo = jObject["data"]["video_list"]["video_3"]["main_url"].ToString();
            var originalHdVideo = jObject["data"]["video_list"]["video_4"]["main_url"].ToString();

            videoList.Add(myLetvSourceConverter.Decode(standardVideo));
            videoList.Add(myLetvSourceConverter.Decode(hdVideo));
            videoList.Add(myLetvSourceConverter.Decode(superHdVideo));
            videoList.Add(myLetvSourceConverter.Decode(originalHdVideo));

            return videoList;
        }

        private void ShowAndPlayVideo(string videoUrl)
        {
            if (string.IsNullOrEmpty(videoUrl)) return;

            var player = new MediaPlayerLauncher { Media = new Uri(videoUrl), Controls = MediaPlaybackControls.All };
            player.Show();
        }

        private Button GetVdieoCacheButton(string text, string src, FullScreenPopup actionPopup)
        {
            const string template = @"<Button xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Width='100' Margin='0' BorderThickness='0' HorizontalAlignment='Center'>
                            <Grid>
                                <Grid.RowDefinitions>
                                    <RowDefinition Height='30'></RowDefinition>
                                    <RowDefinition Height='*'></RowDefinition>
                                </Grid.RowDefinitions>
                                <Path Grid.Row='0' Width='32.24' Height='30'  Stretch='Fill' Fill='#FFFFFF' Data='F1 M 48,39L 56,39L 56,49L 63.25,49L 52,60.25L 40.75,49L 48,49L 48,39 Z M 20,20L 50.25,20L 56,25.75L 56,38L 52,38L 52,27.25L 48.75,24L 48,24L 48,37L 28,37L 28,24L 24,24L 24,52L 42.25,52L 46.25,56L 20,56L 20,20 Z M 39,24L 39,34L 44,34L 44,24L 39,24 Z '/>
                                <TextBlock Grid.Row='1' Text='&TEXT&' Foreground='WhiteSmoke' HorizontalAlignment='Center'></TextBlock>
                            </Grid>
                        </Button>";

            var btn = XamlReader.Load(template.Replace("&TEXT&", text)) as Button;
            if (btn != null)
            {
                btn.Tap += (s, e) =>
                {
                    actionPopup.Hide();

                    var localFileRepository = new LocalFileRepository();
                    //localFileRepository.DownloadAsync(src, "test.mp4", new CancellationToken());
                };
            }

            return btn;
        }

        private Button GetVideoPlayButton(string text, string src, FullScreenPopup actionPopup)
        {
            const string template = @"<Button xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Width='100' Margin='0' BorderThickness='0' HorizontalAlignment='Center'>
                            <Grid>
                                <Grid.RowDefinitions>
                                    <RowDefinition Height='30'></RowDefinition>
                                    <RowDefinition Height='*'></RowDefinition>
                                </Grid.RowDefinitions>
                               <Path Grid.Row='0' Width='36' Height='26'  Stretch='Fill' Fill='#FFFFFF' Data='F1 M 21,25L 55,25C 56.1045,25 57,25.8955 57,27L 57,49C 57,50.1046 56.1045,51 55,51L 21,51C 19.8954,51 19,50.1046 19,49L 19,27C 19,25.8955 19.8954,25 21,25 Z M 26.7692,29C 25.2398,29 24,30.5111 24,32.375L 24,43.625C 24,45.489 25.2398,47 26.7692,47L 42.2307,47C 43.7601,47 45,45.489 45,43.625L 45,41L 51,46L 52,46L 52,30L 51,30L 45,35L 45,32.375C 45,30.5111 43.7601,29 42.2307,29L 26.7692,29 Z '/>
                                <TextBlock Grid.Row='1' Text='&TEXT&' Foreground='WhiteSmoke' HorizontalAlignment='Center'></TextBlock>
                            </Grid>
                        </Button>";

            var btn = XamlReader.Load(template.Replace("&TEXT&", text)) as Button;
            if (btn != null)
            {
                btn.Tap += (s, e) =>
                {
                    actionPopup.Hide();
                    ShowAndPlayVideo(src);
                };
            }

            return btn;
        }

        public async void PrepareLetvVideoActionPopup(FullScreenPopup actionPopup, LetvVideoListInfo videoListInfo, NavigationService ns)
        {
            var mainStackPanel = new StackPanel
            {
                Background = new SolidColorBrush(Colors.Gray),
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            //TODO: TIME LONG
            var videoList = await GetNewsVideoUrlAsync(videoListInfo.Letv_Video_Unique);

            mainStackPanel.Tap += (s, e) => e.Handled = true;

            var titleBorder = new Border()
            {
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = new SolidColorBrush(Colors.DarkGray)
            };

            var titleTextBlock = new TextBlock()
            {
                Text = videoListInfo.Title,
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
                Margin = new Thickness(0, 10, 0, 20)
            };

            //var xamlBtnSd = GetVideoPlayButton("标清", videoList[0], actionPopup);
            var xamlBtnHd = GetVideoPlayButton("高清", videoList[1], actionPopup);
            var xamlBtnSuperHd = GetVideoPlayButton("超清", videoList[2], actionPopup);
            var xamlBtnOriginal = GetVideoPlayButton("原画", videoList[3], actionPopup);
            var xamlBtnCache = GetVdieoCacheButton("下载", videoList[2], actionPopup);

            //actionPanel.Children.Add(xamlBtnSd);
            actionPanel.Children.Add(xamlBtnHd);
            actionPanel.Children.Add(xamlBtnSuperHd);
            actionPanel.Children.Add(xamlBtnOriginal);
            actionPanel.Children.Add(xamlBtnCache);

            grid.Children.Add(actionPanel);

            mainStackPanel.Children.Add(titleBorder);
            mainStackPanel.Children.Add(grid);

            actionPopup.Child = mainStackPanel;
        }
        #endregion
    }
}
