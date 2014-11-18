using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Windows.Storage;
using LolWikiApp.Repository;
using Microsoft.Phone.Controls;
using Telerik.Windows.Controls.DataBoundListBox;

namespace LolWikiApp
{
    public enum VideoDownloadTransferStatus
    {
        Transfering,
        Paused,
        Completed,
        Error,
        WaitingForWiFi
    }

    public class VideoDownloadService
    {
        private bool _isDataLoaded; //标识视频缓存数据是否被加载
        private ObjectPersistentHelper<CachedVideoInfo> _persistentHelper;

        public ObservableCollection<VideoDownloadRequest> Requests { get; private set; }

        public VideoDownloadService()
        {
            Requests = new ObservableCollection<VideoDownloadRequest>();
            _persistentHelper = new ObjectPersistentHelper<CachedVideoInfo>();
        }

        public void PauseAll()
        {
            foreach (var videoDownloadRequest in Requests)
            {
                if (videoDownloadRequest.TransferStatus != VideoDownloadTransferStatus.Completed)
                {
                    videoDownloadRequest.CancelDownload();
                    videoDownloadRequest.TransferStatus = VideoDownloadTransferStatus.Paused;
                }
            }            
        }

        public void AddRequest(VideoDownloadRequest request)
        {
            var foundRequest = Requests.FirstOrDefault(r => r.FileName == request.FileName);
            if (foundRequest == null)
            {
                Requests.Add(request);
                request.Download();
            }
            else
            {
                if (foundRequest.TransferStatus != VideoDownloadTransferStatus.Transfering
                    && foundRequest.TransferStatus != VideoDownloadTransferStatus.Completed)
                {
                    foundRequest.Download();
                }
            }
        }

        public void RemoveRequest(VideoDownloadRequest request)
        {
            request.CancelDownload();
            Requests.Remove(request);
            _persistentHelper.Delete(ConstValues.VideoCacheFolderName, request.FileName);
            _persistentHelper.Delete(ConstValues.VideoCacheFolderName, request.FileName + ".json");
        }

        //public CachedVideoInfo ConvertToCachedVideoInfo(VideoDownloadRequest request)
        //{
        //    var cachedVideoInfo = new CachedVideoInfo()
        //    {
        //        Title = request.FileName,
        //        ImageUrl = request.DisplayUrl,
        //        Length = request.DisplayLength,
        //        Src = request.SourceUrl,
        //        Percent = request.PercentDisplay,
        //        Size = request.TotalBytes
        //    };

        //    return cachedVideoInfo;
        //}

        public VideoDownloadRequest ConvertToVideoDownloadRequestInfo(CachedVideoInfo info)
        {
            var request = new VideoDownloadRequest
            {
                DisplayLength = info.Length,
                DisplayUrl = info.ImageUrl,
                FileName = info.Title,
                SourceUrl = info.Src,
                TotalBytes = info.TotalSize//todo:
            };

            return request;
        }

        public async Task ReadCacheInfoFromIso()
        {
            if (_isDataLoaded == false)
            {
                Debug.WriteLine("load video cache infomation from ISO files.");
                var localFolder = ApplicationData.Current.LocalFolder;

                //Read caching video files
                var cachingFolder = await localFolder.CreateFolderAsync(ConstValues.VideoCacheFolderName, CreationCollisionOption.OpenIfExists);
                var cachingFiles = await cachingFolder.GetFilesAsync();
                foreach (var cachingFile in cachingFiles)
                {
                    if (!cachingFile.Name.EndsWith(".json"))
                        continue;

                    var cacheinfo = await _persistentHelper.Read(cachingFile);
                    try
                    {
                        var videoCacheFile = await cachingFolder.GetFileAsync(cacheinfo.Title);
                        var properties = await videoCacheFile.GetBasicPropertiesAsync();
                        var request = ConvertToVideoDownloadRequestInfo(cacheinfo);

                        Debug.WriteLine("properties.Size:{0}, request.TotalBytes: {1}", properties.Size, request.TotalBytes);

                        request.PercentDisplay = request.TotalBytes == 0 ? 0
                                                                         : (int)((long)properties.Size * 100 / request.TotalBytes);
                        request.TransferStatus = request.PercentDisplay == 100
                                                                ? VideoDownloadTransferStatus.Completed
                                                                : VideoDownloadTransferStatus.Paused;

                        Requests.Add(request);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                }
                _isDataLoaded = true;
            }
        }
    }

    public class TransferProgressChangedEventArgs : EventArgs
    {
        public string Speed { get; set; }

        public long TotalBytes { get; set; }

        public long DownloadedBytes { get; set; }

        public int PercentDisplay { get; set; }
    }

