﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.Foundation.Metadata;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace LolWikiApp
{
    public partial class AllHeroPage : PhoneApplicationPage
    {
        private readonly string[] _heroTags = { "All", "Fighter", "Mage", "Assassin", "Tank", "Marksman", "Support" };
        private readonly WrapPanel[] _heroPanels;
        private bool _isPostback;

        private readonly Border[] _nodataBorders;
        private readonly TextBox[] _keyWordsTextBoxs;
        public AllHeroPage()
        {
            InitializeComponent();

            _heroPanels = new WrapPanel[] { 
                this.AllHeroWrapPanel, 
                this.FighterHeroWrapPanel, this.MageHeroWrapPanel, 
                this.AssassinHeroWrapPanel, this.TankHeroWrapPanel, 
                this.MarksmanHeroWrapPanel, this.SupportHeroWrapPanel };

            _nodataBorders = new Border[]
            {
                NoDataBlockAll,
                NoDataBlockZhanshi,
                NoDataBlockFashi,
                NoDataBlockCike,
                NoDataBlockTangke,
                NoDataBlockSheshou,
                NoDataBlockFuzhu
            };

            _keyWordsTextBoxs = new TextBox[]
            {
                AllKeyWordsTextBox,
                ZhanshiKeyWordsTextBox,
                FashiKeyWordsTextBox,
                CikeKeyWordsTextBox,
                TangkeKeyWordsTextBox,
                SheshouKeyWordsTextBox,
                FuzhuKeyWordsTextBox
            };
        }


        private async void LoadHeroList(int index)
        {
            if (index < 0 || index > 6)
                return;

            if (_heroPanels[index].Children.Count > 0)
                return;

            //SystemTray.ProgressIndicator.IsVisible = true;

            if (!App.ViewModel.IsDataLoaded)
            {
                await App.ViewModel.LoadHeroBaiscInfoDataAsync();
            }

            var t = new Task(() => this.Dispatcher.BeginInvoke(() =>
            {
                foreach (Hero hero in (
                                    index == 0 ? App.ViewModel.HeroBasicInfoCollection
                                               : App.ViewModel.HeroBasicInfoCollection.Where(h => h.Tags.Contains(_heroTags[index]))))
                {
                    AddFreeHeroItem(hero, _heroPanels[index]);
                }

                //SystemTray.ProgressIndicator.IsVisible = false;
            }));
            t.Start();

            //foreach (Hero hero in (index == 0 ? App.ViewModel.HeroBasicInfoCollection
            //                                   : App.ViewModel.HeroBasicInfoCollection.Where(h => h.Tags.Contains(heroTags[index]))))
            //{
            //    AddFreeHeroItem(hero, heroPanels[index]);
            //}

            //Debug.WriteLine("index:" + index.ToString());

            //foreach (Hero hero in
            //    (index == 0 ? App.ViewModel.HeroBasicInfoCollection
            //                : App.ViewModel.HeroBasicInfoCollection.Where(h => h.Tags.Contains(heroTags[index]))))
            //{
            //    AddFreeHeroItem(hero, heroPanels[index]);
            //}
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_isPostback) return;

            _isPostback = true;
            base.OnNavigatedTo(e);
        }

        private void AddFreeHeroItem(Hero hero, WrapPanel wrapPanel)
        {
            if (hero == null)
                return;

            var stackPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Vertical,
                Margin = new Thickness(0, 8, 32, 8)
            };

            var img = new Image
            {
                Source = new BitmapImage(new Uri(hero.ImageUrl, UriKind.Relative)),
                Height = 90,
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

            wrapPanel.Children.Add(stackPanel);
        }

        private void HeroPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadHeroList(this.HeroPivot.SelectedIndex);
        }

        private void Search()
        {
            var index = HeroPivot.SelectedIndex;

            var keyWords = _keyWordsTextBoxs[index].Text.Trim().ToLower();

            List<Hero> list;

            if (string.IsNullOrEmpty(keyWords))
            {
                list = index == 0 ? App.ViewModel.HeroBasicInfoCollection.ToList() : App.ViewModel.HeroBasicInfoCollection.Where(h => h.Tags.Contains(_heroTags[index])).ToList();
            }
            else
            {
                list = index == 0 ? App.ViewModel.HeroBasicInfoCollection.Where(
                        h => h.Name.Contains(keyWords) || h.Title.Contains(keyWords) || h.Id.ToLower().Contains(keyWords))
                        .ToList() :
                    App.ViewModel.HeroBasicInfoCollection.Where(h => h.Tags.Contains(_heroTags[index])).Where(
                        h => h.Name.Contains(keyWords) || h.Title.Contains(keyWords) || h.Id.ToLower().Contains(keyWords))
                        .ToList();
            }

            _heroPanels[index].Children.Clear();

            if (list.Count == 0)
            {
                _nodataBorders[index].Visibility = Visibility.Visible;
                _heroPanels[index].Visibility = Visibility.Collapsed;
            }
            else
            {
                _nodataBorders[index].Visibility = Visibility.Collapsed;
                _heroPanels[index].Visibility = Visibility.Visible;

                foreach (var hero in list)
                {
                    AddFreeHeroItem(hero, _heroPanels[index]);
                }
            }
        }

        //搜索按钮的响应事件
        private void SearchButton_OnTap(object sender, GestureEventArgs e)
        {
            Search();
        }

        private void KeyWordsTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
                HeroPivot.Focus();
            }
        }

        private void KeyWordsTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Search();
        }
    }
}