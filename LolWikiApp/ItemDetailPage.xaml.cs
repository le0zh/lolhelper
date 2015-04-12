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
    public partial class ItemDetailPage : PhoneApplicationPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();

            _itemRepository = new ItemRepository();
        }

        private bool _isPostback;
        private string _itemId;
        private readonly ItemRepository _itemRepository;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_isPostback) return;

            _isPostback = true;

            if (NavigationContext.QueryString.TryGetValue("itemId", out _itemId))
            {
                var detail = await _itemRepository.GetItemDetailByIdAsync(_itemId);
                DataContext = detail;

                foreach (var need in detail.NeedList)
                {
                    AddItemToWrappanel(need, NeedItemWrapPanel);
                }

                foreach (var compose in detail.ComposeList)
                {
                    AddItemToWrappanel(compose, ComposeItemWrapPanel);
                }
            }
        }

        private void AddItemToWrappanel(string id, WrapPanel wrapPanel)
        {
            var stackPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Vertical,
                Margin = new Thickness(0, 8, 32, 8)
            };

            var img = new Image
            {
                Source = new BitmapImage(new Uri("/Data/items/item_img/"+ id+ ".png", UriKind.Relative)),
                Height = 90,
                Width = 90,//90
                Stretch = Stretch.UniformToFill
            };

            //img.Tap += (s, e) => NavigationService.Navigate(new Uri("/ItemDetailPage.xaml?itemId=" + id, UriKind.Relative));

           
            stackPanel.Children.Add(img);

            stackPanel.Tap += (s, e) =>
            {
                var helper = new AnimatonHelper();
                helper.RunShowStoryboard(stackPanel, AnimationTypes.Flash, TimeSpan.FromSeconds(0), (s1, e1) => NavigationService.Navigate(new Uri("/ItemDetailPage.xaml?itemId=" + id, UriKind.Relative)));
            };

            wrapPanel.Children.Add(stackPanel);
        }
    }
}