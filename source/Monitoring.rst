Monitoring
----------

Brighter emits monitoring information from Task Queues using a
configured `Control Bus <https://brightercommand.github.io/Brighter/ControlBus.html>`__

Configuring Monitoring
~~~~~~~~~~~~~~~~~~~~~~

Firstly `configure a Control Bus <https://brightercommand.github.io/Brighter/ControlBus.html#configure>`__ in the
brighter application to emit monitoring messages

Config file
~~~~~~~~~~~

Monitoring requires a new section to be added to the application config
file:

.. highlight:: xml

::

    <configSections>
        <section name="monitoring" type ="paramore.brighter.commandprocessor.monitoring.Configuration.MonitoringConfigurationSection, Brighter.commandprocessor" allowLocation ="true" allowDefinition="Everywhere"/>
    </configSections>

The monitoring config can then be speicified later in the file:

.. highlight:: xml

::

    <monitoring>
        <monitor isMonitoringEnabled="true" instanceName="ManagementAndMonitoring"/>
    </monitoring>

This enables runtime changes to enable/disable emitting of monitoring
messages.

Handler confguration
~~~~~~~~~~~~~~~~~~~~

Each handler that requires monitoring must be configured in two stages,
a Handler attribute and container registration of a MonitorHandler for
the given request:

For example, given:

-  TRequest - a Brighter Request, inheriting from IRequest
-  TRequestHandler - handles the TRequest, inheriting IHandleRequest
   <TRequest>

Attribute
^^^^^^^^^

The following attribute must be added to the Handle method in the
handler, TRequestHandler:

.. highlight:: csharp

::

    [Monitor(step:1, timing:HandlerTiming.Before, handlerType:typeof(TRequestHandler))]

Please note the step and timing can vary if monitoring should be after
another attribute step, or timing should be emitted after.

Container registration
^^^^^^^^^^^^^^^^^^^^^^

The following additional handler must be registered in the application
container (where ``MonitorHandler<T>`` is a built-in Brighter handler):

.. highlight:: csharp

::

    container.Register<TRequest, MonitorHandler<TRequest>>

Monitor message format
~~~~~~~~~~~~~~~~~~~~~~

A message is emitted from the Control Bus on Handler Entry and Handler
Exit. The following is the form of the message:

.. highlight:: javascript

::

    {
        "Exception": null, // or Exception message
        "EventType": "EnterHandler or ExitHandler",
        "EventTime": "2016-06-21T15:48:26.1390192Z",
        "TimeElapsedMs": 0 or Duration,
        "HandlerName": "...",
        "HandlerFullAssemblyName": "...",
        "InstanceName": "ManagementAndMonitoring",
        "RequestBody": "{\"Id\":\"dc32b35f-bc75-4197-9178-c8310a63e4fb\", ... }",
        "Id": "048cc207-e820-40fa-b931-55b60203fbc2"
    }

Messages can be processed from the queue and interated with your
monitoring tool of choice, for example Live python consumers emitting to
console or logstash consumption to the ELK stack using relevant plugins
to provide performance raditators or dashboards.
