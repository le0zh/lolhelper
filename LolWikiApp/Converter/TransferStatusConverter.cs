using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Microsoft.Phone.Reactive;

namespace LolWikiApp
{
    public class SelectedItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var oldVal = (int)value;
            return oldVal + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TransferStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (VideoDownloadTransferStatus)value;
         
            var statusDisplay = string.Empty;
            switch (status)
            {
                case VideoDownloadTransferStatus.Completed:
                    statusDisplay = "下载完成";
                    break;
                case VideoDownloadTransferStatus.Transfering:
                    statusDisplay = "正在下载: ";
                    break;
                case VideoDownloadTransferStatus.Paused:
                    statusDisplay = "暂停";
                    break;
                case VideoDownloadTransferStatus.Error:
                    statusDisplay = "下载被中断";
                    break;
            }

            return statusDisplay;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
