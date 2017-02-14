Paramore
========

Libraries and supporting examples for use with the Ports and Adapters
and CQRS architectural styles for .NET, with support for Task Queues

`View the Project on GitHub
iancooper/Paramore <https://github.com/iancooper/Paramore>`__

-  `Download **ZIP
   File** <https://github.com/iancooper/Paramore/zipball/master>`__
-  `Download **TAR
   Ball** <https://github.com/iancooper/Paramore/tarball/master>`__
-  `View On **GitHub** <https://github.com/iancooper/Paramore>`__

`Paramore Home <../index.html>`__

`Brighter Home <Brighter.html>`__

`Next <PortsAndAdapters.html>`__

`Prev <GreetingsExample.html>`__

Tutorial
--------

This project has been created to be an example how to use Brighter under
AWS SQS Infrastructure. This project is a service which listens 3
different events from AWS SQS.

-  **DocumentCreatedEvent:** Document Id, Folder Id, Title
-  **DocumentUpdatedEvent:** Document Id, Folder Id, Title
-  **FolderCreatedEvent:** Folder Id, Title

And the event handlers are:

-  **DocumentCreatedEventHandler:** If there is no folder with the
   event's Folder Id, requeue the message. Otherwise, save the document
   to the database.
-  **DocumentUpdatedEventHandler:** If there is no document with the
   event's Document Id, requeue the message. If there is no folder with
   the event's Folder Id, requeue the message. Otherwise, update the
   document on database
-  **FolderCreatedEventHandler:** Save the folder to the database.

| IN AWS console, you need to create 3 different SNS topics for the
events
| |image0|

| And create 3 different queues listening those SNS topics
| |image1|

Once you have the topics and the queues, just update the app.config
file's <serviceActivatorConnections/> section and put your queue urls.

You also need to set your AWSSDK profile in app.config file's <aws/>
section. To findout how to setup your credentials please check `Running
Under AWS SQS Infrastructure <RunningUnderAWSSQSInfrastructure.html>`__
section.

Configuring application
-----------------------

To set up your application to use Brighter on SQS, you can use the
following code.

::

    var sqsMessageConsumerFactory = new SqsMessageConsumerFactory(logger);
    var sqsMessageProducerFactory = new SqsMessageProducerFactory(logger);

    var builder = DispatchBuilder
        .With()
        .CommandProcessor(CommandProcessorBuilder.With()
            .Handlers(new HandlerConfiguration(subscriberRegistry, handlerFactory))
            .Policies(policyRegistry)
            .NoTaskQueues()
            .RequestContextFactory(new InMemoryRequestContextFactory())
            .Build()
         )
         .MessageMappers(messageMapperRegistry)
         .ChannelFactory(new InputChannelFactory(sqsMessageConsumerFactory, sqsMessageProducerFactory))
         .ConnectionsFromConfiguration();
    _dispatcher = builder.Build();
            

For details, please check out the example project
`here <https://github.com/iancooper/Paramore/blob/master/Brighter/Examples/DocumentsAndFolders.Sqs/Adapters/ServiceHost/DocumentService.cs>`__

Publishing messages
-------------------

To generate DocumentCreated, DocumentUpdated and FolderCreated events,
you can use
`DocumentsAndFolders.Sqs.EventsGenerator <https://github.com/iancooper/Paramore/tree/master/Brighter/Examples/DocumentsAndFolders.Sqs.EventsGenerator>`__

This project is a console application which can create bulk events and
post it to the SNS topics. The parameters are:

-  **eventType:** E.g.: DocumentCreatedEvent, DocumentUpdatedEvent,
   FolderCreatedEvent
-  **eventCount:** E.g.: Number of events to generate. Default: 10
-  **firstEventId:**\ Default: 1
-  **documentPerFolder:**\ Used for DocumentCreatedEvent and
   DocumentUpdatedEvent. Sets the same folder id to defined number of
   documents. Default: 1
-  **firstFolderId:** Default: 1

Example:

::

    ./.DocumentsAndFolders.Sqs.EventsGenerator.exe -eventType "DocumentCreatedEvent" -eventCount 1000 -firstEventId 1 -documentPerFolder: 10 -firstFolderId: 1
            

This should genereate 1000 DocumentCreatedEvents with DocumentIds 1-1000
and FolderIds 1-100 in DocumentCreated Queue.

This project is maintained by
`iancooper <https://github.com/iancooper>`__

Hosted on GitHub Pages — Theme by
`orderedlist <https://github.com/orderedlist>`__

.. |image0| image:: images/AWSConsoleSNSList.png
.. |image1| image:: images/AWSConsoleQueueList.png

