# Failure and Dead Letter Queues

When a *Request* is passed to **RequestHandler.Handle()** it runs in your application code. If your application code fails, you have a number of options:

- [Retry (and Circuit Break) the *Request* on the Internal Bus](#retry-and-circuit-break-the-request-on-the-internal-bus)
- [Retry (with Delay) the *Request* on the External Bus](#retry-with-delay-the-request-on-the-external-bus)
- [Terminate processing of that *Request*](#terminate-processing-of-that-request)
- [Run a Fallback](#run-a-fallback)
- [Use Custom Middleware](#use-custom-middleware)

Any unhandled exception that leaves the *Request Handling Pipeline* (in other words is not intercepted by middleware) will [Terminate Processing of the Request](#terminate-processing-of-that-request).

## **Retry (and Circuit Break) the *Request* on the Internal Bus**

You can use Brighter's support for Polly policies to retry the operation on the Internal Bus. See [Retry and Circuit Breaker](/contents/PolicyRetryAndCircuitBreaker.md)

A circuit breaker is triggered when all Retry attempts fail, and will prevent further requests from succeeding.

Both the triggering of the circuit breaker, and requests passed to the *Request Handler Pipeline* while the circuit breaker is open will [Terminate processing of that *Request*](#terminate-processing-of-that-request) 

## Retry (with Delay) the *Request* on the External Bus

If you the failure is potentially retriable, but you want to retry on the External Bus (by making the message available to be consume from the External Bus again) then you can throw a **DeferMessageAction** exception. Upon receipt of a **DeferMessageAction** the pump will Reject the message and Requeue it. with a delay. The delay is configured by the External Bus **Subscription.RequeueDelayInMilliseconds** property.

You can configure a limit on the number of requeue attempts by setting the **Subscription.RequeueCount**. A value of -1 will allow infinite retries. 

## Terminate processing of that *Request*

An unhandled exception leaving the pipeline results in us terminating processing of the *Request*. 

- On an Internal Bus that exception will bubble out to the caller.
- On an External Bus we will nack (or reject) the message. On a queue this will delete the message for all consumers, on a stream we will increment the offset past that message for a consumer group.

We do this because you have responsibility to handle exceptions thrown in your code, not the framework and we assume that non-recovered errors are not potentially retriable. 

On and Exernal Bus if your middleware supports a Dead Letter Queue (DLQ), and it is configured in your subscription, when we reject a message it will be copied to the DLQ.

On an External Bus, to prevent discarding too many messages, you can set an **Subscription.UnacceptableMessageLimit**. If the number of messages terminated due to unhandled exceptions equals or exceeds this limit, the message pump processing the External Bus will terminate.

## Run a Fallback

If you want to take action before exiting a handler, due to a failure you can use a Fallback policy. See [Fallback Policy](/contents/PolicyFallback.md) for more details

## Use Custom Middleware

If none of the above options meet your needs, you can define custom approaches to exception handling  by building your own middleware, see [Pipeline](BuildingAPipeline.html).


