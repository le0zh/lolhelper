using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Storage;
using Microsoft.Xna.Framework.Media;

namespace LolWikiApp.Repository
{
    public class HelperRepository
    {

        //public static string SaveTempHtmlFileToIsolatedStorage(string content)
        //{
        //    if (string.IsNullOrEmpty(content))
        //        return string.Empty;

        //    string path = "data/html/game_detail.html";
        //    var bytes = Encoding.Unicode.GetBytes(content);
        //    IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
        //    using (var output = iso.CreateFile(path))
        //    {
        //        output.Write(bytes,0,bytes.Length);
        //    }

        //    return path;
        //}
      

        public static void CopyContentToIsolatedStorage(string file)
        {
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.FileExists(file))
                {
                    Debug.WriteLine("pass..");
                    iso.Dispose();
                    return;
                }

                var fullDirectory = System.IO.Path.GetDirectoryName(file);

                if (fullDirectory == null)
                    return;

                if (!iso.DirectoryExists(fullDirectory))
                    iso.CreateDirectory(fullDirectory);

                using (Stream inputStream = Application.GetResourceStream(new Uri(file, UriKind.Relative)).Stream)
                {
                    if (inputStream != null)
                    {
                        using (var output = iso.CreateFile(file))
                        {
                            byte[] readBuffer = new byte[4096];
                            int bytesRead = -1;
                            while ((bytesRead = inputStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                            {
                                output.Write(readBuffer, 0, bytesRead);
                            }
                        }
                    }
                }

            }
        }


        public async static Task<string> ReadImageHtmlTemplage()
        {
            string content;
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Data/template.txt"));
            using (var stream = await file.OpenReadAsync())
            using (StreamReader sr = new StreamReader(stream.AsStream()))
            {
                content = await sr.ReadToEndAsync();
            }
            return content;
        }
        
        public static bool SaveImage(string fileName, BitmapImage source)
        {
            var isSuccess = false;
            var ms = new MemoryStream();
            try
            {
                var library = new MediaLibrary();
                var bitmap = new WriteableBitmap(source);
                bitmap.SaveJpeg(ms, bitmap.PixelWidth, bitmap.PixelHeight, 0, 100);
                ms.Seek(0, SeekOrigin.Begin);
                ms.Seek(0, SeekOrigin.Current);
                library.SavePicture(fileName, ms);
                ms.Close();
                isSuccess = true;
            }
            catch (Exception ex)
            {
                isSuccess = false;
            }
            finally
            {
                ms.Close();
            }

            return isSuccess;
        }
    }
}
