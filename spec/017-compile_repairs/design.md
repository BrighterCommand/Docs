# Spec 017: Compile Repairs — Design

**Created:** 2026-09-26
**Status:** **APPROVED 2026-09-26** — `.design-approved`. D1–D3 ruled by the maintainer; D4 and Q5 stand as working assumptions

> Every figure here was measured 2026-09-26 at Docs `master` `c7329bb`, against Brighter `10.7.0` and
> Darker `4.1.1`, with the command beside it. **Five experiments ran before this design was written**
> (§ *The experiments*); each has a method, a two-way control and its output, and the probe that ran
> it is committed under `spec/017-compile_repairs/probe/`.

> **What the experiments changed, in one paragraph.** The requirements' P0-7 second clause targets
> **zero** blocks: every `[UsePolicy(` on the four named pages decorates a **sync** `Handle`. The
> shape 016 actually asked to be surveyed — a sync attribute on an async handler — exists on **5
> pages, 6 blocks**, and **running** it shows Brighter throws `ConfigurationException` at send.
> Growing the pin **alone moves no block**. Stubs are real work: of 441 blocks that need one, 95
> build with an empty class, 183 need members, 120 need a typed value, and 43 name a type another
> block on the same page declares — which no stub may supply. The target falls out of those
> numbers: **BUILT ≥ 250**, from two tranches of pages.

## Design Subject

**Process** — declared in `requirements.md` § *Subject*. The deliverable is instruments, scaffold
and repaired blocks on existing pages. **No page is created.**

| Section | Status |
|---|---|
| **SUMMARY.md changes** | **N/A** — a category error on this spec. No page is added, moved, nested or retitled, so no URL moves and no redirect is owed |
| **The outline, file by file** — page type, banner, opening sentence, `##` headings | **N/A for new pages: there are none.** Repaired pages keep their banner, type, opening sentence and headings; a repair edits code blocks and the prose that describes them. A repair that changes an opening sentence re-runs `pagelint --fix` for its `description:` — § *Page Repair Rules*, rule 6 |
| **Code examples, API verified live** | **Applies** — every API this design names is resolved in § *API Resolved For This Design* |
| **Gate prediction** | **Applies** — § *Gate Movement Predicted* |

## Requirements Re-Verified

Each P0/P1 item re-read against what the experiments found. **Approved text is not rewritten**;
where a requirement's words no longer match the corpus, the mismatch is recorded here as a fact and
the change is put to the maintainer (§ *Open Questions For The Design Review*).

