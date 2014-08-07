using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
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
        private const string VideoCacheFolerName = "VideoCache";
        private const string NewsCacheFolerName = "NewsCache";

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

            downloadImgList(imgList,cacheFolder);

            foreach (var newsListInfo in list)
            {
                if (newsListInfo.IsCached == false)
                {
                    newsListInfo.Img = newsListInfo.Img.GetImgFileNameFromSrc();
                    newsListInfo.Thumb_img = "/NewsCache/" + newsListInfo.Thumb_img.GetImgFileNameFromSrc();
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

        private const string imgNotFoundSrc = "Not found";

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
                    var imgNodes = node.SelectNodes("img");
                    if (imgNodes != null && imgNodes.Count > 0)
                    {
                        foreach (HtmlNode imgNode in imgNodes)
                        {
                            var src = imgNode.GetAttributeValue("src", imgNotFoundSrc);
                            imgSrcList.Add(src);
                            Debug.WriteLine(src);
                            Debug.WriteLine(src.GetImgFileNameFromSrc());
                            imgNode.SetAttributeValue("src", src.GetImgFileNameFromSrc());
                        }
                    }
                }
            }

            if (rootImgNodes != null)
            {
                foreach (var imgNode in rootImgNodes)
                {
                    var src = imgNode.GetAttributeValue("src", imgNotFoundSrc);
                    imgSrcList.Add(src);
                    Debug.WriteLine(src);
                    Debug.WriteLine(src.GetImgFileNameFromSrc());
                    imgNode.SetAttributeValue("src", src.GetImgFileNameFromSrc());
                }
            }

            var htmlPath = Path.Combine(newsCacheFolder.Path, id + ".html");
            //doc.ToString();
            Debug.WriteLine("#Caching###################################");
            downloadImgList(imgSrcList, newsCacheFolder);
            doc.Save(htmlPath);

            return htmlPath;
        }

        private void downloadImgList(IReadOnlyList<string> imgSrcList, IStorageFolder folder)
        {
            if (imgSrcList == null || imgSrcList.Count == 0)
                return;

            foreach (var src in imgSrcList)
            {
                Debug.WriteLine("Downloading:{0}", src);

                if (string.IsNullOrEmpty(src))
                    continue;

                HttpWebRequest request = WebRequest.CreateHttp(src);
                string src1 = src;
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
            var cacheFolder = await GetNewsCacheFolderAsync();
            await cacheFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);

            App.NewsViewModel.NewsCacheListInfo.Clear();
        }
    }
}
