using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FlowersFX
{
    public static class UntappdMenu
    {
        static IConfigurationRoot config;

        [FunctionName("UntappdMenu")]
        public static async void Run(
            [TimerTrigger("0 0 * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            string storageConnectionString = config["BLOB_STORAGE_CONNECTION_STRING"];
            var account = CloudStorageAccount.Parse(storageConnectionString);

            CloudBlobClient cloudBlobClient = account.CreateCloudBlobClient();
            var untappdMenu = await GetUntappdMenu(config);
            await UploadBlobString(cloudBlobClient,untappdMenu);
        }

        public static async Task<string> GetUntappdMenu(IConfigurationRoot config )
        {
			var untappdMenuId = config["UNTAPPD_MENU_ID"];
			var url = $"https://business.untappd.com/api/v1/menus/{untappdMenuId}?full=true";
            
            return await Get(url);
        }

        public static async Task UploadBlobString(CloudBlobClient storageClient, string menu)
        {
            var cloudBlobContainer = storageClient.GetContainerReference(config["BLOB_STORAGE_CONTAINER_NAME"]);
            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(config["BLOB_STORAGE_FILE_NAME"]);
            await cloudBlockBlob.UploadTextAsync(menu);
        }

        public static async Task<string> Get(string url)
        {
			var untappdUsername = config["UNTAPPED_USERNAME"];
			var untappedReadAccessToken = config["UNTAPPD_READ_ACCESS_TOKEN"];

            var request = System.Net.WebRequest.Create(url);
            request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{untappdUsername}:{untappedReadAccessToken}")));

            using( var response = await request.GetResponseAsync() )
            using( var reader = new StreamReader( response.GetResponseStream() ) )
            {
                return reader.ReadToEnd();
            }
        }
    }
}
