# **Brighter Control API**
The brighter control API allows direct management of a Service Activator node

## Configuring the API

Brighter's Package:
- **Paramore.Brighter.ServiceActivator.Control.Api**

provides an extension for ASPNet.Core's `IEndpointRouteBuilder`

``` c#
app.UseEndpoints(endpoints =>
{
    endpoints.MapBrighterControlEndpoints();
}
```

When mapping the Brighter Control API you can pass a string to change the base route of these calls, by default it is set to `/control`

## API's Provided

### Get Node Status
You can retrieve the status of a Service Activator node by calling `GET /control/status`

The response contains:
    - **nodeName** : The name of the node running Service Activator
    - **availableTopics** : The Topics that this node can service
    - **subscriptions** : An array of Information about currently configured subscriptions
        - **topicName** : Name of Topic
        - **performers** : An array of performers
        - **activePerformers** : Number of currently active performers
        - **expectedPerformers** : Number of expected performers
        - **isHealthy** : Is this subscription healthy on this node
    - **isHealthy** : Is this node Healthy
    - **numberOfActivePerformers** : The Number of Performers currently running on the Node
    - **timeStamp** : Timestamp of Status Event
    - **executingAssemblyVersion** : The version of the running process

``` JSON
{
    "nodeName": "Brightere4888035-06f4-4ef8-b928-dbd47d958538",
    "availableTopics": [
        "Orders.NewOrderVersionEvent"
    ],
    "subscriptions": [
        {
            "topicName": "Orders.NewOrderVersionEvent",
            "performers": [
                "Orders.NewOrderVersionEvent-0943a9d2-6a00-4cd5-a4cb-cd97106e2bbe"
            ],
            "activePerformers": 1,
            "expectedPerformers": 1,
            "isHealthy": true
        }
    ],
    "isHealthy": true,
    "numberOfActivePerformers": 1,
    "timeStamp": "2024-06-29T15:45:46.8910117Z",
    "executingAssemblyVersion": "9.7.8+476e3ad5c683683086393b17deceea509f68566a"
}
```

### Update Performer Count
You can update the number of running performers by calling `PATCH /control/subscriptions/{{subscriptionName}}/performers/{{numberOfPerformers:int}}`

This will return either :
- OK with a Message such as `Active performers for Orders.NewOrderVersionEvent set to 2`
- BAD REQUEST with a message such as `No such subscription Orders.NewOrderVersionCommand`
