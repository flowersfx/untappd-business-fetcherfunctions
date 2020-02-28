using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs;

namespace FlowersFX
{
    public class Utilities
    {
        static IConfigurationRoot config;

        public Utilities(ExecutionContext context)
        {
            config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public async Task UploadBlobString(CloudBlobClient storageClient, string menu, string filename)
        {
            var cloudBlobContainer = storageClient.GetContainerReference(config["BLOB_STORAGE_CONTAINER_NAME"]);
            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(filename);
            await cloudBlockBlob.UploadTextAsync(menu);
        }

        public async Task<string> Get(string url)
        {
            var untappdUsername = config["UNTAPPD_USERNAME"];
            var untappedReadAccessToken = config["UNTAPPD_READ_ACCESS_TOKEN"];

            var request = System.Net.WebRequest.Create(url);
            request.Headers.Add(System.Net.HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{untappdUsername}:{untappedReadAccessToken}")));

            using (var response = await request.GetResponseAsync())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}