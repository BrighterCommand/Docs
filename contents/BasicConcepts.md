# Basic Concepts

## Command

A command is an instruction to carry out work. It exercises the domain and results in a change of state. It expects a single handler.

An [event](#event) may be used to indicate the outcome of a command.

## Command Processor

In Brighter, a command processor allows you to use the *Command Pattern* to separate caller from the executor, typically when separating I/O from domain code. It acts both as a *Command Dispatcher* which allows the separation of the parameters of a [request](#request) from the [handler](#request-handler) that executes that request and a *Command Processor* that allows you to use a middleware [pipeline](#pipeline) to provide additional and re-usable behaviors when processing that request. 

The Command Processor may dispatch to an [Internal Bus](#internal-bus) or an [External Bus](#external-bus).

## Command-Query Separation (CQS)

Command-Query separation is the principle that because a [query](#query) should never have the unexpected *side-effect* of updating state, a query should clearly be distinguished from a [command](#request). A query reports on the state of a domain, a command changes it. 

## Event

An event is a fact. The domain may be updated to reflect the fact represented by the event. There may be no subscribers to an event. It may be skinny, a notification, where the fact is the event itself, or fat, a document, where the event provides facts describing a change.

An event may be used to indicate the outcome of a [command](#command).

## Event Stream

In [message oriented middleware](#message-oriented-middleware-mom), an event stream delivers [messages](#message) (or records) via a steam. A consumer reads the stream at a specific offset from the start. Consumers can store their offsets to resume reading the stream for where they left off, or reset their offset to re-read a stream. Consumers neither lock, nor delete messages from the stream. For consuming apps to scale, the stream can be partitioned, allowing offsets to be maintained of a partition of the stream. By using separate consumer threads or processes to read a partition, an application can ensure that it is able to reduce the latency of reading the stream.

Examples: Kafka, Kinesis, Redis Streams

## External Bus 

An external bus allows a [command](#command) or [event](#event) to be turned into a [message](#message) and sent over message-oriented-middleware via broker to a [message queue](#message-queue) or [event stream](#event-stream).

Brighter also offers a [service activator](#service-activator) to listen for messages published to a queue or stream and forward them to an [internal bus](#internal-bus) within another process.

## Internal Bus 

A [command](#command), [event](#event) or [query](#query) is executed in-process, passed from the [command processor](#command-processor) or [query processor](#) to a [handler](#request-handler) [pipeline](#pipeline).

## Message

A message is a packet of data sent over message-oriented-middleware. It's on-the-wire representation is defined by the protocol used by [message-oriented-middleware](#message-oriented-middleware-mom).

## Message Oriented Middleware (MoM)

The class of applications that deliver a [message](#message) from one process to another. MoM may send messages either point-to-point (with just a [message queue](#message-queue)) between sender and receiver, or via a broker, which acts as a dynamic router for messages between sender and receiver. With a broker, the receiver often establishes a subscription to a routing table entry (a *routing key* or *topic*) via a [message queue](#message-queue) or an [event stream](#event-stream). 

Brighter abstracts a specific type of message-oriented middleware by a *Transport*.  

For simplicity, Brighter only supports transports that have a broker configuration, not point-to-point. If you need point-to-point semantics, configure your routing table entry so that it only delivers to one consuming queue or stream.

## Message Queue

In [message oriented middleware](#message-oriented-middleware-mom), a message queue delivers [messages](#message) via a queue. A consumer locks a message, processes it, and when it acknowledges it, it is deleted from the queue. Other consumers can process the same queue, and read past any locked messages. This allows scaling via the competing consumers pattern. A nack will release the lock and make a message visible in the queue again, sometimes with a delay. A dead-letter-queue (DLQ) can be used with a nack, to limit the number of retries before a message is considered to be "posion pill" and moved to another queue for undeliverable messages.

Examples: SQS, AMQP 0-9-1 (Rabbit MQ), AMQP 1-0 (Azure Service Bus).

## Pipeline

A pipeline is a sequence of [handlers](#request-handler) that respond to a [request](#request) or [query](#query). The last handler in the sequence is the "target" handler, which forms the pipeline sink. Handlers prior to that form "middleware" that can transform or respond to the request before it reaches the target handler.

Brighter and Darker's pipelines use a "Russian Doll Model" that is, each handler in the pipeline encompasses the call to the next handler, allowing the handler chain to behave like a call stack. 

## Query

A query asks the domain for facts. The [result](#result) of the query reports these facts - the state of the domain. A query does not change the state of the domain, for that use a [request](#request).

## Query Handler

A handler is the entry point to domain code. It receives a query and returns a [result](#result) to the caller. A handler is always part of an [internal bus](#internal-bus). As such a handler forms part of a [pipeline](#pipeline).

It is analogous to a method on an ASP.NET Controller.

## Query Processor

In Darker, a query processor allows you to use the *Query Object Pattern* to separate caller from the executor, typically when separating the code required to execute a query on a specific database/backing store from the parameters of that query. It acts both as a *Query Dispatcher* which allows the separation of the parameters of a [query](#query) from the [handler](#query-handler) that executes that query and a *Query Processor* that allows you to use a middleware [pipeline](#pipeline) to provide additional and re-usable behaviors when processing that query. 

The Query Processor dispatches to an [Internal Bus](#internal-bus).

The Query Processor returns a [result](#result).

## Result

The return value from a [query](#query). The result is returned from a [query handler](#query-handler) and exposed to the caller via the [QueryProcessor](#query-processor).

## Request

In Brighter, either a [command](#command) or an [event](#event), a request for the domain to (potentially) change state in response to an instruction or new facts.

## Request Handler 

A handler is the entry point to domain code. It receives a request, which may be a [command](#command) or an [event](#event). A handler is always part of an [internal bus](#internal-bus) even when the call to the handler was triggered by a [service activator](#service-activator) receiving a [message](#message) sent by another process to an [external bus](#external-bus). As such a handler forms part of a [pipeline](#pipeline).

It is analogous to a method on an ASP.NET Controller.

## Request-Reply

Request-Reply is a pattern in which there is a request for work and a response.

To enforce [Command-Query Separation](#command-query-separation-cqs) Brighter handles commands/events and Darker handles queries.

Where the request changes state, Brighter models this as a [command](#command) and a matching [event](#event) which describes the change. (See [Returning Results from a Handler](/contents/ReturningResultsFromAHandler.md) for a discussion of returning a response directly to the sender of a Command).

Where the request queries for state, Darker models this as a [query](#query), which returns a [result](#result) directly to the caller.

A common approach is to change state via Brighter and query for the results of that state change via Darker (and return those results to the caller). 

If the call to Brighter results in a new entity, and the id for the new entity was not given to the command (for example it relies on the Database generating the id), a common problem is how to then request the details of that newly created entity via Darker. A simple solution is to update the command with the id (as a conceptual *out* parameter), and then retrieve it from there to use in the Darker query. See [update a field on a command](/contents/ReturningResultsFromAHandler.md#update-a-field-on-the-command) for more. 

## Service Activator

A Service Activator triggers execution of your code due to an external input, such as an HTTP call, or a [message](#message) sent over middleware.

In Brighter, the *Dispatcher* acts as a Service Activator, listening for a message from middleware, which it delivers via the [command processor](#command-processor) to a [handler](#request-handler). As such, it turns messages sent over middleware to a call on your [internal bus](#internal-bus).
