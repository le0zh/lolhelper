using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace LolWikiApp
{
    public class ImageSourceConverter:IValueConverter
    {
        private const string DefaultImagePath = "/Data/default@2x.png";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var imgUri = value as string;
            BitmapSource bitmap = string.IsNullOrEmpty(imgUri) ? new BitmapImage(new Uri(DefaultImagePath, UriKind.Relative))
                                                               : new BitmapImage(new Uri(imgUri, UriKind.RelativeOrAbsolute));

            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
