﻿#pragma checksum "D:\Repos\lolhelper\LolWikiApp\NewsCachePage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "76AB8FEE6450821AF259E01D11C8C87A"
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
    
    
    public partial class NewsCachePage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.DataTemplate ToggleHeaderTemplate;
        
        internal System.Windows.DataTemplate ToggleContentTemplate;
        
        internal Microsoft.Phone.Controls.Pivot LayoutPivot;
        
        internal System.Windows.Controls.StackPanel ListReadingTipStackPanel;
        
        internal System.Windows.Controls.TextBlock InfoTextBlock;
        
        internal System.Windows.Controls.StackPanel ContentReadingTipStackPanel;
        
        internal System.Windows.Controls.TextBlock InfoTextBlock2;
        
        internal System.Windows.Controls.ProgressBar CachingProgressBar;
        
        internal System.Windows.Controls.Button StartButton;
        
        internal Telerik.Windows.Controls.RadBusyIndicator CachedSizeLoadingIndicator;
        
        internal System.Windows.Controls.StackPanel DeleteCacheStackPanel;
        
        internal System.Windows.Controls.TextBlock CachedSizeTextBlock;
        
        internal System.Windows.Controls.Button DeleteButton;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/LolWikiApp;component/NewsCachePage.xaml", System.UriKind.Relative));
            this.ToggleHeaderTemplate = ((System.Windows.DataTemplate)(this.FindName("ToggleHeaderTemplate")));
            this.ToggleContentTemplate = ((System.Windows.DataTemplate)(this.FindName("ToggleContentTemplate")));
            this.LayoutPivot = ((Microsoft.Phone.Controls.Pivot)(this.FindName("LayoutPivot")));
            this.ListReadingTipStackPanel = ((System.Windows.Controls.StackPanel)(this.FindName("ListReadingTipStackPanel")));
            this.InfoTextBlock = ((System.Windows.Controls.TextBlock)(this.FindName("InfoTextBlock")));
            this.ContentReadingTipStackPanel = ((System.Windows.Controls.StackPanel)(this.FindName("ContentReadingTipStackPanel")));
            this.InfoTextBlock2 = ((System.Windows.Controls.TextBlock)(this.FindName("InfoTextBlock2")));
            this.CachingProgressBar = ((System.Windows.Controls.ProgressBar)(this.FindName("CachingProgressBar")));
            this.StartButton = ((System.Windows.Controls.Button)(this.FindName("StartButton")));
            this.CachedSizeLoadingIndicator = ((Telerik.Windows.Controls.RadBusyIndicator)(this.FindName("CachedSizeLoadingIndicator")));
            this.DeleteCacheStackPanel = ((System.Windows.Controls.StackPanel)(this.FindName("DeleteCacheStackPanel")));
            this.CachedSizeTextBlock = ((System.Windows.Controls.TextBlock)(this.FindName("CachedSizeTextBlock")));
            this.DeleteButton = ((System.Windows.Controls.Button)(this.FindName("DeleteButton")));
        }
    }
}

