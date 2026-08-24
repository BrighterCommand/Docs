# Spec 009: Getting Started Tutorials — Tasks

**Created:** 2026-08-23 · **Status:** **REVIEWED AND APPROVED 2026-08-24** — four findings
applied in place; the total moved 38 → 39. No verdict, gate, priority or sequencing decision
moved. See § *Re-derive the total* and Task 3.5.
**Works from:** `design.md` (approved 2026-08-03, `.design-approved`; six review findings
applied) and `requirements.md` (approved 2026-08-03)
**Executes against:** a corpus that **Spec 010 moved after this spec was approved** — see §2.

**Total tasks: 39, across 12 phases. 3 done — Phase 1 complete, 2026-08-24.** Re-derived,
not incremented: `grep -c '^- \[x\] \*\*Task'` says 3 and `'^- \[ \]'` says 36.

---

## 1. How this list is organised

**One phase is one pull request**, the same contract Spec 010 ran under: a coherent unit,
merged before the next branch starts. What differs here is that **009 opens pull requests
against two repositories** — four phases against `../Brighter` under the `CLAUDE.md` samples
exception, eight against this one — and a phase is never both.

So 010's *Phase N is PR N* does **not** carry over: the Brighter PRs are numbered by a
repository whose sequence is not ours, and pretending otherwise would put a number in this
document that never matches the one on GitHub. Phases are numbered; PRs are referred to by
the repository they land in.

| Phase / PR | Repo | Goal | Tasks | Deliverables |
|---:|---|---|---:|---|
| **1** | Docs | Plan ratified, the corpus re-measured, #67 re-checked | 3 | this document |
| **2** | Docs | **The five missing glossary terms.** Gates rungs 3 and 4 | 3 | D12 |
| **3** | Docs | **Rung 1**, and the conventions entry it obliges. The only rung exposed to no Brighter PR | 5 | D1 |
| **4** | Docs | The version gate and the release checklist | 5 | D9, D11 |
| **5** | **Brighter** | Rung 2's sample | 3 | D5 |
| **6** | Docs | **Rung 2** | 3 | D2 |
| **7** | **Brighter** | Rung 3's sample | 3 | D6 |
| **8** | Docs | **Rung 3** — the transactional guarantee, both halves | 3 | D3 |
| **9** | Docs | The ladder's landing page (P1) | 2 | D10 |
| **10** | **Brighter** | Rung 4's sample (P1) | 3 | D7 |
| **11** | Docs | **Rung 4** — Kafka (P1) | 3 | D8 |
| **12** | Docs | Acceptance — AC1–AC8 walked — and close | 3 | — |

**Re-derive the total, do not increment it.** `grep -c '^- \[.\] \*\*Task' tasks.md` is the
authority; the phase column above sums to 39 independently. Spec 010 carried two counting
methods that disagreed by four for a fortnight while both printed a plausible number.

**It was 38 at draft, and the review moved it.** Task 3.5 was added — a `CLAUDE.md` entry
design asked for in a paragraph that no box covered. Both figures above were re-derived
afterwards rather than incremented, and they disagreed by one until the phase table was
corrected too, which is the whole reason for keeping two.

**Phases are individually shippable and individually abandonable.** Design § *If a Brighter
sample PR stalls* already rules this: what ships is whatever prefix of the ladder is
complete, and a two-rung ladder is a coherent published state rather than a broken one. The
`SUMMARY.md` rule below is what makes that true mechanically.

**There is no phase that must go in whole or not at all.** 010 had one — PR 2, the tree —
because every later PR filed into it. Here the nearest equivalent is Phase 2, and it is only
a *gate*: rungs 3 and 4 link glossary anchors that do not yet exist, so Phase 2 precedes
them or `linkcheck.py` fails on MISSING ANCHOR.

### Dependencies

**Stated as gates rather than drawn.** This is a DAG, not a tree — Phase 11 alone is gated by
four phases — and an ASCII diagram that carried the spine while omitting the cross-links
would be a picture that disagrees with the list below it. A first draft of this section drew
one, and it hung Phase 10 off Phase 7 when sample 04 is a delta from sample **02**.

- **2 gates 8 and 11.** `at-least-once` and `Box Provisioning` are linked by rung 3;
  `partition`, `consumer group` and `offset` by rung 4.
- **3 gates 4.** `versioncheck.py` scans tutorial pages. Landing it before any page exists
  gives a gate whose first CI run is vacuous — see §2.9, which is why it moved.
- **5 gates 6**, **7 gates 8**, **10 gates 11** — AC4: the companion sample's PR is merged
  before its page ships.
- **5 gates 7 and 10.** Samples 03 and 04 are each *one delta* from sample 02, which is the
  pedagogic point and the review argument (design § Sample Projects).
- **6 gates 8**, and **6 and 8 gate 11** — a rung's *Prerequisites* banner segment links the
  rung below it, and `linkcheck.py` checks it like any other internal link.
- **9 may ship earlier**, listing whatever prefix of the ladder exists. It is placed after 8
  because that is when the list stops changing.

### The standing obligations — every page-writing task owes all eight

Do not restate these in each task; they are assumed by all of them.

1. **Tutorial discipline.** One path, no branching, no option tours. Where a real choice
   exists (the Sweeper versus `ClearOutbox`; Box Provisioning versus self-managed DDL) the
   page takes one and names the other in a single sentence in *Further Reading*. Every step
   states its expected result — console line, table row, or UI element.
2. **Every page carries its banner** one blank line below the H1, separator ` · ` (U+00B7):
   `> **Tutorial** · Applies to **Brighter V10**`, plus a *Prerequisites* segment naming the
   rung below. Every `##` heading is qualified by its subject and unique across pages; the
   five navigation headings are exempt and stay uniform. `## Step N: …` headings are the
   documented deviation from the `CLAUDE.md` skeleton (design § Style Notes).
3. **Every page gets its `SUMMARY.md` entry in the same commit that creates it.** Never
   before: a line pointing at a file that does not exist fails `linkcheck.py` with MISSING
   FILE, and the orphan check has no exemptions.
4. **Append a row to `spec/011-authoring_conventions/pagetypes.tsv` for every page created** —
   `verdict` = `Tutorial`, `applies` = `Brighter V10`. **Append; never re-sort.** No tool
   reads this file except `apply_banners.py`, and `pagelint.py` never reads it at all, so a
   missing row is invisible to every green build and silently skips that page at the next
   version bump. This is 010's standing obligation 7, which it had to add at review for
   exactly this reason.
