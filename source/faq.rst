Frquently Asked Questions
========================

Must I manually register all my handlers?
----------------------------------------
No, of course not. Brighter is intended as a library, not a framework, (see elsewhere) and as such we avoid holding an opinion about how you will create the factories which we call to create instances of your types, or how you will choose to register them.
However, many people implement a factory by wrapping their IoC container of choice. (We do this in the samples, wrapping TinyIOC in a number of them).
Here is a common approach:

- Implement the HandlerFactory and MessageMapperFactory using your IOC container.
- Use reflection to scan your assemblies for message mappers and request handlers
- Register them with your IOC container
- Register them with SubscriberRegistry or MessageMapperRegistry as appropriate.

This approach means that you do not need to worry about updating your setup to add new handlers and message mappers as you develop them.

In addition, you can write a generic message mapper for those cases where you simply serialize all the properties of a type, and have a strategy for naming the topic that is derived from the class name.

This approach liberates you from boilerplate code for each message mapper. It is most useful where your mapping library for serializing to and from the message (i.e. json serialization) is tolerant to new properties it does not understand.

Is Brighter a Framework or a Library?
------------------------------------
The most common way to distinguish between a library and a Framework is that your code calls a library, whereas a framework calls your code.

Under this definition Brighter is a library: you create the command dispatcher, and call it, in order to route messages. Although the command processor calls your handler pipeline, and thus calls your factories and handlers, this is just user initiated routing.

However, Service Activator would be considered a Framework because the dispatcher is the application, and whilst you must initiate it in the host, Brighter then runs your code, dispatching requests from queues to your handlers.

Moving away from this technical definition of the difference though, our objective is to have a low footprint, that does not dictate choices in your app