    public class VideoDownloadRequest : INotifyPropertyChanged
    {
        private ObjectPersistentHelper<CachedVideoInfo> _persistentHelper = new ObjectPersistentHelper<CachedVideoInfo>();
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public string DisplayUrl { get; set; }

        public string DisplayLength { get; set; }

        public string FileName { get; set; }

        public string SourceUrl { get; set; }

        private string _sizeDisplay;
        public string SizeDisplay
        {
            get
            {
                return !string.IsNullOrEmpty(_sizeDisplay) ? _sizeDisplay : (TotalBytes / 1048576).ToString();
            }
            set { _sizeDisplay = value; }
        }

        public object Tag { get; set; }

        public bool IsDownloading
        {
            get { return PercentDisplay != 100; }
        }

        public bool IsDone
        {
            get { return PercentDisplay == 100; }
        }

        private long _totalBytes;
        public long TotalBytes
        {
            get { return _totalBytes; }
            set
            {
                if (value != _totalBytes)
                {
                    NotifyPropertyChanged("TotalBytes");
                    _totalBytes = value;
                }
            }
        }

        private long _downloadedBytes;
        public long DownloadedBytes
        {
            get { return _downloadedBytes; }
            set
            {
                if (value != _downloadedBytes)
                {
                    NotifyPropertyChanged("DownloadedBytes");
                    _downloadedBytes = value;
                }
            }
        }

        private string _speedDisplay;
        public string SpeedDisplay
        {
            get { return _speedDisplay; }
            set
            {
                if (value != _speedDisplay)
                {
                    NotifyPropertyChanged("SpeedDisplay");
                    _speedDisplay = value;
                }
            }
        }

        private int _percentDisplay;
        public int PercentDisplay
        {
            get { return _percentDisplay; }
            set
            {
                if (value != _percentDisplay)
                {
                    NotifyPropertyChanged("PercentDisplay");
                    _percentDisplay = value;
                }
            }
        }

        private VideoDownloadTransferStatus _transferStatus;
        public VideoDownloadTransferStatus TransferStatus
        {
            get
            {
                return _transferStatus;
            }
            set
            {
                if (value != _transferStatus)
                {
                    _transferStatus = value;
                    NotifyPropertyChanged("TransferStatus");
                }
            }
        }

