using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace PhilippsSmartGarten.DailyInstagramPost
{
    public class DailyInstagramPost
    {
        [FunctionName("DailyInstagramPost")]
        public async Task Run([TimerTrigger("0 0 13 * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("Running DailyInstagramPost");

            var accountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNTNAME") ??
                              throw new ArgumentException("AZURE_STORAGE_ACCOUNTNAME is not defined!");
            var accountKey = Environment.GetEnvironmentVariable("AZURE_STORAGE_KEY") ??
                             throw new ArgumentException("AZURE_STORAGE_KEY is not defined!");
            var serviceUri = new Uri(Environment.GetEnvironmentVariable("AZURE_STORAGE_BLOBSERIVCEURI") ??
                                     throw new ArgumentException("AZURE_STORAGE_BLOBSERIVCEURI is not defined!"));
            var containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER") ??
                                throw new ArgumentException("AZURE_STORAGE_CONTAINER is not defined!");
            var instagramBusinessAccountId = Environment.GetEnvironmentVariable("INSTAGRAM_BUSINESS_ACCOUNT_ID") ??
                                             throw new ArgumentException(
                                                 "INSTAGRAM_BUSINESS_ACCOUNT_ID is not defined!");
            var fbPageAccessToken = Environment.GetEnvironmentVariable("FB_PAGE_ACCESS_TOKEN") ??
                                    throw new ArgumentException("FB_PAGE_ACCESS_TOKEN is not defined!");
            var startDateString = Environment.GetEnvironmentVariable("START_DATE") ??
                                  throw new ArgumentException("START_DATE is not defined!");

            var credential = new StorageSharedKeyCredential(accountName, accountKey);
            var serviceClient = new BlobServiceClient(serviceUri, credential);
            var containerClient = serviceClient.GetBlobContainerClient(containerName);

            var today = DateTime.UtcNow;
            var startDate = DateTime.Parse(startDateString);

            var latestItem = await GetLatestItemOfTodaysNoon(containerClient, today);
            log.LogInformation("Got latest item {Name} ({Date})", latestItem.Name, latestItem.Date);
            
            var sasUrl = CreateSasUrl(containerClient.Name, latestItem.BlobItem.Name, accountName, accountKey,
                serviceUri);
            // log.LogInformation("Created SAS url: {SasUrl}", sasUrl);
            
            var caption = CreateCaption(latestItem, today, startDate);
            log.LogInformation("Created caption: {Caption}", caption);
            
            await UploadImage(sasUrl, caption, instagramBusinessAccountId, fbPageAccessToken);
            log.LogInformation("Upload done!");
        }

        private static async Task<SmartGartenBlobItem> GetLatestItemOfTodaysNoon(BlobContainerClient containerClient, DateTime today)
        {
            var items = new List<SmartGartenBlobItem>();
            var todayPrefix = $"{today:yyyy-MM-dd}";
            await foreach (var blob in containerClient.GetBlobsAsync(prefix: todayPrefix))
            {
                items.Add(new SmartGartenBlobItem
                {
                    Name = blob.Name,
                    Date = DateTime.ParseExact(blob.Name.Split(".jpg")[0], "yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture),
                    BlobItem = blob
                });
            }

            return items.Where(x => x.Date.Hour < 13).OrderBy(x => x.Date).Last();
        }

        private static string CreateSasUrl(string containerName, string blobName, string accountName, string accountKey, Uri blobServiceUrl)
        {
            var blobSasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                ExpiresOn = DateTime.UtcNow.AddMinutes(5),
                Protocol = SasProtocol.Https
            };
            blobSasBuilder.SetPermissions(BlobSasPermissions.Read);
            var sasToken = blobSasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(accountName, accountKey))
                .ToString();

            var uriBuilder = new UriBuilder(blobServiceUrl);
            uriBuilder.Path += $"{containerName}/{blobName}";
            uriBuilder.Query += sasToken;

            return uriBuilder.Uri.AbsoluteUri;
        }

        private static string CreateCaption(SmartGartenBlobItem item, DateTime today, DateTime startDate)
        {
            var timespanSinceStart = today - startDate;
            return $"Tag {timespanSinceStart.Days} ({item.Date:dd.MM.yyyy HH:mm:ss}) #smartGarten #timelapse";
        }

        private static async Task UploadImage(string imgUrl, string caption, string instagramBusinessAccountId,
            string fbPageAccessToken)
        {
            var instagramApiService = new InstagramApiService(instagramBusinessAccountId);
            var result = await instagramApiService.CreateMediaObject(fbPageAccessToken, imgUrl, caption);
            await instagramApiService.PublishMediaObject(fbPageAccessToken, result.Id);
        }
    }
}
