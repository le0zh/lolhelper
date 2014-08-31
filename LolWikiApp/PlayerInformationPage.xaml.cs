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
        private Player selectedPlayer;
        private bool isToBind = false;

        public PlayerInformationPage()
        {
            InitializeComponent();

            ServerRepository serverRepository = ServerRepository.instance;

            this.ServerListPicker.ItemsSource = serverRepository.GetServerInfos();

            this.ServerListPicker.SelectionChanged += ServerListPicker_SelectionChanged;

            SetApplicationBarTOSearch();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string mode;
            if (NavigationContext.QueryString.TryGetValue("mode", out mode))
            {
                if (mode == "bind")
                    isToBind = true;
            }

            PivotItem1.Header = isToBind ? "绑定召唤师信息" : "搜索召唤师";

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
            string content;

            HttpActionResult actionResult = await App.ViewModel.GetPlayerDetailInfo(serverName, userName);
           
            SearchLoadingBar.Visibility = Visibility.Collapsed;
            if (searchButton != null) searchButton.IsEnabled = true;

            if (actionResult.Result == ActionResult.Exception404)
            {
                MessageBox.Show("网络不稳定");
            }
            else if (actionResult.Result == ActionResult.NotFound)
            {
                NotFoundTextBlock.Visibility = Visibility.Visible;
            }
            else if (actionResult.Result == ActionResult.Success)
            {
                selectedPlayer = actionResult.Value as Player;

                if (selectedPlayer == null)
                {
                    NotFoundTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                selectedPlayer.ServerInfo = serverInfo;
                PlayerInfoViewer.Visibility = Visibility.Visible;
                this.PlayerInfoViewer.DataContext = selectedPlayer;

                if (isToBind)
                {
                    SetApplicationBarTOAcceptOrCancel();
                }
            }
        }

        private void SetApplicationBarTOSearch()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 1.0;
            ApplicationBarIconButton searchButton = new ApplicationBarIconButton();

            searchButton.IconUri = new Uri("/Assets/AppBar/feature.search.png", UriKind.Relative);
            searchButton.Text = "搜索";

            searchButton.Click += SearchBarIconButton_OnClick;

            ApplicationBar.Buttons.Add(searchButton);
        }

        private void SetApplicationBarTOAcceptOrCancel()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 1.0;
            ApplicationBarIconButton acceptButton = new ApplicationBarIconButton();
            ApplicationBarIconButton cancelButton = new ApplicationBarIconButton();

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
            App.ViewModel.BindedPlayer = selectedPlayer;
            App.ViewModel.SavePlayerInfoToAppSettings(selectedPlayer);

            //返回上一页面
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.PlayerInfoViewer.Visibility = Visibility.Collapsed;
            SetApplicationBarTOSearch();
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            App.ViewModel.SelectedPlayer = selectedPlayer;
            NavigationService.Navigate(new Uri("/PlayerDetailPage.xaml", UriKind.Relative));
        }
    }
}
