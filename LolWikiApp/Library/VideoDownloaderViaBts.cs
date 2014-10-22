using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Phone.BackgroundTransfer;

namespace LolWikiApp
{
    /// <summary>
    /// Class for download videoes via BackgroundTransferService
    /// </summary>
    public class VideoDownloaderViaBts
    {
        public VideoDownloaderViaBts()
        {
            using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("/shared/transfers"))
                {
                    isoStore.CreateDirectory("/shared/transfers");
                }
            }
        }

        public void Download(string fileName, string sourceUrl)
        {
            // Check to see if the maximum number of requests per app has been exceeded. 
            if (BackgroundTransferService.Requests.Count() >= 5)
            {
                // Note: Instead of showing a message to the user, you could store the 
                // requested file URI in isolated storage and add it to the queue later. 
                MessageBox.Show("The maximum number of background file transfer requests for this application has been exceeded. ");
                return;
            }

            var transferUri = new Uri(Uri.EscapeUriString(sourceUrl), UriKind.RelativeOrAbsolute);

            // Create the new transfer request, passing in the URI of the file to  
            // be transferred. 
            var transferRequest = new BackgroundTransferRequest(transferUri)
            {
                Method = "GET",
                TransferPreferences = TransferPreferences.AllowCellularAndBattery
            };

            var downloadUri = new Uri("shared/transfers/" + fileName, UriKind.RelativeOrAbsolute);
            transferRequest.DownloadLocation = downloadUri;

            // Pass custom data with the Tag property. This value cannot be more than 4000 characters. 
            // In this example, the friendly name for the file is passed.  
            transferRequest.Tag = fileName;

             // Add the transfer request using the BackgroundTransferService. Do this in  
            // a try block in case an exception is thrown. 
            try 
            { 
                BackgroundTransferService.Add(transferRequest); 
            } 
            catch (InvalidOperationException ex) 
            { 
                // TBD - update when exceptions are finalized 
                MessageBox.Show("Unable to add background transfer request. " + ex.Message); 
            } 
            catch (Exception) 
            { 
                MessageBox.Show("Unable to add background transfer request."); 
            } 
        }
    }
}
