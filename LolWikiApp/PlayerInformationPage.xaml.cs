using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Windows.System.Threading.Core;
using HtmlAgilityPack;
using LolWikiApp.Repository;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Input;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using Keyboard = System.Windows.Input.Keyboard;

namespace LolWikiApp
{
    public partial class PlayerInformationPage : PhoneApplicationPage
    {
        private Player _selectedPlayer;
        private bool _isToBind;

        public PlayerInformationPage()
        {
            InitializeComponent();

            ServerRepository serverRepository = ServerRepository.instance;

            this.ServerListPicker.ItemsSource = serverRepository.GetServerInfos();

            this.ServerListPicker.SelectionChanged += ServerListPicker_SelectionChanged;

            SetApplicationBarToSearch();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string mode;
            if (NavigationContext.QueryString.TryGetValue("mode", out mode))
            {
                if (mode == "bind")
                    _isToBind = true;
            }

            TitleTextBlock.Text = _isToBind ? "绑定召唤师信息" : "搜索召唤师";

            base.OnNavigatedTo(e);
        }

        private void ServerListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show( ((ServerInfo)this.ServerListPicker.SelectedItem).Value);
        }

        private async void SearchBarIconButton_OnClick(object sender, EventArgs e)
        {
            this.Focus();
            var userName = this.PlayerNameTextBox.Text;
            var serverInfo = (ServerInfo)this.ServerListPicker.SelectedItem;

            if (string.IsNullOrEmpty(userName) || serverInfo == null)
            {
                ToastPromts.GetToastWithImgAndTitle("请输入召唤师和服务器名称!").Show();
                return;
            }

            var serverName = serverInfo.Value;
            var searchButton = this.ApplicationBar.Buttons[0] as ApplicationBarIconButton;

            NotFoundTextBlock.Visibility = Visibility.Collapsed;
            SearchLoadingBar.Visibility = Visibility.Visible;

            if (searchButton != null) searchButton.IsEnabled = false;
       
            var actionResult = await App.ViewModel.GetPlayerDetailInfo(serverName, userName);
           
            SearchLoadingBar.Visibility = Visibility.Collapsed;
            if (searchButton != null) searchButton.IsEnabled = true;

            switch (actionResult.Result)
            {
                case ActionResult.Exception404:
                    ToastPromts.GetToastWithImgAndTitle("貌似网络不稳定，稍后重试.").Show();
                    break;
                case ActionResult.NotFound:
                    NotFoundTextBlock.Visibility = Visibility.Visible;
                    break;
                case ActionResult.Success:
                    _selectedPlayer = actionResult.Value as Player;
                    if (_selectedPlayer == null)
                    {
                        NotFoundTextBlock.Visibility = Visibility.Visible;
                        return;
                    }
                    _selectedPlayer.ServerInfo = serverInfo;
                    PlayerInfoViewer.Visibility = Visibility.Visible;
                    this.PlayerInfoViewer.DataContext = _selectedPlayer;
                    if (_isToBind)
                    {
                        SetApplicationBarToAcceptOrCancel();
                    }
                    break;
            }
        }

        private void SetApplicationBarToSearch()
        {
            ApplicationBar = new ApplicationBar {Opacity = 1.0};
            var searchButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/feature.search.png", UriKind.Relative),
                Text = "搜索"
            };

            searchButton.Click += SearchBarIconButton_OnClick;

            ApplicationBar.Buttons.Add(searchButton);
        }

        private void SetApplicationBarToAcceptOrCancel()
        {
            ApplicationBar = new ApplicationBar {Opacity = 1.0};
            var acceptButton = new ApplicationBarIconButton();
            var cancelButton = new ApplicationBarIconButton();

            acceptButton.IconUri = new Uri("/Assets/AppBar/check.png", UriKind.Relative);
            cancelButton.IconUri = new Uri("/Assets/AppBar/cancel.png", UriKind.Relative);

            acceptButton.Text = "加关注";
            cancelButton.Text = "取消";

            acceptButton.Click += acceptButton_Click;
            cancelButton.Click += cancelButton_Click;

            ApplicationBar.Buttons.Add(acceptButton);
            ApplicationBar.Buttons.Add(cancelButton);
        }

        private void acceptButton_Click(object sender, EventArgs e)
        {
            //保存关注
            App.ViewModel.BindedPlayer = _selectedPlayer;
            App.ViewModel.SavePlayerInfoToAppSettings(_selectedPlayer);

            //返回上一页面
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.PlayerInfoViewer.Visibility = Visibility.Collapsed;
            SetApplicationBarToSearch();
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            App.ViewModel.SelectedPlayer = _selectedPlayer;
            NavigationService.Navigate(new Uri("/PlayerDetailPage.xaml", UriKind.Relative));
        }
    }
}
