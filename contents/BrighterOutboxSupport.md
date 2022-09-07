# Outbox Support

TODO: This needs enhancement to support V9

Brighter supports storing messages that are sent via an External Bus in an Outbox, as per the [Outbox Pattern](/contents/OutboxPattern.md)

This allows you to determine that a change to an entity owned by your application should always result in a message being sent i.e. you have Transactional Messaging.

There are two approaches to using Brighter's Outbox:

* Post: This does not offer Transactional Messaging, but does offer replay
* Deposit and Clear: This approach offers Transactional Messaging.

The **Post** method on the CommandProcessor in Brighter writes first to the **Outbox** and if that succeeds to the Message-Oriented Middleware. If you use Post, then your correctness options are **Ignore/Retry** or **Compensation**. You can use **Post** with **Log Tailing** or **Event Change Capture** but you have to implement those yourself.

The **DepositPost** and **ClearOutbox** methods allow you to use the **Outbox** pattern instead.

## Post

In this approach you choose to **CommandProcessor.Post** a message after your Db transaction writes entity state to the Db. You intend to rely on the *retrying* the call to the broker if it fails. You should make sure that you have setup your **CommandProcessor.RETRYPOLICY** policy with this in mind.

One caveat here is to look at the interaction of the retry on Post and any **UsePolicy** attribute for the handler. If your **CommandProcessor.RETRYPOLICY** policy bubbles up an exception following the last Retry attempt, and your **UsePolicy** attribute for the handler then catches that exception for your handler and forces a Retry, you will end up re-running the database transaction, which may result in duplicate entries. Your **UsePolicy** attribute for the handler needs to explicitly catch the Db errors you wish to retry, and not errors Posting to the message queue in this case.

(As an aside, you should generally write Retry policies to catch specific errors that you know you can retry, not all errors anyway).

In this case, you might also need to consider using a **Fallback** method via the FallbackPolicy attribute to catch **CommandProcessor.Post** exceptions that bubble out and issue a reversing transaction to kill any Db entries made
in error, or raise a log to ensure that there will be manual compensation.

**CommandProcessor.Post** still uses the **Outbox** to store messages you send, but you are not including them in the Db transaction scope, so you have no **guarantees**.

If the failure was on the call to the transport, and not the write to the **Outbox**, you will still have a **Outbox** entry that you can resend via manual compensation later. If the message is posted to the
broker, it **must** have already been written to the **Outbox**.

In you fail to write to the **Outbox**, but have successfully written the entity to the Db, you would need to compensate by reversing the write to the Db in a **Fallback** handler.

## Deposit and Clear

Brighter allows the write to the **Outbox** and the write to the Broker to be separated. This form or Brighter allows you to support Producer-Consumer correctness via the **Outbox Pattern**.

Metaphorically, you can think of this as a post box. You deposit a letter in a post box. Later the postal service clears the post box of letters and delivers them to their recipients.

Within your database transaction you write the message to the Outbox with **CommandProcessor.DepositPost**. This means that if the entity write succeeds, the corresponding write to the **Outbox** will have
taken place. This method returns the Id for that message.

(Note that we use **CommandProcessor.RETRYPOLICY** on the write, but this will only impact the attempt to write within the transaction, not the success or failure of the overall Db transaction, which is under
your control. You can safely ignore Db errors on this policy within this approach for this reason.)

You can then call **CommandProcessor.ClearPostBox** to flush one or more messages from the **Outbox** to the broker. We support multiple messages as your entity write might possibly involve sending multiple downstream messages, which you want to include in the transaction. 

It provides a stronger guarantee than the **CommandProcessor.Post** outside Db transaction with Retry approach as the write to the **Outbox** shares a transaction with the persistance of entity state.

### Immediate or Sweeper

You can call **CommandProcessor.ClearPostBox** directly in your handler, after the Db transaction completes. This has the lowest latency. You are responsible for tracking the ids of messages that you wish to send in **CommandProcessor.ClearPostBox**, we do not maintain this state for you. Note that you cannnot guarantee that this will succeed, although you can Retry. We use **CommandProcessor.RETRYPOLICY** on the write to the Broker, and you should retry errors writing to the Broker in that policy. However, as the message is now in the **Outbox** you can compensate for eventual failure to write to the Broker by replaying the message from the **MessageStore** at a later time.

Alternatively you can use an •Outbox Sweeper* on a dedicated threat that looks for messages that need to be dispatched and sends them. This has lower latency, but because it keeps trying to send the messages until it succeeds is the recommended approach to *Guranteed, At Least Once, Delivery*.

You can combine these by using immediate clearance, and relying on the Outbox Sweeper to pick up anything that was missed. In that case make sure that you ignore errors caused by a failed call to **CommandProcessor.ClearPostBox** in your handler.
