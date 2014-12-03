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
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace LolWikiApp
{
    public partial class PlayerDetailPage : PhoneApplicationPage
    {
        private bool _isPostBack;

        public PlayerDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(_isPostBack) return;

            _isPostBack = true;

            if(DataContext!=null)
                return;            

            string sn;
            string pn;

            if (NavigationContext.QueryString.TryGetValue("sn", out sn) &&
                NavigationContext.QueryString.TryGetValue("pn", out pn))
            {
                LoadAndBindPlayerInfo(sn, pn);
            }
            else
            {
                if (App.ViewModel.SelectedPlayer != null)
                {
                    DataContext = App.ViewModel.SelectedPlayer;
                    LayoutPivot.Visibility = Visibility.Visible;
                }
                else
                {
                    //TODO:处理异常
                }
            }


            base.OnNavigatedTo(e);
        }

        private async void LoadAndBindPlayerInfo(string sn, string pn)
        {
            LoadingGrid.Visibility = Visibility.Visible;
            LayoutPivot.Visibility = Visibility.Collapsed;

            var actionResult = await App.ViewModel.GetPlayerDetailInfo(sn, pn);

            switch (actionResult.Result)
            {
                case ActionResult.Exception404:
                    MessageBox.Show("网络连接不稳定。");
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                    break;
                case ActionResult.NotFound:
                    MessageBox.Show("无该召唤师信息。");
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                    break;

                case ActionResult.Success:
                    var detailPlayerInfo = actionResult.Value as Player;
                    if (detailPlayerInfo == null)
                    {
                        MessageBox.Show("无该召唤师信息。");
                        if (NavigationService.CanGoBack)
                            NavigationService.GoBack();
                    }
                    else
                    {
                        DataContext = detailPlayerInfo;
                        LoadingGrid.Visibility = Visibility.Collapsed;
                        LayoutPivot.Visibility = Visibility.Visible;
                    }
                    break;
            }
        }

        private void RecentGameLongListSelector_OnTap(object sender, GestureEventArgs e)
        {
            if (this.RecentGameLongListSelector.SelectedItem != null)
            {
                var game = this.RecentGameLongListSelector.SelectedItem as GameInfo;
                if (game != null)
                {
                    RecentGameLongListSelector.SelectedItem = null;
                    App.ViewModel.SelectedDetailGameInfoUrl = game.GameDetailUrl;
                    NavigationService.Navigate(new Uri("/GameDetailPage.xaml", UriKind.Relative));
                }
                else
                {
                    Debug.WriteLine("game info is null");
                }
            }
            else
            {
                Debug.WriteLine("selected item is null");
            }
        }
    }
}