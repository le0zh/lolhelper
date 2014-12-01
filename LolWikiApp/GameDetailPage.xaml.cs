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
                Debug.WriteLine("moreInfoUrl:" + detailUrl);

                var reg = new Regex("shareKey=\\w+(?=&)");

                if (reg.Match(detailUrl).Success)
                {
                    var shareKey = reg.Match(detailUrl).Value;
                    var newDetailUrl = "http://zdl.mbox.duowan.com/phone/matchDetail.php?" + shareKey;
                    //this.GameDetailWebBrowser.Navigate(new Uri(detailUrl, UriKind.Absolute));

                    LoadAndShowInWebBrowser(newDetailUrl);
                }
              
            }

            base.OnNavigatedTo(e);
        }

        private async void LoadAndShowInWebBrowser(string url)
        {            
            LoadingPanel.Visibility = Visibility.Visible;
            Debug.WriteLine(url);
            var content = await GetHtmlContentAsync(url);
            if (content.IndexOf("<body>", StringComparison.Ordinal) != -1)
            {
                content = content.Substring(content.IndexOf("<body>", StringComparison.Ordinal));
            }
            else
            {
                content = "<body><h4>对不起，暂时无法查看比赛详细记录。</h4></body>";
            }

            const string headerContentBlack = @"<html>
<head>
	<meta charset='UTF-8'>
	<meta name='MobileOptimized' content='320'>
	<meta name='viewport' content='width=device-width,initial-scale=1.0,minimum-scale=1.0,maximum-scale=1.0,user-scalable=no'>
	<meta name='keywords' content=''>
	<meta name='format-detection' content=''telephone=no'>
	<meta name='format-detection' content='email=no'>
	<meta name='format-detection' content='address=no'>
    <link rel='stylesheet' type='text/css' href='global_black.css' media='all'>
</head>";

            const string headerContentLight = @"<html>
<head>
	<meta charset='UTF-8'>
	<meta name='MobileOptimized' content='320'>
	<meta name='viewport' content='width=device-width,initial-scale=1.0,minimum-scale=1.0,maximum-scale=1.0,user-scalable=no'>
	<meta name='keywords' content=''>
	<meta name='format-detection' content=''telephone=no'>
	<meta name='format-detection' content='email=no'>
	<meta name='format-detection' content='address=no'>
    <link rel='stylesheet' type='text/css' href='global_light.css' media='all'>
</head>";
           

            content = headerContentLight + content;
            this.GameDetailWebBrowser.Background = new SolidColorBrush(Colors.White);
            
            Debug.WriteLine(content);
           
            const string path = "Data/html/game_detail.html";
            var bytes = Encoding.UTF8.GetBytes(content);

            using(IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            using (var output = iso.CreateFile(path))
            {
                output.Write(bytes, 0, bytes.Length);
            }

            this.GameDetailWebBrowser.Navigate(new Uri(path, UriKind.Relative));
        }

        private void GameDetailWebBrowser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
            this.GameDetailWebBrowser.Visibility = Visibility.Visible;
            
            //MessageBox.Show("页面加载完成");
        }
    }
}