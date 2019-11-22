# Untappd Venue Menu Fetcher Functions

An Azure timer function in C# to retrieve the tap menu for a specific venue on Untappd and store it as a JSON file in blob storage.

Deploy to Azure Functions and make sure the following application settings are present:

```UNTAPPD_READ_ACCESS_TOKEN```

A read access token for the venue on Untappd Business.

```UNTAPPD_USERNAME```

The numerical page id of the page to pull events from, can be found by visiting the page and looking at the bottom of the About section.

```UNTAPPD_MENU_ID```

The numerical page id of the page to pull events from, can be found by visiting the page and looking at the bottom of the About section.

```BLOB_STORAGE_CONNECTION_STRING```

The connection string to Azure storage.

```BLOB_STORAGE_CONTAINER_NAME```

The container name for a blob container in the Azure storage.

```BLOB_STORAGE_FILE_NAME```

The filename to store json results in, eg "menu.json"

For local testing, add these application settings to a local.settings.json file in the project root.

menu_id: 44623