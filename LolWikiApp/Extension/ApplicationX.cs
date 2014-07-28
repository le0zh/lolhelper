using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LolWikiApp
{
    public enum Theme
    {
        Light,
        Dark
    }

    public static class ApplicationX
    {
        public static Theme GetTheme(this Application application)
        {
            var visibility = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"];
            return (visibility == Visibility.Visible) ? Theme.Light : Theme.Dark;
        } 
    }
}
