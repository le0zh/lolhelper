﻿using System;
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
        private bool _isNeedReplaceLastSpan = true;

        private AdItem _adItem = null;

        public NewsDetailPage()
        {
            InitializeComponent();

            _popUp = new Popup();
            _adItem = new AdItem { ADKey = "64294ac6f3f1b5b2", AppID = "10000655", Size = SizeMode.SizeW480H80 };
            AdPopup.Child = _adItem;

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
            if (NavigationContext.QueryString.TryGetValue("newsId", out _artId))
            {
                _articleId = _artId;
                LoadNewsDetailAsync(_artId);
            }

            if (NavigationContext.QueryString.TryGetValue("newsUrl", out _artUrl))
            {
                var fullUrl = "http://qt.qq.com/static/pages/news/phone/" + _artUrl;
                LoadingIndicator.IsRunning = true;
                ContentWebBrowser.Navigate(new Uri(fullUrl, UriKind.Absolute));
            }
            
            if (NavigationContext.QueryString.TryGetValue("fullUrl", out _fullUrl))
            {
                _isNeedReplaceLastSpan = false;
                LoadingIndicator.IsRunning = true;
                ContentWebBrowser.Navigate(new Uri(_fullUrl, UriKind.Absolute));
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

    window.onLinkPressed = function() {
        var elem = event.srcElement;
        if ( elem != null ) {
            elem.hideFocus=true;
            window.external.notify(elem.getAttribute('link')+','+srcArray.join(','));
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
            lastSpan.innerHTML = '英雄联盟助手WP版反馈QQ群 49573963';
        }
   }

   

   var srcArray = new Array();

    window.BindLinks = function() {
        var elems = document.getElementsByTagName('img');
        for (var i = 0; i < elems.length; i++) {
            var elem = elems[i];
            var link = elem.getAttribute('src');
            srcArray[i] = link;
            elem.setAttribute('link', i);
            if(link.indexOf('.gif')>0){
                elem.parentNode.removeChild(elem);
            }else{
                elem.attachEvent('onmouseup', onLinkPressed);
            }
        }
    }");
                ContentWebBrowser.InvokeScript("BindLinks");
                ContentWebBrowser.InvokeScript("ChangeStyle");
                if (_isNeedReplaceLastSpan)
                {
                    ContentWebBrowser.InvokeScript("ChangeLastSpan");
                }
                ContentWebBrowser.Visibility = Visibility.Visible;
                LoadingIndicator.IsRunning = false;
                ShowAdPopup();
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
            ImageTextBlock.Text = string.Format("{0}/{1}", HorizontalFlipView.SelectedIndex+1, _totalImage);
        }
        
        //TODO: MULTI-IMAGE-VIEW
        private void ShowImagePopUp(int currentIndex, List<string> srcList)
        {
            HideAdPopup();
            _totalImage = srcList.Count;

            HorizontalFlipView.ItemsSource = srcList;
            HorizontalFlipView.SelectedIndex = currentIndex;

            BigImageWindow.VerticalAlignment = VerticalAlignment.Center;
            BigImageWindow.IsOpen = true;

            ApplicationBar = new ApplicationBar { Opacity = 0.8 };

            var downloadButton = new ApplicationBarIconButton();
            var closeButton = new ApplicationBarIconButton();

            downloadButton.IconUri = new Uri("/Assets/AppBar/save.png", UriKind.Relative);
            downloadButton.Text = "保存";

            closeButton.IconUri = new Uri("/Assets/AppBar/close.png", UriKind.Relative);
            closeButton.Text = "关闭";

            downloadButton.Click += (s2, e2) =>
            {
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
            var parts = e.Value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();

            try
            {
                var currentIndex = Convert.ToInt32(parts[0]);
                ShowImagePopUp(currentIndex,parts.Skip(1).Take(parts.Count-1).ToList());
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
            HideAdPopup();
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

            ShowAdPopup();
        }

        private void ShowAdPopup()
        {
            AdBorder.Visibility = Visibility.Visible;
            _adItem.start();
            _adItem.ShowAd();
            _adItem.ADClosed+= (s, e) =>
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