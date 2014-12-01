using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
                Stretch = Stretch.Fill,
                Message = message,
                //ImageSource = new BitmapImage(new Uri("/Data/logo.png", UriKind.Relative)),
                //Background = new SolidColorBrush(Color.FromArgb(255, 41, 40, 46)),
                Background = new SolidColorBrush(Colors.Red),
                AnimationType = Clarity.Phone.Extensions.DialogService.AnimationTypes.Vetical,
                Margin = new Thickness(0, 0, 0, 0)
            };
        }
    }
}
