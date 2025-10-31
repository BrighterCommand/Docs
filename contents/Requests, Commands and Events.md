# Requests, Commands and Events
## The IRequest Interface

We use the term **Request** for a data object containing parameters that you want to dispatch to a handler. Brighter uses the interface **IRequest** for this concept.

We do not recommend deriving from **IRequest** but instead from the classes **Command** and **Event** which represent types of **Request**.


## What is the difference between a Command and an Event?

 Confusingly, both **Command** or **Event** which implement **IRequest** are examples of the [Command Pattern](https://brightercommand.github.io/Brighter/CommandsCommandDispatcherandProcessor.html). It is easiest to say that within Brighter **IRequest** is the abstraction that represents the Command from the [Command Pattern](https://brightercommand.github.io/Brighter/CommandsCommandDispatcherandProcessor.html).

Why have both **Command** and **Event**? The difference is in how the **Command Dispatcher** dispatches them to handlers.

-   A **Command** is an imperative instruction to do something; it only has one handler. We will throw an error for multiple registered handlers of a command.
-   An **Event** is a notification that something has happened; it has zero or more handlers.

The difference is best explained by the following analogy. If I say \"Bob, make me a cup of coffee,\" I am giving a Command, an imperative instruction. My expectation is that Bob will make me coffee. If Bob does
not, then we have a failure condition (and I am thirsty and cranky). If I say \"I could do with a cup of coffee,\" then I am indicating a state of thirst and caffeine-withdrawal. If Bob or Alice make me a coffee I will be very grateful, but there is no expectation that they will.

So choosing between **Command** or **Event** effects how the **Command Dispatcher** routes requests.

See [Dispatching a Request](DispatchingARequest.html) for more on how to dispatch **Requests** to handlers.

## Message Definitions and Independent Deployability

Some messaging frameworks encourage you to share an assembly containing your message definitions between autonomous components, often as interfaces. Occasionally we see users trying to use **IRequest** for this purpose.

We do not recommend this, instead preferring to keep to the Service Oriented Architecture (SOA) *tenet* of **Share schema not type**.

Between components that we wish to be independently deployable - which might after all be in different languages, or use different frameworks - you should share a schema that defines the shape of the message (for example [AsyncAPI](https://www.asyncapi.com/).

The only exception is where you have two apps that form part of a single service - such as a Task Queue that supports offloading work from a web API - as these tend to be a unit for Continuous Integration and not independently deployable, then sharing types may be appropriate.

Many of our samples share types for convenience, but this is not advice to do that outside of a Task Queue.

