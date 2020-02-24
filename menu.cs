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
using System.Net.Http.Headers;
using System.Globalization;

namespace FlowersFX
{
    public static class UntappdMenu
    {
        static IConfigurationRoot config;

        [FunctionName("UntappdMenu")]
        public static async void Run(
            [TimerTrigger("0 0 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log, ExecutionContext context)
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
            await UploadBlobString(cloudBlobClient, JsonConvert.SerializeObject(untappdMenu));
        }

        public static async Task<Menu> GetUntappdMenu(IConfigurationRoot config)
        {
            var untappdMenuId = config["UNTAPPD_MENU_ID"];
            var url = $"https://business.untappd.com/api/v1/menus/{untappdMenuId}?full=true";

            var json = await Get(url);
            dynamic parsed = JsonConvert.DeserializeObject(json);

            var menu = new Menu
            {
                id = parsed.menu.id,
                name = parsed.menu.name,
                description = parsed.menu.description,
                updated = parsed.menu.updated_at,
                downloaded = DateTime.UtcNow,
                sections = new List<Section>()
            };

            foreach (dynamic parsedsection in parsed.menu.sections)
            {
                // Do not include private sections to the result
                if (!(bool)parsedsection.@public)
                {
                    continue;
                }

                var section = new Section
                {
                    id = parsedsection.id,
                    name = parsedsection.name,
                    description = parsedsection.description,
                    position = parsedsection.position,
                    items = new List<Item>()
                };

                foreach (dynamic parseditem in parsedsection.items)
                {
                    var item = new Item
                    {
                        id = parseditem.untappd_id,
                        number = parseditem.tap_number,
                        rating = parseditem.rating,
                        name = parseditem.name,
                        description = parseditem.description,
                        breweryname = parseditem.brewery,
                        breweryid = parseditem.untappd_brewery_id,
                        abv = parseditem.abv,
                        style = parseditem.style,
                        modified = parseditem.updated_at,
                        containers = new List<Container>()
                    };

                    foreach (dynamic parsedcontainer in parseditem.containers)
                    {
                        var container = new Container
                        {
                            id = parsedcontainer.id,
                            price_sek = parsedcontainer.price,
                            position = parsedcontainer.position,
                            name = parsedcontainer.container_size.name,
                        };

                        string size_in_ounces_as_string = parsedcontainer.container_size.ounces;
                        double size_in_ounces_as_double;

                        if (!string.IsNullOrEmpty(size_in_ounces_as_string) && double.TryParse(size_in_ounces_as_string, NumberStyles.Any, CultureInfo.InvariantCulture, out size_in_ounces_as_double))
                        {
                            container.size_centiliters = (int)Math.Ceiling(size_in_ounces_as_double * 2.95735); // Convert to centiliters
                        }

                        item.containers.Add(container);
                    }

                    section.items.Add(item);
                }

                menu.sections.Add(section);
            }

            return menu;
        }

        public static async Task UploadBlobString(CloudBlobClient storageClient, string menu)
        {
            var cloudBlobContainer = storageClient.GetContainerReference(config["BLOB_STORAGE_CONTAINER_NAME"]);
            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(config["BLOB_STORAGE_FILE_NAME"]);
            await cloudBlockBlob.UploadTextAsync(menu);
        }

        public static async Task<string> Get(string url)
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

    public class Menu
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime updated { get; set; }
        public DateTime downloaded { get; set; }
        public List<Section> sections { get; set; }
    }

    public class Section
    {
        public int id { get; set; }
        public string name { get; set; }
        public int position { get; set; }
        public string description { get; set; }
        public List<Item> items {get; set; }
    }

    public class Item
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string number { get; set; }
        public string breweryname { get; set; }
        public int breweryid { get; set; }
        public string abv { get; set; }
        public string style { get; set; }
        public DateTime modified { get; set; }
        public string rating { get; set; }
        public List<Container> containers { get; set; }
    }

    public class Container {
        public int id { get; set; }
        public string name { get; set; }
        public int position { get; set; }
        public int size_centiliters { get; set; }
        public string price_sek { get; set; }
    }
}
