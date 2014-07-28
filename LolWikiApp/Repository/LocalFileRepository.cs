using System;
using System.Collections.Generic;
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
            var newsCacheFolder = await this.GetNewsCacheFolderAsync(id);
            var op = await newsCacheFolder.CreateFileAsync(id + ".html", CreationCollisionOption.ReplaceExisting);
            using (Stream stream = await op.OpenStreamForWriteAsync())
            using (StreamWriter sw = new StreamWriter(stream))
            {
                sw.Write(htmlContent);
            }

            return NewsCacheFolerName + "\\" + id + ".html";
        }
    }
}
