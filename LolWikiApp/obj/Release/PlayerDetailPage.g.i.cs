﻿#pragma checksum "C:\Users\leozhu\Source\Repos\lolhelper\LolWikiApp\PlayerDetailPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "E656E5D19BFE6655338015E858B59ED5"
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


namespace LolWikiApp {
    
    
    public partial class PlayerDetailPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LoadingGrid;
        
        internal Microsoft.Phone.Controls.Pivot LayoutPivot;
        
        internal Microsoft.Phone.Controls.Pivot LayoutRoot;
        
        internal Microsoft.Phone.Controls.LongListSelector RecentGameLongListSelector;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/LolWikiApp;component/PlayerDetailPage.xaml", System.UriKind.Relative));
            this.LoadingGrid = ((System.Windows.Controls.Grid)(this.FindName("LoadingGrid")));
            this.LayoutPivot = ((Microsoft.Phone.Controls.Pivot)(this.FindName("LayoutPivot")));
            this.LayoutRoot = ((Microsoft.Phone.Controls.Pivot)(this.FindName("LayoutRoot")));
            this.RecentGameLongListSelector = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("RecentGameLongListSelector")));
        }
    }
}