| Req | Said | Measured | Consequence for the design |
|---|---|---|---|
| **P0-1** `--classify` | a committed classifier; settle 016's same-page 21 against the probe's 7 by reading | a seeded sample (`random.seed(17)`), **12 of the 113** *page-type* blocks, each missing type searched for a declaration anywhere on its page, in any fence: **12 of 12 are page-type**; none is declared on the page | **The probe's rule stands.** 016's 21 is not reproduced and not reproducible — its method was never committed. `--classify` implements the probe's rule |
| **P0-1** | classes: import / context / other | the *other* row (334) is two things: **179 blocks do not parse at all** (`blockcheck --parse`), and the rest are binder errors that are not a missing name | `--classify` splits **`parse`** out of *other*, because a block that does not parse needs a different repair (§ *Page Repair Rules*) |
| **P0-1** | a type dump | a missing **extension method** reports `CS1061`, not a missing name, so a type-only dump classifies `services.AddBrighter(…)` without its `using` as a defect. The dump now holds **21,365 types and 3,471 extension methods** | `--classify` reads both (§ *The Classify Mode*) |
| **P0-3** stubs | ~400 blocks wait on scaffold | E3: of **441** STUB blocks, **95** build with an empty class, **183** need members, **120** need a *typed* value, **43** name a same-page type | the 43 are **never stubbed** (016 obligation 8). They stay FAILED, listed with that reason, and the page says the step builds on the one before |
| **P0-6** page defects | shapes `CS1061`, `CS0117`, `CS1739`… | E1 after `using`s: **187** DEFECT blocks. Some are defects, some are *wrapper* artefacts — a method shown without its class, so `base.HandleAsync` binds to `object` (`CS0117`, 18 instances of `HandleAsync`) | a DEFECT is **verified against source before it is called one**; a wrapper artefact is a PARSE-class repair (make the block whole), not an API repair |
| **P0-7** clause 2 | `[UsePolicy(` on async handlers on `ReactorAndProactor.md`, `HowConfiguringTheCommandProcessorWorks.md`, `PolicyFallback.md`, `ImplementingExternalBus.md` | **all five occurrences decorate a sync `Handle`** (`grep -n -A3 '\[UsePolicy(' contents/<page>.md`). 016 tasks line 1881 recorded the shape as **"not surveyed"**; the resume prompt shortened that to "on async handlers" | **0 blocks for the clause as written.** The survey 016 asked for, run by `probe/attr_mismatch.py`: **6 defects on 5 pages**, plus 1 deliberate. Put to the maintainer as **D1** |
| **P0-7** clause 1 | `ITimerProvider` → `TimeProvider` | `grep -rn ITimerProvider contents/` → **4** lines, all `InMemoryScheduler.md` (32, 50, 202, 205); 0 files in Brighter `10.7.0` `src` | unchanged |
| **P1-1** the pin | Hangfire.AspNetCore, Jaeger, Quartz DI | E2: **28 of 94** Brighter `src/` projects at `10.7.0` are unpinned, **20** of them named on a page. The candidate set alone moves **0** verdicts; with `using`s, **+5 BUILT** and **15** DEFECT → STUB | **Grow the pin, but it is not what unblocks the scheduler pages.** The AWS V4 family **cannot share the pin** (below) |
| **Q3** target | BUILT ≥ 250 as a working assumption | § *Target And Tranches* | **BUILT ≥ 250 confirmed as reachable**, and set |
| **Q6** `PageContext.cs` | cut to `connectionString` | `--show contents/DapperOutbox.md 1` names `connectionString` and none of the nine types | unchanged; the unit rule (P0-2) will fail it until cut |

**The AWS V4 constraint, measured.** `Paramore.Brighter.MessagingGateway.AWSSQS` 10.7.0 depends on
`AWSSDK.SQS` **`[3.7.500.5, 4.0.0)`**; its `.V4` twin depends on **`4.0.100.7`**
(`curl -sL https://api.nuget.org/v3-flatcontainer/<id>/10.7.0/<id>.nuspec`). One project restores
one `AWSSDK.SQS`, so **the V4 packages cannot join `refs.csproj`**. The 7 V4 packages named on pages
stay unjudged; a second pin is **D3**.

## The Experiments

All scratch; none changes the tree. Re-run from the Docs root after the two builds in
`tools/README.md` row 9:

```bash
bash spec/017-compile_repairs/probe/run.sh $TMPDIR/probe                 # E0: classify, usings
python3 spec/017-compile_repairs/probe/pages.py $TMPDIR/probe $TMPDIR/probe/r.tsv   # E1
REFS=<scratch pin>/refs.txt python3 spec/017-compile_repairs/probe/stubs.py <dir>   # E3
python3 spec/017-compile_repairs/probe/attr_mismatch.py [--plant]         # E4, instrument
dotnet run --project spec/017-compile_repairs/probe/attrrun               # E4, behaviour
```

### E1 — every failing block given its `using`s, sorted by page

`pages.py` gives **all 872** FAILED blocks (not only *import*) one `using` per missing pinned type
**and per missing extension method**, recompiles, repeats to a fixpoint, then sorts each block.

| Verdict | Blocks (current pin) | (E2 pin) | Means |
|---|---:|---:|---|
| **BUILT** | 64 | **69** | a `using` on the page is the whole repair |
| **STUB** | 431 | **441** | fails only on names no pinned package ships |
| **PARSE** | 175 | **175** | does not parse as staged |
| **DEFECT** | 202 | **187** | a binder error that is not a missing name |

**Controls, both directions, run 2026-09-26:**

