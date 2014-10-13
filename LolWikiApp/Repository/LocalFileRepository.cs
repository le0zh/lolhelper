using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Foundation.Metadata;
using Windows.Storage.Search;
using HtmlAgilityPack;
using System.Threading.Tasks;

using Windows.Storage;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json;

namespace LolWikiApp.Repository
{
    public class LocalFileRepository
    {
        private object _lockobj = new object();
        private const string VideoCacheFolerName = "VideoCache";
        private const string NewsCacheFolerName = "NewsCache";


        public async void SetBitmapSource(string imgName, BitmapSource bitmapSource, string id = "")
        {
            if (string.IsNullOrEmpty(imgName))
                return;

            var localFolder = ApplicationData.Current.LocalFolder;
            var newsCacheRootFolder = await localFolder.CreateFolderAsync("NewsCache", CreationCollisionOption.OpenIfExists);
            StorageFile file;
            if (!string.IsNullOrEmpty(id))
            {
                var htmlFolder = await newsCacheRootFolder.CreateFolderAsync(id, CreationCollisionOption.OpenIfExists);
                file = await htmlFolder.GetFileAsync(imgName);
            }
            else
            {
                file = await newsCacheRootFolder.GetFileAsync(imgName);
            }
            using (var stream = await file.OpenReadAsync())
            {
                bitmapSource.SetSource(stream.AsStreamForRead());
            }
        }

        public async Task<ulong> GetNewsCacheSizeInByte()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var newsCacheRootFolder = await localFolder.CreateFolderAsync("NewsCache", CreationCollisionOption.OpenIfExists);

            var folderProperties = await newsCacheRootFolder.GetBasicPropertiesAsync();
            var totalSize = await GetTotalSizeForFolder(newsCacheRootFolder);

            return totalSize;
        }

        private async Task<ulong> GetTotalSizeForFolder(StorageFolder folder)
        {
            var totalSize = 0UL;

            var files = await folder.GetFilesAsync();
            foreach (var storageFile in files)
            {
                var properties = await storageFile.GetBasicPropertiesAsync();
                totalSize += properties.Size;
            }

            var subFolders = await folder.GetFoldersAsync();
            foreach (var subFolder in subFolders)
            {
                totalSize += await GetTotalSizeForFolder(subFolder);
            }

            return totalSize;
        }

        private async Task<StorageFolder> GetNewsCacheFolderAsync(string id = "")
        {
            var localFolder = ApplicationData.Current.LocalFolder;

            var newsCacheRootFolder = await localFolder.CreateFolderAsync(NewsCacheFolerName, CreationCollisionOption.OpenIfExists);
            if (string.IsNullOrEmpty(id))
            {
                return newsCacheRootFolder;
            }

            var newsCacheFolder =
                    await newsCacheRootFolder.CreateFolderAsync(id, CreationCollisionOption.OpenIfExists);
            return newsCacheFolder;
        }

        public async Task<string> SaveStringToTempHtmlFile(string val)
        {
            const string htmlFileName = "tmp.html";
            var folder = await GetNewsCacheFolderAsync();

            var file = await folder.CreateFileAsync(htmlFileName, CreationCollisionOption.ReplaceExisting);
            var data = Encoding.UTF8.GetBytes(val);

            using (var fs = await file.OpenStreamForWriteAsync())
            {
                await fs.WriteAsync(data, 0, data.Length);
            }

            return Path.Combine("NewsCache", htmlFileName);
        }

        public async Task<bool> CheckNewsIsCachedOrNot(string id)
        {
            var cacheFolder = await GetNewsCacheFolderAsync(id);
            var result = true;
            try
            {
                await cacheFolder.GetFileAsync(id + ".html");
            }
            catch (FileNotFoundException)
            {
                result = false;
            }

            return result;
        }

        public async Task<string> GetNewsCacheListInfoStringAsync(string fileName)
        {
            var cacheFolder = await GetNewsCacheFolderAsync();
            var content = string.Empty;

            Debug.WriteLine("----------- get news list cache,filename: " + fileName);

            try
            {
                var file = await cacheFolder.GetFileAsync(fileName);
                using (var stream = await file.OpenReadAsync())
                using (var sr = new StreamReader(stream.AsStream()))
                {
                    content = await sr.ReadToEndAsync();
                }
            }
            catch (FileNotFoundException ex)
            {
                Debug.WriteLine(fileName + " not found!!!  " + ex.Message);
            }

            return content;
        }

