# Spec 018: Compile Residual — Requirements

**Created:** 2026-10-04
**Status:** **APPROVED 2026-10-04** — `.requirements-approved`. The seven open questions were approved open, so each recommendation is the design's working assumption.

> Every number here carries the command that produced it, measured 2026-10-04 against Docs `master`
> `3a79b20`, Brighter `10.7.0` and `origin/master` `a7b3898aa`. Gate figures are **cited from
> `tools/README.md`**, rows 2 and 9, not restated as this document's own.

> **The README re-derives, but it is not complete.** All six of its figures hold at `3a79b20`
> (§ *Current state*). It missed one input: **017's D3, a second pin for the AWS V4 packages, was ruled
> *"018"*** (`spec/017-compile_repairs/design.md` § decisions, D3). That pin holds **6** FAILED blocks
> on 4 pages, and it is P0-5 below. The README also carried only three of 017's fourteen friction
> repairs (#69, #71, #79). Each of the other eleven is placed below as P0, P1, P2 or out of scope, so
> that none is decided by omission.

## Subject

**Process.** The deliverable is a new `pagelint` rule, an extended `blockcheck` instrument, a second
pin, and repaired C# on existing pages. **It creates no page.**

| Section | Status |
|---|---|
| **SUMMARY.md changes** | **N/A.** No page is added, moved or retitled. On a process spec this section is a category error, not an empty heading |
| **Mode mix** | **N/A.** No page changes what it is for |
| **Target audience** | **N/A as a design input**, per the subject table. Every repair still lands on a published page, so it is held to `CLAUDE.md`'s reader standards through § *Constraints*, as 017's were |

## Topic overview

016 built `tools/blockcheck.py`, which compiles every C# block on its own against the packages pinned
in `tools/blockcheck/refs/refs.csproj`. CI's `blocks` job enforces its baseline in both directions.
017 raised the baseline from 101 to 299 blocks across 75 tranche pages and closed on this sentence
(`spec/017-compile_repairs/tasks.md` § *What 017 shipped*):

> **674 of 990 C# blocks still do not compile against the released packages, and 630 of them sit on
> the 64 pages 017's tranches never reached** … so 018 first builds `pagelint` rule 8 and gives
> `--classify` its second compile, and then tranches those 64 pages by what that instrument finds.

That fixes the order: **instruments first, then pages.** There are two instruments, and both come
from 017:

- **Rule 8** turns the attribute-mismatch probe into a standing gate. The compiler accepts a sync
  handler attribute on `HandleAsync`, and Brighter throws `ConfigurationException` when it builds
  the pipeline. So `blockcheck` would certify such a block as BUILT. The maintainer ruled the form on
  2026-09-29 (017 task 6.7, § *For the maintainer: D4*).
- **The second compile** lets one committed instrument draw the tranches. In 017 the tranche lists
  came from an uncommitted probe in `spec/`, with `--classify` only as a lower bound. **162** blocks
  that `--classify` called *import* had a defect behind the missing `using` (friction #71).

A block that does not build is one a reader cannot paste and trust, and one the gate cannot hold.

## Current state

### The residual, re-derived

```bash
dotnet build tools/blockcheck/refs/refs.csproj -c Release        # 0 errors
dotnet build tools/blockcheck/blockcheck.csproj -c Release        # 0 errors
python3 tools/blockcheck.py --report $S/r.tsv; echo $?            # 0
#   baseline: 299 blocks required to build · scaffold rule: 45 units checked, 0 violations · 0 findings, 17 skipped
cut -f1 $S/r.tsv | sort | uniq -c                                 # 299 BUILT, 674 FAILED, 17 SKIPPED; 990 rows
python3 tools/blockcheck.py --classify > $S/cls.tsv; echo $?      # 0; 674 rows
python3 tools/pagelint.py | tail -1                               # 0 errors, 524 warnings (… 66 pages) across 162 pages
```

These match `tools/README.md` row 9 (`bd95ee0`) and row 2. **Unmoved since then**:
`git diff --stat bd95ee0 3a79b20 -- contents/ tools/` → 3 files. In `contents/` that is two prose
lines in `Telemetry.md` and `ConfiguringOpenTelemetry.md` linking #4510, and no fence is touched.

| Figure | Inherited (017 at `e0385b4`) | Measured at `3a79b20` | Method |
|---|---:|---:|---|
| BUILT / FAILED / SKIPPED | 299 / 674 / 17 | **299 / 674 / 17** | `--report`, then `cut -f1 \| uniq -c` on its rows |
| FAILED on the 75 tranche pages | 44 on 24 pages | **44 on 24** | `--classify` joined to the 75 page names in 017 § *The tranches* (phases 2–5 tables; `grep -oE` the `` `*.md` `` cells → 75) |
| FAILED on the other pages | 630 on 64 | **630 on 64** | the same join, complement |
| …by class | 314 import, 145 parse, 113 page-type, 34 values, 18 other, 6 same-page | **the same** | `Counter` over column 3 of the off-tranche rows |
| Off-tranche pages with nothing BUILT | 36 | **36** | off-tranche pages absent from `--report`'s BUILT rows |
| All pages with a FAILED block / nothing BUILT | 88 / 39 | **88 / 39** | `--report` rows alone |
| `pagelint` `using` debt | 524 / 66 pages | **524 / 66** | `pagelint`'s summary line |

**`--classify`'s own stderr counts the whole corpus**, not the off-tranche share: *"323 import, 146
parse, 130 page-type, 35 values, 21 other, 19 same-page"* (674). The off-tranche figures above are
those less the 44 tranche blocks. Both sum to 674, and that is the second method.

### Where the 630 are

By the `SUMMARY.md` `##` section each page is listed under (the join above, grouped):

| Section | Pages | FAILED blocks |
|---|---:|---:|
| Scheduler | 8 | 129 |
| Commands, Handlers and Pipelines | 9 | 101 |
| Darker | 6 | 87 |
| Using an External Bus | 10 | 83 |
| Understanding Brighter | 4 | 50 |
| Brighter Configuration | 5 | 44 |
| Transports | 6 | 41 |
| V10 Migration | 3 | 37 |
| Outbox and Inbox | 9 | 26 |
| Reference | 1 | 17 |
| Health Checks and Observability | 2 | 9 |
| Get Started | 1 | 6 |
| **Total** | **64** | **630** |

The heaviest pages are `HangfireScheduler.md` and `QuartzScheduler.md` with 27 each, then
`QueryPipeline.md` 25, `AwsScheduler.md` 23, and `AzureScheduler.md`,
`CommandProcessorConfigurationReference.md` and `ImplementAQueryHandler.md` with 21 each.
`QueryPipeline.md` is **12 *parse*** of its 25, and `KafkaConfiguration.md` is **14 of 20**. Neither
a `using` nor a stub reaches those blocks.

### Rule 8's input, re-derived

```bash
python3 spec/017-compile_repairs/probe/attr_mismatch.py; echo $?          # 1
#   contents/PipelineValidation.md:250  RejectMessageOnError on a async handler
python3 spec/017-compile_repairs/probe/attr_mismatch.py --plant; echo $?  # 0, "…: OK"
```

The one hit is **`PipelineValidation.md` #7**, the deliberate *Async Handler with Sync Attributes*
example. `blockcheck --list` puts block 7 at the fence opening at `:247`, and `:250` falls inside it.

**The pair list, two refs.** The derivation, run in `../Brighter` at both refs:

```bash
git grep -hoE 'class [A-Za-z]+Attribute' <ref> -- src | sed -E 's/class ([A-Za-z]+)Attribute/\1/' | sort -u > a
grep -E 'Async$' a | sed 's/Async$//' | sort -u | comm -12 - a
```

At `10.7.0` and at `origin/master` `a7b3898aa`, this gives the same **13** names: `BulkDepositCallSite DeferMessageOnError
DepositCallSite DontAckOnError FallbackPolicy FeatureSwitch Monitor RejectMessageOnError
RequestLogging UseInbox UsePolicy UseResiliencePipeline ValidateRequest`. These are the probe's
`PAIRED`, name for name.

**Said:** *"`master` at `2461094a6` has the same 43 attribute classes"* (017 § *For the maintainer:
D4*). **Measured:** this command gives **52** class names at both refs. The 13 pairs agree, so the
rule's input is unchanged. The total depends on the command, and that is why `PAIRED`'s comment must
hold one (P0-1).

**Nothing outside `spec/` runs the probe:** `git grep -l attr_mismatch -- .github tools` → no output.
**`pagelint` has no plant mechanism today:** `grep -n plant tools/pagelint.py` → no output.

### The V4 pin, measured

```bash
grep -rhoE 'Paramore\.Brighter[A-Za-z.]*\.V4[A-Za-z.]*' contents/ | sort | uniq -c   # 8 spellings, 48 mentions
grep -rhoiE 'Paramore\.Brighter[A-Za-z.]*\.V4' contents/ | tr A-Z a-z | sort -u | wc -l # 7 packages
grep -rlE 'Paramore\.Brighter[A-Za-z.]*\.V4' contents/ | wc -l                        # 9 pages
ls ../Brighter/src | grep -i v4                                                       # 8 directories
```

Eight spellings name seven packages, because `Inbox.DynamoDB.V4` also appears as `Inbox.DynamoDb.V4`
(below). The eighth source directory is `Paramore.Brighter.Tranformers.AWS.V4`, a misspelt sibling
of `Transformers.AWS.V4` that no page names.

The blocks that D3 holds FAILED. Two methods agree on the same six:

| Method | Blocks |
|---|---|
| Staged block text names `.V4` (`--stage`, `grep -l '\.V4'`), joined to FAILED rows | `AwsScheduler.md` #2, #3; `DistributedLock.md` #2; `DynamoDbDistributedLock.md` #1, #2; `S3LuggageStore.md` #1 |
| `--explain` over all 674 FAILED, `CS0234` naming `V4` | the same six |

(A seventh staged block, `AWSSQSMigrateToV10.md` #7, names `.V4` and is BUILT. The match is outside
a `using`.) Three of the six are on 017 tranche pages and are listed in § *Blocks that stay FAILED*
as waiting for "D3, 018". `AwsScheduler.md` is off-tranche.

**One page spells a package two ways.** `contents/DynamoInbox.md:21` writes
`Paramore.Brighter.Inbox.DynamoDb.V4`, while the project is `Paramore.Brighter.Inbox.DynamoDB.V4`
(`ls ../Brighter/src`). NuGet IDs are case-insensitive, so the name resolves. It is recorded here as
a fact for the defect ledger, not as a blocker.

### The pin's framework

`refs.csproj` and `blockcheck.csproj` both target **`net9.0`** (`grep -n TargetFramework
tools/blockcheck/*.csproj tools/blockcheck/refs/*.csproj`). 017's behaviour runs were `net10.0`, where a
reader of `TickerQScheduler.md` gets TickerQ 10.4.0, not the pin's 9.0.2 (friction #76).

### 017's friction ledger, as it lands here

All fourteen of 017's entries (67–80, `spec/017-compile_repairs/tasks.md` § *Friction ledger*), each
placed:

| # | The repair 017 proposed | Here |
|---:|---|---|
| 67 | `blockcheck` aligns fences across refs by content | **P2-1** |
| 68 | Run the two reading criteria in every phase's close task | **P0-6**, obligation |
| 69 | Commit every reused helper and every behaviour run under `spec/018-*/probe/` | **P0-6**, obligation |
| 70 | `--classify` reads the receiver; a pin change lists name collisions | **P0-2** (D1) |
| 71 | `--classify` gets the probe's second stage | **P0-2** |
| 72 | Predict the off-tranche term from the recurrence greps | **P0-6**, obligation |
| 73 | A verified defect is repaired at every recurrence; only scope changes go to the maintainer; a *second pass* task per phase | **P0-6**, obligation |
| 74 | A ledger row only after its grep has run as written | **P0-6**, obligation |
| 75 | The `statements` wrapper lets a block name `Program` | **P1-2** |
| 76 | *Compiles against the released packages, not in the pin* as a verdict; measure `net9.0` vs `net10.0` | **P1-3** (the measurement); the verdict is **P2-2** |
| 77 | A scaffold unit that does not compile is a finding | **P1-4** |
| 78 | `pages.tsv` maps a unit per block | **P2-3** |
| 79 | Exempt the deliberate hit with a marker; name hits by page and block | **P0-1** and AC3 |
| 80 | `pagelint` per-page count; `optioncheck` primary-constructor options | per-page count **P1-5**; `optioncheck` **out of scope** |

## Target state

- `python3 tools/pagelint.py` checks **rule 8, `ATTRIBUTE KIND`**, on every page, as an error both
  repo-wide and under `--changed`. CI proves that it can fail. `CLAUDE.md`'s ledger and conventions
  describe it, and the 017 probe is no longer the instrument.
- `python3 tools/blockcheck.py --classify` answers the question the 017 probe answered: **what
  remains once the `using`s are supplied.** Its classes say which blocks a `using` alone completes and
  which hold a defect behind the `using`.
- A second pin compiles the AWS V4 blocks, so they are judged rather than parked.
- The 64 pages are tranched from that instrument's output, and repaired under 017's page-repair
  rules. Each FAILED block on a reached page leaves in one of the four states 017's design defined,
  and the baseline holds every block that builds.

## Target audience

**N/A as a design input** (§ *Subject*). Every page repair is still read by the ordinary reader
`CLAUDE.md` writes for. § *Constraints* binds that.

## Source material

- **017, the spec this continues.** `spec/017-compile_repairs/tasks.md`: § *What 017 shipped* (the
  residual), § *For the maintainer: D4* (rule 8's proposal and the ruling), § *Friction ledger*
  67–80, § *Blocks that stay FAILED*, § *The tranches* (the recipe and the 75 pages), § 1 *Standing
  obligations*. `spec/017-compile_repairs/design.md`: § *Page Repair Rules*, § *Scaffold Stub
  Rules*, decisions D1–D4
- **The probes being promoted.** `spec/017-compile_repairs/probe/attr_mismatch.py` (rule 8) and
  `probe/run.sh`, `classify.py`, `usings.py`, `pages.py`, `typedump/` (the second compile)
- **The tools.** `tools/pagelint.py` (`APPLIES_TO` at `:164`, the `allow-serviceactivator` opt-out
  at `:215`, `check_code_blocks` at `:475`, `main` at `:1188`), `tools/blockcheck.py` (`classify`
  at `:254`, `CLASS_ORDER` and `classify_failure` at `:1198`–`:1204`),
  `tools/blockcheck/refs/refs.csproj`, `tools/blockcheck/scaffold/pages.tsv`,
  `tools/blockcheck/baseline.tsv`, `.github/workflows/docs.yml` (the `check` and `blocks` jobs)
- **The authorities.** `CLAUDE.md` § *Page Conventions*, § *Enforcement* and its ledger, § *Compiling
  an example, and against what*; `tools/README.md` (gates, exit codes, *One phase is one pull
  request*)
- **The product.** `../Brighter` at `10.7.0` (the pin) and `origin/master`, read-only: the attribute
  classes under `src/`, and the `*.V4` projects under `src/`
- **The command files.** `.claude/commands/spec/*.md`

## Scope

### P0 — the spec is not done without these

- **P0-1: `pagelint` rule 8, `ATTRIBUTE KIND`, as ruled 2026-09-29.** A handler attribute in
  `PAIRED` whose sync or async form does not match the method it decorates is an error, repo-wide and
  under `--changed`. It reads C# blocks through `pagelint.Page`, including blocks that do not
  compile. Its parts:
  - **`PAIRED`** is a tuple beside `APPLIES_TO` in `tools/pagelint.py`. Its comment holds the
    derivation command above, so the version bump that edits `APPLIES_TO` re-derives `PAIRED` in
    the same edit
  - **The opt-out is per block, with a mandatory reason:**
    `<!-- pagelint: attr-mismatch-intended <reason> -->` on the line before the fence. A marker
    with no reason is itself an error. A page-wide marker is not offered, because it would have
    hidden `PipelineValidation.md` #9 and #10 beside the deliberate #7
  - **`PipelineValidation.md` #7** carries the marker, so repo-wide rule 8 hits read **0**
  - **The red-proof is carried over** from the probe's `--plant`: two cases that must hit and one
    that must not, plus a marker with no reason, run in CI as `pagelint --plant` (open question 1)
  - **`CLAUDE.md`:** a ledger row, and a short § *Handler attributes match their handler* under
    *Page Conventions*. **`tools/README.md`:** row 2's figures, and `pagelint`'s description, gain
    rule 8
  - **The probe is retired.** It stays in 017 as history, and no 018 task runs it
- **P0-2: `--classify` gets its second compile (friction #71).** For every FAILED block, supply the
  `using` directives that the pinned type table resolves, compile again, and classify what remains.
  It stays committed, deterministic, read-only, and exit 2 on nothing to report, like the rest of
  `blockcheck`'s modes. It must distinguish at least:
  - a block that **builds** once given its `using`s
  - a block that then fails **only on names no pinned package ships**: stub territory, 017's STUB
  - a block that then fails on a **binder error that is not a missing name**: 017's DEFECT
  - a block that **does not parse**, unchanged

  **Resolution reads the receiver (friction #70).** A name is resolved to a pinned type only when the
  page does not declare it and the staged class's base does not supply it. Evidence: `Order` →
  `StackExchange.Redis`'s enum (22 blocks on 10 pages), a handler's `Context` → `Polly.Context`
  (17 rows on 5 pages), `AddOpenTelemetry` pinned only on `ILoggingBuilder`, and `Build` on
  OpenTelemetry's builders. 017 ruled it *not repaired* (2026-09-27), when the cost was a reading
  rule. Here, a by-name second compile would draw the tranche tables wrong.

  It is red-proofed with a two-way control: one block of each class, in a plant or a recorded run.
  It is reconciled against 017's probe over the same 674 blocks, block by block, with every
  disagreement explained. The output format is open question 2
- **P0-3: tranche the 64 pages from P0-2's output, and repair them.** Tranches are drawn by the
  committed instrument alone, so no tranche table carries a probe column. Repairs follow 017
  `design.md` § *Page Repair Rules* and § *Scaffold Stub Rules*, which 018's design cites and does
  not restate. Every FAILED block on a reached page leaves in one of 017's four states. Each reached
  page's remaining FAILED blocks are named in an 018 § *Blocks that stay FAILED*, with the reason.
  How many pages 018 commits to is open question 3
- **P0-4: the baseline holds what builds.** Each repair phase's new BUILT blocks enter
  `tools/blockcheck/baseline.tsv` in the same PR, and `--report` exits 0 at every merge
- **P0-5: the AWS V4 pin (017 D3, ruled "018").** The seven `*.V4` packages the pages name are
  pinned where `blockcheck` can compile against them, without breaking `refs.csproj`'s single-version
  restore (017 found the V4 family cannot share it). The six blocks above are judged against it. The
  form is a second project or a per-block pin selection: open question 4
- **P0-6: 017's process repairs become 018's standing obligations.** Write them into 018's
  `tasks.md` § 1, beside the programme's seven and 016's five, which 018 inherits by citation:
  - **#69:** every helper used in more than one task, and each behaviour run's `Program.cs` with
    its case and control, is committed under `spec/018-compile_residual/probe/`, and a run table's
    row names its file
  - **#68:** each repair phase's close task runs the two reading criteria (AC9, AC10 below) over
    the blocks that phase touched, so the acceptance walk re-reads criteria already met
  - **#72:** a phase's gate prediction includes the off-tranche term, predicted from the recurrence
    greps
  - **#73:** a defect verified at the pinned release is repaired at every recurrence in the phase
    that finds it. Only a change of scope (a page rewritten, a feature removed, an upstream issue) is
    put to the maintainer. Every repair phase has a *second pass* task
  - **#74:** a defect-ledger row is written only after its grep has run as written. Its *Page*
    column names pages. A carried defect is closed by editing its row
  - **#79:** a criterion names a hit by page and block, never by line
- **P0-7: a handler wrapper.** A members-shaped block that overrides `Handle`, `HandleAsync`,
  `Execute` or `ExecuteAsync` is staged in a class deriving from `RequestHandler<T>`,
  `RequestHandlerAsync<T>`, `QueryHandler<TQ, TR>` or `QueryHandlerAsync<TQ, TR>`, with the type
  arguments read from its signature. A block whose signature names no request type is not wrapped.
  It reaches 32 FAILED blocks (design E5)
- **P0-8: `Shouldly` in `refs.csproj`.** Six blocks on `TestDoubleOptions.md` use its assertions
  (design E2)
- **P0-9: two more accepted skip reasons**, beside 017's three: *"forthcoming: ships after Brighter
  10.7.0, as the page says at line N"*, and *"a single option shown alone; its type is named in the
  sentence before it"*, allowed only where that type resolves in the pin. Every other fragment is
  made whole under 017's rule
- **P0-10: `AddServiceActivator`, dead and shown as current.** `AwsScheduler.md:290` and
  `AzureScheduler.md:231` are rewritten to `AddConsumers`. `tools/symbolwatch.tsv` gains a row for
  the name, with per-symbol opt-outs on `V10MigrationGuide.md` and `FAQ.md`, which discuss it as the
  V9 name. The row and the repairs merge together (`tools/README.md`, rule 3)

### P1 — should, if the instruments leave room

- **P1-2: the `statements` wrapper lets a block name `Program` (friction #75)**, by staging such a
  block as a top-level file, as P1-3 of 017 answered `args`
- **P1-3: measure what the `net9.0` pin misses against `net10.0` (friction #76)**, before 018's
  tranches are drawn: which pinned packages resolve differently, and which verdicts would move
- **P1-4: a scaffold unit that does not compile is a finding, exit 1 (friction #77)**, as a
  unit-rule violation is
- **P1-5: a `pagelint` per-page warning count (friction #80)**, so a phase's debt delta is a diff
  of two outputs

### P2 — later, recorded

- **P2-1:** `blockcheck` aligns fences across two refs by content, and reports *inserted*,
  *removed* and *renumbered* (friction #67)
- **P2-2:** *compiles against the released packages, not in the pin* as a verdict the gate reports
  and re-checks (friction #76's second half)
- **P2-3:** `pages.tsv` maps a scaffold unit per block (friction #78)

## Out of scope

- **New pages, moved pages, retitled pages.** A block that cannot be repaired without rewriting its
  page is named in § *Blocks that stay FAILED*, and the rewrite is put to the maintainer (#73)
- **Changes to Brighter or Darker source**, which is read-only (`CLAUDE.md` § *Key Constraints*). A
  defect found in the product is filed upstream on the maintainer's word, as 017's four and #4510
  were. **Samples** fall under the narrow exception, and each sample needs its own per-PR ask
- **`optioncheck` construction of primary-constructor options** (friction #80's second half). It is
  a different gate, and nothing in this spec's residual depends on it
- **The `v9` branch.** It has no banner and no gate, and it is published as superseded
- **Running every behavioural claim on the 64 pages.** Only blocks a repair *changes* owe a run with
  a control (AC10). Claims on untouched blocks are 019's or nobody's, and are not silently implied
  here

## Deliverables

No page is created, so no page type is chosen. The repaired pages keep the types their banners
already declare (`pagelint` rule 2).

| Deliverable | File(s) |
|---|---|
| Rule 8 | `tools/pagelint.py` (`PAIRED`, the rule, the opt-out, the `--plant` mode) |
| Rule 8 in CI | `.github/workflows/docs.yml`, `check` job: `python3 tools/pagelint.py --plant` |
| Rule 8 documented | `CLAUDE.md` § *Page Conventions* (new subsection) and § *The ledger* (new row); `tools/README.md` rows 2 and 9 and *What each gate actually checks* |
| The marker | `contents/PipelineValidation.md`, above block 7 |
| The second compile | `tools/blockcheck.py` (`--classify`), and the C# half under `tools/blockcheck/` if a mode is needed there |
| The V4 pin | `tools/blockcheck/refs-v4/refs-v4.csproj`; the `v4` column of `tools/blockcheck/scaffold/pages.tsv`; `.github/workflows/docs.yml` `blocks` job |
| The handler wrapper (P0-7) | `tools/blockcheck.py` (`WRAPPERS`, the shape reader) |
| Shouldly (P0-8) | `tools/blockcheck/refs/refs.csproj`; `contents/TestDoubleOptions.md` |
| The skip reasons (P0-9) | `design.md` § *Page Repair Rules*, beside 017's |
| `AddServiceActivator` (P0-10) | `tools/symbolwatch.tsv`; `contents/AwsScheduler.md`, `AzureScheduler.md`, `FAQ.md`, `V10MigrationGuide.md` |
| Repaired pages | the 64 pages under `contents/`, and the 3 tranche pages that hold V4 blocks |
| The baseline | `tools/blockcheck/baseline.tsv`; scaffold units under `tools/blockcheck/scaffold/` and `pages.tsv` |
| Instruments that outlive a task | `spec/018-compile_residual/probe/` (#69) |
| The record | `spec/018-compile_residual/design.md`, `tasks.md` (ledgers, run tables, § *What 018 shipped*) |

## SUMMARY.md changes

**N/A.** No page is added, moved or retitled (§ *Subject*). `python3 tools/linkcheck.py`'s orphan
check stays at 0 and confirms it.

## Constraints

- **`CLAUDE.md` governs every repaired page**, and is cited rather than restated: banner, heading
  qualification, `using` directives, version markers, and compiling against the released packages
- **Brighter and Darker are read-only.** Sample additions happen only by PR, and each needs a
  fresh per-PR ask with the reuse/extend survey done
- **One phase is one pull request** (`tools/README.md`). The tool-only phase changes one page (the
  marker), so it is put to the maintainer for sign-off rather than assumed exempt
- **A gate and the corpus that satisfies it merge together** (`tools/README.md`, rule 3). Rule 8 and
  `PipelineValidation.md`'s marker are in the same PR, or `master` goes red
- **No `--baseline <path>`, and no mode that writes `baseline.tsv`** (`tools/README.md`). P0-2 and
  P0-5 must not add either
- **Exit 2 is never swallowed**, so a new mode or CI step carries no `|| true`
- **`refs.csproj` keeps its single-version restore.** The V4 pin must not downgrade or break it

## Acceptance criteria

| # | Criterion | Instrument |
|---|---|---|
| **AC1** | Rule 8 exists, and fails when it should | `python3 tools/pagelint.py --plant` exits **0** only if both mismatch plants hit and the matched pair is silent, and a red-proof run with a plant **removed** exits non-zero, recorded in `tasks.md`. Two-way. **Built by P0-1; until it exists, this criterion has no instrument** (`--plant` is not a `pagelint` mode at `3a79b20`) |
| **AC2** | Rule 8 is clean on the corpus, with the deliberate example marked | `python3 tools/pagelint.py` → `0 errors`, and `grep -c 'attr-mismatch-intended' contents/PipelineValidation.md` → **1**. A red-proof run with the marker removed reports `ATTRIBUTE KIND` on `PipelineValidation.md` block 7, recorded |
| **AC3** | The opt-out cannot be silent | A marker with no reason is one of `--plant`'s cases and must be reported as an error, so AC1's command decides it. **Built by P0-1; no instrument until then** |
| **AC4** | `CLAUDE.md` and `pagelint` agree on rule 8, in both directions | **No instrument — checked by reading**, by the reviewer: the ledger row, the convention section and the rule's message are read against each other. `pagelint` has no self-check of its ledger, and that is the reason this criterion is a reading |
| **AC5** | The probe is retired | `git grep -l attr_mismatch -- .github tools contents` → no output (exit 1; it reads that today, because nothing outside `spec/` has ever run the probe). That **no 018 task runs it**: `grep -n 'attr_mismatch' spec/018-compile_residual/tasks.md`, each hit **read** by the reviewer, because no grep can tell a task's *Input* from a quotation of 017 |
| **AC6** | `--classify` compiles twice, and can be wrong in both directions | A red-proof with at least one block of each class (built-by-`using`, stub, defect, parse) classifies each correctly. A control block known to be defect-behind-`using` is **not** classified as built. Recorded in `tasks.md`. **Built by P0-2; no instrument until then** |
| **AC7** | `--classify` agrees with 017's probe, or says why not | A block-by-block join of the new `--classify` against 017's `probe/run.sh` + `pages.py` at the same ref, over the FAILED set. Every disagreeing block is listed with its reason. Two methods, recorded. **The probe half runs today; the join needs P0-2** |
| **AC8** | The tranches are the instrument's | **No instrument — checked by reading**, by the reviewer: every tranche table's columns derive from `--classify`'s output by a command shown beside the table, and no column comes from a probe |
| **AC9** | Every repaired block's `// ...` is justified | **No instrument — checked by reading**, by the reviewer, **at every phase's close** (#68): each `// ...` the phase adds, against `--explain`, as 017 task 6.1 did. 017's AC8 second half was unmet at acceptance because this ran only once |
| **AC10** | Behavioural claims in changed blocks are run with a control | **No instrument — checked by reading**, by the reviewer, **at every phase's close** (#68): the phase's changed blocks (`git diff -U0` hunks against fence ranges), each claim mapped to a run whose `Program.cs` is committed under `probe/` (#69) with its case and control. 017's AC11 was unmet at acceptance on 30 blocks |
| **AC11** | The V4 blocks are judged | Each of the six blocks in § *The V4 pin* is BUILT, or FAILED with a diagnostic that is not `CS0234 … V4`: `--explain` on the six, then `grep -c 'CS0234.*V4'` → **0** |
| **AC12** | The residual falls, and the gate holds it | `--report` exits **0** with BUILT above **299**, and every new BUILT block is in `baseline.tsv` (enforced by `--report` itself). The target figure is set at design (open question 3) |
| **AC13** | Every FAILED block on a reached page is accounted for | For each reached page, `--report`'s FAILED rows equal 018 § *Blocks that stay FAILED*'s rows for that page: a join, zero rows either side. **No instrument until that section exists in `tasks.md`**; the join command is written beside it, as 017's AC4 was |
| **AC14** | `tools/README.md` owns every new figure | The corrected-form count from `tools/README.md` (`grep -rn '<figure>' … \| grep -vcE '^(\./)?spec/'`) → **1** for each new gate figure 018 introduces. The form runs today (`'299 BUILT'` → **1**); its inputs are the figures the phases produce |

## Open questions

1. **Rule 8's plant: a `--plant` mode or a plant file?** *Recommendation:* a **`--plant` mode**
   holding its cases in memory, as the probe's `--plant` does, so that the next rule's plants join
   it. A plant file is the costlier form: `pagelint` refuses any path outside `contents/`
   (`python3 tools/pagelint.py <file outside contents/>` → *"not a page under contents/"*, exit 2),
   so a file would mean loosening that refusal. A mode does not touch it. *Depends on:* nothing
   further, since the dependency has been measured.
2. **What does `--classify` print for each new class?** *Recommendation:* keep the four-column
   `page·ordinal·class·names` row, and add classes rather than columns: `built-by-using`, `stub`,
   `defect`, with `parse` unchanged. The first-compile class goes in a fifth column, so the 017-style
   lower bound and the true figure both stay visible. *Depends on:* nothing committed parsing the
   four columns, and nothing does. `git grep -n -- '--classify' tools .github` finds only
   `blockcheck` itself and `tools/README.md`'s description, which the change rewrites.
3. **How many of the 64 pages does 018 commit to?** *Recommendation:* draw the tranches after P0-2
   runs. Commit to every page P0-2 shows as reachable by a `using` or a stub, and set the
   hard-block pages' share by count at design, as 017's ≤ 60 target was. Do not promise all 64 up
   front: **145** *parse* blocks are an unknown quantity, and **26** of them are on two pages
   (`QueryPipeline.md` 12 of its 25, `KafkaConfiguration.md` 14 of its 20). *Depends on:* P0-2's
   output.
4. **The V4 pin: a second project, or a per-block pin selection?** *Recommendation:* **a second
   project**, `refs-v4.csproj`, with the AWS V4 packages and Brighter 10.7.0, selected per page in
   `pages.tsv` by a column. It keeps `refs.csproj`'s restore untouched and costs one more build step
   in CI. *Depends on:* whether the V4 packages and the V3 packages that the same pages also name
   can share one restore. If not, the six blocks need a per-block selection, which is P2-3's shape.
5. **Is P1-1, the receiver-aware `--classify`, re-put to the maintainer before P0-2 or after?**
   *Recommendation:* **before.** P0-2 supplies `using`s from the type table, so without the receiver
   it supplies `using StackExchange.Redis;` for every page-own `Order` (22 blocks, 10 pages at 017
   task 6.2). The second compile then reports a defect that is the instrument's, not the page's.
   *Depends on:* the maintainer, who ruled it *not repaired* on 2026-09-27.
6. **Does phase 1 (rule 8, the second compile, the V4 pin) ship as one PR or three?**
   *Recommendation:* **two.** Rule 8 and its marker go first, because they are small and gate-shaped
   and change one page. The second compile and the V4 pin go second, because the tranches depend on
   both and neither changes a page. *Depends on:* question 4's answer. A V4 pin that changes
   verdicts changes the baseline, and so belongs with the first page repairs.
7. **Is P1-3, the `net10.0` measurement, really P1?** *Recommendation:* **keep it P1, but run it
   before the tranches are drawn**, as friction #76 asks. It is a measurement, not a change, and it
   can only move tranche boundaries. If it shows verdicts moving, raise it to P0 at the design
   review. *Depends on:* nothing but a build.

## Maintainer rulings — 2026-10-04, at the design review

| Ruling | Effect on this document |
|---|---|
| **D1:** the receiver-aware `--classify` is P0 | P1-1 is merged into P0-2, under *Resolution reads the receiver*. P1-1's number is retired, and the other P1 items keep theirs. Open question 5 is answered |
| **D2:** a handler wrapper | P0-7 |
| **D3:** the two skip reasons, the second narrowly | P0-9 |
| **D4:** pin Shouldly | P0-8 |
| **D5:** repair `AddServiceActivator` and watch for it | P0-10 |

The evidence for each is in `design.md` § *The Experiments* and § *Design Decisions*.

## What the review found — 2026-10-04

Every criterion's instrument was run at `3a79b20`. AC2 → `0 errors` (vacuous until rule 8 exists, which is why AC2 also
asks for its red-proof); AC5 → no output; AC11 → **11** `CS0234 … V4` lines across exactly the six
blocks, red as it should be; AC12 → **299**, red as it should be; AC14's form → **1**.

| # | Found | Now |
|---:|---|---|
| 1 | AC1, AC3, AC6, AC7, AC13 and AC14 named instruments that do not exist at `3a79b20`: a plant mode, a new `--classify`, an 018 section. A criterion whose instrument cannot run is a criterion with none | Each says what builds its instrument and that it has none until then |
| 2 | AC5's second half was `grep -c … outside § history quotes`. No grep can exclude a quotation | A grep to find the hits, and the reviewer reads each |
| 3 | Open question 3 said 25 *parse* blocks on two pages. 12 + 14 = **26** | 26 |
| 4 | Open question 1 depended on whether `pagelint` takes a path outside `contents/`. It does not: exit 2 | Measured. The recommendation changed from a plant file to a `--plant` mode |
| 5 | Open question 2 depended on committed consumers of `--classify`'s columns. There are none outside `blockcheck` | Measured. The recommendation stands |

## Quality checklist

- [x] Readable with no prior context: the topic overview quotes 017's residual, and every class and
      term used (*import*, *parse*, *page-type*, STUB, DEFECT, tranche) is defined at first use or
      cited to `tools/README.md`
- [x] P0 / P1 / P2 are separate lists, and each of 017's fourteen friction entries is placed
- [x] Specific sources: files, line numbers and refs, not "the tools"
- [x] A command beside every number. The figures quoted from 017 (`162`, `22 on 10`, `30`) are
      cited to their 017 section, and marked as 017's
- [x] Every acceptance criterion names its instrument or says *no instrument — checked by reading*,
      and who reads it: AC4, AC8, AC9 and AC10. AC9 and AC10 are the two that 017 found unmet,
      and they now run at every phase's close
- [x] Criteria say *contains* where they mean it, and name hits by page and block (#79)
- [x] N/A sections are named and marked N/A, not left empty
