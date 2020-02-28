using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using FlowersFX.Models;

namespace FlowersFX
{
    public static class GetEvents
    {
        static IConfigurationRoot config;
        static Utilities utilities;

        [FunctionName("GetEvents")]
        public static async void Run(
            [TimerTrigger("0 30 * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
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
            var untappdMenu = await GetUntappdEvents(config);
            var filename = config["BLOB_STORAGE_FILE_NAME_PREFIX"] + "-events.json";
            await utilities.UploadBlobString(cloudBlobClient, JsonConvert.SerializeObject(untappdMenu), filename);
        }

        public static async Task<EventsRoot> GetUntappdEvents(IConfigurationRoot config)
        {
            var locationId = config["UNTAPPD_LOCATION_ID"];
            var url = $"https://business.untappd.com/api/v1/locations/{locationId}/events";
            var json = await utilities.Get(url);
            dynamic parsed = JsonConvert.DeserializeObject(json);
           
            var location = new Location
            {
                downloaded = DateTime.UtcNow,
                id = locationId,
                events = new List<Event>()
            };

            foreach (dynamic parsedevent in parsed.events)
            {
                var @event = new Event
                {
                    id = parsedevent.id,
                    name = parsedevent.name,
                    description = parsedevent.description,
                    locationname = parsedevent.location_name,
                    link = parsedevent.link,
                    start_time = parsedevent.start_time,
                    end_time = parsedevent.end_time,
                    updated = parsedevent.updated_at
                };

                location.events.Add(@event);
            }

            return new EventsRoot { location = location };
        }
    }
}
