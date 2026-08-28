# Red-proof fixture — `Subscription`

This is not a documentation page. It is the input the red-proofs in
`redproof_optioncheck.py` mutate, and it lives here rather than under
`contents/` for two reasons: `optioncheck` takes path arguments (tasks.md
§2.5), so a fixture does not have to be a published page; and AC3b has to be
proved in phase 2, before phase 3 has written a `Subscription` table anywhere
in the corpus.

**It must be green as it stands.** A red-proof that starts at its first
mutation cannot tell a rule that fires from a tool that is broken.

The table below is `Paramore.Brighter.Subscription` at `10.7.0`, written from
the type. Every `Default` is the value on a constructed instance — which for
`emptyChannelDelay` is **500 ms** where the signature says `null`, the single
measurement the whole spec rests on.

<!-- optioncheck: Paramore.Brighter.Subscription
     omit: channelFactory — not reader-set; supplied by AddConsumers
     manual: requestType — the constructor rejects its own default of null, so an instance reads back the checker's argument
     manual: getRequestType — no property of that name on the instance
     manual: messagePumpType — the constructor rejects its own default of Unknown
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `subscriptionName` | `SubscriptionName` | `none` | Names the subscription. |
| `channelName` | `ChannelName` | `none` | Names the channel the subscription reads. |
| `routingKey` | `RoutingKey` | `none` | The routing key the channel is bound to. |
| `requestType` | `Type?` | `none` | The request type messages on this channel deserialise to. |
| `getRequestType` | `Func<Message, Type>?` | `null` | Derives the request type from the message where one type per channel is too few. |
| `bufferSize` | `int` | `1` | Messages prefetched per read. |
| `noOfPerformers` | `int` | `1` | Message pumps run for this subscription. |
| `timeOut` | `TimeSpan?` | `300 ms` | How long a read waits for a message. |
| `requeueCount` | `int` | `-1` | Times a message is requeued before it is rejected; -1 is unlimited. |
| `requeueDelay` | `TimeSpan?` | `0 ms` | Delay before a requeued message is available again. |
| `unacceptableMessageLimit` | `int` | `0` | Unacceptable messages tolerated before the pump stops; 0 is unlimited. |
| `messagePumpType` | `MessagePumpType` | `none` | Selects the Reactor or Proactor pump. |
| `makeChannels` | `OnMissingChannel` | `Create` | What the transport does when the channel is missing. |
| `emptyChannelDelay` | `TimeSpan?` | `500 ms` | Pause after a read that found no message. |
| `channelFailureDelay` | `TimeSpan?` | `1000 ms` | Pause after a read that failed. |
| `unacceptableMessageLimitWindow` | `TimeSpan?` | `null` | Window the unacceptable-message limit is counted over. |
