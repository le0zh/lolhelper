﻿#pragma checksum "C:\Users\leozhu\Source\Repos\lolhelper\LolWikiApp\LetvVideoPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "19E3135809FCA6E8FD382CC6998262C5"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34011
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using LolWikiApp;
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


namespace LolWikiApp {
    
    
    public partial class LetvVideoPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal Microsoft.Phone.Controls.Pivot MainPivot;
        
        internal System.Windows.Controls.StackPanel LateastLoadingBar;
        
        internal LolWikiApp.HttpRequest404Control LateastRetryNetPanel;
        
        internal LolWikiApp.RefreshableListBox LatestVideoLongListSelector;
        
        internal System.Windows.Controls.StackPanel JieshuoLoadingBar;
        
        internal LolWikiApp.HttpRequest404Control JieshuoRetryNetPanel;
        
        internal Microsoft.Phone.Controls.WrapPanel JieshuoWrapPanel;
        
        internal System.Windows.Controls.StackPanel FunnyLoadingBar;
        
        internal LolWikiApp.HttpRequest404Control FunnyRetryNetPanel;
        
        internal Microsoft.Phone.Controls.WrapPanel FunnyWrapPanel;
        
        internal System.Windows.Controls.StackPanel ZhongheLoadingBar;
        
        internal LolWikiApp.HttpRequest404Control ZhongheRetryNetPanel;
        
        internal Microsoft.Phone.Controls.WrapPanel ZhongheWrapPanel;
        
        internal System.Windows.Controls.StackPanel MatchLoadingBar;
        
        internal LolWikiApp.HttpRequest404Control MatchRetryNetPanel;
        
        internal Microsoft.Phone.Controls.WrapPanel MatchWrapPanel;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.StackPanel NoCachedVideoPanel;
        
        internal System.Windows.Controls.StackPanel CachedVideoLoadingBar;
        
        internal System.Windows.Controls.ListBox TransferListBox;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/LolWikiApp;component/LetvVideoPage.xaml", System.UriKind.Relative));
            this.MainPivot = ((Microsoft.Phone.Controls.Pivot)(this.FindName("MainPivot")));
            this.LateastLoadingBar = ((System.Windows.Controls.StackPanel)(this.FindName("LateastLoadingBar")));
            this.LateastRetryNetPanel = ((LolWikiApp.HttpRequest404Control)(this.FindName("LateastRetryNetPanel")));
            this.LatestVideoLongListSelector = ((LolWikiApp.RefreshableListBox)(this.FindName("LatestVideoLongListSelector")));
            this.JieshuoLoadingBar = ((System.Windows.Controls.StackPanel)(this.FindName("JieshuoLoadingBar")));
            this.JieshuoRetryNetPanel = ((LolWikiApp.HttpRequest404Control)(this.FindName("JieshuoRetryNetPanel")));
            this.JieshuoWrapPanel = ((Microsoft.Phone.Controls.WrapPanel)(this.FindName("JieshuoWrapPanel")));
            this.FunnyLoadingBar = ((System.Windows.Controls.StackPanel)(this.FindName("FunnyLoadingBar")));
            this.FunnyRetryNetPanel = ((LolWikiApp.HttpRequest404Control)(this.FindName("FunnyRetryNetPanel")));
            this.FunnyWrapPanel = ((Microsoft.Phone.Controls.WrapPanel)(this.FindName("FunnyWrapPanel")));
            this.ZhongheLoadingBar = ((System.Windows.Controls.StackPanel)(this.FindName("ZhongheLoadingBar")));
            this.ZhongheRetryNetPanel = ((LolWikiApp.HttpRequest404Control)(this.FindName("ZhongheRetryNetPanel")));
            this.ZhongheWrapPanel = ((Microsoft.Phone.Controls.WrapPanel)(this.FindName("ZhongheWrapPanel")));
            this.MatchLoadingBar = ((System.Windows.Controls.StackPanel)(this.FindName("MatchLoadingBar")));
            this.MatchRetryNetPanel = ((LolWikiApp.HttpRequest404Control)(this.FindName("MatchRetryNetPanel")));
            this.MatchWrapPanel = ((Microsoft.Phone.Controls.WrapPanel)(this.FindName("MatchWrapPanel")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.NoCachedVideoPanel = ((System.Windows.Controls.StackPanel)(this.FindName("NoCachedVideoPanel")));
            this.CachedVideoLoadingBar = ((System.Windows.Controls.StackPanel)(this.FindName("CachedVideoLoadingBar")));
            this.TransferListBox = ((System.Windows.Controls.ListBox)(this.FindName("TransferListBox")));
        }
    }
}

