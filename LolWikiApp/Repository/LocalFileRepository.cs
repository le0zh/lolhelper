using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Threading.Tasks;

using Windows.Storage;

namespace LolWikiApp.Repository
{
    public class LocalFileRepository
    {
        private const string VideoCacheFolerName = "VideoCache";
        private const string NewsCacheFolerName = "NewsCache";


        private async Task<StorageFolder> GetNewsCacheFolderAsync(string id)
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var newsCacheRootFolder = await localFolder.CreateFolderAsync(NewsCacheFolerName, CreationCollisionOption.OpenIfExists);
            var newsCacheFolder = await newsCacheRootFolder.CreateFolderAsync(id, CreationCollisionOption.OpenIfExists);
            return newsCacheFolder;
        }

        public string GetNewsCachePath(string id)
        {
            //var newsCacheFolder = await this.GetNewsCacheFolderAsync();

            return NewsCacheFolerName + "\\" + id + ".html";
        }

        /// <summary>
        /// 将新闻内容缓存到本地的html文件中
        /// </summary>
        /// <param name="id"></param>
        /// <param name="htmlContent"></param>
        /// <returns></returns>
        public async Task<string> SaveNewsContentToCacheFolder(string id, string htmlContent)
        {
            var newsCacheFolder = await GetNewsCacheFolderAsync(id);


            //var op = await newsCacheFolder.CreateFileAsync(id + ".html", CreationCollisionOption.ReplaceExisting);
            //using (Stream stream = await op.OpenStreamForWriteAsync())
            //using (StreamWriter sw = new StreamWriter(stream))
            //{
            //    sw.Write(htmlContent);
            //}

            const string imgNotFoundSrc = "Not found";

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

            var htmlPath = Path.Combine(newsCacheFolder.Path, id, id + ".html");
            doc.Save(htmlPath);

            downloadImgList(imgSrcList);

            return htmlPath;
        }

        private void downloadImgList(List<string> imgSrcList)
        {
            //TODO
        }
    }
}
