---
description: "The S3LuggageStore is an implementation of IAmAStorageProviderAsync for AWS S3 Object Storage."
layout:
  description:
    visible: false
---

# S3 Luggage Store

> **Reference** · Applies to **Brighter V10**

The **S3LuggageStore** is an implementation of **IAmAStorageProviderAsync** for AWS S3 Object Storage. It allows use of the [Claim Check](/contents/ClaimCheck.md) *Transformer* with S3 Object Storage as the *Luggage Store*.

To use the **S3LuggageStore** you need to include the following NuGet package:

**AWS SDK v3** (legacy support):
* **Paramore.Brighter.Transformers.AWS**

**AWS SDK v4** (recommended for new projects):
* **Paramore.Brighter.Transformers.AWS.V4**

See [AWS SQS Migration](/contents/AWSSQSMigrateToV10.md#migrating-from-aws-sdk-v3-to-v4) for migration guidance between v3 and v4.

We then need to configure our **S3LuggageStore** and register it with our IoC container. Our **ClaimCheckTransformer** has a dependency on **IAmAStorageProviderAsync** and at runtime, when our [** **IAmAMessageTransformerFactory**](/contents/MessageTransforms.md#message-transformer-factory) creates an instance it needs to be able to resolve that dependency. For this reason you need to register the implementation, in this case **S3LuggageStore** with the IoC container to allow it to resolve the dependency.

We provide a builder method, **UseExternalLuggageStore()**, to help with this:

``` csharp
using System.Net.Http;
using Amazon;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Transformers.AWS.V4;
using Paramore.Brighter.Transforms.Storage;

serviceCollection.AddBrighter()
    .UseExternalLuggageStore(provider => new S3LuggageStore(
        new S3LuggageOptions(
            new AWSS3Connection(credentials, RegionEndpoint.EUWest1),
            "brightersamplebucketb0561a06-70ec-11ed-a1eb-0242ac120002")
        {
            BucketRegion = S3Region.EUWest1,
            HttpClientFactory = provider.GetService<IHttpClientFactory>(),
            Strategy = StorageStrategy.CreateIfMissing
        }));
```

<!-- symbolcheck: allow AddS3LuggageStore -->
<!-- symbolcheck: allow S3LuggageStoreCreation -->

> **Coming from V9?** The **IServiceCollection** extension **AddS3LuggageStore()** was removed at V10, along with the **S3LuggageStoreCreation** enum it configured. Build the **S3LuggageStore** yourself and hand it to **UseExternalLuggageStore()**, as above. The name is kept here on purpose, so that a reader arriving from a V9 sample finds the page that tells them it is gone.

You configure an **S3LuggageStore** using **S3LuggageOptions**. The connection and the bucket name are constructor arguments; the rest are properties:

* **connection**: The **AWSS3Connection** that allows us to connect to your account. Used to create an **S3Client** and an **STSClient**
* **bucketName**: The name of the S3 bucket that backs the luggage store. We use one bucket for the luggage store. You may re-use a bucket that you already have.
* **BucketRegion**: Where is the bucket? Bucket names must be unique within a region.
* **Strategy**: What should we do when determining if there is a bucket for the store?
  * **StorageStrategy.CreateIfMissing**: We will create the bucket in the requested region (provided the credentials provided have rights to do this.)
    * **StorageStrategy.Validate**: We will check if the bucket exists in the requested region. We throw an **InvalidOperationException** if it does not.
    * **StorageStrategy.Assume**: We do not check for the bucket, but just assume it exists

If you choose **StorageStrategy.CreateIfMissing** or **StorageStrategy.Validate** then you must register an **IHTTPClientFactory** as we will use this to obtain an HTTP Client for use with the AWS REST API to make a check for the bucket's existence. The simplest way to do this is to use the ServiceCollection extension provided for creating an **IHTTPClientFactory**:

```csharp
 serviceCollection.AddHttpClient();
```

### Bucket Creation

If we create the bucket we do so with the following properties:

* Block public PUT access
* Object ownership transferred to bucket owner

In addition we set the following properties on the bucket, which can be controlled:

* We delete aborted uploads after the time given by **TimeToAbortFailedUploads**. Defaults to 1 day.
* We delete successful uploads after the time given by **TimeToDeleteGoodUploads**. Defaults to 7 days.

We set *Tags* on the bucket if they are provided in the **Tags** property.

We default the **ACLs** for the bucket to **S3CannedACL.Private, but you can choose to override this with another policy as described in [**S3CannedACL**](https://docs.aws.amazon.com/AmazonS3/latest/userguide/s3-access-control.html#RESTCannedAccessPolicies).
