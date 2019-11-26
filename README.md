# Untappd Venue Menu Fetcher Functions

An Azure timer function in C# to retrieve the tap menu for a specific venue on Untappd and store it as a JSON file in blob storage.

Deploy to Azure Functions and make sure the following application settings are present:

```UNTAPPD_READ_ACCESS_TOKEN```

A read access token for the venue user on Untappd Business.

```UNTAPPD_USERNAME```

The username (email) of the venue user on Untappd Business. This is required because Untappd uses Basic authentication request headers to access their API.

```UNTAPPD_MENU_ID```

The numerical id of the menu to fetch. This can be explored by calling the menus endpoint for a venue and get a list of them.

```BLOB_STORAGE_CONNECTION_STRING```

The connection string to Azure storage.

```BLOB_STORAGE_CONTAINER_NAME```

The container name for a blob container in the Azure storage.

```BLOB_STORAGE_FILE_NAME```

The filename to store json results in, eg "venue-tap-menu.json"

For local testing, add these application settings to a local.settings.json file in the project root.