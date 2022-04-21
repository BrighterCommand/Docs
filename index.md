# Paramore

Libraries and supporting examples for use with the Ports and Adapters
and CQRS architectural styles for .NET, with support for Task Queues.

[View the Project on GitHub](https://github.com/BrighterCommand)

# Brighter

    * [How to Implement a Request Handler](/contents/ImplementingAHandler.md)
        * [What is the difference between a Command and an Event?]()
    * [Dispatching Requests]()
        * [Registering a Handler]()
        * [Dispatching Requests]()
        * [Building a Command Dispatcher]()
        * [Returning results to the caller]()
        * [Handling Failure]()
        * [Passing Information to the Caller]()
        * [Using the base class when dispatching a message]()
    * (Building a Pipeline of Request Handlers]()
        * [The Pipes and Filters Architectural Style]()
        * [The Russian Doll Model]()
        * [Implementing a Pipeline]()
        * [Using a Manual Approach]()
    * [How to Implement an Asynchronous Request Handler]()
    * [Dispatching Requests Asynchronously]()
        * [Usage]()
        * [Registering a Handler]()
        * [Dispatching Requests]()
        * [Do Not Block When Calling *Async Methods]()
        * [Asynchronous vs. Work Queues]()
    * [Building a Pipeline of Async Request Handlers]()
    * [Implementing a Pipeline]()
    * [Passing information between Handlers in the Pipeline]()
    * [Supporting Retry and Circuit Breaker]()
        * [Using Brighter’s UsePolicy Attribute]()
        * [Retry and Circuit Breaker with Task Queues]()
        * [Timeout]()
    * [Failure and Fallback]()
        * [Calling the Fallback Pipeline]()
        * [Using the FallbackPolicy Attribute]()
    * [Basic Configuration]()
        * [What you need to provide]
        * [Subscriber Registry]
        * [Handler Factory]
        * [Policy Registry]
        * [Request Context Factory]
        * [Putting it all together]
    * [Supporting Logging]()
        * [Logger]()
        * [Testing]()
    * [Feature Switches]()
        * [Using the Feature Switch Attribute]()
        * [Building a config for Feature Switches with FluentConfigRegistryBuilder]()
        * [Implementing a custom Feature Switch Registry]()
        * [Setting Feature Switching Registry]()
    * [How The Command Processor Works]()
        The Dispatcher
    Implementing a Distributed Task Queue
    Brighter’s Task Queue Architecture
    Do I have to use a Broker, what about MSMQ?
    What happens when the consumer receives the message?
    What does this look like in code
    The Dispatcher
    Configuration
    Routing
        Publish-Subscribe
        Direct Messaging
        Summary
    Distributed Task Queue Configuration
        Why the split?
    Configuring the Dispatcher in Code
        Message Mappers
        Channel Factory
        Connection List
        Creating a Builder
    Microservices
        Boundaries are explicit
        Services are autonomous
        Share Schema not type
        Compatibility is based on policy
        Next
    Event Driven Collaboration
        Messaging
        Temporal Coupling
        Behavioral Coupling
        Event Driven Collaboration
        Next
    Event Carried State Transfer
    Outside and Inside Data
    Caching
    Event Carried State Transfer
    Alternatives to Event Carried State Transfer
    Reference Data
    Worked Scenario
    A Pipeline
    ECST
    Next
    Outbox Pattern Support
        Producer Correctness
    Solutions
    Correctness in Brighter
        Ignore/Retry
        Compensation
        Outbox
    RabbitMQ Configuration
    AWS SQS Configuration
    Monitoring
        Configuring Monitoring
        Config file
        Handler confguration
        Monitor message format
    Frequently Asked Questions
        Must I manually register all my handlers?
        Is Brighter a Framework or a Library?