        /// <summary>
        /// 传输状态发生改变的事件通知
        /// </summary>
        public EventHandler StatusChangedHandler;
        protected void OnStatusChanged()
        {
            if (TransferStatus == VideoDownloadTransferStatus.Completed)
            {
                SizeDisplay = (TotalBytes / 1048576).ToString();
                Debug.WriteLine("SizeDisplay:{0}, TotalBytes:{1}", SizeDisplay, TotalBytes);
                NotifyPropertyChanged("SizeDisplay");
                NotifyPropertyChanged("IsDone");
                NotifyPropertyChanged("IsDownloading");
            }

            if (StatusChangedHandler != null)
            {
                StatusChangedHandler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 传输进度发生改变的事件通知
        /// </summary>
        public EventHandler<TransferProgressChangedEventArgs> TransferProgressChangedHandler;
        protected void OnTransferProgressChanged(TransferProgressChangedEventArgs e)
        {
            SpeedDisplay = e.Speed;
            TotalBytes = e.TotalBytes;
            DownloadedBytes = e.DownloadedBytes;
            PercentDisplay = e.PercentDisplay;

            if (TransferProgressChangedHandler != null)
            {
                TransferProgressChangedHandler(this, e);
            }
        }

        public VideoDownloadRequest() { }

        public VideoDownloadRequest(LetvVideoListInfo videoListInfo, string url)
        {
            FileName = videoListInfo.Title;
            DisplayUrl = videoListInfo.Cover_Url;
            SourceUrl = url;
            DisplayLength = videoListInfo.VideoLengthDisplay;
            Tag = videoListInfo;
            TransferStatus = VideoDownloadTransferStatus.Paused;
        }

        public void CancelDownload()
        {
            _cts.Cancel();
        }

        public async void Download()
        {
            _cts = new CancellationTokenSource();

            //TODO: verify this works or not
            Microsoft.Phone.Shell.PhoneApplicationService.Current.ApplicationIdleDetectionMode = Microsoft.Phone.Shell.IdleDetectionMode.Disabled;

            TransferStatus = VideoDownloadTransferStatus.Transfering;
            OnStatusChanged();

            if (string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(SourceUrl))
                throw new ArgumentException("FileName or SourceUrl is not setted for video downloader.");

            var localFolder = ApplicationData.Current.LocalFolder;
            var newsCacheRootFolder = await localFolder.CreateFolderAsync(ConstValues.VideoCacheFolderName, CreationCollisionOption.OpenIfExists);

            var localFile = await newsCacheRootFolder.CreateFileAsync(FileName, CreationCollisionOption.OpenIfExists);
            var lStartPos = 0L;
            using (var fs = await localFile.OpenStreamForWriteAsync())
            {
                lStartPos = fs.Length;
            }

            try
            {
                var myrq = WebRequest.CreateHttp(SourceUrl);
                myrq.UserAgent = "Mozilla/5.0 (iPhone; U; CPU like Mac OS X) AppleWebKit/420.1 (KHTML, like Gecko) Version/3.0 Mobile/4A93 Safari/419.3 ";
                //myrq.AddRange("bytes", lStartPos);

                if (lStartPos > 0)
                {
                    myrq.Headers["Range"] = "bytes=" + lStartPos + "-";
                    Debug.WriteLine("lStartPos in header: " + lStartPos);
                }
                
                myrq.BeginGetResponse((result) =>
                {
                    var response = myrq.EndGetResponse(result);
                    try
                    {
                        GetResponseCallback(_cts.Token, response, lStartPos, FileName);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        TransferStatus = VideoDownloadTransferStatus.Error;
                        OnStatusChanged();
                    }
                }, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void GetResponseCallback(CancellationToken cancellationToken, WebResponse response, long lStartPos, string fileName)
        {
            Debug.WriteLine("----------------------Accept-Ranges:" + response.Headers["Accept-Ranges"]);

            var totalBytes = response.ContentLength + lStartPos;
            //var totalBytes = response.ContentLength; 
            Debug.WriteLine("--response.ContentLength:{0}", response.ContentLength);
            Debug.WriteLine("--totalBytes:{0}", totalBytes);

            //if temp video file is already there, skip to cache json file .
            if (lStartPos == 0)
            {
                var cacheInfo = new CachedVideoInfo()
                {
                    Title = fileName,
                    ImageUrl = DisplayUrl,
                    Length = DisplayLength,
                    Src = SourceUrl,
                    TotalSize = totalBytes
                };

                await _persistentHelper.Save(cacheInfo, ConstValues.VideoCacheFolderName, fileName + ".json");
            }
           

            var localFolder = ApplicationData.Current.LocalFolder;
            var newsCachingRootFolder = await localFolder.CreateFolderAsync(ConstValues.VideoCacheFolderName, CreationCollisionOption.OpenIfExists);

            var localFile = await newsCachingRootFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

            using (var fs = await localFile.OpenStreamForWriteAsync())
            using (var responseStream = response.GetResponseStream())
            {
                if (lStartPos > 0)
                {
                    fs.Seek(lStartPos, SeekOrigin.Current);
                }

                Debug.WriteLine("lStartPos:{0}", lStartPos);
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
                            //Cancel operation is requested.
                            TransferStatus = VideoDownloadTransferStatus.Paused;
                            SpeedDisplay = string.Empty;

                            OnStatusChanged();

                            Debug.WriteLine("cancelled" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                            return;
                        }

                        totalDownloadedByte = osize + totalDownloadedByte;
                        var endTime = DateTime.Now;
                        //Debug.WriteLine("fs.write: " + osize);
                        fs.Write(by, 0, osize);

                        var downloadedByte = totalDownloadedByte;
                        tmpsize += osize;
                        var time = startTime;
                        var tmpsize1 = tmpsize;

                        var totalSeconds = (endTime - time).TotalSeconds;
                        if (totalSeconds > 0.5)
                        {
                            var seepText = string.Format("{0:F}", tmpsize1 / totalSeconds / 1024);
                            //Debug.WriteLine(seepText + " kb/s");

                            OnTransferProgressChanged(new TransferProgressChangedEventArgs()
                            {
                                Speed = seepText + " KB/s",
                                TotalBytes = totalBytes,
                                DownloadedBytes = downloadedByte,
                                PercentDisplay = (int)(downloadedByte * 100 / totalBytes)
                            });

                            tmpsize = 0;
                            startTime = DateTime.Now;
                        }
                        osize = responseStream.Read(by, 0, by.Length);
                    }
                }
                fs.Close();
                if (responseStream != null) responseStream.Close();

                if (_downloadedBytes / 1048576 == _totalBytes / 1048576)
                {
                    TransferStatus = VideoDownloadTransferStatus.Completed;
                    PercentDisplay = 100;

                    OnStatusChanged();
                }
                else
                {
                    Debug.WriteLine("_downloadedBytes:{0}, _totalBytes:{1}", _downloadedBytes, _totalBytes);
                    SpeedDisplay = string.Empty;
                    TransferStatus = VideoDownloadTransferStatus.Error;
                    OnStatusChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            var handler = PropertyChanged;

            if (handler == null)
                return;

            if (Deployment.Current.Dispatcher.CheckAccess())
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => handler(this, new PropertyChangedEventArgs(propertyName)));
            }
        }
    }
}
