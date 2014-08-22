using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Coding4Fun.Toolkit.Controls;

namespace LolWikiApp
{
    public class ToastPromts
    {
        public static ToastPrompt GetToastWithImgAndTitle(string message)
        {
            //var frameWidth = Application.Current.Host.Content.ActualWidth;

            return new ToastPrompt
            {
                TextOrientation = System.Windows.Controls.Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Center,
                Stretch = Stretch.UniformToFill,
                Message = message,
                //Width = frameWidth,
                Margin = new Thickness(0)
                //ImageSource = new BitmapImage(new Uri("../../logo_62.png", UriKind.RelativeOrAbsolute))
            };
        }
    }
}
