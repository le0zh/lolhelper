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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.ApplicationModel.Core;
using LolWikiApp.Repository;
using LolWikiApp.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Xml.XPath;
using Microsoft.Xna.Framework.Media;
using XAPADStatistics;

namespace LolWikiApp
{
    public partial class NewsDetailPage : PhoneApplicationPage
    {
        private NewsDetail _newsDetail;
        private string _articleId;
        private readonly Popup _popUp;
        private string _artId;
        private string _artUrl;//for tecent news
        private string _fullUrl;
        private bool _isNavigated;

        private bool _isNeedToModify = true;
        private AdItem _adItem = null;

        public NewsDetailPage()
        {
            InitializeComponent();

            _popUp = new Popup();
            _adItem = new AdItem { ADKey = "64294ac6f3f1b5b2", AppID = "10000655", Size = SizeMode.SizeW480H80 };
            AdPopup.Child = _adItem;
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

                        var tmpFilePath = await App.NewsViewModel.NewsRepository.SaveHtmlToTempIsoFile(_newsDetail);
                        ContentWebBrowser.Navigate(new Uri(tmpFilePath, UriKind.Relative));

                        //TODO:在WP8.0中，直接NavigateToString会出现乱码问题，暂时通过先生成临时html文件，在Navigate的方式解决
                        //var content = App.NewsViewModel.NewsRepository.RenderNewsHtmlContent(_newsDetail);
                        //DataContext = _newsDetail;
                        //Debug.WriteLine(content);
                        //ContentWebBrowser.NavigateToString(content);

                        //ContentWebBrowser.Navigate(new Uri("http://www.baidu.com"));
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
            if (_isNavigated)
            {

            }
            else
            {
                _isNavigated = true;
                if (NavigationContext.QueryString.TryGetValue("newsId", out _artId))
                {
                    _articleId = _artId;
                    _isNeedToModify = false;
                    ContentWebBrowser.ScriptNotify += ContentWebBrowser_ScriptNotify;
                    ContentWebBrowser.LoadCompleted += ContentWebBrowser_LoadCompleted;

                    LoadNewsDetailAsync(_artId);
                }

                if (NavigationContext.QueryString.TryGetValue("newsUrl", out _artUrl))
                {
                    _isNeedToModify = true;
                    ContentWebBrowser.ScriptNotify += ContentWebBrowser_ScriptNotify;
                    ContentWebBrowser.LoadCompleted += ContentWebBrowser_LoadCompleted;
                    var fullUrl = "http://qt.qq.com/static/pages/news/phone/" + _artUrl;
                    LoadingIndicator.IsRunning = true;

                    ContentWebBrowser.Navigate(new Uri(fullUrl, UriKind.Absolute));

                    //ContentWebBrowser.NavigateToString("<!doctype html>" +
                    //                                   "<html><head><title>video test</title></head><body style=background-color:black;>video test" +
                    //                                   "<iframe height=\"100%\" frameborder=\"0\" allowfullscreen src=\"http://v.qq.com/iframe/player.html?vid=z0015abq8k6&amp;tiny=0&amp;auto=0\" width=\"100%\"></iframe>" +
                    //                                   "</body></html>");
                }

                if (NavigationContext.QueryString.TryGetValue("fullUrl", out _fullUrl))
                {
                    _isNeedToModify = false;
                    LoadingIndicator.IsRunning = true;
                    ContentWebBrowser.Navigate(new Uri(_fullUrl, UriKind.Absolute));
                }
            }

            base.OnNavigatedTo(e);
        }


        private void RetryButton_OnClick(object sender, RoutedEventArgs e)
        {
            LoadNewsDetailAsync(_articleId);
        }

