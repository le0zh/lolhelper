using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace LolWikiApp
{
    public partial class NewsVideoPage : PhoneApplicationPage
    {
        public NewsVideoPage()
        {
            InitializeComponent();
            //ContentWebBrowser.LoadCompleted += ContentWebBrowser_LoadCompleted;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ContentWebBrowser.Navigate(new Uri("http://v.qq.com/iframe/player.html?vid=w01403d5jvi&tiny=1&auto=0", UriKind.Absolute));
            base.OnNavigatedTo(e);
        }

        void ContentWebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            //DisplayScrollBar.Visibility = Visibility.Visible;
            try
            {
                ContentWebBrowser.InvokeScript("eval",
               @"
 
    function makeFullScreen(divObj) {
           //Use the specification method before using prefixed versions
          if (divObj.requestFullscreen) {
            divObj.requestFullscreen();
          }
          else if (divObj.msRequestFullscreen) {
            divObj.msRequestFullscreen();
          }
          else if (divObj.mozRequestFullScreen) {
            divObj.mozRequestFullScreen();
          }
          else if (divObj.webkitRequestFullscreen) {
            divObj.webkitRequestFullscreen();
          } else {
          } 
    }

   window.AllowFullScreen = function(){
        var iframe1 = document.getElementsByClassName('edui-faked-video')[0];
        if(iframe1){
            makeFullScreen(iframe1);
        }       
   }");
                ContentWebBrowser.InvokeScript("AllowFullScreen");
            }
            catch (Exception)
            {
                throw;
            }
            Debug.WriteLine("新闻页面加载完成");
        }
    }
}