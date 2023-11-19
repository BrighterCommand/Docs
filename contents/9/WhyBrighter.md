# Why Brighter?

There are many options for .NET developers looking for either a package to use as a command processor/dispatcher pattern implementation (sometimes confused with the mediator pattern) or package to use as a messaging framework. So why would you choose Brighter & Darker?

## Reactor Pattern

Brighter's message dispatcher implements the [Reactor Pattern](https://en.wikipedia.org/wiki/Reactor_pattern).

The Service Activator package's **Dispatcher** class acts as a *supervisor which creates a number of **Performers**. A **Performer** is a *reactor*, a single-threaded message pump (event loop) that reads messages from a configured topic/routing key and dispatches them to a handler. The same thread that loops over the queue or stream of messages, is the thread used to run the handler code. 

To scale up you increase the number of **Performers**; to scale out deploy new instances of your app, each of which will use a single-thread. (We recommend using the latter in a container environment). You can use async I/O to allow your thread to read more messages whilst a handler is I/O bound.

This has certain benefits:

* We will process messages from a queue or stream in order. 
* Scaling is predictable, you control the number of Performers and thus the number of threads running.

Brighter has its own synchronization context. This will mean that your callbacks are queued in order onto the single thread. However, if you use ConfigureAwait(true) your callback will use the thread pool, so be cautious around the consequences of this.

Other messaging packaging frameworks use a thread pool thread to process messages read from the queue or stream. 

* This will de-order the messages on the queue or stream
* If the number of in-flight handlers is not limited by a semaphore, then a queue or stream with high load may exhaust the thread pool.
* If a semaphore is used to limit the thread-pool, the semaphore can deadlock under load.
* Scaling is not predictable, you have no control over how many threads are reading work from a stream or queue.

## Command Query Separation

When used with an internal bus, Brighter and Darker provide [Command Query Separation](https://en.wikipedia.org/wiki/Command%E2%80%93query_separation).

Brighter supports dispatching a command or event to their associated handler(s). No value is returned. Use Brighter when you want to modify state.

Darker supports dispatching a query to an associated handler. A value is returned. Use Darker when you want to query but not modify state.

Some other command processor packages for .NET don't distinguish between a command and a query, and so it's not possible to determine if they have side affects.

## Type over Convention

Brighter & Darker recognize that .NET is a statically typed language. A statically typed language creates clarity over the role of a class, typically using an interface or an abstract base class. This allows the compiler to support roles, as opposed to using naming conventions Brighter follows this, so the key classes that you need to implement have a role based interface and may have a base class that provides an implementation that provides default behavior for that interface.

For example, when you write a handler in Brighter you implement the interface **IHandleRequests** typically by inheriting from the base class **RequestHandler**; when you create a request for dispatch by the command processor, you implement the inteface **IRequest** typically by inheriting from the base classes **Command** or **Event**.

Some alternative .NET packages try to emulate a dynamic language, by using convention, such as naming patterns. These implementations tend to be overly influenced by frameworks from dynamic languages where type safety is not available. It often leads to "magical code" that is generated at compile time and a perverse reliance on class names when type information is available. We don't believe that a messaging framework for a statically typed language should forego type safety; we make use of the type system to ensure correctness throughout.

## Code over Configuration

Brighter uses code over configuration. Early versions of Brighter put the configuration of your **Subscriptions** or **Publications** in a configuration file, which it then read. This was error prone and hard to debug. With later versions we moved to make configuration via code. Even with our usage of hostbuilder there is an underlying "push-button" API that you can use to configure Brighter. You should still configure environment based variables (such as the IP address of your broker) in a config file and use from the code.

## Attribute-based Middleware

Brighter uses .NET attributes to allow you to configure middleware for your handlers or message mappers. The use of attributes keeps the middleware declarations in the context of the handler or mapper that you configure them for. This means that you can easily determine the pipeline within which your handler is running, by looking at the handler code.

 The issue with configuring middleware for pipelines at application startup is that it is not obvious to the reader the contet of the middleware pipeline that their handler or mapper is running within.

 ## DI-Friendly Framework

 Under the hood, Brighter uses a DI friendly framework. Instead of providing packages for popular Di-frameworks, Brighter uses a factory abstraction to ask your code to create instances of your code (handlers, message mappers etc.)

 We do provide an implementation of these factories for **ServiceCollection** for convenience, but we can work with your DI library of choice by your implementing the required factor methods.

