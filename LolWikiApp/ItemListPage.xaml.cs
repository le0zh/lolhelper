using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using LolWikiApp.Repository;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace LolWikiApp
{
    public partial class ItemListPage : PhoneApplicationPage
    {
        public ItemListPage()
        {
            InitializeComponent();

            _itemRepository = new ItemRepository();
        }

        private readonly ItemRepository _itemRepository;

        //类别
        private string _tag;

        private string _tagName;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

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
    }
}