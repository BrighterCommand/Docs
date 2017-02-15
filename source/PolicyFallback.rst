`Next <EventSourcing.html>`__

`Prev <PolicyRetryAndCircuitBreaker.html>`__

Failure and Fallback
--------------------

If your **RequestHandler.Handle()** call fails you have a number of
options:

-  The basic failure strategy is to throw an exception. This will
   terminate the request handling pipeline.
-  If you want to support `Retry, and Circuit
   Breaker <PolicyRetryAndCircuitBreaker.html>`__ you can use our
   support for `Polly <https://github.com/michael-wolfenden/Polly>`__
   Policies
-  You can also build your own exception handling into your
   `Pipeline <BuildingAPipeline.html>`__.

If none of these strategies succeed, you may want some sort of backstop
exception handler, that allows you to take compensating action, such as
undoing any partially committed work, issuing a compensating
transaction, or queuing work for later delivery (perhaps using the `Task
Queue <ImplementingDistributedTaskQueue.html>`__).

To support this we provide a **IHandleRequests<TRequest>Fallback**
method. In the Fallback method you write your code to run in the event
of failure.

Calling the Fallback Pipeline
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

We provide a **FallbackPolicy** Attribute that you can use on your
**IHandleRequests<TRequest>.Handle()** method. The implementation of the
**Fallback Policy Handler** is straightforward: it creates a backstop
exception handler by encompassing later requests in the `Request
Handling Pipeline <BuildingAPipeline.html>`__ in a try...catch block.
You can configure it to catch all exceptions, or just `Broken Circuit
Exceptions <PolicyRetryAndCircuitBreaker.html>`__ when a Circuit Breaker
has tripped.

When the **Fallback Policy Handler** catches an exception it calls the
**IHandleRequests<TRequest>.Fallback()** method of the next Handler in
the pipeline, as determined by **IHandleRequests<TRequest>.Successor**

The implementation of **RequestHandler<T>.Fallback()** uses the same
`Russian Doll <BuildingAPipeline.html>`__ approach as it uses for
**RequestHandler<T>.Handle()**. This means that the request to take
compensating action for failure, flows through the same pipeline as the
request for service, allowing each Handler in the chain to contribute.

In addition the **Fallback Policy Handler** makes the originating
exception available to subsequent Handlers using the **Context Bag**
with the key: **CAUSE\_OF\_FALLBACK\_EXCEPTION**

Using the FallbackPolicy Attribute
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

The following example shows a Handler with **Request Handler
Attributes** for `Retry and Circuit Breaker
policies <PolicyRetryAndCircuitBreaker.html>`__ that is configured with
a **Fallback Policy** which catches a **Broken Circuit Exception**
(raised when the Circuit Breaker is tripped) and initiates the Fallback
chain.

::

    public class MyFallbackProtectedHandler: RequestHandler<MyCommand>
    {
        [FallbackPolicy(backstop: false, circuitBreaker: true, step: 1)]
        [UsePolicy("MyCircuitBreakerStrategy", step: 2)]
        [UsePolicy("MyRetryStrategy", step: 3)]
        public override MyCommand Handle(MyCommand command)
        {
            /*Do some work that can fail*/
        }

        public override MyCommand Fallback(MyCommand command)
        {
            if (Context.Bag.ContainsKey(FallbackPolicyHandler<MyCommand>.CAUSE_OF_FALLBACK_EXCEPTION))
            {
                /*Use fallback information to determine what action to take*/
            }
            return base.Fallback(command);
        }

    }
