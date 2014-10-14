using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Windows.Storage;

namespace LolWikiApp
{
    public class VideoDownloader : INotifyPropertyChanged
    {
        private const string VideoCacheFolerName = "VideoCache";

        public string FileName { get; set; }

        public string SourceUrl { get; set; }

        public string SizeDisplay { get; set; }

        /// <summary>
        /// 下载完毕的事件通知
        /// </summary>
        public EventHandler DownloadCompletedEventHandler;
        protected void OndDownloadCompleted()
        {
            if (DownloadCompletedEventHandler != null)
            {
                DownloadCompletedEventHandler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 下载中遇到错误的事件通知
        /// </summary>
        public EventHandler ErrorEncounteredEventHandler;
        protected void OnErrorEncountered()
        {
            if (ErrorEncounteredEventHandler != null)
            {
                ErrorEncounteredEventHandler(this, EventArgs.Empty);
            }
        }

        public VideoDownloader(string fileName, string url)
        {
            FileName = fileName;
            SourceUrl = url;
        }

        public async void DownloadAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(SourceUrl))
                throw new ArgumentException("FileName or SourceUrl is not setted for video downloader.");

            var localFolder = ApplicationData.Current.LocalFolder;
            var newsCacheRootFolder = await localFolder.CreateFolderAsync(VideoCacheFolerName, CreationCollisionOption.OpenIfExists);

            var localFile = await newsCacheRootFolder.CreateFileAsync(FileName, CreationCollisionOption.OpenIfExists);
            var lStartPos = 0L;
            using (var fs = await localFile.OpenStreamForWriteAsync())
            {
                lStartPos = fs.Length;
            }

            try
            {
                var myrq = WebRequest.CreateHttp(SourceUrl);
                myrq.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; .NET CLR 1.0.3705)";

                if (lStartPos > 0)
                {
                    myrq.Headers["bytes"] = lStartPos.ToString();
                    Debug.WriteLine("lStartPos: " + lStartPos);
                }

                myrq.BeginGetResponse(async (result) =>
                {
                    var response = myrq.EndGetResponse(result);
                    GetResponseCallback(cancellationToken, response, lStartPos, FileName);
                }, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void GetResponseCallback(CancellationToken cancellationToken, WebResponse response, long lStartPos, string fileName)
        {
            Debug.WriteLine(response.Headers["Accept-Ranges"]);

            var totalBytes = response.ContentLength + lStartPos;
            Debug.WriteLine("totalBytes: " + totalBytes);
            var localFolder = ApplicationData.Current.LocalFolder;

            var newsCacheRootFolder = await localFolder.CreateFolderAsync(VideoCacheFolerName, CreationCollisionOption.OpenIfExists);

            var localFile = await newsCacheRootFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

            using (var fs = await localFile.OpenStreamForWriteAsync())
            using (var responseStream = response.GetResponseStream())
            {
                if (lStartPos > 0)
                {
                    fs.Seek(lStartPos, SeekOrigin.Current);
                }

                var totalDownloadedByte = lStartPos;
                var by = new byte[1024];
                if (responseStream != null)
                {
                    Debug.WriteLine("responseStream != null");
                    var startTime = DateTime.Now;
                    var osize = responseStream.Read(by, 0, by.Length);
                    var tmpsize = 0;

                    while (osize > 0)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            //Cancell operation is requested.
                            Debug.WriteLine("cancelled" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                            return;
                        }

                        totalDownloadedByte = osize + totalDownloadedByte;
                        var endTime = DateTime.Now;
                        Debug.WriteLine("fs.write: " + osize);
                        fs.Write(by, 0, osize);

                        var downloadedByte = totalDownloadedByte;
                        tmpsize += osize;
                        var time = startTime;
                        var tmpsize1 = tmpsize;

                        var totalSeconds = (endTime - time).TotalSeconds;
                        if (totalSeconds > 0.5)
                        {
                            var seepText = string.Format("{0:F}", tmpsize1 / totalSeconds / 1024);
                            Debug.WriteLine(seepText + " kb/s");
                            tmpsize = 0;
                            startTime = DateTime.Now;
                        }
                        Debug.WriteLine((int)downloadedByte);

                        osize = responseStream.Read(by, 0, by.Length);
                    }
                }
                fs.Close();
                if (responseStream != null) responseStream.Close();

                OndDownloadCompleted();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
