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
        private const string VideoCacheFolderName = "VideoCache";
        private const string CachingVideoInfosFileName = "CachingVideoInfos.json";
        private const string CachedVideoInfosFileName = "CachedVideoInfos.json";
        private bool _isDataLoaded = false; //标识视频缓存数据是否被加载

        private ObjectPersistentHelper<List<CachedVideoInfo>> _persistentHelper;

        public ObservableCollection<VideoDownloadRequest> Requests { get; private set; }

        /// <summary>
        /// 还在缓存的视频列表
        /// </summary>
        public List<CachedVideoInfo> CachingVideoInfos { get; private set; }

        /// <summary>
        /// 缓存完成的视频列表
        /// </summary>
        public ObservableCollection<CachedVideoInfo> CachedVideoInfos { get; private set; }

        public VideoDownloadService()
        {
            Requests = new ObservableCollection<VideoDownloadRequest>();
            CachingVideoInfos = new List<CachedVideoInfo>();
            CachedVideoInfos = new ObservableCollection<CachedVideoInfo>();
            _persistentHelper = new ObjectPersistentHelper<List<CachedVideoInfo>>();
        }

        public void AddRequest(VideoDownloadRequest request)
        {
            var foundRequest = Requests.FirstOrDefault(r => r.FileName == request.FileName);
            if (foundRequest == null)
            {
                Requests.Add(request);
                CachingVideoInfos.Add(ConvertToCachedVideoInfo(request));
                //Task.Factory.StartNew(() => request.DownloadAsync(new CancellationToken()));
                request.DownloadAsync(new CancellationToken());
            }
            else
            {
                foundRequest.DownloadAsync(new CancellationToken());
            }
        }

        public void RemoveRequest(VideoDownloadRequest request)
        {
            Requests.Remove(request);
            CachingVideoInfos.RemoveAll(i => i.Title == request.FileName);
            if (request.TransferStatus == VideoDownloadTransferStatus.Completed)
            {
                CachedVideoInfos.Add(ConvertToCachedVideoInfo(request));
            }
        }

        public CachedVideoInfo ConvertToCachedVideoInfo(VideoDownloadRequest request)
        {
            return new CachedVideoInfo()
            {
                Title = request.FileName,
                ImageUrl = request.DisplayUrl,
                Length = request.DisplayLength,
                Src = request.SourceUrl,
                Percent = request.PercentDisplay
            };
        }

        public VideoDownloadRequest ConvertToVideoDownloadRequestInfo(CachedVideoInfo info)
        {
            return new VideoDownloadRequest()
            {
                DisplayLength = info.Length,
                DisplayUrl = info.ImageUrl,
                FileName = info.Title,
                SourceUrl = info.Src,
                PercentDisplay = info.Percent
            };
        }

        public async Task<bool> SaveCachedListToIso()
        {
            var result = await _persistentHelper.Save(CachedVideoInfos.ToList(), VideoCacheFolderName, CachedVideoInfosFileName);
            return result;
        }

        public async Task<bool> SaveCachingListToIso()
        {
            CachingVideoInfos.Clear();
            foreach (var videoDownloadRequest in Requests)
            {
                if (videoDownloadRequest.TransferStatus != VideoDownloadTransferStatus.Completed)
                {
                    CachingVideoInfos.Add(ConvertToCachedVideoInfo(videoDownloadRequest));
                }
            }
            var result = await _persistentHelper.Save(CachingVideoInfos, VideoCacheFolderName, CachingVideoInfosFileName);
            return result;
        }

        public async Task ReadInfoFromIso()
        {
            if (_isDataLoaded == false)
            {
                Debug.WriteLine("load video cache infomation from ISO files.");
                await ReadCachedListFromIso();
                await ReadCachingListFromIso();
                _isDataLoaded = true;
            }
        }

        public async void SaveInfoToIso()
        {
            var r1 = await SaveCachingListToIso();
            var r2 = await SaveCachedListToIso();
            Debug.WriteLine("----------VideoDownloadService.SaveInfoToIso successed");
        }

        public async Task<bool> ReadCachedListFromIso()
        {
            var list = await _persistentHelper.Read(VideoCacheFolderName, CachedVideoInfosFileName);
            if (list == null)
                return false;

            foreach (var cachedVideoInfo in list)
            {
                CachedVideoInfos.Add(cachedVideoInfo);
            }

            foreach (var cachedVideoInfo in CachedVideoInfos)
            {
                var request = ConvertToVideoDownloadRequestInfo(cachedVideoInfo);
                request.TransferStatus = VideoDownloadTransferStatus.Completed;
                Requests.Add(request);
            }

            return true;

        }

        public async Task<bool> ReadCachingListFromIso()
        {
            var list = await _persistentHelper.Read(VideoCacheFolderName, CachingVideoInfosFileName);
            if (list == null)
                return false;

            foreach (var cachedVideoInfo in list)
            {
                CachingVideoInfos.Add(cachedVideoInfo);
            }

            foreach (var cachingVideoInfo in CachingVideoInfos)
            {
                var request = ConvertToVideoDownloadRequestInfo(cachingVideoInfo);
                request.TransferStatus = VideoDownloadTransferStatus.Transfering;
                Requests.Add(request);
            }
            return true;
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
        private const string VideoCacheFolerName = "VideoCache";

        public string DisplayUrl { get; set; }

        public string DisplayLength { get; set; }

        public string FileName { get; set; }

        public string SourceUrl { get; set; }

        public string SizeDisplay { get; set; }

        public object Tag { get; set; }

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
                    NotifyPropertyChanged("TransferStatus");
                    _transferStatus = value;
                }
            }
        }

        /// <summary>
        /// 传输状态发生改变的事件通知
        /// </summary>
        public EventHandler StatusChangedHandler;
        protected void OnStatusChanged()
        {
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
            DisplayLength = videoListInfo.VideoLengthDisplay;
            SourceUrl = url;
            Tag = videoListInfo;
            TransferStatus = VideoDownloadTransferStatus.Paused;
        }

        public async void DownloadAsync(CancellationToken cancellationToken)
        {
            TransferStatus = VideoDownloadTransferStatus.Transfering;
            OnStatusChanged();

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

                myrq.BeginGetResponse((result) =>
                {
                    var response = myrq.EndGetResponse(result);
                    try
                    {
                        GetResponseCallback(cancellationToken, response, lStartPos, FileName);
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
                                Speed = seepText + " kb/s",
                                TotalBytes = totalBytes / 1048576,
                                DownloadedBytes = downloadedByte / 1048576,
                                PercentDisplay = (int)(downloadedByte * 100 / totalBytes)
                            });

                            tmpsize = 0;
                            startTime = DateTime.Now;
                        }
                        //Debug.WriteLine((int)downloadedByte);

                        osize = responseStream.Read(by, 0, by.Length);
                    }
                }
                fs.Close();
                if (responseStream != null) responseStream.Close();

                TransferStatus = VideoDownloadTransferStatus.Completed;
                OnStatusChanged();
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
