# Bugfix: Reject unmapped request types in documentation examples

**Linked Issue**: [BrighterCommand/Brighter#4499](https://github.com/BrighterCommand/Brighter/issues/4499)
**Status**: Verified — documentation-only correction
**Source baseline**: Brighter `7e897fdf2`; Docs `960ce6d`.

## Symptom

The documentation recommends throwing ArgumentException from getRequestType for an unknown message type, but the pumps acknowledge that message instead of rejecting it. The reported Service Bus emulator reproduction sends mapped, unknown, mapped messages; both mapped messages dispatch, while the unknown message disappears without reaching the DLQ. Returning null or throwing InvalidMessageAction rejects the unknown message instead.

## Suspected Location

Paths below are relative to the named repository.

- Brighter: `src/Paramore.Brighter.ServiceActivator/Reactor.cs:552` and `Proactor.cs:559` invoke the type callback before the translation try. Null results become MessageMappingException at lines 554 and 561.
- Brighter: Reactor lines 347–374 and Proactor lines 379–406 distinguish InvalidMessageAction and MessageMappingException (reject and continue) from ordinary exceptions (log/count, then acknowledge).
- Brighter: `docs/adr/0061-reject_mapping_errors.md:32` and `:53` preserve deliberate catch-all acknowledgement.
- Docs: `contents/DynamicMessageDeserialization.md:135` and `:239` recommend ArgumentException; line 260 promises DLQ routing.
- Docs: `contents/RoutingMultipleMessageTypes.md:32`, `:119`, `:123`, `:150`, `:154`, `:204`, `:256`, `:296`, `:322` repeat the same callback guidance.
- Docs: duplicate callbacks in `contents/AgreementDispatcher.md:131`, `contents/CloudEventsSupport.md:201`, `contents/FAQ.md:265`, and `contents/V10MigrationGuide.md:662`.
- Docs: body routing at `contents/RoutingMultipleMessageTypes.md:140–145` can also fail on malformed JSON, a non-object root, or a non-string discriminator before reaching an explicit fallback.

## Root-Cause Hypothesis

**UNVERIFIED — to be proven or refuted in /bugfix:confirm:** examples use the wrong exception to request rejection. ArgumentException from the callback escapes the translation wrapper and reaches the deliberate acknowledge catch-all. InvalidMessageAction already requests rejection as Unacceptable in both pumps.

The issue's code-wrapper suggestion is **UNVERIFIED**: wrapping callback exceptions could change their classification but must preserve deliberate actions and the general exception contract. The maintainer's documentation-only suggestion is **UNVERIFIED**: recommending InvalidMessageAction should express the rejection intent without changing runtime behavior. Correcting explicit throws alone may leave the body-parsing example's malformed-input cases unaddressed.

## Confirmed Root Cause

The documented callback throws an ordinary exception while promising rejection. Both pumps invoke getRequestType outside the translation exception wrapper. ArgumentException reaches the deliberate log/count/acknowledge path. InvalidMessageAction already rejects as Unacceptable and preserves the exception message as the reason. This is a documentation defect; the source trace does not justify changing every callback exception into a mapping failure.

The maintainer explicitly favors this documentation correction: https://github.com/BrighterCommand/Brighter/issues/4499#issuecomment-5957512682

## Evidence

Independent confirmation traced the callback through the catch chain in both pumps at the locations above. Reactor lines 351 and 430–432 preserve the reason and delegate rejection; Proactor line 383 does the equivalent. InMemoryMessageConsumer lines 236–243 use an invalid-message destination, fall back to a dead-letter destination, or discard when neither is configured. AzureServiceBusConsumer line 318 uses native dead-lettering. Therefore, an unconditional DLQ guarantee is incorrect across transports.

Released-package verification completed: all 13 changed examples compiled against the Docs reference set with Brighter 10.7.0; their extracted callbacks passed 162 in-memory pump runs across Reactor and Proactor. Controls covered the prior ArgumentException behavior, null results, supported types, invalid/dead-letter/no destination configurations, and invalid body/header inputs.

Triage found existing tests for mapper-thrown InvalidMessageAction, mapping errors, and deliberate catch-all acknowledgement. Their type callbacks return a valid type; no dedicated dynamic callback rejection test was found. A behavioral validation should compare ArgumentException, InvalidMessageAction, null, and valid resolution through both pumps, with a valid message following the invalid one.

## Scope Notes

Documentation-only changes in six BrighterCommand/Docs pages. Examples import Paramore.Brighter.Actions and request rejection with InvalidMessageAction. Body-based routing classifies malformed JSON and missing/non-string discriminators explicitly. Rejection destination guidance depends on transport configuration. Brighter runtime source remains unchanged.

## Regression Test

Completed against released Brighter 10.7.0, matching the Docs compile-gate package pins and the reported issue:

1. Execute both real pumps with mapped, unknown, mapped messages and an unlimited unacceptable-message threshold. Compare the documented ArgumentException callback with InvalidMessageAction, null-result, and valid-resolution controls. Verify settlement, handler delivery, rejection reason, and continued consumption.
2. For InvalidMessageAction, verify configured invalid-message routing, dead-letter-only routing, and the no-destination case using in-memory transport configuration.
3. Exercise the body-routing example with malformed JSON, non-object root, non-string discriminator, missing discriminator, unknown string, and known string.
4. Compile changed examples against released packages; run the Docs page, link, symbol, and compilation checks applicable to the changes. Preserve existing gate baselines and report any unrelated pre-existing failures.

Existing source tests provide supplementary evidence; new production behavior is not proposed.

## Suggested-Fix Assessment

- Documentation-only correction: CONFIRMED by source trace, maintainer intent, and released-package validation.
- Changing explicit throws alone: PARTIAL; malformed JSON and invalid discriminator shapes can throw before the fallback.
- Wrapping arbitrary callback exceptions in the pumps: would alter runtime policy and is outside the selected documentation scope.

## Fix

Updated 13 examples across the six listed pages to throw InvalidMessageAction for unsupported/missing types. Added the Actions imports, classified malformed body input, and replaced the unconditional DLQ promise with configuration-dependent guidance linked to Handler Failure. Unrelated ArgumentException descriptions remain unchanged.

Compilation also required small repairs within touched examples: terminating statements, completing Kafka argument lists, importing RMQ.Async, and expressing SQS visibility settings through SqsAttributes with the required channel type. These repairs preserve the examples' intended behavior.

## Validation

- Thirteen changed examples compile against the released package reference set.
- Their exact callbacks pass 162 real Reactor/Proactor runs with in-memory transport, including the old ArgumentException control and continued processing of valid messages before/after the invalid message.
- Page lint: zero errors; 525 existing using-directive warnings, down from 537.
- Link check: zero broken links.
- Symbol check: zero findings.
- Repository compilation gate: zero findings, all 295 required blocks preserved, 17 existing skips; no baseline exemptions added.
- `git diff --check`: clean.

The validation harness is local scratch work at `/private/tmp/brighter-4499-verification`, outside this change. Evidence logs are `/tmp/brighter-4499-runtime.log`, `/tmp/brighter-4499-pagelint.log`, `/tmp/brighter-4499-links.log`, `/tmp/brighter-4499-symbols.log`, and `/tmp/brighter-4499-blocks.log`. No broker integration claim is made; the fix changes documentation only.
