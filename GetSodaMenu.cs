using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Globalization;
using FlowersFX.Models;

namespace FlowersFX
{
    public static class GetFoodMenu
    {
        static IConfigurationRoot config;
        static Utilities utilities;

        [FunctionName("GetSodakMenu")]
        public static async Task Run(
            [TimerTrigger("0 45 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            utilities = new Utilities(context);

            string storageConnectionString = config["BLOB_STORAGE_CONNECTION_STRING"];
            var account = CloudStorageAccount.Parse(storageConnectionString);

            CloudBlobClient cloudBlobClient = account.CreateCloudBlobClient();
            var untappdMenu = await GetUntappdCustomMenu(config);
            var filename = config["BLOB_STORAGE_FILE_NAME_PREFIX"] + "-soda-menu.json";
            await utilities.UploadBlobString(cloudBlobClient, JsonConvert.SerializeObject(untappdMenu), filename);
        }

        public static async Task<MenuRoot> GetUntappdCustomMenu(IConfigurationRoot config)
        {
            var untappedSodaMenuId = config["UNTAPPD_SODA_MENU_ID"];
            var url = $"https://business.untappd.com/api/v1/custom_menus/{untappedSodaMenuId}?full=true";
            var json = await utilities.Get(url);
            dynamic parsed = JsonConvert.DeserializeObject(json);

            var menu = new Menu
            {
                id = parsed.custom_menu.id,
                name = parsed.custom_menu.name,
                description = parsed.custom_menu.description,
                updated = parsed.custom_menu.updated_at,
                downloaded = DateTime.UtcNow,
                sections = new List<Section>()
            };

            foreach (dynamic parsedsection in parsed.custom_menu.custom_sections)
            {
                var section = new Section
                {
                    id = parsedsection.id,
                    name = parsedsection.name,
                    description = parsedsection.description,
                    position = parsedsection.position,
                    items = new List<Item>()
                };

                foreach (dynamic parseditem in parsedsection.custom_items)
                {
                    var item = new Item
                    {
                        id = parseditem.id,
                        name = parseditem.name,
                        type = parseditem.type,
                        description = parseditem.description,
                        modified = parseditem.updated_at,
                        containers = new List<Container>()
                    };

                    foreach (dynamic parsedcontainer in parseditem.custom_containers)
                    {
                        var container = new Container
                        {
                            id = parsedcontainer.id,
                            price_sek = parsedcontainer.price,
                            position = parsedcontainer.position,
                            name = parsedcontainer.name
                        };

                        item.containers.Add(container);
                    }

                    section.items.Add(item);
                }

                menu.sections.Add(section);
            }

            return new MenuRoot { menu = menu };
        }
    }
}
