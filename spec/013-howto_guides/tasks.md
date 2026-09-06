# Spec 013: Task-Oriented How-To Guides — Tasks

**Created:** 2026-09-06 · **Status:** **APPROVED 2026-09-06** — reviewed, two findings applied
(§2.3's backstop family, and §1's ref clause on line numbers), marker `.tasks-approved` on disk
**Works from:** `design.md` (APPROVED 2026-09-06, `.design-approved`; Q5, Q6 and Q7 ruled, Q3
answered by measurement, Q4 deferred) and `requirements.md` (APPROVED 2026-09-04,
`.requirements-approved`, `af38910`, amended by #147/#148 and again by design §2.3.1)

**Total tasks: 43, across 5 phases.** Re-derived, not counted by hand:
`grep -c '^- \[.\] \*\*Task' tasks.md` says 43, and the phase table's Tasks column sums to 43
independently. **Keep both, and re-derive both after any edit** — 009's D-table spent three
sessions wrong because a count was edited beside the row it counted, and 012's own total
disagreed with its phase table by one for exactly as long as it took someone to run the second
command.

**The phases are design §10's five, unchanged.** This document cuts them into tasks; it does
not re-sequence them and it re-opens nothing. Where it records something the design does not
say, it is in §2, and it is a fact about the corpus measured on 2026-09-06 — never a verdict.

> **This list deviates from `/spec:tasks`'s prescribed phase structure, deliberately.** The
> command proposes *Research & Preparation → Core Documentation → Supporting Documentation →
> Polish & Review*. The approved design sequences by **deliverable**, because P0-2 and P0-4 are
> repairs to published pages that must land before anything new is written on top of them, and
> because one PR per phase is the contract 009, 010 and 012 all ran under. **`CLAUDE.md` and an
> approved design win over a command**, per `PROMPT.md`'s standing note; the deviation is
> recorded in §5 as more of spec 014's evidence rather than worked around silently.

---

## 1. How this list is organised

**One phase is one pull request** — a coherent unit, merged before the next branch starts.

| Phase / PR | Goal | Tasks | Deliverables |
|---:|---|---:|---|
| **1** | **P0-2 and P0-4 — the dead-API repairs.** Seventeen dead call sites and three further defects in eight blocks across six pages; ten dead type names in prose across six more | 13 | P0-2, P0-4 |
| **2** | **P0-1 — `PostgreSQLTransportAndOutbox.md`**, the guide #67 is owed | 11 | P0-1 |
| **3** | **P0-3 — `HandlingPoisonMessages.md`** | 7 | P0-3 |
| **4** | **P1-1 and P1-2** — `HandlingLargeMessages.md`, `MSSQLTransportInboxAndOutbox.md` | 8 | P1-1, P1-2 |
| **5** | **Acceptance** — AC1–AC10 walked with evidence, AC9 **backwards**, the defect ledger, and close | 4 | — |

### Dependencies

Stated as gates rather than drawn.

- **1 gates 2 and 4, and the reason is not tidiness.** `PostgresOutbox.md` is P0-1's stated
  prerequisite and `MSSQLOutbox.md` is P1-2's, and both name a type that has never existed
  (design §2.4b). **A guide cannot honestly link a page that does this**, so P0-4 lands first
  or the guides link a lie.
- **1 gates nothing else.** P0-2's six pages are not linked by any new guide; it goes first
  because nothing new should be written on top of an API surface the corpus spells wrongly
  (design §10), not because a later phase reads it.
- **2 gates 4.** P1-2's value is showing design §2.5's composition generalises, which is only
  demonstrable once there is a pattern — so it deliberately shares P0-1's shape, and
  divergence between the two is a defect rather than variety.
- **3 is independent of 2 and 4.** If a phase stalls, take the next one.
- **5 depends on all four**, and AC9 is walked **backwards** — every requirements §3.1 cluster
  with two or more askings, against the delivered set. Forwards can only ever find guides that
  were written.

**Phases 2, 3 and 4 are individually shippable and individually abandonable.** Each adds a page
the tree does not yet reference. **Phase 1 is not optional at any point**: it repairs published
pages that tell readers to call methods the product has never had.

### The standing obligations — every task owes all ten

Do not restate these per task; they are assumed by all of them.

1. **Every API name a page prints is verified at BOTH refs — `10.7.0` and Brighter
   `origin/master` — with a control.** Requirements §11.4 and §11.6. There are **three**
   states, not two: **live** (at the release), **forthcoming** (only on `origin/master`, and
   then the page says so with `> **Not in a released package yet.**`), and **dead or invented**
   — and **only the third is a defect**. Use `git grep -w`. **A zero is also what a broken grep
   returns**, so every sweep proves it can still find something already read with your own eyes
   before its total is read.
2. **Enumerate a replacement family; never extrapolate it.** The eight P0-4 replacements were
   first derived by pattern and one of them —
   `MsSqlEntityFrameworkTransactionProvider` — **does not exist**; the real name keeps `Core`.
   Four of five siblings following a convention is exactly what makes the fifth invisible.
   **Check the names you are writing in with the same control you used on the names you are
   deleting.**
3. **Compile every code block, and know what compiling proves.** 009's harness: one project per
   block, **`<ImplicitUsings>disable</ImplicitUsings>`**, the packages
   `tools/optioncheck/optioncheck.csproj` pins, and **do not add
   `Microsoft.Extensions.Hosting` — that is `NU1605`.** It proves the *type* is satisfied and
   says nothing about **a downcast, a null check, or a default that selects a code path**.
   **Wherever an example is downstream of a base type or an interface, mirror the source
   repository's own test rather than composing one** (design §7's three cases).
