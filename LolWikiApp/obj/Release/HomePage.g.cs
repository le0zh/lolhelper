﻿#pragma checksum "D:\Repos\lolhelper\LolWikiApp\HomePage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "CAE8E8D3B352E275E52CC25010C6A9B9"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
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
    
    
    public partial class HomePage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal Microsoft.Phone.Controls.Pivot MainPivot;
        
        internal System.Windows.Controls.StackPanel NewsLoadingBar;
        
        internal LolWikiApp.HttpRequest404Control NewsRetryNetPanel;
        
        internal System.Windows.Controls.Grid CategoryGrid;
        
        internal System.Windows.Controls.Grid CategoryBackGroundGrid;
        
        internal System.Windows.Controls.TextBlock CategoryTextBlock;
        
        internal LolWikiApp.RefreshableListBox NewsLongListSelector;
        
        internal System.Windows.Controls.StackPanel FreeHeroLoadingBar;
        
        internal LolWikiApp.HttpRequest404Control FreeHeroRetryNetPanel;
        
        internal Microsoft.Phone.Controls.WrapPanel HeroWrapPanel;
        
        internal System.Windows.Controls.StackPanel MyDataLoadingBar;
        
        internal System.Windows.Controls.StackPanel NeedBindPlayerInfoPanel;
        
        internal LolWikiApp.HttpRequest404Control MyInfomationRetryNetPanel;
        
        internal Microsoft.Phone.Controls.LongListSelector PlayerRecordPanel;
        
        internal System.Windows.Controls.Button VideoButton;
        
        internal System.Windows.Controls.Button ShakeItButton;
        
        internal System.Windows.Controls.Button SearchRecordButton;
        
        internal System.Windows.Controls.Button RateButton;
        
        internal System.Windows.Controls.Button HuangliButton;
        
        internal System.Windows.Controls.Button AllHeroButton;
        
        internal System.Windows.Controls.Button AllItemButton;
        
        internal System.Windows.Controls.Primitives.Popup SlideMenuPopup;
        
        internal System.Windows.Controls.Grid PopGrid;
        
        internal System.Windows.Controls.Grid MenuListStackPanel;
        
        internal System.Windows.Controls.ListBox NewsCategoryListBox;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/LolWikiApp;component/HomePage.xaml", System.UriKind.Relative));
            this.MainPivot = ((Microsoft.Phone.Controls.Pivot)(this.FindName("MainPivot")));
            this.NewsLoadingBar = ((System.Windows.Controls.StackPanel)(this.FindName("NewsLoadingBar")));
            this.NewsRetryNetPanel = ((LolWikiApp.HttpRequest404Control)(this.FindName("NewsRetryNetPanel")));
            this.CategoryGrid = ((System.Windows.Controls.Grid)(this.FindName("CategoryGrid")));
            this.CategoryBackGroundGrid = ((System.Windows.Controls.Grid)(this.FindName("CategoryBackGroundGrid")));
            this.CategoryTextBlock = ((System.Windows.Controls.TextBlock)(this.FindName("CategoryTextBlock")));
            this.NewsLongListSelector = ((LolWikiApp.RefreshableListBox)(this.FindName("NewsLongListSelector")));
            this.FreeHeroLoadingBar = ((System.Windows.Controls.StackPanel)(this.FindName("FreeHeroLoadingBar")));
            this.FreeHeroRetryNetPanel = ((LolWikiApp.HttpRequest404Control)(this.FindName("FreeHeroRetryNetPanel")));
            this.HeroWrapPanel = ((Microsoft.Phone.Controls.WrapPanel)(this.FindName("HeroWrapPanel")));
            this.MyDataLoadingBar = ((System.Windows.Controls.StackPanel)(this.FindName("MyDataLoadingBar")));
            this.NeedBindPlayerInfoPanel = ((System.Windows.Controls.StackPanel)(this.FindName("NeedBindPlayerInfoPanel")));
            this.MyInfomationRetryNetPanel = ((LolWikiApp.HttpRequest404Control)(this.FindName("MyInfomationRetryNetPanel")));
            this.PlayerRecordPanel = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("PlayerRecordPanel")));
            this.VideoButton = ((System.Windows.Controls.Button)(this.FindName("VideoButton")));
            this.ShakeItButton = ((System.Windows.Controls.Button)(this.FindName("ShakeItButton")));
            this.SearchRecordButton = ((System.Windows.Controls.Button)(this.FindName("SearchRecordButton")));
            this.RateButton = ((System.Windows.Controls.Button)(this.FindName("RateButton")));
            this.HuangliButton = ((System.Windows.Controls.Button)(this.FindName("HuangliButton")));
            this.AllHeroButton = ((System.Windows.Controls.Button)(this.FindName("AllHeroButton")));
            this.AllItemButton = ((System.Windows.Controls.Button)(this.FindName("AllItemButton")));
            this.SlideMenuPopup = ((System.Windows.Controls.Primitives.Popup)(this.FindName("SlideMenuPopup")));
            this.PopGrid = ((System.Windows.Controls.Grid)(this.FindName("PopGrid")));
            this.MenuListStackPanel = ((System.Windows.Controls.Grid)(this.FindName("MenuListStackPanel")));
            this.NewsCategoryListBox = ((System.Windows.Controls.ListBox)(this.FindName("NewsCategoryListBox")));
        }
    }
}

