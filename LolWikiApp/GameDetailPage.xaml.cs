using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using LolWikiApp.Repository;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace LolWikiApp
{
    public partial class GameDetailPage : PhoneApplicationPage
    {
        public GameDetailPage()
        {
            InitializeComponent();
        }

        private async Task<string> GetHtmlContentAsync(string url)
        {
            var client = new HttpClient();
            var content = await client.GetStringAsync(new Uri(url));
            return content;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!string.IsNullOrEmpty(App.ViewModel.SelectedDetailGameInfoUrl))
            {
                var detailUrl = App.ViewModel.SelectedDetailGameInfoUrl;

                var reg = new Regex("shareKey=[\\s\\S]+?(?=&)");

                if (reg.Match(detailUrl).Success)
                {
                    var shareKey = reg.Match(detailUrl).Value;
                    var newDetailUrl = "http://zdl.mbox.duowan.com/phone/matchDetail.php?" + shareKey;

                    LoadAndShowInWebBrowser(newDetailUrl);
                }
            }
            base.OnNavigatedTo(e);
        }

        private async void LoadAndShowInWebBrowser(string url)
        {
            Debug.WriteLine("moreInfoUrl:" + url);

            LoadingPanel.Visibility = Visibility.Visible;
            GameDetailGrid.Visibility = Visibility.Collapsed;

            var content = await GetHtmlContentAsync(url);
            var playerRepository = new PlayerRepository();

            var gameDetailInfo = playerRepository.ParseGameDetailTest(content);
            GameDetailGrid.DataContext = gameDetailInfo;

            LoadingPanel.Visibility = Visibility.Collapsed;
            GameDetailGrid.Visibility = Visibility.Visible;
            InfoTipGrid.Visibility = Visibility.Visible;

            #region backup

            //            const string headerContentBlack = @"<html>
            //<head>
            //	<meta charset='UTF-8'>
            //	<meta name='MobileOptimized' content='320'>
            //	<meta name='viewport' content='width=device-width,initial-scale=1.0,minimum-scale=1.0,maximum-scale=1.0,user-scalable=no'>
            //	<meta name='keywords' content=''>
            //	<meta name='format-detection' content=''telephone=no'>
            //	<meta name='format-detection' content='email=no'>
            //	<meta name='format-detection' content='address=no'>
            //    <link rel='stylesheet' type='text/css' href='global_black.css' media='all'>
            //</head>";

            //            const string headerContentLight = @"<html>
            //<head>
            //	<meta charset='UTF-8'>
            //	<meta name='MobileOptimized' content='320'>
            //	<meta name='viewport' content='width=device-width,initial-scale=1.0,minimum-scale=1.0,maximum-scale=1.0,user-scalable=no'>
            //	<meta name='keywords' content=''>
            //	<meta name='format-detection' content=''telephone=no'>
            //	<meta name='format-detection' content='email=no'>
            //	<meta name='format-detection' content='address=no'>
            //    <link rel='stylesheet' type='text/css' href='global_light.css' media='all'>
            //</head>";


            //            content = headerContentLight + content;
            //            this.GameDetailWebBrowser.Background = new SolidColorBrush(Colors.White);

            //            Debug.WriteLine(content);

            //            const string path = "Data/html/game_detail.html";
            //            var bytes = Encoding.UTF8.GetBytes(content);

            //            using(IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            //            using (var output = iso.CreateFile(path))
            //            {
            //                output.Write(bytes, 0, bytes.Length);
            //            }

            //            this.GameDetailWebBrowser.Navigate(new Uri(path, UriKind.Relative));

            #endregion
        }

        //private void GameDetailWebBrowser_OnLoadCompleted(object sender, NavigationEventArgs e)
        //{
        //    LoadingPanel.Visibility = Visibility.Collapsed;
        //    this.GameDetailWebBrowser.Visibility = Visibility.Visible;

        //    //MessageBox.Show("页面加载完成");
        //}
        
        /// <summary>
        /// 当点击玩家头像的时候，跳转到该玩家的具体信息页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerImage_OnTap(object sender, GestureEventArgs e)
        {
            var image = sender as Image;
            if (image != null)
            {
                var name = image.Tag.ToString();
                Debug.WriteLine("team member name: " + name);
                e.Handled = true;

                NavigationService.Navigate(new Uri("/PlayerDetailPage.xaml?sn=" + App.ViewModel.SelectedDetailGameServer + "&pn=" + name, UriKind.Relative));
            }
        }

        /// <summary>
        /// 当点击其他地方时候，显示玩家的具体游戏内数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TeamMember_OnTap(object sender, GestureEventArgs e)
        {
            var grid = sender as Grid;
            if (grid != null)
            {
                var animationHelper = new AnimatonHelper();

                if (Math.Abs(grid.Height - 190) < 0.1)
                {
                    //hide
                    animationHelper.RunShowStoryboard(grid, AnimationTypes.TeamMemberDetailInfoHide, TimeSpan.FromSeconds(0), null);
                }
                else
                {
                    animationHelper.RunShowStoryboard(grid, AnimationTypes.SwivelForwardIn, TimeSpan.FromSeconds(0), null);
                    //show
                    animationHelper.RunShowStoryboard(grid, AnimationTypes.TeamMemberDetailInfoShow, TimeSpan.FromSeconds(0), null);
                }
            }
        }
    }
}