using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using Windows.Storage.Streams;

namespace LolWikiApp.Repository
{
    public class Repository
    {
        protected async Task<String> GetJsonStringViaHttpAsync(string url)
        {
            var client = new HttpClient {Timeout = TimeSpan.FromMinutes(3)};
            var json = await client.GetStringAsync(new Uri(url));
            return json;
        }

        /// <summary>
        /// Read a specified StorageFile in async way
        /// </summary>
        /// <param name="jsonFile">StorageFile</param>
        /// <returns></returns>
        protected async Task<string> ReadJsonFileAsync(IRandomAccessStreamReference jsonFile)
        {
            string json;
            using (IRandomAccessStreamWithContentType readStream = await jsonFile.OpenReadAsync())
            using (var sr = new StreamReader(readStream.AsStream()))
            {
                json = await sr.ReadToEndAsync();
            }

            return json;
        }

        /// <summary>
        /// GetStorageFileFromInstalledDataFolderAsync
        /// </summary>
        /// <param name="parts">Path parts to be combined</param>
        /// <returns></returns>
        protected async Task<StorageFile> GetStorageFileFromInstalledDataFolderAsync(params string[] parts)
        {
            string path = parts.Aggregate("ms-appx:///Data/", (current, part) => current + (part + "/"));
            if (path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            StorageFile storageFile;
            try
            {
                storageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(path));
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message + "\n" + path);
                throw;
            }

            return storageFile;
        }
    }
}
