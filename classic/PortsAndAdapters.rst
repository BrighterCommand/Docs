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

`Next <Implementing%20Ports%20and%20Adapters.html>`__

`Prev <TasksExample.html>`__

Brighter
========

Ports & Adapters
----------------

`Ports and
Adapters <http://alistair.cockburn.us/Hexagonal+architecture>`__ is a
*hierarchical architectural style* which makes clear the separation
between the *Domain Model* - which contains the rules of our application
- and the *Adapters*, which abstract the inputs to the system and our
outputs. The advantage of this style is that the application is
decoupled from the nature of the input or output device, and any
frameworks used to implement them.

For example when a client POSTs a request to the REST API exposed by our
application the Adapter recieves the HTTP request, transforms it into a
call onto our Domain, and marshals the response back out to the client
over HTTP. Similarly if our application needs to retrieve persisted
entity state to initialise the domain it calls out to an Adapter that
wraps access to the Db.

|image0|

The layer between the Adapter and the Domain is identified as the
*Ports* layer. Our Domain is inside the Port, Adapters for external
concerns are on the outisde of the port. The notion of a 'port' invokes
the OS idea that any device that adheres to a know protocol can be
plugged into a port. Similarly, many adapters may use our Ports.

One particular asset is *testability*. Our tests can be written against
the Ports, instead of the Adapters. This decouples our tests from the
Adapters - which may change over time as we add or remove frameworks
that we wish to use to surface the application (for example switching
from Webforms to ASP.NET MVC). It also means that our functionality must
be contained inside the Port, against which the test runs, and cannot
bleed into other layers. A traditional problem of layered architectures
is bleeding of domain logic out of the Domain and into the
*Presentation* layer. The Ports and Adapters model seeks to solve this,
by forcing Adapters to use the API exposed by the port. Writing TDD unit
tests at this layer means that the test driven-development pushes the
domain logic required to implement them behind the Port.

We have a notion of *primary* and *secondary* actors in use cases which
map to the Adapter and Port layer. Primary actors exercise our
application, they are inputs into our application. A primary actor uses
a primary Adapter, which calls a primary Port - the chain is one of
inputs into our application. So our REST API is a primary Adapter, so
are our tests. A secondary actor is one that our application exercises
as part of its work, they are outputs from out application. So our Db is
a secondary Adapter, as our mocks and we talk to them over a secondary
Port. Many applications seem to consist only of one primary and one
secondary, but once we factor in tests we may begin to observe that we
have more, and by building for multiple Ports we make our application
more modifiable to new ports in future.

We often show primary Ports on the left and secondary Ports on the
right.

A Port is the 'use case boundary'. Use cases become problematic when
they become focused on technology concerns. Use cases written against
the ports can elide those concerns and focus on the application rules,
making them easier to write and maintain. There is a correlation here
between the use case boundary and the test boundary - tests should focus
on the behaviour expressed by a use case, not on a unit of code.
Contrary to Cockburn, I don't suggest using a ATDD like FIT or SpecFlow
here, preferring to use our xUnit test tool here (and not test
implementation details).

Clean and Onion Architectures
=============================

A number of architectures have similar properties to a ports and
adapters architecture, such as the `Onion
Architecture <http://jeffreypalermo.com/blog/the-onion-architecture-part-1/>`__.
In essence they all have the hierarchical architectural style,
specifically a layered style, with similar invariants and constraints
around the components and connectors within that architectural style.
`Bob Martin <http://blog.8thlight.com/uncle-bob/archive.html>`__ clubs
these together under the heading `Clean
Architecture <http://blog.8thlight.com/uncle-bob/2012/08/13/the-clean-architecture.html>`__

This project is maintained by
`iancooper <https://github.com/iancooper>`__

Hosted on GitHub Pages — Theme by
`orderedlist <https://github.com/orderedlist>`__

.. |image0| image:: images/Hexagonal%20Architecture.png

