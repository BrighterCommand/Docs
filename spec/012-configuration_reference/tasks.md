# Spec 012: Configuration Reference Tables — Tasks

**Created:** 2026-08-27 · **Status:** **REVIEWED AND APPROVED 2026-08-27** — four findings
applied in place; the total moved 56 → 57. No phase, gate, priority or sequencing decision
moved.
**Works from:** `design.md` (approved 2026-08-27, `.design-approved`; four review questions
answered the same day) and `requirements.md` (approved 2026-08-27, `.requirements-approved`;
amended twice by design review — AC4's schedule clause struck, D15 and D16 added)

**Total tasks: 57, across 11 phases.** Re-derived, not counted by hand:
`grep -c '^- \[.\] \*\*Task' tasks.md` says 57, and the phase table's Tasks column sums to 57
independently. **Keep both**, and re-derive both after any edit — 009's D-table spent three
sessions wrong because a count was edited beside the row it counted, and 010 carried two
counting methods that disagreed by four for a fortnight while both printed a plausible number.

**It was 56 at draft, and the review moved it — the two counts then disagreed by one until the
phase table was corrected too**, which is exactly the failure the paragraph above describes,
arriving in the same edit that added the task warning about it. Task 11.3 was added: nothing
in the list recorded what the checker caught, so the spec could have finished able to prove
the corpus is correct *now* and unable to say it was ever wrong.

> ### What the review changed — four findings, 2026-08-27
>
> 1. **§2.9 added.** Design §7 marks four of the eight primary-constructor surfaces a 012 table
>    will meet; the other four carry no mark, three of them on one task. **A mark present on
>    four of eight cases teaches a reader to trust the absence of a mark**, so all eight are now
>    listed against their owning task, and standing obligation 3 points at the list.
> 2. **Task 2.6 no longer asserts its own result.** It expected exit 0 from the seed marker,
>    which nobody has checked. **Exit 1 there is the checker catching real drift on day two**,
>    not a defect in the tool.
> 3. **Standing obligation 10 and task 11.3 added** — the drift ledger. AC2 is an exit code and
>    an exit code has no memory.
> 4. **Task 2.1 carries probe 1.3's package list forward** rather than re-deriving it, so the
>    repository holds one proven list instead of two that can differ silently.

**The phases are design §14's eleven, unchanged.** This document cuts them into tasks; it does
not re-sequence them, and it re-opens nothing. Where it records something design does not say,
it is in §2 and it is a fact about the corpus or about this repository — never a verdict.

---

## 1. How this list is organised

**One phase is one pull request** — a coherent unit, merged before the next branch starts, the
contract 009 and 010 both ran under. Every phase lands in **this** repository: 012 has no
Brighter-side deliverable, and requirements §11 is explicit that its only source need is *read*
access at a tag. **Nothing in this spec licenses a write to `../Brighter`**, so the per-PR
authorisation 009 needed four times is not needed once here.

| Phase / PR | Goal | Tasks | Deliverables |
|---:|---|---:|---|
| **1** | **Three probes and the survey fix.** Nothing is written from a floor | 5 | D11 |
| **2** | **The checker, its CI job, its pin, and three red-proofs** | 11 | D1, D2, D3 |
| **3** | The two core Reference pages — 47 + 26 options, 8 tables | 4 | D4, D5 |
| **4** | The relational reference page — one table, thirteen pages link it | 2 | D15 |
| **5** | The five documented transports — 186 options, plus InMemory | 7 | D6 |
| **6** | **The five new transport pages** — 151 options, five `SUMMARY.md` entries | 8 | D12, part of D14 |
| **7** | The four Firestore/Spanner store pages — **zero options** | 4 | D13, D16, part of D14 |
| **8** | Outbox, inbox and lock pages, and the last of §2.7's twelve tables | 5 | D7, D8 |
| **9** | Schedulers — 25 options the survey never counted | 3 | D9 |
| **10** | The two stale cross-cutting tables | 3 | D10 |
| **11** | Acceptance — AC1–AC9 walked with evidence, the drift ledger, and close | 5 | — |

**Phases 3 through 10 are individually shippable and individually abandonable.** Each adds
tables to pages that already exist or adds pages the tree does not yet reference; a spec that
stopped after phase 6 would leave a corpus that is smaller than intended and correct
throughout. **Phases 1 and 2 are not** — they are the instrument, and every table after them
is written under it.

### Dependencies

Stated as gates rather than drawn. This is a DAG and an ASCII spine would omit the
cross-links.

- **1 gates everything.** The `Default` column of every table in the spec rests on probe 1.2;
  the `optioncheck.csproj` package list rests on probe 1.3; and **no table may be written from
  `survey.py`'s output until task 1.5 lands**, because a figure taken from the unfixed survey
  is wrong rather than absent (design §12.5).
- **2 gates 3–10.** Requirements §14's one ordering obligation is that the tables are written
  under the gate rather than brought under one afterwards.
- **4 gates 5, 6, 7 and 8.** D15 is the page thirteen others link. `PostgreSQLMessageBroker.md`
  (phase 5) and `MSSQLMessageBroker.md` (phase 6) link it for their connection surface;
  `SpannerOutbox.md` and `SpannerInbox.md` (phase 7) link it *instead of* a table; the five
  relational outbox and inbox pages (phase 8) link it too. **A link to a page that does not
  exist is `linkcheck.py` MISSING FILE**, so this is mechanical, not stylistic.
- **6 gates 10.** D10's corrected tables name GCP Pub/Sub, RocketMQ, MSSQL and PostgreSQL, and
  a comparison-table row naming a transport with no page is the dead end §3.3 of the
  requirements describes.
- **2 reaches into phase 8 in one specific way.** Task 2.6 marks `SweeperCircuitBreaking.md`'s
  existing table so phase 2's first CI run is not vacuous. That is one of §2.7's twelve
  tables, done early and on purpose, and §2.7 records who owns each of the other eleven so
  that none is done twice.
- **3, 5 and 9 are independent of each other** and of 7. If a phase stalls, take the next one.

### The standing obligations — every table-writing task owes all ten

Do not restate these in each task; they are assumed by all of them.

1. **Four columns, fixed: `Option | Type | Default | Description`.** Design §4. `Option` is the
   spelling the reader **types** — the constructor parameter, `bufferSize` not `BufferSize`,
   and where the two differ by more than case the description says so. `Type` carries its
   nullable annotation: `TimeSpan?`, never `TimeSpan`. **A `Default` cell is never blank** —
   `none` where there genuinely is no default, because a blank cell is indistinguishable from
   an unfinished row.
2. **Every table carries its `<!-- optioncheck: <fully.qualified.Type> -->` marker**, on the
   line above it. `omit:` and `manual:` **declare and count**; they never silence. A table
   with no marker is a table nobody checks, reported as a table that passed.
3. **Write the table from the type, never from the survey and never from our own prose.**
   Requirements §11: transcribing existing documentation propagates whatever drift is already
   there, and §3.4 establishes that there is some. Design §7's counts are **floors** wherever
   §12.1 or §12.5 applies — **and only four of the eight affected surfaces carry a `†` there.
   §2.9 lists all eight against the task that meets them.** Never infer from an unmarked row
   that its figure is a total.
4. **Do not write a page a table it does not have.** Seven instances across four families
   (design §10): Spanner (no configuration type), MSSQL and PostgreSQL as transports (the
   shared relational configuration), the MSSQL and MySQL locks (a connection provider),
   TickerQ (no factory properties), Redis and MQTT (no publication type). *The absence of a
   configuration type is a fact about the product; the page reports it and links what is
   really there.*
5. **One sentence per description, present tense, no rationale.** That is AC8, **it has no tool
   behind it, and it is walked in the phase that writes the table** — not at phase 11. 009's
   AC7 was the criterion with no tool and it was the one found unmet at the close, on pages
   green under all six gates.
6. **Every edited page keeps its banner, its qualified `##` headings and its opening
   sentence**; every new page gains all three, plus `description:` front matter equal to that
   sentence with the markdown stripped, quoted, with `layout.description.visible: false`.
7. **A new page gets its `SUMMARY.md` entry in the same commit that creates it** — never
   before, because a line pointing at a file that does not exist fails `linkcheck.py` with
   MISSING FILE, and never after, because the orphan check has no exemptions. **Append a row
   to `spec/011-authoring_conventions/pagetypes.tsv` for every page created** — `verdict` =
   `Reference`, `applies` = `Brighter V10`. **Append; never re-sort.** No tool reads that file
   except `apply_banners.py`, so a missing row is invisible to every green build and silently
   skips the page at the next version bump.
8. **Rule 6 turns strict on any code block the diff touches.** A table inserted beside an
   existing C# block pulls that block into `--changed` scope. **The cheapest defence is
   placement** — put the table *before* the section's code block where the prose allows, so
   the diff never reaches it. Where it must reach it, the remedy is real `using` directives or
   a declared `// ...`, which downgrades and is still counted.
9. **Run the gates after every page, and read the whole output.** The six in §2.8 plus
   `optioncheck`. **`git add` first** — `git diff` cannot see untracked files, so a brand-new
   page contributes no strict ranges until it is staged, and a vacuous pass is
   indistinguishable from a real one. Read the scope line of both `pagelint --changed` and
   `optioncheck`: they print what the run reached, and that figure is what says whether it
   meant anything.
10. **Record every mismatch `optioncheck` reports, before you fix it** — page, option, what the
    corpus said, what the assembly says. **Task 11.3 aggregates them, and that ledger is the
    only evidence 012 produces that the drift was real.** Without it the spec finishes able to
    prove that the corpus is correct *now* and unable to say it was ever wrong — which is the
    claim #67 was opened about and the one requirements §14 calls the point of the whole
    exercise: *a wrong default is worse than an absent one, because the reader has no reason
    to doubt it.* A mismatch fixed silently is a defect that never existed.

### Two conventions this document holds itself to

- **Every count in a task body is a claim about the corpus**, and is re-derived when the task
  runs rather than inherited from here. 009 learned this from *"the two places"* in a task
  body that turned out to be three.
- **No task quotes a total.** 619 and 67 are floors (design §12.1, §12.5); per-page figures
  are re-derived with the type when the table is written. `619` appears nowhere below as a
  target.

---

## 2. What this list settles — checked while writing it, 2026-08-27

Design and requirements are approved and this section corrects nothing in either. It records
what was verified in **this** repository before the phases below were cut, plus two places
where design's own summary lines and its measured tables count differently.

### 2.1 All 37 pages the mapping edits exist, and none of the ten new filenames collides

Checked by existence, not by memory: the seven §7.1 pages, the six §7.2 transport pages, the
eight §7.3 outbox/inbox pages, the seven §7.4 lock pages, the six §7.5 scheduler pages and the
three §8/§9 link targets are all present under `contents/`. **None of `RelationalDatabase
ConfigurationReference.md`, `GcpPubSubConfiguration.md`, `RedisConfiguration.md`,
`RocketMQConfiguration.md`, `MQTTConfiguration.md`, `MSSQLMessageBroker.md`,
`FirestoreOutbox.md`, `SpannerOutbox.md`, `FirestoreInbox.md` or `SpannerInbox.md` exists
today**, so all ten are creations and none is an accidental overwrite.

### 2.2 *Transports* is at **7** top-level entries, which is what design §9.2 measured

So D12 takes it to 12, and S2's ceiling is now **20** (`tools/urlmap.py:58`, raised in #125,
`4ef9633`). **Eight of headroom, not zero** — design §9.2 was written before the raise and
records the zero-headroom state as history. Do not re-derive an alarm from that section.

