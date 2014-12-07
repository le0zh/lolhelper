using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private bool _isPostback;

        public ShakeAShakePage()
        {
            InitializeComponent();

            ShakeGesturesHelper.Instance.ShakeGesture += InstanceOnShakeGesture;
            ShakeGesturesHelper.Instance.MinimumRequiredMovesForShake = 2;
            ShakeGesturesHelper.Instance.Active = true;
        }

        //backup: 备份切换动画
        //private bool isBackAnimated;
        //protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        //{
        //    if (!isBackAnimated)
        //    {
        //        var animatonHelper = new AnimatonHelper();
        //        animatonHelper.RunShowStoryboard(LayoutPivot, AnimationTypes.SlideRightOutFade, TimeSpan.FromSeconds(0), (s, o) =>
        //        {
        //            isBackAnimated = true;
        //            NavigationService.GoBack();
        //        });
        //        e.Cancel = true;
        //    }
        //}

        private void InstanceOnShakeGesture(object sender, ShakeGestureEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                if (PlayerPickBox.SelectedIndex >= 0)
                {
                    GetCurrentGameInfo();
                }
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_isPostback) return;

            if (App.ViewModel.BindedPlayerInfoWrappers == null || App.ViewModel.BindedPlayerInfoWrappers.Count == 0)
            {
                BindPlayerPanel.Visibility = Visibility.Visible;
                PlayerPickBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                BindPlayerPanel.Visibility = Visibility.Collapsed;

                PlayerPickBox.ItemsSource = App.ViewModel.BindedPlayerInfoWrappers;
                PlayerPickBox.SelectedIndex = 0;
                PlayerPickBox.Visibility = Visibility.Visible;
            }

            _isPostback = true;

            base.OnNavigatedTo(e);
        }

        //private CurrentGameInfo mockingData()
        //{
        //    CurrentGameInfo gameInfo = new CurrentGameInfo();

        //    for (int i = 0; i < 5; i++)
        //    {
        //        PlayerInfo p = new PlayerInfo();
        //        p.Name = "浪潮之巅";
        //        p.HeroName = "Aatrox";
        //        p.Total = "120";
        //        p.WinRate = "51%";
        //        p.TierDesc = "无平级";
        //        gameInfo.Sort100PlayerInfos.Add(p);
        //    }

        //    for (int i = 0; i < 5; i++)
        //    {
        //        PlayerInfo p = new PlayerInfo();
        //        p.Name = "浪潮之巅";
        //        p.HeroName = "Annie";
        //        p.Total = "120";
        //        p.WinRate = "52%";
        //        p.TierDesc = "无平级";
        //        gameInfo.Sort200PlayerInfos.Add(p);
        //    }

        //    gameInfo.QueueTypeCn = "匹配赛";
        //    return gameInfo;
        //}

        private async void GetCurrentGameInfo()
        {
            //show loading
            RetrayShakeTextBlock.Visibility = Visibility.Collapsed;
            NoGamingPanel.Visibility = Visibility.Collapsed;
            LoadingPanel.Visibility = Visibility.Visible;
            CurrentGameInfo gameInfo = null;

            try
            {
                var playerInfo = PlayerPickBox.SelectedItem as PlayerInfoSettingWrapper;
                if (playerInfo != null)
                {
                    gameInfo = await App.ViewModel.GetCurentGameInfo(playerInfo);
                }

                //TEST, MOCKING DATA
                //gameInfo = mockingData();
            }
            catch (Exception exception404)
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
                var p = this.BlueGroupListSelector.SelectedItem as PlayerInfo;
                navigateToPlayerDetailInfo(p);
            }
        }

        private void PurpleListSelector_OnTap(object sender, GestureEventArgs e)
        {
            if (this.PurpleListSelector.SelectedItem != null)
            {
                var p = this.PurpleListSelector.SelectedItem as PlayerInfo;
                navigateToPlayerDetailInfo(p);
            }
        }

        private void navigateToPlayerDetailInfo(PlayerInfo p)
        {
            //if (p == null)
            //    return;

            //string pn = p.Name;
            //string sn = App.ViewModel.BindedPlayer.ServerInfo.Value;

            //NavigationService.Navigate(new Uri("/PlayerDetailPage.xaml?sn=" + sn + "&pn=" + pn, UriKind.Relative));
        }

    }
}