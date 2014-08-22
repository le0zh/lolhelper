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
using LolWikiApp.Resources;
using LolWikiApp.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Color = Microsoft.Xna.Framework.Color;
using TiltEffect = Microsoft.Phone.Controls.TiltEffect;

namespace LolWikiApp
{
    public partial class HomePage : PhoneApplicationPage
    {
        private List<Hero> _freeHeros;
        private int _currentPage;
        private NewsType _currentNewsType;
        private bool _isPivotFirstLoaded = false;
        private readonly FullScreenPopup _newsCategoryPopup;

        private bool _isQuitConfirmOpened = false;

        public HomePage()
        {
            InitializeComponent();
            _freeHeros = new List<Hero>();
            _currentPage = 0;
            _currentNewsType = NewsType.Latest;
            _newsCategoryPopup = new FullScreenPopup();
        }

        private void HomePageMain()
        {
            ApplicationBar = null;

            switch (MainPivot.SelectedIndex)
            {
                case 0: //资讯
                    NewsDataBindAsync();
                    LoadCachedNews();
                    break;
                case 1: //周免
                    BindFreeHeroInfoAsync();
                    break;
                case 2: //我的
                    BindRecords();
                    break;
                case 3: //工具
                    break;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("HOME PAGE LOAD OnNavigatedTo");
            HomePageMain();

            base.OnNavigatedTo(e);
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
                HomePageMain();
            }
        }
        #endregion

        #region 资讯列表

        private void NewsDataBindAsync()
        {
            if (this.NewsLongListSelector.ItemsSource == null)
            {
                this.NewsLongListSelector.ItemsSource = App.NewsViewModel.NewsListInfObservableCollection;
                LoadNewsData();
            }

            SetAppbarForNewsList();
        }

        private void SetAppbarForNewsList()
        {
            ApplicationBar = new ApplicationBar { Opacity = 1 };

            var refreshButton = new ApplicationBarIconButton();
            var categoryButton = new ApplicationBarIconButton();
            var cacheButton = new ApplicationBarIconButton();

            refreshButton.IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative);
            refreshButton.Text = "刷新";

            categoryButton.IconUri = new Uri("/Assets/AppBar/category.png", UriKind.Relative);
            categoryButton.Text = "分类";

            cacheButton.IconUri = new Uri("/Assets/AppBar/download.png", UriKind.Relative);
            cacheButton.Text = "离线阅读";

            refreshButton.Click += (s, e) => LoadNewsData();
            categoryButton.Click += (s, e) => ShowNewsCategoriesPopup();
            cacheButton.Click += (s, e) => NavigationService.Navigate(new Uri("/NewsCachePage.xaml", UriKind.Relative));

            ApplicationBar.Buttons.Add(refreshButton);
            ApplicationBar.Buttons.Add(categoryButton);
            ApplicationBar.Buttons.Add(cacheButton);
        }

        private void ShowNewsCategoriesPopup()
        {
            if (_newsCategoryPopup.IsOpen)
            {
                _newsCategoryPopup.Hide();
                return;
            }

            var mainStackPanel = new StackPanel
            {
                Background = Application.Current.Resources["PhoneChromeBrush"] as SolidColorBrush,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0,0,0,72)
            };

            var titleTextBlock = new TextBlock()
            {
                Text = "请选择资讯类型 ",
                Foreground = new SolidColorBrush(Colors.DarkGray),
                FontSize = (double)Application.Current.Resources["PhoneFontSizeLarge"],
                Margin = new Thickness(24, 12, 12, 0),
            };

