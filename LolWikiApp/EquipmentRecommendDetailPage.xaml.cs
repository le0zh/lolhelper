using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace LolWikiApp
{
    public partial class EquipmentRecommendDetailPage : PhoneApplicationPage
    {
        public EquipmentRecommendDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.ViewModel.EquipmentRecommendSelected != null)
            {
                this.DataContext = App.ViewModel.EquipmentRecommendSelected;
            }
            else
            {
                //TODO: 处理异常，推荐出装的实体为null
            }
        }
    }
}