4. **Link, never copy.** Option values come from 012's tables by link. A copied table is drift
   with a checker that cannot see it: `optioncheck` binds a marker to a type, and a second
   unmarked copy is invisible to it. **No guide restates
   `RelationalDatabaseConfigurationReference.md`'s table.**
5. **Every new page carries the full convention set**: front matter with a quoted
   `description:` equal to the opening sentence with markdown stripped and
   `layout.description.visible: false`; one H1 on line 1's block; the banner as the first
   non-blank line after it; `## Step N: …` headings per Q7; *Further Reading* unqualified; a
   language tag on every fence. **The opening sentence is ≤200 characters rendered, ends in
   terminal punctuation, not a colon, and is unique across the corpus.**
6. **A new page gets its `SUMMARY.md` entry in the same commit that creates it** — never
   before (`linkcheck.py` MISSING FILE) and never after (the orphan check has no exemptions).
   **Append a row to `spec/011-authoring_conventions/pagetypes.tsv`**, `verdict` = `How-to`,
   `applies` = `Brighter V10`. **Append; never re-sort.** No tool reads that file except
   `apply_banners.py`, so a missing row is invisible to every green build and silently skips
   the page at the next version bump.
7. **Rule 6 turns strict on any code block the diff touches, and phase 1 gives that up on
   purpose.** Eight blocks become strict, so each earns **real** `using` directives — not a
   `// ...`, which declares an omission rather than fixing it. **The warning count is 779 at
   `7062bf9` and must FALL.** On the new pages the directives come by construction: a new page
   is 100% added lines and every block on it is strict.
8. **Every `using Paramore.*` line is checked against `git grep "namespace X" 10.7.0 -- src/`,
   with a control.** Requirements §11.7, standing since 012 phase 9 found
   `Paramore.Brighter.InMemoryScheduler`, a namespace that exists nowhere. **A block that
   passes rule 6 has directives; it does not have the right ones**, and only a compiler tells
   them apart.
9. **Run the seven gates after every page and read the whole output.** `git add` first —
   `git diff` cannot see an untracked file, so a brand-new page contributes no strict ranges
   until it is staged and a vacuous pass is indistinguishable from a real one. **Read the scope
   line, not the verdict**: `pagelint --changed` prints `N code block(s) strict`, and that
   figure is what says whether the run meant anything.
10. **Ask which product before editing, and record every defect before fixing it.**
    `.AddPolicies(` and `.AddHandlersFromAssemblies(` are **real in Darker 4.1.1**, which
    versions independently — a Brighter-shaped sweep condemns two good pages. And task 5.3
    aggregates the ledger: **a defect fixed silently is a defect that never existed**, which is
    the only evidence this spec produces that the corpus was ever wrong.

### Two conventions this document holds itself to

- **Every count in a task body is a claim about the corpus**, re-derived when the task runs
  rather than inherited from here. 009 learned this from *"the two places"* that turned out to
  be three, and 013's own requirements said *five pages* three times over a six-row table.
- **No task quotes a figure without the command that produces it.** A total with no ref is not
  a fact — **and a source line number is that same shape.** Every `<file>:<line>` into
  `../Brighter` in this document means **`@ 10.7.0`**, the pinned ref, and two of them have
  already drifted on `origin/master`: `OutboxProducerMediator.cs:502` is the
  `InvalidOperationException` throw at `10.7.0` and is `if (HasAsyncOutbox())` on master, and
  the DI factory's `??=` is `ServiceCollectionExtensions.cs:705` at `10.7.0` and `:724` on
  master. **The name is checked at both refs; the line number is only ever true at one.**

---

## 2. What this list settles — re-derived 2026-09-06 at `7062bf9`

The design measured at `89195d8`. Everything since is `spec/` and `CLAUDE.md`, so `contents/`
has not moved — but that is a claim, and these are the checks rather than the assumption.
**Design §5's line numbers are current to the line.**

### 2.1 Every count the design asserts reproduces, with its control

| Claim | Command | Result |
|---|---|---|
| P0-2: 10 known-dead sites | `grep -rnE '\.(ResiliencePipelines\|ConfigureResiliencePipelines\|Policies)\(' contents/ \| wc -l` | **10** ✓ |
| P0-2: **6** pages, not the requirements' five | same with `grep -rlE` | **6** ✓ |
| P0-2: the seven extra sites | `CommandProcessorBuilder.With(` 5, `DispatchBuilder.With(` 1, `.Subscribers(` 1 | **7** ✓ → **17 total** |
| The real API is on zero pages | `grep -rn '\.Resilience(\|DefaultResilience\|AddBrighterDefault' contents/` | **0, 0, 0** ✓ — the defect, and the tell |
| P0-4: 10 sites, 6 pages | the eight dead names, `grep -rnE` / `-rlE` | **10 / 6** ✓ |
| P0-4 control: the live NoSQL twins | `grep -rnE 'DynamoDbUnitOfWork\|MongoDbUnitOfWork' contents/` | **13** — the sweep works |
| The eight replacements are live | `git grep -w -l <name> <ref> -- src/`, both refs | **1 each** ✓ |
| …and the pattern-derived wrong one is not | `MsSqlEntityFrameworkTransactionProvider` | **0 at both refs** — design §11 Q6's near-miss reproduces |
| The four new filenames do not collide | `ls contents/`, with `ClaimCheck.md` as control | **0, 0, 0, 0** against control **1** ✓ |
| P0-3's two linked anchors exist | resolved through `linkcheck.py`'s own `slug()` | `#transport-nack-behavior` **FOUND**, `#native-vs-brighter-managed-dlq` **FOUND** ✓ |
| Every `SUMMARY.md` line the §6 diffs attach to | `grep -n` on the nine entries | all present, in the expected order ✓ |

