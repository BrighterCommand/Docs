`Next <CommandsCommandDispatcherandProcessor.html>`__

`Prev <PortsAndAdapters.html>`__

Implementing Ports and Adapters
-------------------------------

A ports and adapters architecture is an `architectural
style <https://www.cs.cmu.edu/afs/cs/project/vit/ftp/pdf/intro_softarch.pdf>`__
that follows the layered architectural style, but has some additional
constraints and invariants around the layers.

The Components are layers. Each component comprises a group of sub-tasks
which implement an abstraction at some layer in the hierarchy. Each
layer provides services to the layer above it and acts as a client to
the layer below.

The Connectors are the protocols that define how the layers can
interact.

Lower layers can pass data and service requests to higher layers via
notifications (i.e. an observable)

A hexagonal architecture has three layers: adapter, ports, and domain

The Domain Layer
----------------

The domain is constrained to avoid concerns outside the process boundary
i.e. I/O and as such should consist of
`POCO <http://en.wikipedia.org/wiki/Plain_Old_CLR_Object>`__ classes.
Thus we should not have references to libraries such as System.Web and
System.IO which live in the layers that implements communication with
us, or from us to other processes.

We include developer tests as something that lives outside the domain,
so we should have no testing framework in this layer either

The Adapter Layer
-----------------

Adapters are where we implement communication between our process and
other processes. This is where the frameworks we use for I/O and our
tests live. They layer is constrained not to communicate directly with
the domain, but a port.

The Ports Layer
---------------

A port is the means by which an adapter exercises the domain. The
adapter may not exercise the domain directly but must use a port which
forms a use case or scenario boundary for the domain.

The port provides co-ordination and control of the domain in response to
a request from a primary adapter,and communication of other ports with
secondary adapters.

We recommend using the
`Command <CommandsCommandDispatcherandProcessor.html>`__ pattern to
implement Ports.

This use case is also described as implementing the Command Invoker
pattern within a service by `Service Design
Patterns <http://www.servicedesignpatterns.com/>`__. The `Command
Invoker <http://servicedesignpatterns.com/WebServiceImplementationStyles/CommandInvoker>`__:

    Create command objects that fully encapsulate common request
    processing logic. Instantiate and invoke these commands from within
    the web service, or forward them to an asynchronous background
    process.

We would identify this as Ports layer, with the service endpoint being
in the Adapters layer.
