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
    public class ImageSourceConverter : IValueConverter
    {
        private const string DefaultImagePath = "/Data/default@2x.png";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var imgUri = value as string;
            var localFileRepository = new LocalFileRepository();


            BitmapSource bitmap = new BitmapImage();

            if (imgUri != null && imgUri.StartsWith("iso::"))
            {
                var fileName = imgUri.Substring(5);
                if (!string.IsNullOrEmpty(fileName))
                {
                    localFileRepository.SetBitmapSource(imgUri.Substring(5), bitmap);
                }
                else
                {
                    bitmap = new BitmapImage(new Uri(@"data\default@2x.png", UriKind.Relative));
                }
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

    public class LevelImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var title = value as string ?? "无";
            BitmapImage bitmap;
            try
            {
                if (title.Contains("黄铜"))
                {
                    bitmap = new BitmapImage(new Uri(@"data\level\huangtong.png", UriKind.Relative));
                }
                else if (title.Contains("黄金"))
                {
                    bitmap = new BitmapImage(new Uri(@"data\level\huangjing.png", UriKind.Relative));
                }

                else if (title.Contains("超凡大师"))
                {
                    bitmap = new BitmapImage(new Uri(@"data\level\master.png", UriKind.Relative));
                }
                else if (title.Contains("铂金"))
                {
                    bitmap = new BitmapImage(new Uri(@"data\level\bojin.png", UriKind.Relative));
                }
                else if (title.Contains("钻石"))
                {
                    bitmap = new BitmapImage(new Uri(@"data\level\zhuanshi.png", UriKind.Relative));
                }
                else if (title.Contains("白银"))
                {
                    bitmap = new BitmapImage(new Uri(@"data\level\baiyin.png", UriKind.Relative));
                }
                else if (title.Contains("最强王者"))
                {
                    bitmap = new BitmapImage(new Uri(@"data\level\zuiqiangwangzhe.png", UriKind.Relative));
                }
                else
                {
                    bitmap = new BitmapImage(new Uri(@"data\level\none.png", UriKind.Relative));
                }
            }
            catch (Exception)
            {
                bitmap = new BitmapImage(new Uri(@"data\level\无.png", UriKind.Relative));
            }

            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
   
}
