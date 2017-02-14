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

`Next <AWSSQSConfiguration.html>`__

`Prev <RabbitMQConfiguration.html>`__

Brighter
========

Running Brighter under AWS SQS Infrastructure
---------------------------------------------

Getting your application to interact with SQS using Brighter is a
trivial task. But, there are couple of things that needs to be done
manually.

First, you should have your AWS Access Key ID and Secret Access Key
ready to be used by AWSSDK. You can check out the AWS documentation on
how to create an account
`here <http://docs.aws.amazon.com/AWSSdkDocsNET/latest/DeveloperGuide/net-dg-setup.html#net-dg-signup>`__

After setting up your credentials, you can either create a profile in
your local computer or you can use the access and secret keys directly
in the application configuration file. Here, we will create a profile
using `AWS Toolkit on Visual
Studio <http://aws.amazon.com/visualstudio/>`__:

| Click to add profile button on AWS Explorer window.
| |image0|

| Then fill the form to create your profile.
| |image1|

You can find more information about profiles and credentials
`here <http://docs.aws.amazon.com/AWSSdkDocsNET/latest/DeveloperGuide/net-dg-config-creds.html>`__.

Creating queues
~~~~~~~~~~~~~~~

| Unfortunately, brighter will not be creating the queues on request for
you; you need to create them manually. To create queues, you should go
to your AWS Console.
| |image2|

| Then create the queue by filling the form below.
| |image3|

-  **Default Visibility Timeout:** The length of time that a message
   received from a queue will be invisible to other receiving
   components.
-  **Message Retention Period:** The amount of time that Amazon SQS will
   retain amessage if it doesn't get deleted.
-  **Maximum Message Size:** Maximum message size in bytes.
-  **Delivery Delay:** The amount of time to delay the first delivery of
   all messages added to the queue.
-  **Receive Message Wait Time:** The maximum amount of time that a long
   polling receive call will wait for a message to become available
   before returning empty response.

After saving the queue, get the queue url to put it in the application
configuration file.

Posting messages
~~~~~~~~~~~~~~~~

| Instead of posting messages directly to the queues, Brighter posts
them to a topic so that any queue can listen or stop listening messages
without changing any code. AWS SNS is used for that purpose.
| |image4|

You can create a topic by clicking on create topic link |image5|

| After that, simply go back to your queue list, select a queue and
click on Subscribe Queue to SNS Topic link at Queue Actions menu.
| |image6|

Select your SNS queue and now your queue will get the messages which are
sent to the SNS topic.

Please follow the link below to see an example application configuration
to use AWS SQS:

`AWS SQS Configuration <AWSSQSConfiguration.html>`__

This project is maintained by
`iancooper <https://github.com/iancooper>`__

Hosted on GitHub Pages — Theme by
`orderedlist <https://github.com/orderedlist>`__

.. |image0| image:: images/AWSToolkitOverview.png
.. |image1| image:: images/AWSToolkitCreateAccount.png
.. |image2| image:: images/AWSConsoleList.png
.. |image3| image:: images/AWSConsoleCreateQueue.png
.. |image4| image:: images/AWSConsoleTopics.png
.. |image5| image:: images/AWSConsoleCreateTopic.png
.. |image6| image:: images/AWSConsoleLinkToSNS.png

