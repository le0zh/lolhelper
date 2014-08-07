using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
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

namespace LolWikiApp
{
    public partial class NewsDetailPage : PhoneApplicationPage
    {
        private NewsDetail _newsDetail;
        private string _articleId;
        private readonly Popup _popUp;

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

            NewsLoadingBar.Visibility = Visibility.Visible;
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
                        ContentWebBrowser.NavigateToString(content);
                    }
                }
            }
            catch (System.Net.Http.HttpRequestException exception404)
            {
                Debug.WriteLine("NewsDetailPage: " + exception404.Message);
                RetryNetPanel.Visibility = Visibility.Visible;
                return;
            }
            finally
            {
                NewsLoadingBar.Visibility = Visibility.Collapsed;
            }


            RetryNetPanel.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string artId;
            if (NavigationContext.QueryString.TryGetValue("newsId", out artId))
            {
                _articleId = artId;
                LoadNewsDetailAsync(artId);
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
        }

        private int _scrollHeight = 0;

        void ContentWebBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            // split 
            var parts = e.Value.Split('=');
            if (parts.Length != 2)
            {
                return;
            }

            // parse
            int number = 0;
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
                //Debug.WriteLine("{0}/{1},{2}", DisplayScrollBar.Value, DisplayScrollBar.Maximum, DisplayScrollBar.Maximum - DisplayScrollBar.Value);
            }
        }

        private void NewsDetailPage_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (_popUp.IsOpen)
            {
                //HideImagePopUp();

                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }
    }
}