        public async Task<bool> SaveNewsListCacheAsync(string fileName, List<NewsListInfo> list)
        {
            var cacheFolder = await GetNewsCacheFolderAsync();

            var imgList = (from newsListInfo in list where newsListInfo.IsCached == false select newsListInfo.Thumb_img).ToList();
            if (imgList.Count == 0)
                return true;

            downloadImgList(imgList, cacheFolder);

            foreach (var newsListInfo in list)
            {
                if (newsListInfo.IsCached == false)
                {
                    newsListInfo.Img = "iso::" + newsListInfo.Img.GetImgFileNameFromSrc();
                    newsListInfo.Thumb_img = "iso::" + newsListInfo.Thumb_img.GetImgFileNameFromSrc();
                }

                newsListInfo.IsCached = true;
            }

            var content = JsonConvert.SerializeObject(list);

            using (var file = await cacheFolder.OpenStreamForWriteAsync(fileName, CreationCollisionOption.ReplaceExisting))
            using (var sr = new StreamWriter(file))
            {
                await sr.WriteAsync(content);
            }

            return true;
        }

        /// <summary>
        /// 根据新闻ID，获取缓存中对应文件的地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetNewsCachePath(string id)
        {
            var path = string.Format("NewsCache/{0}/{1}.html", id, id);
            Debug.WriteLine("GetNewsCachePath: " + path);
            return path;
        }

        private const string ImgNotFoundSrc = "Not found";

        private void ConvertPNodes(HtmlNode pNode, List<string> imgSrcList)
        {
            var imgNodes = pNode.SelectNodes("img");
            if (imgNodes != null && imgNodes.Count > 0)
            {
                foreach (HtmlNode imgNode in imgNodes)
                {
                    var src = imgNode.GetAttributeValue("src", ImgNotFoundSrc);
                    imgSrcList.Add(src);
                    Debug.WriteLine(src.GetImgFileNameFromSrc());
                    imgNode.SetAttributeValue("src", src.GetImgFileNameFromSrc());
                    imgNode.SetAttributeValue("title", "img");
                }
            }

            var subPNodes = pNode.SelectNodes("p");
            if (subPNodes != null && subPNodes.Count > 0)
            {
                foreach (var subPNode in subPNodes)
                {
                    ConvertPNodes(subPNode, imgSrcList);
                }
            }
        }

