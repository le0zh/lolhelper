using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coding4Fun.Toolkit.Controls;

namespace LolWikiApp
{public class ToastPromt
    {
        public static ToastPrompt GetToastWithImgAndTitle(string message)
        {
            return new ToastPrompt
            {
                Title = "提示",
                TextOrientation = System.Windows.Controls.Orientation.Vertical,
                Message = message
                //ImageSource = new BitmapImage(new Uri("../../logo_62.png", UriKind.RelativeOrAbsolute))
            };
        }
    }
}
