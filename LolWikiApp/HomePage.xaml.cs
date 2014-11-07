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
        private int _currentPageForFunyNews;
        private int _currentPageForStoryNews;
        private int _currentPageForMmNews;
        private NewsType _currentNewsType;
        private bool _isPivotFirstLoaded = false;
        private readonly FullScreenPopup _newsCategoryPopup;
        private bool _isQuitConfirmOpened = false;

        public HomePage()
        {
            InitializeComponent();
            _freeHeros = new List<Hero>();
            _currentPage = 0;
            _currentPageForFunyNews = 1;
            _currentNewsType = NewsType.Latest;
            _newsCategoryPopup = new FullScreenPopup();
            _newsCategoryPopup.PopUpHided += (s, e) => SetAppbarForNewsList();
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
                case 1: //搞笑
                    FunnyNewsBind();
                    break;
                case 2: //连载
                    StoryNewsBind();
                    break;
                case 3: //美图
                    MmNewsBind();
                    break;
                case 4: //周免
                    BindFreeHeroInfoAsync();
                    break;
                case 5: //我的
                    BindRecords();
                    break;
                case 6: //更多
                    break;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("HOME PAGE LOAD OnNavigatedTo");
            HomePageMain();

            // Check if ExtendedSplashscreen.xaml is on the backstack  and remove 
            if (NavigationService.BackStack.Count() == 1)
            {
                NavigationService.RemoveBackEntry();
            }

            base.OnNavigatedTo(e);
        }

        private void HomePage_OnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (_newsCategoryPopup.IsOpen)
            {
                _newsCategoryPopup.Hide();
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
                HomePageMain();
            }
        }
        #endregion

        #region 美女
        private void MmNewsBind()
        {
            if (this.TcMmNewsLongListSelector.ItemsSource == null)
            {
                this.TcMmNewsLongListSelector.ItemsSource = App.NewsViewModel.TcMmListInfObservableCollection;
                LoadTcMmNewsData();
            }

            SetAppbarForMmNews();
        }

        private async void LoadTcMmNewsData()
        {
            this.TcMmNewsRetryNetPanel.Visibility = Visibility.Collapsed;
            this.TcMmNewsLoadingBar.Visibility = Visibility.Visible;
            SystemTray.ProgressIndicator.IsVisible = true;

            try
            {
                await App.NewsViewModel.LoadTecentMmListInfosByPageAsync(1, true);
            }
            catch (Exception exception404)
            {
                this.TcMmNewsRetryNetPanel.Visibility = Visibility.Visible;
                return;
            }
            finally
            {
                _currentPageForMmNews = 1;
                this.TcMmNewsLoadingBar.Visibility = Visibility.Collapsed;
                SystemTray.ProgressIndicator.IsVisible = false;
            }

            this.TcMmNewsRetryNetPanel.Visibility = Visibility.Collapsed;
            this.TcMmNewsLongListSelector.Visibility = Visibility.Visible;
        }

        private void SetAppbarForMmNews()
        {
            ApplicationBar = new ApplicationBar { Opacity = 0.8 };

            var refreshButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative),
                Text = "刷新"
            };

            refreshButton.Click += (s, e) => LoadTcMmNewsData();

            var aboutMenuItem = new ApplicationBarMenuItem { Text = "关于" };
            aboutMenuItem.Click += (s, e) => NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));

            ApplicationBar.MenuItems.Add(aboutMenuItem);

            ApplicationBar.Buttons.Add(refreshButton);
        }

        private async void TcMmNewsLongListSelector_OnGettingMoreTriggered(object sender, EventArgs e)
        {
            TcNewsListInfo lastNews = null;
            var nextPage = _currentPageForMmNews + 1;
            try
            {
                lastNews = App.NewsViewModel.TcMmListInfObservableCollection.Last();
                await App.NewsViewModel.LoadTecentMmListInfosByPageAsync(nextPage);
            }
            catch (Exception exception404)
            {
                ToastPromts.GetToastWithImgAndTitle("网络不太稳定，加载获取失败.").Show();
                return;
            }
            finally
            {
                this.TcMmNewsLongListSelector.HideGettingMorePanel();
            }

            if (lastNews != null)
            {
                this.TcMmNewsLongListSelector.ScrollTo(lastNews);
            }

            _currentPageForMmNews = nextPage;
        }

        private void TcMmNewsLongListSelector_OnTap(object sender, GestureEventArgs e)
        {
            if (this.TcMmNewsLongListSelector.SelectedItem != null)
            {
                var newsInfo = this.TcMmNewsLongListSelector.SelectedItem as TcNewsListInfo;
                if (newsInfo != null)
                {
                    this.TcMmNewsLongListSelector.SelectedItem = null; //reset selected item
                    NavigationService.Navigate(new Uri("/NewsDetailPage.xaml?newsUrl=" + newsInfo.article_url, UriKind.Relative));
                }
            }
        }

        #endregion

        #region 连载

        private void StoryNewsBind()
        {
            if (this.TcStoryNewsLongListSelector.ItemsSource == null)
            {
                this.TcStoryNewsLongListSelector.ItemsSource = App.NewsViewModel.TcStoryListInfObservableCollection;
                LoadTcStoryNewsData();
            }

            SetAppbarForStoryNews();
        }

        private async void LoadTcStoryNewsData()
        {
            this.TcSotryNewsRetryNetPanel.Visibility = Visibility.Collapsed;
            this.TcSotryNewsLoadingBar.Visibility = Visibility.Visible;
            SystemTray.ProgressIndicator.IsVisible = true;

            try
            {
                await App.NewsViewModel.LoadTecentStoryListInfosByPageAsync(1, true);
            }
            catch (Exception exception404)
            {
                this.TcSotryNewsRetryNetPanel.Visibility = Visibility.Visible;
                return;
            }
            finally
            {
                _currentPageForStoryNews = 1;
                this.TcSotryNewsLoadingBar.Visibility = Visibility.Collapsed;
                SystemTray.ProgressIndicator.IsVisible = false;
            }

            this.TcSotryNewsRetryNetPanel.Visibility = Visibility.Collapsed;
            this.TcStoryNewsLongListSelector.Visibility = Visibility.Visible;
        }

        private void SetAppbarForStoryNews()
        {
            ApplicationBar = new ApplicationBar { Opacity = 0.8 };

            var refreshButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative),
                Text = "刷新"
            };

            refreshButton.Click += (s, e) => LoadTcStoryNewsData();

            var aboutMenuItem = new ApplicationBarMenuItem { Text = "关于" };
            aboutMenuItem.Click += (s, e) => NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));

            ApplicationBar.MenuItems.Add(aboutMenuItem);

            ApplicationBar.Buttons.Add(refreshButton);
        }

        private async void TcStoryNewsLongListSelector_OnGettingMoreTriggered(object sender, EventArgs e)
        {
            TcNewsListInfo lastNews = null;
            var nextPage = _currentPageForStoryNews + 1;
            try
            {
                lastNews = App.NewsViewModel.TcStoryListInfObservableCollection.Last();
                await App.NewsViewModel.LoadTecentStoryListInfosByPageAsync(nextPage);
            }
            catch (Exception exception404)
            {
                ToastPromts.GetToastWithImgAndTitle("网络不太稳定，加载获取失败.").Show();
                return;
            }
            finally
            {
                this.TcStoryNewsLongListSelector.HideGettingMorePanel();
            }

            if (lastNews != null)
            {
                this.TcStoryNewsLongListSelector.ScrollTo(lastNews);
            }

            _currentPageForStoryNews = nextPage;
        }
        
        private void TcStoryNewsLongListSelector_OnTap(object sender, GestureEventArgs e)
        {
            if (this.TcStoryNewsLongListSelector.SelectedItem != null)
            {
                var newsInfo = this.TcStoryNewsLongListSelector.SelectedItem as TcNewsListInfo;
                if (newsInfo != null)
                {
                    this.TcStoryNewsLongListSelector.SelectedItem = null; //reset selected item
                    NavigationService.Navigate(new Uri("/NewsDetailPage.xaml?newsUrl=" + newsInfo.article_url, UriKind.Relative));
                }
            }
        }
        
        #endregion


        #region 搞笑

        private void FunnyNewsBind()
        {
            //TcFunnyNewsLongListSelector
            if (this.TcFunnyNewsLongListSelector.ItemsSource == null)
            {
                this.TcFunnyNewsLongListSelector.ItemsSource = App.NewsViewModel.TcNewsListInfObservableCollection;
                LoadTcNewsData();
            }

            SetAppbarForFunnyNews();
        }

        private async void LoadTcNewsData()
        {
            this.TcFunyNewsRetryNetPanel.Visibility = Visibility.Collapsed;
            this.TcFunnyNewsLoadingBar.Visibility = Visibility.Visible;
            SystemTray.ProgressIndicator.IsVisible = true;
            try
            {
                await App.NewsViewModel.LoadTecentNewsListInfosByTypeAndPageAsync(1, true);
            }
            catch (Exception exception404)
            {
                this.TcFunyNewsRetryNetPanel.Visibility = Visibility.Visible;
                return;
            }
            finally
            {
                _currentPageForFunyNews = 1;
                this.TcFunnyNewsLoadingBar.Visibility = Visibility.Collapsed;
                SystemTray.ProgressIndicator.IsVisible = false;
            }

            this.TcFunyNewsRetryNetPanel.Visibility = Visibility.Collapsed;
            this.TcFunnyNewsLongListSelector.Visibility = Visibility.Visible;
        }

        private void SetAppbarForFunnyNews()
        {
            ApplicationBar = new ApplicationBar { Opacity = 0.8 };

            var refreshButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative),
                Text = "刷新"
            };

            refreshButton.Click += (s, e) => LoadTcNewsData();

            var aboutMenuItem = new ApplicationBarMenuItem { Text = "关于" };
            aboutMenuItem.Click += (s, e) => NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));

            ApplicationBar.MenuItems.Add(aboutMenuItem);

            ApplicationBar.Buttons.Add(refreshButton);
        }

        private async void TcFunnyNewsLongListSelector_OnGettingMoreTriggered(object sender, EventArgs e)
        {
            TcNewsListInfo lastNews = null;
            var nextPage = _currentPageForFunyNews + 1;
            try
            {
                lastNews = App.NewsViewModel.TcNewsListInfObservableCollection.Last();
                await App.NewsViewModel.LoadTecentNewsListInfosByTypeAndPageAsync(nextPage);
            }
            catch (Exception exception404)
            {
                ToastPromts.GetToastWithImgAndTitle("网络不太稳定，加载获取失败.").Show();
                return;
            }
            finally
            {
                this.TcFunnyNewsLongListSelector.HideGettingMorePanel();
            }

            if (lastNews != null)
            {
                this.TcFunnyNewsLongListSelector.ScrollTo(lastNews);
            }

            _currentPageForFunyNews = nextPage;
        }

        private void TcFunnyNewsLongListSelector_OnTap(object sender, GestureEventArgs e)
        {
            if (this.TcFunnyNewsLongListSelector.SelectedItem != null)
            {
                var newsInfo = this.TcFunnyNewsLongListSelector.SelectedItem as TcNewsListInfo;
                if (newsInfo != null)
                {
                    this.TcFunnyNewsLongListSelector.SelectedItem = null; //reset selected item
                    NavigationService.Navigate(new Uri("/NewsDetailPage.xaml?newsUrl=" + newsInfo.article_url, UriKind.Relative));
                }
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
            ApplicationBar = new ApplicationBar { Opacity = 0.8 };

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

            var aboutMenuItem = new ApplicationBarMenuItem { Text = "关于" };
            aboutMenuItem.Click += (s, e) => NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));

            ApplicationBar.MenuItems.Add(aboutMenuItem);

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
                Margin = new Thickness(0, 0, 0, 72)
            };

            mainStackPanel.Tap += (s, e) => e.Handled = true;

            var titleTextBlock = new TextBlock()
            {
                Text = "请选择资讯类型 ",
                Foreground = Application.Current.Resources["PhoneTextLowContrastBrush"] as SolidColorBrush,
                FontSize = (double)Application.Current.Resources["PhoneFontSizeLarge"],
                Margin = new Thickness(24, 12, 12, 0),
            };

            var newsTypeListBox = new ListBox
            {
                Height = 335,
                Width = 480,
                ItemsSource = App.NewsViewModel.NewsTypeList,
                Margin = new Thickness(0, 12, 0, 0),
                Padding = new Thickness(0, 4, 0, 0),
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
            };
            cancelButton.Click += (s, e) => _newsCategoryPopup.Hide();

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

            this.NewsRetryNetPanel.Visibility = Visibility.Collapsed;
            this.NewsLoadingBar.Visibility = Visibility.Visible;
            SystemTray.ProgressIndicator.IsVisible = true;
            try
            {
                Debug.WriteLine(_currentNewsType);

                await App.NewsViewModel.LoadNewsListInfosByTypeAndPageAsync(_currentNewsType, 1, true);
            }
            catch (Exception exception404)
            {
                var toast = ToastPromts.GetToastWithImgAndTitle("加载失败，读取离线文章。");
                toast.Show();

                App.NewsViewModel.LoadeNewsListInfoFromCache(_currentNewsType);
                if (App.NewsViewModel.NewsListInfObservableCollection.Count > 0)
                {
                    this.NewsLongListSelector.Visibility = Visibility.Visible;
                }
                else
                {
                    this.NewsRetryNetPanel.Visibility = Visibility.Visible;
                }

                return;
            }
            finally
            {
                _currentPage = 1;
                this.NewsLoadingBar.Visibility = Visibility.Collapsed;
                SystemTray.ProgressIndicator.IsVisible = false;
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
                        }

                        this.NewsLongListSelector.SelectedItem = null; //reset selected item
                        NavigationService.Navigate(new Uri("/NewsDetailPage.xaml?newsId=" + newsInfo.Id, UriKind.Relative));
                    }
                }
            }
        }

        private void NewsLongListSelector_OnRefreshTriggered(object sender, EventArgs e)
        {
            NewsDataBindAsync();
            //LoadNewsData();
            this.NewsLongListSelector.HideRefreshPanel();
        }

        private void CancelNewsListGetMoreButton_OnClick(object sender, RoutedEventArgs e)
        {
            //NewsListGetMoreRetryNetPanel.Visibility = Visibility.Collapsed;
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
            try
            {
                var nextPage = _currentPage + 1;
                Debug.WriteLine("[news]nextpage: " + nextPage);

                lastNews = App.NewsViewModel.NewsListInfObservableCollection.Last();
                Debug.WriteLine("load more news : " + _currentNewsType);
                await App.NewsViewModel.LoadNewsListInfosByTypeAndPageAsync(_currentNewsType, nextPage);
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

            _currentPage = App.NewsViewModel.CurrentPage;
        }

        private void HorizontalFlipView_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var item = ((FlipView)sender).SelectedItem as NewsListInfo;
            if (item != null)
            {
                if (_newsCategoryPopup.IsOpen)
                {
                    _newsCategoryPopup.Hide();
                    //SetAppbarForNewsList();
                }

                NavigationService.Navigate(new Uri("/NewsDetailPage.xaml?newsId=" + item.Id, UriKind.Relative));
            }
        }
        #endregion

        #region 每周免费英雄
        private void BindFreeHeroInfoAsync()
        {
            ApplicationBar = new ApplicationBar { Opacity = 0.8 };
            var refreshButton = new ApplicationBarIconButton();
            var moreButton = new ApplicationBarIconButton();

            refreshButton.IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative);
            refreshButton.Text = "刷新";

            moreButton.IconUri = new Uri("/Assets/AppBar/more-1.png", UriKind.Relative);
            moreButton.Text = "全部英雄";

            moreButton.Click += (s, e) => NavigationService.Navigate(new Uri("/AllHeroPage.xaml", UriKind.Relative));
            refreshButton.Click += (s1, e1) => RefreshFreeHeroList(true);

            var aboutMenuItem = new ApplicationBarMenuItem { Text = "关于" };
            aboutMenuItem.Click += (s, e) => NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));

            ApplicationBar.MenuItems.Add(aboutMenuItem);

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

        private int GetHeroKeyById(string id)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadHeroBaiscInfoDataAsync();
            }

            var hero = App.ViewModel.HeroBasicInfoCollection.First(h => h.Id == id);
            var key = 0;
            if (hero != null)
            {
                key = hero.Key;
            }

            return key;
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
            img.Tap += (s, e) => NavigationService.Navigate(new Uri("/HeroDetailsPage.xaml?selectedId=" + hero.Id, UriKind.Relative));

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

            HeroWrapPanel.Children.Add(stackPanel);
        }
        #endregion

        #region 战绩
        private void SetBindAppBar()
        {
            ApplicationBar = new ApplicationBar { Opacity = 1.0 };
            var pinButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/pin-1.png", UriKind.Relative),
                Text = "绑定"
            };

            pinButton.Click += (s, e) => NavigationService.Navigate(new Uri("/PlayerInformationPage.xaml?mode=bind", UriKind.Relative));

            var aboutMenuItem = new ApplicationBarMenuItem { Text = "关于" };
            aboutMenuItem.Click += (s, e) => NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));

            ApplicationBar.MenuItems.Add(aboutMenuItem);

            ApplicationBar.Buttons.Add(pinButton);
        }

        private void SetRetryAppBar()
        {
            ApplicationBar = new ApplicationBar { Opacity = 1.0 };
            var retryButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/sync.png", UriKind.Relative),
                Text = "重试"
            };

            retryButton.Click += (s, e) => BindRecords(); ;

            var aboutMenuItem = new ApplicationBarMenuItem { Text = "关于" };
            aboutMenuItem.Click += (s, e) => NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));

            ApplicationBar.MenuItems.Add(aboutMenuItem);

            ApplicationBar.Buttons.Add(retryButton);
        }

        private void SetMoreUnBindAndSearchAppBar()
        {
            ApplicationBar = new ApplicationBar { Opacity = 1.0 };
            var moreButton = new ApplicationBarIconButton();
            var unpinButton = new ApplicationBarIconButton();
            var searchButton = new ApplicationBarIconButton();

            moreButton.IconUri = new Uri("/Assets/AppBar/more-1.png", UriKind.Relative);
            moreButton.Text = "更多信息";

            unpinButton.IconUri = new Uri("/Assets/AppBar/unpin-1.png", UriKind.Relative);
            unpinButton.Text = "解除绑定";

            searchButton.IconUri = new Uri("/Assets/AppBar/feature.search.png", UriKind.Relative);
            searchButton.Text = "搜索召唤师";

            moreButton.Click += (s, e) => MoreAboutPlayer();

            unpinButton.Click += (s2, e2) => RemoveBindOfPlayer();

            searchButton.Click += (s3, e3) => SearchPlayer();

            var aboutMenuItem = new ApplicationBarMenuItem { Text = "关于" };
            aboutMenuItem.Click += (s, e) => NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));

            ApplicationBar.MenuItems.Add(aboutMenuItem);

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

                SetBindAppBar();
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
                    SetMoreUnBindAndSearchAppBar();
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
                    SetRetryAppBar();
                    break;
                case ActionResult.NotFound:
                    NeedBindPlayerInfoPanel.Visibility = Visibility.Visible;
                    PlayerRecordPanel.Visibility = Visibility.Collapsed;
                    SetBindAppBar();
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
                    SetMoreUnBindAndSearchAppBar();
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

        private void RateButton_OnClick(object sender, RoutedEventArgs e)
        {
            var reviewTask = new MarketplaceReviewTask();
            reviewTask.Show();
        }

        private void VideoButton_OnClick(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new Uri("/VideoPage.xaml", UriKind.Relative));
            NavigationService.Navigate(new Uri("/LetvVideoPage.xaml", UriKind.Relative));
        }

        private void HuangliButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewsDetailPage.xaml?fullUrl=http://lol.qq.com/lolApp/news/lolhuangli.htm", UriKind.Relative));
        }
        #endregion

        private void LongListSelector_OnListScrollingUp(object sender, EventArgs e)
        {
            Debug.WriteLine("visible:false");
            PivotTitlContainer.Visibility = Visibility.Collapsed;
            //visible:false
        }

        private void LongListSelector_OnListScrollingDown(object sender, EventArgs e)
        {
            Debug.WriteLine("visible:true");
            PivotTitlContainer.Visibility = Visibility.Visible;
            //visible:true
        }



    }
}