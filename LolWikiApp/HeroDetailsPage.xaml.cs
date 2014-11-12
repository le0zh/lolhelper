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
        private HeroDetail _hero;
        private readonly ObservableCollection<EquipmentRecommend> equipmentRecommends;
        private readonly ObservableCollection<HeroSkin> heroSkins;
        private readonly ObservableCollection<HeroRankWrapper> heroRankList;
        private readonly Popup _imagePopUp;
        private readonly FullScreenPopup _actionPopup;
        private bool _isPivotFirstLoaded;
        private int _currentLateastPage = 1;
        private bool _isPostBack;

        // Constructor
        public DetailsPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
            equipmentRecommends = new ObservableCollection<EquipmentRecommend>();
            heroSkins = new ObservableCollection<HeroSkin>();
            heroRankList = new ObservableCollection<HeroRankWrapper>();
            _imagePopUp = new Popup();
            _actionPopup = new FullScreenPopup();

            _letvHeroVideoListInfos = new ObservableCollection<LetvVideoListInfo>();
        }

        private async void DetailPageMain()
        {
            if (_hero == null)
            {
                string selectedId = "";
                if (NavigationContext.QueryString.TryGetValue("selectedId", out selectedId))
                {
                    _hero = await App.ViewModel.GetHeroDetailByKeyAsync(selectedId);
                    _hero.SetLevel(1);
                    DataContext = _hero;

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
                case 2: //视频
                    BindHeroVideoList();
                    break;
                case 3: //数据
                    break;
                case 4: //排行
                    LoadRankList();
                    break;
                case 5: //背景
                    break;
                case 6: //皮肤
                    LoadSkinList();
                    break;
            }
        }

        private void ShowRetryPanelAppBar(Action retryAction)
        {
            ApplicationBar = new ApplicationBar { Opacity = 1.0 };
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

                List<EquipmentRecommend> list = await App.ViewModel.LoadEquipmentRecommendList(_hero.Name);
                foreach (EquipmentRecommend recommend in list)
                {
                    equipmentRecommends.Add(recommend);
                }

                EquipmentLongListSelector.Visibility = Visibility.Visible;
            }
            catch (Exception exception404)
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

                List<HeroSkin> list = await App.ViewModel.LoadHeroSkinListAsync(_hero.Name);
                foreach (HeroSkin skin in list)
                {
                    heroSkins.Add(skin);
                }

                SkinLongListSelector.Visibility = Visibility.Visible;
            }
            catch (Exception exception404)
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
                    await client.GetStringAsync(new Uri("http://lolbox.duowan.com/phone/heroTop10Players_ios.php?hero=" + _hero.Name));

                foreach (HeroRankWrapper wrapper in LoadHeroRankData(content))
                {
                    heroRankList.Add(wrapper);
                }

                HeroRanListBox.Visibility = Visibility.Visible;
            }
            catch (Exception exception404)
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
            if (_isPivotFirstLoaded == false && HeroDetailMainPivot.SelectedIndex == 0)
            {
                _isPivotFirstLoaded = true;
            }
            else
            {
                DetailPageMain();
            }
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_isPostBack) return;

            _isPostBack = true;
            DetailPageMain();
        }

        private static List<HeroRankWrapper> LoadHeroRankData(string content)
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

        private void HeroLevelSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.HeroLevelTextBlock != null)
            {
                int level = Convert.ToInt32(e.NewValue);
                this.HeroLevelTextBlock.Text = level.ToString(CultureInfo.InvariantCulture);
                if (_hero != null)
                {
                    _hero.SetLevel(level);
                }
            }
        }

        private void EquipmentLongListSelector_OnTap(object sender, GestureEventArgs e)
        {
            if (EquipmentLongListSelector.SelectedItem != null)
            {
                App.ViewModel.EquipmentRecommendSelected = EquipmentLongListSelector.SelectedItem as EquipmentRecommend;
                EquipmentLongListSelector.SelectedItem = null; //reset selected item
                NavigationService.Navigate(new Uri("/EquipmentRecommendDetailPage.xaml", UriKind.Relative));
            }
        }

        private async void EquipmentLongListSelector_OnRefreshTriggered(object sender, EventArgs e)
        {
            List<EquipmentRecommend> list = await App.ViewModel.LoadEquipmentRecommendList(_hero.Name);
            equipmentRecommends.Clear();
            foreach (EquipmentRecommend recommend in list)
            {
                equipmentRecommends.Add(recommend);
            }
            this.EquipmentLongListSelector.HideRefreshPanel();
        }

        private void HideImagePopUp()
        {
            if (_imagePopUp.IsOpen)
            {
                _imagePopUp.IsOpen = false;
                ApplicationBar = null;
            }
        }

        private void DetailsPage_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (_imagePopUp.IsOpen)
            {
                HideImagePopUp();
                e.Cancel = true;
            }

            if (_actionPopup.IsOpen)
            {
                _actionPopup.Hide();
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

            var wb = new WebBrowser { Height = 800, Width = 480 };

            var tmpFilePath = await App.NewsViewModel.NewsRepository.SaveHtmlToTempIsoFile(htmlContent);
            wb.Navigate(new Uri(tmpFilePath, UriKind.Relative));

            wb.Background = Application.Current.GetTheme() == Theme.Dark ? new SolidColorBrush(Colors.Black)
                                                                         : new SolidColorBrush(Colors.White);
            _imagePopUp.Child = wb;

            ApplicationBar = new ApplicationBar { Opacity = 1 };
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
                _imagePopUp.IsOpen = true;
            };
        }

        private void HeroRanListBox_OnTap(object sender, GestureEventArgs e)
        {
            if (HeroRanListBox.SelectedItem != null)
            {
                HeroRankWrapper heroRankWrapper = HeroRanListBox.SelectedItem as HeroRankWrapper;
                if (heroRankWrapper != null)
                {
                    HeroRanListBox.SelectedItem = null; //reset selected item.
                    string url = string.Format("/PlayerDetailPage.xaml?sn={0}&pn={1}", heroRankWrapper.ServerName, heroRankWrapper.PlayerName);
                    NavigationService.Navigate(new Uri(url, UriKind.Relative));
                }
            }
        }

        private ObservableCollection<LetvVideoListInfo> _letvHeroVideoListInfos;

        protected async void BindHeroVideoList()
        {
            if (HeroVideoLongListSelector.ItemsSource == null)
            {
                HeroVideoLongListSelector.ItemsSource = _letvHeroVideoListInfos;
            }

            HeroVideoLoadingBar.Visibility = Visibility.Visible;
            HeroVideoLongListSelector.Visibility = Visibility.Collapsed;

            try
            {
                if (_currentLateastPage == 1)
                {
                    _letvHeroVideoListInfos.Clear();
                }
                var heroVideoList = await App.ViewModel.VideoRepository.GetTypedLetvVideoListyAsync(_hero.Name, _currentLateastPage);
                foreach (var info in heroVideoList)
                {
                    _letvHeroVideoListInfos.Add(info);
                }
                HeroVideoLongListSelector.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
                HeroVideoRetryNetPanel.Visibility = Visibility.Visible;
            }
            finally
            {
                HeroVideoLoadingBar.Visibility = Visibility.Collapsed;
            }
        }


        private void VideoListLongListSelector_OnTap(object sender, GestureEventArgs e)
        {
            var longListSelector = sender as RefreshableListBox;
            if (longListSelector != null && longListSelector.SelectedItem != null)
            {
                var videoListInfo = longListSelector.SelectedItem as LetvVideoListInfo;
                if (videoListInfo != null)
                {
                    longListSelector.SelectedItem = null;//reset selected item
                    App.ViewModel.VideoRepository.PrepareLetvVideoActionPopup(_actionPopup, videoListInfo);
                    _actionPopup.Show();
                }
            }
        }

        private async void HeroVideoLongListSelector_OnGettingMoreTriggered(object sender, EventArgs e)
        {
            LetvVideoListInfo lastVideoListInfo = null;

            try
            {
                _currentLateastPage += 1;
                lastVideoListInfo = _letvHeroVideoListInfos.Last();

                var lateastVideoList = await App.ViewModel.VideoRepository.GetTypedLetvVideoListyAsync(_hero.Name, _currentLateastPage);
                foreach (var info in lateastVideoList)
                {
                    _letvHeroVideoListInfos.Add(info);
                }
            }
            catch (Exception)
            {
                ToastPromts.GetToastWithImgAndTitle("网络不太稳定，加载获取失败.").Show();
                return;
            }
            finally
            {
                HeroVideoLongListSelector.HideGettingMorePanel();
            }

            if (lastVideoListInfo != null)
            {
                HeroVideoLongListSelector.ScrollTo(lastVideoListInfo);
            }
        }
    }
}