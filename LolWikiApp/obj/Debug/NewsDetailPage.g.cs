﻿#pragma checksum "D:\Repos\lolhelper\LolWikiApp\NewsDetailPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "00FDD5A1F1AEF2187688426A4F4DEA04"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;
using Telerik.Windows.Controls;


namespace LolWikiApp {
    
    
    public partial class NewsDetailPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.StackPanel RetryNetPanel;
        
        internal System.Windows.Controls.Button RetryButton;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal Microsoft.Phone.Controls.WebBrowser ContentWebBrowser;
        
        internal System.Windows.Controls.Border AdBorder;
        
        internal Telerik.Windows.Controls.RadWindow BigImageWindow;
        
        internal Microsoft.Phone.Controls.FlipView HorizontalFlipView;
        
        internal System.Windows.Controls.Image CurrentImage;
        
        internal System.Windows.Controls.TextBlock ImageTextBlock;
        
        internal Telerik.Windows.Controls.RadBusyIndicator LoadingIndicator;
        
        internal System.Windows.Controls.Primitives.Popup AdPopup;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/LolWikiApp;component/NewsDetailPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.RetryNetPanel = ((System.Windows.Controls.StackPanel)(this.FindName("RetryNetPanel")));
            this.RetryButton = ((System.Windows.Controls.Button)(this.FindName("RetryButton")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.ContentWebBrowser = ((Microsoft.Phone.Controls.WebBrowser)(this.FindName("ContentWebBrowser")));
            this.AdBorder = ((System.Windows.Controls.Border)(this.FindName("AdBorder")));
            this.BigImageWindow = ((Telerik.Windows.Controls.RadWindow)(this.FindName("BigImageWindow")));
            this.HorizontalFlipView = ((Microsoft.Phone.Controls.FlipView)(this.FindName("HorizontalFlipView")));
            this.CurrentImage = ((System.Windows.Controls.Image)(this.FindName("CurrentImage")));
            this.ImageTextBlock = ((System.Windows.Controls.TextBlock)(this.FindName("ImageTextBlock")));
            this.LoadingIndicator = ((Telerik.Windows.Controls.RadBusyIndicator)(this.FindName("LoadingIndicator")));
            this.AdPopup = ((System.Windows.Controls.Primitives.Popup)(this.FindName("AdPopup")));
        }
    }
}

