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
using ShakeGestures;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace LolWikiApp
{
    public partial class ShakeAShakePage : PhoneApplicationPage
    {
        public ShakeAShakePage()
        {
            InitializeComponent();

            ShakeGesturesHelper.Instance.ShakeGesture += InstanceOnShakeGesture;
            ShakeGesturesHelper.Instance.MinimumRequiredMovesForShake = 2;
            ShakeGesturesHelper.Instance.Active = true;
        }


        private void InstanceOnShakeGesture(object sender, ShakeGestureEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                if (App.ViewModel.BindedPlayer != null)
                {
                    GetCurrentGameInfo();
                }
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.ViewModel.BindedPlayer == null)
            {
                this.BindPlayerPanel.Visibility = Visibility.Visible;
                this.PlayerNameTextBlock.Visibility = Visibility.Collapsed;

            }
            else
            {
                this.BindPlayerPanel.Visibility = Visibility.Collapsed;
                this.PlayerNameTextBlock.Text =
                    string.Format("摇一摇 ({0}  {1})", App.ViewModel.BindedPlayer.Name, App.ViewModel.BindedPlayer.ServerInfo.DisplayName);
                this.PlayerNameTextBlock.Visibility = Visibility.Visible;
            }

            base.OnNavigatedTo(e);
        }

        private CurrentGameInfo mockingData()
        {
            CurrentGameInfo gameInfo = new CurrentGameInfo();

            for (int i = 0; i < 5; i++)
            {
                PlayerInfo p = new PlayerInfo();
                p.Name = "浪潮之巅";
                p.HeroName = "Aatrox";
                p.Total = "120";
                p.WinRate = "51%";
                p.TierDesc = "无平级";
                gameInfo.Sort100PlayerInfos.Add(p);
            }

            for (int i = 0; i < 5; i++)
            {
                PlayerInfo p = new PlayerInfo();
                p.Name = "浪潮之巅";
                p.HeroName = "Annie";
                p.Total = "120";
                p.WinRate = "52%";
                p.TierDesc = "无平级";
                gameInfo.Sort200PlayerInfos.Add(p);
            }

            gameInfo.QueueTypeCn = "匹配赛";
            return gameInfo;
        }

        private async void GetCurrentGameInfo()
        {
            //show loading
            RetrayShakeTextBlock.Visibility = Visibility.Collapsed;
            
            LoadingPanel.Visibility = Visibility.Visible;
            CurrentGameInfo gameInfo;

            try
            {
                gameInfo = await App.ViewModel.GetCurentGameInfo();

                //TEST, MOCKING DATA
                //gameInfo = mockingData();
            }
            catch (System.Net.Http.HttpRequestException exception404)
            {
                RetrayShakeTextBlock.Visibility = Visibility.Visible;
                return;
            }
            finally
            {
                //hide loading                
                LoadingPanel.Visibility = Visibility.Collapsed;
            }

            if (gameInfo == null)
            {
                this.NoGamingPanel.Visibility = Visibility.Visible;
            }
            else
            {
                this.ContentPanel.DataContext = gameInfo;
                this.TipTextBlock.Visibility = Visibility.Collapsed;
                this.NoGamingPanel.Visibility = Visibility.Collapsed;
                this.ContentPanel.Visibility = Visibility.Visible;
            }
        }

        private void BindPlayerButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/PlayerInformationPage.xaml?mode=bind", UriKind.Relative));
        }

        private void BlueGroupListSelector_OnTap(object sender, GestureEventArgs e)
        {
            if (this.BlueGroupListSelector.SelectedItem != null)
            {
                PlayerInfo p = this.BlueGroupListSelector.SelectedItem as PlayerInfo;
                navigateToPlayerDetailInfo(p);
            }
        }

        private void PurpleListSelector_OnTap(object sender, GestureEventArgs e)
        {
            if (this.PurpleListSelector.SelectedItem != null)
            {
                PlayerInfo p = this.PurpleListSelector.SelectedItem as PlayerInfo;
                navigateToPlayerDetailInfo(p);
            }
        }

        private void navigateToPlayerDetailInfo(PlayerInfo p)
        {
            if (p == null)
                return;

            string pn = p.Name;
            string sn = App.ViewModel.BindedPlayer.ServerInfo.Value;

            NavigationService.Navigate(new Uri("/PlayerDetailPage.xaml?sn=" + sn + "&pn=" + pn, UriKind.Relative));
        }
    }
}