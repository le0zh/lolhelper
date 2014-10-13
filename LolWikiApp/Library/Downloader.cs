using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LolWikiApp
{
    public class Downloader
    {
        public async void DownloadAsync(string url, string fileName, ProgressBar prog, CancellationToken cancellationToken)
        {
            using (var fs = File.OpenWrite(fileName))
            {
                var lStartPos = fs.Length;

                if (lStartPos > 0)
                {
                    fs.Seek(lStartPos, SeekOrigin.Current);
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
                                var startTime = DateTime.Now;
                                var osize = responseStream.Read(by, 0, (int)by.Length);
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
                                    fs.Write(by, 0, osize);

                                    var downloadedByte = totalDownloadedByte;
                                    tmpsize += osize;
                                    var time = startTime;
                                    var tmpsize1 = tmpsize;
                                    //this.Dispatcher.BeginInvoke(new Action(() =>
                                    //{
                                    //    var totalSeconds = (endTime - time).TotalSeconds;
                                    //    if (totalSeconds > 0.5)
                                    //    {
                                    //        var seepText = string.Format("{0:F}", tmpsize1 / totalSeconds / 1024);
                                    //        SpeedLabel.Content = seepText + " kb/s";
                                    //        tmpsize = 0;
                                    //        startTime = DateTime.Now;
                                    //    }
                                    //    prog.Value = (int)downloadedByte;
                                    //}));

                                    osize = responseStream.Read(by, 0, (int)by.Length);
                                }
                            }

                            fs.Close();

                            if (responseStream != null) responseStream.Close();
                            //MessageBox.Show("下载完毕!");
                        }
                    }, null);

                    //Debug.WriteLine(response.Headers["Accept-Ranges"]);
                    //var totalBytes = response.ContentLength + fs.Length;

                    //Debug.WriteLine("totalBytes: " + totalBytes);

                    //this.Dispatcher.BeginInvoke(new Action(() =>
                    //{
                    //    TotalSizeLabel.Content = string.Format("{0:F} MB", totalBytes / 1048576);
                    //    prog.Maximum = (int)totalBytes;
                    //}));

                    //using (var responseStream = response.GetResponseStream())
                    //{
                    //    var totalDownloadedByte = fs.Length;
                    //    byte[] by = new byte[1024];
                    //    if (responseStream != null)
                    //    {
                    //        DateTime startTime = DateTime.Now;
                    //        var osize = responseStream.Read(by, 0, (int)by.Length);
                    //        int tmpsize = 0;

                    //        while (osize > 0)
                    //        {
                    //            if (cancellationToken.IsCancellationRequested)
                    //            {
                    //                //Cancell operation is requested.
                    //                Debug.WriteLine("cancelled" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    //                return;
                    //            }

                    //            totalDownloadedByte = osize + totalDownloadedByte;
                    //            DateTime endTime = DateTime.Now;
                    //            fs.Write(by, 0, osize);

                    //            var downloadedByte = totalDownloadedByte;
                    //            tmpsize += osize;
                    //            DateTime time = startTime;
                    //            int tmpsize1 = tmpsize;
                    //            this.Dispatcher.BeginInvoke(new Action(() =>
                    //            {
                    //                var totalSeconds = (endTime - time).TotalSeconds;
                    //                if (totalSeconds > 0.5)
                    //                {
                    //                    var seepText = string.Format("{0:F}", tmpsize1 / totalSeconds / 1024);
                    //                    SpeedLabel.Content = seepText + " kb/s";
                    //                    tmpsize = 0;
                    //                    startTime = DateTime.Now;
                    //                }
                    //                prog.Value = (int)downloadedByte;
                    //            }));

                    //            osize = responseStream.Read(by, 0, (int)by.Length);
                    //        }
                    //    }
                    //    fs.Close();
                    //    if (responseStream != null) responseStream.Close();
                    //    //MessageBox.Show("下载完毕!");
                    //}
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("exception: " + ex.Message);
                }
            }
        }
    }
}
