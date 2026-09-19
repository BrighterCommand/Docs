# Spec 015: The Triage Method

**Written:** 2026-09-19, phase 2 · **Deliverable D3** · **Satisfies AC4**

> **This is not a documentation page.** It lives under `spec/`, it is outside `pagelint`'s and
> `linkcheck`'s corpora, and it is read by the maintainer rather than by a reader of the site. It
> carries no banner and no `description:` front matter for that reason.

This document is the method `--census`'s output is triaged by: what a verdict may say, what
evidence each one rests on, what the evidence costs, and when the method is finished. Sections 1 to
4 are written here in phase 2; the record of running them over all 819 names is phase 3's, and it
appends to this file rather than replacing it.

**Every figure below is measured at the census's pinned SHA pair** — `../Brighter` `09f5d988f`,
`../Darker` `2f76cda`, the two SHAs `CENSUS_PINS` in `tools/symbolcheck.py` records and
`--census` prints in its header. That is standing obligation 9, and it is not a formality: a
name's history is a property of a ref, so a verdict taken at one SHA and a census taken at another
are two claims about two worlds.

## 1. The vocabulary — three verdicts, and why two were not enough

A candidate leaves the triage with exactly one of three verdicts.

| Verdict | The test | What it means | What phases 4 and 5 do with it |
|---|---|---|---|
| **LIVE** | resolves at either ref of either product | **the census missed it** — a defect in the instrument, not in the page | fix the instrument; the page is innocent |
| **EXISTED, REMOVED** | resolves nowhere, but the name appears in `src/` history | **provisional** — see below | a watchlist row and a page repair, **once stage 3 confirms it** |
| **NEVER EXISTED** | resolves nowhere and has never appeared in `src/` history | invented domain, a misremembered name, or another library's | no row, no repair — but it is **counted**, not discarded |

The requirements talk about names that are *dead*. History splits that word into two populations
that want opposite treatment, and the split is mechanical rather than a matter of judgement:

```bash
git -C ../Brighter log -S'IMessageScheduler'     --oneline 09f5d988f | wc -l   # 0
git -C ../Brighter log -S'IAmAMessageScheduler'  --oneline 09f5d988f | wc -l   # 17  (live)
git -C ../Brighter log -S'IAmACommandStoreAsync' --oneline 09f5d988f | wc -l   # 18  (removed)
```

**`IMessageScheduler` — 014's one real discovery — has never existed in Brighter at any point in
its history.** It was not an API the product dropped; it was a plausible name the documentation
asserted. A two-valued vocabulary records it identically to `IAmACommandStoreAsync`, and the two
are not the same defect: one is the product changing under the documentation, the other is the
documentation inventing something. **The repair 014 made was right and its stated reason was one
class off**, which is the case for the third value.

### 1.1 `EXISTED, REMOVED` is provisional, and it says so before the run rather than after

> **The pickaxe reports a name leaving the source. It cannot report whose name it was.**

`git log -S` answers *did the count of this string in the tree change in this commit?* A name that
Brighter's own source **used and later stopped using** answers yes exactly as loudly as a name
Brighter **published and later removed**. The pilot ran into this head-on:

```bash
git -C ../Brighter log -S'[^A-Za-z0-9_]GetSection[^A-Za-z0-9_]' --pickaxe-regex -p \
    09f5d988f -- 'src/*.cs' | grep -E '^[+-].*GetSection' | head -1
#  -   var configSection = ConfigurationManager.GetSection(BrighterConfigSectionName) as …
```

against the planted positive:

```bash
git -C ../Brighter log -S'[^A-Za-z0-9_]IAmACommandStoreAsync[^A-Za-z0-9_]' --pickaxe-regex -p \
    09f5d988f -- 'src/*.cs' | grep -E '^[+-].*IAmACommandStoreAsync' | head -1
#  -   private readonly IAmACommandStoreAsync _commandStore;
```

One is a call on somebody else's type; the other is a type in declaration position. **`EXISTED,
REMOVED` therefore means *this name left the product's source*, never *the product removed this
API*** — and the second claim is the one a watchlist row makes. Promoting the first to the second
is stage 3's job and stage 3 is a person, by construction rather than by under-investment: the
boundary here is the one D12 drew and the 757 using-directive blocks sit on. **Only a compiler
resolves a reference.**

**Do not let the provisional verdict harden by being written down.** Every row phase 3 emits
carries the verdict *and* the evidence it was taken from, so a later reader can see which of the
two claims was actually established.

### 1.2 There is a third reading, and the pickaxe cannot see past it either

**`git log -S` searches the raw diff text. Comments and string literals are in it.** The census
does not work that way — `strip_noncode()` takes comments and strings out of the documentation
side before a token is ever nominated — so the two halves of the method disagree about what counts
as a use, and only the history half is permissive.

Measured on `Date`, 2026-09-19: **20 of its 26 whole-word history occurrences carry a quote on the
line**, and the visible ones are `[DynamoDBHashKey("Command+Date")]` and
`AttributeName = "Topic+Date"`. A verdict resting on those alone would mean *this name was once a
substring of a string literal in the product's source*, which is a third thing again, and not one
any watchlist row should assert.

### 1.3 Read the whole evidence set, never its first line

`design.md` §10.6 read the seven head survivors with `… | head -1`. On `Date` that first line is
`Get<T>(DateTime date, …)` — a parameter name, and the table concludes *Own API? no*. The **full**
set of 26 also contains:

```text
-        public DateTime Date { get; set; }
```

a public property of `DynamoDbMessage` in `src/Paramore.Brighter.Outbox.DynamoDB/`, removed in
2019. **Same name, same query, opposite readings — and only the truncation decided which one a
person saw.**

This does not overturn the design's verdict on `Date`, and phase 3 may well clear it again. What it
overturns is the *reason*: `Date` clears because the documentation's `Date` is a token of its own
example domain, **not** because Brighter never had a public `Date`. Brighter did. Under
[§1](#1-the-vocabulary--three-verdicts-and-why-two-were-not-enough)'s vocabulary the reason is the
verdict, so a right answer reached through a wrong reason is a row waiting to be re-litigated.

**`probe/triagerun.py` therefore keeps up to 40 distinct evidence lines per name per product**, and
prints the `dotted`/`bare` totals over *all* matches beside them, so a reader can always see
whether the lines in front of them are the whole set.

## 2. The stages — four of them, and only the last one decides anything

The honest query costs roughly eleven times the cheap one, so the method screens cheaply and pays
only for survivors. **Measured at the pin, 2026-09-19**, both products, wall clock per name:

| Stage | What it asks | Brighter | Darker | Both, per name |
|---|---|---:|---:|---:|
| **1** | has this string *ever* been in `src/`? | 0.48s | 0.04s | **≈ 0.52s** |
| **2** | …as a whole identifier, not a substring? | 5.6s | 0.14s | **≈ 5.75s** |
| **live** | does it resolve at the pin today? | 0.06s | 0.04s | ≈ 0.10s |

**The two products do not cost the same, and the design's estimate assumed they did.** It budgeted
stage 2 at `99 × 2 × 5.4s ≈ 18 min` by charging Darker Brighter's price; Darker's history is a
fortieth of the size, so the real figure is `99 × 5.75s ≈ 9.5 min`. The budget phase 3 works to is
the one in this table, not the one in `design.md` §5.

**End to end, as `probe/triagerun.py` actually runs it** — which is the figure phase 3 budgets
from, because it includes the live query at all four refs that the design costed at nothing.
Measured over a 60-name pilot, 2026-09-19:

| | Names | Per name | |
|---|---:|---:|---|
| cleared by stage 1, no stage 2 | 47 | **0.66s** | one `-S` and four `grep -lwF` per name |
| went on to stage 2 | 13 | **5.76s** | the above, plus the bracket-class form per product |
| **projected over the census** | **819** | — | `720 × 0.66s + 99 × 5.76s` ≈ **17.4 min** |

The 720/99 split is `design.md` §10.5's measurement, not this pilot's — the pilot takes the top 60
by page-spread, and the head is not representative of the tail by construction
([§4](#4-the-stopping-condition)). **Phase 3 re-derives the split; only the per-name costs above
are phase 2's to hand over.**

### 2.1 Stage 1 — the cheap screen, conclusive in one direction only

```bash
git -C ../Brighter log -S"$name" --oneline 09f5d988f -- 'src/*.cs' | wc -l
```

Plain `-S`, substring semantics, no regex. **A zero here is conclusive**: if the identifier ever
existed in `src/`, its substring existed too, so a substring miss cannot be hiding a real name.
The converse is not true — `Date` returns 118 because of `UpdateDate` — which is why a *non*-zero
means only *go to stage 2*.

**The asymmetry is the whole design.** Stage 1 is conservative in the one direction that matters,
and that is what makes it safe to clear several hundred names in seven minutes and never look at
them again.

### 2.2 Stage 2 — the honest form, on survivors only

```bash
git -C ../Brighter log -S"[^A-Za-z0-9_]$name[^A-Za-z0-9_]" --pickaxe-regex \
    --oneline 09f5d988f -- 'src/*.cs' | wc -l