### 2.3 Design §3.1's D8 summary and §7.4's table count links differently

§3.1 says *"7 pages, 8 own options + 3 links"*. §7.4's table — the measured one, taken
constructor by constructor — shows **three pages with tables** (Dynamo 4, Azure Blob 3,
Postgres 1 = the 8 own options), **two that link §7.3's tables** (Mongo, Firestore) and **two
with no options type at all** (MSSQL, MySQL — a connection provider). **Write phase 8 from
§7.4.** The summary line is a summary; the table is the measurement.

### 2.4 Design §11's *"the eight new pages"* predates D16

§11's compile obligation was written before §13.4 commissioned `FirestoreInbox.md` and
`SpannerInbox.md`. **It applies to all ten new pages** — the obligation is *an example nobody
has run should at least be an example that compiles*, and nothing about it distinguishes the
two pages that arrived last. Tasks 6.7 and 7.4 carry it.

### 2.5 `optioncheck` takes optional path arguments, like the two tools beside it

`linkcheck.py` and `pagelint.py` both check the whole corpus with no arguments and just the
named files with them. `optioncheck` does the same — which is what lets phase 2's red-proofs
run against fixtures outside `contents/` rather than depending on a page no phase has written
yet. Recorded here because it is a decision this list makes, small and consequential: without
it, AC3b's red-proof cannot run until phase 3.

### 2.6 The pin is `10.7.0`, and it is the same version every figure in the spec is stamped at

`versioncheck.py` resolves `10.7.0` from NuGet today for all three tutorial packages, so the
pin `optioncheck.csproj` takes is also the current release. **That will stop being true**, and
when it does the two tools go different colours for opposite reasons — `versioncheck.py` red
on its own (it resolves *latest*), `optioncheck` green until a human bumps the pin (it
resolves *pinned*). Design §13.2. **Do not reason about the two together.**

### 2.7 The twelve tables to normalise are **distributed**, and only one of them is task 8.4's

Design §12.2 counts twelve option-shaped tables across nine pages and says *"all twelve tables
are touched"*. Located here, by header row, 2026-08-27 — and the list reproduces requirements
§3.1's 12 / 9 exactly:

| Page | Tables | Normalised by |
|---|---:|---|
| `PostgreSQLMessageBroker.md` | 3 | **task 5.5**, which replaces all three |
| `AzureBlobDistributedLock.md`, `DynamoDbDistributedLock.md`, `FirestoreDistributedLock.md`, `MongoDbDistributedLock.md`, `PostgresDistributedLock.md` | 5 | **task 8.3**, which owns those pages |
| `SweeperCircuitBreaking.md` | 1 | **task 2.6**, the seed marker |
| `CausationTrackingStores.md` | 2 | **nobody** — not option tables, no marker (§12.2) |
| `AsyncAPISupport.md` | 1 | **task 8.4, and it is the only one left** |

**So the normalisation is not a body of work sitting beside the phases; it is a property of
them.** Eleven of the twelve are normalised by whoever owns the page anyway, which is what
makes standing obligation 1 sufficient. The residue is one table — and it is on a **P2** page
(design §7.6 files `AsyncApiOptions`, 6 options, under P2), which is why task 8.4 exists at
all rather than dissolving entirely.

**Five of the twelve are lock pages**, which independently confirms design §7.4's *"five of the
seven lock pages already have tables and not one is in the §7.1 shape"*.

### 2.8 The six gates, at `4ef9633`, before anything below runs

```bash
python3 tools/linkcheck.py                 # No broken internal links (150 files checked).
python3 tools/pagelint.py                  # 0 errors, 790 warnings, 148 pages
python3 tools/urlmap.py --check-shape      # 0 — 147 pages, 12 sections, deepest 4 of 4, widest 10 of 20
python3 tools/urlmap.py --check-redirects  # 0 — 77 entries, 7858 bytes, printable ASCII
python3 tools/versioncheck.py              # 0 stale pins of 18 examined across 5 page(s).
python3 tools/urlmap.py --verify           # predicted 147, published 147, 147 agree  (not in CI)
```

All six re-run and matched to the digit on 2026-08-27 before this list was written. **Ten new
pages will move four of them** — link, pagelint, shape and `--verify` — and `--check-redirects`
should not move at all, because slugs are filename-derived and adding to a section moves no
URL. That has held five times; **assert it rather than assuming it** each phase.

### 2.9 Eight of design §12.5's thirteen primary-constructor surfaces land on a 012 table

The floors are not spread evenly and they are not all marked. Design §7 carries a `†` on four
rows; §12.5's list is longer, and cross-referencing the two against §7's mapping gives the
eight that a writer in this spec will actually meet:

| Surface | §7 says | Owning task | Marked `†` in design §7? |
|---|---|---|---|
| `InMemorySubscription` | 2 | **5.6** | yes — **17 by the type** |
| `PostgresMessagingGatewayConnection` | 1 | **5.5** | yes — **absent from the 67 entirely** |
| `RocketMessagingGatewayConnection` | 4 | **6.3** | yes — under-counted by 1 |
| `PostgresLockingProviderOptions` | 1 | **8.3** | yes — **absent from the 67 entirely** |
| `FirestoreConfiguration` | 7 | **7.1** | **no** — under-counted by 2 |
| `AzureBlobLockingProviderOptions` | 3 | **8.3** | **no** — under-counted by 2 |
| `DynamoDbLockingProviderOptions` | 4 | **8.3** | **no** — under-counted by 2 |
| `DynamoDbInboxConfiguration` | 1 | **8.2** | **no** — under-counted by 1 |

**Four of the eight carry no mark in design §7**, and three of those four are on the lock
pages, which is one task. The remaining five of the thirteen are the luggage and archive
types — `AzureBlobArchiveProviderOptions`, `MongoDbLuggageStoreOptions`, `FileSystemOptions`
and their neighbours — which design §7.6 files under **P2** and 012 does not schedule.

**This table is the reason standing obligation 3 is worded as it is.** A writer who trusts
§7's column writes a two-row table for `InMemorySubscription` and a three-row one for the
Azure Blob lock, and **every gate in this repository is green on both** — `optioncheck`
included, until it enumerates the type and finds rows the page never had. *A mark that is
present on four of eight cases teaches a reader to trust the absence of a mark.*

---

## Phase 1 — Three probes and the survey fix (Docs PR)

**Goal:** the instrument, before anything is measured with it. Requirements §5.2 names the
first probe as the implementation phase's first task, and the whole `Default` column rests on
it; design §6.4's package-conflict risk is a `csproj` edit on day two and a redesign after
twelve tables.

**This phase must not:** write a table, create a page, or touch `SUMMARY.md`. It also must not
*correct* design §7's counts in place — the floors are marked `†` there and §12.5 explains
them; what this phase produces is a fixed tool and a fresh run, recorded here.

- [x] **Task 1.1:** Write this task list and take it through review
  - Input: `design.md` (§7 the mapping, §14 the phases, §12 the errata), `requirements.md`
    (§9 deliverables, §12 acceptance criteria), `spec/009-getting_started_tutorials/tasks.md`
    §1 as the structural model
  - Output: `spec/012-configuration_reference/tasks.md`; `.tasks-approved` on approval
  - Notes: `spec/.current-spec` already reads `012-configuration_reference` and both approval
    markers exist, so no repointing is needed — unlike 009, where it was part of Task 1.1

- [x] **Task 1.2:** Probe the body-coalesced default — instantiate `Subscription`, read
      `EmptyChannelDelay` back, assert 500 ms
  - Input: requirements §5.1 (`Subscription.cs:208`, `:236` at `10.7.0`); design §6.2
  - Output: `spec/012-configuration_reference/probes/` — a small C# project, kept and
    re-runnable, that constructs the type and prints the parameter default beside the
    instance value
  - Notes: **This is the premise of AC3b and of the entire `Default` column.** Requirements
    §5.1 infers it from source and says so in its own sentence — *reading the authority is not
    measuring it*. The probe must print **both** routes, because the finding is the
    *difference*: `ParameterInfo` says `null`, the instance says 500 ms. If they agree, the
    spec's central premise is wrong and phase 2 changes shape — say so loudly rather than
    proceeding.

- [x] **Task 1.3:** Probe the package load — reference every surface package at `10.7.0` in one
      project and instantiate one type from each
  - Input: design §6.4; the package list implied by §7.2, §7.3, §7.4 and §7.5
  - Output: the same probes project, extended; a recorded verdict — clean, or the conflicting
    pair named
  - Notes: **Metadata reflection never runs a static constructor; instantiation resolves
    dependencies for real**, so the risk lands entirely on the route §6.2 requires. Do **not**
    reference the `.V4` packages — requirements §8 puts them out of scope and they are exactly
    the pairs that would put two AWS SDK majors in one process. If a conflict appears anyway,
    the fallback is **one process per package family**, not `AssemblyLoadContext`, which does
    not isolate native dependencies; and **not `MetadataLoadContext`**, which cannot
    instantiate.

- [x] **Task 1.4:** Re-derive design §6.3's synthesis table **by construction**
  - Input: design §6.3's table — 24 types, 48 required parameters, 20 of the 24 needing only
    strings, enums and the three subscription arguments
  - Output: the recorded result of actually constructing each of the 24; the four that need a
    hand-written factory confirmed as four, or corrected
  - Notes: §6.3 is parsed from source with `survey.py`'s parser, which is the same instrument
    the 619 came from — and design says in the section itself that this **is not a measurement
    of the running tool**. `HandlerConfiguration` is the one of the four that is P0 and on D4,
    so its factory is phase 2's problem and not P2's.

- [x] **Task 1.5:** Teach `survey.py` to read C# primary constructors, and re-run it at
      `--ref 10.7.0`
  - Input: design §12.5 — `widest_ctor` matches `public TypeName(` *inside* a class body; 13
    surface types, 40 parameters; `PostgresLockingProviderOptions` and
    `PostgresMessagingGatewayConnection` absent from the 67 entirely; `InMemorySubscription`
    **2 by the survey, 17 by the type**
  - Output: `spec/012-configuration_reference/survey.py` (D11) reading both constructor forms;
    the new totals recorded **in this file with the ref beside them**
  - Notes: **The three `†` rows in design §7.2 and the one in §7.4 are the ones that move.**
    `optioncheck` is unaffected — a primary constructor is just a constructor to reflection —
    so this task changes no design decision; it changes what a writer would copy. **Quote the
    new figures with the ref or not at all.**

