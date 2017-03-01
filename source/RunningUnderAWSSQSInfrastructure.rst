Running Brighter under AWS SQS Infrastructure
---------------------------------------------

Getting your application to interact with SQS using Brighter is a
trivial task. But, there are couple of things that needs to be done
manually.

First, you should have your AWS Access Key ID and Secret Access Key
ready to be used by AWSSDK. You can check out the AWS documentation on
how to create an account
`here <https://docs.aws.amazon.com/AWSSdkDocsNET/latest/DeveloperGuide/net-dg-setup.html#net-dg-signup>`__

After setting up your credentials, you can either create a profile in
your local computer or you can use the access and secret keys directly
in the application configuration file. Here, we will create a profile
using `AWS Toolkit on Visual
Studio <https://aws.amazon.com/visualstudio/>`__:

Click to add profile button on AWS Explorer window.

|AWSToolkitOverview|

Then fill the form to create your profile.

|AWSToolkitCreateAccount|

You can find more information about profiles and credentials
`here <https://docs.aws.amazon.com/AWSSdkDocsNET/latest/DeveloperGuide/net-dg-config-creds.html>`__.

Creating queues
~~~~~~~~~~~~~~~

Unfortunately, brighter will not be creating the queues on request for
you; you need to create them manually. To create queues, you should go
to your AWS Console.

|AWSConsoleList|

Then create the queue by filling the form below.

|AWSConsoleCreateQueue|

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

Instead of posting messages directly to the queues, Brighter posts
them to a topic so that any queue can listen or stop listening messages
without changing any code. AWS SNS is used for that purpose.

|AWSConsoleTopics|

You can create a topic by clicking on create topic link |AWSConsoleCreateTopic|

After that, simply go back to your queue list, select a queue and
click on Subscribe Queue to SNS Topic link at Queue Actions menu.

|AWSConsoleLinkToSNS|

Select your SNS queue and now your queue will get the messages which are
sent to the SNS topic.

Please follow the link below to see an example application configuration
to use AWS SQS:

`AWS SQS Configuration <AWSSQSConfiguration.html>`__

.. |AWSToolkitOverview| image:: _static/images/AWSToolkitOverview.png
.. |AWSToolkitCreateAccount| image:: _static/images/AWSToolkitCreateAccount.png
.. |AWSConsoleList| image:: _static/images/AWSConsoleList.png
.. |AWSConsoleCreateQueue| image:: _static/images/AWSConsoleCreateQueue.png
.. |AWSConsoleTopics| image:: _static/images/AWSConsoleTopics.png
.. |AWSConsoleCreateTopic| image:: _static/images/AWSConsoleCreateTopic.png
.. |AWSConsoleLinkToSNS| image:: _static/images/AWSConsoleLinkToSNS.png