- **Must be found BUILT:** `AWSSQSConfiguration_1` (BUILT, no scaffold) with its 7 `using` lines
  deleted and its row flipped to FAILED → **BUILT, with exactly the 7 namespaces it lost** restored.
  That is also the evidence that the probe's namespace ranking chooses what an author writes
- **Must be found DEFECT:** `Telemetry_1` with `RequestContext` → `RecordRequestContext` → **DEFECT**
- *A first positive control was wrong and is recorded because it was:* `DapperOutbox_1` with its
  `using`s stripped came back STUB, correctly — the strip took its injected `using static
  PageContext;`, which supplies `connectionString`

### E2 — the pin grown by the candidates

A scratch copy of `refs.csproj` with 20 packages added — Hangfire.AspNetCore / SqlServer /
PostgreSql / Redis.StackExchange / MySqlStorage / MemoryStorage, Quartz.Extensions.DependencyInjection
and .Hosting at **3.18.1** (what `Paramore.Brighter.MessageScheduler.Quartz` 10.7.0 depends on),
Hangfire at **1.8.24** (likewise), EF Core **9.0.15** (the floor `Paramore.Brighter.MsSql.EntityFrameworkCore`
10.7.0 sets — 9.0.9 fails restore with `NU1605`), the four Brighter EF packages, the three
Validation packages, `OpenTelemetry.Exporter.Jaeger` 1.5.1, xunit, Moq. **534 assemblies against
501.**

| | Result |
|---|---|
| Unmodified corpus, old pin vs new | **0 verdicts change** in either direction — the control: nothing that builds stops |
| With E1's `using`s | **+5 BUILT; 15 DEFECT → STUB**; one page moves from *has a hard block* to *stubs only* |
| **Phase 1's pin**: E2 less Jaeger, plus the five Brighter packages E5 and the `src/` listing found | **538 assemblies; 0 verdicts change** against the current pin — the prediction phase 1 carries, measured before it is built |

### E3 — mechanical stubs on every STUB block

`stubs.py` gives each STUB block an empty class per missing type (with its generic arity) and a
`dynamic` per missing value, **except** names another block on the same page declares.

| Outcome | Blocks | Means for the real stub |
|---|---:|---|
| **BUILT** | 95 | an empty class suffices |
| **MEMBERS** | 183 | every remaining error names a stubbed type: the stub needs those members |
| **HIDDEN** | 120 | errors not naming a stub — **159 of them `CS1977`**, *"cannot use a lambda as an argument to a dynamically dispatched operation"*: an artefact of `dynamic`. These need the value **typed**, as 016's units already do. Beyond that artefact: `CS9176` 35 (also `dynamic`), `CS0155` 4, `CS0411` 4, `CS1061` 2 |
| **SAME-PAGE** | 43 | not stubbed, by rule |

### E4 — sync attribute on async handler: found, and run

**The instrument** (`probe/attr_mismatch.py`) reads blocks through `pagelint.Page` and reports a
handler attribute whose sync/async form does not match the method under it. The 13 paired names are
read from Brighter 10.7.0 (`git grep -h -E 'class [A-Za-z]+Attribute *: *[A-Za-z]*Attribute' 10.7.0 -- src`).
**Red-proof:** `--plant` — a sync-on-async plant must hit, an async-on-sync plant must hit, a
matched pair must not → **OK**. **Its first run over-reported**: 6 of 13 hits were Darker's
`[FallbackPolicy]` on `ExecuteAsync`, and Darker 4.1.1 has **no async twin**
(`FallbackPolicyAttribute QueryHandlerAttribute QueryLoggingAttribute RetryableQueryAttribute`). It
now matches Brighter's `HandleAsync` only.

| Hit | Verdict by reading |
|---|---|
| `HowServiceActivatorWorks.md:366` `UseInbox` | **defect** → `UseInboxAsync` |
| `PipelineValidation.md:250` `RejectMessageOnError` | **deliberate** — the page's *Before (error)* example, labelled `// wrong` |
| `PipelineValidation.md:280` `UseResiliencePipeline` | **defect**, in a *Before (warning)* example about step order, not about sync/async |
| `PipelineValidation.md:289` `UseResiliencePipeline` | **defect**, in the *After (fixed)* example; also names an argument before a positional one |
| `PolicyRetryAndCircuitBreaker.md:326` | **defect** |
| `ReactorAndProactor.md:190` | **defect** — under *Proactor Middleware/Attributes*, commented `// Async resilience pipeline` |
| `V10MigrationGuide.md:282` | **defect** — under *Use the new attribute* |

