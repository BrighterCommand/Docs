# Spec 018: Compile Residual — Design

**Created:** 2026-10-04
**Status:** Draft, for `/spec:review`
**Requirements:** approved 2026-10-04 (`.requirements-approved`), seven open questions approved open

> Every number here was measured 2026-10-04 at Docs `master` `3a79b20`, Brighter `10.7.0` and
> `origin/master` `a7b3898aa`, Darker `4.1.1`. The probe that produced the experiments is committed
> under `spec/018-compile_residual/probe/` (friction #69) and reproduces them in about 95 seconds:
>
> ```bash
> dotnet build tools/blockcheck/refs/refs.csproj -c Release
> dotnet build tools/blockcheck/blockcheck.csproj -c Release
> bash spec/018-compile_residual/probe/run.sh $W; echo $?        # 0 — E1, E2
> bash spec/018-compile_residual/probe/v4pin.sh $W; echo $?      # 0 — E3
> ```

## Design Subject

**Process**, as the requirements declare. No page is created, so no outline is written file by file.

| Section | Status |
|---|---|
| **SUMMARY.md changes** | **N/A.** No page is added, moved or retitled. On a process spec this is a category error, not an empty section |
| **Page outlines** (type, banner, opening sentence, headings) | **N/A.** Every repaired page keeps the banner, type and headings it has. A repair that changes an opening sentence re-runs `pagelint --fix <page>` (017 `design.md` § *Page Repair Rules*, item 6) |
| **The one new prose section** | `CLAUDE.md` § *Handler attributes match their handler*, outlined under § *Rule 8* below |

## Requirements Re-Verified

The experiments below changed four deliverables and added two.

| Requirement | Delta |
|---|---|
| **P0-1, rule 8** | **Unchanged in shape. Two details fixed.** The marker binds to the **next C# block below it**, the way `blockcheck`'s skip marker does (`tools/blockcheck.py:280–300`). So the two markers can stack above one fence, and "the line before the fence" no longer has to be literal. `PAIRED` is re-derived as **13** names at both refs (§ *API Resolved*) |
| **P0-2, the second compile** | **Changed shape.** E4 shows that a second compile which supplies `using`s by name, as 017's probe does, misreads **at least 39** of the 64 pages' FAILED blocks. Those are 17 *import* rows for a handler's `Context` property resolved to `Polly.Context`, and 22 for a page's own `Order` resolved to `StackExchange.Redis.Order`. At the second compile, **29 of 119** DEFECT verdicts are such artefacts. P1-1, the receiver-aware read, cannot stay P1 if P0-2 is to decide the tranches. **D1, ruled 2026-10-04: promoted into P0-2** |
| **P0-3, the tranches** | **Changed shape.** 017 tranched by hard-block count. E1 shows that rule reaches **10** of the 64 pages: only 2 have no hard block, and 8 have one. The hard blocks are spread across every section. So 018 phases by **section group**, with each group's page tables drawn from the committed instrument at the end of phase 2 (AC8) |
| **P0-5, the V4 pin** | **Answered.** Open question 4 asked whether V4 and V3 can share one restore. `refs.csproj:191–192` already says they cannot (`AWSSDK.SQS` 4.x against a cap below 4.0). E3 shows that a **second project with the seven packages swapped** removes every `CS0234` from the six blocks, and breaks **9** BUILT V3 blocks. So the pin is chosen **per page**, by the namespace the page's blocks import |
| **Q3, how many pages** | **Answered: all 64**, in five section groups of 107–137 FAILED blocks each (§ *Target And Phases*). The targets are set from E1 |
| **Added: P0-10, `AddServiceActivator` on two pages** | **New, found by E2.** `AwsScheduler.md:290` and `AzureScheduler.md:231` call `builder.Services.AddServiceActivator(…)` as current code. At `10.7.0` and `master` the name appears only in two doc comments, with no definition. `tools/symbolwatch.tsv` has no row for it, so `symbolcheck` is green over it. **D5, ruled 2026-10-04: repaired in phase 3, with a watchlist row** |
| **Added: P0-7, P0-8, P0-9 — E2's repair categories with no 017 rule** | Forthcoming APIs (4 blocks), single options shown alone (most of 64 *fragment* blocks), handler methods shown without their class (32 blocks), and Shouldly assertions (6 blocks). **D2, D3 and D4, ruled 2026-10-04**: a handler wrapper, two new skip reasons, and Shouldly pinned |

**Figures carried forward, re-derived:** BUILT / FAILED / SKIPPED **299 / 674 / 17** (`--report`
rows, `cut -f1 | uniq -c`); off-tranche **630 on 64** (`--classify` joined to
`probe/tranche_pages.py`'s 75 pages); `pagelint` **524 / 66** (its summary line). These match
`requirements.md` § *Current state* and `tools/README.md` rows 2 and 9.

## The Experiments

### E1 — the second compile over the 64 pages

**Method.** `probe/run.sh` runs 017's E1 (`run.sh`, `pages.py`) and E3 (`stubs.py`) at the current pin,
with `Order` excluded as 017 § *The tranches* rules, then joins the result to the 64 pages
(`probe/offtable.py`). Its verdicts: **PARSE**; **DEFECT** (a binder error once `using`s are
supplied); **BUILT** by a `using`; and STUB, split by mechanical stubs into **BUILT** (empty stub),
**MEMBERS** (the stub needs members), **HIDDEN** (a defect masked by a `dynamic` stub) and
**SAME-PAGE**. *Hard* = PARSE + DEFECT. *Reachable* = the two BUILTs + MEMBERS + HIDDEN, 017's
definition.

**Control.** The join over the **75** tranche pages gives **44** FAILED blocks, the figure 017 closed
on. The 64-page table is byte-identical across two runs from fresh directories (`diff` of the sorted
`offtable.tsv`, silent).

**Output, 630 blocks:**

| Verdict | Blocks |
|---|---:|
| BUILT by a `using` alone | **34** |
| BUILT by an empty stub | **69** |
| MEMBERS | **161** |
| HIDDEN | **71** |
| SAME-PAGE | **31** |
| PARSE | **145** |
| DEFECT | **119** |
| *Reachable* | **335** |
| *Hard* | **264** |

**Pages by hard blocks:** 0 → **2**; 1 → **8**; 2 → **17**; 3 → **8**; 4 → **4**; 5 or more →
**25**. Of the **36** off-tranche pages with nothing BUILT, **28** have a reachable block, and **11**
reach one by a `using` or an empty stub alone. The eight with nothing reachable are
`DispatcherConfigurationReference.md`, `ConfiguringOpenTelemetry.md`, `BrighterInboxSupport.md`,
`BrighterOutboxSupport.md`, `CausationTrackingStores.md`, `SwitchingSchedulers.md`,
`AzureServiceBusConfiguration.md` and `RabbitMQConfiguration.md`.

**What it changes:** 017's tranche rule (hard ≤ 1) holds **10** pages and 53 reachable blocks. The
other 54 pages hold 282 reachable blocks behind their hard ones. Tranching by hardness would leave
most of the reachable work for a 019.

### E2 — the 264 hard blocks, by the repair each needs

**Method.** `probe/hardcat.py`. DEFECT blocks are re-explained in their *after-`using`* state
(`stageP`), so a missing `using` cannot mask the error. Each is sorted by the first matching tag, in
this order: forthcoming → pin-gap → two-snippets → collision → wrapper → fragment → api. PARSE blocks
are sorted by their text. **A heuristic for sizing, not an instrument.** P0-2's `--classify` decides,
and AC7 reconciles the two.

| DEFECT (119) | Blocks | Repair rule |
|---|---:|---|
| **api**: a member, named argument or conversion the pinned release does not have | **50** | 017: verify at 10.7.0, repair at every recurrence, one ledger row |
| **wrapper**: `object` / `Context` has no `Handle`, `HandleAsync`, `Bag`, i.e. a handler member staged without its base class | **22** | built by the handler wrapper (D2) |
| **two-snippets**: `CS0128`, `CS0111`, a name declared twice in one fence | **15** | split the fence, as 016 and 017 did |
| **fragment**: `CS0161`, `CS0825`, `CS0116` and kin | **14** | 017: make whole, or skip with an accepted reason |
| **collision?**: `Task`, `Tag`, `User`, `Activity` and kin, a page type named like a pinned one | **7** | a page type, read by the receiver-aware resolution (D1) |
| **pin-gap**: Shouldly's `ShouldBe…`, not pinned | **6** | all on `TestDoubleOptions.md`; `Shouldly` is pinned (D4) |
| **forthcoming**: `OnceOnlyAction.Replay`, on `master` and not in 10.7.0 | **4** | SKIPPED, *forthcoming* (D3) |
| unexplained | **1** | read at the phase |

| PARSE (145) | Blocks | Repair rule |
|---|---:|---|
| **fragment**: a single option (`OnConflict = OnSchedulerConflict.Overwrite`), a bare expression, an attribute list | **64** | SKIPPED, *a single option shown alone*, where its type resolves in the pin (D3); otherwise made whole under 017's rule |
| **literal `...`** in code: `AddBrighter(...)` | **59** | 017: rewrite as `// ...`, and complete the statement |
| **leading `.` chain**: `.AddProducers(…)` | **12** | 017: a fragment |
| **types and statements** in one fence | **7** | split the fence, or reorder to top-level form |
| **not C#**: JSON IAM policies in a `csharp` fence (`AwsScheduler.md` #13, #14; `AzureScheduler.md` #19) | **3** | 017: retag. The corpus falls by 3 |

**Verified by reading, three per bucket** (`blockcheck --show`): `AwsScheduler.md` #13 is a JSON IAM
policy; `CommandProcessorConfigurationReference.md` #13 is `services.AddBrighter(...)`;
`KafkaConfiguration.md` #10 is `configHook: config => {…}`, a named argument shown alone;
`QueryPipeline.md` #12 is three attributes with nothing to decorate.

### E3 — the V4 pin, case, control and reverse

**Method.** `probe/v4pin.sh` copies `refs.csproj` with seven `PackageReference`s swapped for their
`.V4` twins, builds it (0 errors; 546 reference assemblies; `awssdk.sqs/4.0.100.7` against the main
pin's `3.7.500.5`), and compiles three sets against both pins.

| Set | Against the V4 pin | Against the main pin |
|---|---|---|
| **Case**: the six `CS0234 … V4` blocks | **0** `CS0234`. Four are left with **1** diagnostic each (`CS0103`, a value): `DistributedLock.md` #2, `DynamoDbDistributedLock.md` #1, #2, `S3LuggageStore.md` #1. `AwsScheduler.md` #2, #3 keep 6 and 14 missing names | **11** `CS0234` across the six |
| **Reverse**: the 15 BUILT blocks on AWS-family pages | **9 break**: `AWSSQSConfiguration.md` #1, #3–#6, `DynamoInbox.md` #1, `DynamoOutbox.md` #1–#3. All V3 namespaces | all build |

So the V4 pin cannot replace the main one, and a page cannot be selected by **the packages its prose
names**: `DynamoInbox.md` names the V4 package at `:21`, and its one block imports V3 and breaks
under V4. It is selected by **the namespace its C# blocks import**. On the four pages with a V4
block, every BUILT block (`AwsScheduler.md` #19, `DistributedLock.md` #1, `S3LuggageStore.md` #2)
still builds under V4. So **per-page selection is enough today**, and per-block selection (P2-3) is
not needed.

### E4 — what a by-name `using` gets wrong

**Method.** Over the off-tranche *import* rows of `--classify`, the names it took as import evidence
whose only pinned namespaces are third-party.

| Name | *import* rows | Pages | Resolved to | What the page means |
|---|---:|---:|---|---|
| `Order` | **22** | 10 | `StackExchange.Redis` | the page's own domain type (017 § *The tranches*) |
| `Context` | **17** | 5 | `Polly`, `Google.Api` | `RequestHandler<T>.Context`, an `IRequestContext?` property (`RequestHandler.cs:62`) |
| `Policy` | 3 | 2 | AWS, GCP model types | Polly's, or the page's own |
| `Tag`, `User` | 2 each | 1, 2 | AWS model types | page types |

Then **at the second compile**, 22 DEFECT blocks are `wrapper` and 7 are `collision?` (E2): **29 of
119**. 017 met the same blind spot and ruled it *not repaired* (2026-09-27). Each phase then carried
an `awk` exclusion and a reading rule (friction #70). If P0-2 builds the second compile on by-name
resolution, the tranche tables inherit 39 wrong *import* rows and 29 wrong DEFECT verdicts, and AC8
reads an instrument that misleads.

### E5 — handler methods shown without their class

**Method.** Members-shaped C# blocks containing an `override` of `Handle`, `HandleAsync`, `Execute`
or `ExecuteAsync`, over the whole corpus, by verdict.

**Output:** **32** FAILED, all off-tranche; 1 SKIPPED on a tranche page. They fail on `Context`,
`base.HandleAsync` and the like, because the `members` wrapper stages them in a `Holder` with no
base class (`tools/blockcheck.py:355–368`). This is the `wrapper` bucket of E2 from the other side.

## API Resolved For This Design

```bash
# PAIRED: the same 13 at both refs (52 attribute class names in total)
git -C ../Brighter grep -hoE 'class [A-Za-z]+Attribute' <ref> -- src | sed -E 's/class ([A-Za-z]+)Attribute/\1/' | sort -u > a
grep -E 'Async$' a | sed 's/Async$//' | sort -u | comm -12 - a
#   BulkDepositCallSite DeferMessageOnError DepositCallSite DontAckOnError FallbackPolicy FeatureSwitch
#   Monitor RejectMessageOnError RequestLogging UseInbox UsePolicy UseResiliencePipeline ValidateRequest
git -C ../Brighter grep -n 'class ConfigurationException' 10.7.0 -- src       # live, ConfigurationException.cs:32
git -C ../Brighter grep -n 'ValidatePipelines' 10.7.0 -- src                  # live, BrighterPipelineValidationExtensions.cs:58
git -C ../Brighter grep -nE 'class RequestHandler(Async)?<' 10.7.0 -- src/Paramore.Brighter   # live, :52 and :54
git -C ../Brighter grep -n ' Context' 10.7.0 -- src/Paramore.Brighter/RequestHandler.cs       # IRequestContext? Context, :62
git -C ../Darker grep -nE 'abstract class QueryHandler(Async)?<' 4.1.1 -- src  # live, QueryHandler.cs:9, QueryHandlerAsync.cs:9
git -C ../Brighter grep -n 'AddServiceActivator' 10.7.0 -- src                # DEAD: doc comments only, no definition
git -C ../Brighter grep -n -A14 'enum OnceOnlyAction' <ref> -- src            # Replay: absent at 10.7.0; FORTHCOMING on master (#4067)
ls ../Brighter/src | grep -i v4                                               # 8 dirs: 7 packages the pages name + a misspelt Tranformers.AWS.V4
```

`pagelint` functions this design extends are resolved by line in `tools/pagelint.py`: `APPLIES_TO`
`:164`, `OPT_OUT` `:215`, `check_code_blocks` `:475`, `check_terminology` `:806`, `main` `:1188`.
`blockcheck`'s are resolved in `tools/blockcheck.py`: `classify` `:254`, `SKIP_RE` `:169`, the
marker binding `:280–300`, `WRAPPERS` `:355`, `CLASS_ORDER` / `classify_failure` `:1198–1204`.

## Rule 8 — `ATTRIBUTE KIND`

**What it reads.** Every fenced block whose info string is `csharp`, `c#` or `cs`, through
`pagelint.Page`, so FAILED, SKIPPED and BUILT blocks alike, on every page `pagelint` walks. It does
**not** consult the banner's product. `PAIRED` names Brighter attributes only, and Darker 4.1.1 has no
async twins, so a Darker page cannot hit. That keeps rule 8 independent of rule 2.

**What it reports.** For an attribute line `[Name(` or `[NameAsync(` with `Name` in `PAIRED`, it skips
following attribute and blank lines to the decorated line. It reports when a sync attribute
decorates a line naming `HandleAsync`, or an `…Async` attribute decorates one naming `Handle(`. This
is the probe's `scan()`, moved and unchanged, so its 25 runs in 017 stand as its history. Message:

```text
contents/PipelineValidation.md:250: ATTRIBUTE KIND: [RejectMessageOnError] is the sync attribute, on HandleAsync. Use [RejectMessageOnErrorAsync], or mark the block <!-- pagelint: attr-mismatch-intended <reason> --> if the mismatch is the point
```

**Level.** Error, repo-wide and under `--changed`, per the ledger row below.

**`PAIRED`**, beside `APPLIES_TO`:

```python
# Brighter handler attributes that exist with and without the Async suffix. Re-derive with the
# version bump that edits APPLIES_TO, in ../Brighter at the new tag:
#   git grep -hoE 'class [A-Za-z]+Attribute' <tag> -- src | sed -E 's/class ([A-Za-z]+)Attribute/\1/' \
#     | sort -u > a; grep -E 'Async$' a | sed 's/Async$//' | sort -u | comm -12 - a
PAIRED = ('BulkDepositCallSite', 'DeferMessageOnError', 'DepositCallSite', 'DontAckOnError',
          'FallbackPolicy', 'FeatureSwitch', 'Monitor', 'RejectMessageOnError', 'RequestLogging',
          'UseInbox', 'UsePolicy', 'UseResiliencePipeline', 'ValidateRequest')
```

**The opt-out.** `<!-- pagelint: attr-mismatch-intended <reason> -->` binds to the next C# block
below it, as `blockcheck`'s skip does. It is reported in the same three ways that one is: a marker
with **no reason** is an error, a marker with **no C# block after it** is an error, and a **second**
marker on the same block is an error. Every honoured marker prints with its reason and a count,
`symbolcheck`'s convention: *"1 block marked attr-mismatch-intended"*. **The marker silences rule 8
only**, for that one block.

**`--plant`.** A mode, not a file: `pagelint` refuses paths outside `contents/` (exit 2), and a plant
file would loosen that. It runs in-memory cases and exits **0** only if every one behaves:

| Plant | Must |
|---|---|
| sync attribute on `HandleAsync` | hit |
| `…Async` attribute on `Handle` | hit |
| matched pair, sync on `Handle` | not hit |
| a mismatched block under a marker with a reason | not hit |
| a marker with no reason | report the marker |

The last two are new, beyond the probe's three, because the opt-out is new. The red-proof (AC1)
removes one plant's expectation and shows exit 1.

**CI.** `.github/workflows/docs.yml`, `check` job, after `python3 tools/pagelint.py`:
`- run: python3 tools/pagelint.py --plant`. It is bare, with no `|| true`.

**The page.** `contents/PipelineValidation.md`, the line above block 7's fence (`:247`):
`<!-- pagelint: attr-mismatch-intended the Before (error) example: a sync attribute on HandleAsync is the mistake this section teaches -->`.
The rule and the marker are in **one PR** (`tools/README.md`, rule 3).

**`CLAUDE.md`.** A ledger row, after rule 6's:

| Convention | Rule | Repo-wide | `--changed` |
|---|---|---|---|
| A handler attribute's kind matches its method's: sync on `Handle`, `…Async` on `HandleAsync` | 8 (`ATTRIBUTE KIND`) | error | error, unless the block is marked `attr-mismatch-intended` with a reason |

And **§ *Handler attributes match their handler***, under *Page Conventions* after *Complete code
blocks*. It is about 15 lines and has three parts:
1. **The rule and why the compiler cannot catch it.** Brighter throws `ConfigurationException` when
   it builds the pipeline, and `ValidatePipelines()` reports it at startup. A block can therefore
   compile, enter `blockcheck`'s baseline, and still be wrong
2. **`PAIRED` and where it lives.** Cite the tuple; do not list it
3. **The opt-out**, with `PipelineValidation.md`'s marker as the worked example, and why it is
   per-block: a page-wide marker would have hidden `#9` and `#10` beside the deliberate `#7`

**`tools/README.md`.** Row 2's ref and description; *What each gate actually checks*, `pagelint`'s
bullet, gains rule 8; *The other modes* gains `python3 tools/pagelint.py --plant`.

**Retiring the probe.** `spec/017-compile_repairs/probe/attr_mismatch.py` stays in place as 017's
history. No 018 task names it as an instrument (AC5).

## The Second Compile — `--classify`

**What changes.** `classify` (`tools/blockcheck.py:254`) gains the probe's second stage. For every
FAILED block that is not *parse*, it supplies the `using` directives the pinned type table resolves,
recompiles, and classifies what remains. The first-compile class is kept.

**Output.** Still one row per FAILED block, sorted, and still exit 2 on nothing to classify. It now has
five columns:

```text
page<TAB>ordinal<TAB>class<TAB>first<TAB>names
```

`class` is the second-compile class and `first` is today's class. That keeps 017's lower bound beside
the true figure, as open question 2 recommended. Nothing parses the four columns today
(`git grep -n -- '--classify' tools .github` → only `blockcheck` and `tools/README.md`).

**Classes**, in `CLASS_ORDER`:

| Class | Means | Repair rule (017 § *Page Repair Rules*) |
|---|---|---|
| `parse` | does not parse as staged | as 017 |
| `built-by-using` | builds once given its `using`s | the `using`s, in the block |
| `defect` | after the `using`s, a binder error that is not a missing name | verify and repair |
| `same-page` | after the `using`s, only names another block on the page declares | FAILED, listed |
| `values` | after the `using`s, only lower-case missing names | a value stub |
| `page-type` | after the `using`s, a capitalised name no pin ships and the page never shows | a type stub |

**Resolution is receiver-aware (D1).** A name is resolved to a pinned type only when it
is not something the block or its page already supplies:
- it is **not declared on the page** (`TYPE_DECL_RE` over the page's blocks, as `same-page` already
  does), which removes `Order`, `Task`, `Tag` and `User`
- it is **not a member of the staged class's base**, which removes `Context`. The handler wrapper
  (D2) makes the base known. A members block it does not wrap, because its signature names no request
  type, has `Context`, `Bag` and `base.Handle…` read as wrapper artefacts: classed `defect` with a
  note, never `built-by-using`

The pinned type table is read by `Program.cs --types`, which exists (017 phase 1). No new C# mode is
needed: the recompile is the existing `--explain` over a rewritten stage.

**Red-proof (AC6).** One recorded run over five named blocks, each with a known answer. A
`built-by-using` block (one of the 34); a `defect` behind a `using` (one of the 50 *api*); a
`page-type` block; a `parse` block; and the **control**: a block that page-declares a type also
pinned elsewhere (an `Order` block), which must **not** come out `built-by-using`. A second run
with the page-declared check disabled must turn the control `built-by-using`, which shows the
check is what decides it.

**Reconciled (AC7).** A block-by-block join of the new `--classify` against `probe/run.sh`'s
`verdicts.tsv` at the same ref. Every disagreement is listed with its reason. The expected
disagreements are E4's 39 and 29, each read by the probe by name and by `--classify` by receiver.

## The V4 Pin

```text
tools/blockcheck/
├── refs/refs.csproj           unchanged: 100 packages, V3 AWS
├── refs-v4/refs-v4.csproj     new: refs.csproj with the 7 AWS packages swapped for .V4
└── scaffold/pages.tsv         gains a third column: `v4` on the pages whose blocks import a .V4 namespace
```

- **Selection is per page**, by a `v4` cell in `pages.tsv`, the file that already maps pages to their
  scaffold. The rule, enforced by `--report` beside the unit rule: a page marked `v4` has a C# block
  importing a `.V4` namespace, and a page with such a block is marked `v4`. Today that is **4 pages**:
  `AwsScheduler.md`, `DistributedLock.md`, `DynamoDbDistributedLock.md`, `S3LuggageStore.md`
- **`--report` prints a fourth scope line**: `N reference assemblies (v4 pin)`, beside the main
  pin's, because a pin that did not restore must not shrink silently (`tools/README.md` § *Reading a
  number before you trust it*)
- **CI's `blocks` job** gains one build step: `dotnet build tools/blockcheck/refs-v4/refs-v4.csproj -c Release`.
  Without it the gate exits 2, the same contract as the main pin
- **Its comment** says why it exists, citing `refs.csproj:191–192`, and that its versions move with
  `refs.csproj`'s Brighter version in the same edit
- **The baseline key is unchanged.** A block is still (page, ordinal); the pin is a property of the
  page, as the scaffold unit is

**Expected verdict movement:** none at phase 2. Four of the six need a value stub, and two need more
(E3). Each is repaired in the phase that holds its page's section: `AwsScheduler.md` #2, #3 in S1;
`S3LuggageStore.md` #1 in S4; `DistributedLock.md` #2 and `DynamoDbDistributedLock.md` #1, #2 in S5.
The last four are on 017 tranche pages, listed in 017 § *Blocks that stay FAILED* as waiting for
"D3, 018", so S4's and S5's tables carry them beside the off-tranche pages.

## Page Repair Rules

**017 `design.md` § *Page Repair Rules* and § *Scaffold Stub Rules* apply unchanged**, and are cited
rather than restated. Every FAILED block on a reached page leaves in one of four states: BUILT,
SKIPPED with an accepted reason, FAILED and listed, or retagged. Rule 8 of that section, *"`probe/attr_mismatch.py`
runs in every tranche PR"*, is replaced by rule 8 the gate, which now runs on every push.

**What 018 adds**, ruled 2026-10-04:

| Case | Blocks (E2/E5) | Rule | Ruling |
|---|---:|---|---|
| A forthcoming API, which the page already says ships after the pinned release | 4 | SKIPPED, reason *"forthcoming: ships after Brighter 10.7.0, as the page says at line N"* | **D3** |
| A single option, argument or expression shown alone, with no enclosing call on the page | ≤ 64 | SKIPPED, reason *"a single option shown alone; its type is named in the sentence before it"*, where that type resolves in the pin | **D3** |
| A handler method shown without its class | 32 | built by the handler wrapper | **D2** |
| Shouldly assertions | 6 | `Shouldly` pinned in `refs.csproj` | **D4** |
| `AddServiceActivator` as current code | 2 sites | rewrite to `AddConsumers`, add a `symbolwatch.tsv` row, and opt out the two V9 discussion pages | **D5** |

**And from 017's friction, as standing obligations in `tasks.md` § 1** (P0-6): #68 (both reading
criteria at every phase close), #69 (probes committed here), #72 (the off-tranche term predicted),
#73 (repair at every recurrence; only scope goes to the maintainer; a *second pass* task per
phase), #74 (a ledger row only after its grep has run), #79 (hits named by page and block).

## Target And Phases

### The section groups

From E1, grouped by `SUMMARY.md` section so that a page family is repaired together. The
schedulers share a structure, with five *parse* blocks each on four of them:

| Group | Sections | Pages | FAILED | Reachable | Hard (parse + defect) | Nothing BUILT |
|---|---|---:|---:|---:|---:|---:|
| **S1** | Scheduler | 8 | 129 | 81 | 29 + 16 | 3 |
| **S2** | Commands, Handlers and Pipelines; Get Started | 10 | 107 | 66 | 15 + 23 | 7 |
| **S3** | Darker; Understanding Brighter | 10 | 137 | 75 | 31 + 20 | 6 |
| **S4** | Using an External Bus; Transports | 16 | 124 | 59 | 39 + 19 | 10 |
| **S5** | Brighter Configuration; V10 Migration; Outbox and Inbox; Reference; Health Checks and Observability | 20 | 133 | 54 | 31 + 41 | 10 |
| **Total** | | **64** | **630** | **335** | **145 + 119** | **36** |

**These are working groups, not the tranche tables.** AC8 requires the tables to be the committed
instrument's, so each group's page table is drawn in `tasks.md` from `--classify` at phase 2's close,
with the command beside it.

### The targets

- **BUILT ≥ 560 at the close.** That is 299 + 264 (the reachable blocks less the 71 HIDDEN, whose
  stubs mask the real error), with nothing assumed from the 264 hard blocks. 017 set its target the
  same way and beat it, because hard repairs and recurrences added blocks a reachable count cannot
  see
- **Pages with nothing BUILT: 39 → ≤ 14.** Three of the 39 are tranche pages:
  `ReturningResultsFromAHandler.md`, `ReplayOnSeenReference.md` (forthcoming, D3) and
  `DynamoDbDistributedLock.md`, which the V4 pin can reach (below). None is counted on. Of the 36
  off-tranche pages, 28 have a reachable block. 39 − 25 = 14, so **25 of those 28** must gain a BUILT
  block. The eight with nothing reachable are not counted on
- **Every FAILED block on the 64 pages** leaves in one of the four states, and is named in 018
  § *Blocks that stay FAILED* if it stays (AC13)

### Phases

Each phase is **one PR** (`tools/README.md` § *One phase is one pull request*).

```text
Phase 1  rule 8                          one page: the marker on PipelineValidation.md #7 — sign-off asked
  ├─ tools/pagelint.py: PAIRED, ATTRIBUTE KIND, the opt-out and its three faults, --plant
  ├─ .github/workflows/docs.yml: pagelint --plant
  ├─ CLAUDE.md: the ledger row; § Handler attributes match their handler
  └─ tools/README.md: row 2, the pagelint bullet, the other modes
Phase 2  instruments                     no page changes
  ├─ --classify: the second compile, five columns, the receiver-aware read (D1)
  ├─ refs-v4.csproj, pages.tsv's v4 column and its rule, the scope line, the CI build step
  ├─ the handler wrapper (D2); Shouldly in refs.csproj (D4)
  ├─ P1: the `statements` Program (#75), a non-compiling unit as a finding (#77), pagelint's per-page count (#80)
  ├─ P1-3: the net10.0 measurement, recorded before any table is drawn
  └─ tasks.md: the S1–S5 page tables, drawn from --classify (AC8)
Phase 3  S1 — Scheduler                  + D5's AddServiceActivator repair and watchlist row
Phase 4  S2 — Commands, Handlers and Pipelines; Get Started
Phase 5  S3 — Darker; Understanding Brighter
Phase 6  S4 — Using an External Bus; Transports   + S3LuggageStore.md #1 (V4)
Phase 7  S5 — the remaining five sections        + DistributedLock.md #2, DynamoDbDistributedLock.md #1, #2 (V4)
Phase 8  close                           acceptance walk, ledgers, § What 018 shipped, the residual for 019
```

**Phase 2 changes no page, but it may move verdicts.** D2's wrapper and #75's `Program` change how
blocks are staged, so blocks may build with no page edit. Any that do enter `baseline.tsv` in phase
2's PR, because the gate requires equality. Phase 2's first task measures that set before writing a
row (§ *Gate Movement Predicted*).

## Gate Movement Predicted

Each gate's corpus is as `tools/README.md` § *What each gate actually checks* states. Figures are
cited from its rows, not restated.

| Gate | Phase 1 | Phase 2 | Phases 3–7 | Why |
|---|---|---|---|---|
| `linkcheck` | **unmoved** | **unmoved** | **unmoved**, unless a repair adds a link | no new `.md` under its walk: `spec/` is excluded, and `--plant` is a mode, not a file |
| `pagelint` errors | **0, unmoved** | 0 | 0 | rule 8 lands with the marker that satisfies it |
| `pagelint` warnings | **unmoved**: the marker is an HTML comment, and no block's text changes | **unmoved** | **fall**, by the blocks each phase gives their `using`s; predicted per phase from its table | the debt is per block |
| `pagelint --plant` | **new, exit 0** | 0 | 0 | |
| shape / redirects / `--verify` | **unmoved** | **unmoved** | **unmoved** | no `SUMMARY.md` change |
| `versioncheck` | **unmoved** | **unmoved** | **unmoved** | no tutorial pin touched. A repair that adds one is predicted in its phase |
| `optioncheck` | **unmoved** | **unmoved** | **unmoved** | no option table touched |
| `symbolcheck` | **unmoved** | **unmoved** | phase 3: **entries 22 → 23**, and **silenced** rises by the opt-out sites on `V10MigrationGuide.md` and `FAQ.md` (D5); otherwise unmoved | the `AddServiceActivator` row and its opt-outs |
| `blockcheck` corpus | **unmoved, 990** | **unmoved** | **moves both ways**: retags (−3 for E2's JSON) and fence splits (+ for E2's 15 two-snippet and 7 mixed blocks); predicted per phase | a split adds a block |
| `blockcheck` BUILT | **unmoved, 299** | **rises by the blocks the handler wrapper (D2), the `Program` fix (#75) and the Shouldly pin (D4) make build**, measured by phase 2's first task before any row is written. Up to 32 + 6, less those that fail for another reason | **rises**, to ≥ 560 by the close | |
| `blockcheck` scope lines | unmoved | **+1 line**, the V4 pin's assemblies; the main pin's assemblies **rise** by Shouldly's (D4); the scaffold rule also checks the `v4` column | units rise with each phase's stubs | |

## Design Decisions

D1–D5 were ruled by the maintainer on 2026-10-04, each as recommended. D6 and D7 needed no ruling.

| # | Question | Decision | Ruled |
|---|---|---|---|
| **D1** | **Promote P1-1, the receiver-aware `--classify`, into P0-2?** It changes approved scope | **Yes.** E4: 39 off-tranche *import* rows and 29 of 119 second-compile DEFECTs are misreads by name. AC8 asks the tranches to be the instrument's, and a by-name instrument is wrong on about one block in nine. 017's *not repaired* ruling was taken before the second compile existed, when the cost was a reading rule; now it is a wrong table | **Yes**, the maintainer, 2026-10-04 |
| **D2** | **A handler wrapper for members blocks that override `Handle`, `HandleAsync`, `Execute` or `ExecuteAsync`?** The `Holder` derives from `RequestHandler<T>`, `RequestHandlerAsync<T>`, `QueryHandler<TQ, TR>` or `QueryHandlerAsync<TQ, TR>`, with the type arguments read from the signature | **Yes.** 32 blocks, all off-tranche. The alternative is 32 page edits that wrap a method in a class the reader did not need to see. It is the same kind of help the `members` wrapper already gives, and it is enforced the same way: a block whose signature names no request type is not wrapped | **Yes**, the maintainer, 2026-10-04 |
| **D3** | **Two new accepted skip reasons:** *forthcoming, as the page says*; and *a single option shown alone, its type named in the sentence before it* | **Yes to the first.** 4 blocks; the page already tells the reader, and the gate should not argue. **Yes to the second, narrowly**: the type must resolve in the pin, so the skip cannot hide a dead name. The rest of the 64 fragments are made whole under 017's rule | **Agreed**, the maintainer, 2026-10-04 |
| **D4** | **Pin Shouldly?** | **Yes**: one package, 6 blocks, all on `TestDoubleOptions.md`, a package the page's reader installs | **Yes**, the maintainer, 2026-10-04 |
| **D5** | **`AddServiceActivator`:** rewrite the two sites to `AddConsumers`, and add a `symbolwatch.tsv` row, with per-symbol opt-outs on `V10MigrationGuide.md` and `FAQ.md`, which discuss the V9 name | **Yes, in phase 3** (S1 holds both sites). 015's triage is the precedent for a watchlist row with opt-outs | **Yes**, the maintainer, 2026-10-04 |
| **D6** | **Is the V4 pin's selection per page enough?** | **Yes, today.** E3: every BUILT block on the four V4 pages builds under V4. Per-block selection stays P2-3, triggered only by a page that mixes V3 and V4 blocks | — |
| **D7** | **Raise P1-3, the `net10.0` measurement, to P0?** | **No, but run it in phase 2 before the tables are drawn**, as friction #76 asks. Raise it if it moves a verdict | — |

## Design Quality Checklist

- [x] Readable with no prior context: it opens with the subject, the deltas and the experiments, and
      every verdict word is defined in E1
- [x] No page is created, so page types, banners and headings are **N/A**, marked so. The one new
      prose section (`CLAUDE.md`) is outlined
- [x] Every API named is resolved, with the command: `PAIRED`, `ConfigurationException`,
      `ValidatePipelines`, the handler base classes, `Context`, `AddServiceActivator` (dead),
      `OnceOnlyAction.Replay` (forthcoming)
- [x] Every number has its command, or the committed probe that produced it. The one unmeasured
      figure, the BUILT movement from D2 and #75, is named, and phase 2 measures it first
- [x] Experiments have two-way controls: E1 against 017's 44; E3's case, control and reverse; AC6's
      `Order` control
- [x] Gate movement predicted for all nine gates plus `--plant`, including "unmoved"
- [x] Structure shown as trees and tables
