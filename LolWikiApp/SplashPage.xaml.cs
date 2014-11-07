using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace LolWikiApp
{
    public partial class SplashPage : PhoneApplicationPage
    {
        public SplashPage()
        {
            InitializeComponent();
            Splash_Screen();
        }
        async void Splash_Screen()
        {
            await Task.Delay(TimeSpan.FromSeconds(3)); // set your desired delay
            NavigationService.Navigate(new Uri("/HomePage.xaml", UriKind.Relative)); // call MainPage
        }
    }
}