5. **Run the gates after every page, and read the whole output.** `linkcheck.py`,
   `pagelint.py`, `pagelint.py --changed origin/master`, `urlmap.py --check-shape`,
   `urlmap.py --check-redirects`. **`git add` first** — `git diff` cannot see untracked
   files, so a brand-new page contributes no strict ranges until it is staged, and the
   vacuous pass is indistinguishable from a real one. Read `--changed`'s scope line: it
   prints how many code blocks the diff reached, and that figure is what says whether the
   run meant anything.
6. **Every C# block is complete and carries its `using` directives.** These pages are 100%
   added lines, so rule 6 is strict on every block in them under `--changed`. `// ...` is
   permitted only for configuration a previous rung already showed in full, and then only
   with a link back to that rung — it downgrades to a warning and is still counted, never
   silenced.
7. **Page blocks match the sample's `.cs` file below its licence region**, and the `.csproj`
   is the other documented exception — the page shows `PackageReference` with a pinned
   version because that is what a reader creating a fresh project has. **Each page states
   that divergence in one line** so nobody trips over it.
8. **Expected-output blocks are captured verbatim from a real run**, never hand-written. A
   tutorial that predicts output the reader does not see is indistinguishable from a broken
   one.

### The Brighter-repo obligations — every sample task owes all four

1. **Samples directories only**, always by pull request off `origin/master`, never a direct
   commit. `src/`, `tests/`, `docs/adr/` and release notes remain strictly read-only.
2. **Register every new project in `Brighter.slnx`** in the same PR. Being under `samples/`
   does not make a project built — see §2.4, where one already is not.
3. **Branch from `origin/master`, not from the local working tree**, which is dirty and 140
   commits ahead of the release (§2.6).
4. **Each sample is one readable delta from the one below it.** If a block would need
   cutting to fit its page, the sample is too big for the rung.

### Two conventions this document holds itself to

- **A page's length is `len(text.splitlines())`** — not `wc -l`, which under-reports files
  with no trailing newline, and not `read().split("\n")`, which over-reports those with one.
  Every figure in §2 was derived that way on 2026-08-23.
- **Every figure quoted here was measured today**, at `c195809`. None is carried forward from
  `design.md`, because eight of design's Docs-side figures have since moved (§2.8).

---

## 2. What this list settles — nine items, all measured 2026-08-23

Spec 009's design was approved **2026-08-03**. Spec 010 completed **2026-08-22** and moved
the tree underneath it; Spec 011 completed in between and put a banner on every page. So the
first job of this phase is not planning — it is re-measuring, because *a plan that ships
months after it was approved is measuring a different file*.

### 2.1 `SUMMARY.md` already has a `## Get Started` section, and it is not ours

Design § *SUMMARY.md Changes* shows a *Before* of `## Overview` holding `ShowMeTheCode.md`,
`BasicConcepts.md` and `WhyBrighter.md`, and an *After* that adds `## Get Started` above it.
**Neither block describes the file today.** 010 replaced the nineteen-section tree with
twelve sections, and section 1 is *already called* `## Get Started`, holding those same three
pages:

```markdown
## Get Started

* [Why Brighter?](/contents/WhyBrighter.md)
* [Basic Concepts](/contents/BasicConcepts.md)
* [Show me the code!](/contents/ShowMeTheCode.md)
```

**Settled: the five new entries join that section; no new section is created.** The ladder
goes above the three orientation pages, which is design's on-ramp-first intent honoured
against the tree that exists:

```markdown
## Get Started

* [Get Started with Brighter](/contents/GetStarted.md)
* [1. Your First Command](/contents/TutorialFirstCommand.md)
* [2. Your First Message Over a Broker](/contents/TutorialFirstMessage.md)
* [3. Adding a Durable Outbox](/contents/TutorialDurableOutbox.md)
* [4. Streaming with Kafka](/contents/TutorialStreamingWithKafka.md)
* [Why Brighter?](/contents/WhyBrighter.md)
* [Basic Concepts](/contents/BasicConcepts.md)
* [Show me the code!](/contents/ShowMeTheCode.md)
```

Two things this does not cost, both measured rather than assumed:

- **No URL moves.** A page's slug comes from its **filename**, not from its `SUMMARY.md`
  title or position — 010 Phase 10 established this by repointing an entry at a real file
  and watching the URL move. Re-ordering inside a section is therefore free: no redirect
  entry, no `--check-redirects` change.
- **No shape failure.** S2 caps a section at 12 top-level entries. This section goes from
  **3 to 8**; today's widest is 10 of 12. S1 (≥2 pages) and S3 (≤4 segments) are untouched.

The *two front doors* problem requirements raised is unchanged in substance and now sits
inside one section rather than across two, which makes it easier rather than harder:
`GetStarted.md` carries the one sentence sending evaluators to `ShowMeTheCode.md` and
learners down the ladder.

### 2.2 All five D12 terms are still absent — and the twelve present anchors still resolve

`PROMPT.md` flagged that session 23 added nine terms to `Glossary.md` and that D12's five
should be re-checked rather than assumed. **Checked: all five are still absent.** Resolved
through `linkcheck.py`'s own `slug()`, over `Glossary.md` at 678 lines and 109 headings:

| Anchor | State |
|---|---|
| `#at-least-once` `#box-provisioning` `#partition` `#consumer-group` `#offset` | **absent — D12 adds all five** |
| `#command` 22 · `#event` 30 · `#command-processor` 87 · `#handler` 139 · `#outbox` 192 · `#sweeper` 216 · `#subscription` 289 · `#publication` 295 · `#routing-key` 383 · `#reactor` 417 · `#message-pump` 441 · `#partition-key` 533 | present, all twelve |

**Design's line numbers for those twelve are stale** — it records `#partition-key` at `:444`
and it is at `:533` — but every anchor resolves, which is the property that matters. Cite the
table above, not design's.

**`#dispatcher` is unblocked.** Design § D2 says `Dispatcher` is defined twice
(`Glossary.md:95` and `:393`) and tells the writer not to link it until 011 step 6 merges
them. **That merge landed.** There is now one `### Dispatcher` at `:119`; the other two
Dispatcher-bearing headings — `## Dispatcher and Consumers` and `### Agreement Dispatcher` —
slug differently and do not collide. Rung 2 may link `#dispatcher` directly.

