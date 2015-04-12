using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Coding4Fun.Toolkit.Controls;
using HtmlAgilityPack;
using LolWikiApp.Repository;
using LolWikiApp.Resources;
using LolWikiApp.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using TiltEffect = Microsoft.Phone.Controls.TiltEffect;


namespace LolWikiApp
{
    public partial class HomePage : PhoneApplicationPage
    {
        private List<Hero> _freeHeros;
        private int _currentPage;

        private NewsTypeWrapper _currentNewsTypeWrapper;

        private bool _isPivotFirstLoaded = false;
        private bool _isQuitConfirmOpened = false;

        private ApplicationBarMenuItem _feedBackApplicationBarMenuItem;
        private ApplicationBarMenuItem _aboutApplicationBarMenuItem;

        private bool _isPostBack;

        public HomePage()
        {
            InitializeComponent();
            Debug.WriteLine(NavigationCacheMode);

            _freeHeros = new List<Hero>();
            _currentPage = 0;
            _currentNewsTypeWrapper = new NewsTypeWrapper() { Type = NewsType.Latest, Source = "HELPER" };

            _feedBackApplicationBarMenuItem = new ApplicationBarMenuItem() { Text = "意见反馈" };
            _feedBackApplicationBarMenuItem.Click += (s, e) =>
            {
                var task = new EmailComposeTask { To = "newlight@qq.com", Subject = "意见反馈-英雄联盟助手v2.0" };
                task.Show();
            };

            _aboutApplicationBarMenuItem = new ApplicationBarMenuItem() { Text = "关于" };
            _aboutApplicationBarMenuItem.Click += (s, e) => NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void HomePageMain()
        {
            ApplicationBar = null;

            switch (MainPivot.SelectedIndex)
            {
                case 0: //资讯
                    NewsDataBindAsync();
                    LoadCachedNews();
                    MainPivot.Margin = new Thickness(0, 0, 0, -90);
                    break;
                case 1: //周免
                    BindFreeHeroInfoAsync();
                    MainPivot.Margin = new Thickness(0);
                    break;
                case 2: //我的
                    BindRecords();
                    MainPivot.Margin = new Thickness(0);
                    break;
                case 3: //更多
                    break;
            }
        }

        private string _cacheId;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_isPostBack)
            {
                if (MainPivot.SelectedIndex == 2)
                {
                    BindRecords();
                }
                return;
            }

            _isPostBack = true;

            if (NavigationContext.QueryString.TryGetValue("cacheId", out _cacheId))
            {

            }

            Debug.WriteLine("HOME PAGE LOAD OnNavigatedTo");
            HomePageMain();

            // Check if ExtendedSplashscreen.xaml is on the backstack  and remove 
            if (NavigationService.BackStack.Count() == 1)
            {
                NavigationService.RemoveBackEntry();
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //base.OnNavigatingFrom(e);
        }

        private void HomePage_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (SlideMenuPopup.IsOpen)
            {
                HideNewsCategoryPopup();
                e.Cancel = true;
            }
            else if (_isHidden)
            {
                ShowPivotHeader();
                e.Cancel = true;
            }
            else
            {
                if (_isQuitConfirmOpened)
                {
                    //_isQuitConfirmOpened = false;
                    return;
                }

                var confirmQuiToastPromt = ToastPromts.GetToastWithImgAndTitle("再按一次退出英雄联盟助手!");
                confirmQuiToastPromt.Height = 60;
                _isQuitConfirmOpened = true;
                confirmQuiToastPromt.Show();
                e.Cancel = true;
                confirmQuiToastPromt.Opened += (s, e3) =>
                {
                    //_isQuitConfirmOpened = false;
                    //_isQuitConfirmOpened = true;
                };

                confirmQuiToastPromt.Completed += (s, e2) =>
                {
                    _isQuitConfirmOpened = false;
                };
            }
        }

