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
                Background = new SolidColorBrush(Color.FromArgb(255, 41, 40, 46)),
                AnimationType = Clarity.Phone.Extensions.DialogService.AnimationTypes.SlideHorizontal,
                Margin = new Thickness(-8, 0, 0, 0),
                IsAppBarVisible = true
            };
        }
    }
}
