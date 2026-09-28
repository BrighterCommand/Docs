---
description: "The Brighter Control API allows direct management of a Dispatcher node."
layout:
  description:
    visible: false
---

# **Brighter Control API**

> **Reference** · Applies to **Brighter V10**

The Brighter Control API allows direct management of a Dispatcher node.

## Configuring the API

Brighter's Package:
- `Paramore.Brighter.ServiceActivator.Control.Api`

provides an extension for ASP.NET Core's `IEndpointRouteBuilder`, which `WebApplication` implements, so you map the endpoints directly on your app:

```csharp
using Paramore.Brighter.ServiceActivator.Control.Api;

app.MapBrighterControlEndpoints();
```

The endpoints are mapped under `/control` by default. Pass a base route to put them somewhere else — `app.MapBrighterControlEndpoints("/ops/brighter")` serves `GET /ops/brighter/status`, and `/control/status` then returns 404.

## API's Provided

### Get Node Status
You can retrieve the status of a Dispatcher node by calling `GET /control/status`.

The response contains:

- **nodeName**: The name of the node running the Dispatcher — the Dispatcher's `HostName`, `Brighter` followed by a UUID unless you set one
- **availableTopics**: The **subscription names** this node services. Despite the field's name these are not topics: a subscription named `orders-subscription` on the routing key `Orders.OrderPlaced` appears as `orders-subscription`
- **subscriptions**: An array with one entry per subscription:
  - **topicName**: The subscription name, as in `availableTopics`
  - **performers**: The names of the subscription's open performers
  - **activePerformers**: The number of open performers
  - **expectedPerformers**: The number of performers the subscription is configured to run
  - **isHealthy**: `true` when `activePerformers` equals `expectedPerformers`
- **isHealthy**: `true` when every subscription is healthy
- **numberOfActivePerformers**: The number of open performers across all subscriptions
- **timeStamp**: When the status was taken
- **executingAssemblyVersion**: The version of Brighter's control package, not of your application

A node running one subscription, `orders-subscription`, with one performer:

```json
{
    "nodeName": "Brighter01a0e8e2-8d96-770e-9a8b-afc8e684fc23",
    "availableTopics": [
        "orders-subscription"
    ],
    "subscriptions": [
        {
            "topicName": "orders-subscription",
            "performers": [
                "orders-subscription-01a0e8e2-8d9d-7668-a035-011682a1a192"
            ],
            "activePerformers": 1,
            "expectedPerformers": 1,
            "isHealthy": true
        }
    ],
    "isHealthy": true,
    "numberOfActivePerformers": 1,
    "timeStamp": "2026-09-28T16:39:17.211498+00:00",
    "executingAssemblyVersion": "10.7.0+c1b8af886235ba3ddc9b3a88e880c121050aec77"
}
```

### Update Performer Count
You can change the number of performers a subscription runs by calling `PATCH /control/subscriptions/{subscriptionName}/performers/{numberOfPerformers}`, where `numberOfPerformers` is an integer. The Dispatcher opens or closes performers to match, and the change shows in the next `GET /control/status`.

`subscriptionName` is the subscription's name — the value `/control/status` reports as `topicName` — not its routing key. This returns either:

- **200 OK** with a message such as `Active performers for orders-subscription set to 3`
- **400 BAD REQUEST** with a message such as `No such subscription Orders.OrderPlaced`, when no subscription has that name

**Match the name's case exactly.** The check for an unknown name ignores case but the update does not, so `ORDERS-SUBSCRIPTION` passes the check and then fails with a 500, an `InvalidOperationException` from the Dispatcher.
