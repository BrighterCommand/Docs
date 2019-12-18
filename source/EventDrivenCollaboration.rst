Event Driven Collaboration
==========================

Event Driven Collaboration is the approach to communicating between microservices that uses events. 
Events, in this case would be packets of data, sent asynchronously over middleware, using a publish-subscribe
approach.

Asychronicity
-------------

Why is it important that this integration is asynchronous? The answer is that we want to avoid Temporal Coupling.
Temporal Coupling occurs when our microservice responds to a request, another microservice must be available.

In the illustration below we imagine hotel software.