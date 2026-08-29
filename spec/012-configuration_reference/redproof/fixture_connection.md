# Fixture — a type with an environment-derived default

Not a documentation page. `redproof_optioncheck.py` mutates this file; it lives
outside `contents/` so no gate in this repository reads it, and `optioncheck`
sees it only because it takes path arguments.

`RmqMessagingGatewayConnection.Name` is `= Environment.MachineName`. The checker
can read that value and cannot determine the default, because the value is this
machine's hostname and the CI runner's is different — so the row is `manual:`,
and deleting the declaration must be a finding rather than a pass.

<!-- optioncheck: Paramore.Brighter.MessagingGateway.RMQ.Async.RmqMessagingGatewayConnection
     manual: Name — the default is Environment.MachineName, which differs on every machine
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `Name` | `string` | the machine name | Names the connection in broker diagnostics. |
| `AmpqUri` | `AmqpUriSpecification?` | `null` | The AMQP URI the client connects with. |
| `Exchange` | `Exchange?` | `null` | The exchange messages are published to. |
| `DeadLetterExchange` | `Exchange?` | `null` | The exchange dead-lettered messages are routed to. |
| `Heartbeat` | `ushort` | `20` | Seconds between heartbeats. |
| `PersistMessages` | `bool` | `false` | Whether published messages are written to disk. |
| `ContinuationTimeout` | `ushort` | `20` | Seconds a protocol operation waits for its reply. |
| `ClientCertificate` | `X509Certificate2?` | `null` | The client certificate presented for mutual TLS. |
| `ClientCertificatePath` | `string?` | `null` | Path to the client certificate file. |
| `ClientCertificatePassword` | `string?` | `null` | Password for the client certificate file. |
| `TrustServerSelfSignedCertificate` | `bool` | `false` | Whether a self-signed broker certificate is accepted. |
