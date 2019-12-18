Event Carried State Transfer
---------------------------

Outside and Inside Data
----------------------

In his white paper "Data on the Outside vs. Data on the Inside", Pat Helland classifies data according to 
whether it exists inside a service boundary or outside that boundary. He calls the former Inside Data and the
latter Outside Data.

Inside Data is the data inside the service boundary. No one outside the service  boundary can take a dependency 
on Inside Data. The service is the single writer of this data, the system of record, and is at liberty to change 
its schema. The backing store for a service holds Inside Data.

Outside Data is what the service communicates to consumers at its boundary. It has three interesting properties:

1. It is immutable. Changing this data does not impact the Inside Data of the originating service. That data
can only change from outside by using a provided API, if any, at the boundary.
2. It is stale. As soon as data leaves our service it risks being stale. Any update to the data in the 
originating service will not be reflected in that data. Consider that if we use an HTTP API and GET the state
of a resource, a PUT that arrives to change that resource immediately after will mean the copy that we have
is now stale.
3. It should be versioned. Because the data risks being stale, we need to version it, so that we can compare
it against potentially fresher versions. For example in HTTP we can use the If-None-Match header with an ETag
to determine if our data is stale, or if the resource would be the same if we retrieved it again.

(It's worth noting that data supplied by a client as part of a command or request sent to the service is also 
Outside Data.)

Caching
-------

The above properties make Outside Data make amenable to caching. Indeed this is how the web scales, we expose 
Outside Data from the Origin Server that is immutable (changing the response body has no impact on Inside Data), 
stale (post our GET a subsequent POST may modify the Inside Data our response was built from, perhaps before we
have even parsed the result, and versioned (use of a Last Modified or ETag header allows the Origin Server to
version the result that it responds with).

Event Carried State Transfer
----------------------------

It's not just HTTP APIs whose results can be cached, we could also cache an event from the origin server, raised
via AMQP, Kafka, ATOM or some other protocol.

If the origin server raises events whenever a change occurs to the resources it manages, then consuming services
downstream can cache those results, thus preventing the need to make a request to the origin server to GET
the current state of the entity. We can think of this as a push based cache instead of a pull based one.

The name given to building a cache upstream of the origin server from events is Event Carried State Transfer
 (ECST).

There are some valuable aspects to this kind of cache.

When we need to combine the state from two or more microservices to answer a query or respond to a command,
we want to avoid making requests to that other service. This is because we temporally couple the two 
services together - for our service to work, the other services must also be available. 

To solve this problem, we could work with a cache of the data we need to enrich our service built by ECST. By 
listening to the microservice we need the data from, we are able to join to the cache of data we require without 
making a request to that service.

Usually a worker process listens for events from the other service and populates our local cache from the other
microservice's events.

Note that what we are putting in the cache is Outside Data, not Inside Data. We do not want to couple our consuming
microservice to the the internals of the producing microservices model. We want to store the equivalent to what
we would recieve if we queried for it. This is why we prefer ECST over simple replication of data between services
which would couple us to the details.

Alternatives to Event Carried State Transfer
--------------------------------------------
We could also meet the constraint that our service needs data from another to respond by ensuring that the 
request has all the data we require to process the event. If we think of the sender as the data source, 
and our service as the data sink, we can build a pipeline where the original request is enriched with the 
required data by the microservices that own that data as a filter step. 

Where no central process controls this pipeline we refer to it as choreography,  and we refer to it as 
orchestration when a process controls the pipeline.

Whilst this could also work for a query, it is less common to take this approach to populating a response 
due to the likely latency of the response.

Reference Data
--------------
Pat Helland uses the term Reference Data to describe the types of data suitable for sharing via a mechanism 
like Event Carried State Transfer (although he allows for other styles). He calls out three main classes 
of data:

1. Shared Collections. This is ubiquitous data that everyone needs to use to do work, such as a list of users,
products, suppliers, brokers etc. It is so common for code to need to join this data that it makes sense
to copy it to each service that needs it.
2. Operand Data. Constructing requests to other microservices may require a service to understand a set of 
available options such as customer billing plans, or product categories. Operand data is where we share the
range of available options we can use to construct requests.
3. Snapshots. Where we want to query across multiple microservices we can end up with chatty solutions making
requests to other microservices which we then need to join in the caller. An alternative is to listen to events
so as to build an model that we can query. This is the model used by many Big Data pipelines or by Composite View
Models.

When trading off between ECST and passing the required state through a pipeline, consider whether the data
that you are sharing falls into one of these categories. If it does, consider Event Carried State Transfer. If
not consider whether passing the information via the pipeline is a better option.

Worked Scenario
---------------
Imagine that we are writing software for a hotel. We have identified a number of microservices for our hotel:

|HotelMicroservices|

DirectBooking: Lets a customer reserve a room. May be a customer with an account or a guest.
Credit Card Payments: Handles taking payments from a customer.
Accounts: Holds information on account holders, including card details
Housekeeping: Prepares rooms for a guest's stay and provides upkeep of the room during the stay
Channel Manager: Markets our hotel rooms via various aggregator sites.

When an account holder books a room they use the DirectBooking API to POST a booking. DirectBooking validates
the booking and then raises an event to indicate that there has been a BookingMadeOnAccount. A number of services
listen for this message:

Channel Manager: Decrements the rooms available on aggregator sites.
Housekeeping: Schedules occupancy, cleaning of the room prior to occupancy, during and after.
Credit Card Payments: Takes a payment from the Account holder.

How does the Credit Card Payments system take the payment, when Accounts holds the account holders credit card
details? We don't want to call a credit card details HTTP directly as this moves us back to a request driven
architecture.

We have two options:

(a) A pipeline. Accounts listens for DirectBookingMadeOnAccount. It adds the credit card details to the booking
and raises a DirectBookingMadeOnAccountWithCardDetails message. It is this message that Credit Card Payments listens
to and then takes the card payment via.
(b) ECST. Accounts publishes an event whenever an account holder changes name, address, or credit card details,
called AccountDetailsChanged. Credit Card Payments subscribes to this event and caches the data in its own backing
store. Then when a payment request comes in via BookingMadeOnAccount it is able to look up the credit card
details and take the payment. When we cross-check we can see that account details would seem to be a clear
case of Shared Collection Reference Data and suitable for use in ECST.

Our preference for the two may depend on the extent to which we want to allow Credit Card Payments to take a 
payment even if Accounts is down, as Credit Card Payments is working with a cache. we may decide that a
bulkhead is valuable enough to us to use ECST over choreography via a pipeline.

.. |HotelMicroservices| image:: _static/images/HotelMicroservices.png
