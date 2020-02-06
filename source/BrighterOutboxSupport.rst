Correctness in Brighter
=======================

The **Post** method on the CommandProcessor in Brighter writes first to 
the **Outbox** and if that succeeds to the Message-Oriented Middleware. 
If you use Post, then your correctness options are **Ignore/Retry** or 
**Compensation**. You can use **Post** with **Log Tailing** or **Event Change Capture**
but you have to implement those yourself.

The **DepositPost** and **ClearOutbox** methods allow you to use the **Outbox** pattern 
instead.

Ignore/Retry
^^^^^^^^^^^^
In this approach you choose to Post a message after your Db transaction 
writes entity state to the Db. You intend to rely on the *retrying* the
call to the broker if it fails. You should make sure that you have setup 
your **CommandProcessor.RETRYPOLICY** policy with this in mind.

One caveat here is to look at the interaction of the retry on Post and 
any **UsePolicy** attribue for the handler. If your **CommandProcessor.RETRYPOLICY** 
policy bubbles up an exception following the last Retry attempt, 
and your **UsePolicy** attribue for the handler then catches that 
exception for your handler and forces a Retry, you will end up re-running 
the database transaction, which may result in duplicate entries. 
Your **UsePolicy** attribue for the handler needs to explicitly 
catch the Db errors you wish to retry, and not errors Posting 
to the message queue in this case.

(As an aside, you should generally write Retry policies to catch specific
errors that you know you can retry, not all errors anyway).

In this case, you might also need to consider using a **Fallback** method 
via the FallbackPolicy attribute to catch Post exceptions that bubble 
out and issue a reversing transaction to kill any Db entries made in error, 
or raise a log to ensure that there will be manual compensation. 

Post still uses the **Outbox** to store messages you send, but you are not including
them in the Db transaction scope, so you have no **guarantees**.

If the failure was on the call to the transport, and not the write to the **Outbox**, 
you will still have a **Outboxe** entry that you can resend via manual 
compensation later. If the message is posted to the broker, it **must** have 
already been written to the **Outbox**.

In you fail to write to the **MessageStore**, but have successfully written 
the entity to the Db, you would need to compensate by reversing the write to 
the Db in a **Fallback** handler.

Compensation
^^^^^^^^^^^^
In this approach, you choose to issue your Post whilst holding open the Db transaction
which writes the entity. You need to carefully consider the cost of posting a message 
to your broker, and the likely impact on the duration of your transaction 
and scaling impact of that, before choosing this option. If you are using 
an RMQ broker on the same rack as your application in a data center the 
latency is low, and you may be able to use this approach without any scaling concerns. 
If you are using SNS+SQS over a WAN then you will probably find the the 
latency causes you to hold open the Db transaction too long.

Under this approach the write to your **Outbox** is part of the transaction 
that writes the entity, and will be rolled back along with the entity 
state change on any failure. So your **Outboxe** will be 
consistent with your entity store.

Because this is not a distributed transaction, it is possible that the 
write to the broker will succeed, but the Db transaction will then fail, 
resulting in you sending the event indicating a change to the 
broker for consumption by consumers, but rolling the change back on the producer, 
a phantom message. This will cause downstream systems to become inconsistent.


Outbox
^^^^^^

Brighter allows the write to the **Outbox** and the write to the Broker to be separated. 
This form or Brighter allows you to support Producer-Consumer correctness via 
the **Outbox Pattern**. 

Metaphorically, you can think of this as a post box. You deposit a letter in a post box. 
Later the postal service clears the post box of letters and delivers them to their 
recipients. 

Within your database transaction you write the message to the Outbox 
with **CommandProcessor.DepositPost**. This means that if the entity 
write succeeds, the corresponding write to the **Outbox** will have taken place. 
This method returns the Id for that message. 

(Note that we use **CommandProcessor.RETRYPOLICY** on the write, 
but this will only impact the attempt to write within the transaction, 
not the success or failure of the overall Db transaction, which is under 
your control. You can safely ignore Db errors on this policy within this 
approach for this reason.)

You can then call **CommandProcessor.ClearPostBox** to flush one or more 
messages from the **Outbox** to the broker. We support multiple 
messages as your entity write might possibly involve sending multiple 
downstream messages, which you want to include in the transaction. 
Note that you cannnot guarantee that this will succeed, although you can 
Retry. We use **CommandProcessor.RETRYPOLICY** on the write to the Broker, 
and you should retry errors writing to the Broker in that policy. 
However, as the message is now in the **Outbox** you can compensate for 
eventual failure to write to the Broker by replaying the message from 
the **MessageStore** at a later time.

You are responsible for tracking the ids of messages that you wish 
to send in **CommandProcessor.ClearPostBox**, we do not maintain this state for you.

It provides a stronger guarantee than the **CommandProcessor.Post** outside Db 
transaction with Retry approach as the write to the **Outbox** shares a 
transaction with the persistance of entity state. 

Log Tailing
-----------

We do not provide explicit support for log tailing (as yet). You would need to implement
reading from a transaction log yourself. You should be able to use **CommandProcessor.Post** 
to dispatch the messages from the log, as you do not have a Db transaction to co-ordinate
with.

Event Capture
-------------

No additional support is needed to support Event Capture. Use **CommandProcessor.Post** to
send the message that triggers a change to entity state, write a handler to consume that
message and write to the database, and then configure a consumer of that message as normal.






 
