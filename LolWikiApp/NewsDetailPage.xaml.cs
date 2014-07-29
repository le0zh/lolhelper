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
        }

        private async void LoadNewsDetailAsync(string artId)
        {
            RetryNetPanel.Visibility = Visibility.Collapsed;

            NewsLoadingBar.Visibility = Visibility.Visible;
            try
            {
                _newsDetail = await App.NewsViewModel.GetNewsDetailAsync(artId);
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

            DataContext = _newsDetail;
            RetryNetPanel.Visibility = Visibility.Collapsed;

            RenderNewsContent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DataContext == null)
            {
                string artId;
                if (NavigationContext.QueryString.TryGetValue("newsId", out artId))
                {
                    _articleId = artId;
                    LoadNewsDetailAsync(artId);
                }
            }

            base.OnNavigatedTo(e);
        }

        private void RetryButton_OnClick(object sender, RoutedEventArgs e)
        {
            LoadNewsDetailAsync(_articleId);
        }

        private async void RenderNewsContent()
        {
            if (_newsDetail == null)
                return;

            //newsDetail.Content = newsDetail.Content.Replace("width=550",)

            const string htmlTemplate = @"
<html>
<head>
<meta charset='UTF-8'>
<title>$title$</title>
<meta http-equiv='X-UA-Compatible' content='IE=edge' />
<meta name='viewport' content='width=device-width, initial-scale=1.0, user-scalable=no, minimum-scale=1.0, maximum-scale=1.0'>
<style>
/* Reset */
html,body,div,span,object,iframe,h1,h2,h3,h4,h5,h6,p,blockquote,pre,a,abbr,acronym,address,code,del,dfn,em,img,q,dl,dt,dd,ol,ul,li,fieldset,form,label,legend,table,caption,tbody,tfoot,thead,tr,th,td{border:0;font-weight:inherit;font-style:inherit;font-size:100%;font-family:inherit;vertical-align:baseline;margin:0;padding:0;}
table{border-collapse:separate;border-spacing:0;margin-bottom:1.4em;}
caption,th,td{text-align:left;font-weight:400;}
blockquote:before,blockquote:after,q:before,q:after{content:'';}
blockquote,q{quotes:;}
a img{border:none;}

/* Layout */
@-webkit-viewport{width:device-width}
@-moz-viewport{width:device-width}
@-ms-viewport{width:device-width}
@-o-viewport{width:device-width}
@viewport{width:device-width}
img,video{ max-width: 100%; }
.container{width:90%;margin-left:auto;margin-right:auto;}
.container div{ font-size: 1.3em;}
body{
	font-size: 100%;
	line-height: 1.5;	
}

h1{
	font-size: 2.0em;
	text-align: left;
	
}

span.info{
    color: #555555;
}

p{
    font-size: 1.3em;    
	margin-bottom: 0.5em;
}

</style>

<script lang='javascript'> 

    var isNotify = false;
    function initialize() { 
        if(document.body.clientHeight){
            window.external.notify('clientHeight=' + document.body.clientHeight.toString());
            isNotify = true;
        }
        if(document.body.scrollHeight){
            window.external.notify('scrollHeight=' + document.body.scrollHeight.toString()); 
        }
      window.onscroll = onScroll; 
    }
     
    function onScroll(e) {
        if (isNotify == false) {
            window.external.notify('clientHeight=' + document.body.clientHeight.toString());
            isNotify = true;
        }
        var top = (document.documentElement && document.documentElement.scrollTop) ||  document.body.scrollTop;
        window.external.notify('scrollTop=' + top.toString()); 
    }

    window.onload = initialize;
</script>
</head>

<body>

 <div class='container'>
        <h1>$title$</h1>
        <span class='info'>发表时间：$postTime$   来源： $site$</span>     
        <hr />   
        $content$
 </div>
 
</body>
</html>";

            #region old-tempalte

            //            const string htmlTemplate = @"<html>
//<head>
//<meta name='viewport' content='width=device-width, initial-scale=1.0, user-scalable=no, minimum-scale=1.0, maximum-scale=1.0' />
//
//<script>
//if (navigator.userAgent.match(/IEMobile\/10\.0/)) {
//    var msViewportStyle = document.createElement('style');
//    msViewportStyle.appendChild(
//        document.createTextNode(
//            '@-ms-viewport{width:auto!important}'
//        )
//    );
//    document.getElementsByTagName('head')[0].
//        appendChild(msViewportStyle);
//}
//</script>
//
//<style>
//@-ms-viewport{width:device-width}
//</style>
//<title>$title$</title>
//</head>
//<body>
//<h3>$title$</h3>
//<span>更新时间：$postTime$  来源：$site$</span>
//<hr/>
//$content$
//<body/>
            //</html>";
            #endregion

            

            var content = htmlTemplate.Replace("$title$", _newsDetail.Title).Replace("$postTime$", _newsDetail.Posttime).Replace("$site$", _newsDetail.Site).Replace("$content$", _newsDetail.Content.Replace("<div","<p").Replace("</div", "</p"));


            Debug.WriteLine(content);

            ContentWebBrowser.ScriptNotify += ContentWebBrowser_ScriptNotify;

            ContentWebBrowser.NavigateToString(content);
            ContentWebBrowser.LoadCompleted += ContentWebBrowser_LoadCompleted;
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