            var newsTypeListBox = new ListBox
            {
                Height = 335,
                Width = 480,
                ItemsSource = App.NewsViewModel.NewsTypeList,
                Margin = new Thickness(0, 12, 0, 0),
                Padding = new Thickness(0,4,0,0),
                SelectionMode = SelectionMode.Single,
                Background = new SolidColorBrush(Colors.DarkGray),
                ItemTemplate = Application.Current.Resources["NewsTypeListBoxTemplate"] as DataTemplate,
                ItemContainerStyle = Application.Current.Resources["NewsTypeListBoxItemStyle"] as Style,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            newsTypeListBox.SetValue(TiltEffect.IsTiltEnabledProperty, true);

            var index = 0;
            foreach (var item in newsTypeListBox.Items)
            {
                var type = item as NewsTypeWrapper;
                if (type != null && type.Type == _currentNewsType)
                {
                    break;
                }
                index++;
            }

            newsTypeListBox.SelectedIndex = index;

            ApplicationBar = new ApplicationBar { Opacity = 1 };

            var selectButton = new ApplicationBarIconButton();
            var cancelButton = new ApplicationBarIconButton();

            selectButton.IconUri = new Uri("/Assets/AppBar/check.png", UriKind.Relative);
            selectButton.Text = "选择";

            cancelButton.IconUri = new Uri("/Assets/AppBar/close.png", UriKind.Relative);
            cancelButton.Text = "取消";

            selectButton.Click += (s, e) =>
            {
                if (newsTypeListBox.SelectedItem != null)
                {
                    var newsType = newsTypeListBox.SelectedItem as NewsTypeWrapper;
                    _currentNewsType = newsType == null ? NewsType.Latest : newsType.Type;
                    LoadNewsData();
                }
                SetAppbarForNewsList();
            };
            cancelButton.Click += (s, e) =>
            {
                _newsCategoryPopup.Hide();
                SetAppbarForNewsList();
            };

            ApplicationBar.Buttons.Add(selectButton);
            ApplicationBar.Buttons.Add(cancelButton);

            mainStackPanel.Children.Add(titleTextBlock);
            mainStackPanel.Children.Add(newsTypeListBox);

            _newsCategoryPopup.Child = mainStackPanel;
            _newsCategoryPopup.Show();
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
            if (_newsCategoryPopup.IsOpen)
            {
                _newsCategoryPopup.Hide();
            }

            this.NewsLoadingBar.Visibility = Visibility.Visible;
            try
            {
                Debug.WriteLine(_currentNewsType);

                await App.NewsViewModel.LoadNewsListInfosByTypeAndPageAsync(_currentNewsType, 1, true);
            }
            catch (System.Net.Http.HttpRequestException exception404)
            {
                //this.NewsRetryNetPanel.Visibility = Visibility.Visible;

                var toast = ToastPromts.GetToastWithImgAndTitle("加载失败，读取离线文章。");
                toast.Show();

                App.NewsViewModel.LoadeNewsListInfoFromCache(_currentNewsType);
                this.NewsLongListSelector.Visibility = Visibility.Visible;

                return;
            }
            finally
            {
                _currentPage = 1;
                this.NewsLoadingBar.Visibility = Visibility.Collapsed;
            }

            this.NewsRetryNetPanel.Visibility = Visibility.Collapsed;
            this.NewsLongListSelector.Visibility = Visibility.Visible;
        }

