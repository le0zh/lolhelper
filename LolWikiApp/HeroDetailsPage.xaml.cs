using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using HtmlAgilityPack;
using LolWikiApp.Repository;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Shell;
using LolWikiApp.Resources;
using Microsoft.Phone.Tasks;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace LolWikiApp
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        private HeroDetail hero;
        private readonly ObservableCollection<EquipmentRecommend> equipmentRecommends;
        private readonly ObservableCollection<HeroSkin> heroSkins;
        private readonly ObservableCollection<HeroRankWrapper> heroRankList;
        private readonly Popup popUp;
        private bool isPivotFirstLoaded;

        // Constructor
        public DetailsPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
            equipmentRecommends = new ObservableCollection<EquipmentRecommend>();
            heroSkins = new ObservableCollection<HeroSkin>();
            heroRankList = new ObservableCollection<HeroRankWrapper>();
            popUp = new Popup();
        }

        private async void DetailPageMain()
        {
            if (hero == null)
            {
                string selectedId = "";
                if (NavigationContext.QueryString.TryGetValue("selectedId", out selectedId))
                {
                    hero = await App.ViewModel.GetHeroDetailByKeyAsync(selectedId);
                    hero.SetLevel(1);
                    DataContext = hero;

                    EquipmentLongListSelector.ItemsSource = equipmentRecommends;
                    SkinLongListSelector.ItemsSource = heroSkins;
                    HeroRanListBox.ItemsSource = heroRankList;
                }
            }

            switch (HeroDetailMainPivot.SelectedIndex)
            {
                case 0: //技能
                    break;
                case 1: //出装
                    LoadEquipList();
                    break;
                case 2: //数据
                    break;
                case 3: //排行
                    LoadRankList();
                    break;
                case 4: //背景
                    break;
                case 5: //皮肤
                    LoadSkinList();
                    break;
            }
        }

        private void ShowRetryPanelAppBar(Action retryAction)
        {
            ApplicationBar = new ApplicationBar {Opacity = 1.0};
            var refreshButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative),
                Text = "刷新"
            };

            refreshButton.Click += (s, e) => retryAction();
            //refreshButton.Click += (s, e) =>  NavigationService.Navigate(new Uri("/NewsListPage.xaml", UriKind.Relative));

            ApplicationBar.Buttons.Add(refreshButton);
        }

        private async void LoadEquipList()
        {
            if (equipmentRecommends.Count != 0)
            {
                return;
            }

            try
            {
                ApplicationBar = null;
                EquipmentLoadingBar.Visibility = Visibility.Visible;
                Equipment404TextBlock.Visibility = Visibility.Collapsed;

                List<EquipmentRecommend> list = await App.ViewModel.LoadEquipmentRecommendList(hero.Name);
                foreach (EquipmentRecommend recommend in list)
                {
                    equipmentRecommends.Add(recommend);
                }

                EquipmentLongListSelector.Visibility = Visibility.Visible;
            }
            catch (System.Net.Http.HttpRequestException exception404)
            {
                Equipment404TextBlock.Visibility = Visibility.Visible;

                ShowRetryPanelAppBar(LoadEquipList);
            }
            finally
            {
                EquipmentLoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadSkinList()
        {
            if (heroSkins.Count != 0)
            {
                return;
            }

            try
            {
                ApplicationBar = null;
                SkinLoadingBar.Visibility = Visibility.Visible;
                Skin404TextBlock.Visibility = Visibility.Collapsed;

                List<HeroSkin> list = await App.ViewModel.LoadHeroSkinListAsync(hero.Name);
                foreach (HeroSkin skin in list)
                {
                    heroSkins.Add(skin);
                }

                SkinLongListSelector.Visibility = Visibility.Visible;
            }
            catch (System.Net.Http.HttpRequestException exception404)
            {
                Skin404TextBlock.Visibility = Visibility.Visible;
                ShowRetryPanelAppBar(LoadSkinList);
            }
            finally
            {
                SkinLoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void LoadRankList()
        {
            if (heroRankList.Count != 0)
            {
                return;
            }

            try
            {
                ApplicationBar = null;
                TopPlayerLoadingBar.Visibility = Visibility.Visible;
                rank404TextBlock.Visibility = Visibility.Collapsed;

                HttpClient client = new HttpClient();
                string content =
                    await client.GetStringAsync(new Uri("http://lolbox.duowan.com/phone/heroTop10Players_ios.php?hero=" + hero.Name));

                foreach (HeroRankWrapper wrapper in loadHeroRankData(content))
                {
                    heroRankList.Add(wrapper);
                }

                HeroRanListBox.Visibility = Visibility.Visible;
            }
            catch (System.Net.Http.HttpRequestException exception404)
            {
                rank404TextBlock.Visibility = Visibility.Visible;
                ShowRetryPanelAppBar(LoadRankList);
            }
            finally
            {
                TopPlayerLoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private void HeroDetailMainPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isPivotFirstLoaded == false && HeroDetailMainPivot.SelectedIndex == 0)
            {
                isPivotFirstLoaded = true;
            }
            else
            {
                DetailPageMain();
            }
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DetailPageMain();
        }

        private List<HeroRankWrapper> loadHeroRankData(string content)
        {
            List<HeroRankWrapper> list = new List<HeroRankWrapper>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);

            HtmlNodeCollection liNodeCollection = doc.DocumentNode.SelectNodes("/html[1]/body[1]/section[1]/div[1]/ul[1]/li");

            if (liNodeCollection == null)
            {
                Debug.WriteLine("liNodeCollection is null");
                return list;
            }

            foreach (HtmlNode node in liNodeCollection.Skip(1))
            {
                HtmlNodeCollection spanNodes = node.SelectNodes("span");
                HeroRankWrapper wrapper = new HeroRankWrapper()
                {
                    ServerName = spanNodes[0].InnerText,
                    PlayerName = spanNodes[1].InnerText,
                    RankInfo = spanNodes[2].InnerText
                };
                list.Add(wrapper);
            }

            return list;
        }

        #region  Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
        #endregion

        private void HeroLevelSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.HeroLevelTextBlock != null)
            {
                int level = Convert.ToInt32(e.NewValue);
                this.HeroLevelTextBlock.Text = level.ToString(CultureInfo.InvariantCulture);
                if (hero != null)
                {
                    hero.SetLevel(level);
                }
            }
        }

        private void EquipmentLongListSelector_OnTap(object sender, GestureEventArgs e)
        {
            if (EquipmentLongListSelector.SelectedItem != null)
            {
                App.ViewModel.EquipmentRecommendSelected = EquipmentLongListSelector.SelectedItem as EquipmentRecommend;
                NavigationService.Navigate(new Uri("/EquipmentRecommendDetailPage.xaml", UriKind.Relative));
            }
        }

        private async void EquipmentLongListSelector_OnRefreshTriggered(object sender, EventArgs e)
        {
            List<EquipmentRecommend> list = await App.ViewModel.LoadEquipmentRecommendList(hero.Name);
            equipmentRecommends.Clear();
            foreach (EquipmentRecommend recommend in list)
            {
                equipmentRecommends.Add(recommend);
            }
            this.EquipmentLongListSelector.HideRefreshPanel();
        }

        private void HideImagePopUp()
        {
            if (popUp.IsOpen)
            {
                popUp.IsOpen = false;
                ApplicationBar = null;
            }
        }

        private void DetailsPage_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (popUp.IsOpen)
            {
                HideImagePopUp();

                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        private async void SkinImage_OnTap(object sender, GestureEventArgs e)
        {
            var img = sender as Image;
            if (img == null)
                return;

            var bitmap = img.Source as BitmapImage;

            var htmlContent = await App.ViewModel.GetImageHtmlTemplate();
            htmlContent = htmlContent.Replace("$src$", img.Tag.ToString())
                                     .Replace("$w2$", (bitmap.PixelWidth / 2).ToString())
                                     .Replace("$h2$", (bitmap.PixelHeight / 2).ToString())
                                     .Replace("$w$", bitmap.PixelWidth.ToString())
                                     .Replace("$h$", bitmap.DecodePixelHeight.ToString());

            var wb = new WebBrowser {Height = 800, Width = 480};
            wb.NavigateToString(htmlContent);

            wb.Background = Application.Current.GetTheme() == Theme.Dark ? new SolidColorBrush(Colors.Black)
                                                                         : new SolidColorBrush(Colors.White);
            popUp.Child = wb;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 0.8;
            var downloadButton = new ApplicationBarIconButton();
            var closeButton = new ApplicationBarIconButton();

            downloadButton.IconUri = new Uri("/Assets/AppBar/save.png", UriKind.Relative);
            downloadButton.Text = "保存";

            closeButton.IconUri = new Uri("/Assets/AppBar/close.png", UriKind.Relative);
            closeButton.Text = "关闭";

            downloadButton.Click += (s2, e2) =>
            {
                var isSuccess = HelperRepository.SaveImage(DateTime.Now.ToFileTime().ToString(), bitmap);
                if (isSuccess)
                {
                    ToastPromts.GetToastWithImgAndTitle("图片保存成功").Show();
                }
                else
                {
                    ToastPromts.GetToastWithImgAndTitle("图片保存失败").Show();
                }

            };

            closeButton.Click += (s3, e3) => HideImagePopUp();

            ApplicationBar.Buttons.Add(downloadButton);
            ApplicationBar.Buttons.Add(closeButton);

            wb.LoadCompleted += (s3, e3) =>
            {
                popUp.IsOpen = true;
            };
        }


        private void HeroRanListBox_OnTap(object sender, GestureEventArgs e)
        {
            if (HeroRanListBox.SelectedItem != null)
            {
                HeroRankWrapper heroRankWrapper = HeroRanListBox.SelectedItem as HeroRankWrapper;
                if (heroRankWrapper != null)
                {
                    string url = string.Format("/PlayerDetailPage.xaml?sn={0}&pn={1}", heroRankWrapper.ServerName, heroRankWrapper.PlayerName);
                    NavigationService.Navigate(new Uri(url, UriKind.Relative));
                }
            }
        }
    }
}