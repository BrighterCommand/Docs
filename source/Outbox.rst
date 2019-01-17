Outbox Pattern Support
----------------------

Producer Correctness
~~~~~~~~~~~~~~~~~~~~

Imagine that I have a Membership service that manages users of my application. We can register, unregister and update the user details through our service. When we perform of these actions we send a message: UserRegistered, UserDeregistered, and UserDetailsUpdated. Although these messages may seem to be events or notifications, we include in them the details of the user, so that both UserRegistered and UserDetailsUpdated act as document messages, with both context and content. Downstream services ca choose to respond to these messges by keeping a local copy of the user record, saving round-trips to the membership service. The records are effectively read-only, because to change Membership, the *system of record* you must use its API.

It is important in this scenario that the data in Membership and any downstream services are **consistent**, with the provision that it many be *eventually* ** consistent**. For example, if the user changes their contact details, we want downstream systems to agree on the new contact details with the upstream Membership system.

In many scenarios that Membership service stores its system of a record in its own database and then posts the message onto the queue with that change. Or vice-versa.

This raises a number of potential failure modes, depending on the order of the operation:

- The new Membership User record is saved to the Db, but the event is not raised so downsteam systems become inconsistent. A lost update.
- The new Membership User record is posted to downstream systems, but the local database call is rejected, and so the upstream system is now inconsistent. An inconsistent update.


Solutions
~~~~~~~~~

There are a number of solutions to this problem:

- Keep retrying until everything succeeds. This can lead to issues of the consumption of resources on the producer in retry - you need to timeout eventually.
- Use a distributed transaction. This requires the ability for both queue and Db to be enlist in a distributed transaction. This scales badly.
- Compensate for failure by reversing already commmitted work. This may involve deleting from the Db or sending a reveral message. This adds complexity.
- Write to a queue, and then use a consumer on that queue to write to the database. This adds eventual consistency to the entity update.
- Use a shared append only log as the messaging and persistence solution, so that consumers can read the entries made by the producer. Note that this risks 
  becoming *shared database* integration and you have to be careful with schema design, think about using weak schema in consumer deserialization etc.
- Tail the transaction log on the Db and send messages in response to new entries in the transaction log. 
- The Outbox pattern (http://gistlabs.com/2014/05/the-outbox/). Write the message to an 'outbox' in the Db as part of the same transaction that updates entity 
  state. Then send it to the queue later, retrying on failure. This allows us to guarantee we can send a consistent message, eventually. This has similarities 
  to the tail the transaction log approach.     


Brighter and Post
~~~~~~~~~~~~~~~~~

The Post method on Brighter writes first to the MessageStore and if that succeeds to the message broker. This can cause a problem if you intend to use it to synchronize producer and consumer state.


Post After of the Db transaction
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

In this approach you choose to Post a message after your Db transaction writes entity state to the Db. You intend to rely on the *keep retrying* approach in this case. You should make sure that you have setup your **CommandProcessor.RETRYPOLICY** policy with this in mind.

One caveat here is to look at the interaction of the retry on Post and any **UsePolicy** attribue for the handler. If your Post hander retry policy bubbles up an exception following the last Retry attempt, and your **UsePolicy** attribue for the handler then catches that exception for your handler and forces a Retry, you will end up re-running the database transaction, which may result in duplicate entries. Your **UsePolicy** attribue for the handler needs to explicitly catch Db errors, and not errors Posting to the message queue in this case.

In this case, you might also wish to consider using a **Fallback** method via the FallbackPolicy attribute to catch Post exceptions and issue a reversing transaction to kill any Db entries made in error, or raise a log to ensure that there will be manual compensation. If the failure was on the call to the transport, you should have a MessageBox entry that you can resend via manual compensation.

Note that a message may be saved to the **MessageStore** and can thus be replayed, even if the Post to the broker fails. If the message is posted to the broker, it **must** have been written to the **MessageStore**.

In this case failure to write to the Db, will mean that no message is posted to the broker.

Post Inside the Db transaction
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

In this approach, you choose to issue your Post within the Db transation. You need to carefully consider the cost of posting a message to your broker, and the likely impact on the length of your Db transaction and scaling because of that, before choosing this option. If you are using a RMQ broker on the same rack as your application in a data center the latency is low, and you may be able to use this approach without any scaling concerns. But if you are using SNS over a WAN then you will probably find the the latency causes you to hold open the Db transaction so long that you cannot scale as you timeout on Db transactions waiting for locks to clear.

Under this approach the write to your **MessageStore** is part of the Db transaction that writes the entity, and will be rolled back along with the entity state change if the entity state change write fails, or the post to the broker fails. 

Because this is not a distributed transaction, it is possible that the write to the broker will succeed, but the Db transaction will fail, resulting in you sending the event indicating a change to the broker for consumption by consumers, but rolling the change back on the producer. This will cause downsteam systems to become incosistent.

Recommendation
^^^^^^^^^^^^^^

If you use Post we recommend using the *Post after the Db transaction* approach, and ensure that you have your policies setup to provide an effective retry strategy agains failure to post to the broker such that it does not cause a repeat attempt to persist entity state (i.e. only catch Db exceptions).


Brighter and DepositPost with ClearPostBox
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Brighter allows the write to the Database and the write to the broker to be separated.

Within your database transaction you write the message to the store with **DepositPost**. This means that if the entity write succeeds, the corresponding write to the **MessageStore** will have taken place. This method returns the Id for that message.

You can then call **ClearPostBox** to write one ore more messages in the **MessageStore** to the broker. Note that you cannnot guarantee that this will succeed, although you can retry. However, as the message is now in the **MessageStore** you can compensate for failure to write to the broker by replaying the message from the **MessageStore** at a later time.

This form or Brighter allows you to support Producer-Consumer correctness via the Outbox pattern: http://gistlabs.com/2014/05/the-outbox/ Think of this as a post box, where you deposit a letter and require a clearance by the post office to ensure the letter is transmitted.

It provides a stronger guarantee than the Post outside Db transaction with Retry approach as the write to the **MessageStore** shares a transaction with the persistance of entity state. 

The cost of this approach is that you have to remember to write both lines of code, one to save and one to post to the broker. 


 




 
