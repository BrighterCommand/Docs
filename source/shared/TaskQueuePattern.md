# The Task Queue Pattern

The Task Queue Pattern let's you use an External Bus to handle work asynchronously. It is a common use of an External Bus outside of using it for an [Event Driven Architecture](/contents/EventDrivenCollaboration.md).

## Doing Work Asynchronously
You might have an HTTP API with a rule that any given request to that API must execute in under 100ms. On measuring the performance of a key POST or PUT operation to your API you find that you exceed this
value. Upon realizing that much of your time is spent I/O you consider two options:

-   Use the TPL (Task parallel library) to perform the work concurrently - Offload the work to a distributed task queue, ack the message, and allow the work to complete asynchronously

Either way you probably return a 202 Accepted to the caller, with a Link header that points to an endpoint where the caller can poll for completion and/or monitor progress. This might be a resource you are
creating that will return a 404 until it exists, or a progress indicator that indicates how far through the work you are and redirects to the resource once it is complete. (You can store progress in a backing store, perhaps using a distributed cache such as Redis).

There is a problem with the TPL approach is that your operation can only meet the 100ms threshold if your work can be parallelised such that no sub-task takes longer than 100ms. Your speed is always constrained by the slowest operation that you need to parallelize. If you are I/O bound on a resource experiencing contention beyond 100ms, you will not meet your goal by introducing more threads. Your minimum time is your minimum time.

You might try to fix this by acking (acknowledging) the request, and completing the work asynchronously. This option is particularly attractive if the work is I/O bound as you can process other requests whilst you wait for the I/O to complete.

The downside of the async approach is that you risk that the work will be lost if the server fails prior to completion of the work, or the app simply recycles.

These requirements tend to push you in the direction of [Guaranteed Delivery](http://www.eaipatterns.com/GuaranteedMessaging.html) to ensure that work you ack will eventually be handled.

