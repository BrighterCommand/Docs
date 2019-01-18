Outbox Pattern Support
----------------------

Producer Correctness
~~~~~~~~~~~~~~~~~~~~

Imagine that I have a Membership service that manages users of my application. We can register, unregister and update the user details through our service. When we perform of these actions we send a message: UserRegistered, UserDeregistered, and UserDetailsUpdated. Although these messages may seem to be events or notifications, we include in them the details of the user, so that both UserRegistered and UserDetailsUpdated act as document messages, with both context and content. 

Downstream services can choose to react to these messges by creating and updating a local read-only cache of Membership's User record, saving round-trips to the membership service when it needs User information. The records are effectively read-only, because to change Membership, the *system of record* you must use its API.

It is important in this scenario that the data in Membership and any downstream services are **consistent**, with the provision that it may reach consistency *eventually*. For example, if the user changes their contact details, we want downstream systems to agree on the new User contact details with the upstream Membership system.

In many scenarios that Membership service stores its system of a record in its own database and then posts the message onto the queue with that change. Or vice-versa.

This raises a number of potential failure modes, depending on the order of the operation:

- The new Membership User record is saved to the Db, but the event is not raised so downsteam systems become inconsistent. A lost send.
- The new Membership User record is posted to downstream systems, but the local database call is rejected, and so the upstream system is now inconsistent. A phantom send.


Solutions
~~~~~~~~~

There are a number of solutions to this problem:

- Keep retrying the write to the broker until everything succeeds. This can lead to your application consuming all of the resources on the producer 
  in a retry loop, you need to timeout eventually at which point you get a lost send.
- Use a distributed transaction. This requires the ability for both queue and Db to be enlist in a distributed transaction. This scales badly.
- Compensate for failure by reversing already commmitted work. This may involve deleting from the Db or sending a reveral message. This adds complexity.
- Write to a queue, and then use a consumer on that queue to write to the database. This adds eventual consistency to the entity update on the local Db.
- Use a shared append-only log as the messaging and persistence solution, so that consumers can read the entries made by the producer. Note that this risks 
  becoming *shared database* integration and you have to be careful with schema design, think about using weak schema in consumer deserialization etc.
- Tail the transaction log on the Db and send messages in response to new entries in the transaction log. 
- The Outbox pattern (http://gistlabs.com/2014/05/the-outbox/). Write the message to an 'outbox' in the Db as part of the same transaction that updates entity 
  state. Then send it to the queue later, retrying on failure. This allows us to guarantee we can send a consistent message, eventually. This has similarities 
  to the tail the transaction log approach.     


CommandProcessor.Post
~~~~~~~~~~~~~~~~~~~~~

The Post method on Brighter writes first to the MessageStore and if that succeeds to the message broker. This can cause a problem if you intend to use it to synchronize producer and consumer state.


CommandProcessor.Post After the Db transaction
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

In this approach you choose to Post a message after your Db transaction writes entity state to the Db. You intend to rely on the *keep retrying* approach in this case. You should make sure that you have setup your **CommandProcessor.RETRYPOLICY** policy with this in mind.

One caveat here is to look at the interaction of the retry on Post and any **UsePolicy** attribue for the handler. If your **CommandProcessor.RETRYPOLICY** policy bubbles up an exception following the last Retry attempt, and your **UsePolicy** attribue for the handler then catches that exception for your handler and forces a Retry, you will end up re-running the database transaction, which may result in duplicate entries. Your **UsePolicy** attribue for the handler needs to explicitly catch the Db errors you wish to retry, and not errors Posting to the message queue in this case.

(As an aside, you should generally write Retry policies to catch specific errors that you know you can retry, not all errors anyway).

In this case, you might also need to consider using a **Fallback** method via the FallbackPolicy attribute to catch Post exceptions that bubble out and issue a reversing transaction to kill any Db entries made in error, or raise a log to ensure that there will be manual compensation. 

If the failure was on the call to the transport, and not the write to the **MessageStore**, you will still have a **MessageStore** entry that you can resend via manual compensation later. If the message is posted to the broker, it **must** have already been written to the **MessageStore**.

In you fail to write to the **MessageStore**, but have successfully written the entity to the Db, you would need to compensate by reversing the write to the Db in a **Fallback** handler.

CommandProcessor.Post Inside the Db transaction
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

In this approach, you choose to issue your Post within the Db transation. You need to carefully consider the cost of posting a message to your broker, and the likely impact on the duration of your transaction and scaling impact of that, before choosing this option. If you are using a RMQ broker on the same rack as your application in a data center the latency is low, and you may be able to use this approach without any scaling concerns. If you are using SNS+SQS over a WAN then you will probably find the the latency causes you to hold open the Db transaction too long.

Under this approach the write to your **MessageStore** is part of the transaction that writes the entity, and will be rolled back along with the entity state change on any failure. So your **MessageStore** will be consistent with your entity store.

Because this is not a distributed transaction, it is possible that the write to the broker will succeed, but the Db transaction will then fail, resulting in you sending the event indicating a change to the broker for consumption by consumers, but rolling the change back on the producer,a phantom message. This will cause downsteam systems to become incosistent.


CommandProcessor.Post and Db Transactions: Our Recommendation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

If you use **CommandProcessor.Post** we recommend using the *Post after the Db transaction* approach, and ensure that you have your policies setup to provide an effective retry strategy agains failure to post to the broker such that it does not cause a repeat attempt to persist entity state (i.e. only catch Db exceptions).


CommandProcessor.DepositPost with CommandProcessor.ClearPostBox
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Brighter allows the write to the **MessageStore** and the write to the Broker to be separated. This form or Brighter allows you to support Producer-Consumer correctness via the Outbox pattern: http://gistlabs.com/2014/05/the-outbox/ 

Metaphorically, you can think of this as a post box. You deposit a letter in a post box. Later the postal service clears the post box of letters and delivers them to their recipients. 

Within your database transaction you write the message to the store with **CommandProcessor.DepositPost**. This means that if the entity write succeeds, the corresponding write to the **MessageStore** will have taken place. This method returns the Id for that message. (Note that we use **CommandProcessor.RETRYPOLICY** on the write, but this will only impact the attempt to write within the transaction, not the success or failure of the overall Db transaction, which is under your control. You can safely ignore Db errors on this policy within this approach for this reason.)

You can then call **CommandProcessor.ClearPostBox** to write one or more messages from the **MessageStore** to the broker. We support multiple messages as your entity write might possibly involve sending multiple downstream messages, which you want to include in the transaction. Note that you cannnot guarantee that this will succeed, although you can Retry. We use **CommandProcessor.RETRYPOLICY** on the write to the Broker, and you should retry errors writing to the Broker in that policy. However, as the message is now in the **MessageStore** you can compensate for eventual failure to write to the Broker by replaying the message from the **MessageStore** at a later time.

You are responsible for tracking the ids of messages that you wish to send in **CommandProcessor.ClearPostBox**, we do not maintain this state for you.


It provides a stronger guarantee than the **CommandProcessor.Post** outside Db transaction with Retry approach as the write to the **MessageStore** shares a transaction with the persistance of entity state. 

The cost of this approach is that you have to remember to write both lines of code, one to save and one to post to the broker outside the Db transaction. 

CommandProcessor.DepositPost and Db Transactions: Our Recommendation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

When you want to sychronize new entities, or changes to entity state, with a downstream system, we recommend using the Outbox pattern and using **CommandProcessor.DepositPost**. within the Db transaction and **CommandProcessor.ClearPostBox** once the transaction successfully completes.

 




 