`BoxProvisioning.md`'s `## When to use Box Provisioning` is at `:14` and its anchor resolves,
so rung 3's *Further Reading* link is good as design writes it.

### 2.3 `Tutorial` is a page type no page in this corpus has ever used

`pagetypes.tsv` is 142 rows: **55 Reference, 53 How-to, 34 Explanation, 0 Tutorial**.
Spec 009 introduces the first five. `BANNER_RE`'s `Tutorial` alternative has therefore never
matched a real page, and `apply_banners.py` has never written one.

Nothing here is expected to break — the vocabulary is a literal alternation and `Tutorial` is
first in it — but *"a rule that never fires is invisible to everything but an enumeration"* is
this programme's own lesson, and the cost of checking is one command. **Task 3.3 asserts it
on the first page rather than at the acceptance pass**, where a surprise would be expensive.

### 2.4 A sample under `samples/` is **not** necessarily built by Brighter's CI

This is the finding most likely to bite, and it undercuts a premise stated in three places —
`CLAUDE.md`'s exception, 009's README, and AC4 — all of which argue that tutorial code
belongs in `samples/` because *Brighter's CI compiles it there*.

Measured at `21e558772`:

- `.github/workflows/ci.yml` builds with `dotnet build --configuration Release` at the
  repository root, with no project argument. That resolves to `Brighter.slnx`.
- `Brighter.slnx` names **109** sample `.csproj` files. There are **110** on disk.
- The one it does not name is
  `samples/WebAPI/WebAPI_mTLS_TestHarness/TodoApi/TodoApi.csproj` — a sample that sits in
  `samples/`, is not in the solution, and is therefore **never built by CI**.

So the guarantee is real but conditional, and the condition is a manual registration step
that no checklist in either repository mentions. **Settled: every sample PR adds its projects
to `Brighter.slnx`, and the acceptance check for AC4 is presence in the solution plus a green
CI run that names the new projects — not presence in the directory.** Standing Brighter
obligation 2 carries it.