### Phase 1 as executed — 2026-08-28, 5/5

**The full record is `probes/README.md`**, which carries the tables, the
reconciliations and the method. This is the summary and the four numbers a later phase
needs. Everything is stamped at Brighter **`10.7.0`** and re-runnable:
`dotnet run --project spec/012-configuration_reference/probes`.

**1. The premise holds.** `emptyChannelDelay` is `null` by `ParameterInfo` and **500 ms**
on the instance. Phase 2 does not change shape. And the shape is not uniform, which is the
part that could not be read off the source: of `Subscription`'s six `null` signature
defaults, **four come back as a value and two really are null** — so `null` in a signature
means *both* "no default" and "the default is assigned below", and nothing distinguishes
them until the object exists.

**2. The packages do not fight, and `optioncheck` can be one project.** 64 Brighter
assemblies and 65 third-party assemblies in one process; one type instantiated from each
package for real. **Design §6.4's fallback is not needed.**

> **The conflict §6.4 predicted is real and it is a different pair.** Not AWS —
> **`RabbitMQ.Client`, loaded at 7.0.0.0 while `RMQ.Sync` was built against 6.0.0.0.**
> Exactly **one of `RMQ.Sync`'s 57 types** fails to load, and it is not one 012
> documents: both `RmqSubscription` types construct, and the probe re-derives design
> §7.2's ruling from the assemblies — **Async 24 params with `queueType`, Sync 23
> without**. Left alone deliberately. If it ever bites, drop `RMQ.Sync` from the
> checker's `csproj`; 012 binds no type in it.

**3. The synthesis burden is bigger in one way and smaller in another, and design §6.3 is
superseded on both.** Measured **34 types and 70 required parameters**, against §6.3's 24
and 48 — §6.3 subtracted **four** `.V4` duplicates where there are **six**, and it was
parsed by a `survey.py` that task 1.5 then found five defects in. **The rebuilt parser and
the running probe now agree exactly at 34 / 70.**

- **Thirteen constructors reject their own defaults** — every subscription type in the
  product. **All thirteen require a request type** (`requestType`, or `dataType` on
  `PostgresSubscription`) and **eleven also require `messagePumpType`**, whose declared
  default of `Unknown` the body refuses; the two exceptions default it to a usable value
  themselves. A synthesiser using only the parameters that carry no default builds **19 of
  34**; adding defaulted `enum` and `Type` parameters takes it to **32**. **Phase 2 must
  budget for this and design does not name it.**
  *(Necessity is measured — each candidate is put back to its own default one at a time.
  A first draft read what pass 2 **supplied** as what the constructors **require** and
  reported that all thirteen need `makeChannels`; **not one of them does.**)*
- **Two types need a hand-written factory, not four** — `AzureBlobArchiveProviderOptions`
  and `S3LuggageOptions`. The other three build because their unbuildable parameters are
  *reference* types and `null` is accepted. **That is a new obligation, not a saving:** a
  `null`-built instance reads defaults correctly *unless a constructor body derives one
  from the missing object*, so phase 2 decides that per type, and where it cannot the
  answer is `manual:` — which declares and counts — never a silent pass.

**4. `survey.py` is rebuilt, and at `10.7.0` the corpus is 72 configuration types and 628
reader-facing options** — against the **67 and 619** every document in this spec quotes.
Both figures were floors, as design §12.1 and §12.5 said, and both were also *wrong* in
places rather than merely low. **Quote them with the ref or not at all.**

The task asked for one fix. Probe 1.4 found two more of the same kind and a new
reflection oracle found two beyond that — five in all, every one of them a number rather
than an error:

| Defect | What it cost |
|---|---|
| Primary constructors invisible (design §12.5) | `InMemorySubscription` **2 → 17**; two types absent from the population entirely |
| The class was assumed to be the file | `RocketMqSubscription.cs` declares **`RocketSubscription`**; `AWSMessagingGatewayConfiguration.cs` declares **`AWSMessagingGatewayConnection`**; the MQTT file declares three types |
| Properties counted per file, not per class | `RmqMessagingGatewayConnection` **19 → 11** — the other 8 belong to `AmqpUriSpecification` and `Exchange`, which share its file |
| A generic argument contains a space — `Dictionary<string, object>?` never matched | `Publication` **8 → 10**, and three more |
| `private set`, `internal set` and `static` counted as reader-facing | `ProducersConfiguration` **26 → 22**, and two more |

> **The oracle is the method worth keeping.** `survey.py --tsv` and
> `probes -- counts` print the same quantity, one by parsing source and one by
> reflecting over the assemblies. The first diff had **nine** disagreements, every one a
> parser defect and none visible any other way. It now reads **`survey 72, reflection 68,
> both 63, disagree 0`** — the nine the survey sees alone are `.V4`, the five reflection
> sees alone carry zero options. **Run that diff after any change to `survey.py`.**

**Two type NAMES in design §7 do not exist, and that matters more than any count.** A
marker binds a fully-qualified type, so `RocketMqSubscription` (really
**`RocketSubscription`**, 23 not 22) and `MQTTMessagingGatewayConfiguration` (really
**`MqttMessagingGatewayConfiguration`**, 7 not 8) would each have failed in phase 6 with
*the type is gone*. Loud, but late, and free to know now. **`D4 is 47 options over 5
tables` becomes 45.**

> **§2.9 of this document inherits an overstatement from design §12.5, and its verdict is
> untouched.** §12.5's *"13 surfaces, 40 parameters"* is additive, and the convention is
> **`max(props, ctor)`**. Measured with the primary constructor readable, **three of the
> thirteen move** — `InMemorySubscription` and the two that were absent. The rest do not,
> because **a C# primary constructor on a class assigns to properties rather than
> creating them**: `AzureBlobLockingProviderOptions`'s two parameters are two of its three
> properties. §2.9's instruction — *never infer from an unmarked row that its figure is a
> total* — is right and is what caught this, applied to §2.9 itself.

**Two things neither instrument counts**, both for the phase that meets them: **public
fields** (`AzureBlobLockingProviderOptions.StorageLocationFunc` has a default and is
invisible to both), and **surface types whose name says otherwise** — `AmqpUriSpecification`
and `Exchange` are RabbitMQ configuration a reader writes by hand, and **phase 5 owns**
whether `RabbitMQConfiguration.md` gains a table for them.

**Phase 1 wrote no table, created no page and did not touch `SUMMARY.md`.** The six gates
in §2.8 are unmoved, as they must be.

---

## Phase 2 — `optioncheck`, its CI job, its pin, and three red-proofs (Docs PR)

**Goal:** D1, D2, D3. A gate that is proven to fail before it is trusted to pass.

**This phase must not:** ship with a guard that keeps the build green while the tool is absent.
009's D9 earned that rule — *a guard that outlives its tool silently un-gates the check*. Nor
may it add a `schedule:` trigger: AC4's schedule clause was struck at design review (§13.2),
and a pinned checker on a schedule can only repeat yesterday's verdict or exit 2 on a NuGet
outage.

- [x] **Task 2.1:** Create `tools/optioncheck` — project, pinned package references, and the
      run contract
  - Input: design §3.1's file list, §6.4's pinning rule, and **task 1.3's package list carried
    forward verbatim, not re-derived** — probe 1.3 assembles exactly this set and proves it
    loads in one process, so re-assembling it here would put a second, unproven list in the
    repository and the difference between them would be silent
  - Output: `tools/optioncheck/optioncheck.csproj`, one `PackageReference` per surface package,
    every one pinned to `10.7.0`; `dotnet run --project tools/optioncheck` runs the whole
    corpus and `-- <paths>` runs just those files (§2.5)
  - Notes: **No `.V4` packages.** The pin lives here and nowhere else — design §5.2: a version
    on every marker would be a hundred pins to bump instead of one.

- [x] **Task 2.2:** `Binding.cs` — parse the marker and its two keys
  - Input: design §5 and §5.1
  - Output: a parser for `<!-- optioncheck: <type> -->`, with `omit:` and `manual:` lines, each
    carrying a reason; the table rows beneath it
  - Notes: **Both escapes declare rather than silence, and both are counted.** That is
    `pagelint.py` rule 6's `// ...` applied to a second tool. A parser that accepts `omit:`
    with no reason is a parser that lets 012 reach a green build by writing `omit:` over the
    hard half of every table.

- [x] **Task 2.3:** `Reflect.cs` — the two routes, one per column
  - Input: design §6.2's table
  - Output: name and type from `ParameterInfo` / `PropertyInfo`; **`Default` from an
    instantiated object, read back, always**
  - Notes: **Always, including where the parameter default would have been right.** Choosing
    per parameter would make the tool's correctness depend on a judgement about which of the
    three shapes a parameter is in — and shape three is precisely the one that *looks like*
    shape two. Reader-facing members are settable properties and the **widest** constructor's
    parameters, which is `survey.py:143`'s `max(props, ctor)` convention expressed as code.

- [x] **Task 2.4:** `Synthesise.cs` — constructor arguments for the 24 types that need them
  - Input: task 1.4's re-derived table; design §6.3
  - Output: strings, enums and the three subscription arguments handled generically; a
    hand-written factory for `HandlerConfiguration`; the other three declared `manual:` where
    they are P2
  - Notes: 20 of the 24 need only the generic path. **`HandlerConfiguration` is P0** — it is on
    D4, phase 3 — so it gets the factory rather than the declaration.

- [x] **Task 2.5:** `Program.cs` — scope line, verdict, exit 0/1/2
  - Input: design §6.1's seven steps and §6.5's exit codes
  - Output: the scope printed **before** the verdict — tables, rows, types, and the `omit:` and
    `manual:` counts, each named; then the diff; then exit 0, 1 or 2
  - Notes: **`0 mismatches` across 0 tables and across 619 must not print the same line** —
    that is the family contract `versioncheck.py` set, and it is the whole defence against a
    vacuous green. Exit **2** is *authority unreachable*, and it is not a pass.

- [x] **Task 2.6:** Mark `SweeperCircuitBreaking.md`'s existing table, so the first CI run is
      not vacuous
  - Input: design §12.2 — it is the **one** table in the corpus already in the §7.1 shape, and
    it has a single row; `OutboxCircuitBreakerOptions`, 1 option (§7.3)
  - Output: a marker above that table; `optioncheck` reporting a scope of 1 table, 1 row, 1
    type — **and either exit code recorded as a result**
  - Notes: **Do not write "exit 0" into this task's expected output.** Nobody has checked that
    row against `OutboxCircuitBreakerOptions`. Exit 0 says the seed works; **exit 1 says the
    checker has caught real drift on the corpus's one already-well-shaped table**, which is
    012's premise arriving on day two and belongs in task 11.3's ledger, not in a bug report
    against the tool. Fix the row, keep the finding. **This is one of §2.7's twelve tables,
    done early and on purpose** — 009 moved D9 into a later phase for exactly this reason, a
    gate whose first CI run is vacuous being a gate nobody has tested — and **§2.7 records
    that this one is spent**, so no phase-8 task repeats it.

