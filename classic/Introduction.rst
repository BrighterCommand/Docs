Introduction
============

What is Brighter?
-----------------

Brighter is an implementation of the Command Dispatcher and Command
Processor patterns. These allow you to decouple a endpoint, such as an
`MVC Input
Controller <http://martinfowler.com/eaaCatalog/modelViewController.html>`__
from the domain logic that handles the request. The intent is to support
the Open-Closed Principle - open to extension, closed for modification -
by allowing handlers to be replaced or added, without modifying the
calling code; or for the endpoint framework to be replaced without
impacting the domain code.

One of the most common uses of Brighter, although not it's only use case
is implementing the Command Invoker pattern within a service. `Service
Design Patterns <http://www.servicedesignpatterns.com/>`__ describes a
`Command
Invoker <http://servicedesignpatterns.com/WebServiceImplementationStyles/CommandInvoker>`__
as:

    Create command objects that fully encapsulate common request
    processing logic. Instantiate and invoke these commands from within
    the web service, or forward them to an asynchronous background
    process.

Brighter is intended to be a library not a framework, so it is
consciously lightweight and divided into packages that allow you to
consume only those facilities that you need in your project.

Brighter provides utility packages to help build quality of service into
the handler pipeline, which can also serve as examples for your own
pipelines.

In addition, Brighter supports Tasks Queues: passing commands and events
over an amqp broker, such as RabbitMQ, to be handle asynchronously by
one or more event consumers.

The intent of Brighter is to make the difference between synchronous and
asynchronous handling as small as possible: both write the same handlers
with the same steps in the sequence. On the producer side, sending
asynchronously is just a different method on the dispatcher. On the
consumer side a Service Activator library helps implement the endpoint
code so that marshalling to an asynchronous handler is as friction-free
a manner as possible.

The latter functionality has similar goals to
`NServiceBus <http://particular.net/nservicebus>`__, `Mass
Transit <http://masstransit-project.com/>`__, `Fubu
Transportation <https://github.com/DarthFubuMVC/FubuTransportation>`__,
`Rebus <https://github.com/rebus-org/Rebus>`__ or
`Celery <http://www.celeryproject.org/>`__

However, internally our goals have far more to do with a .NET equivalent
of `Hystrix <https://github.com/Netflix/Hystrix>`__ perhaps even
`Vert.x <http://vertx.io/>`__

Bus, Processor or Dispatcher, what's in a name?
-----------------------------------------------

.NET projects that support task queues - pushing work onto a queue to be
handled asynchronously by one or more consumers - tend to use the term
BUS in their naming regardless of whether or not they are implemented
using a Bus topology (i.e. publish subscribe, with no central message
broker). Brighter tries to avoid this confusion by talking about
Dispatcher, Processor, Service Activator patterns and a Broker
architectural style instead

`Getting Started <QuickStart.html>`__