### 2.2 One addition: `InputChannelFactory` is on TWO sites, and the design accounts for one

`grep -rn 'InputChannelFactory' contents/` returns **2**, both on
`HowConfiguringTheDispatcherWorks.md`:

- **`:71`**, inside the code block design §2.3 rewrites — accounted for.
- **`:48`**, in **prose**, four lines above that block: *"We pass the **Dispatcher** an
  instances of **InputChannelFactory**…"*

The type is **0 files at both refs across the whole repository**, against a control of **18**
declaring `ChannelFactory`. **The prose site survives the block rewrite**, so a phase that
repairs only the fence leaves the page still naming a type that has never existed — in a
sentence, which is where the claims that rot are made. This is P0-4's exact shape arriving on a
P0-2 page, and it was found only because the site count was re-derived rather than inherited.

**Task 1.8 owns it**, and the same sentence needs a second repair anyway: `CLAUDE.md`'s
terminology rule says a type goes in `backticks`, not **bold**.

### 2.3 A second addition: the backstop family is SIX, and the one the guide needs is on zero pages

Task 3.3 first said *"the three `…OnErrorAttribute` backstops"*. Enumerated rather than
extrapolated — `git grep -lE 'class [A-Za-z]*OnError[A-Za-z]*Attribute' 10.7.0 -- src/` — the
family is **six**, three sync and three async, every one live at both refs:

| Attribute | Pages naming it today |
|---|---:|
| `RejectMessageOnErrorAttribute` | 3 |
| `DeferMessageOnErrorAttribute` | 1 |
| `DontAckOnErrorAttribute` | 1 |
| `RejectMessageOnErrorAsyncAttribute` | 1 |
| **`DeferMessageOnErrorAsyncAttribute`** | **0** |
| `DontAckOnErrorAsyncAttribute` | 1 |

**The zero is the tell, and it is the same tell as `AddBrighterDefault` and
`UseExternalLuggageStore`** — the member a reader most needs is the member the corpus never
names. It lands squarely on P0-3's own gotcha: `Subscription<T>` defaults to
**`MessagePumpType.Proactor`** (`Subscription.cs:291` @ `10.7.0`), so the reader this guide is
written for — MSSQL, Redis or MQTT, where a nack discards and `DeferMessageAction` is the only
safe choice — is on the **async** pump by default and needs
`DeferMessageOnErrorAsyncAttribute`, which is documented nowhere.

**Deriving "three" from the sync family is standing obligation 2's exact failure mode**,
committed in the document that states obligation 2 — the same shape as design §11 Q6's
`MsSqlEntityFrameworkTransactionProvider`. **Task 3.3 owns it.**

### 2.4 What was checked and is NOT a defect

- **`RmqMessageConsumerFactory(` on five other pages is correct** — each passes a connection.
  Only `HowConfiguringTheDispatcherWorks.md:62` passes `logger`. The five are the control that
  the sweep discriminates rather than condemning a spelling.
- **`.AddPolicies(` on `QueryPipelinePolicies.md` and `DarkerBasicConfiguration.md` is real**,
  in Darker 4.1.1. **Do not sweep it.**
- **`IUnitOfWork` at `DispatchingARequest.md:153` returns 0 at both refs and is not a defect** —
  it illustrates the reader's *own* repository interface (design §2.4).

---

## Phase 1 — P0-2 and P0-4, the dead-API repairs

**Goal:** no published page tells a reader to call a method or name a type the product has
never had. **Thirteen tasks. One PR.**

**This phase expects NO gate to move except pagelint's warning count**, which is exactly when a
vacuous pass is invisible. **Read `--changed`'s scope line — expect `8 code block(s) strict`.**

- [ ] **Task 1.1:** Re-derive P0-2's site table before editing anything
  - Input: design §2.3, §2.3.1, §5; the commands in §2.1 above
  - Output: a confirmed list of 17 sites / 6 pages / 8 blocks, with the current line numbers
  - Notes: **Run the control in the same breath** — `.Resilience(`, `DefaultResilience` and
    `AddBrighterDefault` must return **0 in `contents/`** and non-zero in `src/`. A sweep that
    cannot find the live API is not evidence about the dead one.

- [ ] **Task 1.2:** Repair `PolicyRetryAndCircuitBreaker.md` — 2 blocks, 4 sites
  - Input: design §5's rows for `:351`, `:355`, `:356`, `:372`; §2.1's declarations
  - Output: `CommandProcessorBuilder.StartNew()`; `.Policies(policyRegistry)` **folded into**
    `.Resilience(registry, policyRegistry)`; `.ConfigureResiliencePipelines(…)` at `:372`
    becomes `options.ResiliencePipelineRegistry = …` inside `AddBrighter`
  - Notes: **`.Policies(` folds, it does not map.** V9 had two calls where V10 has one with an
    optional second parameter, so a site-for-site substitution prints two calls where one
    belongs. Both blocks earn real `using` directives.

- [ ] **Task 1.3:** Repair `MigratingToPollyV8.md` — 2 blocks, 5 sites, and the shape error
  - Input: design §5's rows for `:99`, `:101`, `:109`, `:111`, `:112`; and `:116`
  - Output: both `With()` → `StartNew()`; both `.Policies(` folded;
    `.ResiliencePipelines(registry)` → `.Resilience(registry, policyRegistry)`
  - Notes: **This is the worst site in P0-2 and the ✅ is part of the defect.** `:112` presents
    the non-existent method as the ✅ *"New: Polly v8 pipelines"* form, and `:116` then tells
    the reader to call **both** methods — so the page is wrong about the **shape**, not the
    spelling, on the migration page, for the highest-demand topic in the corpus. **Remove the
    ✅ marker**: it was marking a method that never existed, which is the one case in the corpus
    where the version convention actively endorsed an invention.

