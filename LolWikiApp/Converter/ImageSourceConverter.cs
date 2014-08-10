using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Windows.Storage;
using LolWikiApp.Repository;

namespace LolWikiApp
{
    public class ImageSourceConverter:IValueConverter
    {
        private const string DefaultImagePath = "/Data/default@2x.png";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var imgUri = value as string;
            var localFileRepository = new LocalFileRepository();


            BitmapSource bitmap = new BitmapImage();

            if (imgUri.StartsWith("iso::"))
            {
                //const string imgUrl = "115.28.105.168_thumb_lol_news_16000_16499_16241_183.jpg";

                Debug.WriteLine("img: " + imgUri.Substring(5));
                localFileRepository.SetBitmapSource(imgUri.Substring(5),bitmap);
                //localFileRepository.SetBitmapSource(imgUrl, bitmap);
            }
            else
            {
                bitmap = string.IsNullOrEmpty(imgUri) ? new BitmapImage(new Uri(DefaultImagePath, UriKind.Relative))
                                                   : new BitmapImage(new Uri(imgUri, UriKind.RelativeOrAbsolute));
            }

            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
