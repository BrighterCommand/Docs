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