        void ContentWebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            //DisplayScrollBar.Visibility = Visibility.Visible;
            try
            {
                ContentWebBrowser.InvokeScript("eval",
               @"
    var srcArray = new Array();
    var videoLink;
   
    window.onLinkPressed = function() {
        var elem = event.srcElement;
        if ( elem != null ) {
            elem.hideFocus=true;
            window.external.notify('img,' + elem.getAttribute('link')+','+srcArray.join(','));
        }
        return false;
    }

    window.onVideoPressed = function() {
        var elem = event.srcElement;
        if ( elem != null ) {
            elem.hideFocus=true;
            window.external.notify('video,' + videoLink);
        }
        return false;
    }

   window.ChangeStyle = function(){
        var h1Element = document.getElementsByTagName('h1')[0];
        var authorElement =  document.getElementsByClassName('article_author')[0];
        var metaElement =  document.getElementsByClassName('article_meta')[0];
        
        if(h1Element){
            h1Element.setAttribute('style','background:#29282e;color:#fff');
        }
        if(authorElement){
             authorElement.setAttribute('style','background:#29282e;color:#656565;');
        }        
        if(metaElement){
             metaElement.setAttribute('style','background:#29282e;');
        }
   }

   window.ChangeLastSpan = function(){
        var spans = document.getElementsByTagName('span');
        if(spans){
            var lastSpan = spans[spans.length - 1];
            if(lastSpan.innerHTML.indexOf('掌上英雄联盟反馈QQ群') != -1){
                lastSpan.innerHTML = '英雄联盟助手WP版反馈QQ群 49573963';
            }       
        }
   }   

    var srcArray = new Array();

    window.InitTcVideo = function(){
        var iframeObj = document.getElementsByTagName('iframe')[0];
        if(iframeObj){
             var link = iframeObj.getAttribute('src');
             var contentDiv = document.getElementsByClassName('article_content')[0];
             if(contentDiv){
                iframeObj.setAttribute('src','');
                iframeObj.setAttribute('style', 'display: none;');
                var videoDiv =  document.createElement('div');
                videoLink = link;   
                videoDiv.attachEvent('onmouseup', onVideoPressed);
                videoDiv.setAttribute('style','background:red;margin-top:12px;');
                videoDiv.innerHTML = '<div > <img src=\''+ iframeObj.getAttribute('_img') + '\'  /></div><div style=\'position : absolute;display:block;top:30%;width:40px;margin:0 auto; left:0px;right:0px;z-index:100\'><img src=\'http://ossweb-img.qq.com/images/qqtalk/act/lol_app_bg/playIcon.png\' /> </div>';
                contentDiv.appendChild(videoDiv);
             }           
        }
    }

    window.BindLinks = function() {
        var elems = document.getElementsByTagName('img');
        for (var i = 0; i < elems.length; i++) {
            var elem = elems[i];
            var link = elem.getAttribute('src');
            if(link.indexOf('hand_lol')>0){
            }
            else{
                srcArray[i] = link;
                elem.setAttribute('link', i);
                if(link.indexOf('.gif')>0){
                    elem.parentNode.removeChild(elem);
                }else{
                    elem.attachEvent('onmouseup', onLinkPressed);
                }
            }           
        }
    }");
                ContentWebBrowser.InvokeScript("BindLinks");

                if (_isNeedToModify)
                {
                    ContentWebBrowser.InvokeScript("ChangeStyle");
                    ContentWebBrowser.InvokeScript("ChangeLastSpan");
                    ContentWebBrowser.InvokeScript("InitTcVideo");                    
                }

                Debug.WriteLine(ContentWebBrowser.SaveToString());

                ContentWebBrowser.Visibility = Visibility.Visible;
                LoadingIndicator.IsRunning = false;
                //ShowAdPopup();
            }
            catch (Exception)
            {
                throw;
            }
            Debug.WriteLine("新闻页面加载完成");
        }

        //private int _scrollHeight = 0;

        private int _totalImage;

        private void HorizontalFlipView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ImageTextBlock.Text = string.Format("{0}/{1}", HorizontalFlipView.SelectedIndex + 1, _totalImage);
        }

        //TODO: MULTI-IMAGE-VIEW
        private void ShowImagePopUp(int currentIndex, List<string> srcList)
        {
            HideAdPopup();
            _totalImage = srcList.Count;

            HorizontalFlipView.ItemsSource = srcList;
            HorizontalFlipView.SelectedIndex = currentIndex;

            BitmapImage bitmap;
            var input = srcList[currentIndex];
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

            //PanZoom.Source = bitmap;

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

                CurrentImage.Source = bitmap;
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
            var parts = e.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            try
            {
                if (parts[0] == "img")
                {
                    var currentIndex = Convert.ToInt32(parts[1]);
                    ShowImagePopUp(currentIndex, parts.Skip(2).Take(parts.Count - 1).ToList());
                }
                else
                {
                    var src = parts[1];
                    if (!string.IsNullOrEmpty(src))
                    {
                        var wbt = new WebBrowserTask { Uri = new Uri(src) };
                        wbt.Show();
                    }

                }
            }
            catch (Exception)
            {
                Debug.WriteLine("ContentWebBrowser_ScriptNotify error");
            }

            //// split 
            //var parts = e.Value.Split('=');
            //if (parts.Length != 2)
            //{
            //    return;
            //}

            //// parse
            //var number = 0;
            //if (!int.TryParse(parts[1], out number))
            //{
            //    return;
            //}

            //// decide what to do
            //if (parts[0] == "scrollHeight")
            //{
            //    _scrollHeight = number;

            //    // IE、Opera 认为 scrollHeight 是网页内容实际高度
            //    DisplayScrollBar.Maximum = _scrollHeight - 789;
            //}
            //else if (parts[0] == "scrollTop")
            //{
            //    // 确定ScrollBar位置.
            //    DisplayScrollBar.Value = number;
            //}
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void NewsDetailPage_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (BigImageWindow.IsOpen)
            {
                HideImagePopUp();
                e.Cancel = true;
            }
            else
            {
                HideAdPopup();
                //if (string.IsNullOrEmpty(_fullUrl))
                //{
                //    NavigationService.RemoveBackEntry();
                //    NavigationService.Navigate(new Uri("/HomePage.xaml?cacheId=NEWS", UriKind.Relative));
                //    e.Cancel = true;
                //}
            }

            base.OnBackKeyPress(e);
        }

        private void HideImagePopUp()
        {
            if (BigImageWindow.IsOpen)
            {
                BigImageWindow.IsOpen = false;
                ApplicationBar = null;
                //ShowAdPopup();
            }
        }

        private void ShowAdPopup()
        {
            if (AdPopup.IsOpen)
                return;

            AdBorder.Visibility = Visibility.Visible;
            _adItem.start();

            _adItem.ShowAd();
            _adItem.ADClosed += (s, e) =>
            {
                AdBorder.Visibility = Visibility.Collapsed;
            };
            AdPopup.IsOpen = true;
        }

        private void HideAdPopup()
        {
            AdBorder.Visibility = Visibility.Collapsed;
            _adItem.stop();
            _adItem.HideAd();
            AdPopup.IsOpen = false;
        }
    }
}