        private void NewsLongListSelector_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (this.NewsLongListSelector.SelectedItem != null)
            {
                var newsInfo = this.NewsLongListSelector.SelectedItem as NewsListInfo;
                if (newsInfo != null)
                {
                    if (!newsInfo.IsFlipNews)
                    {
                        if (_newsCategoryPopup.IsOpen)
                        {
                            _newsCategoryPopup.Hide();
                            SetAppbarForNewsList();
                        }

                        NavigationService.Navigate(new Uri("/NewsDetailPage.xaml?newsId=" + newsInfo.Id, UriKind.Relative));
                    }
                }
            }
        }

        private void NewsLongListSelector_OnRefreshTriggered(object sender, EventArgs e)
        {
            NewsDataBindAsync();
            this.NewsLongListSelector.HideRefreshPanel();
        }

        private void CancelNewsListGetMoreButton_OnClick(object sender, RoutedEventArgs e)
        {
            NewsListGetMoreRetryNetPanel.Visibility = Visibility.Collapsed;
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
            NewsListInfo lastNews = null;

            NewsListGetMoreRetryNetPanel.Visibility = Visibility.Collapsed;

            try
            {
                var nextPage = _currentPage + 1;
                Debug.WriteLine("[news]nextpage: " + nextPage);

                lastNews = App.NewsViewModel.NewsListInfObservableCollection.Last();
                Debug.WriteLine("load more news : " + _currentNewsType);
                await App.NewsViewModel.LoadNewsListInfosByTypeAndPageAsync(_currentNewsType, nextPage);
            }
            catch (System.Net.Http.HttpRequestException exception404)
            {
                NewsListGetMoreRetryNetPanel.Visibility = Visibility.Visible;
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

            _currentPage = App.NewsViewModel.CurrentPage;
        }
        #endregion

        #region 每周免费英雄
        private void BindFreeHeroInfoAsync()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 1.0;
            ApplicationBarIconButton refreshButton = new ApplicationBarIconButton();
            ApplicationBarIconButton moreButton = new ApplicationBarIconButton();

            refreshButton.IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative);
            refreshButton.Text = "刷新";

            moreButton.IconUri = new Uri("/Assets/AppBar/more-1.png", UriKind.Relative);
            moreButton.Text = "全部英雄";

            moreButton.Click += (s, e) => NavigationService.Navigate(new Uri("/AllHeroPage.xaml", UriKind.Relative));
            refreshButton.Click += (s1, e1) => refreshFreeHeroList(true);

            ApplicationBar.Buttons.Add(refreshButton);
            ApplicationBar.Buttons.Add(moreButton);

            if (_freeHeros.Count == 0)
            {
                refreshFreeHeroList();
            }
        }

        private async void refreshFreeHeroList(bool isForced = false)
        {
            FreeHeroLoadingBar.Visibility = Visibility.Visible;
            this.FreeHeroRetryNetPanel.Visibility = Visibility.Collapsed;

            try
            {
                _freeHeros.Clear();
                this.wrapPanel.Children.Clear();
                _freeHeros = await App.ViewModel.LoadFreeHeroInfoListAsync(isForced);
                _freeHeros.ForEach(AddFreeHeroItem);
            }
            catch (System.Net.Http.HttpRequestException exception404)
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

            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = System.Windows.Controls.Orientation.Vertical;
            stackPanel.Margin = new Thickness(0, 8, 18, 8);
            //stackPanel.Height = 76;
            //stackPanel.Width = 76;

            Image img = new Image();
            img.Source = new BitmapImage(new Uri(hero.ImageUrl, UriKind.Relative));
            img.Height = 90;
            img.Width = 90;
            img.Stretch = Stretch.UniformToFill;
            img.Tap += (s, e) => NavigationService.Navigate(new Uri("/HeroDetailsPage.xaml?selectedId=" + hero.Id, UriKind.Relative));

            TextBlock textBlock = new TextBlock();

            string tmpTitle = hero.Title.Length > 4 ? hero.Title.Substring(0, 4) + ".." : hero.Title;
            textBlock.Text = tmpTitle;

            stackPanel.Children.Add(img);
            stackPanel.Children.Add(textBlock);

            wrapPanel.Children.Add(stackPanel);
        }
        #endregion

        #region 战绩

        private void setBindAppBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 1.0;
            ApplicationBarIconButton pinButton = new ApplicationBarIconButton();

            pinButton.IconUri = new Uri("/Assets/AppBar/pin-1.png", UriKind.Relative);
            pinButton.Text = "绑定";

            pinButton.Click += (s, e) => NavigationService.Navigate(new Uri("/PlayerInformationPage.xaml?mode=bind", UriKind.Relative));


            ApplicationBar.Buttons.Add(pinButton);
        }

        private void setRetryAppBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 1.0;
            ApplicationBarIconButton retryButton = new ApplicationBarIconButton();

            retryButton.IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative);
            retryButton.Text = "重试";

            retryButton.Click += (s, e) => BindRecords(); ;


            ApplicationBar.Buttons.Add(retryButton);
        }

        private void setMoreUnBindAndSearchAppBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 1.0;
            ApplicationBarIconButton moreButton = new ApplicationBarIconButton();
            ApplicationBarIconButton unpinButton = new ApplicationBarIconButton();
            ApplicationBarIconButton searchButton = new ApplicationBarIconButton();

            moreButton.IconUri = new Uri("/Assets/AppBar/more-1.png", UriKind.Relative);
            moreButton.Text = "更多信息";

            unpinButton.IconUri = new Uri("/Assets/AppBar/unpin-1.png", UriKind.Relative);
            unpinButton.Text = "解除绑定";

            searchButton.IconUri = new Uri("/Assets/AppBar/feature.search.png", UriKind.Relative);
            searchButton.Text = "搜索召唤师";

            moreButton.Click += (s, e) => MoreAboutPlayer();

            unpinButton.Click += (s2, e2) => RemoveBindOfPlayer();

            searchButton.Click += (s3, e3) => SearchPlayer();

            ApplicationBar.Buttons.Add(moreButton);
            ApplicationBar.Buttons.Add(searchButton);
            ApplicationBar.Buttons.Add(unpinButton);
        }

        private void BindRecords()
        {
            if (App.ViewModel.BindedPlayer == null)
            {
                NeedBindPlayerInfoPanel.Visibility = Visibility.Visible;
                PlayerRecordPanel.Visibility = Visibility.Collapsed;

                setBindAppBar();
            }
            else
            {
                NeedBindPlayerInfoPanel.Visibility = Visibility.Collapsed;

                if (!App.ViewModel.BindedPlayer.IsDataLoaded)
                {
                    LoadPlayerInfo();
                }
                else
                {
                    PlayerRecordPanel.Visibility = Visibility.Visible;
                    PlayerRecordPanel.DataContext = App.ViewModel.BindedPlayer;

                    MyInfomationRetryNetPanel.Visibility = Visibility.Collapsed;
                    setMoreUnBindAndSearchAppBar();
                }
            }
        }

        private void MoreAboutPlayer()
        {
            App.ViewModel.SelectedPlayer = App.ViewModel.BindedPlayer;
            NavigationService.Navigate(new Uri("/PlayerDetailPage.xaml", UriKind.Relative));
        }

        private void RemoveBindOfPlayer()
        {
            if (App.ViewModel.BindedPlayer == null)
                return;

            string message = string.Format("确定解除对召唤师 {0} 的绑定？", App.ViewModel.BindedPlayer.Name);
            if (MessageBoxResult.OK == MessageBox.Show(message, "确认", MessageBoxButton.OKCancel))
            {
                App.ViewModel.RemovePlayerBind();

                App.ViewModel.BindedPlayer = null;
                PlayerRecordPanel.DataContext = App.ViewModel.BindedPlayer;
                BindRecords();
            }
        }

        private void SearchPlayer()
        {
            NavigationService.Navigate(new Uri("/PlayerInformationPage.xaml?mode=search", UriKind.Relative));
        }

        private async void LoadPlayerInfo()
        {
            Player briefPlayerInfo = App.ViewModel.BindedPlayer;
            MyDataLoadingBar.Visibility = Visibility.Visible;

            HttpActionResult actionResult =
                await App.ViewModel.GetPlayerDetailInfo(briefPlayerInfo.ServerInfo.Value, briefPlayerInfo.Name);

            MyDataLoadingBar.Visibility = Visibility.Collapsed;

            switch (actionResult.Result)
            {
                case ActionResult.Exception404:
                    MyInfomationRetryNetPanel.Visibility = Visibility.Visible;
                    PlayerRecordPanel.Visibility = Visibility.Collapsed;
                    setRetryAppBar();
                    break;
                case ActionResult.NotFound:
                    NeedBindPlayerInfoPanel.Visibility = Visibility.Visible;
                    PlayerRecordPanel.Visibility = Visibility.Collapsed;
                    setBindAppBar();
                    break;

                case ActionResult.Success:
                    Player detailPlayerInfo = actionResult.Value as Player;
                    PlayerRecordPanel.Visibility = Visibility.Visible;
                    if (detailPlayerInfo == null)
                    {
                        NeedBindPlayerInfoPanel.Visibility = Visibility.Visible;
                        PlayerRecordPanel.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        detailPlayerInfo.ServerInfo = briefPlayerInfo.ServerInfo;
                        App.ViewModel.BindedPlayer = detailPlayerInfo;
                        PlayerRecordPanel.DataContext = App.ViewModel.BindedPlayer;
                    }

                    PlayerRecordPanel.Visibility = Visibility.Visible;
                    PlayerRecordPanel.DataContext = App.ViewModel.BindedPlayer;

                    MyInfomationRetryNetPanel.Visibility = Visibility.Collapsed;
                    setMoreUnBindAndSearchAppBar();
                    break;
            }
        }
        #endregion

        #region 工具
        private void ShakeItButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ShakeAShakePage.xaml", UriKind.Relative));
        }

        private void SearchRecordButton_OnClick(object sender, RoutedEventArgs e)
        {
            SearchPlayer();
        }

        private void AboutRecordButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }
        #endregion

        private void HomePage_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (_newsCategoryPopup.IsOpen)
            {
                _newsCategoryPopup.Hide();
                SetAppbarForNewsList();
                e.Cancel = true;
            }
            else
            {
                if (_isQuitConfirmOpened) return;

                var confirmQuiToastPromt = ToastPromts.GetToastWithImgAndTitle("再按一次退出英雄联盟助手!");
                _isQuitConfirmOpened = true;
                confirmQuiToastPromt.Show();
                e.Cancel = true;
                confirmQuiToastPromt.Completed+= (s, e2) =>
                {
                    _isQuitConfirmOpened = false;
                };
            }
        }

        private void VideoButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/VideoPage.xaml", UriKind.Relative));
        }

        private void HorizontalFlipView_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var item = ((FlipView)sender).SelectedItem as NewsListInfo;
            if (item != null)
            {
                if (_newsCategoryPopup.IsOpen)
                {
                    _newsCategoryPopup.Hide();
                    SetAppbarForNewsList();
                }

                NavigationService.Navigate(new Uri("/NewsDetailPage.xaml?newsId=" + item.Id, UriKind.Relative));
            }
        }
    }
}