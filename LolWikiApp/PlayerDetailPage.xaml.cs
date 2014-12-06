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
        private Player _currentPlayer;

       

        public PlayerDetailPage()
        {
            InitializeComponent();
        }

        private string _sn;
        private string _pn;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(_isPostBack) return;

            _isPostBack = true;

            if(DataContext!=null)
                return;            
            
            if (NavigationContext.QueryString.TryGetValue("sn", out _sn) &&
                NavigationContext.QueryString.TryGetValue("pn", out _pn))
            {
                LoadAndBindPlayerInfo(_sn, _pn);
            }
            else
            {
                if (App.ViewModel.SelectedPlayer != null)
                {
                    _currentPlayer = App.ViewModel.SelectedPlayer;
                    _sn = _currentPlayer.ServerInfo.Value;
                    _pn = _currentPlayer.Name;
                    DataContext = _currentPlayer;
                    LayoutPivot.Visibility = Visibility.Visible;
                    SetBindAppBar();
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
                        _currentPlayer = detailPlayerInfo;
                        DataContext = _currentPlayer;
                        LoadingGrid.Visibility = Visibility.Collapsed;
                        LayoutPivot.Visibility = Visibility.Visible;
                        SetBindAppBar();
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

        private void SetBindAppBar()
        {
            ApplicationBar = new ApplicationBar { Opacity = 1, Mode = ApplicationBarMode.Minimized};

            if (_currentPlayer.IsBinded == false)
            {
                var pinButton = new ApplicationBarIconButton
                {
                    IconUri = new Uri("/Data/appbar.add.png", UriKind.Relative),
                    Text = "加关注"
                };

                pinButton.Click += (s, e) =>
                {
                    App.ViewModel.AddBindedPlayer(_currentPlayer);

                    var confirmQuiToastPromt = ToastPromts.GetToastWithImgAndTitle("添加关注成功!");
                    confirmQuiToastPromt.Show();    
                    LoadAndBindPlayerInfo(_sn, _pn);
                };
                ApplicationBar.Buttons.Add(pinButton);
            }
            
            var refreshButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative),
                Text = "刷新"
            };

            refreshButton.Click += (s, e) => LoadAndBindPlayerInfo(_sn, _pn);

            ApplicationBar.Buttons.Add(refreshButton);
        }
    }
}