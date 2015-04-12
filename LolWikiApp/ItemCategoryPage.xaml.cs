using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using LolWikiApp.Repository;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace LolWikiApp
{
    public partial class ItemCategoryPage : PhoneApplicationPage
    {
        private readonly ItemRepository _itemRepository;
        private bool _isPostback;

        public ItemCategoryPage()
        {
            InitializeComponent();
            _itemRepository= new ItemRepository();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if(_isPostback) return;

            _isPostback = true;
            var list = await _itemRepository.GetItemCategoryAsync();

            CategoryLongListSelector.ItemsSource = list;
        }

        private void CategoryLongListSelector_OnTap(object sender, GestureEventArgs e)
        {
            if (CategoryLongListSelector.SelectedItem != null)
            {
                var category = CategoryLongListSelector.SelectedItem as ItemCategory;
                if (category != null)
                {
                    CategoryLongListSelector.SelectedItem = null;
                    NavigationService.Navigate(new Uri("/ItemListPage.xaml?tag=" + category.tag + "&tagName=" + category.text, UriKind.Relative));
                }
            }
        }
    }
}