`Next <Implementing%20Ports%20and%20Adapters.html>`__

`Prev <TasksExample.html>`__

Control Bus
-----------

What is a Control Bus?
~~~~~~~~~~~~~~~~~~~~~~

A `Control Bus is defined by
Wikipedia <https://en.wikipedia.org/wiki/Control_bus>`__ as:

(part of) a computer bus, used by CPUs for communicating with other
devices within the computer. While the address bus carries the
information on which device the CPU is communicating with and the data
bus carries the actual data being processed, the control bus carries
commands from the CPU and returns status signals from the devices

Within a system comprising Distributed Task Queues it can be thought of
as a mechanism for controlling and managing the queues without
interferring with dispatch to those queues.

The control bus within Brighter is aligned to that defined by Greghor
Hope:

"Use a Control Bus to manage an enterprise integration system." in his
`Enterprise Integration
Patterns <http://www.enterpriseintegrationpatterns.com/patterns/messaging/ControlBus.html>`__

|image0|

Gregor additionally further states that a Control Bus;

uses the same messaging mechanism used by the application data, but uses
separate channels to transmit data

Brighter Control Bus
~~~~~~~~~~~~~~~~~~~~

Within a Brighter context this results in the use of the same mechanism
of dispatch for control bus messages as for Commands or Events - namely
using the ChannelFactory (this ensures the same mechanism is used). An
alternative channel is configured by using an alternative **broker** to
transmit the Control Messages. This ensures independence to the Task
Messages and adds additional resilience in times of faults on the
Messaging broker.

The Control Bus is to be used for Controlling and Configuring Tasks
queues. It is also used for broadcasting and issuing `Monitoring
information <Monitoring.html>`__

Configuring a Control Bus
~~~~~~~~~~~~~~~~~~~~~~~~~

As covered in `Configuring the Distributed Task
Queue <DistributedTaskQueueConfiguration.html>`__ the following need to
be considered:

-  Channel Factory
-  Connection List

For the following example we will again be using a rabbitMQ Channel,
with an independent, named connection. We'll assume the Configuration
Section is already present in the config file (again detailed in
`Configuring the Distributed Task
Queue <DistributedTaskQueueConfiguration.html>`__)'.

Then the channel connection for the control bus can be specified to use
rabbit using the `standard rabbitMq configuration for
brighter <RabbitMQConfiguration.html>`__:

::

    <rmqMessagingGateway>
        <connection name="monitoring">
          <amqpUri uri="amqp://uri:port" connectionRetryCount="9" retryWaitInMilliseconds="50" circuitBreakTimeInMilliseconds="60000" />
          <exchange name="exchangeName" type="direct" durable="true" />
          <queues highAvailability="true" />
        </connection>
    </rmqMessagingGateway>

The next step is to instantiate a Control Bus using the Brighter
Factory, referring to the Connection configured, in this case
"monitoring":

::

    container.Register(new ControlBusSenderFactory()
    .Create(new InMemoryMessageStore(), new RmqMessageProducer("monitoring")));

A Control Bus is now active and ready to transmit monitoring messages.

.. |image0| image:: http://www.enterpriseintegrationpatterns.com/img/ControlBus.gif

