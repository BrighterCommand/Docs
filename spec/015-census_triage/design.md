# Spec 015: Census Triage — Design

**Created:** 2026-09-18
**Status:** **Approved 2026-09-18.** `.design-approved` exists. Q2 was reversed at this review.
**Requirements:** approved 2026-09-18 (`.requirements-approved`), six questions ruled, Q4 and Q6
overturning the recommendation.

> **Every figure below was re-derived for this document, not inherited from `requirements.md`.**
> The command sits beside the number. Measured 2026-09-18 against Docs `master` `df9bb85`,
> `../Brighter` `origin/master` `09f5d988f`, `../Darker` `origin/master` `2f76cda`.
>
> **Three probes were run to write this design, and all three changed it.** Their method, controls
> and output are in [§10](#10-the-probes-method-controls-output). One of them **failed its own
> control on the first run**, and the control was wrong rather than the filter — that is recorded
> in [§10.3](#103-the-control-that-was-wrong-rather-than-the-instrument) because it is the more
> useful half.

## 1. What re-verifying the requirements changed

The requirements were re-read deliverable by deliverable against the instrument and the corpus,
not quoted. **Four deltas, and three of them change what gets built.**

| Deliverable | Requirements said | Design found | Effect |
|---|---|---|---|
| **P0-4's slice** | **112 names** at ≥3 pages | first **103**, then **819 — the whole census** | **Shrank, then was abolished.** The ≥3 figure moved to 103 because the filter must be *per page* to match `DECL_RE`, and both filters report the same headline 819 so the requirements' number could not distinguish them ([§2](#2-p0-1-the-member-filter-and-which-member-filter)). Then **Q2 was reversed** and the slice stopped existing ([§1.1](#11-the-measurement-that-reversed-a-ruling)) |
| **P0-3's method** | a method to be written | **the discriminator it would naturally reach for is already inside the census** | **Changed shape.** A candidate is, by construction, *not* declared on the pages that count it, so "does the page declare it?" cannot triage anything. The method that works is a history query, and it needed its own two-way control — [§4](#4-p0-3-the-triage-method) |
| **P1-1** | *"state how much of finding E's 808-name gap is recoverable"* | **measured here: 270 by methods, 57 more by properties and fields, 481 unexplained** | **Already done.** P1-1 becomes a paragraph to write, not an investigation to run — [§8](#8-p1-1-answered-at-design-time-the-gap-is-not-members) |
| **Probes** | *"already shipped in #167"* | correct, and a **third** was written for this design | Unchanged, plus `perpageprobe.py` |

**Nothing was found already done that the requirements thought was outstanding, and nothing
inverted.** The one that comes closest is P1-1: the requirements offered *"or say the original
filter set is unrecoverable and stop guessing"* as an alternative outcome, and that is the outcome.

### 1.1 The measurement that reversed a ruling

**Q2 ruled the 599-name single-page tail out of scope**, and its *depends on* was explicit:
*"what P0-4 costs per name on the head. Measure there first, then rule."*

This design measured it: the mechanical stage costs **0.4s per name per product**, the expensive
stage runs only on survivors, and on the head **84 of 103 names never reach it at all**. The tail
is minutes, not the grind the recommendation assumed. **The design reported that and did not act on
it**, because reversing a ruling is not a design's to do.

**Ruled 2026-09-18 at the design review: the tail is in, and P0-4 triages the whole census.**

**This section first said the recommendation *"was not wrong about the yield — it was wrong about
the cost"*. Then the tail was screened, and it was wrong about both.** The sentence is replaced
rather than deleted because the correction is the finding:

> **The ≥3 head contains no Brighter API. The 1–2 page tail contains twelve**, including
> `UseMsSqlOutbox`, `CommandProcessorLifetime` and `IUnitOfWork`, on pages including
> **`ShowMeTheCode.md`** and **`BrighterBasicConfiguration.md`**
> ([§5.1](#51-what-the-tail-actually-contained--twelve-dead-brighter-apis)).

*"A single-page single-site name is the least likely to be a real API"* was the load-bearing claim
in Q2's recommendation, it was never measured, and **it is backwards** — for the reason in
[friction 45](#13-workflow-friction). So the ruling did not merely buy the tail cheaply; **it is
what made 015 find anything at all.**

So **P0-4's corpus is all 819 candidates**, not a slice; **P1-2 is withdrawn**, because a policy
about what you did not triage is not needed once you have triaged it; and **AC8 moves rather than
retiring** — what 015 does not triage is now the surfaces the census itself never reaches
([§9](#9-what-015-still-does-not-triage)).

**Three of Q1–Q6's six recommendations are now overturned, and not one of them by argument.** Q4
and Q6 were rulings the maintainer simply took differently; Q2 was reversed by its own stated
condition arriving. **A *depends on* that nobody ever returns to is decoration** — this is the one
that was returned to.

## 2. P0-1: the member filter, and *which* member filter

### 2.1 The two filters the requirements did not distinguish

`methodprobe.py` — the probe the requirements measured and the probe AC1 names as its red-proof —
computes the filter **post-hoc and all-or-nothing**: a name is removed only when *every* page using
it also declares it as a method. That is a property of the finished census.

`DECL_RE`, the type filter already inside `symbolcheck.census()`, does something different. It is
applied **per page, inside the loop** (`tools/symbolcheck.py:462-473`): a token declared on page X
is not a candidate *on page X*, and a page that uses it without declaring it still contributes. A
name declared on some pages and used bare on others therefore **survives with a reduced
page-spread** rather than vanishing or staying whole.

**P0-1 says "restore the member filter". These are two different filters and only one can ship.**

```bash
python3 spec/015-census_triage/probe/perpageprobe.py
```

| | names | ≥7 | ≥5 | ≥4 | **≥3** | ≥2 |
|---|---:|---:|---:|---:|---:|---:|
| shipped today, types only | 929 | 40 | 63 | 73 | 115 | 233 |
| all-or-nothing (`methodprobe`) | **819** | 38 | 60 | 70 | **112** | 220 |
| **per page — what P0-1 ships** | **819** | 36 | 57 | 65 | **103** | 210 |

> **The two filters are indistinguishable by the headline number and differ by nine names in the
> slice the spec was budgeted from at the time.** 819 either way. A design that carried 819 forward
> and trusted it would have inherited the wrong slice — which is
> [friction 43](#13-workflow-friction): *a number that survives a change is not evidence that
> nothing changed.*

**Q2's reversal took the sting out of this without making it wrong.** With the whole census
triaged, both filters yield the **same 819 names**, so the choice no longer changes *what* gets a
verdict — only the band ordering, and the bands are now presentation rather than scope. **The
finding stands and its consequence shrank**, which is worth saying plainly: had Q2 not been
reversed, this would have been the difference between triaging 112 names and the right 103.

**Per page is still the one that ships**, for two reasons — the third, *"it costs nine names of
triage budget"*, died with the slice:

1. **Consistency with the filter it extends.** `DECL_RE` is per-page; a member filter that behaved
   differently would make "page-declared" mean two things in one function.
2. **It is more precise where it differs.** All nine names it moves down a band are the
   documentation's invented domain — `CancelOrder`, `CreateOrder`, `GetById`,
   `GetEnvironmentVariable`, `GetPeopleQuery`, `OrderCreatedEvent`, `PagedResult`,
   `PersonRepository`, `TaskName` — each declared as a method on all but one or two of its pages.
   **No name moved up**, so the change is strictly a demotion of noise.

### 2.2 The change to `tools/symbolcheck.py`

Three lines inside `census()`, beside the existing type filter:

```python
# A method declaration: a modifier, a return type, the name, an open paren.
# Anchored on the modifier, so a bare call `Foo(bar)` cannot match.
MEMBER_DECL_RE = re.compile(
    r'\b(?:public|private|protected|internal|static|async|override|virtual|'
    r'sealed|partial|extern|new)\b[^;=(\n]*?'
    r'\b([A-Za-z_][A-Za-z0-9_]*)\s*(?:<[^>()]*>)?\s*\(')

        declared = set()
        for body in blocks:
            declared.update(DECL_RE.findall(body))
            declared.update(MEMBER_DECL_RE.findall(strip_noncode(body)))   # NEW
```

`strip_noncode` matters on the new line and not on the old one: a method *declaration* inside a
comment is not a declaration, and `DECL_RE` gets away without it because a commented-out `class Foo`
is rare enough to have never mattered. **Keep the asymmetry and comment it**, rather than
"tidying" `DECL_RE` into the stripped body as a drive-by — that would change the type filter's
output in the same commit that changes the member filter's, and neither movement could then be
attributed.

### 2.3 The enumeration's blind spot, measured rather than assumed

`MEMBER_DECL_RE` is anchored on a **modifier**, so a declaration carrying none — `void Handle(Order o)`
in an interface body, or a local function — cannot match it. Friction 36 asks that an instrument
built from an enumeration get a positive case from **outside** the enumeration; here the outside
case is a whole syntax, so it gets a count instead of a boolean:

```bash
python3 spec/015-census_triage/probe/perpageprobe.py | tail -3
```

**One** candidate in the whole census is declared method-shaped with no modifier and therefore
invisible to the filter: `Greeting`, at 7 pages. **The blind spot is one name wide**, which is
what makes "methods only, anchored on a modifier" safe to ship rather than merely convenient.

### 2.4 Q1's breadth question, closed by measurement

Q1 ruled **methods now, the rest measured and not implemented**. The measurement is in
[§8](#8-p1-1-answered-at-design-time-the-gap-is-not-members) and it argues against ever
implementing the rest: properties and fields buy **57 names** against methods' 270, and every one
of them is a wider regex over a report a person reads.

## 3. P0-2: the pin, and the SHA header

### 3.1 What ships

```python
# Q6: the census reads a RECORDED SHA, not whatever origin/master is today, so a
# figure it prints can be reproduced. Refreshing it is a deliberate commit with
# the new count beside it -- constraint 10.
#
# CENSUS-SCOPED. PRODUCT_REFS below feeds --verify-list too, and that gate runs
# on a daily schedule precisely because what invalidates a watchlist row is a
# removal in ANOTHER repository. Pinning the shared constant would freeze the one
# gate whose purpose is noticing the world move.
CENSUS_PINS = {
    'brighter': '09f5d988f',   # origin/master @ 2026-09-16
    'darker':   '2f76cda',     # origin/master @ 2026-09-16
}
```

`universe()` gains a resolution step and the header gains a column:

```python
def resolve_sha(repo, rev):
    """The SHA `rev` names, or CensusError. Exit 2 -- nothing was checked."""
    proc = subprocess.run(['git', '-C', os.path.join(ROOT, repo),
                           'rev-parse', '--short', rev],
                          capture_output=True, text=True)
    if proc.returncode or not proc.stdout.strip():
        raise CensusError(
            f'{repo} cannot resolve {rev}: {proc.stderr.strip()[:120]}. '
            f'A pinned census that silently falls back to a branch is a figure '
            f'wearing another figure\'s SHA')
    return proc.stdout.strip()
```

Printed:

```text
token sets, src/ only, release tag and pinned master per product:
  brighter  10.7.0          c1b8af886    7593 tokens
  brighter  pinned master   09f5d988f    7713 tokens
  darker    4.1.1           ddb71ee       313 tokens
  darker    pinned master   2f76cda       313 tokens
```

**All four refs print their SHA, not two.** A header that prints some of its refs invites the
reader to assume the rest, and two of the four being tags does not make them self-evident — the
tags carry **no `v` prefix** in either repository, which is the sort of thing that costs an hour:

```bash
git -C ../Brighter rev-parse --short 10.7.0    # c1b8af886
git -C ../Brighter rev-parse --short v10.7.0   # fatal: Needed a single revision
```

### 3.2 An unresolvable pin is exit 2, not a fallback

A pin naming a SHA the local checkout has not fetched **must** raise `CensusError`, which
`run_census` already turns into **exit 2 — nothing was checked** (`tools/symbolcheck.py:484-488`).

**Run both ways at the design review, because a function printed in a design is a function nobody
has executed:**

```text
../Brighter  10.7.0       -> c1b8af886      ../Darker  4.1.1      -> ddb71ee
../Brighter  09f5d988f    -> 09f5d988f      ../Darker  2f76cda    -> 2f76cda
../Brighter  deadbeef1    -> CensusError: cannot resolve deadbeef1: fatal: Needed a single revision
```
The tempting alternative — fall back to `origin/master` with a warning — produces a census whose
header says one thing and whose numbers mean another, which is worse than no census.
`tools/README.md`'s exit-code contract already says exit 2 is *nothing was checked*; this is that
contract, not a new rule.

### 3.3 What must not change, and how AC6 proves it

`verify_row` reads `PRODUCT_REFS` directly (`tools/symbolcheck.py:623`) and must keep doing so.
The gate's output is the assertion:

```bash
python3 tools/symbolcheck.py --verify-list    # must still name origin/master, not a SHA
python3 tools/symbolcheck.py                  # 0 findings, 5 entries, 161 pages, 1 silenced
```

## 4. P0-3: the triage method

### 4.1 The discriminator that cannot work, and why it is worth writing down

The obvious first question — *does the documentation declare this name itself?* — **cannot triage
anything**, because it is already inside the instrument. `census()` skips any token in the page's
`declared` set, so **every candidate is, by construction, undeclared on the pages that count it.**
Measured on eight head names, the check returns 0 for seven of them and is uninformative on the
eighth:

```bash
for n in OrderId CustomerId HostBuilderContext ProcessOrderCommand IOrderRepository; do
  printf '%-22s %s\n' "$n" "$(grep -rhoE "(class|record|struct|interface|enum) $n\b" contents/ | wc -l)"
done                                   # 0 0 0 0 0
```

**A triage method that starts by re-running the filter gets a clean sheet and learns nothing.**
This is the design's own instance of *a green from a single-use instrument is a claim about the
instrument*, and it is why the method below is a history query rather than a corpus query.

### 4.2 The three-way verdict, and why two values were not enough

The requirements talk about names that are *dead*. The history splits that into two populations
that want different treatment, and the split is mechanical:

| Verdict | Test | What it means | What P0-5/P0-6 do |
|---|---|---|---|
| **LIVE** | resolves at either ref of either product | the census missed it — a defect in the instrument | fix the instrument, not the page |
| **EXISTED, REMOVED** | resolves nowhere, but appears in `src/` history | **provisional** — the name left the product's source. Whether it was *the product's own API* or a BCL name the product merely used is a person's call, and the pilot shows the difference matters: see [§10.6](#106-the-limit-the-pilot-found-stage-2-cannot-finish-the-job) | watchlist row **and** page repair, **once stage 3 confirms it** |
| **NEVER EXISTED** | resolves nowhere and never appeared in history | invented domain, a misremembered name, or another library's | no row, no repair; counted |

**014's one real discovery is in the third class, not the second, and that is a finding.**

```bash
git -C ../Brighter log -S'IMessageScheduler'    --oneline origin/master | wc -l   # 0
git -C ../Brighter log -S'IAmAMessageScheduler' --oneline origin/master | wc -l   # 17  (live)
git -C ../Brighter log -S'IAmACommandStoreAsync' --oneline origin/master | wc -l  # 18  (removed)
```

**`IMessageScheduler` has never existed in Brighter, anywhere, at any point in its history.** It
was not a removed API; it was a plausible name the documentation asserted. The repair 014 made was
right and its *reason* was one class off. A two-valued vocabulary would have recorded this one the
same way as `IAmACommandStoreAsync`, and the two are not the same defect: one is a name the
product dropped, the other is a name the documentation invented.

### 4.3 The mechanical stage, and the trap inside it

```bash
# does the identifier resolve today, at either ref, in either product?
git -C ../Brighter grep -lwF "$name" origin/master -- 'src/*.cs' | wc -l

# has it EVER been in src/, as a whole identifier rather than a substring?
git -C ../Brighter log -S"[^A-Za-z0-9_]$name[^A-Za-z0-9_]" --pickaxe-regex \
    --oneline origin/master -- 'src/*.cs' | wc -l
```

**Three ways of asking "as a whole word" were tried and two of them are broken instruments**, which
is the `git grep -w` trap from `PROMPT.md` in a new suit:

| Form | `Date` | `IAmACommandStoreAsync` (must stay > 0) | |
|---|---:|---:|---|
| `-S'Date'` — substring | 118 | 4 | matches `UpdateDate`; unusable |
| `-S'\bDate\b' --pickaxe-regex` | **0** | **0** | **broken** — `\b` is unsupported, so everything reads as never-existed |
| `-S'\<Date\>' --pickaxe-regex` | 0 | **0** | **broken** the same way |
| `-S'[^A-Za-z0-9_]Date[^A-Za-z0-9_]'` | 5 | **2** | **works**, and the control survives |

> **The `\b` form returns zero for every name including the positive control.** Without a control
> that must be non-zero, it reads as *"no candidate has ever existed"* — a clean, confident,
> entirely false census. **Plausible zero number eight, and the second one this programme has found
> in a word-boundary flag.**

**`-lwF` is safe here and `-w` was not safe in `PROMPT.md`'s case, for a reason worth stating
once:** `-w` tests the characters *adjacent to the match*, so it behaves correctly exactly when the
pattern begins and ends with word characters. A watchlist symbol is an identifier and qualifies;
`.Handle(` is not and does not. **The rule is a property of the pattern, not of the flag.**

### 4.4 Two stages, because the honest query costs 13× the cheap one

```bash
time git -C ../Brighter log -S'PagedResult' --oneline origin/master -- 'src/*.cs'          # 0.40s
time git -C ../Brighter log -S'[^A-Za-z0-9_]PagedResult[^A-Za-z0-9_]' --pickaxe-regex …    # 5.43s
```

So the method screens cheaply and pays only for survivors:

1. **Stage 1, 0.4s/name/product — plain `-S`.** A zero here is **conclusive**: if the identifier
   ever existed, its substring existed, so a substring miss cannot hide a real name. The cheap
   stage is allowed to be the whole answer only in the direction where it cannot be wrong.
2. **Stage 2, 5.4s/name/product — the bracket-class regex**, run only on stage 1's survivors, to
   throw out substring coincidences.
3. **Stage 2.5, evidence rather than a filter** — for each survivor, whether its historical
   occurrences sit after a `.`. `ConfigurationManager.GetSection(…)` is a call on another type;
   `private readonly IAmACommandStoreAsync _commandStore;` is a type in declaration position. This
   is **printed for the person, never applied as a filter**, because a Brighter extension method is
   called with a dot too — the same collision D12 named and `bclprobe` demonstrated.
4. **Stage 3, a person**, on what survives: read the sites and the diffs, decide *the product's own
   API* against *a name the product's source merely contained*, and write the verdict with its
   evidence and its control.

**The asymmetry is the design.** Stage 1 is conservative in the only direction that matters, which
is what makes it safe to run 84 names through it and never look at them again.

### 4.5 The stopping condition

**AC4 requires one, and the pilot makes it exhaustive rather than heuristic:**

> **The method stops when every name in the census has a verdict.** Not after N unproductive
> names, and not when a budget runs out.

That is affordable because stage 1 is cheap and stage 3 sees only what survives stages 1 and 2,
measured in [§5](#5-p0-4-executing-the-method-and-what-it-will-cost). A heuristic stopping
condition ("stop after 20 consecutive NEVER EXISTED") was drafted and **rejected**: page-spread
ordering puts the invented domain at the top, so any run-length rule stops before reaching the part
of the list most likely to hold a real name. **An ordering chosen to surface signal early is
exactly the ordering a run-length stopping rule misreads.**

**Q2's reversal made the stopping condition simpler, not harder.** While the corpus was a slice,
"exhaust it" needed the slice boundary defended and AC8 had to state what sat outside it. With the
whole census in, **exhaustion is the whole instrument's output** and the boundary argument
disappears — what remains outside is the census's own filters, which is
[§9](#9-what-015-still-does-not-triage).

### 4.6 The planted positive — keep it even now the corpus yields

**The plant was designed for a corpus expected to yield nothing.** While the ≥3 slice was the
scope, a zero was the likely outcome, and a method that returns zero on its only run is
indistinguishable from a method that cannot return anything — this programme has found **eight**
plausible zeros.

The census now yields twelve real names
([§5.1](#51-what-the-tail-actually-contained--twelve-dead-brighter-apis)), so the plant is no
longer load-bearing. **Keep it anyway**, and the reason is not habit:

- The **per-name** verdicts still need controlling. Twelve positives somewhere in 819 rows does not
  demonstrate that row 400 was classified by a working instrument — the plant does, because it is
  checked in the same run as its neighbours.
- **A yield is not a proof of sensitivity.** A method that finds `UseMsSqlOutbox` may still be
  missing a whole class of name it cannot see, and the plant is the only thing that tests the
  *mechanism* rather than sampling the *corpus*.

**So P0-4 runs with two known-removed names planted** — `IAmACommandStoreAsync` and
`UseExternalInbox`, both on the watchlist, both absent from `src/` at both refs, both present in
history. Both must classify **EXISTED, REMOVED**. They are removed from the record before it is
published, and the run that found them is quoted in `triage.md`.

This is obligation 3 with the positive case sourced from **outside the corpus** — which was
originally because the corpus might contain none, and is now because a control drawn from the
corpus tests the corpus rather than the instrument.

## 5. P0-4: executing the method, and what it will cost

```bash
python3 spec/015-census_triage/probe/perpageprobe.py     # the slice: 103 at >= 3 pages
```

| | Names | Cost |
|---|---:|---|
| the whole census after P0-1 | **819** | — |
| stage 1 clears them as **never existed, even as a substring** | **720** | **420s measured**, not estimated |
| stage 2 runs on the survivors | **99** | 99 × 2 × 5.4s ≈ **18 min** |
| **total machine cost** | | **≈ 25 min** |
| **stage 3 — a person reads and rules** | **at most 99** | the only human cost in P0-4 |

The 99 survivors sit **19 at ≥3 pages, 12 at 2 pages and 68 at 1 page** — so **80 of them are in
the band Q2 had ruled out.**

### 5.1 What the tail actually contained — twelve dead Brighter APIs

**The ≥3 slice contains no Brighter API. The tail contains twelve.** Run over the 1–2 page
survivors that look like product surface:

| Name | history | live | pages that still print it |
|---|---:|---:|---|
| `UseMsSqlOutbox` | 2 | 0 | `SweeperCircuitBreaking.md`, `UsingSweeperCircuitBreaking.md` |
| `CommandProcessorLifetime` | 2 | 0 | **`ShowMeTheCode.md`**, **`BrighterBasicConfiguration.md`** |
| `IUnitOfWork` | 3 | 0 | `DispatchingARequest.md` |
| `UseInMemoryOutbox` | 2 | 0 | **`ShowMeTheCode.md`** |
| `NoTaskQueues` | 4 | 0 | `FeatureSwitches.md` |
| `UseMySqlOutbox`, `UseDynamoDbOutbox`, `UseScoped`, `AddS3LuggageStore`, `BeginOrGetTransaction`, `UseDynamoDbTransactionConnectionProvider`, `UseMySqTransactionConnectionProvider` | 2–5 | 0 | — |

And they are **declarations of Brighter's own public surface**, not uses of somebody else's:

```text
-  public static IBrighterBuilder UseMsSqlOutbox(
-  public ServiceLifetime CommandProcessorLifetime { get; set; } = ServiceLifetime.Transient;
-  public interface IUnitOfWork : IAmABoxTransactionConnectionProvider, IDisposable
-  public bool UseScoped { get; set; } = false;
```

> **This is 014's residual gap, found.** *"A dead API written into prose on an existing page —
> uncompiled, and not on `symbolwatch.tsv` — is caught by nothing."* Twelve of them, on pages
> including **`ShowMeTheCode.md`** and **`BrighterBasicConfiguration.md`**, which are where a
> newcomer starts.

**P0-4 still runs**, rather than adopting this design's answer: these verdicts are unrecorded and
uncontrolled per name, and AC5 requires a row for all 819 with the evidence beside each. **A design
that measured the answer has not produced the artefact the spec is for** — and the twelve above are
a *lower bound*, since 87 of the 99 survivors have not been read.

**P0-4's deliverable is `triage.md`**: one row per name in the slice — 103 rows — carrying the
verdict, the two commands that produced it, and the control. AC5 counts the rows against the slice
count the tool reports; AC6 is the criterion that fails quietly, and it is why every row carries a
command rather than an adjective.

## 6. P0-5: routing to the watchlist

Every **EXISTED, REMOVED** name becomes a `tools/symbolwatch.tsv` row: symbol, product,
replacement, evidence, first_seen. **NEVER EXISTED names do not get rows**, and the reason is
written into `triage.md` rather than left implicit: the watchlist's job is to notice a name coming
back, and a name the product never had cannot come back.

**Two rulings from 014 bind here and neither is relitigated:** the replacement column is a **name,
not a type** — `IAmAMessageScheduler` is a marker interface with no members, so a repair that
pastes the replacement verbatim can satisfy the gate and fail to compile — and **an opt-out is
never silent**, so a name kept in prose prints with its count.

**If P0-5 adds rows, `symbolcheck`'s gate figure moves from 5 entries.** That number changes in
`tools/README.md` and nowhere else (constraint 6), and `--verify-list` must stay at 0 findings
with every new row resolving DEAD at both refs of its product.

## 7. P0-6: the repairs, and the condition attached to them

Q4 ruled that a confirmed-dead name is repaired in 015 rather than listed for later. **This design
cannot say how many pages that is, and says so rather than guessing**: it is exactly what P0-4
measures.

**The honest statement of P0-6's scope, written before the run:**

> P0-6 repairs the pages carrying every name stage 3 **confirms** as a removed product API. That
> set is **at least twelve names across at least six pages**
> ([§5.1](#51-what-the-tail-actually-contained--twelve-dead-brighter-apis)), and it is a lower
> bound: 87 of the 99 survivors are unread.

**This section said "expected to be empty" until Q2 was reversed, and the sentence is worth keeping
in view rather than quietly replacing.** The prediction was honest and it was drawn from the ≥3
slice, which is the only place the design had looked. **The slice was not a sample of the census;
it was the part of the census least likely to contain what the spec was looking for**, and nothing
in the ≥3 evidence could have revealed that.

**Two of the twelve are on `ShowMeTheCode.md` and one is on `BrighterBasicConfiguration.md`**, so
P0-6 now has a reader on the other end in the most literal sense — those are starting pages. The
repairs there need the full care of §7's three rules, because a starting page is the worst place
for a block that no longer compiles.

Where there is work, each repaired block:

1. **Carries its `using` directives**, because editing a fence puts it in rule 6's **strict scope**
   under `pagelint --changed origin/master` — error-level, block granularity. A block whose
   omission is genuine says `// ...` and is downgraded to a counted warning rather than silenced.
2. **Builds against the released packages** — never a `ProjectReference` into `../Brighter/src`,
   which vouches for an API nobody can install. AC12.
3. **Changes nothing else about the page.** No banner, no page type, no opening sentence: those
   are settled, and a triage repair that reorganises a page cannot be reviewed as a triage repair.

**P0-6 is the only part of 015 that changes the published site**, so obligation 7 binds on its pull
request: ask before merging, and name the head-ref deletion in the same breath.

## 8. P1-1: answered at design time — the gap is not members

014's finding E recorded that the probe behind `design.md` §3.2 does not exist, and that the
reconstruction lands ~6% high: **2019 against a recorded 1,211, a gap of 808.** The requirements
supposed the remainder was *"properties, fields and locals"*. Measured:

```bash
# the staged count, with each filter set added in turn
python3 spec/015-census_triage/probe/perpageprobe.py --stages
```

| Stage | Candidates | Recovers, cumulative |
|---|---:|---:|
| types only — shipped today | **2019** | — |
| + methods | **1749** | **270** |
| + properties and fields | **1692** | **327** — so 57 more |
| `design.md` §3.2's recorded figure | **1,211** | — |
| **unexplained after both** | | **481** of 808 |

**So P1-1's answer is: 270 of 808 by methods, 57 more by properties and fields, and 481 — three
fifths of the gap — unexplained by member declarations of any kind.** The requirements offered
*"or say the original filter set is unrecoverable and stop guessing"*, and that is the finding:
**whatever the original probe filtered, it was not members.**

**This is why Q1's ruling is right for a reason it was not given.** Methods buy 270 for a regex
with a one-name blind spot; properties and fields buy 57 for a wider regex over a report a person
reads — and neither closes the gap, so neither is a step towards reproducing 1,211. **Do not
"correct" 831 or 1,211 in 014's `design.md`**: they are the numbers that design was approved on,
and 015's job is to explain them, not overwrite them.

## 9. What 015 still does not triage

**P1-2 is withdrawn.** It was *"a written policy on the 599-name tail"*, and Q2's reversal makes it
a policy about work now being done. Withdrawn rather than silently dropped, in `requirements.md`
and here.

**AC8 survives the reversal by moving.** With the whole census triaged, the untriaged set is no
longer a slice boundary — it is everything the census's own filters remove before a candidate is
ever printed. Each figure gets its command in `tasks.md`:

| Not triaged | Why, and roughly how many |
|---|---|
| **Page-declared members** | the **110** names P0-1 itself removes. They are not untriaged so much as *answered* — the page declares them — but the number belongs in the record, because P0-1 is a filter 015 wrote and a filter that hides evidence is the failure mode this spec keeps meeting |
| **`NOISE_EXACT` and `NOISE_PREFIX`** | `System*`, `Microsoft*`, `Task`, `Guid` and the rest — deliberate, documented at `tools/symbolcheck.py:348-359`, and never triaged by anything |
| **Names that resolve** | the point of the instrument; they are live and need no verdict |
| **The prose surface** | D12 conclusion 2: **160 of 161** pages carry an unresolved prose token, so prose is unusable for a census and fine only for a curated watchlist. This is the largest untriaged surface and the one most likely to be misread as covered |

**The last row is the one to write carefully.** A spec that triages 819 names in code fences and
says nothing about prose invites the reading that the documentation has been swept. It has not:
**only its C# blocks have.**

## 10. The probes: method, controls, output

### 10.1 What each one is for

| Probe | Question | Verdict |
|---|---|---|
| `probe/methodprobe.py` | how many candidates are page-declared methods? | shipped in #167; **AC1's red-proof**, and still valid — a name declared on every page using it vanishes under per-page filtering too, so it reports 0 after P0-1 |
| `probe/bclprobe.py` | does resolving against the .NET ref pack help? | shipped in #167 as **evidence against**: it strikes `Date`, `Email`, `Total`, `Product` — the docs' own domain |
| **`probe/perpageprobe.py`** | **per-page or all-or-nothing, and what does each cost?** | **new, this design.** Produced §2's table, §2.3's blind-spot count and §8's staged table |

**`bclprobe.py` is rejected as a filter and adopted as an input.** The reason it fails as a filter —
it removes names the documentation invented, because `System.DateTime.Date` contributes the bare
token `Date` — is not a reason to withhold the same information from a person doing stage 3. **A
filter must be right about every name; evidence on a screen only has to be relevant.**

### 10.2 The drift control — the check every number here depends on

`perpageprobe.py` **copies** `census()` so that what P0-1 would ship is what gets measured, and a
copy drifts. **If it has drifted, every figure in §2, §5 and §8 is measuring something that is not
the tool.**

The control is to run the copy with **no extra filter** and require it to reproduce the shipped
tool exactly:

```python
sc.census(pages())            # shipped candidates stage      -> 2019
stage_count(pages(), [])      # the probe's copy, extra=[]    -> 2019   match
stage_count(pages(), [METHOD_DECL_RE])   # 1749
len(census_per_page(pages()))            # 1749   the two probe paths agree
```

**2019 = 2019, and the probe's two code paths agree at 1749**, so the added line is the only
difference and §2's 819 and 103 are what the three-line change produces. Added at the design
review, because until it was run the design's numbers rested on a copy nobody had compared.

### 10.3 The control that was wrong rather than the instrument

`perpageprobe.py`'s first run **failed its own control**: `IMessageScheduler MUST survive the
filter` returned `False`, which reads as *the member filter eats 014's one real discovery* — the
precise charge that got the BCL filter rejected.

**The filter was innocent.** 014 repaired every site, so the name is not in `contents/` at all:

```bash
grep -rl 'IMessageScheduler' contents/ | wc -l     # 0
```

and a name absent from the corpus cannot survive a filter over it. **A control must be present in
the corpus to test removal from it** — otherwise it is unsatisfiable whatever the instrument does,
which is a plausible zero wearing a control's clothes. The control was replaced with
`HostBuilderContext`, which is on 14 pages and is never page-declared.

> **The first instinct on a failing control is to doubt the instrument. The second should be to
> check that the control can pass at all.** Both halves of a two-way control need that check, and
> the absent half is where it gets skipped.

### 10.4 Controls, as they now stand

```text
positive  ConfigureBrighter removed   : True     (declared on all 14 of its pages)
negative  HostBuilderContext survives : True     (14 pages, never page-declared)
negative  CommandProcessor absent     : True     (resolves; never a candidate)
```

and for the triage method:

```text
positive  IAmACommandStoreAsync  existed-and-removed : True   (18 commits, 0 live)
negative  IAmAMessageScheduler   live                : True   (17 commits, 60 live)
negative  OrderId                never existed       : True   (0 commits, 0 live)
```

### 10.5 The pilot: the triage method run over the whole census

**Run twice — first over the ≥3 slice while Q2 stood, then over all 819 after it was reversed.**
Both runs are kept, because the difference between them is the finding.

Stage 1 over the slice, with its own controls:

```text
controls: cheap screen keeps IAmACommandStoreAsync, drops OrderId -- both OK

slice size                        : 103
screened out by the cheap stage   : 84   (never existed, even as a substring)
survivors needing the 5.4s form   : 19
```

Stage 1 over the whole census, **measured, not extrapolated from the run above**:

```text
names screened                    : 819
stage 1 wall clock                : 420s
screened out (never existed)      : 720
survivors needing the 5.4s form   : 99
  of which >=3 pages              : 19
  of which  2 pages               : 12
  of which  1 page                : 68
stage 2 estimate                  : 99*2*5.4s = 18 min
TOTAL estimate                    : 25 min
```

**80 of the 99 survivors sit in the band Q2 had ruled out**, and twelve of them are dead Brighter
APIs ([§5.1](#51-what-the-tail-actually-contained--twelve-dead-brighter-apis)). The slice run and
the census run agree exactly where they overlap — 19 survivors at ≥3 pages in both — which is what
says the second run is the first one extended rather than a different instrument.

Stage 2, over the 19 survivors, with **two known-removed names planted** into the input:

```text
name                     B.hist B.live D.hist D.live  verdict
Date                          5      0      0      0  EXISTED, REMOVED
AddHours                      2      0      0      0  EXISTED, REMOVED
Repository                    7      0      3      0  EXISTED, REMOVED
BuildServiceProvider          0      0      4      0  EXISTED, REMOVED
TenantId                      4      0      0      0  EXISTED, REMOVED
DbParameter                   7      0      0      0  EXISTED, REMOVED
GetSection                    2      0      0      0  EXISTED, REMOVED
MyCommand, Email, Total, Product, Amount, OrderCreated, DbConnectionString,
Fact, ToListAsync, CountAsync, Pending, UseSqlite   (12)   NEVER EXISTED
IAmACommandStoreAsync         2      0      0      0  EXISTED, REMOVED   <-- PLANTED
UseExternalInbox              2      0      0      0  EXISTED, REMOVED   <-- PLANTED
```

**Both planted positives classify correctly, so the method can find something.** And the result
that matters is the one beside them.

### 10.6 The limit the pilot found: stage 2 cannot finish the job

**Seven real candidates and two planted ones land in the same class, and the seven are not Brighter
APIs.** `Date`, `AddHours`, `GetSection`, `BuildServiceProvider` and `DbParameter` are BCL and
ASP.NET names; they carry history in `src/` because **Brighter's own source used them and later
stopped**. The pickaxe reports a name leaving the source; it cannot report *whose* name it was.

```bash
git -C ../Brighter log -S'[^A-Za-z0-9_]GetSection[^A-Za-z0-9_]' --pickaxe-regex -p \
    origin/master -- 'src/*.cs' | grep -E '^[+-].*GetSection' | head -1
#  -   var configSection = ConfigurationManager.GetSection(BrighterConfigSectionName) as …
```

against the planted positive:

```bash
git -C ../Brighter log -S'[^A-Za-z0-9_]IAmACommandStoreAsync[^A-Za-z0-9_]' --pickaxe-regex -p \
    origin/master -- 'src/*.cs' | grep -E '^[+-].*IAmACommandStoreAsync' | head -1
#  -   private readonly IAmACommandStoreAsync _commandStore;
```

**One is a call on another type; the other is a type in declaration position, and the tell is the
dot.** So stage 3 gets one more piece of mechanical evidence — whether the historical occurrences
are preceded by `.` — presented **as evidence, never as a filter**, for the reason D12 gave and
`bclprobe` demonstrated: every cheap filter here is a name filter, and names collide. A Brighter
extension method is called with a dot too.

> **`EXISTED, REMOVED` is a provisional verdict, and the design says so before the run rather than
> after.** It means *this name left the product's source*, not *the product removed this API*. The
> final verdict is a person's, which is the same boundary D12 drew and the same one the 757
> using-directive blocks sit on: **only a compiler resolves a reference.**

**All seven of the slice's survivors were read at the design review**, because §5's then-*expected
zero* had otherwise been an eyeball on a list of familiar-looking names — and two of them, `Repository` at 7 Brighter and 3
Darker commits and `TenantId` at 4, are not obviously BCL:

| Name | What the history actually contains | Own API? |
|---|---|---|
| `Repository` | `GetPropertyPortable("Repository")` — a **string literal** in log4net reflection, and a local `repositoryProperty` | no |
| `TenantId` | `_azureTenantIdKey = "AZURE_TENANT_ID"`, `private readonly string _azureTenantId` — Azure credential plumbing, private | no |
| `DbParameter` | `private DbParameter CreateSqlParameter(…)` — `System.Data.Common.DbParameter` in a **private** signature | no |
| `AddHours` | `DateTime.UtcNow.AddHours(-1 * hoursDispatchedSince)` | no |
| `Date` | `Get<T>(DateTime date, …)` — a parameter name | no |
| `GetSection` | `ConfigurationManager.GetSection(…)` | no |
| `BuildServiceProvider` | Darker's use of the ASP.NET extension | no |

**Seven of seven are uses, not surfaces** — which is why the ≥3 head yields nothing, and why
[§5.1](#51-what-the-tail-actually-contained--twelve-dead-brighter-apis)'s twelve all come from
below it. That is what turns the head's zero from a guess into a measurement — and it is the same shape as the `ConfigureBrighter` case the requirements opened
with, which also sat at the top of a list looking exactly like a dead API.

## 11. Gate predictions

**`tools/README.md` carries the expected figure for each gate at a named ref; this section names
movement and cause, and cites rather than pastes.**

| Gate | P0-1…P0-5 | P0-6 | Cause, or why none |
|---|---|---|---|
| `linkcheck` | **none** | **none expected** | `spec/` is in `SKIP_DIRS` (`tools/linkcheck.py:52`), so every document 015 writes is outside the corpus — checked, not assumed. A repair that adds or retargets a link would move it, and any repair that does **re-runs it** |
| `pagelint` | **none** | **the 757 warning count may FALL** | P0-6 repairs `using` directives in the blocks it edits. **Name the blocks that moved it** — a debt figure that drops for unexplained reasons stops meaning anything. `--changed` must be **0 errors**, which is AC12 |
| shape | **none** | **none** | `SUMMARY.md` is untouched: no page is created, nested or moved |
| redirects | **none** | **none** | redirects follow `SUMMARY.md`, which is untouched |
| `versioncheck` | **none** | **none expected** | it reads version pins in pages; P0-6 edits fences. A repair landing on a pinned-version block would move it and must say so |
| `optioncheck` | **none** | **none expected** | it reads option tables; P0-6 edits fences, not tables |
| `--verify` | **none** | **none** | published-URL set is unchanged, for the same reason as shape |
| `symbolcheck` | **the entry count MOVES if P0-5 adds rows** | — | new watchlist rows change *5 entries*. That number changes in `tools/README.md` and nowhere else; `--verify-list` must stay at **0 findings** with every new row DEAD at both refs |

**Two predictions of "none" are the ones worth distrusting**, and both are named above rather than
assumed: `linkcheck`'s, because `tools/` being inside its walk moved it 164 → 165 once before and
"a gate's scope is not the site's scope"; and `pagelint`'s repo-wide count, because it is the one
number P0-6 is most likely to move without anyone intending it.

## 12. Sections this design makes N/A

| Section | Why |
|---|---|
| **The outline, file by file** | 015 creates **no page**. `triage.md` and `tasks.md` are spec documents under `spec/`, which is not published, carries no banner and has no page type — the four-type vocabulary is about `contents/`. P0-6 *edits* published pages and is forbidden from changing their type, banner or opening sentence ([§7](#7-p0-6-the-repairs-and-the-condition-attached-to-them)) |
| **SUMMARY.md changes** | A category error here, as the requirements said, and **still true after Q4**: P0-6 edits pages that already exist and creates none, and `SUMMARY.md` carries pages, not their contents |
| **Nesting and redirects** | Nothing moves, so no URL moves, so no redirect is owed. This is the deliverable that gets forgotten, which is why it is named rather than omitted |
| **Glossary terms** | No new concept reaches a reader. The vocabulary in [§4.2](#42-the-three-way-verdict-and-why-two-values-were-not-enough) is internal to `triage.md` |

## 13. Workflow friction

**43. A number that survives a change is not evidence that nothing changed.** The member filter
measures **819 candidates whether it is applied per page or all-or-nothing**, and the two differ by
nine names in the ≥3 slice — the slice this spec's budget, its AC5 count and its P0-4 task list are
all derived from. The requirements carried 112 in good faith, having re-derived the headline
number by two agreeing methods, because **both methods agreed on the number that did not
discriminate.** Standing obligation 1 asks for two methods that agree; this is the case where
agreement is the trap. **Re-derive the number the decision turns on, not the number the document
leads with.** Same family as friction 42 — a claim about an instrument that nothing checks — one
layer down: the claim is a count, and the count is true.

**44. A control that cannot pass is not a control.** `perpageprobe.py` asserted that
`IMessageScheduler` must survive the member filter, and it failed — reading as *the filter eats
014's one real discovery*. The name is absent from `contents/` entirely, so the assertion was
unsatisfiable whatever the filter did. **The first instinct on a red control is to doubt the
instrument; the second should be to check the control can go green at all.** The absent half of a
two-way control is where this gets skipped, because absence is what it is testing for.

**45. Page-spread ordering buries exactly what it was chosen to surface.** 014 sorted the census by
page-spread *"because that ordering is what found `IMessageScheduler`"* — a name on seven pages
being likelier to be a real API than a one-page example type. Measured over the whole census, the
opposite holds: the ≥3-page head contains **no** Brighter API, and the 1–2 page tail contains
**twelve**, including `UseMsSqlOutbox`, `CommandProcessorLifetime` and `IUnitOfWork`.

The mechanism is obvious in hindsight and was invisible in front of it. **What repeats across many
pages is the documentation's own example domain** — `OrderId` on 31 pages, `CustomerId` on 17 —
because every page needs a domain to show. **A real API that was removed is mentioned where it was
relevant, which is once or twice.** So page-spread ranks the invented domain highest and the dead
product surface lowest, and a slice taken off the top is a slice taken from the wrong end.

**`IMessageScheduler` at 7 pages was not the rule; it was the exception that set the rule** — and
it turns out not even to have been a removed API ([§4.2](#42-the-three-way-verdict-and-why-two-values-were-not-enough)).
**One confirming instance, generalised into an ordering, then used to justify a budget.** Keep the
ordering — it costs nothing and reads well — but never again let it decide what gets *skipped*.

## 14. Design quality checklist

- [x] Self-contained — 014's finding E, D12 and the requirements' rulings are quoted, not cited
- [x] Every figure re-derived for this document, command beside it
- [x] Every API named is resolved with the command shown — `IMessageScheduler`,
      `IAmAMessageScheduler`, `IAmACommandStoreAsync`, `UseExternalInbox`, `ConfigureBrighter`
- [x] Gate movement predicted for all eight, including six "none" with reasons
- [x] The probes' method, controls and output are here, including the control that was wrong
- [x] N/A sections named and reasoned rather than omitted
- [x] Detailed enough to write from — the regex, the three lines it goes beside, the header format,
      the two-stage query and its costs are all here