**The behaviour, run with a control** (`probe/attrrun`, against released `Paramore.Brighter` and
`Paramore.Brighter.Extensions.DependencyInjection` 10.7.0):

```text
control: UseResiliencePipelineAsync on HandleAsync
  handler ran
  -> completed
case:    UseResiliencePipeline on HandleAsync
  -> ConfigurationException: All handlers in an async pipeline must derive from IHandleRequestsAsync.
     You cannot have a mixed pipeline by including handler ResilienceExceptionPolicyHandler`1
```

**The compiler accepts every one of these attributes; only running them fails.** All six blocks are
FAILED today for other reasons — `HowServiceActivatorWorks.md` #16, `PipelineValidation.md` #9 and
#10, `PolicyRetryAndCircuitBreaker.md` #14, `ReactorAndProactor.md` #6, `V10MigrationGuide.md` #10,
each on `CS0246` and kin in `--report`. **So a tranche repair that supplies their `using`s and stubs
turns them BUILT and writes them into the baseline while they still throw.** The compile gate would
then certify them. That is why § *Page Repair Rules* rule 8 runs the instrument in every tranche
PR, not only on these pages.

### E5 — the page-type sample

Twelve *page-type* blocks, `random.seed(17)`: **12 of 12** name a type declared nowhere on the page.
Two named things that are not domain types, and the classifier must not call them stubs:
`BrighterServiceActivatorHealthCheck` is real, in the **unpinned**
`Paramore.Brighter.ServiceActivator.Extensions.Diagnostics`; `GetOrCreateRequestSchedulerId` is a
**property** of `AwsSchedulerFactory`, named inside an object-initializer fragment.

## API Resolved For This Design

| Name | State at 10.7.0 / 4.1.1 | Command |
|---|---|---|
| `ITimerProvider` | **dead** — 0 files | `git -C ../Brighter grep -c ITimerProvider 10.7.0 -- src` |
| `InMemorySchedulerFactory.TimeProvider` | **live**, `= TimeProvider.System` | `git -C ../Brighter grep -n TimeProvider 10.7.0 -- src/Paramore.Brighter/InMemorySchedulerFactory.cs` |
| `UseResiliencePipelineAsyncAttribute(string policy, int step)` | **live** | `git -C ../Brighter show 10.7.0:src/Paramore.Brighter/Policies/Attributes/UseResiliencePipelineAsyncAttribute.cs` |
| `UsePolicy`, `UsePolicyAsync` | **live, `[Obsolete]`** — for `UseResiliencePipeline(Async)` | same directory, `grep Obsolete` |
| `UseInboxAsync`, `RejectMessageOnErrorAsync` | **live** | the attribute listing in E4 |
| `IRequest.CorrelationId` (`Id?`) | **live** — the `CS0535` blocks that omit it are defects | `git -C ../Brighter grep -n CorrelationId 10.7.0 -- src/Paramore.Brighter/IRequest.cs` |
| `ResiliencePipelineRegistry<string>.AddBrighterDefault()` | **live**, namespace `Paramore.Brighter.Extensions` | E4's control needed it |
| Darker `FallbackPolicyAttribute` | **live**, no async twin | `git -C ../Darker grep … 4.1.1 -- src` |

## The Classify Mode

`python3 tools/blockcheck.py --classify [file]` — P0-1.

```text
tools/blockcheck.py --classify
  ├─ stage the corpus (as --report)                         existing
  ├─ Program.cs  <stage> <refs.txt>                         existing: verdicts
  ├─ Program.cs --parse <stage>                             existing: which blocks do not parse
  ├─ Program.cs --explain <stage> <refs.txt> <FAILED ids>   existing: diagnostics
  ├─ Program.cs --types <refs.txt>                          NEW: type<TAB>Name<TAB>Ns, ext<TAB>Method<TAB>Ns
  └─ classify each FAILED block, first rule that matches:
        parse       the block does not parse
        import      a missing name is a pinned type, or a CS1061 names a pinned extension method
        other       any diagnostic that is not a missing name
        same-page   every missing name is a value or a type another block on the page declares,
                    and at least one is the latter
        values      every missing name is lower-case or _
        page-type   the rest
