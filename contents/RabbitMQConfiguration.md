# RabbitMQ Configuration

TODO: Needs a rewrite

The available configuration options are:

-   amqpUri: Describes how to connect to RabbitMQ
    -   uri: A [uri](https://www.rabbitmq.com/uri-spec.html) in the format amqp://{user}:{password}@{host}:{port}/{vhost}. Default uri is amqp://guest:<guest@localhost>:5672/%2f.
    -   connectionRetryCount: The retry count for when a connection fails. Default count is 3 retries.
    -   retryWaitInMilliseconds: The time in milliseconds to wait before retrying to connect again. Default duration is 1000 ms.
    -   circuitBreakTimeInMilliseconds: The time in milliseconds to keep the circuit broken. Default duration is 60000 ms.
-   exchange: Describes where messages are sent
    -   name: The name of the exchange.
    -   type: The
        [type](https://www.rabbitmq.com/tutorials/amqp-concepts.html) of
        the exchange. Can be one of:
        -   direct (default)
        -   fanout
        -   headers
        -   topic
    -   durable: Indicates whether the exchange is durable. Default value is false.
-   queues: Defines general settings for queues
    -   highAvailability: Indicates whether all queues should be mirrored across all nodes in the cluster. Default value is false.
    -   qosPrefetchSize: Allows you to limit the number of unacknowledged messages on a channel (or connection) when consuming (aka [\"prefetch count\"](https://www.rabbitmq.com/consumer-prefetch.html)).      Default count is 1.
