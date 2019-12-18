Microservices
=============

It is possible to think of microservices as 3rd generation SOA. First generation SOA was SOAP based web
services. 2nd generation SOA was messaging, sometimes over SOAP, but also over middleware, often an 
Enterpise Service Bus (ESB). The third generation emphasizes "smart endpoints, dumb pipes" over the 
use of an ESB, either via REST or a lightweight broker such as RMQ.

But much of what applied to SOA,. still applies to microservices.

"SOA is focused on business processes. These processes are performed in different steps (also called activities 
or tasks) on different systems. The primary goal of a service is to represent a “natural” step of business 
functionality. That is, according to the domain for which it’s provided, a service should represent 
a self-contained functionality that corresponds to a real-world business activity."

Josuttis, Nicolai M.. SOA in Practice: The Art of Distributed System Design. 

Don Box, the creator of SOAP, defined 4 tenets for a SOA service. These rules are still useful 
for Microservices. 

1. Boundaries are explicit
2. Services are Autonomous
3. Share schema not type
4. Compatibility is based on policy

|Microservices|

Boundaries are explicit
-----------------------

We must have an API (it may be HTTP, gRPC, AMQP, Kafka etc.). The API hides our implementation details. 
We allow consumers to couple to this API, but not to the contents. The API should be a stable abstraction, it
is our contract with our consumers. The implementation details can be unstable. 

Services are autonomous
-----------------------

We want to be able to release this microservice and this microservice alone. The implication of this includes the 
idea that because no one couples to our implementation details, then provided we do not alter 
the contract expressed by the API, we can re-release easily. But this also implies that
we are the single writer to any backing storage that keeps our state. Otherwise we would couple the 
schema of that backing store to another service and would not be able to release independently of 
other services if they had coupled to those details.

Microservices are a logical, not a physical boundary, and they might consist of more than one container, 
such as web container and a console container, provided all the containers are considered to be part
of the release boundary for CI or CD. This is common where we have different scaling requirements for say
the API served from the web container and a worker process reading from a task queue served from a console 
container.

The key idea  here is Independent Deployability.

Share Schema not type
---------------------

Our software system may not be homogeneous, we may have services developed in multiple languages. As a
result we must not prevent interoperability between microservices by use of the type system from language in the
API. Instead we should use platform neutral alternatives such as plain text formats (JSON, XML, YAML) or binary 
ones (Avro, ProtoBuf).

Compatibility is based on policy
--------------------------------

For our microservices to communicate we need to agree on the protocols we will use. In the SOAP era this led to the 
growth of WS- specifications that described policies for a wide range of service capabilities. Under microservices
there is no similar standards movement, but organizations still need to make assertions about the protocols
that the will use in order to provide interoperability.



.. |Microservices| image:: _static/images/Microservices.png
