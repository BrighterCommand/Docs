Outbox Pattern Support
----------------------

Event Carried State Transfer article first, then seperate this idea out

Producer Correctness
~~~~~~~~~~~~~~~~~~~~
When a microservice changes the state for which it is the system of record, and then signals to subscribers
via an event that it has changed its state, how do we ensure that subscribers receive the event and are
then consistent with the producer (even if that consistency is eventual). 

When the producing microservice stores its system of a record in its own backing store and then publishes an event 
there is no guarantee that we will both write to the backing store and send the message. 

- The new record is saved to the backing store, but the event is not raised so subscribing systems become 
inconsistent. **A lost send**.

Distributed Transactions may seem like an answer, but possess two issues. First, we are probably using a 
backing store and message-oriented middleware from different vendors or OSS projects that don't support 
the same distributed transaction protocol. Second, distributed transactions don't scale well.

We might naively try to fix this by sending the  message first, then updating the backing store if that succeeds.
But this won't necessarily work either, as we might fail to write to the database.

- The new record is posted to downstream systems, but the local database call is rejected, and so the upstream 
system is now inconsistent. A phantom send.

In either solution we might simply decide that the best option is to ensure that we can retry what is hopefully
a transient error. This may solve the problem in many instances, and is a good first step. But an endless retry
loop has its own dangers, consuming resources and reducing throughput, and if the app crashes we will still
only be partially complete. So it cannot guarantee delivery of the message that matches the write.

Solutions
~~~~~~~~~

The Outbox Pattern
------------------
In the Outbox pattern, we use the ACID properites of an RDBMS. We write not only to the table that stores the entity
that we are inserting, updating, or deleting, but also we write the message we intend to send to  an 'outbox table' 
in the same Db. 


- Compensate for failure by reversing already commmitted work. This may involve deleting from the Db or sending a reveral message. This adds complexity.

- Write to a queue, and then use a consumer on that queue to write to the database. This adds eventual consistency to the entity update on the local Db.

- Use a shared append-only log as the messaging and persistence solution, so that consumers can read the entries made by the producer. Note that this risks 
  becoming *shared database* integration and you have to be careful with schema design, think about using weak schema in consumer deserialization etc.
- Tail the transaction log on the Db and send messages in response to new entries in the transaction log. 

- 

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

 




 
