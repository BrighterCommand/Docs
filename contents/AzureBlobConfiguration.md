# Azure Archive Provider Configuration

## General
Azure Service Bus (ASB) is a fully managed enterprise message broker and is [well documented](https://docs.microsoft.com/en-us/azure/service-bus-messaging/) Brighter handles the details of sending to or receiving from ASB.  You may find it useful to understand the [concepts](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-queues-topics-subscriptions) of the ASB.

## Connection
At this time Azure Blob Archive Provider only supports Token Credential for authentication

## Permissions
For the archiver to work the calling credential will require the role **Storage Blob Data Owner** however if **TagBlobs** is set to False then **Storage Blob Data Contributor** will be adequate.  If you feel that Data Owner is too high you can create a custom role encompasing Contributor and 'Microsoft.Storage/storageAccounts/blobServices/containers/blobs/tags/write'

## Options

* **BlobContainerUri** : The URI of the Blob container to store messages in (i.e. "https://BlobTest.blob.core.windows.net/messagearchive)
* **TokenCredential** : The Credential to use when writing the Blob
* **AccessTier** : The Access Tier to write to the blob
* **TagBlobs** : if this is set to True the defined in **TagsFunc** will be written to the blobs
* **TagsFunc**  : The function to arrange the tags to add when storing, please note that **TagBlobs** must be True for these to be used, default Tags :
    - topic
    - correlationId
    - message_type
    - timestamp
    - content_type
* **StorageLocationFunc** : The function to provide the location to store the message inside of the Blob container, default location : The Id of the message at the root of the **BlobContainerUri**