- [x] **Task 2.7:** AC3's red-proof — a changed constructor default is caught
  - Input: requirements §12 AC3; `spec/009-getting_started_tutorials/redproof_versioncheck.py`
    as the shape to copy
  - Output: `spec/012-configuration_reference/redproof/` — a fixture `.md` with a marker and a
    deliberately wrong constructor-parameter row; a recorded exit **1** naming the row
  - Notes: **Not merely that the tool runs.** AC3 is the criterion §2 of the requirements
    exists to create: a checker that passes while blind to 43% of the surface is the vacuous
    green this programme has met repeatedly. Assert the fixture is in scope **before** reading
    the verdict — 010's S2 red-proof was vacuous twice over for skipping that step.

- [x] **Task 2.8:** AC3b's red-proof — a body-coalesced default is reported as its real value,
      never as `null`
  - Input: task 1.2's measurement; requirements §12 AC3b
  - Output: a fixture whose `EmptyChannelDelay` row says `null`; a recorded exit **1** saying
    the value is 500 ms
  - Notes: **This is the red-proof that matters.** A tool that reads the signature documents
    this option *wrong, not missing*, and a reader has no reason to doubt a wrong default.
    Prove it fires before trusting the 619 rows that follow.

- [x] **Task 2.9:** AC5's red-proof — exit 2 is distinguishable from exit 0
  - Input: requirements §12 AC5; design §6.5
  - Output: a recorded run with the package source removed, exiting **2**, with a message that
    names the authority rather than the symptom
  - Notes: 009's `versioncheck.py` has the same contract and PROMPT records why it matters:
    *exit 2 is authority unreachable, which is not a pass.*

- [x] **Task 2.10:** D2 — add the job to `.github/workflows/docs.yml`, unguarded
  - Input: design §6.5's YAML
  - Output: a third job running `dotnet run --project tools/optioncheck` on push and pull
    request; a green CI run **naming the job** in the evidence
  - Notes: **No `schedule:` trigger** (AC4 as amended). **No guard.** Read the whole of
    `gh pr checks` — it lists a `push` row and a `pull_request` row per commit and they
    legitimately disagree; a `tail` of that output merged a red build in session 23.

- [x] **Task 2.11:** D3 — the pin joins `versioncheck.py`'s in `RELEASE_CHECKLIST.md`
  - Input: `RELEASE_CHECKLIST.md`; requirements §9 D3; design §13.2
  - Output: a step bumping `optioncheck.csproj`'s pin, beside the existing tutorial-pin step
  - Notes: **State the trigger, because the two tools have opposite ones.**
    `versioncheck.py` goes red *by itself* when Brighter ships; `optioncheck` goes red *at this
    step*, performed by someone who is at that moment looking for exactly this information.

### Phase 2 as executed — 2026-08-28, 11/11

**The gate exists, it is proved to fail before it is trusted to pass, and it is unguarded in
CI.** Everything below is at Brighter **`10.7.0`**, the pin in
`tools/optioncheck/optioncheck.csproj` and nowhere else:

```bash
dotnet run --project tools/optioncheck                    # every page under contents/
dotnet run --project tools/optioncheck -- <paths>         # just these files (§2.5)
python3 spec/012-configuration_reference/redproof/redproof_optioncheck.py
```

**Six files, where design §3.1 lists five.** The sixth is `Authority.cs`, and it exists
because **exit 2 has to be reachable as a verdict rather than as a restore failure**: a failed
`dotnet restore` never runs the program at all, so a run whose packages were missing could
never have said so. The `csproj` now writes its own `PackageReference` list into the assembly
at build time, and the checker holds what loaded against what was pinned. That is what task
2.9's red-proof measures, and it is not a change to any design decision — §6.5's exit codes
are unchanged.

**Task 2.6's seed marker exits 0, and the task was right to refuse to predict it.**
`SweeperCircuitBreaking.md`'s single row says `CooldownCount`, `int`, `10`, and
`OutboxCircuitBreakerOptions` at `10.7.0` says `CooldownCount`, `int`, `10`. **The corpus's
one already-well-shaped table is correct**, so day two produced no ledger entry — which is a
result, not an absence, and it is the first row of task 11.3's ledger either way: *checked,
and it was right.* What the task did change is presentation — a bold `**CooldownCount**`
became the spelling a reader types in backticks, the default gained its backticks, and the
description gained a full stop.

**The red-proofs: eight branches, all eight fired.** The two that matter are AC3b and AC5:

| Branch | Expected | Got |
|---|---:|---:|
| 0. Baseline — the fixture must be green, and its scope asserted first | 0 | 0 |
| 1. **AC3** — `bufferSize`'s *constructor* default changed 1 → 10 | 1 | 1 |
| 2. **AC3b** — `emptyChannelDelay` written as `null`, which is what the *signature* says | 1 | 1 |
| 3. `BufferSize` for `bufferSize` — the property spelling, not the parameter's | 1 | 1 |
| 4. `omit:` with no reason | 1 | 1 |
| 5. The type is gone — the marker renamed | 1 | 1 |
| 6a. **AC5 control** — the built output copied elsewhere, nothing removed | 0 | 0 |
| 6b. **AC5** — one pinned assembly deleted from that copy | 2 | 2 |

**6a is not padding.** Copying the output somewhere else is itself a change, and without the
control an exit 2 from the copy would prove nothing about the missing package. Branch 2 fires
saying **`emptyChannelDelay` is `500 ms`**, which is phase 1's measurement arriving as a
verdict. Branch 5 is the mechanised form of the two type names phase 1 found in design §7:
`RocketMqSubscription` and `MQTTMessagingGatewayConfiguration` would each have produced it.

**Four decisions phase 2 made that design does not name.** Each was forced by something phase
1 measured:

1. **A parameter with no declared default reads `none`, not a value.** Whatever the instance
   holds for it is the argument the checker passed in. That is standing obligation 1's word
   for it and a fact about the parameter rather than a limit of the tool.
2. **An argument the checker had to supply is declared, never printed.** Phase 1's *thirteen
   constructors reject their own defaults* lands exactly here: on `Subscription`,
   `requestType` and `messagePumpType` are supplied and therefore unreadable, so the table
   owes a `manual:` for each — which declares and counts. **Necessity is measured per type**,
   probe 1.4's method carried into the tool: each supplied candidate is put back to its own
   default and kept only if removal breaks construction. `makeChannels` is the case that
   proves it works — it reads `Create` from its own default and stays fully checkable.
3. **`max(props, ctor)` is a SELECTION, not a union.** The union would document `bufferSize`
   and `BufferSize` as two options, when they are one option and the case hazard requirements
   §7.1 names. Properties are `DeclaredOnly`, so a transport publication owes rows for its own
   options and not for `Publication`'s ten — which is what design §7 maps as two tables.
4. **Both escapes are checked against the type.** `OMIT NAMES NOTHING` and `MANUAL NAMES
   NOTHING` catch a stale escape; `MANUAL NOT NEEDED` catches a `manual:` over a default the
   tool can read, which would be an unchecked row wearing a declaration.

**A third instrument now agrees with the other two.** `survey.py` parses source, probe 1.5's
oracle reflects over assemblies, and the checker enumerates what a table owes. Twelve types
run through it, and **every count matches phase 1's** — including the five where design §7 is
superseded:

| Type | Design §7 | `optioncheck` |
|---|---:|---:|
| `GcpPubSubSubscription` | 33 | 33 |
| `KafkaSubscription` | 30 | 30 |
| `KafkaPublication` | 17 | 17 |
| `KafkaMessagingGatewayConfiguration` | 11 | 11 |
| `ProducersConfiguration` | 26 | **22** |
| `Publication` | 8 | **10** |
| `InMemorySubscription` | 2 † | **17** |
| `RmqMessagingGatewayConnection` | 19 | **11** |
| `RelationalDatabaseConfiguration` | 8 | 8 |
| `AzureBlobLockingProviderOptions` | 3 | 3 |
| `DynamoDbInboxConfiguration` | 1 | 1 |
| `QuartzSchedulerFactory` | 4 (§12.1) | 4 |

**None of the twelve failed to construct and none produced an unprintable default**, so the
synthesiser reaches every shape phase 3 through phase 9 will meet. **`QuartzSchedulerFactory`
is the one to notice**: design §12.1 says the schedulers are invisible to `survey.py` because
`SURFACE_RE` matches filenames and a factory is not a `*Configuration.cs`. **The marker binds
a type, so the tool does not care** — phase 9's 25 options are checkable on the same terms as
everything else.