```

The bracket class is not a stylistic choice over `\b`. See [§3](#3-the-query-forms--and-the-two-that-are-broken):
the two obvious alternatives return zero for **every** name, including a positive control that must
be non-zero.

### 2.3 Stage 2.5 — evidence for the person, never a filter

For each stage-2 survivor, whether its historical occurrences are preceded by a `.`. That is the
tell between `ConfigurationManager.GetSection(…)` and `private readonly IAmACommandStoreAsync`,
and it is **printed, never applied**:

> A Brighter extension method is called with a dot too. Every cheap filter available here is a
> **name** filter, and names collide — which is what D12 established and `bclprobe` demonstrated by
> striking `Date`, `Email`, `Total` and `Product`, the documentation's own invented domain.

**A filter has to be right about every name; evidence on a screen only has to be relevant.** That
is the reason `bclprobe`'s output is rejected as a filter and adopted as an input to this stage.

### 2.4 Stage 3 — a person, and what they are allowed to conclude

Read the sites and read the diffs. Decide *the product's own public surface* against *a name the
product's source merely contained*, and write the verdict down with its evidence and its control.

**Clearing a name because it "looks like an example" is not a verdict**, it is the reasoning that
left `ConfigureBrighter` unexamined for a year, and the requirements name it as the criterion that
fails quietly. Standing obligation 8 applies to every row: evidence, and a control.

## 3. The query forms — and the two that are broken

Three ways of asking *as a whole word* were tried. **Two of them are broken instruments** that
return a clean, confident, entirely false answer:

| Form | `Date` | `IAmACommandStoreAsync` — **must stay > 0** | |
|---|---:|---:|---|
| `-S'Date'` — substring | 118 | 4 | matches `UpdateDate`; unusable as a verdict |
| `-S'\bDate\b' --pickaxe-regex` | **0** | **0** | **broken** — `\b` is unsupported, so every name reads as never-existed |
| `-S'\<Date\>' --pickaxe-regex` | 0 | **0** | **broken** the same way |
| `-S'[^A-Za-z0-9_]Date[^A-Za-z0-9_]'` | 5 | **2** | **works**, and the control survives |

> **The `\b` form returns zero for every name including the positive control**, so without a
> control that *must* be non-zero it reads as *"no candidate has ever existed"*. Plausible zero
> number eight, and the second this programme has found in a word-boundary flag.

**Do not simplify the bracket class back to `\b`.** The two forms look equivalent and one of them
silently empties the census. The red-proof recorded in `tasks.md` § *Phase 2 as executed* is the
only thing that stops a later reader making that edit.

### 3.1 Why `-lwF` is safe in the live query when `-w` was not safe elsewhere

```bash
git -C ../Brighter grep -lwF "$name" 09f5d988f -- 'src/*.cs' | wc -l
```

`-w` tests the characters *adjacent to the match*, so it is correct exactly when the pattern begins
and ends with word characters. A census candidate is an identifier and qualifies. `.AddPolicies(`
is a watchlist row, is not an identifier, and does not — which is the case `word_pattern()` in
`tools/symbolcheck.py` exists to handle. **The rule is a property of the pattern, not of the flag**,
and the two instruments disagree about what a non-word edge means.

### 3.2 The runner does not go through a shell, and that is load-bearing

`probe/triagerun.py` builds every one of these as a `subprocess` argument list. Running the stage-2
form through an interactive shell instead is **plausible zero number nine**, found while measuring
the costs in [§2](#2-the-stages--four-of-them-and-only-the-last-one-decides-anything):

```bash
# zsh, with the name in a variable — returns 0 for EVERY name, including the control
git -C ../Brighter log -S"[^A-Za-z0-9_]$name[^A-Za-z0-9_]" --pickaxe-regex --oneline "$sha" …
# (eval):1: bad math expression: operand expected at `^A-Za-z0-9...'
```

**`$name[` is an array subscript in zsh**, so `$name[^A-Za-z0-9_]` is parsed as a subscript and
evaluated as arithmetic. The command dies, `wc -l` reports `0`, and — because the error goes to
stderr and the count goes to stdout — a loop that captures only the count sees a tidy column of
zeros. The literal, single-quoted form in `design.md` §4.3 has no `$` in it and is unaffected,
which is why this survived the design review: **the trap is in the parameterised form, and the
parameterised form is the only one that can be run 819 times.**

## 4. The stopping condition

> **The method stops when every name in the census has a verdict.** Not after N unproductive
> names, and not when a budget runs out.

AC4 requires a stopping condition, and this one is exhaustive rather than heuristic. It is
affordable because of the shape measured in [§2](#2-the-stages--four-of-them-and-only-the-last-one-decides-anything):
stage 1 clears most of the census for about seven minutes of machine time, and stage 3 — the only
expensive stage, because it is a person — sees only what survives stages 1 and 2.

**A heuristic condition was drafted and rejected.** *"Stop after 20 consecutive NEVER EXISTED"* is
cheap, plausible, and wrong here for a reason specific to this corpus:

> **Page-spread ordering buries exactly what it was chosen to surface.** What repeats across many
> pages is the documentation's *own example domain* — `OrderId` on 31 pages, `CustomerId` on 17 —
> because every page needs a domain to show. **A removed API is mentioned where it was relevant,
> which is once or twice.** So the ordering ranks invented domain highest and dead product surface
> lowest, and any run-length rule stops before reaching the part of the list most likely to hold a
> real name.

That is friction 45, and it is not hypothetical: the head of the list contains **no Brighter API**
and the 1–2 page tail contains at least twelve. **An ordering chosen to surface signal early is
exactly the ordering a run-length stopping rule misreads.**

**Keep the ordering; never let it decide what gets skipped.** Page-spread is how the report is
read — it is what found `IMessageScheduler` at 7 pages — and it remains the order phase 3 works in.
What it is not is a budget.

### 4.1 What "every name" means, and what sits outside it

The census is the corpus, so the stopping condition inherits the census's own boundaries and
nothing else. Those boundaries are `design.md` §9 and they are **not** a backlog this method is
quietly deferring:

- names inside a page's own declarations, which `DECL_RE` and `MEMBER_DECL_RE` forgive per page
- names in the `NOISE_EXACT` and `NOISE_PREFIX` sets
- names outside C# fences — **prose is not censused**, which is exactly where the twelve dead APIs
  also appear, and is why phase 4's repair pass reads prose as well as fences
- anything below `TOKEN_RE`'s four-character floor

**A name the census never nominated cannot get a verdict here**, and saying so is the difference
between a bounded method and one that claims more than it checked.

### 4.2 The two planted positives stay in, now that the corpus yields

`IAmACommandStoreAsync` and `UseExternalInbox` are planted into every run and both must classify
**EXISTED, REMOVED**. They were added when a zero was the expected outcome — a method that returns
zero on its only run is indistinguishable from a method that cannot return anything, and this
programme has found nine plausible zeros. The census now yields twelve real names, and the plant
stays anyway:

- **Twelve positives somewhere in 819 rows does not show that row 400 was classified by a working
  instrument.** The plant does, because it is checked in the same run as its neighbours.
- **A yield is not a proof of sensitivity.** A method that finds `UseMsSqlOutbox` may still be blind
  to a whole class of name, and the plant tests the *mechanism* rather than sampling the *corpus*.

This is standing obligation 3 with the positive case sourced from **outside** the corpus, which is
friction 36 — and friction 44 is the check that goes with it: **a control must be able to pass at
all.** Both planted names are verified present in history and absent from `src/` at both refs on
every run, so a plant that stopped being a plant would be caught rather than read as a pass.

## 5. The record — the method run over the whole census

**Written 2026-09-19, phase 3.** Sections 1 to 4 are the method; this is what it returned. It
appends rather than replacing: a record that edited the method it was produced by would leave
nobody able to tell which version of the method produced which row.

### 5.1 How to read a row, and what produced it

**Every row below is machine output, and §§5.2–5.3 are generated rather than typed:**

```bash
python3 spec/015-census_triage/probe/triagerun.py            # the run: one JSONL row per name
python3 spec/015-census_triage/probe/triagetable.py --check  # the counts, and the ruling check
python3 spec/015-census_triage/probe/triagetable.py          # regenerates §§5.2–5.3
```

A table of 819 hand-copied rows is a table nobody can re-derive, and obligation 1 asks for the
command beside the figure. So the record is **regenerated from `triage-checkpoint.jsonl`**, which is
committed beside it, and the generator **refuses to emit a section at all** when a stage-2 survivor
has no stage-3 ruling — in both directions, so a ruling naming something this run did not produce is
refused too.

**The two commands behind every row are these, with `$name` substituted**, at the pin, `src/*.cs`
only. They are given once rather than copied 819 times: a row's numbers *are* their output, and the
copies would be the only part of the row a reader could not check.

```bash
name=<the row's name>                                      # stage 1: is it anywhere, ever?
git -C ../Brighter log -S"$name" --oneline 09f5d988f -- 'src/*.cs' | wc -l

                                                           # stage 2: as a whole identifier?
git -C ../Brighter log -S"[^A-Za-z0-9_]$name[^A-Za-z0-9_]" --pickaxe-regex \
    --oneline 09f5d988f -- 'src/*.cs' | wc -l

                                                           # live? the census guarantees 0
git -C ../Brighter grep -lwF "$name" 09f5d988f -- 'src/*.cs' | wc -l
```

**Run them from a shell and read
[§3.2](#32-the-runner-does-not-go-through-a-shell-and-that-is-load-bearing) first**: written exactly
as above under `zsh`, the stage-2 form is an array subscript and returns a confident zero for every
name including the control. The runner builds them as argument lists for that reason. Each is shown
here for Brighter; the row's `B/D` columns are the same query at `../Darker` `2f76cda`.

**The controls are the run's, not each row's, and that is the honest way round.** Every row in this
record was produced by one invocation of one instrument, so what a per-row control would test is the
same instrument 819 times. What it is checked against is printed at the head of every run, before
any row is written:

| | Control | Required | What it rules out |
|---|---|---|---|
| positive | `IAmACommandStoreAsync` | `EXISTED, REMOVED` | a stage-2 query that cannot find a known-removed API — the `\b` trap and the zsh subscript both land here |
| negative | `IAmAMessageScheduler` | `LIVE` | a live query stuck at zero, which would read every name as dead |
| negative | `OrderId` | `NEVER EXISTED` | a classifier that calls everything removed |
| positive | `UseExternalInbox` | `EXISTED, REMOVED` | as the first, from a second name outside the corpus |

**Three distinct verdicts are required across four controls**, so a classifier stuck on one value
cannot pass them, and **both plants are checked absent from the census** on every run — a plant that
had drifted into the corpus would be testing the corpus instead of the instrument
([§4.2](#42-the-two-planted-positives-stay-in-now-that-the-corpus-yields)).

**Where a row carries its own second piece of evidence it is in [§5.3](#53-the-survivors-with-the-evidence-each-verdict-was-taken-from)**:
a stage-3 ruling names the line it was taken from and the contrasting name it was read against.
`NOT SURFACE` is read against the planted declaration; `SURFACE` against
`ConfigurationManager.GetSection(…)`, the case that looks identical in the counts and is not the
same thing at all.

### What the columns mean

| Column | Reading |
|---|---|
| **pages / sites** | the census's own page-spread — the ordering the method walks, never a budget (friction 45) |
| **s1 B/D** | commits whose diff changed the count of the name **as a substring**, Brighter / Darker. **Zero is conclusive**; non-zero means only *go to stage 2* |
| **s2 B/D** | the same, as a **whole identifier**. This is the number the verdict turns on |
| **live B/D** | files containing the name at the pin **and** at the release tag, summed per product. The census guarantees 0, so a non-zero here is a defect **in the instrument** |
| **verdict** | [§1](#1-the-vocabulary--three-verdicts-and-why-two-were-not-enough)'s three values, taken by `classify()` from the three columns above it |
| **stage 3** | what a person concluded, or why no person was needed: *screened at stage 1* and *substring only at stage 2* are verdicts with evidence, not gaps |

### 5.2 What stage 3 confirmed — seventeen names, ten pages, twenty-eight sites

**This is the set phase 4 acts on**, and it is the answer to the question 014's closing sentence
asked. Of 819 candidates, 720 were screened out at stage 1, 55 more were substrings at stage 2, and
**44 reached a person. Seventeen of the 44 are Brighter's own public surface, removed.**

| | Names | |
|---|---:|---|
| **SURFACE** — the product published it and then removed it | **17** | a watchlist row and a page repair (P0-5, P0-6) |
| **NOT SURFACE** — the product's source merely contained it | **27** | no row: the watchlist's job is to notice a name coming back, and these are not the product's names to bring back |
| **INSTRUMENT** — the census missed a live name | **0** | none found, which is the outcome `LIVE` exists to make visible rather than a thing nobody looked for |

**The design's twelve are all here, and five more are not in it:**

```text
UseMsSqlOutbox · UseMySqlOutbox · UseDynamoDbOutbox · UseInMemoryOutbox
UseDynamoDbTransactionConnectionProvider · UseMySqTransactionConnectionProvider
AddS3LuggageStore · CommandProcessorLifetime · UseScoped · NoTaskQueues
IUnitOfWork · BeginOrGetTransaction                                     <- the design's twelve

ApplicationJson · S3LuggageStoreCreation · StoreCreation
BeginOrGetTransactionAsync · UnitOfWork                                 <- and five more
```

**Twelve was a floor and the floor held.** The five the design did not have are the ones its
`≥3`-page reading could not reach: every one of them is a **one-page** name, and three of them
(`S3LuggageStoreCreation`, `StoreCreation`, `BeginOrGetTransactionAsync`) sit beside a name the
design *did* find — the option enum next to its registration method, the async twin next to the
sync one. **A name is dead in the company of its neighbours**, which is an argument for triaging a
family rather than a page-spread band.

`ApplicationJson` is the one that is not like the others. It is on `MessageMappers.md`, a page the
design never looked at, and the page prints it **inside Brighter's own constructor signature**:

```csharp
public MessageBody(string body, string contentType = ApplicationJson, CharacterEncoding characterEncoding = CharacterEncoding.UTF8)
```

The constructor at the pin reads `MessageBody(string? body, ContentType? contentType = null, …)`.
So the page documents a signature that no longer exists, in a block a reader would copy — and
**at line 132 the same page already uses the replacement**, `MediaTypeNames.Application.Octet`,
sixteen lines after the last of its five `ApplicationJson` sites (41, 94, 104, 112, 116). The page
contradicts itself inside one screen, and nothing could see it.

**Where the seventeen are printed**, re-derived two ways that agree — per name with `grep -rlw`
unioned, and once with a single alternation:

```bash
NAMES=$(awk -F'\t' '$2=="SURFACE"{print $1}' spec/015-census_triage/stage3.tsv)   # 17
for n in $NAMES; do grep -rlw "$n" contents/; done | sort -u | wc -l              # 10
grep -rlE "\b($(echo "$NAMES" | paste -sd'|' -))\b" contents/ | sort -u | wc -l   # 10
```

| Page | Fenced sites | Prose sites |
|---|---:|---:|
| `MessageMappers.md` | 5 | 0 |
| `DapperOutbox.md` | 4 | 0 |
| `DynamoOutbox.md` | 3 | 0 |
| `S3LuggageStore.md` | 3 | **2** |
| `BrighterBasicConfiguration.md` | 2 | 0 |
| `ShowMeTheCode.md` | 2 | 0 |
| `DispatchingARequest.md` | 1 | 0 |
| `FeatureSwitches.md` | 1 | 0 |
| `SweeperCircuitBreaking.md` | 1 | **3** |
| `UsingSweeperCircuitBreaking.md` | 1 | 0 |
| **10 pages** | **23, in 15 distinct blocks** | **5** |

**Finding 3 counted nine pages and eighteen sites for the twelve; the seventeen are ten and
twenty-eight.** `MessageMappers.md` is the page that joins, and it joins for a name the design did
not have.

**The five prose sites are not automatic repairs.** Ruling 6 — *a removed name may stay in prose
behind a visible opt-out* — and `SweeperCircuitBreaking.md`'s three are a bulleted list of provider
methods in running text, which is the case that ruling was written for. `S3LuggageStore.md`'s two
name options in a sentence describing the configuration block above them, so they stand or fall
with that block.

> **A `SURFACE` ruling is not a repair instruction.** 014's ruling that **the replacement column is
> a name, not a type** applies to every row phase 4 writes: `IAmAMessageScheduler` was the right
> name and the wrong type at 9 of 17 sites, satisfying the gate without compiling. The controls in
> `stage3.tsv` name what is live **in the same family**; they do not claim it is a drop-in.
### 5.3 The survivors, with the evidence each verdict was taken from

**44 of 819 names reached stage 2 with a whole-identifier match in history.** Each one below carries the evidence set the run kept, the stage-3 ruling, and the control that ruling was checked against.

`B` and `D` are the product the line came from. **A line opening `…` was clipped to put the match in view, which drops the diff's `+`/`-`** — the window is centred on the match rather than on the start of the line, because the first run printed lines whose only visible occurrence was a different name. Nothing turns on the sign here: `live` is 0 for every name in this section, so every occurrence shown is historical whichever way the diff ran.

#### `Date` — EXISTED, REMOVED · **NOT SURFACE**

12 page(s), 39 site(s) · stage 1 `118/4` · stage 2 `5/0` · live `0/0` · `0/0` dotted, `26/0` bare (B/D)

```text
B  …    [DynamoDBHashKey("Command+Date")]
B  …     AttributeName = "Command+Date",
B  …       AttributeName = "Topic+Date",
B  …sage has been sent to and the Date it was sent on
B  …      [DynamoDBHashKey("Topic+Date")]
B  -        public DateTime Date { get; set; }
B  -            Date = message.Header.TimeStamp == DateTime.MinValue ? DateTime.UtcNow : message.Header.TimeStamp
B  …e = $"{message.Header.Topic}+{Date:yyyy-MM-dd}";
B  -            Time = $"{Date.Ticks}";
B  -            TimeStamp = $"{Date}";
B  +        public DateTime Date { get; set; }
B  +            Date = message.Header.TimeStamp == DateTime.MinValue ? DateTime.UtcNow : message.Header.TimeStamp
… 2 more distinct line(s) in the checkpoint
```

**Ruling:** The pages mean `DateTimeOffset.UtcNow.Date` and their own `GenerateDailyReportCommand { Date = … }`. Brighter DID have `- public DateTime Date { get; set; }` on `DynamoDbMessage` until 2019, and 20 of the 26 whole-word history lines are inside string literals such as `[DynamoDBHashKey("Command+Date")]`

**Control:** the planted positive `IAmACommandStoreAsync`, whose evidence in the same run is `- private readonly IAmACommandStoreAsync _commandStore;` -- declaration position for a name the product published. This one's decisive lines are a BCL property and a page's own example

#### `AddHours` — EXISTED, REMOVED · **NOT SURFACE**

7 page(s), 11 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `2/0` dotted, `0/0` bare (B/D)

```text
B  …atchedSince = DateTime.UtcNow.AddHours( -1 * hoursDispatchedSince);
```

**Ruling:** `DateTime.UtcNow.AddHours(-1 * hoursDispatchedSince)` -- 2 dotted, 0 bare, a call on `DateTime`

**Control:** as `Date`: read against the planted declaration, which is 0 dotted / 4 bare

#### `Repository` — EXISTED, REMOVED · **NOT SURFACE**

6 page(s), 49 site(s) · stage 1 `9/3` · stage 2 `7/3` · live `0/0` · `64/12` dotted, `64/12` bare (B/D)

```text
B  …BoundaryDeclaringType, logger.Repository, logger.Name, level, message, exception)); }
B  …ventType.GetPropertyPortable("Repository");
B  …laringType, ((ILogger)logger).Repository, ((ILogger)logger).Name, (Level)level, message, exception); }
B  …ssion.Property(instanceCast, "Repository"),
D  …BoundaryDeclaringType, logger.Repository, logger.Name, level, message, exception)); }
D  …ventType.GetPropertyPortable("Repository");
D  …laringType, ((ILogger)logger).Repository, ((ILogger)logger).Name, (Level)level, message, exception); }
D  …ssion.Property(instanceCast, "Repository"),
```

**Ruling:** `eventType.GetPropertyPortable("Repository")` and `logger.Repository` -- log4net reflection, and 64 dotted against 64 bare, precisely ambiguous in the counts alone

**Control:** the counts do not discriminate here at all, so the ruling rests on the lines; the plant's lines are declarations and these are a string literal and a property access

#### `BuildServiceProvider` — EXISTED, REMOVED · **NOT SURFACE**

4 page(s), 8 site(s) · stage 1 `0/4` · stage 2 `0/4` · live `0/0` · `0/8` dotted, `0/0` bare (B/D)

```text
D  …turn (IQueryHandler)_services.BuildServiceProvider().GetService(handlerType);
D  …          return (T)_services.BuildServiceProvider().GetService(decoratorType);
D  …zy<IServiceProvider>(services.BuildServiceProvider);
D  …_serviceProvider => _services.BuildServiceProvider();
```

**Ruling:** `_services.BuildServiceProvider().GetService(handlerType)` in Darker -- 8 dotted, 0 bare, the ASP.NET extension

**Control:** as `AddHours`: the plant is 0 dotted / 4 bare in the same run

#### `TenantId` — EXISTED, REMOVED · **NOT SURFACE**

3 page(s), 13 site(s) · stage 1 `5/0` · stage 2 `4/0` · live `0/0` · `0/0` dotted, `8/0` bare (B/D)

```text
B  -                TenantId = _azureTenantId,
B  +                TenantId = _azureTenantId,
```

**Ruling:** `- TenantId = _azureTenantId,` -- Azure credential plumbing, assigned to a private field

**Control:** as `DbParameter`: nothing here is a published member

#### `DbParameter` — EXISTED, REMOVED · **NOT SURFACE**

3 page(s), 3 site(s) · stage 1 `12/0` · stage 2 `7/0` · live `0/0` · `0/0` dotted, `62/0` bare (B/D)

```text
B  -        private DbParameter CreateSqlParameter(string parameterName, object value)
B  -            params DbParameter[] parameters)
B  …mand(DbConnection connection, DbParameter[] parameters, int timeoutInMilliseconds)
B  -        private DbParameter[] InitAddDbParameters<T>(T command, string contextKey) where T : class, IRequest
B  -        public DbParameter CreateSqlParameter(string parameterName, object value)
B  +        private DbParameter CreateSqlParameter(string parameterName, object value)
B  …timeoutInMilliseconds, params DbParameter[] parameters)
B  …l, int timeoutInMilliseconds, DbParameter[] parameters, CancellationToken cancellationToken = default)
B  …private void FormatAddCommand(DbParameter[] parameters, DbCommand sqlcmd, string sqlAdd, int timeoutInMillisec
B  …ramtersParamArrayToCollection(DbParameter[] parameters, DbCommand command)
B  -        private static DbParameter CreateSqlParameter(string parameterName, object value)
B  -        private static DbParameter[] InitAddDbParameters(string topic, T message)
… 17 more distinct line(s) in the checkpoint
```

**Ruling:** `- private DbParameter CreateSqlParameter(…)` and `params DbParameter[] parameters` -- `System.Data.Common.DbParameter` in PRIVATE signatures

**Control:** the plant is a `public interface` and a field of a published type; every decisive line here is `private`

#### `GetSection` — EXISTED, REMOVED · **NOT SURFACE**

3 page(s), 3 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `2/0` dotted, `0/0` bare (B/D)

```text
B  …ection = ConfigurationManager.GetSection(BrighterConfigSectionName) as MessageViewerSection;
```

**Ruling:** `var configSection = ConfigurationManager.GetSection(BrighterConfigSectionName)` -- the case `design.md` 10.6 chose as its worked example

**Control:** the plant, in the same run, at identical counts: 2 commits, live 0. Same numbers, opposite readings

#### `GetEnvironmentVariable` — EXISTED, REMOVED · **NOT SURFACE**

2 page(s), 5 site(s) · stage 1 `4/2` · stage 2 `4/2` · live `0/0` · `16/2` dotted, `0/0` bare (B/D)

```text
B  … _azureUserName = Environment.GetEnvironmentVariable(_azureUserNameKey) ?? throw new InvalidOperationException
B  … _azureTenantId = Environment.GetEnvironmentVariable(_azureTenantIdKey) ?? throw new InvalidOperationException
B  -            Environment.GetEnvironmentVariable(_azureUserNameKey),
B  -            Environment.GetEnvironmentVariable(_azureTenantIdKey), authenticationTokenScopes)
B  … _azureUserName = Environment.GetEnvironmentVariable(_azureUserNameKey);
B  … _azureTenantId = Environment.GetEnvironmentVariable(_azureTenantIdKey);
B  +            Environment.GetEnvironmentVariable(_azureUserNameKey),
B  +            Environment.GetEnvironmentVariable(_azureTenantIdKey), authenticationTokenScopes)
D  …     var envVar = Environment.GetEnvironmentVariable(LogProvider.DisableLoggingEnvironmentVariable);
```

**Ruling:** `Environment.GetEnvironmentVariable(_azureUserNameKey)` -- 16 dotted, 0 bare in Brighter

**Control:** as `AddHours`

#### `CommandProcessorLifetime` — EXISTED, REMOVED · **SURFACE**

2 page(s), 2 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `2/0` dotted, `4/0` bare (B/D)

```text
B  …       public ServiceLifetime CommandProcessorLifetime { get; set; } = ServiceLifetime.Transient;
B  -        ServiceLifetime CommandProcessorLifetime { get; set; }
B  …uildCommandProcessor, options.CommandProcessorLifetime));
B  …       public ServiceLifetime CommandProcessorLifetime { get; set; } = ServiceLifetime.Singleton;
B  +        ServiceLifetime CommandProcessorLifetime { get; set; }
```

**Ruling:** `- public ServiceLifetime CommandProcessorLifetime { get; set; } = ServiceLifetime.Transient;`, plus the interface member `- ServiceLifetime CommandProcessorLifetime { get; set; }`

**Control:** `HandlerLifetime`, a sibling property on the same `BrighterOptions`, is live at 2 files; this name is 0. Same class, same shape, opposite answer

#### `UseMsSqlOutbox` — EXISTED, REMOVED · **SURFACE**

2 page(s), 2 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `0/0` dotted, `2/0` bare (B/D)

```text
B  …ublic static IBrighterBuilder UseMsSqlOutbox(
B  …tatic IBrighterHandlerBuilder UseMsSqlOutbox(
```

**Ruling:** `- public static IBrighterBuilder UseMsSqlOutbox(` -- an extension method on Brighter's own builder, removed

**Control:** `UseOutboxSweeper`, the surviving member of the same extension family, is live at 1 file by the same query; this name is 0

#### `Globals` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 10 site(s) · stage 1 `4/0` · stage 2 `3/0` · live `0/0` · `0/0` dotted, `20/0` bare (B/D)

```text
B  …        messageHeader.Bag.Add(Globals.DispatchedAtKey, dispatchedDate);
B  …        messageHeader.Bag.Add(Globals.PreviousEventIdKey, previousEventId);
B  …essage.Header.Bag.TryGetValue(Globals.DispatchedAtKey, out object value)
B  …sEventId = message.Header.Bag[Globals.PreviousEventIdKey] as string;
B  …        if (!args.ContainsKey(Globals.StreamArg))
B  …hrow new ArgumentException($"{Globals.StreamArg} missing", nameof(args));
B  …            var stream = args[Globals.StreamArg] as string;
B  …hrow new ArgumentException($"{Globals.StreamArg} value must not be null or empty", nameof(args));
B  …essage.Header.Bag.ContainsKey(Globals.DispatchedAtKey)
B  …         ? message.Header.Bag[Globals.DispatchedAtKey] as string
```

**Ruling:** Brighter's history holds `Globals.DispatchedAtKey` and `Globals.StreamArg`; the page means its OWN constants holder, `Globals.MYRETRYPIPELINE`. Same name, two different things

**Control:** the collision D12 named, measured: the name resolves in history and the documentation is not claiming that name

#### `ReadCapacityUnits` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 7 site(s) · stage 1 `7/0` · stage 2 `7/0` · live `0/0` · `0/0` dotted, `14/0` bare (B/D)

```text
B  …t = new ProvisionedThroughput{ReadCapacityUnits = 10, WriteCapacityUnits = 10};
B  -                ReadCapacityUnits = readCapacityUnits,
B  … ?? new ProvisionedThroughput{ReadCapacityUnits = 10, WriteCapacityUnits = 5};
B  -                    ReadCapacityUnits = readCapacityUnits,
B  +                ReadCapacityUnits = readCapacityUnits,
B  +                    ReadCapacityUnits = readCapacityUnits,
```

**Ruling:** `new ProvisionedThroughput{ReadCapacityUnits = 10, WriteCapacityUnits = 10}` -- an AWS SDK model property. The page's DynamoDB table-creation example is current

**Control:** `Amazon` is a `NOISE_PREFIX`, so the namespace is filtered and the member name on it is not: this row is the noise set's own boundary, visible

#### `WriteCapacityUnits` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 7 site(s) · stage 1 `7/0` · stage 2 `7/0` · live `0/0` · `0/0` dotted, `14/0` bare (B/D)

```text
B  …ghput{ReadCapacityUnits = 10, WriteCapacityUnits = 10};
B  -                WriteCapacityUnits = writeCapacityUnits
B  …ghput{ReadCapacityUnits = 10, WriteCapacityUnits = 5};
B  -                    WriteCapacityUnits = writeCapacityUnits
B  +                WriteCapacityUnits = writeCapacityUnits
B  +                    WriteCapacityUnits = writeCapacityUnits
```

**Ruling:** as `ReadCapacityUnits`, from the same object initialiser

**Control:** as `ReadCapacityUnits`

#### `ApplicationJson` — EXISTED, REMOVED · **SURFACE**

1 page(s), 5 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `0/0` dotted, `8/0` bare (B/D)

```text
B  -        public const string ApplicationJson = "application/json";
B  …ng body, string contentType = ApplicationJson, CharacterEncoding characterEncoding = CharacterEncoding.UTF8)
B  …] bytes, string contentType = ApplicationJson, CharacterEncoding characterEncoding = CharacterEncoding.UTF8)
B  …e> body, string contentType = ApplicationJson, CharacterEncoding characterEncoding = CharacterEncoding.UTF8)
B  +        public const string ApplicationJson = "application/json";
B  …ng body, string contentType = ApplicationJson, CharacterEncoding encoding = CharacterEncoding.UTF8)
B  …] bytes, string contentType = ApplicationJson, CharacterEncoding encoding = CharacterEncoding.UTF8)
B  …e> body, string contentType = ApplicationJson, CharacterEncoding encoding = CharacterEncoding.UTF8)
```

**Ruling:** `- public const string ApplicationJson = "application/json";` -- and `MessageMappers.md` prints it inside Brighter's own constructor signature, `public MessageBody(string body, string contentType = ApplicationJson, …)`

**Control:** `MediaTypeNames` is live at 27 files and `ContentType` at 67; the constructor today reads `MessageBody(string? body, ContentType? contentType = null, …)`. The same page already uses `MediaTypeNames.Application.Octet` eighty lines further down

#### `Authorization` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 4 site(s) · stage 1 `4/0` · stage 2 `2/0` · live `0/0` · `4/0` dotted, `0/0` bare (B/D)

```text
B  …_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
```

**Ruling:** `_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(` -- a property of `HttpClient`'s headers

**Control:** as `AddHours`

#### `Resources` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 3 site(s) · stage 1 `6/0` · stage 2 `2/0` · live `0/0` · `4/0` dotted, `0/0` bare (B/D)

```text
B  …er.MessageViewer.Adaptors.API.Resources;
```

**Ruling:** `Paramore.Brighter.MessageViewer.Adaptors.API.Resources;` -- a namespace segment in a removed project

**Control:** the plant is a type; a namespace segment is not a name anything can use

#### `General` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 2 site(s) · stage 1 `14/3` · stage 2 `3/0` · live `0/0` · `0/0` dotted, `24/0` bare (B/D)

```text
B  -// General Information about an assembly is controlled through the following
B  +// General Information about an assembly is controlled through the following
```

**Ruling:** `// General Information about an assembly is controlled through the following` -- an AssemblyInfo COMMENT. The pages use the word in prose headings

**Control:** the census strips comments from the documentation side and nothing strips them from the history side; this row is that asymmetry, visible

#### `StackExchange` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 2 site(s) · stage 1 `5/0` · stage 2 `5/0` · live `0/0` · `0/0` dotted, `14/0` bare (B/D)

```text
B  -using StackExchange.Redis;
B  …to use BRPOP or BLPOP because StackExchange Redis, which is a multiplexed library, and so can't
B  …tps://stackexchange.github.io/StackExchange.Redis/PipelinesMultiplexers
B  +using StackExchange.Redis;
B  +﻿using StackExchange.Redis;
```

**Ruling:** the `StackExchange.Redis` namespace, and the pages use it in a package name and a `using`

**Control:** as `Resources`: a namespace segment, and the one prose page that names it says explicitly that the connection string is ServiceStack's and not this one's

#### `AddS3LuggageStore` — EXISTED, REMOVED · **SURFACE**

1 page(s), 1 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `0/0` dotted, `2/0` bare (B/D)

```text
B  -        public static void AddS3LuggageStore(this IServiceCollection services, Action<S3LuggageOptions> confi
B  +        public static void AddS3LuggageStore(this IServiceCollection services, Action<S3LuggageOptions> confi
```

**Ruling:** `- public static void AddS3LuggageStore(this IServiceCollection services, Action<S3LuggageOptions> configure)`

**Control:** `S3LuggageOptions` is live at 4 files and `S3LuggageStore` is a live class, so the FEATURE survives and only its registration API is gone

#### `AddTransient` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 1 site(s) · stage 1 `2/3` · stage 2 `2/3` · live `0/0` · `4/8` dotted, `0/0` bare (B/D)

```text
B  -                services.AddTransient<IAmACommandProcessorProvider, ScopedCommandProcessorProvider>();
B  -                services.AddTransient<IAmACommandProcessorProvider, CommandProcessorProvider>();
B  +                services.AddTransient<IAmACommandProcessorProvider, ScopedCommandProcessorProvider>();
B  +                services.AddTransient<IAmACommandProcessorProvider, CommandProcessorProvider>();
D  -            _services.AddTransient(handlerType);
D  -            _services.AddTransient(decoratorType);
D  +            _services.AddTransient(handlerType);
D  +            _services.AddTransient(decoratorType);
D  -                services.AddTransient(subscriber.HandlerType);
D  -                _services.AddTransient(decoratorType);
D  +                services.AddTransient(subscriber.HandlerType);
D  +                _services.AddTransient(decoratorType);
```

**Ruling:** `services.AddTransient(…)` -- the Microsoft DI extension, 4 dotted in Brighter and 8 in Darker, 0 bare in both

**Control:** as `AddHours`

#### `BeginOrGetTransaction` — EXISTED, REMOVED · **SURFACE**

1 page(s), 1 site(s) · stage 1 `4/0` · stage 2 `4/0` · live `0/0` · `8/0` dotted, `12/0` bare (B/D)

```text
B  -        DbTransaction BeginOrGetTransaction();
B  …lic TransactWriteItemsRequest BeginOrGetTransaction()
B  …    TransactWriteItemsRequest BeginOrGetTransaction();
B  …n (SqlTransaction)_unitOfWork.BeginOrGetTransaction();
B  -        public DbTransaction BeginOrGetTransaction()
B  …(MySqlTransaction)_unitOfWork.BeginOrGetTransaction();
B  …nsaction = dynamoDbUnitOfWork.BeginOrGetTransaction();
B  …SqliteTransaction)_unitOfWork.BeginOrGetTransaction();
B  +        public DbTransaction BeginOrGetTransaction()
B  +        DbTransaction BeginOrGetTransaction();
```

**Ruling:** `- DbTransaction BeginOrGetTransaction();` -- an interface member, with implementations returning `SqlTransaction`, `MySqlTransaction` and `TransactWriteItemsRequest`

**Control:** `IAmABoxTransactionProvider` live at 26; this name 0 at both refs of both products

#### `BeginOrGetTransactionAsync` — EXISTED, REMOVED · **SURFACE**

1 page(s), 1 site(s) · stage 1 `3/0` · stage 2 `3/0` · live `0/0` · `0/0` dotted, `8/0` bare (B/D)

```text
B  -        Task<DbTransaction> BeginOrGetTransactionAsync(CancellationToken cancellationToken);
B  …lic async Task<DbTransaction> BeginOrGetTransactionAsync(CancellationToken cancellationToken)
B  +        Task<DbTransaction> BeginOrGetTransactionAsync(CancellationToken cancellationToken);
```

**Ruling:** `- Task<DbTransaction> BeginOrGetTransactionAsync(CancellationToken cancellationToken);` -- the async twin, and a name the design's twelve did not carry

**Control:** as `BeginOrGetTransaction`: `IAmABoxTransactionProvider` live at 26, this name 0

#### `Categories` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 1 site(s) · stage 1 `7/3` · stage 2 `7/3` · live `0/0` · `0/0` dotted, `32/6` bare (B/D)

```text
B  …ntryType.GetPropertyPortable("Categories"),
D  …ntryType.GetPropertyPortable("Categories"),
```

**Ruling:** `entryType.GetPropertyPortable("Categories")` -- a log4net property name inside a STRING LITERAL, in both products. The page means `_dbContext.Categories`, an EF DbSet in its own example

**Control:** `triage.md` 1.2's third reading, measured: a string-literal match is not a use of a name at all

#### `ConfigurationManager` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 1 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `0/0` dotted, `2/0` bare (B/D)

```text
B  …          var configSection = ConfigurationManager.GetSection(BrighterConfigSectionName) as MessageViewerSecti
```

**Ruling:** the `System.Configuration` type, in the same removed block as `GetSection`

**Control:** as `GetSection`, and the two names come from one line of history

#### `DeserializeObject` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 1 site(s) · stage 1 `15/0` · stage 2 `15/0` · live `0/0` · `48/0` dotted, `0/0` bare (B/D)

```text
B  …ertToCommand() => JsonConvert.DeserializeObject<T>(CommandBody);
B  …           return JsonConvert.DeserializeObject<TResult>(body);
B  …y<string, object>)JsonConvert.DeserializeObject(value.StringValue);
B  …    var message = JsonConvert.DeserializeObject<T>(json, settings);
B  …    var headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headersJson);
B  …        var bag = JsonConvert.DeserializeObject<Dictionary<string, object>>(bagJson);
B  …        var bag = JsonConvert.DeserializeObject<Dictionary<string, string>>(HeaderBag);
B  -                JsonConvert.DeserializeObject<MessageHeader>(json);
B  …r dictionaryBag = JsonConvert.DeserializeObject<Dictionary<string, string>>(headerBag);
B  …           return JsonConvert.DeserializeObject<ConfigurationCommand>(message.Body.Value);
B  …           return JsonConvert.DeserializeObject<T>(inboxItem.RequestBody);
B  …           return JsonConvert.DeserializeObject<MonitorEvent>(message.Body.Value);
… 5 more distinct line(s) in the checkpoint
```

**Ruling:** `JsonConvert.DeserializeObject(…)` -- 48 dotted, 0 bare, Newtonsoft

**Control:** as `AddHours`. `Newtonsoft` is a `NOISE_PREFIX`; the method name on it is not

#### `GetCurrentClassLogger` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 1 site(s) · stage 1 `7/3` · stage 2 `7/3` · live `0/0` · `0/0` dotted, `32/6` bare (B/D)

```text
B  -        static ILog GetCurrentClassLogger()
B  +        static ILog GetCurrentClassLogger()
D  -        static ILog GetCurrentClassLogger()
D  +        static ILog GetCurrentClassLogger()
```

**Ruling:** `LogProvider.GetCurrentClassLogger()` -- LibLog, vendored into both products and removed with it. The page prints it in a constructor a reader cannot compile today

**Control:** the plant is Brighter's own published interface; this is third-party code that was carried, not published

#### `HttpRequestException` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 1 site(s) · stage 1 `2/0` · stage 2 `3/0` · live `0/0` · `0/0` dotted, `5/0` bare (B/D)

```text
B  -            catch (HttpRequestException he)
B  +            catch (HttpRequestException he)
```

**Ruling:** `catch (HttpRequestException he)` -- `System.Net.Http`. This is the name the first full run died on, at 597 of 819

**Control:** as `DbParameter`: a catch clause is not a published member

#### `HttpResponseMessage` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 1 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `0/0` dotted, `1/0` bare (B/D)

```text
B  …    public T ParseResponse<T>(HttpResponseMessage response) where T : class, new()
```

**Ruling:** `public T ParseResponse<T>(HttpResponseMessage response)` -- a BCL type in a Brighter signature, which is a parameter type and not a name Brighter published

**Control:** the plant is the NAME being declared; here the declared name is `ParseResponse`

#### `IUnitOfWork` — EXISTED, REMOVED · **SURFACE**

1 page(s), 1 site(s) · stage 1 `3/0` · stage 2 `3/0` · live `0/0` · `0/0` dotted, `14/0` bare (B/D)

```text
B  -    public interface IUnitOfWork : IAmABoxTransactionConnectionProvider, IDisposable
B  -        private readonly IUnitOfWork _unitOfWork;
B  …MsSqlDapperConnectionProvider(IUnitOfWork unitOfWork)
B  …MySqlDapperConnectionProvider(IUnitOfWork unitOfWork)
B  …qliteDapperConnectionProvider(IUnitOfWork unitOfWork)
B  +        private readonly IUnitOfWork _unitOfWork;
B  +    public interface IUnitOfWork : IDisposable
```

**Ruling:** `- public interface IUnitOfWork : IAmABoxTransactionConnectionProvider, IDisposable`

**Control:** `IAmABoxTransactionProvider` is live at 26 files; the interface that replaced it survives and this one does not

#### `InfoFormat` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 1 site(s) · stage 1 `16/9` · stage 2 `16/9` · live `0/0` · `110/32` dotted, `32/6` bare (B/D)

```text
B  …                _logger.Value.InfoFormat("SqsMessageConsumer: Received message from queue {0}, message: {1}{2}
B  …                _logger.Value.InfoFormat("SqsMessageConsumer: Deleted the message {0} with receipt handle {1}
B  …                _logger.Value.InfoFormat(
B  …                _logger.Value.InfoFormat("SqsMessageConsumer: Purging the queue {0}", _queueName);
B  …                _logger.Value.InfoFormat("SqsMessageConsumer: Purged the queue {0}", _queueName);
B  …                _logger.Value.InfoFormat("SqsMessageConsumer: requeueing the message {0}", message.Id);
B  …                _logger.Value.InfoFormat("SqsMessageConsumer: requeued the message {0}", message.Id);
B  …                _logger.Value.InfoFormat("Parition Added {0}", String.Join(",", partitions));
B  …                _logger.Value.InfoFormat("Partitions for consumer revoked {0}", string.Join(",", revokedPartit
B  …                _logger.Value.InfoFormat("Partitions for consumer lost {0}", string.Join(",", lostPartitions))
B  -            _logger.Value.InfoFormat($"Kakfa consumer subscribing to {Topic}");
B  …                _logger.Value.InfoFormat($"Storing offset {new Offset(topicPartitionOffset.Offset + 1).Value}
… 58 more distinct line(s) in the checkpoint
```

**Ruling:** `_logger.Value.InfoFormat(…)`, 110 dotted in Brighter, and Darker's own `- public static void InfoFormat(this ILog logger, …)` -- LibLog's extension, vendored. The page prints `logger.InfoFormat(…)` in a handler a reader cannot compile today

**Control:** as `GetCurrentClassLogger`, and the two rows are the same finding twice

#### `LogCritical` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 1 site(s) · stage 1 `3/0` · stage 2 `3/0` · live `0/0` · `18/0` dotted, `0/0` bare (B/D)

```text
B  …                     s_logger.LogCritical(configurationException, "MessagePump: Stopping receiving of messages
B  -                    s_logger.LogCritical(configurationException,"MessagePump: Stopping receiving of messages
B  -            s_logger.LogCritical(
B  -                s_logger.LogCritical(
B  +                    s_logger.LogCritical(configurationException,"MessagePump: Stopping receiving of messages
B  +            s_logger.LogCritical(
B  +                s_logger.LogCritical(
B  +                    s_logger.LogCritical(configurationException,
B  +                    s_logger.LogCritical(exception,
```

**Ruling:** `s_logger.LogCritical(configurationException, …)` -- 18 dotted, 0 bare, `Microsoft.Extensions.Logging`

**Control:** as `AddHours`. `Microsoft` is a `NOISE_PREFIX` and the method name on it is not

#### `NoTaskQueues` — EXISTED, REMOVED · **SURFACE**

1 page(s), 1 site(s) · stage 1 `4/0` · stage 2 `4/0` · live `0/0` · `6/0` dotted, `6/0` bare (B/D)

```text
B  …           ? messagingBuilder.NoTaskQueues()
B  …uesBuilder = messagingBuilder.NoTaskQueues();
B  …oach, then use the <see cref="NoTaskQueues"/> method to indicate your intent
B  …  public INeedARequestContext NoTaskQueues()
B  -        INeedARequestContext NoTaskQueues();
B  +        INeedARequestContext NoTaskQueues();
```

**Ruling:** `public INeedARequestContext NoTaskQueues()` and the interface member `- INeedARequestContext NoTaskQueues();` -- a step in the fluent builder

**Control:** `NoExternalBus` -- the surviving member of the same fluent step -- is live at 2 files; this name is 0

#### `Octet` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 1 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `10/0` dotted, `0/0` bare (B/D)

```text
B  …pe(MediaTypeNames.Application.Octet), CharacterEncoding.Raw)
```

**Ruling:** `MediaTypeNames.Application.Octet` -- and the page prints exactly that, correctly

**Control:** `MediaTypeNames` is live at 27 files, so the page's line is current; only the leaf token failed to resolve

#### `Outcome` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 1 site(s) · stage 1 `4/0` · stage 2 `4/0` · live `0/0` · `10/0` dotted, `0/0` bare (B/D)

```text
B  -            if (result.Outcome != OutcomeType.Successful)
B  +            if (result.Outcome != OutcomeType.Successful)
```

**Ruling:** `if (result.Outcome != OutcomeType.Successful)` -- Polly. The page means `Outcome.FromResult(Product.Default)`, Polly v8

**Control:** as `AddHours`. `Polly` is a `NOISE_PREFIX`; a member on a Polly type is not

#### `S3LuggageStoreCreation` — EXISTED, REMOVED · **SURFACE**

1 page(s), 1 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `0/0` dotted, `26/0` bare (B/D)

```text
B  -            StoreCreation = S3LuggageStoreCreation.CreateIfMissing;
B  -        public S3LuggageStoreCreation StoreCreation { get; set; }
B  -    public enum S3LuggageStoreCreation
B  …     /// Note that if you set S3LuggageStoreCreation.CreateIfMissing or ValidateExists the factor will make HT
B  -            S3LuggageStoreCreation storeCreation = S3LuggageStoreCreation.AssumeExists,
B  …StoreCreation storeCreation = S3LuggageStoreCreation.AssumeExists,
B  …         if (storeCreation == S3LuggageStoreCreation.ValidateExists || storeCreation == S3LuggageStoreCreation
B  …ateExists || storeCreation == S3LuggageStoreCreation.CreateIfMissing)
B  …         if (storeCreation == S3LuggageStoreCreation.CreateIfMissing)
B  …         if (storeCreation == S3LuggageStoreCreation.CreateIfMissing || storeCreation == S3LuggageStoreCreatio
B  …IfMissing || storeCreation == S3LuggageStoreCreation.ValidateExists)
B  …         if (storeCreation == S3LuggageStoreCreation.ValidateExists)
… 5 more distinct line(s) in the checkpoint
```

**Ruling:** `- public enum S3LuggageStoreCreation` -- a public enum of Brighter's own

**Control:** `S3LuggageOptions` live at 4; this name 0. The options class outlived the enum it carried

#### `SetBasePath` — EXISTED, REMOVED · **NOT SURFACE**

1 page(s), 1 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `4/0` dotted, `0/0` bare (B/D)

```text
B  -                .SetBasePath(currentDirectory)
B  -                .SetBasePath(env.ContentRootPath).Build();
B  +                .SetBasePath(currentDirectory)
B  +                .SetBasePath(env.ContentRootPath).Build();
```

**Ruling:** `.SetBasePath(env.ContentRootPath).Build()` -- `Microsoft.Extensions.Configuration`

**Control:** as `AddHours`

#### `StoreCreation` — EXISTED, REMOVED · **SURFACE**

1 page(s), 1 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `2/0` dotted, `8/0` bare (B/D)

```text
B  -        /// StoreCreation: Create if Missing
B  -            StoreCreation = S3LuggageStoreCreation.CreateIfMissing;
B  …ed if you choose a <see cref="StoreCreation"/> of Assume Exists.
B  …public S3LuggageStoreCreation StoreCreation { get; set; }
B  …       storeCreation: options.StoreCreation,
B  +        /// StoreCreation: Create if Missing
B  +            StoreCreation = S3LuggageStoreCreation.CreateIfMissing;
```

**Ruling:** `- public S3LuggageStoreCreation StoreCreation { get; set; }` -- a public property of the options class

**Control:** `S3LuggageOptions` live at 4; this name 0

#### `UnitOfWork` — EXISTED, REMOVED · **SURFACE**

1 page(s), 1 site(s) · stage 1 `31/0` · stage 2 `3/0` · live `0/0` · `0/0` dotted, `12/0` bare (B/D)

```text
B  -    public class UnitOfWork : IUnitOfWork
B  -        public UnitOfWork(DbConnectionStringProvider dbConnectionStringProvider)
B  +    public class UnitOfWork : IUnitOfWork
B  +        public UnitOfWork(DbConnectionStringProvider dbConnectionStringProvider)
```

**Ruling:** `- public class UnitOfWork : IUnitOfWork` -- the implementation the page names by its full type, `Paramore.Brighter.MySql.Dapper.UnitOfWork`

**Control:** as `IUnitOfWork`: `IAmABoxTransactionProvider` live at 26, this name 0

#### `UseDynamoDbOutbox` — EXISTED, REMOVED · **SURFACE**

1 page(s), 1 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `0/0` dotted, `2/0` bare (B/D)

```text
B  …ublic static IBrighterBuilder UseDynamoDbOutbox(
B  …tatic IBrighterHandlerBuilder UseDynamoDbOutbox(
```

**Ruling:** `- public static IBrighterBuilder UseDynamoDbOutbox(`

**Control:** as `UseMsSqlOutbox`: `UseOutboxSweeper` live at 1, this name 0

#### `UseDynamoDbTransactionConnectionProvider` — EXISTED, REMOVED · **SURFACE**

1 page(s), 1 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `0/0` dotted, `2/0` bare (B/D)

```text
B  …ublic static IBrighterBuilder UseDynamoDbTransactionConnectionProvider(
```

**Ruling:** `- public static IBrighterBuilder UseDynamoDbTransactionConnectionProvider(`

**Control:** `IAmABoxTransactionProvider`, the interface that replaced the connection-provider family, is live at 26 files; this name is 0

#### `UseInMemoryOutbox` — EXISTED, REMOVED · **SURFACE**

1 page(s), 1 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `0/0` dotted, `2/0` bare (B/D)

```text
B  …ublic static IBrighterBuilder UseInMemoryOutbox(this IBrighterBuilder brighterBuilder)
B  …tatic IBrighterHandlerBuilder UseInMemoryOutbox(
```

**Ruling:** `- public static IBrighterBuilder UseInMemoryOutbox(this IBrighterBuilder brighterBuilder)`

**Control:** as `UseMsSqlOutbox`: `UseOutboxSweeper` live at 1, this name 0

#### `UseMySqTransactionConnectionProvider` — EXISTED, REMOVED · **SURFACE**

1 page(s), 1 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `0/0` dotted, `2/0` bare (B/D)

```text
B  …ublic static IBrighterBuilder UseMySqTransactionConnectionProvider(
```

**Ruling:** `- public static IBrighterBuilder UseMySqTransactionConnectionProvider(` -- and note the product's own typo, `MySq`, which the page reproduces faithfully

**Control:** as above: `IAmABoxTransactionProvider` live at 26, this name 0

#### `UseMySqlOutbox` — EXISTED, REMOVED · **SURFACE**

1 page(s), 1 site(s) · stage 1 `2/0` · stage 2 `2/0` · live `0/0` · `0/0` dotted, `2/0` bare (B/D)

```text
B  …ublic static IBrighterBuilder UseMySqlOutbox(
B  …tatic IBrighterHandlerBuilder UseMySqlOutbox(
```

**Ruling:** `- public static IBrighterBuilder UseMySqlOutbox(`, with a second overload on `IBrighterHandlerBuilder`

**Control:** as `UseMsSqlOutbox`: `UseOutboxSweeper` live at 1, this name 0

#### `UseScoped` — EXISTED, REMOVED · **SURFACE**

1 page(s), 1 site(s) · stage 1 `5/0` · stage 2 `5/0` · live `0/0` · `2/0` dotted, `6/0` bare (B/D)

```text
B  -        public bool UseScoped { get; set; } = false;
B  -        bool UseScoped { get; set; }
B  -            if (options.UseScoped)
B  +        bool UseScoped { get; set; }
B  +        public bool UseScoped { get; set; } = false;
B  +            if (options.UseScoped)
```

**Ruling:** `- public bool UseScoped { get; set; } = false;`, and `- if (options.UseScoped)` reading it

**Control:** `BrighterOptions` itself is live at 3 files, so the options class is not what went away

### 5.4 The record — one row per census name

| name | pages | sites | s1 B/D | s2 B/D | live B/D | verdict | stage 3 |
|---|---:|---:|---:|---:|---:|---|---|
| `OrderId` | 31 | 113 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CustomerId` | 17 | 55 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HostBuilderContext` | 14 | 21 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MyCommand` | 13 | 169 | 2/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `GreetingEvent` | 13 | 47 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Date` | 12 | 39 | 118/4 | 5/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `CustomerName` | 12 | 37 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Email` | 12 | 36 | 1/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `CreateDefaultBuilder` | 12 | 18 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Quantity` | 11 | 37 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateBuilder` | 11 | 21 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `WebApplication` | 11 | 21 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderStatus` | 11 | 19 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Orders` | 11 | 19 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateApplicationBuilder` | 11 | 13 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ApplicationDbContext` | 10 | 35 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ProcessOrderCommand` | 10 | 25 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Total` | 10 | 16 | 49/1 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `OrderDate` | 9 | 27 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddGreeting` | 9 | 23 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AsNoTracking` | 9 | 17 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Price` | 8 | 35 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Product` | 8 | 21 | 8/4 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `IOrderRepository` | 8 | 20 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Amount` | 8 | 17 | 4/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `TotalAmount` | 8 | 16 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Greeting` | 7 | 53 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderCreated` | 7 | 48 | 1/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `GetByIdAsync` | 7 | 18 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GreetingCommand` | 7 | 18 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Person` | 7 | 17 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GreetingMade` | 7 | 16 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IsDevelopment` | 7 | 13 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddHours` | 7 | 11 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `PersonId` | 7 | 11 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Cancelled` | 7 | 7 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Repository` | 6 | 49 | 9/3 | 7/3 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `ProcessPayment` | 6 | 23 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Greetings` | 6 | 21 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ProductId` | 6 | 19 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ProcessPaymentCommand` | 6 | 13 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddDays` | 6 | 8 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DbConnectionString` | 6 | 8 | 3/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `AddDbContext` | 6 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Customers` | 6 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Greet` | 6 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PlaceOrder` | 5 | 53 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UnitPrice` | 5 | 25 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ProcessOrder` | 5 | 18 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Fact` | 5 | 13 | 143/20 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `TaskCreated` | 5 | 12 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ToListAsync` | 5 | 12 | 1/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `ProductName` | 5 | 9 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaskUpdated` | 5 | 9 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddCheck` | 5 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddHealthChecks` | 5 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GenerateDailyReportCommand` | 5 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetOrderQuery` | 4 | 16 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddCircuitBreaker` | 4 | 8 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `BuildServiceProvider` | 4 | 8 | 0/4 | 0/4 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `OrderDto` | 4 | 8 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderSummary` | 4 | 8 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseSqlServer` | 4 | 7 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddTimeout` | 4 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HttpGet` | 4 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TenantId` | 3 | 13 | 5/0 | 4/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `CustomerDto` | 3 | 11 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetPersonNameQuery` | 3 | 10 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LargeOrder` | 3 | 10 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MaxPrice` | 3 | 10 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MinPrice` | 3 | 10 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StockQuantity` | 3 | 10 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ProcessSchedulerId` | 3 | 9 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateOrderCommand` | 3 | 8 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IHostEnvironment` | 3 | 8 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SearchTerm` | 3 | 8 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DueDate` | 3 | 7 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Country` | 3 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `InStock` | 3 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Products` | 3 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AsSyncOverAsync` | 3 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PersonCreated` | 3 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TotalCount` | 3 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TotalOrders` | 3 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AverageOrderValue` | 3 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `BreakDuration` | 3 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FailureRatio` | 3 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MinimumThroughput` | 3 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MyHandler` | 3 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `NotEmpty` | 3 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RecipientId` | 3 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TotalRevenue` | 3 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CircuitBreakerStrategyOptions` | 3 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CountAsync` | 3 | 3 | 5/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `CreateParameter` | 3 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DbParameter` | 3 | 3 | 12/0 | 7/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `GetSection` | 3 | 3 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `GreetingsEntityGateway` | 3 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HttpPost` | 3 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Pending` | 3 | 3 | 17/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `ProcessAsync` | 3 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ThenInclude` | 3 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseSqlite` | 3 | 3 | 3/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `TaskCommand` | 2 | 15 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DbConfiguration` | 2 | 14 | 13/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `IPersonRepository` | 2 | 12 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CategoryId` | 2 | 9 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetPeopleQuery` | 2 | 9 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderCreatedEvent` | 2 | 9 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddHangfireServer` | 2 | 8 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetScheduler` | 2 | 8 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ISchedulerFactory` | 2 | 8 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StandardHandler` | 2 | 8 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetProductQuery` | 2 | 7 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IsEnvironment` | 2 | 7 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MessagingInstrumentationOptions` | 2 | 7 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OfWork` | 2 | 7 | 31/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `PaymentSchedulerId` | 2 | 7 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TryGetAWSCredentials` | 2 | 7 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `EXPONENTIAL_RETRYPOLICYASYNC` | 2 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetAllAsync` | 2 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetBySystemName` | 2 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Handler1` | 2 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Handler2` | 2 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HighPriorityHandler` | 2 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IOrderService` | 2 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RegistryClient` | 2 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddHangfire` | 2 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `EndDate` | 2 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetById` | 2 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetEnvironmentVariable` | 2 | 5 | 4/2 | 4/2 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `ISchemaRegistryClient` | 2 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SensitiveOrder` | 2 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SortBy` | 2 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StartDate` | 2 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `USEast1` | 2 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseSqlServerStorage` | 2 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddOtlpExporter` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `BrighterInstrumentation` | 2 | 4 | 3/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `GetOrderSummaryQuery` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HighPriorityHandlerAsync` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ICustomerRepository` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IEmailService` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IsAny` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PagedResult` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ReadPreference` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ReminderTo` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SendEmailCommand` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StandardHandlerAsync` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StandardOrderHandler` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaskCompleted` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaskId` | 2 | 4 | 5/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `Term` | 2 | 4 | 2/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `TotalPages` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UKOrderCreatedHandler` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `USOrderCreatedHandler` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseRabbitMQ` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `WriteConcern` | 2 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddOpenTelemetry` | 2 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `BreakerName` | 2 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Handler3` | 2 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ItemCount` | 2 | 3 | 1/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `MyApp` | 2 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderPlaced` | 2 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OutboxSweeperOptions` | 2 | 3 | 3/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `RecordMessageInformation` | 2 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ShippingAddress` | 2 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SweepInterval` | 2 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddControllers` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddGreetingCommand` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ApiController` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AsQueryable` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AttemptNumber` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Average` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `BreakerTimeInMilliseconds` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CancelOrder` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CommandProcessorInstrumentationOptions` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CommandProcessorLifetime` | 2 | 2 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `CreateOrder` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateOrderRequest` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `EUWest1` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FindByEmailAsync` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetListAsync` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetPersonQueryHandler` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetSubscriptions` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `InsertAsync` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MapControllers` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MongoClientSettings` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderCount` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderItemDto` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderShipped` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PersonRepository` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Predicates` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Primary` | 2 | 2 | 2/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `RecordMessageBody` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RecordRequestInformation` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RecordServerInformation` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RetryReads` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RetryWrites` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SaveChangesAsync` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SendReminderCommand` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ServiceURL` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Shipped` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TotalValue` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TraceIdentifier` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseConsoleLifetime` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseEndpoints` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseMsSqlOutbox` | 2 | 2 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `UseMySql` | 2 | 2 | 2/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `WMajority` | 2 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Globals` | 1 | 10 | 4/0 | 3/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `ShouldBe` | 1 | 10 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FromQuery` | 1 | 9 | 8/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `ShipOrder` | 1 | 8 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaskReminderCommand` | 1 | 8 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseHangfireDashboard` | 1 | 8 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CheckinInterval` | 1 | 7 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ReadCapacityUnits` | 1 | 7 | 7/0 | 7/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `UseJsonSerializer` | 1 | 7 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `WriteCapacityUnits` | 1 | 7 | 7/0 | 7/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `AddQuartz` | 1 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CustomSchedulerObject` | 1 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `EditTaskCommand` | 1 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UsePersistentStore` | 1 | 6 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ApplicationJson` | 1 | 5 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `BackgroundJob` | 1 | 5 | 1/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `BeforeCommit` | 1 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreatedEvent` | 1 | 5 | 1/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `DiscountPercent` | 1 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ShouldBeTrue` | 1 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SubTotal` | 1 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseClustering` | 1 | 5 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddTickerQ` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AfterOrderId` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Authorization` | 1 | 4 | 4/0 | 2/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `CredentialManagement` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CustomSchedulerAPI` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DashboardOptions` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DbConnectionProvider` | 1 | 4 | 10/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `Details` | 1 | 4 | 4/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `FastHandler` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FirstName` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ITasksDAO` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `InStockOnly` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LargeOrderPlaced` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LastName` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MYCIRCUITBREAKER` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PlacedAt` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SlowHandler` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SortDescending` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StockOnly` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TablePrefix` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaskDescription` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaskName` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseMicrosoftDependencyInjectionJobFactory` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `WorkerCount` | 1 | 4 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddService` | 1 | 3 | 5/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `AllowedBeforeBreaking` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CheckinMisfireThreshold` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CompatibilityLevel` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateDefault` | 1 | 3 | 1/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `CreatePerson` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CustomerOrderSummary` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FailBeforeCommit` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetProduct` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GreetingsSender` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HelloWorld` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HostControl` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IAmAMailGateway` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ImageUrl` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ItemAdded` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MYRETRYPIPELINE` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MYRETRYPOLICY` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MiddleName` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `NameFilter` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OfBreak` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderDetailsDto` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderItem` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PaymentReceivedEvent` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PublishEventCommand` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `QueuePollInterval` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ResourceBuilder` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Resources` | 1 | 3 | 6/0 | 2/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `SetDataCompatibilityLevel` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SetResourceBuilder` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StatisticsIntervalMs` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseRecommendedSerializerSettings` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseSimpleAssemblyNameTypeSerializer` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseTickerQ` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Version_180` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `WasHandled` | 1 | 3 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddGreetingMessageMapper` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddJaegerExporter` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddJobListener` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddPerson` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddQuartzHostedService` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddResiliencePipeline` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AllowEmptyStrings` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ApplyStateContext` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ApprovalHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `BRT007` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CS8600` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CalculateTax` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CancelOrderRefundHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CategoryName` | 1 | 2 | 1/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `CircuitBreakerState` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CommandBatchMaxTimeout` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CommandHandler` | 1 | 2 | 3/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `CompiledQuery` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CompletedAt` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CompletedBy` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CompressedOrderMapper` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CompressionType` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ConfirmationSchedulerId` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateUser` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateUserLatestHandlerAsync` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateUserV1HandlerAsync` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateUserV2HandlerAsync` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateUserV3HandlerAsync` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Cursor` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CustomHeaders` | 1 | 2 | 1/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `CustomerEmail` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DashboardTitle` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DefaultOrderCreatedHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Deserializer` | 1 | 2 | 5/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `DigitalOrderHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DisableGlobalLocks` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DiscountedPrice` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `EUOrderHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `EUPaymentHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ExpiryCheckInterval` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FailureCount` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FallbackCredentialsFactory` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FallbackRegionFactory` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FetchMinBytes` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FetchWaitMaxMs` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FinalizeOrderHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FindById` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FindCustomerByEmailQuery` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FraudCheckHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FromDate` | 1 | 2 | 2/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `FullRefundHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `General` | 1 | 2 | 14/3 | 3/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `GetActiveOrdersQuery` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetCachedCountQuery` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetCredentials` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetCustomerByIdQuery` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetCustomerNameQuery` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetCustomerOrderSummaryQuery` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetCustomerWithOrdersQuery` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetNameByIdAsync` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetOrderDetailsQuery` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetRegionEndpoint` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GreetingCommandHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GreetingCommandMessageMapper` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GreetingEventAsyncMessageMapper` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GreetingEventMessageMapper` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HasMore` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HasNextPage` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HasPreviousPage` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HazmatOrderHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HostOptions` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ICache` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ICourier` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IFeatureFlags` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IInMemoryCache` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IMapper` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IPaymentGateway` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IPricingService` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IProductRepository` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IUserRepository` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IWebHostEnvironment` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IWriteOnlyTransaction` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IncludeOrders` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `InternationalOrderCreatedHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `InternationalPaymentHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `InternationalPaymentHandlerAsync` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IsStarted` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ItemRemoved` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `JapanPaymentHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `JobStorage` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `JsonSerializerDefaults` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LargeDataCommand` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LastAccessed` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LegacyTaxOrderHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LineNumber` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LineTotal` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LocalPaymentHandlerAsync` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LoggingHandler` | 1 | 2 | 1/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `MapHealthChecks` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MaximumOrderValue` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MinimumOrderValue` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ModernTaxOrderHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MyCommandHandlerAsync` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MyImplicitHandlerAsync` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MyTransaction` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `NextCursor` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OnRetry` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderEvent` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PartialRefundHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PayPalPaymentHandlerAsync` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PaymentId` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PaymentProcessed` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PerRecipient` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Period` | 1 | 2 | 5/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `Place` | 1 | 2 | 5/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `PlacedEvent` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PreOrderHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PremiumOrderHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ProcessNotes` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ProcessRefund` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ProcessorCount` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ProductDto` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `QueryHandlers` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RedisStorageOptions` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RegistryConfig` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ReturnAndRefundHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ReturnsAsync` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RuleFor` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SchemaRegistryClient` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Setup` | 1 | 2 | 0/1 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `SimpleCommand` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SimpleCommandHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SlidingInvisibilityTimeout` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SpecialHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SqlServer` | 1 | 2 | 4/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `SqlServerStorageOptions` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SslEndpointIdentificationAlgorithm` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StackExchange` | 1 | 2 | 5/0 | 5/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `StockAdjusted` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StripePaymentHandlerAsync` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaskCompletedEvent` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaskDueDate` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaxCalculator2024` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaxCalculator2025` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaxRate` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TestString` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TickerQDbContext` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TinyIoCContainer` | 1 | 2 | 2/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `ToDate` | 1 | 2 | 1/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `TotalDuration` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TotalPrice` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TrialEndDate` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TrippedAt` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UKOrderHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UKPaymentHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `USOrderHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `USPaymentHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseMemoryStorage` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseNpgsqlConnection` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UsePostgreSqlStorage` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseProperties` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseRecommendedIsolationLevel` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseRedisStorage` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ValidateOrderHandler` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `WaitForJobsToComplete` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `WithMessage` | 1 | 2 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `WriteTo` | 1 | 2 | 7/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `ACTIVE` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AbsoluteExpirationRelativeToNow` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AbstractValidator` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ActionGenerator` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddAspNetCoreInstrumentation` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddAzureMonitorTraceExporter` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddConcurrencyLimiter` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddConsole` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddDashboard` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddEndpointsApiExplorer` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddFallback` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddGreetingResponse` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddHedging` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddHttpClient` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddHttpClientInstrumentation` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddLogging` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddNpgsql` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddOperationalStore` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddS3LuggageStore` | 1 | 1 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `AddSerilog` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddSwaggerGen` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddTransient` | 1 | 1 | 2/3 | 2/3 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `AddTriggerListener` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AddZipkinExporter` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AgentHost` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AgentPort` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AggregateAnalyticsCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Alert` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AlertSeverity` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AmqpUri` | 1 | 1 | 4/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `ApiVersion` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AppSettings` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AutoDetect` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AverageItemPrice` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AverageValue` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `AzCliCredential` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `BeginOrGetTransaction` | 1 | 1 | 4/0 | 4/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `BeginOrGetTransactionAsync` | 1 | 1 | 3/0 | 3/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `Benchmark` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Book` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Break` | 1 | 1 | 14/2 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `BrighterTriggerListener` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `BusinessContext` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `BusinessId` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CacheCircuitBreaker` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CacheKey` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CalculateComplexRouting` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CalculateTotalAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CallAlternativeEndpoint` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CallExternalApi` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CallExternalService` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CanAccessOrderAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CancelOrderCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CancelableOperationCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Categories` | 1 | 1 | 7/3 | 7/3 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `Charge` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ClearDispatchedState` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ClearProviders` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CommandRepository` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CompileAsyncQuery` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ComplexCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ConcurrencyLimiterOptions` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ConfigurationManager` | 1 | 1 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `ConfigureScheduler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Consignment` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ContainsHazardousMaterials` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CountersAggregateInterval` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateChannelFactory` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateGlobalSecondaryIndexAction` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateOrderDto` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreatePersonCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateTracerProviderBuilder` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CreateUserDto` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CustomerCategory` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CustomerCreated` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CustomerQueries` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `CustomerQuery` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Dashboard` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DashboardContext` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DashboardJobListLimit` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DataRow` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DatabaseCircuitBreaker` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DatabaseFixture` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DatabaseUnavailableException` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DaysRemaining` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DelayInMilliseconds` | 1 | 1 | 16/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `DeserializeObject` | 1 | 1 | 15/0 | 15/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `Digital` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DisplayStorageConnectionString` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DistributedCacheEntryOptions` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `DownstreamCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `EUW1` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `EffectiveDate` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `EmailSent` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `EnableGaplessGuarantee` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `EnableSsl` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `EnableSslCertificateVerification` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `EncryptedMapper` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `EnsureCreated` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ExternalApiCircuitBreaker` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FailedCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FallbackAction` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FallbackStrategyOptions` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FetchMaxBytes` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FindByCausation` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FindOrderQuery` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FindUserQuery` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FireRequestMessage` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FirstAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `FromBody` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetActiveOrdersAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetByCustomerIdAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetCount` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetCurrentClassLogger` | 1 | 1 | 7/3 | 7/3 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `GetCurrentUserId` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetCustomer` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetCustomerQuery` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetCustomersQuery` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetDataAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetDefaultValue` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetHttpContext` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetMetaData` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetMonitoringApi` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetOrdersQuery` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetPeopleQueryHandler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetProductDetailsQuery` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetProductListQuery` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GetStatistics` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GlobalJobFilters` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GlobalSecondaryIndexUpdate` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GlobalSecondaryIndexUpdates` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GreetingAsyncEvent` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GreetingMadeHandlerAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `GreetingsReceiver` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HandlerTypeName` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HealthCheckOptions` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HedgingStrategyOptions` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HighVolumeEvent` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `HttpRequestException` | 1 | 1 | 2/0 | 3/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `HttpResponseMessage` | 1 | 1 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `Https` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IApplyStateFilter` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IDashboardAuthorizationFilter` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IDistributedCache` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IJobListener` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IProductCache` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IServerFilter` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ISpan` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ITimerProvider` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IUnitOfWork` | 1 | 1 | 3/0 | 3/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `IndexDdl` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `InfoFormat` | 1 | 1 | 16/9 | 16/9 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `IsActive` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IsAuthenticated` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IsDeleted` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IsInternational` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IsOSPlatform` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `IsPreOrder` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ItemAddedHandler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ItemRemovedHandler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `JobExecutionException` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `JobExpirationCheckInterval` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `KEYS_ONLY` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LargeEvent` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LargePayload` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LastViewed` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LetterConsumer` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LetterQueueHandling` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ListActiveOrdersQuery` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ListRecentProductsQuery` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LoadConfigFromDatabase` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `LogCritical` | 1 | 1 | 3/0 | 3/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `MapToDto` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MarkAsCreated` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MaxHedgedAttempts` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MaxResults` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MaxRetries` | 1 | 1 | 2/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `MemoryProducer` | 1 | 1 | 4/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `MemoryStorage` | 1 | 1 | 3/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `MessageComponentType` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MigrationsAssembly` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MinimumLength` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MinimumLevel` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MisfireThreshold` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MyCommandHandler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MyCustomContextFactory` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MyFeatureSwitchedConfigHandler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MyIncompleteHandler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MyRequestHandlerAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `MySqlStorageOptions` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `NewGreeting` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `NewState` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `NoTaskQueues` | 1 | 1 | 4/0 | 4/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `NormalEvent` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `NotificationEvent` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `NotificationRequest` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `NotificationSent` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `NumberOfJobsExecuted` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OSPlatform` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Octet` | 1 | 1 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `OnClosed` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OnOpened` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OperationCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OperationId` | 1 | 1 | 5/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `OrderApprovalDto` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderData` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderDbContext` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderDelivered` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderDetails` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderLineDto` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderNotFoundException` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderQueries` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderQuery` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderReminderEvent` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderType` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderUpdated` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OrderValue` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OriginalCommandId` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OtherCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OtherCommandHandler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `OtlpExportProtocol` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Outcome` | 1 | 1 | 4/0 | 4/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `PartiallyReturned` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PaymentDeclinedException` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PaymentGatewayUnavailableException` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `People` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PerformedContext` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PerformingContext` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PermitLimit` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PlaceOrderRequest` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Placed` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PrepareSchemaIfNecessary` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ProcessOrderAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ProcessPaymentHandler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ProductQueries` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ProductQuery` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `PushNotificationSent` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `QueueForRetry` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `QueueLimit` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RabbitMqConfiguration` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ReScheduleAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Recipient_Id` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RecordMessageHeaders` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RecordRequestBody` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RecordRequestContext` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Regiter` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ReminderText` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RemovePII` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ReplaceOneAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RescheduleAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ResiliencePropertyKey` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ResponseWriter` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RestorePII` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RetryInterval` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RetryOperationCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `RunningSince` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `S3LuggageStoreCreation` | 1 | 1 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `SamplingDuration` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SaveActionAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SaveChanges` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SchedulePollingInterval` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Scheduled` | 1 | 1 | 3/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `SchedulerInstanceId` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SchedulerName` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SchedulerTimeZone` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SchemaRegistryConfig` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SearchCustomersQuery` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SecureLargeOrderMapper` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SeedOrders` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SendAlert` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SendEmail` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SendNotification` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SendOrderConfirmationAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SendOrderConfirmationCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SendReminderEmailCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SendWelcomeEmailCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SensitiveData` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SerializeAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ServiceControl` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ServiceUnavailableException` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SetBasePath` | 1 | 1 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | NOT SURFACE |
| `SetEndpoints` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SetMinimumLevel` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SetProperty` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SetRequestTimeout` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SetSampler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SetString` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ShouldBeFalse` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ShouldBeOfType` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SingleAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SmsSent` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SomeAsyncOperation` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SomeOperationAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `SomeProperty` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StartChannel` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StatsPollingInterval` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StockAdjustedHandler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StopChannel` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StoreCreation` | 1 | 1 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `StoreWithCausation` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `StringLength` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Substitute` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Succeeded` | 1 | 1 | 6/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `TRequestHandler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TablesPrefix` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaskCompletedHandler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaskCreatedHandler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaskDeleted` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TaskUpdatedHandler` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TestCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TestDatabase` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `ThrowsAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TimeZoneInfo` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TimerCallback` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Times` | 1 | 1 | 61/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `ToDictionaryAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TotalItems` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TradeCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TransactionId` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TransactionIsolationLevel` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TransactionTimeout` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TrialExpiringEvent` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `TrialStartDate` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UndoCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UniqueCustomers` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UnitOfWork` | 1 | 1 | 31/0 | 3/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `UpdateOrder` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UpdateTableAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UpdateTableRequest` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseAuthorization` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseBinarySerializer` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseCloudEventsConventionsAttributes` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseCommandSourcingAsync` | 1 | 1 | 3/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `UseDynamoDbOutbox` | 1 | 1 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `UseDynamoDbTransactionConnectionProvider` | 1 | 1 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `UseFastPath` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseHttpsRedirection` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseInMemoryArchiveProvider` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseInMemoryOutbox` | 1 | 1 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `UseMessagingSemanticConventionsAttributes` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseMisfire` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseMongoDbOutbox` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseMySqTransactionConnectionProvider` | 1 | 1 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `UseMySqlOutbox` | 1 | 1 | 2/0 | 2/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `UseNpgsql` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseOutbox` | 1 | 1 | 2/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `UsePostgres` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseScoped` | 1 | 1 | 5/0 | 5/0 | 0/0 | EXISTED, REMOVED | SURFACE |
| `UseStorage` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseSwagger` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseSwaggerUI` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UseTickerQDbContext` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UserCreated` | 1 | 1 | 1/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `UserDeleted` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `UserUpdated` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `Verify` | 1 | 1 | 4/0 | 0/0 | 0/0 | NEVER EXISTED | substring only at stage 2 |
| `VolumeSubscription` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `WaitForShutdown` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `WaitForShutdownAsync` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
| `YourCommand` | 1 | 1 | 0/0 | 0/0 | 0/0 | NEVER EXISTED | screened at stage 1, 0 history |