~~Reporting `TodoApi` upstream is out of scope for this spec~~ — **raised 2026-08-24 at the
maintainer's direction**, rather than left as an observation on a later PR:
**[Brighter#4272](https://github.com/BrighterCommand/Brighter/issues/4272)** for the defect,
**[#4274](https://github.com/BrighterCommand/Brighter/pull/4274)** fixing it (five lines of
`Brighter.slnx`; the sample builds clean, so nothing was keeping it out), and
**[#4273](https://github.com/BrighterCommand/Brighter/issues/4273)** for a CI guard that fails
the build when any `.csproj` is unreferenced.

**None of the three is a 009 deliverable.** Do not track them in this list and do not block a
phase on them. Standing Brighter obligation 2 holds whether or not the guard ever lands — it
is what makes AC4 true for *our* samples.

> **Read #4273 before writing a guard here.** Two details in it each cost a wrong answer
> first. Compare against **`git ls-files`, not `find`**: `find` reads the working filesystem,
> so on a case-insensitive macOS checkout it reports
> `src/Paramore.Brighter.MessageScheduler.Aws` where the repository — and Linux CI — both have
> `.AWS`. That phantom mismatch does not exist on the runner, and it was nearly published as a
> second finding. And **scope the whole repo**: `.csproj` files live under five top-level
> directories, not just `samples/`.

### 2.5 The pin is 10.7.0 today, and the two authorities agree

- NuGet, `api.nuget.org/v3-flatcontainer/paramore.brighter/index.json`, highest
  non-prerelease: **10.7.0** (preceded by 10.6.0, 10.5.1, 10.5.0, 10.4.2).
- `../Brighter/release_notes.md`: `## Master` then `## 10.7.0`, so the latest *released*
  heading is **10.7.0** too.

They agree, which is what D9 expects and reports rather than silently resolves. **Do not
carry 10.7.0 into a page from this line** — re-derive it at writing time, which is
`versioncheck.py`'s whole job and takes one command.

### 2.6 The Brighter working tree is dirty and 140 commits past the release

`git describe` gives **`10.7.0-140-g21e558772`**, and two files are modified in the working
tree (`.agent_instructions/documentation.md` and a file under `specs/`). Neither is ours.

**Branch every sample PR from `origin/master`.** Do not branch from the local tree and do not
`git add -A` in it — `git add -A` obeys the branch's `.gitignore`, not the one you were
working under, which is how 2,862 lines of local notes reached a published branch in session
23. The same hazard, in the repository where we have the least standing to make a mess.

### 2.7 What `versioncheck.py` must not be allowed to do: pass vacuously

Design § D9 fixes the authority (NuGet), the fallback (`--release-notes PATH`), the patterns
and the exit codes. It does not say what happens when a page in `TUTORIAL_PAGES` **does not
exist yet**, which is the normal state for most of this spec's life — and a gate that
silently scans nothing is the single failure mode this programme has hit most often.

**Settled, and Task 4.1 implements it:**

- A **listed page that does not exist is skipped, and the skip is printed.** The ladder ships
  incrementally by design, so absence is expected — but it is never silent.
- A **page that exists and contributes zero pins is an error** (exit 1). A tutorial whose
  prose pins no version is a tutorial the reader cannot reproduce, which is what the pin is
  for.
- The tool **prints its scope before its verdict**: pages listed, pages found, pins examined.
  `0 stale pins` out of `0 pins examined` and `0 stale pins` out of `7` must not print the
  same line.
- **Exit 2 — authority unreachable — is not a pass**, and CI treats it as a failure.

### 2.8 Eight of design's Docs-side figures have moved; none of its Brighter figures has

Both halves of this are load-bearing, and they point in opposite directions.

**The Brighter citations all still hold, line-exact, 140 commits later** — spot-checked
today: `Subscription.cs:201` (`noOfPerformers = 1`), `KafkaSubscription.cs:298`
(`MessagePumpType.Reactor`), `KafkaMessageConsumer.cs:47` (`IAmAMessageConsumerSync`) and
`:122` (`commitBatchSize = 10`), `KafkaMessagePublisher.cs:79` (`Key =
message.Header.PartitionKey.Value`), `PostgreSqlBoxProvisioningExtensions.cs:17,48`
(`AddPostgreSqlOutbox`), and `AddGreetingHandlerAsync.cs` still in place. **Cite design for
these; do not re-derive them.** Q4 and Q5 stand unchanged.

**Every Docs-side page length design quotes is now wrong:**

| Page | 009 says | Today | Why |
|---|---:|---:|---|
| `BrighterBasicConfiguration.md` | 1,068 | **244** | 010 split it |
| `BrighterOutboxSupport.md` | 514 | **271** | 010 split it |
| `RabbitMQConfiguration.md` | 565 | **337** | 010 split it |
| `KafkaConfiguration.md` | 606 | **615** | +9 |
| `ReactorAndProactor.md` | 440 | **449** | +9 |
| `ShowMeTheCode.md` | 221 | **230** | +9 |
| `WhyBrighter.md` | 66 | **75** | +9 |
| `BasicConcepts.md` | 137 | **194** | +9, plus 011's per-term glossary links |

**The +9 is not noise and it is the same nine lines on every unsplit page**: six of
`description:` front matter from 010 Phase 10, the banner from 011, and the blank lines
around them. That the deltas are fully explained is the check — an unexplained delta would
mean something else moved too.

The consequence for writing is small but real: requirements calls
`BrighterBasicConfiguration.md` *"1,068 lines, every option, no single path through it"* and
uses that as the reason the tutorials exist. **The page is now a 244-line hub**, and the
argument has to be made about the family it heads rather than about one page's length. The
tutorials are still needed; the sentence justifying them needs rewriting when a page quotes
it. *A heuristic in the style guide is not an acceptance criterion*, and neither is a line
count from a tree that has moved.

### 2.9 D9 moves from design's sequencing step 6 to Phase 4

Design § Sequencing puts `versioncheck.py` at step 6, after rung 3. **It moves to Phase 4,
immediately after rung 1**, and this is a sequencing refinement rather than a design change —
design's own condition for step 6 was that Spec 011 had landed `.github/workflows/docs.yml`,
which it has, at line 94, guard at lines 102–106.

The reason is §2.7. Landing the gate before any tutorial page exists gives it several phases
of green vacuous runs; landing it immediately after rung 1 means **its first CI run scans a
real page and finds a real pin**, and Task 4.2 can prove it red by bumping that pin. It also
brings forward *the cheapest win in the programme*: the CI slot is already built, so D9 is
one file plus deleting five lines of guard.

Nothing else in design's sequencing moves.

---

## Phase 1 — Plan (Docs PR)

**Goal:** this list, reviewed; the errata of §2 recorded where a writer will meet it; #67
re-checked before the ladder's shape is final.

**This phase must not:** re-open `requirements.md` or `design.md`. Both are approved. §2 is
errata *about* them — corrections of fact that the corpus forced — and it does not move a
single verdict, threshold or ruling. The one edit permitted to an approved document is a
dated pointer line, and only where a writer following the document would otherwise be
misled.

- [x] **Task 1.1:** Write this task list and take it through review — **DONE 2026-08-24**
  - Input: `design.md`, `requirements.md`, `spec/010-information_architecture/tasks.md` §1 as
    the structural model
  - Output: `spec/009-getting_started_tutorials/tasks.md`; `.tasks-approved` on approval
  - Notes: `spec/.current-spec` was repointed from 010 to 009 as part of this task

- [x] **Task 1.2:** Add dated pointers from the two places in `design.md` a writer would be
      misled by — **DONE 2026-08-24, and it was three places, not two**
  - Input: §2.1 and §2.2 above
  - Output: one line beneath design's *SUMMARY.md Changes* Before/After block pointing at
    §2.1; one line beneath the D12 anchor table pointing at §2.2
  - Notes: **Pointers only** — do not rewrite the blocks. The stale text is evidence of when
    it was written, and §2 is where the correction lives. Do not touch the eight page lengths
    of §2.8: no writer reads a line count as an instruction.
  - **What it found:** a **third** place, `design.md` § D2 (`:266–268`), which tells the
    writer *"do not link before it lands"* about `#dispatcher`. That merge landed, and it is
    the only sentence in the document instructing a writer **not** to do something that is
    now safe — the exact criterion this phase's preamble states for a permitted pointer, so
    it took one. **Task 6.1 already carries the correction**, which is why the task was
    written as two: the writer following the *task list* was never at risk. The writer
    following `design.md` § D2, which is Task 6.1's own stated Input, was. **A count inside a
    task is a claim about the corpus like any other, and this list said to re-derive totals
    rather than inherit them** — §1 says it about its own 39. It did not occur to anyone that
    *"the two places"* in a task body is the same kind of number.
  - The D12 pointer sits below the table **and** its *Confirmed present* list, not between
    them: the stale line numbers §2.2 corrects (`#partition-key` at `:444`, now `:533`) are
    in the list, so a pointer above it would have left them unflagged.

- [x] **Task 1.3:** Re-check [#67](https://github.com/BrighterCommand/Docs/issues/67) and
      tick the README — **DONE 2026-08-24**
  - Input: the issue thread
  - Output: a line in this document recording the state; `README.md` § Status Checklist gains
    *Writing tasks identified*
  - Notes: **Checked 2026-08-23 — open, no reply from the issue author since 2026-08-03.**
    Requirements § Notes makes this a precondition on the ladder's shape, so it is a box, not
    a habit. Re-check once more before Phase 12 comments on the issue.
  - **State 2026-08-24: unchanged, and the ladder's shape stands.** #67 is **OPEN**, two
    comments, both `iancooper`, `updatedAt` **2026-08-03T11:43:25Z** — the thread has not
    moved in three weeks. **PR #72 was checked too**, because that is the other place
    pushback was invited: **MERGED**, one comment (ours), **zero reviews**. So the two
    positions flagged for pushback — Diátaxis as authoring discipline, and prose-vs-generated
    reference — are unchallenged rather than endorsed, and nothing obliges a change of shape.
  - `README.md` § *Next Steps* was **marked spent** in the same commit. Its three items were
    all resolved during requirements and design and it had gone stale directly beneath the
    checklist that contradicts it — including *"the three-tutorial ladder"*, when the ladder
    is four rungs. Marked, not deleted, on §2's own principle: stale text is evidence of when
    it was written.

---

## Phase 2 — The five glossary terms (Docs PR)

**Goal:** D12. Make good on the ladder's central promise — it names concepts and links them
out, which only works if the targets exist.

**This phase must not:** explain the concepts. A glossary entry is two or three sentences and
a link to the page that treats it properly. Nor may it re-open the `BasicConcepts.md` →
`Glossary.md` merge, which the maintainer withdrew 2026-08-04.

- [ ] **Task 2.1:** Add the three Kafka terms
  - Input: design § D12; `KafkaConfiguration.md`; §2.8's verified source citations for the
    offset numbers
  - Output: `partition`, `consumer group`, `offset` in `contents/Glossary.md`
  - Notes: `partition` is **distinct from the existing `Partition Key`** at `:533` and must
    say how they relate. `offset` states that commits are batched at `commitBatchSize`,
    default 10, so a crash redelivers up to a batch — that is rung 4's at-least-once point
    and the entry should not soften it.

- [ ] **Task 2.2:** Add `at-least-once` and `Box Provisioning`
  - Input: design § D12; `BrighterOutboxSupport.md`; `BoxProvisioning.md`
  - Output: both entries in `contents/Glossary.md`
  - Notes: `Box Provisioning` summarises and links `BoxProvisioning.md` — it does not restate
    the three paths. `at-least-once` is linked from both rung 3 and rung 4, so it must read
    correctly for a reader who has met neither Kafka nor the Outbox.

- [ ] **Task 2.3:** Verify the anchors, in both directions
  - Input: `tools/linkcheck.py`'s `slug()`
  - Output: evidence in this document that all five new anchors resolve, and that the twelve
    of §2.2 still do
  - Notes: **Enumerate; do not read.** Rule 3b also applies — the five new headings must not
    collide with anything already on a 678-line page. `pagelint.py` catches that, and
    `linkcheck.py` catches nothing about a heading nobody links yet, so run both.

---

## Phase 3 — Rung 1 (Docs PR)

**Goal:** D1. A command dispatched to a handler in-process, in ten minutes, no Docker — and
the corpus's first `Tutorial`-typed page.

**This phase must not:** open a Brighter PR. D4 reuses `samples/CommandProcessor/HelloWorld`
unchanged — 3 files, 114 lines, verified present today — which is what makes rung 1 the one
rung nothing upstream can stall. If the sample turns out to need a change, that is a finding
to record, not a change to make quietly.

- [ ] **Task 3.1:** Run `HelloWorld` and settle the `host.Run()` question by observation
  - Input: `../Brighter/samples/CommandProcessor/HelloWorld/`
  - Output: a recorded decision — drop `host.Run()` in the page's version and explain why the
    sample keeps it, or tell the reader to press Ctrl+C
  - Notes: design says **decide at writing time by running it; do not guess**. A tutorial
    whose last step leaves the reader at a hung prompt has failed at the last step, which is
    the failure mode this spec exists to prevent.

- [ ] **Task 3.2:** Write `contents/TutorialFirstCommand.md`
  - Input: design § D1 (outline, five code examples, glossary links); the sample's three
    `.cs` files
  - Output: `contents/TutorialFirstCommand.md`, ~180 lines
  - Notes: blocks start at the first `using`, below the ~25-line MIT `#region Licence` —
    AC3's documented exception. The `.csproj` divergence gets its one line. Link
    `#command`, `#handler`, `#command-processor`; all three resolve (§2.2).

- [ ] **Task 3.3:** Land the page's `SUMMARY.md` entry, its `pagetypes.tsv` row, and assert
      the `Tutorial` banner type actually passes
  - Input: §2.1's block; §2.3
  - Output: one `SUMMARY.md` line under `## Get Started`; one appended `pagetypes.tsv` row;
    a green `pagelint.py` naming this page
  - Notes: **the assertion is the point** — this is the first page in 142 to carry
    `> **Tutorial** · …`, so confirm rule 1 and rule 2 accept it rather than inferring it from
    a whole-repo `0 errors`. Run `--changed` with the page **staged**, and read the code-block
    count in its scope line.

- [ ] **Task 3.4:** Clean-machine timed run (AC1, AC2)
  - Input: the page as written
  - Output: a measured duration recorded in this document and reflected on the page
  - Notes: **follow the page's own `dotnet add package` lines, not the sample's project
    references** — that divergence is exactly what this run exists to exercise. Fresh clone,
    empty NuGet cache. Adjust the page to the measurement, never the reverse; the 10-minute
    figure is an estimate until this task replaces it.

- [ ] **Task 3.5:** Write the `## Step N:` deviation into `CLAUDE.md`
  - Input: design § Style Notes; `CLAUDE.md` § *File Organization Pattern*
  - Output: a sentence in `CLAUDE.md` recording that tutorial pages use `## Step N: …`
    headings in place of the Key Concepts / Configuration / Best Practices skeleton, and why
  - Notes: **design asks for this explicitly** — *"this deviation should be written into the
    conventions rather than left as an exception someone later 'fixes'"* — and it landed in a
    paragraph rather than a box, which is how it nearly shipped unwritten. It belongs here,
    at the first page that takes the deviation, not at the acceptance pass: an unrecorded
    exception is at risk from every sweep run in between. The headings still satisfy rule 3 —
    they are subject-qualified and unique — so this records a convention, it does not seek an
    exemption.

---

## Phase 4 — The version gate and the release checklist (Docs PR)

**Goal:** D9 and D11 — the two halves of making the pin hold. See §2.9 for why this sits here
rather than at design's step 6.

**This phase must not:** create a second workflow. D9 adds to `.github/workflows/docs.yml`,
which Spec 011 owns and which already carries the slot. And it must not leave the
`if [ -f tools/versioncheck.py ]` guard in place — inheriting it silently un-gates the check,
which is the same shape of defect as the gate itself.

- [ ] **Task 4.1:** Write `tools/versioncheck.py`
  - Input: design § D9 — patterns, NuGet authority, `--release-notes` fallback, exit codes;
    §2.7 for the vacuity contract; `tools/linkcheck.py` for house shape
  - Output: `tools/versioncheck.py`
  - Notes: stdlib only, single file, paths optional, non-zero exit. `TUTORIAL_PAGES` is an
    explicit list, never a glob — a glob would start policing pages that quote old versions
    deliberately. Prints scope before verdict; missing page skipped and announced; existing
    page with zero pins is exit 1; exit 2 is not a pass. When both authorities are available
    and disagree, report it rather than resolving it.

- [ ] **Task 4.2:** Prove it red before trusting it green
  - Input: Task 4.1's tool; `TutorialFirstCommand.md` as a real, non-empty input
  - Output: a red-proof recorded here — a stale pin, a page with no pins, and an unreachable
    authority, each forced separately
  - Notes: **print a baseline first, and assert the mutation landed** — not merely that the
    text changed, but that it produced *the input the branch rejects*. Three of Spec 010's
    rule-7 red-proofs reported SILENT and all three were the probe's fault. Restore from a
    copy taken aside, never `git checkout --`, and assert byte-identity afterwards. When an
    assertion disagrees with the exit code, the assertion is the suspect.

- [ ] **Task 4.3:** Remove the guard and wire the gate in
  - Input: `.github/workflows/docs.yml`, `versions:` job at line 94, guard at 102–106
  - Output: the job runs `python3 tools/versioncheck.py` unconditionally
  - Notes: the job keeps **both** triggers — pull request and the daily `schedule:` — because
    the event this catches happens in another repository. A PR-only trigger leaves a stale
    pin undetected until someone happens to touch the docs.

- [ ] **Task 4.4:** Write `RELEASE_CHECKLIST.md`
  - Input: design § D11 outline
  - Output: `RELEASE_CHECKLIST.md` at the repository root, ~50 lines
  - Notes: the run table carries a **date column**, so *"when was rung 3 last actually run?"*
    has an answer in the repository rather than in someone's memory. Root, not `contents/`:
    it is a maintainer document, deliberately absent from `SUMMARY.md`. **Confirm
    `linkcheck.py`'s orphan check does not object** — it covers `contents/` only. If it does
    object, the file moves; the check is not loosened.

- [ ] **Task 4.5:** Confirm the gate's first CI run is not vacuous
  - Input: the PR's own checks
  - Output: evidence that the run scanned `TutorialFirstCommand.md` and examined ≥1 pin
  - Notes: `gh pr checks` lists a `push` row and a `pull_request` row per commit and they
    legitimately disagree. **Read the whole output**, and re-check with
    `gh run list --json conclusion` *after* merging, not only before.

---

## Phase 5 — Rung 2's sample (Brighter PR)

**Goal:** D5 — `samples/Tutorials/02-FirstMessage`, derived from `RMQTaskQueue`.

**This phase must not:** touch anything outside `samples/`, or trim `RMQTaskQueue` itself.
The extras being dropped — Serilog, `CustomPublicationFinder`, the second event type, the
explicit scheduler — are *why that sample exists*; removing them there would destroy its
teaching purpose, which is the whole argument for a new sample rather than an edit in place.

- [ ] **Task 5.1:** Build `samples/Tutorials/02-FirstMessage`
  - Input: `../Brighter/samples/TaskQueue/RMQTaskQueue/`; `docker-compose-rmq.yaml`
  - Output: a sender, a receiver console, and the shared event type, under
    `samples/Tutorials/02-FirstMessage/`
  - Notes: − Serilog, − `CustomPublicationFinder`, − `FarewellEvent`, − explicit scheduler.
    `ProjectReference` into `../../../src/` like every other sample, and no `Version`
    attributes on third-party references — central package management (requirements § Q1).

- [ ] **Task 5.2:** Register the new projects in `Brighter.slnx`
  - Input: §2.4; the existing `<Folder Name="/samples/…">` structure
  - Output: a `<Project Path="samples/Tutorials/02-FirstMessage/…" />` entry per project,
    under a `/samples/Tutorials/` folder
  - Notes: **this is the step that makes AC4 true.** Without it the sample is in `samples/`
    and not in the build, which is the state `TodoApi` is in today.

- [ ] **Task 5.3:** Open the PR and confirm CI builds the new projects
  - Input: a branch off `origin/master` (§2.6)
  - Output: a merged Brighter PR; the CI log naming the new projects
  - Notes: AC4 is *merged*, not *opened*. If it stalls beyond a couple of weeks, **say so on
    #67** rather than letting the thread go quiet — and do not work around it by inlining the
    sample into the page, which fails AC4 rather than satisfying it. **`master` may be red for
    reasons that are not yours** — it was on 2026-08-24, on one `net10.0` test in
    `Paramore.Brighter.Transforms.Adaptors.Tests` — so read *which* check failed before
    concluding anything about your sample.

---

## Phase 6 — Rung 2 (Docs PR)

**Goal:** D2. A message over RabbitMQ from one process, consumed in another.

**This phase must not:** ship before Phase 5 merges (AC4), and must not tell the reader to
fetch a file from a repository they have never cloned.

- [ ] **Task 6.1:** Write `contents/TutorialFirstMessage.md`
  - Input: design § D2 (outline, six code examples); the merged D5 sample
  - Output: `contents/TutorialFirstMessage.md`, ~260 lines
  - Notes: **the compose file is inlined in the page** (~12 lines, `rabbitmq:management`, the
    two ports), because the reader has their own project and no Brighter checkout; the sample
    keeps using the root file. The **`ServiceActivator` collision** gets its one sentence at
    the `using` block: prose says Dispatcher, the API says `ServiceActivator`, and a beginner
    who meets the mismatch unexplained concludes they are on the wrong page. Link
    `#dispatcher` — unblocked, see §2.2.

- [ ] **Task 6.2:** `SUMMARY.md` entry, `pagetypes.tsv` row, banner with its *Prerequisites*
      segment
  - Input: §2.1's block
  - Output: the entry, the row, and a banner naming rung 1
  - Notes: the *Prerequisites* link is an ordinary internal link and `linkcheck.py` checks it.
    `apply_banners.py` **preserves** a Prerequisites segment as of `5498cd6` — it used to
    strip them — so a later sweep will not eat it.

- [ ] **Task 6.3:** Clean-machine timed run, two terminals (AC1, AC2)
  - Input: the page as written
  - Output: both terminals' output captured **verbatim** into the page; a measured duration
  - Notes: also walk step 6 — the exchange and queue visible at `localhost:15672` — since a
    reader who cannot see them has diverged and needs to know it there.

---

## Phase 7 — Rung 3's sample (Brighter PR)

**Goal:** D6 — `samples/Tutorials/03-DurableOutbox`, one delta from D5.

**This phase must not:** reuse `WebAPI_Dapper`'s shape. Its env-var matrix — four databases ×
two transports — plus migration assemblies, telemetry and a README step that hand-edits an
absolute database path is four forks and a manual step before the first message moves.

- [ ] **Task 7.1:** Build `samples/Tutorials/03-DurableOutbox`
  - Input: D5; `AddGreetingHandlerAsync.cs` for the transaction shape (design § item 5,
    verified still present §2.8); `OutboxFactory.cs:75` for registration;
    `docker-compose-postgres.yaml`
  - Output: the sample — Postgres outbox, `UseBoxProvisioning(opts =>
    opts.AddPostgreSqlOutbox(cfg))`, a `Greeting` domain table, the transactional handler,
    the Sweeper hosted in-process, and a deliberate failure switch
  - Notes: **omit `ClearOutboxAsync`** — the delay while the Sweeper picks the row up is the
    feature being taught. The `Greeting` table is created by the sample's own startup code;
    `UseBoxProvisioning` owns the Outbox table and nothing else, and the two must not blur.

- [ ] **Task 7.2:** Register the new projects in `Brighter.slnx`
  - Input: §2.4
  - Output: the entries, under `/samples/Tutorials/`
  - Notes: as Task 5.2. The check is presence in the solution, not in the directory.

- [ ] **Task 7.3:** Open the PR and confirm CI builds it
  - Input: a branch off `origin/master`
  - Output: a merged Brighter PR; CI green, naming the new projects
  - Notes: a reviewer should be able to diff 03 against 02 and see exactly what a durable
    Outbox costs. If they cannot, the delta is wrong, not the diff.

---

## Phase 8 — Rung 3 (Docs PR)

**Goal:** D3. The transactional guarantee — and, more importantly, seeing it hold when
things fail.

**This phase must not:** treat step 7 as an appendix. Steps 1–6 could be mistaken for
configuration trivia; **step 7 is the page**, because the reader queries two tables and finds
nothing in either. Gates: Phase 2 (two of its glossary anchors), Phase 6 (the Prerequisites
link), Phase 7 (AC4).

- [ ] **Task 8.1:** Write `contents/TutorialDurableOutbox.md`
  - Input: design § D3 (outline, eight code examples); the merged D6 sample
  - Output: `contents/TutorialDurableOutbox.md`, ~300 lines
  - Notes: state plainly that the database user needs `CREATE TABLE` and `ALTER TABLE` rights
    — true for a Docker Postgres container, not true in many real deployments. Box
    Provisioning is Option A of two; the tutorial takes it without discussion and surfaces
    the choice in *Further Reading* via
    `BoxProvisioning.md#when-to-use-box-provisioning`, whose anchor resolves (§2.2). Link
    `#outbox`, `#sweeper`, `#at-least-once`, `#box-provisioning`.

- [ ] **Task 8.2:** `SUMMARY.md` entry, `pagetypes.tsv` row, banner naming rung 2
  - Input: §2.1's block
  - Output: the entry, the row, the banner
  - Notes: as Task 6.2.

- [ ] **Task 8.3:** Clean-machine timed run — **both** paths (AC1, AC2)
  - Input: the page as written
  - Output: verbatim output for the happy path *and* the failure path; a measured duration
  - Notes: the failure path is run, not reasoned about. Query both tables after the throw and
    capture what comes back — a reader who is told "you will find neither" and finds one has
    been misled by the page that was supposed to prove the point.

---

## Phase 9 — The landing page (Docs PR) · P1

**Goal:** D10 — the front door: what the ladder is, what you need, where to start.

**This phase must not:** list a rung that has not shipped. The page states the ladder that
exists; if Kafka slips, it lists three rungs and the ladder still stands, which is why the
numbering lives in the display text and not in the file names.

- [ ] **Task 9.1:** Write `contents/GetStarted.md`
  - Input: design § D10 outline
  - Output: `contents/GetStarted.md`, ~90 lines
  - Notes: the four sections are *The Ladder* (rung, what you add, time, needs Docker), *What
    You Need Installed* (.NET 9 SDK; Docker for rungs 2–4; ports 5672/15672, 5432, 9092),
    *Just Want to See the Code?*, *Where to Go After the Ladder*. The **two-front-doors
    sentence** lives in the third: the ladder is for building something,
    `ShowMeTheCode.md` is the two-minute look at what Brighter code reads like. Times come
    from the measured runs of Tasks 3.4, 6.3 and 8.3 — not from design's estimates.

- [ ] **Task 9.2:** Execute §2.1's `SUMMARY.md` block and append the row
  - Input: §2.1
  - Output: `## Get Started` holding the ladder above the three orientation pages; one
    appended `pagetypes.tsv` row
  - Notes: re-ordering inside a section moves no URL (§2.1), so `--check-redirects` should be
    unchanged at 77 entries — assert that rather than assuming it. S2 goes 3 → 8 of 12.

---

## Phase 10 — Rung 4's sample (Brighter PR) · P1

**Goal:** D7 — `samples/Tutorials/04-Kafka`, one delta from D5.

**This phase must not:** ship an async handler and mapper. Q4 runs the pump as a **Reactor**,
which obliges **synchronous** equivalents; `KafkaTaskQueue` supplies only the `…Async` pair,
and that widened delta is the argument for a separate sample.

- [ ] **Task 10.1:** Build `samples/Tutorials/04-Kafka`
  - Input: D5; `../Brighter/samples/TaskQueue/KafkaTaskQueue/`; `docker-compose-kafka.yaml`
  - Output: the sample — transport → Kafka, 3-partition topic, `PartitionKey` set on the
    producer, `groupId` on the subscription, `MessagePumpType.Reactor`, sync handler and
    mapper, − the Polly `PolicyRegistry`
  - Notes: `KafkaSubscription` already defaults to `MessagePumpType.Reactor`
    (`:298`, re-verified §2.8), so the default is the teaching point rather than an override
    to explain.

- [ ] **Task 10.2:** Register the new projects in `Brighter.slnx`
  - Input: §2.4
  - Output: the entries, under `/samples/Tutorials/`
  - Notes: as Task 5.2.

- [ ] **Task 10.3:** Open the PR and confirm CI builds it
  - Input: a branch off `origin/master`
  - Output: a merged Brighter PR; CI green
  - Notes: as Task 5.3.

---

## Phase 11 — Rung 4, Kafka (Docs PR) · P1

**Goal:** D8. Partitions, a consumer group rebalancing, per-key ordering, offset commits —
and the connection to Brighter's single-threaded pump, stated once and then linked out.

**This phase must not:** say the pump maps one-to-one onto partition assignment. It is **one
pump per performer, hence per group member** — `noOfPerformers` defaults to 1
(`Subscription.cs:201`), `Dispatcher.CreateConsumers` builds one `Consumer` per performer
(`Dispatcher.cs:589`), so one process is one member and Kafka gives it all three partitions,
which its single thread drains sequentially. Nor may it run the `noOfPerformers = 3`
experiment in the body: that is a fork, and it belongs to `ReactorAndProactor.md`.

- [ ] **Task 11.1:** Write `contents/TutorialStreamingWithKafka.md`
  - Input: design § D8 (eight steps); the merged D7 sample; §2.8's verified offset numbers
  - Output: `contents/TutorialStreamingWithKafka.md`, ~320 lines
  - Notes: step 8 is the honest at-least-once statement — offsets batch at `commitBatchSize`
    (default 10) and **are** committed for revoked partitions on rebalance, so step 6 does not
    silently lose position, but a crash redelivers up to a batch. Ordering holds *per key*,
    because the key selects the partition. Link `#partition`, `#consumer-group`, `#offset`,
    `#partition-key`, `#reactor`.

- [ ] **Task 11.2:** `SUMMARY.md` entry, `pagetypes.tsv` row, banner naming **both** rungs
  - Input: §2.1's block; design § Reading order
  - Output: the entry, the row, and a banner whose *Prerequisites* segment names rungs 2
    **and** 3
  - Notes: banner, *Before You Start* and the ladder diagram must all say the same two rungs.
    An earlier draft of design said it three different ways, which a reader reads as a mistake
    in the ladder itself.

- [ ] **Task 11.3:** Clean-machine timed run, including the rebalance (AC1, AC2)
  - Input: the page as written
  - Output: both terminals' rebalance output captured verbatim; a measured duration
  - Notes: the second consumer instance is a second copy of the app, not a second broker
    (Q2). This is the run most likely to overshoot its estimate; adjust the page to the
    measurement.

---

## Phase 12 — Acceptance and close (Docs PR)

**Goal:** walk AC1–AC8 with evidence, and close the spec on what it landed.

**This phase must not:** infer a criterion from a green build. AC3 (blocks match the sample)
and AC7 (prerequisites, pins, expected output, hand-off) have no tool behind them, and *when
a convention has a mechanical half and an editorial half, a green build is evidence about the
mechanical half only*.

- [ ] **Task 12.1:** Walk AC1–AC8, one at a time, with evidence per shipped rung
  - Input: this document's recorded runs and timings; the merged Brighter PRs
  - Output: an *acceptance pass as executed* section here, in 010's format
  - Notes: AC1 and AC2 are the recorded clean-machine runs; AC3 is walked by eye against each
    sample's `.cs` below its licence region, with the `.csproj` exception noted; AC4 is
    **presence in `Brighter.slnx` plus a green CI run**, not presence in `samples/` (§2.4);
    AC8 is walked by following each page using only the links it offers.

- [ ] **Task 12.2:** Enumerate the parity that no tool checks, and run every gate
  - Input: `pagetypes.tsv`; the banners; the five gates plus `versioncheck.py`
  - Output: recorded numbers for all six gates, and a page-by-page parity check in **both
    directions** — every new page has a row, every new row has a page, type matches banner,
    version matches banner
  - Notes: **enumerate; do not read.** `pagelint.py` never reads the TSV, so nothing else
    will catch a missing row. Expect it to report **143 + one per shipped page**. On the
    warning count: today's 790 is the inherited debt, and **a rise is not automatically a
    defect** — standing obligation 6 permits `// ...` for configuration a previous rung showed
    in full, and `// ...` downgrades to a warning while **still being counted**. So the
    question is not *did 790 move* but *is every block above 790 one of the permitted
    back-references*. Enumerate the new warnings by page and check each against that rule; a
    C# block in these pages that simply lacks its `using` directives **is** a defect in this
    spec's own work.

- [ ] **Task 12.3:** Close
  - Input: the acceptance pass
  - Output: `README.md` checklist completed; `PROMPT.md`'s board updated; a comment on #67
    naming what shipped
  - Notes: #67 stays **open** — 012 and 013 still owe it. Say plainly which rungs shipped and
    which did not; a partial ladder honestly described is the valid end state design
    anticipated. Re-check the thread for a reply before commenting (Task 1.3).

---

## Traceability

| Requirement / design item | Phase |
|---|---|
| D12 five glossary terms | 2 |
| D1 rung 1 · D4 `HelloWorld` reused | 3 |
| D9 `versioncheck.py` · D11 `RELEASE_CHECKLIST.md` | 4 |
| D5 sample · D2 rung 2 | 5, 6 |
| D6 sample · D3 rung 3 | 7, 8 |
| D10 `GetStarted.md` | 9 |
| D7 sample · D8 rung 4 | 10, 11 |
| AC1, AC2 clean-machine run and timing | 3.4, 6.3, 8.3, 11.3, 12.1 |
| AC3 blocks match the sample | standing obligation 7; 12.1 |
| AC4 sample PR merged first | 5.3, 7.3, 10.3; **and §2.4** |
| AC5 `linkcheck.py` clean | standing obligation 5; 12.2 |
| AC6 `versioncheck.py` passes | 4.2, 4.5, 12.2 |
| AC7 prerequisites, pins, output, hand-off | standing obligations 1 and 8; 12.1 |
| AC8 completable without unlinked pages | 2; 12.1 |
| Q1 version pinning | standing obligation 7; §2.5 |
| Q4 Reactor for Kafka | 10.1, 11.1 |
| Q5 Box Provisioning | 7.1, 8.1 |
| Style Notes — the `## Step N:` deviation written into the conventions | 3.5 |
| Verification items 1–6 | closed at design; **re-verified §2.8** |