**AC4's evidence, naming the job.** The `options` job ran on
[#129](https://github.com/BrighterCommand/Docs/pull/129) and passed in **33s**, and the line
that matters is the one it printed on the runner:

```text
optioncheck — Brighter 10.7.0, 62 pinned packages (tools/optioncheck/optioncheck.csproj)
scope: 1 table, 1 row, 1 type, on 1 page of 147 files scanned.
0 mismatches across 1 table and 1 row.
```

**That is the same scope the local run reports**, which is what says the CI job is checking the
corpus rather than passing on an empty one — and it is why task 2.6 exists. All eight check
rows were read in full: `check`, `options` and `versions` on both the `push` and the
`pull_request` event, plus the two GitBook rows.

**The six gates in §2.8 are unmoved to the digit**, on the branch and after staging: link 150,
pagelint 0 errors / 790 warnings / 148 pages, shape 147 pages / 12 sections / widest 10 of 20,
redirects 77 entries / 7858 bytes, versioncheck 0 stale of 18 across 5, `--verify` 147/147/147.
**`pagelint --changed` reaches 0 code blocks and says so** — the one page edited is a table,
not a fence, so the strict run is legitimately vacuous and prints that it is.

---

## Phase 3 — D4 and D5, the two core Reference pages (Docs PR)

**Goal:** 73 options across 8 tables, on the two pages already named *Configuration Reference*
that carry **zero** tables today — 012's premise confirmed at the highest-traffic surface.

**This phase must not:** move any prose that is already on those pages. The edit is additive
(requirements §3.2): each named surface gains a marker and a table under a qualified heading,
in the section that already discusses it.

- [x] **Task 3.1:** `CommandProcessorConfigurationReference.md` — `AddProducers` and
      `AddBrighter`
  - Input: design §7.1; `ProducersConfiguration` (26), `BrighterOptions` (9) at `10.7.0`
  - Output: `## AddProducers Options` and `## AddBrighter Options`, each with its marker and
    table
  - Notes: `ProducersConfiguration`'s 26 settable properties make it the **second-widest
    surface in Brighter**, behind `GcpPubSubSubscription`'s 33. Both are property-driven, so
    `Option` is the property name here and the parameter-versus-property hazard does not
    arise — **check that rather than assuming it.**

- [x] **Task 3.2:** `CommandProcessorConfigurationReference.md` — publication, handlers,
      pipeline validation
  - Input: `Publication` (8), `HandlerConfiguration` (2), `BrighterPipelineValidationOptions`
    (2)
  - Output: `## Publication Options`, `## Handler Configuration Options`,
    `## Pipeline Validation Options`
  - Notes: **`Publication`'s 8 are the base every transport inherits** (design §3.2), so this
    table is the one phases 5 and 6 link *up* to. Get its wording right before five transport
    pages point at it. `HandlerConfiguration` is the P0 member of task 2.4's factory list.

- [x] **Task 3.3:** `DispatcherConfigurationReference.md` — subscription, global inbox,
      `AddConsumers`
  - Input: `Subscription` (17), `InboxConfiguration` (5), `ConsumersOptions` (4)
  - Output: `## Subscription Options`, `## Global Inbox Options`, `## AddConsumers Options`
  - Notes: **`Subscription`'s 17 are the base every transport subscription inherits**, and
    `EmptyChannelDelay` — task 1.2's probe — is on this table. It is the first row in the spec
    where the signature and the truth disagree, and the page it lands on is one of the two
    highest-traffic Reference pages. **Rule 5 is an error and this is the page most likely to
    want the assembly name**: backticks for a real type or assembly, "Dispatcher" in prose.

- [x] **Task 3.4:** Verify phase 3 — `optioncheck`, the six gates, and the AC8 walk
  - Input: the 73 descriptions written above
  - Output: `optioncheck` exit 0 with a scope naming 8 tables and 73 rows; the six gates; a
    recorded AC8 verdict
  - Notes: **AC8 is the walk, not the gates.** Read every description and ask whether it
    describes or argues; one sentence, present tense, rationale linked rather than restated.
    `git add` before `pagelint --changed`, and read its scope line — these pages have C# blocks
    and standing obligation 8 applies.

### Phase 3 as executed — 2026-08-28, 4/4

**Eight tables, 71 rows, on the two pages that carried none.** `optioncheck` reports
**9 tables, 72 rows, 9 types across 3 pages** with the phase 2 seed, exit 0.

**The phase is 71 options, not the 73 the phase table says.** D4's 47 became **45** at phase
1 — `ProducersConfiguration` 26 → 22 and `Publication` 8 → 10 — and D5's 26 held exactly.
Every figure was re-derived from the type when its table was written, which is standing
obligation 3, and the phase table above is left as approved.

**The tables, and what each cost:**

| Table | Type | Rows | `manual:` |
|---|---|---:|---:|
| `## AddBrighter Options` | `BrighterOptions` | 9 | 2 |
| `## Pipeline Validation Options` | `BrighterPipelineValidationOptions` | 2 | 0 |
| `## AddProducers Options` | `ProducersConfiguration` | 22 | 1 |
| `## Publication Options` | `Publication` | 10 | 0 |
| `## Handler Configuration Options` | `HandlerConfiguration` | 2 | 0 |
| `## Subscription Options` | `Subscription` | 17 | 3 |
| `## AddConsumers Options` | `ConsumersOptions` | 4 | 1 |
| `## Global Inbox Options` | `InboxConfiguration` | 5 | 2 |

**Nine `manual:` declarations across 71 rows — 13%, and that is the residue requirements
§7.3 asked to be measured rather than estimated.** Every one is the same shape: a default
assigned in a constructor body as an *object*, which has no printable value —
an `InMemoryInbox`, an `InMemoryRequestContextFactory`, an empty `ProducerRegistry` — or a
parameter the constructor refuses its own default for. **This is requirements §5.1's third
shape with an object on the end of it**, and neither the requirements nor the design names
that variant: §5.1's example coalesces to a `TimeSpan`, which prints.

### The ledger — two entries, both on `CommandProcessorConfigurationReference.md`

Standing obligation 10 says record the mismatch before fixing it. **Neither was found by
`optioncheck`**, which is worth as much as the entries: both are in a **bullet list**, and
the tool reads tables. They were found by writing the correct table beside the prose.

1. **`MaxOutstandingMessages` and `MaxOutStandingCheckIntervalMilliSeconds` were documented
   as publication options.** They are on `ProducersConfiguration`, not `Publication` — the
   `Publication` type has neither. A reader following that list sets them on the wrong
   object.
2. **`MaxOutStandingCheckIntervalMilliSeconds` is not the name of anything.** The member is
   `MaxOutStandingCheckInterval` and it is a `TimeSpan`; the `MilliSeconds` suffix is a V9
   spelling that survived on the page.

Both bullets are corrected in place and the section now points at the two tables. **This is
012's premise, on the highest-traffic Reference page in the corpus, found in the first phase
that wrote a table.**

> **A third finding, in Brighter rather than here, and out of scope for this repository.**
> `Publication.Type`'s doc comment says the default is
> *"goparamore.io.Paramore.Brighter.Message for backward compatibility"*; the code assigns
> `CloudEventsType.Empty` and the constructed instance reads back empty. The page says what
> the assembly does and notes the disagreement. `src/` is outside this programme's
> exception, so this is a Brighter issue if anyone raises one — the same shape as
> [Brighter#4277](https://github.com/BrighterCommand/Brighter/issues/4277).

### What phase 3 changed in the instrument, and why each was forced

**`optioncheck --describe <type>…`** prints what a table for a type owes, in the four
columns, with the descriptions empty. Standing obligation 3 is *write the table from the
type*, and before this the only way to learn a default was to write a wrong one and read the
mismatch — transcription by trial and error, sixty-odd tables still to come.

> **The objection is circularity**: a table pasted from the tool agrees with the tool by
> construction, so AC2 proves nothing. It does not bite, and the reason is worth stating.
> `Option`, `Type` and `Default` **are** the assembly's truth by definition — design §6.2
> makes the default readable only from an instance, so a human writing that column is
> transcribing this output or guessing. And **AC2's subject is drift**: today's table against
> tomorrow's assembly, which no agreement today can fake. The column the tool cannot supply
> is the one carrying meaning, and it is left blank on purpose.

**Two defects in the checker, both found by pointing it at types phase 2 never met:**

1. **A property fed by a required constructor argument read back the checker's own value.**
   The ctor route already handled this; the property route only caught string-shaped
   sentinels, so an `int` of 1 or an enum would have been published as a default. The
   synthesiser now reports **every** parameter it injected, not only the defaulted ones, and
   a property assigned from one is `manual:`.
2. **A value whose printable form is empty rendered as nothing**, which is indistinguishable
   from a blank cell — and a blank `Default` is a finding in its own right, so no table for
   `Publication` could ever have gone green. It renders `empty` now, and a table may write
   `""` instead.

**`pagelint.py` rule 5 gained a fourth exception: an HTML comment.**
`ConsumersOptions` lives in `Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection`,
so its marker carries the V9 name **by construction** and cannot put it in backticks — the
marker's grammar takes a bare type name. Task 3.3 predicted this page would be the one to
want it. The exemption rests on the same argument as the fenced-block and code-span ones,
and phase 2 measured it rather than assuming: **an HTML comment publishes as nothing.** It
is not the opt-out comment and does not replace it — this is *the word is not on the page*,
where the opt-out is *the page is about the word*. **Red-proved**: a bare
`The Service Activator reads messages` appended to the same page still errors, and the page
was restored byte-identical.

### AC8, walked

**Every one of the 71 descriptions is one sentence, present tense, and states what the option
does rather than why.** Swept mechanically as well as read: no cell contains *because*, *so
that*, *should*, *prefer*, *recommend* or *note that*, none contains two sentences, and all
71 open with a capital and end in a full stop. Where rationale was worth having it is in
prose **after** the table — `PolicyRegistry` being obsolete, `ShutdownTimeout` being the one
to raise for long-running handlers, `null` on the producers configuration usually meaning
Brighter supplies something itself.

**Two things the tables say that no reader could have got from the prose**, both from
standing obligation 1's *the spelling the reader types*: on `Subscription`,
`subscriptionName` is the `Name` property and `getRequestType` is `MapRequestType` — **two
parameters whose property differs by more than case**, which the page now says in as many
words. And `Publication.Source` reads back `http://goparamore.io/`, with the trailing slash
`Uri` normalises in, where the source says `http://goparamore.io`.

**Placement worked.** Standing obligation 8's cheapest defence — put the table where the
diff cannot reach a code block — held: `pagelint --changed` reports **0 code blocks strict**
across 28 hunks, and says so in its own words rather than passing quietly.

**The six gates are unmoved to the digit**: link 150, pagelint 0 errors / 790 warnings / 148
pages, shape 147 / 12 / widest 10 of 20, redirects 77 / 7858, versioncheck 0 stale of 18
across 5, `--verify` 147/147/147. **No page was created**, so `SUMMARY.md` is untouched and
none of the four page-count gates could move. The phase 2 red-proof still fires **8/8** after
the checker changes.

> **Phase 2's publication check, recorded here because it was measured after that PR
> merged** — the pattern where a phase's evidence lands in the following PR.
> `outbox-and-inbox/sweepercircuitbreaking` returned **200, 14,711 bytes**, the normalised
> row published as written, and **`grep -c optioncheck` over the published `.md` variant is
> 0**. Design §5's first reason for the marker — *"it is invisible to readers"* — rested on
> the `<!-- pagelint: allow-serviceactivator -->` precedent, and it is now measured for this
> marker too. It is also what licenses the rule 5 exemption above.

### The ledger — a third entry, and it does not compile

**Found after phase 3 merged**, by reading the two pages as published rather than as
written — which is the check `--verify` exists to prompt. Recorded here because task 11.3
aggregates this section, and fixed in its own PR because it is a correction to code rather
than a table.

**Six code examples across three published pages set two properties on an `RmqPublication`
that `RmqPublication` does not have.** `MaxOutStandingMessages` and
`MaxOutStandingCheckIntervalMilliSeconds` belong to `ProducersConfiguration` — the second
under a name it has not had since V9, and as a `TimeSpan` rather than an `int` of
milliseconds. The pages are `BrighterBasicConfiguration.md` (**the page the whole corpus
links to as the one path that works**), `RabbitMQConfiguration.md` (three of the six) and
`CommandProcessorConfigurationReference.md` (two).

**Proved with the compiler, not with reflection**, which is 009's method and the reason it
is worth keeping: the block pasted verbatim into a project referencing the pinned packages
fails with **`CS0117` twice** — *'RmqPublication' does not contain a definition for
'MaxOutStandingMessages'* and *…for 'MaxOutStandingCheckIntervalMilliSeconds'*. The
corrected form — the two settings moved onto `configure`, with
`MaxOutStandingCheckInterval` taking a `TimeSpan` — builds clean.

**A second defect fell out of compiling the corrected block**, and no reflection pass would
have found it: `BrighterBasicConfiguration.md`'s example calls `UseOutboxSweeper()`, which
lives in `Paramore.Brighter.Outbox.Hosting` — a package and namespace the page never names.
`CS1061` until that `using` is added. Rule 6 turning strict on the three blocks this edit
touched is what forced them to carry `using` directives at all, so **the defect was found by
a convention, not by looking for it**; the repo-wide debt goes 790 → **787** as a result.

**Why `optioncheck` did not catch any of this, and should not be changed to:** it reads
tables. A code block is prose to it. The instrument for a code block is the compiler, and
009's standing lesson is to *extract the page's own fences into a project and build them*
rather than diffing after the fact. **All three ledger entries so far were found by a human
writing a table beside prose that was already there** — which is what requirements §11 means
by *transcribing existing documentation propagates whatever drift is already there*.

---

## Phase 4 — D15, the relational reference page (Docs PR)

**Goal:** the eight relational options documented **once** instead of thirteen times. This is
the largest single application of *do not duplicate — link to the authoritative source* in the
spec, and it is the one structural addition design proposed.

**This phase must not:** put the table on an existing page. Every candidate host was checked
and each failed — `BrighterOutboxSupport.md` and `BrighterInboxSupport.md` are **Explanation**,
`BrighterBasicConfiguration.md` is **How-to**, and `PostgresOutbox.md` privileges one of five
providers (design §8.4).

- [ ] **Task 4.1:** Write `contents/RelationalDatabaseConfigurationReference.md`
  - Input: design §8.4's outline; `RelationalDatabaseConfiguration` (8 options); §7.3's
    seventeen components across four families
  - Output: the page — banner, opening sentence, front matter, four sections:
    `## Relational Database Configuration Options` (the marked table),
    `## Which Components Take the Relational Configuration` (the seventeen, by family, each
    linked), `## Registering the Relational Configuration`, `## Further Reading`
  - Notes: **Two of those headings were `## Which Components Take This Configuration` and
    `## Registering the Configuration` in a draft** — both unique across the corpus, so rule
    3a's *tooled* half passes on them, and both unattributable in a retrieval chunk, which is
    the entire reason the convention exists. *A rule with a tool for half of it will pass the
    half nobody checks.* The registration section links `PostgresOutbox.md`, which shows the
    `AddSingleton<IAmARelationalDatabaseConfiguration>` line twice; the ergonomics of needing
    it at all are already **[Brighter#4279](https://github.com/BrighterCommand/Brighter/issues/4279)**
    and are not this page's argument to make. Budget ~140 lines — a prose budget, not a
    prediction.

- [ ] **Task 4.2:** Land the entry, the row, and the checks
  - Input: design §9.1's first diff
  - Output: the `SUMMARY.md` entry **nested under *Basic Configuration*** beside the two
    Reference pages it joins; a `pagetypes.tsv` row; the six gates and `optioncheck`
  - Notes: **Nesting is what keeps *Brighter Configuration* from growing a top-level entry**,
    and the entry text is what reaches `/llms.txt` — write it equal to the H1. Expect link,
    pagelint, shape and (after publication) `--verify` each to move by one, and
    **`--check-redirects` not to move at all**; assert that rather than assuming it.

---

## Phase 5 — D6, the five documented transports (Docs PR)

**Goal:** 186 options across the five, plus `InMemoryTransport.md`'s table. These pages already
exist and already have configuration prose; what they lack is a table with a default in it.

**This phase must not:** rewrite the pages' existing examples. It also must not restate
`Subscription`'s 17 or `Publication`'s 8 — a transport table lists what the transport **adds or
overrides** and links up to phase 3's tables (design §3.2).

- [ ] **Task 5.1:** `RabbitMQConfiguration.md` — 44 options, 3 tables
  - Input: `RmqSubscription` 24, `RmqMessagingGatewayConnection` 19, `RmqPublication` 1
  - Output: three marked tables
  - Notes: **One table, the Async client's** — ruled at design §13.3. `RMQ.Async`'s
    `RmqSubscription` takes 24 constructor parameters and `RMQ.Sync`'s takes 23; the difference
    is **`queueType`**, measured by diffing the parameter lists rather than inferred from the
    two counts. **`queueType`'s row says the Sync client has no such parameter.** Two tables
    would be 23 near-duplicate rows for a one-parameter difference.

- [ ] **Task 5.2:** `KafkaConfiguration.md` — 58 options, 3 tables
  - Input: `KafkaSubscription` 30, `KafkaPublication` 17, `KafkaMessagingGatewayConfiguration`
    11
  - Output: three marked tables
  - Notes: **`KafkaSubscription` contributes 30, not 47** — `max(props, ctor)`, not their sum
    (`survey.py:143`). It is the largest single table in the spec after
    `GcpPubSubSubscription`'s. `MakeChannels` is on it, and design's own illustration is that
    the corpus mentions `MakeChannels` 23 times across 11 pages **without once pairing it with
    the word "default"** — this row is where that ends.

- [ ] **Task 5.3:** `AWSSQSConfiguration.md` — 31 options, 3 tables
  - Input: `SqsSubscription` 24, `SqsPublication` 4, `SnsPublication` 3
  - Output: three marked tables
  - Notes: `SqsSubscription` is the one type in task 1.4's table needing a **fourth**
    constructor argument, `channelType`. If task 2.4's synthesiser handles it generically this
    is unremarkable; if it does not, this is where that shows.

- [ ] **Task 5.4:** `AzureServiceBusConfiguration.md` — 26 options, 3 tables
  - Input: `AzureServiceBusSubscription` 18, `AzureServiceBusSubscriptionConfiguration` 6,
    `AzureServiceBusConfiguration` 2
  - Output: three marked tables
  - Notes: two configuration-shaped types on one page, one nested inside the subscription.
    **Qualify both headings by what they configure**, not by the type name alone.

- [ ] **Task 5.5:** `PostgreSQLMessageBroker.md` — 27 options, three existing tables replaced
  - Input: `PostgresSubscription` 24, `PostgresPublication` 3,
    `PostgresMessagingGatewayConnection` **1†, and task 1.5's real figure**
  - Output: three marked tables replacing the three that are there
  - Notes: **This page loses three tables and gains three.** Its existing three are in two
    non-§7.1 shapes and one has **no `Default` column at all** (design §12.2). Its connection
    surface is **D15's**, not its own — the PostgreSQL transport takes
    `RelationalDatabaseConfiguration` (§7.3), which was not obvious and is why §8.3's MSSQL
    ruling is a rule. `PostgresMessagingGatewayConnection` is a `†` row: **absent from the 67
    entirely** because it has a primary constructor. Write it from the type.

- [ ] **Task 5.6:** `InMemoryTransport.md` — one table, and it is **17 rows, not 2**
  - Input: `InMemorySubscription` — **2 by the unfixed survey, 17 primary-constructor
    parameters by the type** (design §12.5); task 1.5's re-run
  - Output: one marked table
  - Notes: **The single clearest case in the spec for standing obligation 3.** A writer copying
    design §7.2's column would write a two-row table and every gate in this repository would be
    green on it — `optioncheck` included, until it enumerates the type. Write it from the type.

- [ ] **Task 5.7:** Verify phase 5 — `optioncheck`, the six gates, and the AC8 walk
  - Input: the tables written above
  - Output: exit 0 with a scope naming 16 tables; the six gates; a recorded AC8 verdict
  - Notes: **No page is created, so link, pagelint, shape and `--verify` should not move.**
    The one that can move is pagelint's **warning count**, if a table lands beside a C# block
    with no `using` directives — standing obligation 8. Read `--changed`'s scope line to see
    whether it reached one.

---

## Phase 6 — D12, the five new transport pages (Docs PR)

**Goal:** 151 options and five pages, satisfying requirements §14's one ordering obligation —
these tables are the first in the spec written under a gate that already exists, rather than
the last brought under one.

**This phase must not:** work from a template. **Redis and MQTT ship no publication type;
MSSQL ships neither a publication nor a connection type** (design §8.2). A writer working from
§8.1's skeleton would produce a `## Redis Publication` section describing `Publication`'s base
eight as though Redis added something, **and every gate in this repository would be green on
it.**

- [ ] **Task 6.1:** Write `contents/GcpPubSubConfiguration.md` — 43 options, 3 tables
  - Input: `GcpPubSubSubscription` 33, `GcpMessagingGatewayConnection` 7, `GcpPublication` 3;
    design §8.1's skeleton
  - Output: the page — General, Connection, Publication, Subscription, Configuration Example,
    Further Reading; three marked tables. Budget ~190 lines
  - Notes: **`GcpPubSubSubscription`'s 33 constructor parameters make it the widest subscription
    Brighter ships**, wider than Kafka's 30, with **19 of its 30 defaults of the `null` kind** —
    so this page is where task 1.2's probe earns its keep, nineteen times. **There is no GCP
    Pub/Sub sample anywhere in Brighter** and the transport has **zero mentions in the corpus
    today**: the example is written from the source types, and task 6.7 compiles it.

- [ ] **Task 6.2:** Write `contents/RedisConfiguration.md` — 33 options, **2** tables
  - Input: `RedisSubscription` 19, `RedisMessagingGatewayConfiguration` 14
  - Output: the page; **no `## Redis Publication` section.** Budget ~150 lines
  - Notes: Redis appears in the corpus today **only as a row in cross-cutting comparison
    tables** — `ReactorAndProactor.md` tells a reader it supports both APIs natively while
    offering nowhere to configure it. Sample material exists: `samples/TaskQueue/RedisTaskQueue`,
    three files.

- [ ] **Task 6.3:** Write `contents/RocketMQConfiguration.md` — 29 options, 3 tables
  - Input: `RocketMqSubscription` 22, `RocketMessagingGatewayConnection` **4†**,
    `RocketMqPublication` 3; task 1.5's re-run
  - Output: the page. Budget ~155 lines
  - Notes: `RocketMessagingGatewayConnection` is a `†` row — under-counted by one because of a
    primary constructor. **Zero mentions in the corpus and no sample in Brighter**; written
    from the source types, compiled at task 6.7.

- [ ] **Task 6.4:** Write `contents/MQTTConfiguration.md` — 27 options, **2** tables
  - Input: `MqttSubscription` 19, `MQTTMessagingGatewayConfiguration` 8
  - Output: the page; **no publication section.** Budget ~140 lines
  - Notes: no sample in Brighter. Requirements §7.2.1 already rules that these pages need no
    running broker — they are Reference pages, not tutorials — so nothing here proposes to
    build one.

- [ ] **Task 6.5:** Write `contents/MSSQLMessageBroker.md` — 19 options, **1 table and a link**
  - Input: `MsSqlSubscription` 19; design §8.3 — `MsSqlMessageProducer` takes
    `RelationalDatabaseConfiguration` (`MsSqlMessageProducer.cs:69`, `:86`), and there is no
    `MsSqlMessagingGatewayConfiguration` and no `MsSqlPublication` anywhere in the package
  - Output: the page — Overview, Connection (**links D15; no table here**), Subscription,
    Configuration Example, Further Reading. Budget ~130 lines
  - Notes: **Named to sit parallel with `PostgreSQLMessageBroker.md`** — both are a relational
    database pressed into service as a transport, and *Further Reading* points at
    `PostgreSQLBrokerTradeOffs.md`, which is the same trade-off. Sample material exists:
    `samples/TaskQueue/MsSqlMessagingGateway`, eight files.

- [ ] **Task 6.6:** Five `SUMMARY.md` entries, five `pagetypes.tsv` rows
  - Input: design §9.1's second diff
  - Output: **MSSQL beside PostgreSQL** (they are the same idea), then GCP Pub/Sub, RocketMQ,
    MQTT, Redis in the order requirements §10 lists them, all **top-level**, all before the
    InMemory entry
  - Notes: this takes *Transports* **7 → 12** against S2's ceiling of **20**, so eight of
    headroom (§2.2). **Ten transports are peers** — a reader picks one and never reads the
    other nine — which is why they stay flat rather than gaining a family parent, and why S2
    moved instead. No entry moves a URL: slugs are filename-derived.

- [ ] **Task 6.7:** Compile every C# block on the five new pages
  - Input: design §11; the packages `optioncheck` has already pinned and restored
  - Output: a recorded compile of every ```` ```csharp ```` block, extracted from the published
    markdown and built — not run, compiled
  - Notes: **Three of these five transports have no sample anywhere in Brighter**, so
    compilation is the only check those examples get. It catches an invented method name, a
    wrong namespace and a missing `using` — the whole class of defect a written-from-source
    example is exposed to — and it reuses a restore that exists anyway. **Extract from the
    page, not from a draft**: 009 proved that a transcription error is caught by building the
    page's own fences and is invisible to reading the two side by side.

- [ ] **Task 6.8:** Verify phase 6 — `optioncheck`, the six gates, AC6 and the AC8 walk
  - Input: the five pages
  - Output: exit 0 with a scope naming 11 new tables; the six gates; **AC6 walked** — all ten
    shipping transports now have a configuration page, no orphan, no pagelint error on any of
    the five; a recorded AC8 verdict
  - Notes: expect link, pagelint and shape each **+5**, and `--check-redirects` unmoved.
    `--verify` moves only after publication — predict the five paths with `urlmap.py`, never
    guess them, and check `sitemap-pages.xml` lists them before probing.

---

## Phase 7 — D13 and D16, the four Firestore and Spanner store pages (Docs PR)

**Goal:** four pages and **zero options**. Both stores ship at `10.7.0` on both the outbox and
the inbox side with no page at all; three of the four pages document everything by link.

**This phase must not:** give Spanner a table. It has **no configuration type** —
`SpannerOutbox.cs:32` takes `IAmARelationalDatabaseConfiguration` — and the same holds one
family over. **These two pages are the cleanest statement of standing obligation 4 in the
spec.**

- [ ] **Task 7.1:** Write `contents/FirestoreOutbox.md` — 7 options, 1 table
  - Input: `FirestoreConfiguration` (7, and a `†` row in design §12.5 — under-counted by two);
    design §8.5's outline; `PostgresOutbox.md` as the registration example's shape
  - Output: the page — Configuration, Options (the marked table), Provisioning (links
    `BoxProvisioning.md`), Further Reading. Budget ~120 lines
  - Notes: **this table is linked by the Firestore inbox and the Firestore lock**, so it is
    written once and pointed at twice — the same economy D15 makes at four times the scale.

- [ ] **Task 7.2:** Write `contents/SpannerOutbox.md` — **0 options, 0 tables**
  - Input: design §8.5; `SpannerOutbox.cs:32`
  - Output: the page — Configuration (**links D15**), Provisioning (what is genuinely
    Spanner-specific), Connection Provider, Further Reading. Budget ~110 lines
  - Notes: **AC6b is this task and 7.1 together**: Firestore with a table, Spanner without. The
    page reports the absence rather than working around it.

- [ ] **Task 7.3:** Write `contents/FirestoreInbox.md` and `contents/SpannerInbox.md` — **0
      options between them**
  - Input: design §8.5's second block and §12.6; both take
    **Prerequisites: [Inbox Support](/contents/BrighterInboxSupport.md)**
  - Output: two pages, four sections each, no table: Firestore links `FirestoreOutbox.md`'s
    table, Spanner links D15. Budgets ~100 each
  - Notes: **D16 exists because a ruling made about one family is a claim about a rule.** The
    outbox side was settled at requirements §13.2; nobody asked what the inbox side looked
    like, and one `git ls-tree` found it identical — eight stores, the same two missing.
    *The cheapest way to test a rule is to point it at the neighbouring family before anyone
    else does.*

- [ ] **Task 7.4:** Four `SUMMARY.md` entries, the compile, and the checks
  - Input: design §9.1's third diff
  - Output: four **nested** entries — Firestore and Spanner outbox under *Outbox Support*,
    the two inboxes under *Inbox Support*, each immediately **before** its family's InMemory
    entry, matching the order both families already use; four `pagetypes.tsv` rows; every C#
    block on the four pages compiled (§2.4); `optioncheck`; the six gates; AC6b and AC8
  - Notes: nesting is why these four add **nothing** to *Outbox and Inbox*'s top-level count —
    the section already carries 39 pages behind 9 entries, which is family nesting doing the
    only job that controls a section's width.

---

## Phase 8 — D7 and D8, the outbox, inbox and lock pages (Docs PR)

**Goal:** 35 options across eight outbox/inbox pages, 8 across three lock pages, and the last
of §2.7's twelve tables — the largest editing phase by page count, and the one that most needs
the checker to be trusted already.

**It is smaller than design §3.1 implies, and §2.7 is the reason.** D8's *"normalisation; five
already have tables"* reads as a body of work; located page by page, eleven of the twelve
tables belong to a task that owns the page for another reason. What is left here is
`AsyncAPISupport.md`.

**This phase must not:** normalise `CausationTrackingStores.md`'s two tables. They are
`Member | What your implementation must do` — an interface contract for a reader writing their
own store, with no type, no default and nothing for reflection to check. They match §3.1's
option-shaped regex correctly and are **out of scope for the checker: they get no marker**
(design §12.2).

- [ ] **Task 8.1:** The outbox pages — `DynamoOutbox.md`, `MongoDBOutbox.md`,
      `InMemoryOutbox.md`
  - Input: `DynamoDbConfiguration` 7, `MongoDbConfiguration` 7, `MongoDbCollectionConfiguration`
    5, `InMemoryBoxConfiguration` 4
  - Output: four marked tables across three pages
  - Notes: **`MongoDbConfiguration`'s table is linked by the Mongo inbox and the Mongo lock**,
    like Firestore's — write it once and point at it. `MongoDbLuggageStoreOptions` is a
    different type and is **P2**, not this task.

- [ ] **Task 8.2:** The sweeper, archiver, provisioning and inbox pages —
      `BrighterOutboxSupport.md`, `OutboxArchiver.md`, `BoxProvisioningConfiguration.md`,
      `DynamoInbox.md`
  - Input: `TimedOutboxSweeperOptions` 4, `TimedOutboxArchiverOptions` 4,
    `BoxProvisioningOptions` 2, `DynamoDbInboxConfiguration` **1†, unmarked in design §7.3
    and under-counted by one (§2.9)**
  - Output: four marked tables across four pages
  - Notes: **`BrighterOutboxSupport.md` is an Explanation page** and gains a Reference table —
    that is why D15 exists as a separate page rather than living here, and the same objection
    applies in miniature. Keep the table to the sweeper's own options and link the rationale.
    `DynamoDbInboxConfiguration` is a `†` row; write it from the type.

- [ ] **Task 8.3:** The seven lock pages — three tables, two links, two absences
  - Input: design §7.4, measured constructor by constructor — **and three of this task's four
    figures are floors (§2.9), which is more than any other task in the spec**.
    `DynamoDbDistributedLock.md` (`DynamoDbLockingProviderOptions` **4†**),
    `AzureBlobDistributedLock.md` (`AzureBlobLockingProviderOptions` **3†**),
    `PostgresDistributedLock.md` (`PostgresLockingProviderOptions` **1†, and absent from the
    67 entirely**);
    `MongoDbDistributedLock.md` and `FirestoreDistributedLock.md` link §7.3's tables;
    `MsSqlDistributedLock.md` and `MySqlDistributedLock.md` take a connection provider and
    have **no options type at all**
  - Output: three marked tables, two link sections, two pages stating the absence — **and
    five of §2.7's twelve normalised in passing**, because all five pages with existing
    tables are on this list
  - Notes: **A draft of design §7.4 said the Postgres, MSSQL and MySQL locks take
    `RelationalDatabaseConfiguration`. None of them does.** It was checked because §7.3's list
    of seventeen did not name them — *the tell was the omission, not the claim*. **Write from
    §7.4, not from §3.1's summary line** (§2.3). **Design §7.4 marks only the Postgres row**;
    the Azure Blob and DynamoDB options types are under-counted by two each and carry no mark
    at all (§2.9). All three tables are written from the type.

- [ ] **Task 8.4:** Normalise `AsyncAPISupport.md`'s table — the one of the twelve no other
      task owns
  - Input: §2.7's table; design §12.2's six header shapes; `AsyncApiOptions`, 6 options
  - Output: one table in the §7.1 shape with its marker
  - Notes: **This task is one table, not twelve, and §2.7 is why.** Eleven of the twelve are
    normalised by whoever owns the page anyway — three at task 5.5, five at task 8.3, one at
    task 2.6, and `CausationTrackingStores.md`'s two are **not option tables and get no
    marker**. `AsyncAPISupport.md` is the residue, and it is the one page here design §7.6
    files under **P2**: its table already exists and is *"in nearly the right shape"*, so the
    cost is shape and a marker rather than six new rows. **If the phase is running short, this
    is the task to drop** — dropping it leaves eleven of twelve normalised and one P2 table
    unchanged, which is a coherent state; dropping any of the other four leaves a page
    half-converted.

- [ ] **Task 8.5:** Verify phase 8 — `optioncheck`, the six gates, and the AC8 walk
  - Input: everything above
  - Output: exit 0; the six gates; a recorded AC8 verdict; **the `omit:` and `manual:` counts
    read off the scope line and recorded**
  - Notes: **This is where requirements §7.3 item 10 gets measured rather than estimated** —
    the residue is the sum of the `manual:` declarations, printed per option and by name.
    Record it here, because phase 11 will quote it and a number nobody derived is a number
    somebody guessed.

---

## Phase 9 — D9, the schedulers (Docs PR)

**Goal:** 25 options across six pages — the family `survey.py` cannot see at all, because
**no scheduler ships a configuration type**. The surface is factory properties.

**This phase must not:** give `TickerQScheduler.md` a table. `TickerQSchedulerFactory` has
**zero** settable properties (design §12.1). That is standing obligation 4's seventh instance.

- [ ] **Task 9.1:** `AwsScheduler.md`, `QuartzScheduler.md`, `AzureScheduler.md`
  - Input: `AwsSchedulerFactory` 10, `QuartzSchedulerFactory` 4,
    `AzureServiceBusSchedulerFactory` 4
  - Output: three marked tables
  - Notes: **the marker binds a *type*, not a filename**, so
    `<!-- optioncheck: Paramore.Brighter.MessageScheduler.Aws.AwsSchedulerFactory -->` is a
    valid marker and D9 needs no new mechanism. That is the whole practical consequence of
    §12.1; the rest of it is a caution about figures.

- [ ] **Task 9.2:** `InMemoryScheduler.md`, `HangfireScheduler.md`, `TickerQScheduler.md`
  - Input: `InMemorySchedulerFactory` 4, `HangfireMessageSchedulerFactory` 3,
    `TickerQSchedulerFactory` **0**
  - Output: two marked tables, and **one page stating that TickerQ's factory exposes no
    settable properties** — with what a reader configures instead
  - Notes: `## Hangfire Best Practices` is better than `## Hangfire Scheduler Best Practices`,
    and no rule can tell you that — heading qualification's editorial half is a judgement.

- [ ] **Task 9.3:** Verify phase 9 — `optioncheck`, the six gates, and the AC8 walk
  - Output: exit 0 with a scope naming five new tables and 25 rows; the six gates; a recorded
    AC8 verdict
  - Notes: **25 options that no figure in the requirements counts.** Nothing in the
    requirements is wrong — §2 is explicit that the survey counts *configuration types*, and a
    factory is not one. This is *ask what a figure counted.*

---

## Phase 10 — D10, the two stale cross-cutting tables (Docs PR)

**Goal:** the one **corrective** rather than additive edit in the spec. These two tables are
the drift 012 exists to stop, happening today, with every gate green on them.

**This phase must not:** correct them from memory or from the other pages. AC7 is *diffed
against `survey.py` output at the release ref* — the fixed survey, after task 1.5.

- [ ] **Task 10.1:** `ReactorAndProactor.md` — `#transport-native-support`
  - Input: the anchor, not the line number (requirements §11); `survey.py --ref 10.7.0` after
    task 1.5
  - Output: rows for **GCP Pub/Sub and RocketMQ**, which the table omits
  - Notes: the table today tells a reader that Redis supports both APIs natively while the
    corpus offers nowhere to configure it — phase 6 closed that half. **Cite the anchor**:
    requirements §11 cites `ReactorAndProactor.md:284` deliberately, knows it will rot, and a
    draft of that very sentence invented an anchor — `#transport-support-matrix` — that has
    never existed.

- [ ] **Task 10.2:** `HandlerFailure.md` — `#transport-nack-behavior`
  - Input: the anchor; the same survey run
  - Output: rows for **GCP Pub/Sub, RocketMQ, PostgreSQL and MSSQL** — this table omits four,
    two more than the other
  - Notes: the corpus **asserts nack semantics** for transports it offers no configuration
    page for. That is the precise shape of the defect: not silence, but a claim with no
    landing place.

- [ ] **Task 10.3:** Walk AC7, and verify phase 10
  - Input: both corrected tables; the survey output at the ref
  - Output: a recorded diff showing the two tables and the shipped transport set agree; the six
    gates; `optioncheck`
  - Notes: **`optioncheck` does not check these tables** — they are comparison tables, not
    option tables, and they get no marker. AC7's tool is the survey, and this is the one
    criterion in the spec whose evidence is a diff rather than an exit code.

---

## Phase 11 — Acceptance and close (Docs PR)

**Goal:** AC1–AC9 walked with evidence, recorded in this file, and the spec closed.

**This phase must not:** treat the walk as bookkeeping. **009's Phase 12 was not bookkeeping:
AC7 was the one criterion with no tool behind it, and walking it found it unmet** — on pages
green under all six gates, with the obvious repair wrong in three of eleven places. **AC8 is
this spec's equivalent**, which is why every phase above walks it; phase 11 is where that is
confirmed rather than where it starts.

- [ ] **Task 11.1:** Walk AC1, AC6, AC6b and AC8 page by page, with evidence
  - Input: every page 012 touched; the traceability table below
  - Output: a § *The acceptance pass as executed* section in this file, naming the evidence
    per criterion
  - Notes: **AC1 is walked page by page and there is no tool for it** — `optioncheck` proves a
    table is *correct*, never that a surface *has* one. The scope line's type count against
    design §7's mapping is the nearest mechanical check, and it is not the same claim.

- [ ] **Task 11.2:** Re-run the six gates and `optioncheck`, and record the closing numbers
  - Input: §2.8's starting numbers
  - Output: the six figures at the closing commit, with the deltas explained — link, pagelint,
    shape and `--verify` each **+10**, `--check-redirects` unmoved, `versioncheck.py`
    unmoved; `optioncheck`'s scope line in full
  - Notes: **`--check-redirects` not moving is a result, not an omission** — §2.1's
    *"re-ordering inside a section moves no URL"*, asserted rather than assumed for a sixth
    time. **Predict every new path with `urlmap.py` before probing it**, and check
    `sitemap-pages.xml` lists it so the probe is not premature.

- [ ] **Task 11.3:** Compile the drift ledger — what the checker caught, and what it did not
  - Input: standing obligation 10's per-phase records, from task 2.6 onward
  - Output: a § *What the checker caught* section in this file: every mismatch, as page,
    option, **what the corpus said** and **what the assembly says**; the `manual:` residue by
    name from task 8.5; and the count of surfaces documented for the first time
  - Notes: **This is the only evidence 012 produces that the drift was real.** Every other
    criterion proves the corpus is correct *now* — AC2 is an exit code, and an exit code has no
    memory. Without this section the spec can say the tables are right and cannot say they were
    ever wrong, which is the claim #67 was opened about and the one task 11.4 needs to make.
    **Three things are already known to belong in it before the work starts**: design §3.4's
    two stale cross-cutting tables, `AzureBlobConfiguration.md`'s wrong-product opening
    sentence as the precedent for *green under every check and wrong since 2023*, and whatever
    task 2.6 finds on day two. **Record the misses too** — a `manual:` declaration is a row
    nobody verified, and a ledger that lists only successes is an advertisement.

- [ ] **Task 11.4:** Re-check [#67](https://github.com/BrighterCommand/Docs/issues/67) and
      comment
  - Input: the issue thread; **task 11.3's ledger**; what 012 actually shipped
  - Output: a comment naming the prose configuration reference as delivered, **citing the
    ledger's headline figure rather than the number of tables written**, and **013 as the
    remaining half** — #67's comparison to Microsoft Learn's API browser names two products and
    012 is only the second of them
  - Notes: **Re-check immediately before commenting.** 009 recorded *"none is reported"* a day
    early twice and re-checking costs one command. The generated API reference belongs in the
    Brighter repository, not here, and the comment should say so rather than leave it implied.
    **#67 stays open** — 013 still owes it the PostgreSQL-for-both-transport-and-outbox guide.

- [ ] **Task 11.5:** Re-derive the totals, tick the README, and close
  - Input: this file
  - Output: `grep -c '^- \[x\] \*\*Task'` and the phase table's Tasks column, **re-derived and
    agreeing**; `README.md` § Status Checklist ticked; `spec/.current-spec` repointed to 013
  - Notes: **Re-derive, never increment.** If the walk adds a task — 009's did, at 12.4 — both
    figures move by measurement, and the phase table is the one that gets forgotten: 009's two
    counts disagreed by one until the table was corrected too, which is the whole reason for
    keeping two.

---

## Traceability

| AC | Walked by | Evidence |
|---|---|---|
| AC1 | 11.1, and phases 3–9 as they land | Page by page against design §7's mapping. **No tool** |
| AC2 | every verification task; **11.3** | `optioncheck` exit 0, with a scope line that is not empty — and the ledger of what it caught before it went green, which the exit code cannot carry |
| AC3 | **2.7** | A fixture with a wrong constructor-parameter row, exit 1 |
| AC3b | **1.2, 2.8** | `EmptyChannelDelay` — the probe measures it, the red-proof proves the tool catches it |
| AC4 | **2.10** | The job in `docs.yml`, push and PR, unguarded, **no schedule** (struck §13.2) |
| AC5 | **2.9** | Exit 2 with the package source removed |
| AC6 | **6.8** | Ten transports, ten pages; no orphan, no pagelint error |
| AC6b | **7.2, 7.4** | Firestore with a table, Spanner without — and the same pair one family over |
| AC7 | **10.3** | The two tables diffed against `survey.py` at the ref, **after 1.5 fixes it** |
| AC8 | **3.4, 5.7, 6.8, 7.4, 8.5, 9.3**, confirmed at 11.1 | No tool, by design. Walked in every phase that writes a table |
| AC9 | every verification task; **11.2** | The six gates of §2.8 |

| Deliverable | Phase | Tasks |
|---|---|---|
| D1 `tools/optioncheck` | 2 | 2.1–2.5 |
| D2 CI job | 2 | 2.10 |
| D3 `RELEASE_CHECKLIST.md` | 2 | 2.11 |
| D4 `CommandProcessorConfigurationReference.md` | 3 | 3.1, 3.2 |
| D5 `DispatcherConfigurationReference.md` | 3 | 3.3 |
| D6 five documented transports | 5 | 5.1–5.6 |
| D7 outbox and inbox pages | 8 | 8.1, 8.2 |
| D8 lock pages and the normalisation | 8 | 8.3, 8.4 |
| D9 scheduler pages | 9 | 9.1, 9.2 |
| D10 the two cross-cutting tables | 10 | 10.1, 10.2 |
| D11 `survey.py` | 1 | 1.5 |
| D12 five new transport pages | 6 | 6.1–6.5 |
| D13 Firestore and Spanner outboxes | 7 | 7.1, 7.2 |
| D14 `SUMMARY.md` — ten entries | 4, 6, 7 | 4.2, 6.6, 7.4 |
| D15 the relational reference page | 4 | 4.1 |
| D16 Firestore and Spanner inboxes | 7 | 7.3 |

**Sixteen deliverable rows are not sixteen artefacts, and the row count is a count of rows.**
Derived from the tasks above rather than from the table: **ten pages created**, **32 existing
pages under `contents/` edited** — 2 core, 6 documented transports, 8 outbox/inbox/sweeper, 7
locks, 6 schedulers, 2 cross-cutting and `AsyncAPISupport.md` — plus one tool created and five
files edited outside `contents/` (`survey.py`, `docs.yml`, `RELEASE_CHECKLIST.md`,
`SUMMARY.md`, and `pagetypes.tsv` appended to). Requirements §9 says the same about its own table,
and 009's PROMPT block on deliverable counts is the precedent — *the row count is the authority
for rows, and it is not a count of anything else.*

**Re-derive that 32 rather than quoting it**; it is the sum of six task groups and it will move
if a phase does. `grep -oE 'contents/[A-Za-z0-9]+\.md' tasks.md | sort -u` is the check, minus
the ten creations and the pages named only as link targets.
