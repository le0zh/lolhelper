using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using LolWikiApp.Repository;
using LolWikiApp.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Xml.XPath;
using Microsoft.Xna.Framework.Media;

namespace LolWikiApp
{
    public partial class NewsDetailPage : PhoneApplicationPage
    {
        private NewsDetail _newsDetail;
        private string _articleId;
        private readonly Popup _popUp;
        private string _artId;

        public NewsDetailPage()
        {
            InitializeComponent();

            _popUp = new Popup();

            ContentWebBrowser.ScriptNotify += ContentWebBrowser_ScriptNotify;
            ContentWebBrowser.LoadCompleted += ContentWebBrowser_LoadCompleted;
        }

        private async void LoadNewsDetailAsync(string artId)
        {
            RetryNetPanel.Visibility = Visibility.Collapsed;

            //NewsLoadingBar.Visibility = Visibility.Visible;
            LoadingIndicator.IsRunning = true;

            try
            {

                var isCached = await App.NewsViewModel.FileRepository.CheckNewsIsCachedOrNot(artId);
                if (isCached)
                {
                    var cachedPath = App.NewsViewModel.FileRepository.GetNewsCachePath(artId);
                    Debug.WriteLine("##News detail, load form CACHE:" + cachedPath);
                    ContentWebBrowser.Navigate(new Uri(cachedPath, UriKind.Relative));
                }
                else
                {
                    Debug.WriteLine("##not cached");
                    if (DataContext == null)
                    {
                        _newsDetail = await App.NewsViewModel.GetNewsDetailAsync(artId);
                        var content = App.NewsViewModel.NewsRepository.RenderNewsHtmlContent(_newsDetail);
                        DataContext = _newsDetail;
                        Debug.WriteLine(content);
                        ContentWebBrowser.NavigateToString(content);
                    }
                }
            }
            catch (Exception exception404)
            {
                Debug.WriteLine("NewsDetailPage: " + exception404.Message);
                RetryNetPanel.Visibility = Visibility.Visible;
                return;
            }
            finally
            {
                //NewsLoadingBar.Visibility = Visibility.Collapsed;
                LoadingIndicator.IsRunning = false;
            }


            RetryNetPanel.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (NavigationContext.QueryString.TryGetValue("newsId", out _artId))
            {
                _articleId = _artId;
                LoadNewsDetailAsync(_artId);
            }

            base.OnNavigatedTo(e);
        }

        private void RetryButton_OnClick(object sender, RoutedEventArgs e)
        {
            LoadNewsDetailAsync(_articleId);
        }

        void ContentWebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            ContentWebBrowser.Visibility = Visibility.Visible;
            DisplayScrollBar.Visibility = Visibility.Visible;

            try
            {
                ContentWebBrowser.InvokeScript("eval",
                    @"
    window.onLinkPressed = function() {
        var elem = event.srcElement;
        if ( elem != null ) {
            window.external.notify(elem.getAttribute('link'));
        }
        return false;
    }
    window.BindLinks = function() {
        var elems = document.getElementsByTagName('img');
        for (var i = 0; i < elems.length; i++) {
            var elem = elems[i];
            var link = elem.getAttribute('src');
            elem.setAttribute('link', link);
            if(link.indexOf('.gif')>0){
                elem.parentNode.removeChild(elem);
            }else{
                elem.attachEvent('onmouseup', onLinkPressed);
            }
        }
    }");
                ContentWebBrowser.InvokeScript("BindLinks");
            }
            catch (Exception)
            {
                throw;
            }
            Debug.WriteLine("新闻页面加载完成");
        }

        private int _scrollHeight = 0;


        private void ShowImagePopUp(string input)
        {
            BitmapImage bitmap;
            Debug.WriteLine("------->" + input);
            if (Regex.IsMatch(input, @"http://[^\[^>]*?(gif|jpg|png|jpeg|bmp|bmp)"))
            {
                bitmap = new BitmapImage(new Uri(input, UriKind.Absolute));
            }
            else
            {
               
                bitmap = new BitmapImage();
                App.NewsViewModel.FileRepository.SetBitmapSource(input, bitmap, _artId);
            }
           
            PanZoom.Source = bitmap;

            BigImageWindow.VerticalAlignment = VerticalAlignment.Center;
            BigImageWindow.IsOpen = true;

            ApplicationBar = new ApplicationBar { Opacity = 1 };

            var downloadButton = new ApplicationBarIconButton();
            var closeButton = new ApplicationBarIconButton();

            downloadButton.IconUri = new Uri("/Assets/AppBar/save.png", UriKind.Relative);
            downloadButton.Text = "保存";

            closeButton.IconUri = new Uri("/Assets/AppBar/close.png", UriKind.Relative);
            closeButton.Text = "关闭";

            downloadButton.Click += (s2, e2) =>
            {
                var isSuccess = HelperRepository.SaveImage(DateTime.Now.ToFileTime().ToString(), bitmap);

                if (isSuccess)
                {
                    ToastPromts.GetToastWithImgAndTitle("图片保存成功").Show();
                }
                else
                {
                    ToastPromts.GetToastWithImgAndTitle("图片保存失败").Show();
                }

            };

            closeButton.Click += (s3, e3) => HideImagePopUp();

            ApplicationBar.Buttons.Add(downloadButton);
            ApplicationBar.Buttons.Add(closeButton);
        }


        void ContentWebBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            if (Regex.IsMatch(e.Value, @"[^\[^>]*?(gif|jpg|png|jpeg|bmp|bmp)"))
            {
                ShowImagePopUp(e.Value);
                return;
            }

            // split 
            var parts = e.Value.Split('=');
            if (parts.Length != 2)
            {
                return;
            }

            // parse
            var number = 0;
            if (!int.TryParse(parts[1], out number))
            {
                return;
            }

            // decide what to do
            if (parts[0] == "scrollHeight")
            {
                _scrollHeight = number;

                // IE、Opera 认为 scrollHeight 是网页内容实际高度
                DisplayScrollBar.Maximum = _scrollHeight - 789;
            }
            else if (parts[0] == "scrollTop")
            {
                // 确定ScrollBar位置.
                DisplayScrollBar.Value = number;
            }
        }

        private void NewsDetailPage_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (BigImageWindow.IsOpen)
            {
                HideImagePopUp();

                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        private void HideImagePopUp()
        {
            if (BigImageWindow.IsOpen)
            {
                BigImageWindow.IsOpen = false;
                ApplicationBar = null;
            }
        }
    }
}