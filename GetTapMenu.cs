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
    public static class GetTapMenu
    {
        static IConfigurationRoot config;
        static Utilities utilities;

        [FunctionName("GetTapMenu")]
        public static async Task Run(
            [TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            if (config["UNTAPPD_TAP_MENU_ID"] == null)
            {
                return;
            }

            utilities = new Utilities(context);

            string storageConnectionString = config["BLOB_STORAGE_CONNECTION_STRING"];
            var account = CloudStorageAccount.Parse(storageConnectionString);

            CloudBlobClient cloudBlobClient = account.CreateCloudBlobClient();
            var untappdMenu = await GetUntappdMenu(config);
            var filename = config["BLOB_STORAGE_FILE_NAME_PREFIX"] + "-tap-menu.json";
            await utilities.UploadBlobString(cloudBlobClient, JsonConvert.SerializeObject(untappdMenu), filename);
        }

        public static async Task<MenuRoot> GetUntappdMenu(IConfigurationRoot config)
        {
            var untappdMenuId = config["UNTAPPD_TAP_MENU_ID"];
            var url = $"https://business.untappd.com/api/v1/menus/{untappdMenuId}?full=true";
            var json = await utilities.Get(url);
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
                        type = "beer",
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

            var untappdSecondaryMenuId = config["UNTAPPD_WINE_MENU_ID"];

            if (untappdSecondaryMenuId == null)
            {
                return new MenuRoot { menu = menu };
            }

            var urlSecondary = $"https://business.untappd.com/api/v1/custom_menus/{untappdSecondaryMenuId}?full=true";
            var jsonSecondary = await utilities.Get(urlSecondary);
            dynamic parsedSecondary = JsonConvert.DeserializeObject(jsonSecondary);

            foreach (dynamic parsedsection in parsedSecondary.custom_menu.custom_sections)
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
                        breweryname = parseditem.description,
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
