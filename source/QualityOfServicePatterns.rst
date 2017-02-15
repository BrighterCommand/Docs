Quality of Service Patterns
===========================

Timeout
-------

Calls to remote components, such as a database, broker, or http service
can fail. Code that makes assumption that they will succeed is subject
to the `*fallacies of distributed
computing* <http://mira.sai.msu.ru/~megera/docs/Web/systems/fallacies.pdf>`__,
in particular that the network is reliable.

For this reason we need to set a limit on the time we wait for a
response from the a server (or other remote resource). Otherwise we risk
tying up a thread waiting for a response that will never come. This can
potentially cause a cascade effect: as the number of blocked threads on
the server handling the request waiting for a response increase that
server can in turn become unresponsive or even actively refuse requests.

Retry
-----

In the presence of unreliable calls between two components our first
approach to achieving a high quality of service is to recognize that
many such failures are transient: a timeout because a resource is busy,
temporary loss of connectivity, the loss of one node that will be
replaced by failover to another. In this case the fault is
self-correcting, the node comes up, the load on the database or server
declines and their is capacity for our call, or network connectivity is
restored. This means that our call will succeed if we retry after a
delay to allow the transient fault to resolve.

The `Retry <http://msdn.microsoft.com/en-us/library/dn589788.aspx>`__
pattern is simply that if the call fails, we can try again. It is
important to have an upper bound on retries in case a fault that appears
transient is not. See Circuit Breaker as well.

We need to exercise caution with a multiple retry approach that is does
not overload a service that already has too many requests with increased
traffic (an Attack of Self-Denial). For this reason we should delay
further retry attempts by using an algorithm that increases the interval
between retries

It is useful to use a retry policy library that encapsulates calls
within a retry policy, instead of forcing each call to a remote
component to implement retry afresh. We recommend
`**Polly** <https://github.com/michael-wolfenden/Polly>`__ as a suitable
library on the .NET framework. Brighter supports this through its
ExceptionPolicy assembly which provides an attribute for running
subsequent steps in the pipeline within a retry policy. The `Transient
Fault Handling Application
Block <http://msdn.microsoft.com/en-us/library/hh680934%28v=pandp.50%29.aspx>`__
provides similar functionality, though we don't have explicit support
for it in Brighter

Circuit Breaker
---------------

The problem with Retry to fix transient issues in a remote resource may
be that continued retries by clients of a server exacerbate the problem
- an attack of Self-Denial. In some circumstances, it can be better to
stop further requests to the server until a timeout has passed instead
of retrying.

The `Circuit
Breaker <http://msdn.microsoft.com/en-us/library/dn589784.aspx>`__
pattern is intended to protect a server that is suffering from excessive
load and prevent cascade failures from that component failing

The metaphor is of a wiring circuit breaker. When it is closed, electric
current flows, when it is open current does not flow through the wires.
This can prevent excess load from damaging the system.

We can use the same approach with calls to servers, allow calls whilst
the circuit is closed, and not when the circuit is opened following us
triggering the ciruit (perhaps due to catching an exception or timing
out. An additional aspect it that the circuit remains open for a time
period, after which we put the circuit in a half-open state. The next
call 'tests' the circuit. If the call succeeds we close the circuit, if
the call fails we wait for another time period (possibly increasing the
delay with each try) before half-opening again.

The client needs to handle a broken circuit gracefully - one option is
for a server to throttle requests when it breaks a circuit

It is useful to use a cicruit breaker library, instead of forcing each
call to implement afresh. Again we recommend
`**Polly** <https://github.com/michael-wolfenden/Polly>`__ as a suitable
library on the .NET framework. Brighter supports this through its
ExceptionPolicy assembly which provides an attribute for running
subsequent steps in the pipeline within a Circuit Breaker policy.

If you are interested in the topics here I advise you to check out
Michael Nygard's book `Release
It! <http://pragprog.com/book/mnee/release-it>`__

This project is maintained by
`iancooper <https://github.com/iancooper>`__

Hosted on GitHub Pages — Theme by
`orderedlist <https://github.com/orderedlist>`__