- [ ] **Task 1.4:** Repair `CommandProcessorConfigurationReference.md:104` — 1 block, 1 site
  - Input: design §5; §2.1's `BrighterOptions.ResiliencePipelineRegistry` (a **settable
    property**, `BrighterOptions.cs:59`)
  - Output: `.ConfigureResiliencePipelines(…)` → `options.ResiliencePipelineRegistry = …`
  - Notes: **There is no fluent DI method**, which is why the invented one was so easy to
    write. Say so in a sentence; the next person to reach for a fluent call is the reader.

- [ ] **Task 1.5:** Repair `CQRSWithBrighterAndDarker.md:378` — 1 block, 1 site
  - Input: design §5
  - Output: as task 1.4
  - Notes: **Check which product before editing.** This page covers both, and Darker's
    `.AddPolicies(` is real. The edit is to the Brighter half only, and the page's banner
    (`Brighter and Darker V10`) is the reminder.

- [ ] **Task 1.6:** Repair `HowConfiguringTheCommandProcessorWorks.md` — 1 block, 2 sites
  - Input: design §5's rows for `:234`, `:236`
  - Output: `With()` → `StartNew()`; `.Policies(policyRegistry)` →
    `.Resilience(registry, policyRegistry)`
  - Notes: this is the one site where `.Policies(` maps rather than folds, because there is no
    adjacent `.ResiliencePipelines(` to fold into. **The two cases look identical in a grep and
    are not** — read the block.

- [ ] **Task 1.7:** Rewrite `HowConfiguringTheDispatcherWorks.md:54-74` from the source's own test
  - Input: `tests/Paramore.Brighter.RMQ.Sync.Tests/MessageDispatch/When_building_a_dispatcher.cs`;
    design §2.3's four defects
  - Output: a block with `DispatchBuilder.StartNew()`, `.Subscriptions(`, a real
    `ChannelFactory`, `new RmqMessageConsumerFactory(connection)` and a **four-argument**
    `.MessageMappers(` — sync registry, async registry, transformer factory, async transformer
    factory
  - Notes: **This block is rewritten, not substituted.** Passing nulls for the async half
    compiles and then throws at `Receive()`, because `Subscription<T>` defaults to `Proactor`
    (`Subscription.cs:291`). That is Brighter#4302's lesson: **compiling proves the type is
    satisfied and nothing about a default that selects a code path.** Mirror the test; do not
    compose.

- [ ] **Task 1.8:** Repair `HowConfiguringTheDispatcherWorks.md:48`'s prose — §2.2's addition
  - Input: §2.2 above; `CLAUDE.md`'s terminology rule
  - Output: the sentence names `ChannelFactory` in backticks, not **InputChannelFactory** in
    bold
  - Notes: **Not in the design, found by re-deriving the count.** The prose site survives a
    fence-only repair, and it is four lines above the block that gets it right — P0-4's shape
    on a P0-2 page. Grep the page for `InputChannelFactory` afterwards and expect **0**.

- [ ] **Task 1.9:** Add what is on zero pages today — `AddBrighterDefault` and the `??=` trap
  - Input: design §2.1; `ResiliencePipelineRegistryExtensions.cs:57`;
    `CommandProcessorBuilder.cs:144-148`, `:171`; the DI factory at `:705`
  - Output: every edited block gains `.AddBrighterDefault()`, plus prose covering **(a)** that
    supplying your own registry without it fails at **startup** — `Resilience()` throws
    `ConfigurationException` on its first statement — and **(b)** that `AddBrighterDefault`
    uses `TryAddBuilder`, so it **backfills**: a reader adds their own pipelines, calls it, and
    loses nothing
  - Notes: this is the highest-demand deliverable in the spec — 5 independent askings, and a
    reader lost four hours to it in Brighter#3960, answered in the end by another user. Also
    add **`UseResiliencePipelineAsync`**, which is on 2 pages and neither is the resilience
    how-to.

- [ ] **Task 1.10:** P0-4 — repair the ten dead relational type names across six pages
  - Input: design §11 Q6's **verified replacement table**; §2.1's control run
  - Output: `PostgresOutbox.md`, `MSSQLOutbox.md`, `MySQLOutbox.md`, `SqliteOutbox.md`,
    `BrighterBasicConfiguration.md`, `CommandProcessorConfigurationReference.md` — each dead
    name replaced in prose
  - Notes: **Use the table; do not re-derive the names by pattern.** MSSQL's EF provider is
    `MsSqlEntityFrameworkCoreTransactionProvider` — it keeps `Core` where its three siblings
    drop it, and the pattern-derived spelling returns **0 at both refs**. **The NoSQL stores
    genuinely have a unit of work**, so `DynamoDbUnitOfWork` and `MongoDbUnitOfWork` stay and
    are the control. Prefer a prose fix that does not touch the adjacent code block — rule 6
    placement, standing obligation 7.

- [ ] **Task 1.11:** Compile all eight repaired blocks
  - Input: 009's harness; `tools/optioncheck/optioncheck.csproj`'s pinned packages
  - Output: 8/8 building, 0 errors, 0 warnings, with `<ImplicitUsings>disable</ImplicitUsings>`
  - Notes: **Do not add `Microsoft.Extensions.Hosting` — `NU1605`.** Check every
    `using Paramore.*` against `git grep "namespace X" 10.7.0 -- src/` with a control
    (obligation 8): a block passing rule 6 has directives, not necessarily the right ones.
    **And record what compiling did not prove** — task 1.7's block is the case in point.

