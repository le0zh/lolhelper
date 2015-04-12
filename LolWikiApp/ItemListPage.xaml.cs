using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using LolWikiApp.Repository;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace LolWikiApp
{
    public partial class ItemListPage : PhoneApplicationPage
    {
        public ItemListPage()
        {
            InitializeComponent();

            _itemRepository = new ItemRepository();
            _data = new List<Item>();
        }

        private readonly ItemRepository _itemRepository;

        //类别
        private string _tag;
        private bool _isPostback;
        private string _tagName;

        private readonly List<Item> _data;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_isPostback) return;

            _isPostback = true;

            _data.Clear();

            if (NavigationContext.QueryString.TryGetValue("tag", out _tag) && NavigationContext.QueryString.TryGetValue("tagName", out _tagName))
            {
                MianPivotItem.Header = _tagName;

                var list = await _itemRepository.GetItemsByCategoryAsync(_tag);

                if (list == null)
                {
                    WrapperScrollViewer.Visibility = Visibility.Collapsed;
                    NoDataBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    WrapperScrollViewer.Visibility = Visibility.Visible;
                    NoDataBlock.Visibility = Visibility.Collapsed;

                    foreach (var item in list)
                    {
                        _data.Add(item);
                        AddItemItem(item);
                    }
                }
            }
        }

        private void AddItemItem(Item item)
        {
            if (item == null)
                return;

            var stackPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Vertical,
                Margin = new Thickness(0, 8, 32, 8)
            };

            var img = new Image
            {
                Source = new BitmapImage(new Uri(item.ImageUrl, UriKind.Relative)),
                Height = 90,
                Width = 90,//90
                Stretch = Stretch.UniformToFill
            };

            //img.Tap += (s, e) => NavigationService.Navigate(new Uri("/ItemDetailPage.xaml?itemId=" + item.id, UriKind.Relative));

            var textBlock = new TextBlock();
            var tmpTitle = item.text.Length > 4 ? item.text.Substring(0, 4) + ".." : item.text;
            textBlock.Text = tmpTitle;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.FontSize = (double)Application.Current.Resources["PhoneFontSizeSmall"];
            textBlock.Foreground = new SolidColorBrush(Colors.Black);

            stackPanel.Children.Add(img);
            stackPanel.Children.Add(textBlock);

            stackPanel.Tap += (s, e) =>
            {
                var helper = new AnimatonHelper();
                helper.RunShowStoryboard(stackPanel, AnimationTypes.Flash, TimeSpan.FromSeconds(0), (s1, e1) => NavigationService.Navigate(new Uri("/ItemDetailPage.xaml?itemId=" + item.id, UriKind.Relative)));
            };

            ItemWrapPanel.Children.Add(stackPanel);
        }


        private void SearchButton_OnTap(object sender, GestureEventArgs e)
        {
            Search();
        }

        private void Search()
        {
            var keywords = KeyWordsTextBox.Text.Trim();

            List<Item> list;
            if (string.IsNullOrEmpty(keywords))
            {
                list = _data;
            }
            else
            {
                ItemWrapPanel.Children.Clear();

                var query = from s in _data
                            where s.text.Contains(keywords)
                            select s;

                list = query.ToList();
            }

            if (list.Count == 0)
            {
                WrapperScrollViewer.Visibility = Visibility.Collapsed;
                NoDataBlock.Visibility = Visibility.Visible;
            }
            else
            {
                WrapperScrollViewer.Visibility = Visibility.Visible;
                NoDataBlock.Visibility = Visibility.Collapsed;

                foreach (var item in list)
                {
                    AddItemItem(item);
                }
            }
        }

        private void KeyWordsTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
                WrapperScrollViewer.Focus();
                //this.SearchButton.Focus();
            }
        }

        private void KeyWordsTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Search();
        }
    }
}