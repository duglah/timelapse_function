# Daily Instagram Post Azure Function
Azure function to post a daily image of my smartGarten timelapse to instagram (See https://www.instagram.com/philipps_smartgarten/).

Currently a Raspberry Pi with a [hd camera module](https://www.raspberrypi.com/products/raspberry-pi-high-quality-camera/) takes every 15 minutes a picture and uploads it to a Azure Blob Storage Container using two Azure IoT Edge Modules (See https://github.com/smagribot/iot-edge-rpicamera).

The functions starts daily at 13 o'clock (UTC) and searches for images from today with the prefix filter, because the images are in the format yyyy-MM-dd_HH-mm-ss.jpg.
Then a sas token url gets created for the latest image of today, so the Meta Api can download the picture and create a image container which can be published.

Please read the documentation for more information on how to create a developer account and all the prerequisites to use this function.

# Environnement configuration
- `AZURE_STORAGE_ACCOUNTNAME`: Azure Storage account name
- `AZURE_STORAGE_KEY`: Azure Storage key
- `AZURE_STORAGE_BLOBSERIVCEURI`: Azure Storage Blob service uri
- `AZURE_STORAGE_CONTAINER`: Name of the container which contains the timelapse images
- `INSTAGRAM_BUSINESS_ACCOUNT_ID`: Instagram business account id which should publish the images
- `FB_PAGE_ACCESS_TOKEN`: Facebook page access token (Must be created with a long lived access token, so it has no expiration!)
- `START_DATE`: Start of the smartGarten grow (Used for the caption)

# Helpful documentation to get started

## Azure Functions
- https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process

## Azure Blob Storage
- https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet

## Meta Developer Resources
- https://developers.facebook.com/docs/instagram-api/getting-started
- https://developers.facebook.com/docs/instagram-api/guides/content-publishing
- https://developers.facebook.com/docs/instagram-api/reference/ig-user/media#create-photo-container
- https://developers.facebook.com/docs/instagram-api/reference/ig-user/media_publish
- https://developers.facebook.com/docs/pages/access-tokens#limitations
- https://developers.facebook.com/docs/pages/access-tokens#get-a-page-access-token