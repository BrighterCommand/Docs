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

`Paramore Home <index.html>`__

Paramore
========

Paramore contains libraries for implementing hierarchical architectural
styles in .NET specifically `Ports and
Adapters <http://alistair.cockburn.us/Hexagonal+architecture>`__ and
`CQRS <https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf>`__.

In addition, Brighter supports `Distributed Task
Queues <http://parlab.eecs.berkeley.edu/wiki/_media/patterns/taskqueue.pdf>`__.
As such it can be used to improve performance by introducing concurrency
using a queue, and/or as an integration strategy between
`Microservices <http://martinfowler.com/articles/microservices.html>`__
using messaging via a `lightweight
broker <http://martinfowler.com/articles/microservices.html#SmartEndpointsAndDumbPipes>`__.

The projects here are:

-  |Brighter Canon| **`Brighter <BrighterVersion.html>`__**: This
   project is a Command Processor & Dispatcher implementation that can
   be reused in other projects. It also provides a Task Queue so that
   commands can be handled asynchronously.
-  |Light Bulb| **Brightside**: A Python project that provides a
   ServiceActivator compatible with Brighter, implemented in Python.
   *Under Construction*.

Paramore.Contrib
----------------

Supporting projects for **Paramore** this repository contains example
code such as **Renegade**, examples, utility code such as IoC specific
handler factory examples, and parts of the tool chain such a RestMS
server implementation. It is where non-core elements of the project
live.

-  |Bike| **Renegade**: An implementation of the
   `RESTMS <http://www.restms.org/>`__ protocol. This is not production
   grade. Originally built to provide an AMQP-like protocol over HTTP to
   confirm the design of Paramore.Brighter.ServiceActivator
-  |Brighter Canon| **Rewind**: A project demonstrating the CQRS
   architectural pattern and the use of DDD to create a domain model.
   The project also uses RavenDb as a backing store - partially to show
   the affinity between aggregates and document databases. In addition
   the project uses OpenRasta to expose functionality as a REST endpoing
   and an SPA to provide web access. Old code that will be re-worked for
   ASP.NET Core so *Under Construction.*

You can find more information on the projects over on the wiki

This project is maintained by
`iancooper <https://github.com/iancooper>`__

Hosted on GitHub Pages — Theme by
`orderedlist <https://github.com/orderedlist>`__

.. |Brighter Canon| image:: https://openclipart.org/people/amilo/canon.svg
.. |Light Bulb| image:: https://openclipart.org/download/97987/bulb-01.svg
.. |Bike| image:: https://openclipart.org/download/170451/biker-b-w.svg
.. |Brighter Canon| image:: https://openclipart.org/people/pydubreucq/replay-sign.svg

