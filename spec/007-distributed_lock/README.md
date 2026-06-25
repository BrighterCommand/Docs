# Spec 007: Distributed Lock

**Created:** 2026-06-25
**Status:** Writing complete — ready for review/close

## Topic Overview

Brighter provides a distributed lock abstraction that allows components to
coordinate exclusive access to a shared resource across multiple instances of an
application. This documentation will explain what the distributed lock is, why it
matters in messaging and CQRS scenarios (for example, ensuring a single instance
performs work such as outbox sweeping or scheduled tasks), and how to configure
and use the available lock providers.

## Status Checklist

- [x] Requirements gathered
- [x] Requirements reviewed and approved
- [x] Documentation outline created
- [x] Outline reviewed and approved
- [x] Writing tasks identified
- [x] Writing complete
- [x] Documentation reviewed
- [ ] Spec closed

## Completion Summary

All 23 tasks across the 4 phases are complete. QA checklist verified:

- **Code quality** — all C# examples use V10 patterns and were verified against
  Brighter source. Two real defects were caught and fixed during Task 4.2:
  the relational examples used non-existent `*UnitOfWork` types (corrected to
  `PostgreSqlTransactionProvider` / `MsSqlTransactionProvider` / `MySqlTransactionProvider`),
  and the provider-page examples called `AddProducers` directly on `IServiceCollection`
  (corrected to enter through `AddBrighter()`). The archiver generic uses the
  transaction *type* `TransactWriteItemsRequest`, with a note distinguishing it from the
  `DynamoDbUnitOfWork` provider.
- **Content** — explains both what and why; defines **Distributed Lock** and
  **Archiver** in the Glossary; consistent terminology (no "ServiceActivator").
- **Structure** — `SUMMARY.md` updated with the general page and 7 nested provider
  pages; all internal links and anchors added by this spec resolve (Task 4.3). Two
  pre-existing broken links remain out of scope; the trivial `Azure Archive Provider`
  SUMMARY link was fixed (Task 4.4).
- **Accuracy** — provider names, options, lease defaults, and lock resource names
  (`"OutboxSweeper"`, `"Archiver"`) verified; see `verification-notes.md`.

### Deliverables

- New: `DistributedLock.md` + `DynamoDbDistributedLock.md`, `PostgresDistributedLock.md`,
  `MsSqlDistributedLock.md`, `MySqlDistributedLock.md`, `AzureBlobDistributedLock.md`,
  `MongoDbDistributedLock.md`, `FirestoreDistributedLock.md`
- Updated: `BrighterOutboxSupport.md` (Singleton Sweeper note, Archiver subsection,
  out-of-process worker), `Glossary.md`, `SUMMARY.md`, and cross-links in
  `DynamoOutbox.md`, `PostgresOutbox.md`, `MSSQLOutbox.md`, `MySQLOutbox.md`,
  `MongoDBOutbox.md`

## Next Steps

1. Review existing documentation in SUMMARY.md for related content
2. Identify source material (source code, ADRs, release notes, samples)
3. Identify gaps in current documentation
4. Create requirements document
5. Get requirements approved before proceeding

## Notes

- Follow CLAUDE.md guidelines for documentation standards
- Reference source code in ../Brighter and ../Darker as needed
- Ensure SUMMARY.md is updated when new files are created