        #region Pivot
        private void MainPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isPivotFirstLoaded == false && MainPivot.SelectedIndex == 0)
            {
                _isPivotFirstLoaded = true;
            }
            else
            {
                Debug.WriteLine("HOME PAGE LOAD MainPivot_OnSelectionChanged");
                ShowPivotHeader();
                HomePageMain();
            }
        }
        #endregion

        #region 资讯中心
        private void NewsLongListSelector_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_cacheId == "NEWS")
            {
                var cachedNewsListInfo = App.ViewModel.CachedObject as NewsListInfo;
                if (cachedNewsListInfo != null)
                {
                    NewsLongListSelector.ScrollTo(App.NewsViewModel.NewsListInfObservableCollection.Last());
                }
            }
        }

        private void NewsDataBindAsync()
        {
            if (this.NewsLongListSelector.ItemsSource == null)
            {
                LoadNewsData();
                NewsLongListSelector.ItemsSource = App.NewsViewModel.NewsListInfObservableCollection;
            }

            SetAppbarForNewsList();
        }

        private void SetAppbarForNewsList()
        {
            ApplicationBar = new ApplicationBar { Opacity = 1, Mode = ApplicationBarMode.Minimized };

            var refreshButton = new ApplicationBarIconButton();
            var cacheButton = new ApplicationBarIconButton();

            refreshButton.IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative);
            refreshButton.Text = "刷新";

            cacheButton.IconUri = new Uri("/Assets/AppBar/download.png", UriKind.Relative);
            cacheButton.Text = "离线阅读";

            refreshButton.Click += (s, e) => LoadNewsData();
            cacheButton.Click += (s, e) => NavigationService.Navigate(new Uri("/NewsCachePage.xaml", UriKind.Relative));

            ApplicationBar.MenuItems.Add(_feedBackApplicationBarMenuItem);
            ApplicationBar.MenuItems.Add(_aboutApplicationBarMenuItem);

            ApplicationBar.Buttons.Add(refreshButton);
            ApplicationBar.Buttons.Add(cacheButton);
        }

        private async void LoadCachedNews()
        {
            //Launch new task to load cached news list
            if (!App.NewsViewModel.NewsCacheListInfo.IsDataLoaded)
            {
                await Task.Run(() => App.NewsViewModel.LoadCachedNewsList());
            }
        }

        private async void LoadNewsData()
        {
            CategoryGrid.Projection = new PlaneProjection() { RotationX = 90 };

            NewsRetryNetPanel.Visibility = Visibility.Collapsed;
            NewsLoadingBar.Visibility = Visibility.Visible;
            NewsLongListSelector.Visibility = Visibility.Collapsed;
            SystemTray.ProgressIndicator.IsVisible = true;
            try
            {
                Debug.WriteLine(_currentNewsTypeWrapper);

                await App.NewsViewModel.LoadNewsListInfosByTypeAndPageAsync(_currentNewsTypeWrapper, 1, true);
            }
            catch (Exception exception404)
            {
                var toast = ToastPromts.GetToastWithImgAndTitle("加载失败，读取离线文章。");
                toast.Show();

                App.NewsViewModel.LoadeNewsListInfoFromCache(_currentNewsTypeWrapper.Type);

                if (App.NewsViewModel.NewsListInfObservableCollection.Count > 0)
                {
                    NewsLongListSelector.Visibility = Visibility.Visible;
                }
                else
                {
                    NewsRetryNetPanel.Visibility = Visibility.Visible;
                }

                return;
            }
            finally
            {
                _currentPage = 1;
                NewsLoadingBar.Visibility = Visibility.Collapsed;
                SystemTray.ProgressIndicator.IsVisible = false;
            }

            NewsRetryNetPanel.Visibility = Visibility.Collapsed;
            NewsLongListSelector.Visibility = Visibility.Visible;
            _adAnimatonHelper.RunShowStoryboard(CategoryGrid, AnimationTypes.SwivelForwardIn, TimeSpan.FromSeconds(0.6));
        }

        private void NewsLongListSelector_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (NewsLongListSelector.SelectedItem != null)
            {
                var newsInfo = NewsLongListSelector.SelectedItem as NewsListBaseInfo;
                if (newsInfo != null && newsInfo.NewsType != null)
                {
                    if (newsInfo.NewsType.Source == "HELPER")
                    {
                        var helperNews = newsInfo as NewsListInfo;
                        if (helperNews != null && !helperNews.IsFlipNews)
                        {
                            NewsLongListSelector.SelectedItem = null; //reset selected item
                            //App.ViewModel.CachedObject = newsInfo;
                            NavigationService.Navigate(new Uri("/NewsDetailPage.xaml?newsId=" + helperNews.Id, UriKind.Relative));
                        }
                    }

                    if (newsInfo.NewsType.Source == "TC")
                    {
                        var tcNews = newsInfo as TcNewsListInfo;
                        if (tcNews != null)
                        {
                            NewsLongListSelector.SelectedItem = null; //reset selected item
                            NavigationService.Navigate(new Uri("/NewsDetailPage.xaml?newsUrl=" + tcNews.article_url, UriKind.Relative));
                        }
                    }
                }
            }
        }


        //点击分类，显示侧边菜单框
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            NewsCategoryListBox.ItemsSource = App.NewsViewModel.NewsTypeList;

            var index = 0;
            foreach (var item in NewsCategoryListBox.Items)
            {
                var type = item as NewsTypeWrapper;
                if (type != null && type.Type == _currentNewsTypeWrapper.Type)
                {
                    break;
                }
                index++;
            }

            NewsCategoryListBox.SelectedIndex = index;
            ApplicationBar = null; //隐藏appbar
            SlideMenuPopup.IsOpen = true;
        }

        //实现点击空白区域，隐藏侧边框
        private void PopGrid_OnTap(object sender, GestureEventArgs e)
        {
            if (SlideMenuPopup.IsOpen)
            {
                HideNewsCategoryPopup();
            }
        }

        //阻止事件下传，实现点击菜单区域，不会激发空白区域的event
        private void MenuListStackPanel_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
        }

        //点击具体类别
        private void NewsCategoryListBox_OnTap(object sender, GestureEventArgs e)
        {
            var selectedCateogry = NewsCategoryListBox.SelectedItem;
            if (selectedCateogry != null)
            {
                var category = selectedCateogry as NewsTypeWrapper;
                if (category != null)
                {
                    _currentNewsTypeWrapper = category;
                    CategoryTextBlock.Text = category.DisplayName;
                    HideNewsCategoryPopup();
                    LoadNewsData();
                }
            }
        }

        private void NewsListRetryGetMoreLoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            LoadMoreNewsList();
        }

        private void NewsLongListSelector_OnGettingMoreTriggered(object sender, EventArgs e)
        {
            LoadMoreNewsList();
        }

        private async void LoadMoreNewsList()
        {
            NewsListBaseInfo lastNews = null;
            try
            {
                var nextPage = _currentPage + 1;
                Debug.WriteLine("[news]nextpage: " + nextPage);

                var count = App.NewsViewModel.NewsListInfObservableCollection.Count;
                var index = count > 2 ? count - 2 : count - 1;
                lastNews = App.NewsViewModel.NewsListInfObservableCollection[index];
                Debug.WriteLine("load more news : " + _currentNewsTypeWrapper.Type);
                await App.NewsViewModel.LoadNewsListInfosByTypeAndPageAsync(_currentNewsTypeWrapper, nextPage);
            }
            catch (Exception exception404)
            {
                //NewsListGetMoreRetryNetPanel.Visibility = Visibility.Visible;
                ToastPromts.GetToastWithImgAndTitle("网络不太稳定，加载获取失败.").Show();
                return;
            }
            finally
            {
                this.NewsLongListSelector.HideGettingMorePanel();
            }

            if (lastNews != null)
            {
                this.NewsLongListSelector.ScrollTo(lastNews);
            }

            _currentPage++;
        }

        private void HorizontalFlipView_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var item = ((FlipView)sender).SelectedItem as NewsListInfo;
            if (item != null && !string.IsNullOrEmpty(item.Id))
            {
                 //((FlipView)sender).SelectedIndex
                 //((FlipView)sender).Items.Count
                NavigationService.Navigate(new Uri("/NewsDetailPage.xaml?newsId=" + item.Id, UriKind.Relative));
            }
        }
        #endregion

        #region 周免英雄
        private void BindFreeHeroInfoAsync()
        {
            ApplicationBar = new ApplicationBar { Opacity = 1, Mode = ApplicationBarMode.Minimized };
            var refreshButton = new ApplicationBarIconButton();
            var moreButton = new ApplicationBarIconButton();

            refreshButton.IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative);
            refreshButton.Text = "刷新";

            moreButton.IconUri = new Uri("/Assets/AppBar/more-1.png", UriKind.Relative);
            moreButton.Text = "全部英雄";

            moreButton.Click += (s, e) => NavigationService.Navigate(new Uri("/AllHeroPage.xaml", UriKind.Relative));
            refreshButton.Click += (s1, e1) => RefreshFreeHeroList(true);

            ApplicationBar.MenuItems.Add(_feedBackApplicationBarMenuItem);
            ApplicationBar.MenuItems.Add(_aboutApplicationBarMenuItem);

            ApplicationBar.Buttons.Add(refreshButton);
            ApplicationBar.Buttons.Add(moreButton);

            if (_freeHeros.Count == 0)
            {
                RefreshFreeHeroList();
            }
        }

        private async void RefreshFreeHeroList(bool isForced = false)
        {
            FreeHeroLoadingBar.Visibility = Visibility.Visible;
            this.FreeHeroRetryNetPanel.Visibility = Visibility.Collapsed;

            try
            {
                _freeHeros.Clear();
                this.HeroWrapPanel.Children.Clear();
                _freeHeros = await App.ViewModel.LoadFreeHeroInfoListAsync(isForced);
                _freeHeros.ForEach(AddFreeHeroItem);
            }
            catch (Exception exception404)
            {
                Debug.WriteLine("refresh free hero 404");
                this.FreeHeroRetryNetPanel.Visibility = Visibility.Visible;
                return;
            }
            finally
            {
                FreeHeroLoadingBar.Visibility = Visibility.Collapsed;
            }

            this.FreeHeroRetryNetPanel.Visibility = Visibility.Collapsed;
            //SystemTray.ProgressIndicator.IsVisible = false;
        }

        private void AddFreeHeroItem(Hero hero)
        {
            if (hero == null)
                return;

            var stackPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Vertical,
                Margin = new Thickness(0, 8, 32, 8)
            };
            //120
            Debug.WriteLine(hero.Title + ", " + hero.Name);
            //var key = GetHeroKeyById(hero.Id);
            //var imgSource = string.Format("http://cdn.tgp.qq.com/pallas/images/minicard/{0}.jpg", key);

            var img = new Image
            {
                Source = new BitmapImage(new Uri(hero.ImageUrl, UriKind.Relative)),
                Width = 90,
                Stretch = Stretch.UniformToFill
            };
            //img.Tap += (s, e) => NavigationService.Navigate(new Uri("/HeroDetailsPage.xaml?selectedId=" + hero.Id, UriKind.Relative));

            var textBlock = new TextBlock();

            var tmpTitle = hero.Title.Length > 4 ? hero.Title.Substring(0, 4) + ".." : hero.Title;
            textBlock.Text = tmpTitle;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.FontSize = (double)Application.Current.Resources["PhoneFontSizeSmall"];
            textBlock.Foreground = new SolidColorBrush(Colors.Black);

            var textBlock2 = new TextBlock();
            var tmpName = hero.Name.Length > 4 ? hero.Name.Substring(0, 4) + ".." : hero.Name;
            textBlock2.Text = tmpName;
            textBlock2.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock2.FontSize = (double)Application.Current.Resources["PhoneFontSizeSmall"];
            textBlock2.Foreground = new SolidColorBrush(Colors.Gray);

            stackPanel.Children.Add(img);
            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(textBlock2);

            stackPanel.Tap += (s, e) =>
            {
                var helper = new AnimatonHelper();
                helper.RunShowStoryboard(stackPanel, AnimationTypes.Flash, TimeSpan.FromSeconds(0), (s1, e1) => NavigationService.Navigate(new Uri("/HeroDetailsPage.xaml?selectedId=" + hero.Id, UriKind.Relative)));
            };


            //stackPanel.Tap += (s, e) => NavigationService.Navigate(new Uri("/HeroDetailsPage.xaml?selectedId=" + hero.Id, UriKind.Relative));

            HeroWrapPanel.Children.Add(stackPanel);
        }
        #endregion

        #region 我的关注
        private void SetBindAppBar()
        {
            ApplicationBar = new ApplicationBar { Opacity = 1, Mode = ApplicationBarMode.Minimized };
            var pinButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Data/appbar.add.png", UriKind.Relative),
                Text = "加关注"
            };

            var refreshButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative),
                Text = "刷新"
            };

            refreshButton.Click += (s, e) => BindRecords(); ;
            pinButton.Click += (s, e) => NavigationService.Navigate(new Uri("/PlayerInformationPage.xaml?mode=bind", UriKind.Relative));

            ApplicationBar.MenuItems.Add(_feedBackApplicationBarMenuItem);
            ApplicationBar.MenuItems.Add(_aboutApplicationBarMenuItem);
            ApplicationBar.Buttons.Add(pinButton);
            ApplicationBar.Buttons.Add(refreshButton);
        }

        private void SetRetryAppBar()
        {
            ApplicationBar = new ApplicationBar { Opacity = 0.8 };
            var retryButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative),
                Text = "重试"
            };

            retryButton.Click += (s, e) => BindRecords(); ;

            ApplicationBar.MenuItems.Add(_feedBackApplicationBarMenuItem);
            ApplicationBar.MenuItems.Add(_aboutApplicationBarMenuItem);

            ApplicationBar.Buttons.Add(retryButton);
        }

        //private void SetMoreUnBindAndSearchAppBar()
        //{
        //    ApplicationBar = new ApplicationBar { Opacity = 0.8 };
        //    var moreButton = new ApplicationBarIconButton();
        //    var unpinButton = new ApplicationBarIconButton();
        //    var searchButton = new ApplicationBarIconButton();

        //    moreButton.IconUri = new Uri("/Assets/AppBar/more-1.png", UriKind.Relative);
        //    moreButton.Text = "更多信息";

        //    unpinButton.IconUri = new Uri("/Assets/AppBar/unpin-1.png", UriKind.Relative);
        //    unpinButton.Text = "解除绑定";

        //    searchButton.IconUri = new Uri("/Assets/AppBar/feature.search.png", UriKind.Relative);
        //    searchButton.Text = "搜索召唤师";

        //    moreButton.Click += (s, e) => MoreAboutPlayer();

        //    unpinButton.Click += (s2, e2) => RemoveBindOfPlayer();

        //    searchButton.Click += (s3, e3) => SearchPlayer();

        //    ApplicationBar.MenuItems.Add(_feedBackApplicationBarMenuItem);
        //    ApplicationBar.MenuItems.Add(_aboutApplicationBarMenuItem);

        //    ApplicationBar.Buttons.Add(moreButton);
        //    ApplicationBar.Buttons.Add(searchButton);
        //    ApplicationBar.Buttons.Add(unpinButton);
        //}

        private void BindRecords()
        {
            if (App.ViewModel.BindedPlayerInfoWrappers.Count == 0)
            {
                NeedBindPlayerInfoPanel.Visibility = Visibility.Visible;
                PlayerRecordPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                NeedBindPlayerInfoPanel.Visibility = Visibility.Collapsed;

                if (!App.ViewModel.IsBindedPlayerInfoDataLoaded)
                {
                    LoadPlayersInfo();
                }
                else
                {
                    PlayerRecordPanel.Visibility = Visibility.Visible;
                    PlayerRecordPanel.ItemsSource = App.ViewModel.BindedPlayers;
                    MyInfomationRetryNetPanel.Visibility = Visibility.Collapsed;
                }
            }

            SetBindAppBar();
        }

        private void PlayerRecordPanel_OnTap(object sender, GestureEventArgs e)
        {
            var player = PlayerRecordPanel.SelectedItem as Player;
            if (player != null)
            {
                PlayerRecordPanel.SelectedItem = null;
                App.ViewModel.SelectedPlayer = player;
                NavigationService.Navigate(new Uri("/PlayerDetailPage.xaml", UriKind.Relative));
            }
        }

        private void MoreAboutPlayer()
        {

        }

        private void RemoveBindOfPlayer()
        {
            //if (App.ViewModel.BindedPlayer == null)
            //    return;

            //string message = string.Format("确定解除对召唤师 {0} 的绑定？", App.ViewModel.BindedPlayer.Name);
            //if (MessageBoxResult.OK == MessageBox.Show(message, "确认", MessageBoxButton.OKCancel))
            //{
            //    App.ViewModel.RemovePlayerBind();

            //    App.ViewModel.BindedPlayer = null;
            //    PlayerRecordPanel.DataContext = App.ViewModel.BindedPlayer;
            //    BindRecords();
            //}
        }

        private void SearchPlayer()
        {
            NavigationService.Navigate(new Uri("/PlayerInformationPage.xaml?mode=search", UriKind.Relative));
        }

        //取消关注
        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var player = menuItem.Tag as Player;
                if (player != null)
                {
                    App.ViewModel.DeleteBindedPlayer(player);
                }
            }
        }

        private async void LoadPlayersInfo()
        {
            MyDataLoadingBar.Visibility = Visibility.Visible;
            foreach (var playerInfoSettingWrapper in App.ViewModel.BindedPlayerInfoWrappers)
            {
                if (playerInfoSettingWrapper.IsDataLoaded == false)
                {
                    var actionResult = await App.ViewModel.GetPlayerDetailInfo(playerInfoSettingWrapper.ServerInfo.Value, playerInfoSettingWrapper.Name);
                    if (actionResult.Result == ActionResult.Success)
                    {
                        var player = actionResult.Value as Player;
                        if (player != null)
                        {
                            playerInfoSettingWrapper.IsDataLoaded = true;
                            player.IsDataLoaded = true;
                            App.ViewModel.BindedPlayers.Add(player);
                        }
                    }
                }
            }

            MyDataLoadingBar.Visibility = Visibility.Collapsed;

            PlayerRecordPanel.Visibility = Visibility.Visible;
            PlayerRecordPanel.ItemsSource = App.ViewModel.BindedPlayers;
            MyInfomationRetryNetPanel.Visibility = Visibility.Collapsed;

            //switch (actionResult.Result)
            //{
            //    case ActionResult.Exception404:
            //        MyInfomationRetryNetPanel.Visibility = Visibility.Visible;
            //        PlayerRecordPanel.Visibility = Visibility.Collapsed;
            //        SetRetryAppBar();
            //        break;
            //    case ActionResult.NotFound:
            //        NeedBindPlayerInfoPanel.Visibility = Visibility.Visible;
            //        PlayerRecordPanel.Visibility = Visibility.Collapsed;
            //        SetBindAppBar();
            //        break;

            //    case ActionResult.Success:
            //        Player detailPlayerInfo = actionResult.Value as Player;
            //        PlayerRecordPanel.Visibility = Visibility.Visible;
            //        if (detailPlayerInfo == null)
            //        {
            //            NeedBindPlayerInfoPanel.Visibility = Visibility.Visible;
            //            PlayerRecordPanel.Visibility = Visibility.Collapsed;
            //        }
            //        else
            //        {
            //            detailPlayerInfo.ServerInfo = briefPlayerInfo.ServerInfo;
            //            App.ViewModel.BindedPlayer = detailPlayerInfo;
            //            PlayerRecordPanel.DataContext = App.ViewModel.BindedPlayer;
            //        }

            //        PlayerRecordPanel.Visibility = Visibility.Visible;
            //        PlayerRecordPanel.DataContext = App.ViewModel.BindedPlayer;

            //        MyInfomationRetryNetPanel.Visibility = Visibility.Collapsed;
            //        SetMoreUnBindAndSearchAppBar();
            //        break;
            //}
        }
        #endregion

        #region 更多
        private void ShakeItButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ShakeAShakePage.xaml", UriKind.Relative));
        }

        private void SearchRecordButton_OnClick(object sender, RoutedEventArgs e)
        {
            SearchPlayer();
        }

        private void RateButton_OnClick(object sender, RoutedEventArgs e)
        {
            var reviewTask = new MarketplaceReviewTask();
            reviewTask.Show();
        }

        private void VideoButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/LetvVideoPage.xaml", UriKind.Relative));
        }

        private void HuangliButton_OnClick(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new Uri("/NewsDetailPage.xaml?fullUrl=http://lol.qq.com/lolApp/news/lolhuangli.htm", UriKind.Relative));

            NavigationService.Navigate(new Uri("/NewsDetailPage.xaml?item=all", UriKind.Relative));
        }

        private void AllHeroButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AllHeroPage.xaml", UriKind.Relative));
        }
        #endregion

        private AnimatonHelper _adAnimatonHelper = new AnimatonHelper();
        private bool _isHideing = false;
        private bool _isHidden = false;

        private void LongListSelector_OnListScrollingUp(object sender, EventArgs e)
        {
            HidePivotHeader();
        }

        private bool _isShowing = false;
        private void LongListSelector_OnListScrollingDown(object sender, EventArgs e)
        {
            ShowPivotHeader();
        }

        private void ShowPivotHeader()
        {
            if (_isShowing == false && _isHideing == false && _isHidden == true)
            {
                _isShowing = true;
                _adAnimatonHelper.RunShowStoryboard(CategoryBackGroundGrid, AnimationTypes.FadeOut, TimeSpan.FromSeconds(0.6), null, "0.8");
                _adAnimatonHelper.RunShowStoryboard(MainPivot, AnimationTypes.SlideDown, TimeSpan.FromSeconds(0),
                    (o, args) =>
                    {
                        _isShowing = false;
                        _isHidden = false;
                    });
            }
        }

        private void HidePivotHeader()
        {
            if (_isHideing == false && _isShowing == false && _isHidden == false)
            {
                _isHideing = true;
                _adAnimatonHelper.RunShowStoryboard(CategoryBackGroundGrid, AnimationTypes.FadeIn,
                    TimeSpan.FromSeconds(0.6), null, "0.8");
                _adAnimatonHelper.RunShowStoryboard(MainPivot, AnimationTypes.SlideUp, TimeSpan.FromSeconds(0),
                    (o, args) =>
                    {
                        _isHideing = false;
                        _isHidden = true;

                        //防止在隐藏过程中切换pivot item导致header不显示
                        if (MainPivot.SelectedIndex != 0)
                        {
                            ShowPivotHeader();
                        }
                    });
            }
        }

        private void FunnyNewsListItemBorder_OnLoaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("FunnyBorderLoaded: " + DateTime.Now);
        }

        private void HideNewsCategoryPopup()
        {
            _adAnimatonHelper.RunShowStoryboard(MenuListStackPanel, AnimationTypes.SlideRightOutFade, TimeSpan.FromSeconds(0),
                    (o, args) =>
                    {
                        SlideMenuPopup.IsOpen = false;
                        SetAppbarForNewsList();
                    });
        }


    }
}