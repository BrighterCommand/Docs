# Fixture — a type that cannot be constructed at all

Not a documentation page. `redproof_optioncheck.py` mutates this file; it lives
outside `contents/` so no gate in this repository reads it, and `optioncheck`
sees it only because it takes path arguments.

`HangfireMessageSchedulerFactory` throws `InvalidOperationException: Current
JobStorage instance has not been initialized yet` from Hangfire's own static the
moment anything constructs it, so **every** default on the type is unreadable —
not one member, the whole surface, permanently. Phase 9 declared all three rows
`manual:` and the table still exited 1 on `CANNOT CONSTRUCT`, whose predicate
never consulted the declarations. That left no green path to a correct table:
the only remedy was deleting the marker, which takes the `Option` and `Type`
columns out of scope too.

The predicate now fires unless every unreadable member is declared. **This
fixture is the assertion that it still fires**: delete one `manual:` line and
the finding must come back, naming the row that is no longer declared.

<!-- optioncheck: Paramore.Brighter.MessageScheduler.Hangfire.HangfireMessageSchedulerFactory
     manual: Queue — the type cannot be constructed, so no default on it is readable
     manual: Client — the type cannot be constructed, so no default on it is readable
     manual: TimeProvider — the type cannot be constructed, so no default on it is readable
-->

| Option | Type | Default | Description |
|---|---|---|---|
| `Queue` | `string?` | `null` | The Hangfire queue scheduled jobs are enqueued on. |
| `Client` | `IBackgroundJobClientV2` | a `BackgroundJobClient` | The Hangfire client the scheduler enqueues through. |
| `TimeProvider` | `TimeProvider` | `TimeProvider.System` | The clock delays are measured against. |