        /// <summary>
        /// 将新闻内容缓存到本地的html文件中
        /// </summary>
        /// <param name="id"></param>
        /// <param name="htmlContent"></param>
        /// <returns></returns>
        public async Task<string> SaveNewsContentToCacheFolder(string id, string htmlContent)
        {
            StorageFolder newsCacheFolder = await GetNewsCacheFolderAsync(id);

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var pNodes = doc.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/p");
            var rootImgNodes = doc.DocumentNode.SelectNodes("/html[1]/body[1]/div[1]/img");
            var imgSrcList = new List<string>();

            if (pNodes != null)
            {
                foreach (HtmlNode node in pNodes)
                {
                    ConvertPNodes(node, imgSrcList);
                }
            }

            if (rootImgNodes != null)
            {
                foreach (var imgNode in rootImgNodes)
                {
                    var src = imgNode.GetAttributeValue("src", ImgNotFoundSrc).Trim();
                    imgSrcList.Add(src);
                    Debug.WriteLine(src);
                    Debug.WriteLine(src.GetImgFileNameFromSrc());
                    imgNode.SetAttributeValue("src", src.GetImgFileNameFromSrc());
                    imgNode.SetAttributeValue("title", "img");
                }
            }

            var htmlPath = Path.Combine(newsCacheFolder.Path, id + ".html");
            //doc.ToString();
            Debug.WriteLine("#Caching###################################");

            try
            {
                downloadImgList(imgSrcList, newsCacheFolder);

                if (!File.Exists(htmlPath))
                {
                    doc.Save(htmlPath, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }

            return htmlPath;
        }

        private void downloadImgList(IReadOnlyList<string> imgSrcList, IStorageFolder folder)
        {
            if (imgSrcList == null || imgSrcList.Count == 0)
                return;

            foreach (var src in imgSrcList)
            {
                Debug.WriteLine("Downloading:{0}", src);

                if (string.IsNullOrEmpty(src) || !src.ToLower().StartsWith("http://"))
                    continue;

                var request = WebRequest.CreateHttp(src);
                var src1 = src;
                request.BeginGetResponse(async (result) =>
                {
                    var response = request.EndGetResponse(result);

                    using (var stream = response.GetResponseStream())
                    {
                        var file = await folder.CreateFileAsync(src1.GetImgFileNameFromSrc(), CreationCollisionOption.ReplaceExisting);
                        var data = new byte[(int)response.ContentLength];

                        stream.Read(data, 0, data.Length);

                        using (var fs = await file.OpenStreamForWriteAsync())
                        {
                            await fs.WriteAsync(data, 0, data.Length);
                        }
                    }
                }, null);
            }
            //Windows Phone Power Tools
        }

        public async Task ClearNewsCache()
        {
            try
            {
                var cacheFolder = await GetNewsCacheFolderAsync();
                await cacheFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);

                App.NewsViewModel.NewsCacheListInfo.Clear();
            }
            catch (FileNotFoundException)
            {

            }
        }

        public async void DownloadAsync(string url, string fileName, CancellationToken cancellationToken)
        {
            var localFolder = ApplicationData.Current.LocalFolder;

            var newsCacheRootFolder = await localFolder.CreateFolderAsync(VideoCacheFolerName, CreationCollisionOption.OpenIfExists);

            var localFile = await newsCacheRootFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            var lStartPos = 0L;
            using (var fs = await localFile.OpenStreamForWriteAsync())
            {
                lStartPos = fs.Length;

                //if (lStartPos > 0)
                //{
                //    fs.Seek(lStartPos, SeekOrigin.Current);
                //}
            }

            try
            {
                var myrq = (HttpWebRequest)WebRequest.CreateHttp(url);
                myrq.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; .NET CLR 1.0.3705)";

                if (lStartPos > 0)
                {
                    myrq.Headers["bytes"] = lStartPos.ToString();
                    Debug.WriteLine("lStartPos: " + lStartPos);
                }

                myrq.BeginGetResponse(async (result) =>
                {
                    var response = myrq.EndGetResponse(result);

                    Debug.WriteLine(response.Headers["Accept-Ranges"]);

                    var totalBytes = response.ContentLength + lStartPos;
                    Debug.WriteLine("totalBytes: " + totalBytes);

                    using (var responseStream = response.GetResponseStream())
                    {
                        var totalDownloadedByte = lStartPos;
                        var by = new byte[1024];
                        if (responseStream != null)
                        {
                            Debug.WriteLine("responseStream != null");
                            var startTime = DateTime.Now;
                            var osize = responseStream.Read(by, 0, (int)by.Length);
                            var tmpsize = 0;
                            Debug.WriteLine("osize: " + osize);
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
                                Debug.WriteLine("fs.write");
                                //fs.Write(by, 0, osize);

                                //var downloadedByte = totalDownloadedByte;
                                //tmpsize += osize;
                                //var time = startTime;
                                //var tmpsize1 = tmpsize;

                                //var totalSeconds = (endTime - time).TotalSeconds;
                                //if (totalSeconds > 0.5)
                                //{
                                //    var seepText = string.Format("{0:F}", tmpsize1 / totalSeconds / 1024);
                                //    Debug.WriteLine(seepText + " kb/s");
                                //    tmpsize = 0;
                                //    startTime = DateTime.Now;
                                //}
                                //Debug.WriteLine((int)downloadedByte);

                                osize = responseStream.Read(by, 0, (int)by.Length);
                            }
                        }
                        //fs.Close();
                        if (responseStream != null) responseStream.Close();
                    }
                }, null);

            }
            catch (Exception ex)
            {

            }

        }
    }

}