- [ ] **Task 1.12:** Run the seven gates and assert the phase moved what it predicted
  - Input: design §9's phase-1 row
  - Output: link **160** unmoved, pagelint pages **158** unmoved, shape unmoved, redirects
    unmoved, versioncheck unmoved, optioncheck unmoved — and **warnings BELOW 779**
  - Notes: **`git add` first**, then read `--changed`'s scope line and assert it says **8 code
    block(s) strict**. A phase that edits eight blocks and moves the warning count by nothing
    has not edited what it thought it did. **This is the dangerous phase precisely because it
    expects nothing to move.**

- [ ] **Task 1.13:** Record every defect found, then open the PR
  - Output: a ledger entry per site — page, line, what the corpus said, what the assembly says
  - Notes: feeds task 5.3. **A defect fixed silently is a defect that never existed.** Ask for
    the merge **and the head-ref deletion by name in the same breath**; this repository does
    not auto-delete a merged head ref.

---

## Phase 2 — P0-1, `contents/PostgreSQLTransportAndOutbox.md`

**Goal:** the guide Docs#67 is owed. **Eleven tasks. One PR.** ~330 lines, How-to, nested under
`PostgreSQLMessageBroker.md`.

- [ ] **Task 2.1:** Ask the Q4 question, now that the PR exists
  - Input: design §11 Q4
  - Output: a ruling on whether P0-1 gets a compiled sample in `../Brighter/samples/`
  - Notes: **Deferred by agreement, not open.** A write to `../Brighter` is authorised **per
    PR**, so it is asked here and not before. If granted: branch from `origin/master` in a
    **worktree**, register the project in `Brighter.slnx` in the same PR, never `git add -A`,
    and expect `build` to be a **coin-flip** (Brighter#4276) — re-run the job rather than
    pushing an empty commit.

- [ ] **Task 2.2:** Write the front matter, H1, banner and opening sentence
  - Input: design §4.1
  - Output: quoted `description:` with `layout.description.visible: false`; H1 *Use PostgreSQL
    for Both Transport and Outbox*; the banner naming both prerequisites
  - Notes: the opening sentence is **156 characters rendered** as drafted — re-measure
    **rendered, not as typed**, and assert it is unique across the corpus before writing it.
    `pagelint.py --fix` will write the front matter *from* the sentence; it refuses if the
    sentence fails rule 7, which is the check.

- [ ] **Task 2.3:** Steps 1–2 — packages and the two tables' DDL
  - Input: `PostgresOutbox.md` §NuGet re-pinned; `PostgreSQLMessageBroker.md:39`;
    `PostgresOutbox.md:64`
  - Output: `## Step 1: Install the Packages`, `## Step 2: Create the Queue and Outbox Tables`
  - Notes: pin versions against `10.7.0`. **`versioncheck.py` scans only `TUTORIAL_PAGES`**, so
    a pin here is checked by nothing — grep the tools for this filename before assuming either
    way (a checker's inclusion list is where its unstated obligations live).

- [ ] **Task 2.4:** Step 3 — one `RelationalDatabaseConfiguration`, three tables
  - Input: design §2.5; `src/Paramore.Brighter/RelationalDatabaseConfiguration.cs:21`
  - Output: `## Step 3: Describe Both Tables in One Configuration`
  - Notes: **This is the pivot the whole guide turns on** — `queueStoreTable`,
    `outBoxTableName` and `inboxTableName` are three parameters on **one** object, so the
    reader does not reconcile two configurations. Exactly one page in the corpus names both of
    the first two. **Link `RelationalDatabaseConfigurationReference.md` for the option table;
    never restate it.**

- [ ] **Task 2.5:** Step 4 — register `IAmARelationalDatabaseConfiguration`, and say why
  - Input: `PostgresOutbox.md:116`; Brighter #3721 / #3755 (closed, *not a bug*) and #4279
  - Output: `## Step 4: Register the Configuration`
  - Notes: **the #3721 trap.** `TransactionProvider` is a **`Type`**, activated by the
    container, so the configuration must be registered separately — omit it and the host
    starts, provisions the Outbox, ticks the Sweeper, and only the first
    `GetRequiredService<IAmACommandProcessor>()` throws, naming a type the reader's code never
    mentions. Mirror **009 rung 3's sample**, per obligation 3.

- [ ] **Task 2.6:** Steps 5–6 — producer, Outbox and consumer
  - Input: design §7 examples 5 and 6; `PostgreSQLMessageBroker.md:145`
  - Output: `## Step 5: Wire the Producer and the Outbox`, `## Step 6: Wire the Consumer`
  - Notes: **`AddConsumers` extends `IServiceCollection`; `AddProducers` extends the
    `IBrighterBuilder` it returns.** So a consumer registration comes **first** and everything
    else chains off it — `services.AddBrighter().AddProducers(…).AddConsumers(…)` is
    **`CS1929`**, and eleven blocks across eight pages get this wrong today.

- [ ] **Task 2.7:** Steps 7–8 — deposit/commit/clear, and the Sweeper
  - Input: `PostgreSQLMessageBroker.md:362` made runnable; `PostgresOutbox.md:169`
  - Output: `## Step 7: Deposit and Clear Inside Your Transaction`, `## Step 8: Run the Outbox
    Sweeper`
  - Notes: the package that carries `UseOutboxSweeper` is
    **`Paramore.Brighter.Outbox.Hosting`** — 009's design omitted it from a step list and the
    omission was invisible until the sample was built. Link `PostgresDistributedLock.md` for
    multi-instance sweepers.

- [ ] **Task 2.8:** Step 9 — the verification step, measured on a real run
  - Input: design §7 example 9
  - Output: `## Step 9: Verify It Worked` — verification SQL and the **expected log lines**
  - Notes: **AC7, and it is the criterion with no tool behind it.** 009's AC7 was found unmet
    at the close on pages green under every gate, and its repair asserted a build state that
    was **false three times of eleven**. **When a page makes a factual claim about the reader's
    machine, the claim needs a measurement, not a diagnosis.** Run it.

- [ ] **Task 2.9:** The failures section — the two exceptions, by their text
  - Input: design §4.1's two named failures
  - Output: `## PostgreSQL Transport and Outbox Failures`
  - Notes: print the **exception text a reader will have searched for** —
    `InvalidOperationException: No Async outbox defined.` (`OutboxProducerMediator.cs:502`,
    from `OutboxSweeper.SweepAsync`, Q&A #3795 verbatim, resolved by setting
    `ConnectionProvider` and `TransactionProvider` on `AddProducers`) and the missing
    `IAmARelationalDatabaseConfiguration` registration from task 2.5.

- [ ] **Task 2.10:** `SUMMARY.md`, `pagetypes.tsv`, *Further Reading*, and compile
  - Input: design §6's `## Transports` diff
  - Output: the nested entry under `PostgreSQLMessageBroker.md`; a `pagetypes.tsv` row
    appended; every block compiled
  - Notes: **the `SUMMARY.md` entry text is the `/llms.txt` title, not the H1** — the entry is
    *PostgreSQL for Transport and Outbox*, deliberately shorter than the H1. Entry and page in
    the **same commit**.

- [ ] **Task 2.11:** Gates, and assert the four that move
  - Input: design §9's four-new-pages row, taken one page at a time
  - Output: link 160 → **161**, pagelint 158 → **159**, shape 157 → **158** with **widest
    unmoved at 12 of 20**, redirects **unmoved**, optioncheck **unmoved**; `--verify` after
    publication
  - Notes: **a nested page does not move the widest and does not move redirects** — held eight
    times, and asserted rather than assumed each time. Predicted URL:
    `transports/postgresqlmessagebroker/postgresqltransportandoutbox`. **Do not probe it until
    `sitemap-pages.xml` moves**; a premature probe can cache a 404, and the discriminator is
    `curl <path>.md`.

---

## Phase 3 — P0-3, `contents/HandlingPoisonMessages.md`

**Goal:** the route a reader takes to get a poison message off the channel. **Seven tasks. One
PR.** ~300 lines, How-to, nested under `HandlerFailure.md`.

**Per design §2.2 this page repairs nothing.** Q3's premise was stale: 012's phase 10 fixed
`HandlerFailure.md`'s nack table at `05ab80c`, three days before the requirements described the
defects as open. **The guide's contribution is the route**, which is on no page — the two
existing pages carry semantics and options and never have the reader do it.

- [ ] **Task 3.1:** Front matter, H1, banner, opening sentence
  - Input: design §4.2
  - Output: H1 *Handle a Poison Message and Route It to a Dead Letter Queue*; banner naming
    `HandlerFailure.md` and `ErrorHandlingOptions.md`
  - Notes: 150 characters rendered as drafted — re-measure and assert uniqueness.

- [ ] **Task 3.2:** Steps 1–2 — confirm the diagnosis, then choose the action
  - Input: `HandlerFailure.md#transport-nack-behavior` (**verified to resolve**, §2.1)
  - Output: `## Step 1: Confirm You Have a Poison Message`, `## Step 2: Choose Between Requeue,
    Reject and Don't Acknowledge`
  - Notes: **the gotcha the guide must state, because a reader cannot infer it:** on MSSQL,
    Redis and MQTT a nack **discards** the message, so `DontAckAction` loses it and
    `DeferMessageAction` is the only safe choice. `HandlerFailure.md:276` says this — **point at
    it, do not repeat it.**

- [ ] **Task 3.3:** Steps 3–4 — requeue count, DLQ routing key, backstop attribute
  - Input: `ErrorHandlingOptions.md:122`; `HandlerFailure.md:217`
  - Output: `## Step 3: Set a Requeue Count and a Dead Letter Routing Key`, `## Step 4: Add a
    Backstop Attribute`
  - Notes: named APIs, **all verified live at both refs**: `RejectMessageAction`,
    `DeferMessageAction`, `DontAckAction`, `InvalidMessageAction`, the **six**
    `…OnErrorAttribute` backstops (§2.3), `RequeueCount`, `RequeueDelay`,
    `UnacceptableMessageLimit`, `UnacceptableMessageLimitWindow`, `DontAckDelay`,
    `DeadLetterNamingConvention`, `InvalidMessageNamingConvention`,
    `IUseBrighterDeadLetterSupport`. **Re-verify at the ref before printing them** — the
    programme's most-repeated defect is a plausible name assembled from surrounding vocabulary.
    **Six, not three** — the three `…OnErrorAsyncAttribute` twins are live and
    `DeferMessageOnErrorAsyncAttribute` is on **0 of 158 pages**. Print the async twin beside
    each sync one and say which pump each belongs to: `Subscription<T>` defaults to
    **`Proactor`** (`Subscription.cs:291` @ `10.7.0`), so the async attribute is what the
    default reader needs, and it is the one this corpus has never named.

- [ ] **Task 3.4:** Step 5 — the verification step
  - Output: `## Step 5: Verify the Message Reaches the Dead Letter Queue`
  - Notes: AC7. **Never read a broker's counters until the connection you removed is gone** — a
    stopped consumer whose connection has not been reaped reads `0`, which is
    indistinguishable from a lost message and is the opposite of what you are verifying.

- [ ] **Task 3.5:** Steps 6–7 — enrichment headers, replay or discard
  - Output: `## Step 6: Read the Enrichment Headers`, `## Step 7: Decide Whether to Replay or
    Discard`
  - Notes: link `ReplayOnSeen.md` rather than restating it.

- [ ] **Task 3.6:** The per-transport section — **linked, never copied**
  - Output: `## Poison Message Handling on Your Transport`
  - Notes: links `HandlerFailure.md#transport-nack-behavior` and
    `ErrorHandlingOptions.md#native-vs-brighter-managed-dlq` **by anchor**. Requirements §8 puts
    copying either table out of scope, and a copied table is drift no checker can see. Both
    anchors were resolved through `linkcheck.py`'s own `slug()` in §2.1 — **resolve them again
    after writing, because the check is cheap and an invented anchor is invisible to review.**

- [ ] **Task 3.7:** `SUMMARY.md`, `pagetypes.tsv`, compile, gates
  - Output: nested entry under `HandlerFailure.md`; link 161 → **162**, pagelint 159 → **160**,
    shape 158 → **159**, redirects and optioncheck **unmoved**
  - Notes: predicted URL `using-an-external-bus/handlerfailure/handlingpoisonmessages`.

---

## Phase 4 — P1-1 and P1-2

**Goal:** the claim-check recipe, and the proof that design §2.5's composition generalises. **Eight
tasks. One PR.**

- [ ] **Task 4.1:** `HandlingLargeMessages.md` — front matter, H1, banner, opening sentence
  - Input: design §4.3
  - Output: H1 *Put a Large Payload Behind a Claim Check*; prerequisites `ClaimCheck.md` and
    `MessageMappers.md`

- [ ] **Task 4.2:** Steps 1–2 — the size limit, and the six luggage stores
  - Input: `git grep -l 'IAmAStorageProviderAsync' 10.7.0 -- src/`
  - Output: `## Step 1: Find Your Transport's Message Size Limit`, `## Step 2: Choose a Luggage
    Store`
  - Notes: **`ClaimCheck.md` lists one store where six ship** — `S3LuggageStore` (and its `.V4`
    twin), `AzureBlobLuggageStore`, `GcsLuggageStore`, `MongoDbLuggageStore`,
    `FileSystemStorageProvider`, `InMemoryStorageProvider`, plus `NullLuggageStore`. **Enumerate
    them with the command, do not extrapolate the family** (obligation 2). Its list also ends in
    an **unclosed bold** — `**IAmAStorageProviderAsync:` — which is how long it has been since
    anyone read the bottom of that page.

- [ ] **Task 4.3:** Step 3 — register the luggage store. **This is why the page exists**
  - Input: `ServiceCollectionExtensions.cs:951`, `:971`, `:992`
  - Output: `## Step 3: Register the Luggage Store`
  - Notes: **`UseExternalLuggageStore<TStoreProvider>` is on 0 of 157 pages**, against a control
    of 13 pages mentioning `ClaimCheck` — the identical tell as `AddBrighterDefault`. A reader
    following `ClaimCheck.md` attaches the attribute and gets **no store**. Written from the
    type, three overloads.

- [ ] **Task 4.4:** Steps 4–6, the failures section, and `ClaimCheck.md`'s pointer
  - Input: `ClaimCheck.md:25`, `:38`; `MessageTransforms.md`'s ruling
  - Output: `## Step 4: Attach the Claim Check to Your Mapper`, `## Step 5: Choose a
    Threshold`, `## Step 6: Verify the Payload Went to the Store`, `## Claim Check Failures`;
    and a pointer **added to `ClaimCheck.md`** at the six-store table
  - Notes: **`MessageTransforms.md`'s ruling binds this page** — a transform *of your own*
    needs a custom mapper to attach it to, because you cannot put an attribute on a type you do
    not own. The default `JsonMessageMapper<TRequest>` already carries `[CloudEvents(0)]`, so
    "default mappers do not run transforms" is **false** and must not be written.

- [ ] **Task 4.5:** `MSSQLTransportInboxAndOutbox.md` — front matter, H1, banner, opening sentence
  - Input: design §4.4
  - Output: H1 naming transport, Inbox and Outbox together; prerequisites `MSSQLMessageBroker.md`
    and `MSSQLOutbox.md`
  - Notes: **`MSSQLOutbox.md` must already be repaired by task 1.10** — it is a prerequisite
    this guide links, and it named a type that has never existed.

- [ ] **Task 4.6:** The MSSQL steps, mirroring P0-1 with an Inbox step inserted after step 5
  - Input: design §4.4; `MSSQLMessageBroker.md:107`
  - Output: the step sequence, and *Further Reading* pointing back at P0-1 so the two read as
    one pattern
  - Notes: **divergence from P0-1's shape is a defect here, not variety** — the value of this
    page is that the pattern generalises.

- [ ] **Task 4.7:** The `MsSqlSubscription` caveat — load-bearing, from Brighter#4302
  - Input: `MessagingGateway.MsSql/ChannelFactory.cs:46`, `:65`, `:88`;
    `MSSQLMessageBroker.md:142`
  - Output: every subscription typed `MsSqlSubscription<T>`, **with the reason stated**
  - Notes: `ChannelFactory` **downcasts** `Subscription` to `MsSqlSubscription` and throws
    `ConfigurationException` when the cast fails. **A `Subscription<T>` compiles perfectly and
    dies at `dispatcher.Receive()`** — strictly worse than a compile error, because the page
    looks authoritative right up to the throw. Mirror the MsSql gateway tests (obligation 3).

- [ ] **Task 4.8:** Both `SUMMARY.md` entries, both `pagetypes.tsv` rows, compile, gates
  - Output: link 162 → **164**, pagelint 160 → **162**, shape 159 → **161** with widest still
    **12 of 20**, redirects and optioncheck **unmoved**; `--verify` **161/161** after publication
  - Notes: two nested pages in one PR — **assert the widest and the redirect count individually
    rather than as a pair.**

---

## Phase 5 — Acceptance

**Goal:** walk AC1–AC10 with evidence, and find what the phases did not. **Four tasks. One PR.**

- [ ] **Task 5.1:** Walk AC1–AC8 and AC10 forwards, with evidence per criterion
  - Input: requirements §12
  - Output: one paragraph per criterion, each naming the command and its output
  - Notes: **the criteria with no tool behind them are where the defects are** — 009's AC7 and
    012's AC1 were both found unmet at the close, and both were the criterion nothing checked.
    AC10 is API liveness, and its rule has **three** states: only *dead or invented* is a defect.

- [ ] **Task 5.2:** Walk **AC9 backwards** — the delivered set against the demand census
  - Input: requirements §3.1's clusters, every one with two or more askings
  - Output: a cluster-by-cluster table, marked delivered / not delivered / deliberately deferred
  - Notes: **This is the task most likely to find something, and that is why it exists.** A
    mapping walked forwards can only ever find pages that were written; 012's AC1 failure took
    one command to find backwards, at the last possible moment, after eleven phases had been
    written from a mapping with a hole in it.

- [ ] **Task 5.3:** Publish the defect ledger
  - Input: tasks 1.13 and every phase's recorded findings
  - Output: a ledger — page, line, what the corpus said, what the product says, which task
    fixed it
  - Notes: **the only evidence this spec produces that the corpus was ever wrong.** Twenty
    P0-2 defects, ten P0-4 names, plus §2.2's addition and whatever the phases turn up. A
    defect fixed silently is a defect that never existed.

- [ ] **Task 5.4:** Close the spec, and settle Docs#67
  - Output: `README.md` boxes ticked, the board updated, and a comment on
    [Docs#67](https://github.com/BrighterCommand/Docs/issues/67)
  - Notes: **#67 stays open until P0-1 lands, and P0-1 is phase 2** — so this is where it can
    finally be closed, if the maintainer agrees. **Write the record AFTER the act**: the
    comment publishes under the maintainer's account, *"let's merge"* does not authorise a
    public comment, and 012's `tasks.md` shipped at `9aef400` claiming a comment had been
    posted while it sat unposted for three days. **No tool in this programme can ask whether
    something happened on GitHub.**

---

## 3. Acceptance criteria — where each is met

| AC | Met by |
|---|---|
| AC1 four guides exist, correct type and banner | tasks 2.2, 3.1, 4.1, 4.5; walked at 5.1 |
| AC2 every guide traces to demonstrated demand | design §4's *Traces to* rows; walked **backwards** at 5.2 |
| AC3 no guide copies a reference table | tasks 2.4, 3.6; obligation 4 |
| AC4 `SUMMARY.md` nesting, no new section | tasks 2.10, 3.7, 4.8 |
| AC5 every code block compiles | tasks 1.11, 2.10, 3.7, 4.8; obligation 3 |
| AC6 conventions — banner, headings, front matter, opening sentence | obligation 5; `pagelint.py` at every phase |
| AC7 every guide ends with a verification step | tasks 2.8, 3.4, 4.4; **walked in the phase that writes it**, not at the close |
| AC8 every guide names the failure a reader meets | tasks 2.9, 3.2, 4.4, 4.7 |
| AC9 demand census walked **backwards** | task 5.2 |
| AC10 API liveness — three states, only the third a defect | obligation 1; tasks 1.1, 3.3; walked at 5.1 |

---

## 4. What this list does not do

- **It does not schedule the V9 `.UseXxxOutbox(` family.** Ten sites across six pages,
  **recorded, not scheduled**, per Q6's ruling as proposed. None is on a page this spec links,
  and each needs a genuine V10 rewrite rather than a substitution. **If the intent was to
  absorb both families, P0-4 doubles to 20 sites across 12 pages** — that is the one line to
  correct, and it changes phase 1's size and nothing else.
- **It does not touch the 53 existing How-to pages' headings.** Q7 binds pages written from
  here on; requalifying published anchors buys nothing a reader can see.
- **It does not re-open Q1, Q2, Q3, Q5, Q6 or Q7.** All are settled. **Q4 is asked at task
  2.1** and nowhere earlier.

---

## 5. Workflow friction — more of spec 014's evidence

Recorded as met, per 014's README instruction to record rather than route around. **New, beyond
requirements §14's four and design §13's four.**

9. **`/spec:tasks` prescribes a phase structure that contradicts the approved design it is
   told to work from.** The command's *"Typical phases for documentation: Research &
   Preparation → Core Documentation → Supporting Documentation → Polish & Review"* is a
   reasonable default and is wrong here: 013's phases are **deliverable-shaped**, because P0-2
   and P0-4 are repairs that must precede the writing, and because one PR per phase is the
   contract every spec in this programme has run under. **A command that proposes a structure
   without saying it is a default invites a task list that fights its own design.**
10. **No command asks a task list to re-derive the counts it inherits.** §2 above exists because
    `PROMPT.md` says to, and it found one: `InputChannelFactory` is on **two** sites where the
    approved design accounts for one, and the second is prose that survives the repair the
    design specifies. **The design was not careless** — it mapped sites to enclosing blocks,
    which is the right instrument for a rule-6 budget and the wrong one for finding a claim
    made in a sentence. Three consecutive phases of this spec have now found a stale or
    incomplete inherited count; the pattern is strong enough to be a step in the command.