```

- **Output:** `page<TAB>ordinal<TAB>class<TAB>names`, one terminated row per FAILED block, on stdout;
  a summary line of counts on stderr. Rows sorted by page, then ordinal, so two runs `diff` clean
- **`--types` is computed at run time** (Q1): `System.Reflection.Metadata` over `refs.txt`, as
  `probe/typedump` does. It took seconds in E0, so no generated file is committed to go stale
- **Exit 2** when the tool is unbuilt, when `refs.txt` is missing or its stamp is stale (the check
  `--report` already makes), or when there are no FAILED blocks to classify. **Exit 0** otherwise:
  a classification is data, like `--list`
- **What it must not claim.** It classifies by the *first* errors the compiler reports. A class is
  what blocks the block today, not everything that will. E1 showed 226 *import* blocks become STUB
  after their `using`s

`--explain` becomes a Python mode in the same change (P1-2), and its stderr summary is corrected:
it now reads *"N blocks explained"* rather than a build summary over the whole corpus.

## The Unit Rule, Enforced

P0-2. Checked on **every `--report` run**, so CI's `blocks` job enforces it; a violation is
**exit 1**, printed before the verdicts as `SCAFFOLD RULE: <unit>: <what>`.

A unit mapped (by `pages.tsv`) to pages **P** passes when:

1. **Every top-level type it declares is named by a BUILT block on P** — a token match over the
   block's text, via `--identifiers`
2. **No block on P declares a type of that name** — the obligation-8 leak, by construction
3. **Every member it declares — field, property, method — is named by a BUILT block on P**
4. **It declares no `global using`** — a unit's plain `using`s are file-scoped and cannot reach a
   block; `global using` would (AC7)

**Rule 1 relaxes `pages.tsv`'s "no domain type"** for exactly the stubs P0-3 allows, and the file's
comment is rewritten to state rules 1–4 in place of the old sentence. **Red-proof, both ways:** a
plant unit declaring a type no block names → exit 1; one declaring a type a block on its page
declares → exit 1; one declaring an unnamed member → exit 1; the real scaffold after phase 1 → exit 0.
**Measured against today's 14 units, by a dry run of rules 1–3** (`--identifiers` over
`units/*.cs`, tokens from the BUILT blocks on each unit's mapped pages): **13 pass; `PageContext.cs`
fails 26 times** — nine types and seventeen members no block names. That is the positive control
that the check reads the real tree, and the other thirteen are its negative. `PageContext.cs` is
cut to `connectionString` in the same PR (Q6), so phase 1 exits 0.

## The List Skips Mode

`python3 tools/blockcheck.py --list-skips` — P0-8. One row per skip,
`page<TAB>ordinal<TAB>reason`, terminated; **exit 2 on an empty enumeration**, as every listing mode
does. It reads `scan_skips` — the code `--report` already uses — so the two cannot disagree. AC12:
`wc -l` equals `--report`'s SKIPPED.

The other two instruments are corrections to what `tools/README.md` tells a reader to run:
`find tools/blockcheck -name '*.csproj' -not -path '*/obj/*'` for 016's AC3, and
`grep -vcE '^(\./)?spec/'` for its AC10, each written into § *Reading a number before you trust it*
with the reason the old form fails.

## Scaffold Stub Rules

Stubs live in `tools/blockcheck/scaffold/units/<Page>Context.cs`, one unit per page unless two pages
share a world (as `RelationalTransportContext` already serves two). A stub:

1. **Declares only the members the page's blocks use**, with the types those uses imply — never a
   member no block names (enforced: rule 3)
2. **Types a value from a pinned package or the BCL** — never `dynamic`. E3's 120 HIDDEN blocks are
   what `dynamic` costs
3. **Never supplies a type another block on the page declares** (enforced: rule 2). Those blocks
   stay FAILED and are listed
4. **Never supplies a namespace.** A missing `using` is a page repair (P0-4)
5. **Carries a one-line comment naming the page line that names it**, so a reader of the unit can
   check it against the page

## Page Repair Rules

Every FAILED block on a tranche page leaves the tranche in one of four states: **BUILT** (with its
baseline row), **SKIPPED** with an accepted reason, **FAILED and listed** in `tasks.md` with its
diagnostic and why it stays, or **retagged** because it was never C#.

| `--classify` says | Repair | Where |
|---|---|---|
| `import` | the `using`s a reader needs, **in the block** | page |
| `values`, `page-type` | a stub per § *Scaffold Stub Rules* | unit |
| `same-page` | none: **FAILED, listed**. If the page does not already say the step builds on an earlier one, one sentence saying so | page prose |
| `parse` — literal `...` placeholder (66 of 179) | rewrite as `// ...`, which is what `CLAUDE.md` § *Complete code blocks* asks for, and complete the statement it sat in | page |
| `parse` — a fragment: a signature, a chain beginning `.AddX(`, an argument list | make it whole if the page's reader needs the whole; otherwise **SKIPPED** with one of the accepted reasons below | page |
| `parse` — not code at all (a list of names in a C# fence) | retag the fence `text` | page |
| `other` — verified wrong against 10.7.0 / 4.1.1 source | repair; **grep for the same falsehood on every page**; one row in the defect ledger | page |
| `other` — a wrapper artefact (a method shown without its class) | treat as a fragment | page |

**Accepted skip reasons** — the only ones a tranche PR may add without a maintainer ruling:
*"V9 form, shown beside its V10 replacement"* (016's), *"a method signature shown for comparison,
with no body or class to compile in"* (016's), and *"a fragment of the configuration shown whole in
block N"*, where block N is BUILT. Anything else is put to the maintainer in the PR.

**Also, on every tranche page:**

6. A repair that changes a page's **opening sentence** re-runs `python3 tools/pagelint.py --fix
   <page>`, so its `description:` follows (`CLAUDE.md` § *Page descriptions*)
7. A block asserting behaviour is **run, with a control**, and the run recorded in the PR (P0-10)
8. **`probe/attr_mismatch.py` runs in every tranche PR, and its count may only fall.** A block a
   tranche makes BUILT is checked for an attribute of the wrong kind before its baseline row is
   written — E4's six are FAILED today and would otherwise enter the baseline broken. The six are
   repaired on whichever phase holds their page
9. **A tutorial block stays in step with the sample it cites.** `TutorialFirstCommand.md` cites
   `samples/CommandProcessor/HelloWorld`; adding a `using` does not diverge from it, but a change to
   the code does. Where a repair would diverge, the PR stops and asks — sample writes are per-PR
   (`CLAUDE.md` § *Key Constraints*)

## Target And Tranches

From E1 and E3, pages ranked by **hard** blocks — PARSE plus DEFECT, the ones needing a judgement
rather than a `using` or a stub:

| Pages with ≤ N hard blocks | Pages | …with no same-page block | `using` / empty stub | stub with members or typed values | same-page (stay FAILED) | hard |
|---|---:|---:|---:|---:|---:|---:|
| **0** | **35** | 26 | 38 | 50 | 21 | **0** |
| **1** | **75** | 65 | 64 | 96 | 23 | **40** |
| 2 | 95 | 84 | 79 | 145 | 24 | 80 |

```bash
# E1 and E3 against the E2 pin, into one work dir, then:
python3 spec/017-compile_repairs/probe/tranches.py <work-dir>
```

**Tranche 1 — the 35 pages with no hard block.** 88 blocks reachable by `using`s and stubs; 21
tutorial blocks stay FAILED as same-page. Largest by FAILED blocks: `RequestValidation.md` (14, of
which 8 reachable), `BoxProvisioningConfiguration.md` (9), `TickerQScheduler.md`,
`ErrorHandlingOptions.md`, `QueryResultTypes.md` (6 each), `AnalyzerSupport.md` (5); the full list
is `awk -F'\t' '$6+$7==0' $TMPDIR/probe/pages.tsv` over E1's output. **The page lists the phases
work from are `tasks.md` § *The tranches***, fixed against the committed pin, where `Order` from
`StackExchange.Redis` is read as the page type it is and the tranches are 37 / 38.

**Tranche 2 — the 40 pages with exactly one hard block.** 72 more reachable blocks and 40 judgements.

**The target, set:** **BUILT ≥ 250** at the close — 101 + tranche 1's 88 + tranche 2's 72 is 261 if
every stub works, and E3 says most need members, so 250 leaves room for honest failure. **Pages with
nothing BUILT: 97 → ≤ 60**, measured by the same `awk` as § *Current state* of the requirements.
Of the 97, **43** are in the two tranches with at least one reachable block, and **25** of those
reach one by a `using` or an empty stub alone (the E1 × E3 join, restricted to pages with no BUILT
block in `r.tsv`). 97 − 43 = 54, so **the second target needs 37 of those 43 pages** — reachable,
not slack. It is the target to watch at each tranche's close.
**Tranche 3 (pages with two hard blocks) is P2** — it opens 018 if 017 closes with room.

## Phases

Each phase is **one PR** (`tools/README.md` § *One phase is one pull request*).

```text
Phase 1  instruments + pin          no page changes
  ├─ Program.cs --types; blockcheck.py --classify, --list-skips, --explain
  ├─ the unit rule in --report, with its four plants
  ├─ PageContext.cs cut to connectionString (Q6)
  ├─ refs.csproj: E2's packages less Jaeger (D2), plus five unpinned Brighter packages pages
  │    name — ServiceActivator.Extensions.Diagnostics, Testing, ServiceActivator.Control,
  │    ServiceActivator.Control.Api, AsyncAPI.NJsonSchema. Not the AWS V4 family (D3); not the
  │    two Analyzer packages, which are analyzers, not compile references
  └─ tools/README.md: row 9, § The other modes, the AC3 and AC10 commands
Phase 2  tranche 1a                 the first ~18 of the 35, by section
Phase 3  tranche 1b                 the rest of the 35
Phase 4  tranche 2a                 ~20 of the 40
Phase 5  tranche 2b                 the rest, + ITimerProvider and the E4 defects not already on a tranche page
Phase 6  close                      acceptance walk, ledgers, the residual sentence for 018
```

**Why tranche 1 is split:** 35 pages is 35 page reviews, and the site-change sign-off is per PR.
The split is by `SUMMARY.md` section, so a reviewer reads related pages together.

## Gate Movement Predicted

`tools/README.md` owns the figures; these are directions and causes.

| Gate | Phase 1 | Phases 2–5 | Phase 6 |
|---|---|---|---|
| 1 `linkcheck` | **none** — `tools/README.md` is already in its walk; no file added under `tools/` is `.md` | **none** | none |
| 2 `pagelint` | **none** | **warnings fall** as blocks gain `using`s; **errors stay 0**; could **rise** only if a split creates a block without `using`s — explained if so | none |
| 3 shape, 4 redirects, 7 `--verify` | **none** — no `SUMMARY.md` change in any phase | **none** | none |
| 5 `versioncheck` | **none** — it reads the version strings on its five `TUTORIAL_PAGES`, not `refs.csproj` | **none**, and its **scope must hold at 18 pins across 5 pages**: tranche 1 edits four of its five pages (`TutorialDurableOutbox.md`, `TutorialFirstCommand.md`, `TutorialFirstMessage.md`, `TutorialStreamingWithKafka.md`). A scope that fell would mean a repair deleted a pinned version | none |
| 6 `optioncheck` | **none** — its pin is its own | **none** unless a repaired block sits in an options table's page region; re-run each phase | none |
| 8 `symbolcheck` | **none** | **none** expected; the E4 repairs remove `UseResiliencePipeline` on async handlers, and that name is not watchlisted | none |
| 9 `blockcheck` | **BUILT unchanged at 101** (E2: the pin alone moves nothing); **exit stays 0** with `PageContext.cs` cut in the same PR; the scope line gains the unit-rule result | **BUILT rises** each phase; SKIPPED rises only by accepted reasons; the baseline moves with it | the closing figure |

**The prediction worth watching is phase 1's "BUILT unchanged."** A pin change that moved it would
mean a verdict changed for a reason nobody repaired — the E2 control says it should not.

## Open Questions For The Design Review

| # | Question | Recommendation | Depends on |
|---:|---|---|---|
| **D1** | **P0-7's second clause targets zero blocks.** Re-scope it to *"a sync attribute on an async handler, or the reverse, surveyed across the 13 paired attributes"* — 6 defects, 5 pages — and add `attr_mismatch.py` as its instrument (AC9)? | **Ruled 2026-09-26: re-scope.** P0-7 and AC9 rewritten in `requirements.md` | the maintainer |
| **D2** | **Pin `OpenTelemetry.Exporter.Jaeger`?** Its last release is 1.5.1; upstream OpenTelemetry deprecated it for OTLP | **Ruled 2026-09-26: not pinned.** | the maintainer |
| **D3** | **A second pin for the AWS V4 family?** 7 V4 packages are named on pages and cannot share `refs.csproj` | **Ruled 2026-09-26: 018.** | — |
| **D4** | **Make `attr_mismatch` a standing gate** (a `pagelint` rule or a `blockcheck` mode) rather than a probe? | **Not in 017** — a new convention needs a `CLAUDE.md` ledger row. Propose it at the close, with 017's run as its evidence | the maintainer |
| Q3 | target | **set:** BUILT ≥ 250; pages with nothing BUILT ≤ 60 | settled by § *Target And Tranches* |
| Q5 | stubs extend to ~400 blocks, members only as used | **stands**, now with rules 1–4 enforced | the maintainer, if not ruled at the requirements review |
| Q7 | grow the pin early | **stands, in phase 1**, less D2 and D3 | settled by E2 |

## What the Design Review Found

2026-09-26. The nine gates all read at `tools/README.md`'s figures; none exited 2. **Six findings,
each a claim the design made without the measurement behind it.** All repaired above.

| # | Said | Measured | Now |
|---:|---|---|---|
| **1** | E4's six defect blocks *"compile"* | all six are **FAILED** in `--report` (`CS0246` and kin) | the danger is stated: a tranche repair would baseline them broken; rule 8 runs the instrument in every tranche PR |
| **2** | phase 1 pins *"E2's packages"* | E2 omitted five unpinned Brighter packages that pages name. The complete set: **538 assemblies, 0 verdicts change** | phase 1's pin names all of them; the measurement is in E2's table |
| **3** | only `PageContext.cs` fails the unit rule | a dry run of rules 1–3 over all 14 units: **13 pass, `PageContext.cs` 26 violations** — true, but it had not been run | the measurement is under § *The Unit Rule, Enforced* |
| **4** | pages with nothing BUILT → ≤ 60 | **43** of the 97 are reachable in the tranches; **37** must land | the arithmetic is beside the target |
| **5** | `versioncheck` — *"none, it reads page pins"* | it reads five `TUTORIAL_PAGES`, **four of which tranche 1 edits** | its scope, 18 across 5, is named as what must hold |
| **6** | nothing on tutorial samples | `TutorialFirstCommand.md` cites `samples/CommandProcessor/HelloWorld`; sample writes are per-PR | rule 9 |

## Design Quality Checklist

- [x] Readable with no prior context; self-contained — the experiments carry their own method
- [x] Page type, banner, opening sentence, qualified headings — **N/A**: no page is created
- [x] Every API named is resolved, with the command (§ *API Resolved For This Design*)
- [x] Every number has its command, or its experiment
- [x] Gate movement predicted for all nine, including "none", by phase
- [x] Structure as trees and tables
- [x] Detailed enough to build from — the modes, their rows and exit codes, the four unit rules, the
      repair table and the accepted skip reasons
