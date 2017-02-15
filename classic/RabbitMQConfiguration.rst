`Next <RunningUnderAWSSQSInfrastructure.html>`__

`Prev <DistributedTaskQueueConfiguration.html>`__


RabbitMQ Configuration
----------------------

Getting your application to interact with RabbitMQ using Brighter is a
trivial task. Simply add the *<rmqMessagingGateway/>* section in your
configuration file and ensure that an instance of *RmqMessageProducer*
is being used when building the command processor.

The available configuration options are:

-  amqpUri: Describes how to connect to RabbitMQ

   -  uri: A `uri <https://www.rabbitmq.com/uri-spec.html>`__ in the
      format amqp://{user}:{password}@{host}:{port}/{vhost}.
      Default uri is amqp://guest:guest@localhost:5672/%2f.
   -  connectionRetryCount: The retry count for when a connection fails.
      Default count is 3 retries.
   -  retryWaitInMilliseconds: The time in milliseconds to wait before
      retrying to connect again.
      Default duration is 1000 ms.
   -  circuitBreakTimeInMilliseconds: The time in milliseconds to keep
      the circuit broken.
      Default duration is 60000 ms.

-  exchange: Describes where messages are sent

   -  name: The name of the exchange.
   -  type: The
      `type <https://www.rabbitmq.com/tutorials/amqp-concepts.html>`__
      of the exchange. Can be one of:

      -  direct (default)
      -  fanout
      -  headers
      -  topic

   -  durable: Indicates whether the exchange is durable.
      Default value is false.

-  queues: Defines general settings for queues

   -  highAvailability: Indicates whether all queues should be mirrored
      across all nodes in the cluster.
      Default value is false.
   -  qosPrefetchSize: Allows you to limit the number of unacknowledged
      messages on a channel (or connection) when consuming (aka
      `"prefetch
      count" <https://www.rabbitmq.com/consumer-prefetch.html>`__).
      Default count is 1.

Here's an example of an App.config file:

::

     <?xml version="1.0" encoding="utf-8"?>
    <configuration>
        <configSections>
            <section name="rmqMessagingGateway" type="paramore.brighter.commandprocessor.messaginggateway.rmq.MessagingGatewayConfiguration.RMQMessagingGatewayConfigurationSection, paramore.brighter.commandprocessor.messaginggateway.rmq" />
        </configSections>
        <rmqMessagingGateway>
            <amqpUri uri="amqp://guest:guest@localhost:5672/%2f" connectionRetryCount="3" retryWaitInMilliseconds="1000" circuitBreakTimeInMilliseconds="60000" />
            <exchange name="paramore.brighter.exchange" type="direct" durable="false" />
            <queues highAvailability="false" qosPrefetchSize="1" />
        </rmqMessagingGateway>
    </configuration>
