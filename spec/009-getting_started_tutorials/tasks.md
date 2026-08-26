# Spec 009: Getting Started Tutorials — Tasks

**Created:** 2026-08-23 · **Status:** **REVIEWED AND APPROVED 2026-08-24** — four findings
applied in place; the total moved 38 → 39. No verdict, gate, priority or sequencing decision
moved. See § *Re-derive the total* and Task 3.5.
**Works from:** `design.md` (approved 2026-08-03, `.design-approved`; six review findings
applied) and `requirements.md` (approved 2026-08-03)
**Executes against:** a corpus that **Spec 010 moved after this spec was approved** — see §2.

**Total tasks: 39, across 12 phases. 30 done — Phases 1, 2 and 3 complete 2026-08-24;
Phases 4, 5 and 6 complete 2026-08-25; Phases 7, 8 and 9 complete 2026-08-26.**
Re-derived, not incremented: `grep -c '^- \[x\] \*\*Task'` says 30 and `'^- \[ \] \*\*Task'`
says 9. The phase table's Tasks column still sums to **39** independently.

**Phase 7's three boxes were ticked in Phase 8's PR, and that is the pattern**: a tick is
a Docs commit, and Phase 7 was a Brighter PR that could not carry one. **Phase 10's will be
ticked in Phase 11's**, for the same reason.

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

> **Corrected at Phase 2, 2026-08-24 — read this row as *"the anchor is absent"*, which is
> all it ever measured.** `#box-provisioning` does not resolve and never did. **The term
> does**: `### BoxProvisioning` has been at `Glossary.md:245` since Spec 005, with four
> inbound links to `#boxprovisioning`. **D12 adds four entries, not five**, and every link to
> that term uses the closed-form anchor. A slug lookup cannot tell an absent term from a
> differently-spelled one, and this table ran nothing else. See Task 2.2.
>
> **And the line numbers in the table above are themselves stale now** — Phase 2 inserted
> entries above four of the twelve. That is the second time in two phases this document has
> had to say it: **cite anchors, not line numbers.**

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

- [x] **Task 2.1:** Add the three Kafka terms — **DONE 2026-08-24**
  - Input: design § D12; `KafkaConfiguration.md`; §2.8's verified source citations for the
    offset numbers
  - Output: `partition`, `consumer group`, `offset` in `contents/Glossary.md`
  - Notes: `partition` is **distinct from the existing `Partition Key`** at `:533` and must
    say how they relate. `offset` states that commits are batched at `commitBatchSize`,
    default 10, so a crash redelivers up to a batch — that is rung 4's at-least-once point
    and the entry should not soften it.
  - **As shipped:** `### Partition`, `### Consumer Group`, `### Offset`, all under
    `## Messaging Terms` immediately after `### Event Stream`, which already introduces all
    three in prose. `Partition` says how it relates to the existing `Partition Key` — the key
    chooses, the partition is what it chooses. `Offset` states the batch commit and names the
    default: **verified in the Brighter source**, `commitBatchSize = 10` at
    `KafkaMessageConsumer.cs:122` and `KafkaSubscription.cs:203`, and the XML doc at
    `KafkaMessageConsumer.cs:92` says in its own words that a crash means the group processes
    those records again. **No line numbers reached the page** — a glossary entry that cited
    them would be stale at the next Brighter commit, and §2.6 says that tree is 140 commits
    past the release.

- [x] **Task 2.2:** Add `at-least-once` and `Box Provisioning` — **DONE 2026-08-24, and it
      added one entry, not two**
  - Input: design § D12; `BrighterOutboxSupport.md`; `BoxProvisioning.md`
  - Output: both entries in `contents/Glossary.md`
  - Notes: `Box Provisioning` summarises and links `BoxProvisioning.md` — it does not restate
    the three paths. `at-least-once` is linked from both rung 3 and rung 4, so it must read
    correctly for a reader who has met neither Kafka nor the Outbox.
  - **`Box Provisioning` was not absent, and adding it would have been the defect.**
    `Glossary.md:245` has carried `### BoxProvisioning` since Spec 005 — under a whole
    `## Database Provisioning` section, with a fuller definition than this task would have
    written, and **four inbound links across two pages already point at `#boxprovisioning`**
    (`BoxProvisioning.md:12`, `:196`, `BoxProvisioningUpgrade.md:222`). Spec 005's own
    `tasks.md:103` names that anchor spelling as a decision, not an accident. A second entry
    would have recreated the `Dispatcher`-defined-twice defect **that Spec 011 had to spend a
    task merging, and that §2.2 celebrates being rid of four paragraphs above this one.**
    **Remedy: nothing is added and every link to the term is `Glossary.md#boxprovisioning`.**
    Design's D12 table and its D3 glossary list both carry a dated correction.
  - **How §2.2 got it wrong, which is the part worth keeping.** §2.2 is not careless — it is
    exactly right about what it measured. It resolved five *anchors* through
    `linkcheck.py`'s `slug()` and reported all five absent, **and all five are absent**.
    `#box-provisioning` genuinely does not resolve. What no query asked is whether the
    **term** was absent, and a slug lookup cannot answer that: `Box Provisioning` and
    `BoxProvisioning` are the same term and different slugs, so the one instrument in use
    was constitutionally unable to see the entry sitting there. **This programme already
    knew to ask what a figure counted rather than whether it is right** — the new face is
    that a *lookup* counts too, and an absent anchor and an absent term are different
    findings that look identical in the output. The tell was free and nobody ran it:
    `grep -i box contents/Glossary.md`.
  - `at-least-once` shipped as `### At-Least-Once`, placed after
    `### Message Oriented Middleware (MoM)` because at-least-once is the promise MoM makes —
    `BrighterInboxSupport.md:14` says exactly that. It reaches the Outbox, streams and the
    Inbox in three clauses, so it reads for a reader who has met none of them, and links
    `#outbox`, `#sweeper`, `#offset` and `#inbox` rather than explaining any of them.

- [x] **Task 2.3:** Verify the anchors, in both directions — **DONE 2026-08-24**
  - Input: `tools/linkcheck.py`'s `slug()`
  - Output: evidence in this document that all five new anchors resolve, and that the twelve
    of §2.2 still do
  - Notes: **Enumerate; do not read.** Rule 3b also applies — the five new headings must not
    collide with anything already on a 678-line page. `pagelint.py` catches that, and
    `linkcheck.py` catches nothing about a heading nobody links yet, so run both.
  - **Forward — the four new anchors resolve, and the fifth spelling does not.** `Glossary.md`
    is **722 lines, 114 headings, 114 distinct slugs, no collisions**, so rule 3b is satisfied
    by enumeration as well as by `pagelint.py`'s 0 errors. `#at-least-once` `:333` ·
    `#partition` `:365` · `#consumer-group` `:376` · `#offset` `:386`.
  - **Proved end-to-end, not by the slug function alone.** *"`linkcheck.py` catches nothing
    about a heading nobody links yet"* is the task's own warning, and resolving slugs with the
    same `slug()` the checker imports would have answered *"does my arithmetic agree with
    itself"*. So six probe links were appended to `contents/FAQ.md`, the mutation asserted to
    have landed, and `linkcheck.py` run: **exactly one MISSING ANCHOR, the `#box-provisioning`
    control**, exit 1. The four new anchors and `#boxprovisioning` passed as real link targets.
    `FAQ.md` was restored **from a copy taken beforehand and asserted byte-identical** — never
    `git checkout --`, which restores from `HEAD` and would have discarded the staged
    `Glossary.md` alongside the probe.
  - **Backward — the twelve of §2.2 all still resolve, and four of them moved.**
    `#routing-key` `383 → 427`, `#reactor` `417 → 461`, `#message-pump` `441 → 485`,
    `#partition-key` `533 → 577`, because Phase 2 inserted above them. §2.2 told the reader to
    cite its table rather than design's line numbers; **§2.2's own line numbers are now stale
    in the same way**, one phase later. The anchors are the durable record. **Cite anchors.**
  - Gates after the edit: `linkcheck.py` clean at 144 files; `pagelint.py` **0 errors, 790
    warnings, 143 pages** — the debt is unmoved, as it must be, since a glossary entry carries
    no C# block; `--changed origin/master` reached **1 page and 0 code blocks** and said so,
    which is the honest result for prose and not a pass to lean on; both `urlmap.py` checks 0.

---

## Phase 3 — Rung 1 (Docs PR)

**Goal:** D1. A command dispatched to a handler in-process, in ten minutes, no Docker — and
the corpus's first `Tutorial`-typed page.

**This phase must not:** open a Brighter PR. D4 reuses `samples/CommandProcessor/HelloWorld`
unchanged — 3 files, 114 lines, verified present today — which is what makes rung 1 the one
rung nothing upstream can stall. If the sample turns out to need a change, that is a finding
to record, not a change to make quietly.

- [x] **Task 3.1:** Run `HelloWorld` and settle the `host.Run()` question by observation —
      **DONE 2026-08-24. Decision: the page drops it.**
  - Input: `../Brighter/samples/CommandProcessor/HelloWorld/`
  - Output: a recorded decision — drop `host.Run()` in the page's version and explain why the
    sample keeps it, or tell the reader to press Ctrl+C
  - Notes: design says **decide at writing time by running it; do not guess**. A tutorial
    whose last step leaves the reader at a hung prompt has failed at the last step, which is
    the failure mode this spec exists to prevent.
  - **Observed, not inferred.** `dotnet run` in the sample printed the pipeline lines, then
    `Hello Ian`, then `Application started. Press Ctrl+C to shut down.` and **sat there** —
    still running at 45 seconds, killed by hand. The greeting arrives *before* the host
    starts, so `Send` completes and `host.Run()` contributes nothing but the block. Design's
    worry was right.
  - **Why it contributes nothing, checked in the source rather than assumed:**
    `AddBrighter().AutoFromAssemblies()` registers **no `IHostedService`**. The only two in
    `Paramore.Brighter.Extensions.DependencyInjection` are `BrighterValidationHostedService`
    and `BrighterDiagnosticHostedService`, and both are registered from
    `BrighterPipelineValidationExtensions.cs:96` and `:129` — **opt-in methods `AddBrighter`
    does not call.** So the host is asked to run with nothing to run.
  - **The near-miss, and it is Phase 2's lesson wearing different clothes.** Asking which
    siblings share the shape, `git grep "host.Run()"` over
    `samples/CommandProcessor/**/Program.cs` matched **one file of three** — and the draft
    finding written from that was *"`HelloWorld` is an outlier among its own siblings"*, which
    is **false**. `HelloWorldAsync` and `HelloWorldInternalBus` both end
    `await host.RunAsync()`. One term, two spellings, and the query was well-formed and
    answering a different question — exactly `Box Provisioning` against `BoxProvisioning` one
    phase earlier. **Reading the two files settled in ten seconds what the grep had got
    backwards.**
  - **So the page's sentence is the true one:** all three samples run the host, and
    `HelloWorldInternalBus` genuinely needs to, because it registers
    `ServiceActivatorHostedService` and consumes. `HelloWorld` inherits the shape from a
    sibling that needs it. The page keeps the sample's code to the line and drops that one
    line, saying why — **the second documented page/sample divergence, alongside the
    `.csproj`** (standing obligation 7). **No change was made to Brighter**, per this phase's
    *must not*.

- [x] **Task 3.2:** Write `contents/TutorialFirstCommand.md` — **DONE 2026-08-24**
  - Input: design § D1 (outline, five code examples, glossary links); the sample's three
    `.cs` files
  - Output: `contents/TutorialFirstCommand.md`, ~180 lines
  - Notes: blocks start at the first `using`, below the ~25-line MIT `#region Licence` —
    AC3's documented exception. The `.csproj` divergence gets its one line. Link
    `#command`, `#handler`, `#command-processor`; all three resolve (§2.2).
  - **As shipped: 222 lines** (`len(text.splitlines())`, the house convention), **7 code
    blocks — 3 `csharp`, 1 `xml`, 2 `bash`, 1 `text`.** Design estimated ~180.
    Design's outline is followed step for step; the `xml` block is the extra one, and it earns
    its place as Step 1's **expected result** (standing obligation 1 requires every step to
    state one, and for "add three packages" the result *is* those three `PackageReference`
    lines). The three C# blocks reproduce the sample below its licence region, to the line,
    with the single `host.Run()` deletion of Task 3.1.
  - **All three C# blocks carry real `using` directives and none uses the `// ...` escape**,
    so the page adds **nothing** to the using-directive debt: it stayed at 790 across a page
    count that went 143 → 144. AC1's baseline is unmoved by construction rather than by luck.
  - **The pin was re-derived at writing time, as §2.5 insists, not carried from §2.5.**
    `api.nuget.org/v3-flatcontainer/paramore.brighter/index.json`, highest non-prerelease:
    **10.7.0**, and `paramore.brighter.extensions.dependencyinjection` agrees at 10.7.0. The
    third package, `Microsoft.Extensions.Hosting 9.0.0`, is deliberately **not** a
    `Paramore.Brighter` line, so D9 will leave it alone — design § D9 restricts its scan to
    lines mentioning `Paramore.Brighter`, and this page is the first real test of that.
  - **Design's D1 outline links rung 2 and rung 2 does not exist yet.** Written as outlined,
    the page carried two `/contents/TutorialFirstMessage.md` links and would have failed
    `linkcheck.py` with **MISSING FILE** — standing obligation 3 states the rule for a page's
    *own* `SUMMARY.md` entry, and this is the same rule pointing outward. Both were rewritten
    to name the next rung without linking it. **Every rung has this problem with the rung
    above it**, so Phases 6, 8 and 11 each inherit it: link *down* the ladder freely, never
    *up*. The upward links are Phase 9's to add, when `GetStarted.md` lists what exists.
  - **The page's code was proved to be the code that ran, not merely code that looks like
    it.** After the last edit, the three `csharp` blocks were extracted from the published
    markdown and compared byte for byte against the three `.cs` files in the timed run's
    working directory: **all three identical**. The `xml` block was compared against the
    `<ItemGroup>` the three `dotnet add package` commands actually generated: **identical**.
    This is the check AC2 and AC3 are really asking for. *"Test all code examples"* is
    normally satisfied by having run something similar at some point; here the artefact that
    compiled and the artefact that publishes are provably the same bytes, and the comparison
    is one script that any later phase can re-run.
  - **Four reader-facing claims were checked against the Brighter source rather than
    asserted, and two of them were wrong as first drafted:**
    - *"`AutoFromAssemblies()` scans the assemblies of your application"* — too vague to be
      useful and slightly wrong. It scans `AppDomain.CurrentDomain.GetAssemblies()`, skipping
      dynamic assemblies and anything named `System.*`, `Microsoft.*` or `Paramore.Brighter*`
      (`ServiceCollectionBrighterBuilder.cs:116`). The page now says that, which is also what
      makes the *"`Found 0 pipelines`"* troubleshooting line meaningful.
    - *"`Send` is the entire API surface for dispatch … rung 2 swaps in a broker and this
      line does not change"* — **false, and it was a promise about a page not yet written.**
      Rung 2 uses `Post`. Rewritten to say what is true: your code never names a handler, and
      the method you choose decides how the request travels.
    - *"The pipeline is built per request, not per application"* — **true**, and worth
      keeping because a reader can see it in the output. `Send` constructs a
      `PipelineBuilder<T>` inside a `using` and calls `Build` on every invocation
      (`CommandProcessor.cs:317-321`).
    - *"`Post` sends it over a broker"* — true, and the corpus already says so in those words
      at `DispatchingARequest.md:249`, so the page links there rather than re-explaining.
  - **Four link texts were retitled to match their targets' H1s** (`Building a Pipeline of
    Request Handlers`, `Basic Configuration`, `Dispatching Requests` ×2). `linkcheck.py`
    cannot see this — the targets resolved either way — and a link whose text does not match
    the page it lands on is the sort of thing only reading catches.

- [x] **Task 3.3:** Land the page's `SUMMARY.md` entry, its `pagetypes.tsv` row, and assert
      the `Tutorial` banner type actually passes — **DONE 2026-08-24**
  - Input: §2.1's block; §2.3
  - Output: one `SUMMARY.md` line under `## Get Started`; one appended `pagetypes.tsv` row;
    a green `pagelint.py` naming this page
  - Notes: **the assertion is the point** — this is the first page in 142 to carry
    `> **Tutorial** · …`, so confirm rule 1 and rule 2 accept it rather than inferring it from
    a whole-repo `0 errors`. Run `--changed` with the page **staged**, and read the code-block
    count in its scope line.
  - **Landed as §2.1 settled it**: `* [1. Your First Command](/contents/TutorialFirstCommand.md)`
    is the **first** entry of the existing `## Get Started`, above the three orientation
    pages; no section was created. `pagetypes.tsv` went **143 → 144 rows**, appended not
    re-sorted, `verdict` = `Tutorial` and `applies` = `Brighter V10`. Section width went
    3 → 4 of 12, so S2 is untroubled, and `--check-shape` reports the tree at 143 pages.
  - **The assertion, three ways, because a repo-wide `0 errors` is a silence.**
    First, `BANNER_RE` was called **directly** on the literal banner string and asserted to
    match with `group(1) == 'Tutorial'` — the vocabulary's never-fired alternative, fired.
    Then two red-proofs on the page itself, each asserting the mutation produced *the input
    the branch rejects* before the tool ran: `**Tutorial**` → `**Walkthrough**`, asserted
    `BANNER_RE.match(...) is None`, gave **BANNER MALFORMED, exit 1**; the banner deleted
    outright, asserted the first non-blank line after the H1 was no longer a banner, gave
    **BANNER MISSING, exit 1**. Restored from a copy taken aside — never `git checkout --` —
    and asserted **byte-identical**.
  - **So the page is read, and `Tutorial` is accepted rather than skipped.** *"A rule that
    never fires is invisible to everything but an enumeration"* is why this was worth five
    minutes at the first page rather than a surprise at Phase 12.
  - **An incidental confirmation worth keeping**: the banner-deleted proof also raised
    **DESCRIPTION MISMATCH**, reporting the opening line as *"handler through Brighter's
    Command Processor…"* — rule 7's extractor skipping the line after the H1, which on this
    page is the banner and, with the banner gone, was the first line of a hard-wrapped
    sentence. Exactly the documented behaviour, and it proves **rule 7 reads this page too**;
    the unmutated page raises nothing, so the `description:` front matter equals the rendered
    opening sentence.
  - **Gates, with the page staged.** `linkcheck.py` clean at **145 files**; `pagelint.py`
    **0 errors, 790 warnings, 144 pages**; both `urlmap.py` checks **0**. And
    `--changed origin/master` reported **4 files, 4 hunks, 1 documentation page, 7 code
    blocks strict** — **the first non-vacuous strict run this spec has had.** Phases 1 and 2
    reached 0 code blocks and said so; six of Spec 010's phases were vacuous for three
    different reasons. All 7 blocks passed rule 6 on their own `using` directives.
  - **Predicted URL, from the tool rather than guessed**: `get-started/tutorialfirstcommand`
    (`python3 tools/urlmap.py | grep -i tutorialfirstcommand`). **Probe it once after the
    sync, not in an until-loop** — a wrong path and an unsynced page are the same silence.

- [x] **Task 3.4:** Clean-machine timed run (AC1, AC2) — **DONE 2026-08-24**
  - Input: the page as written
  - Output: a measured duration recorded in this document and reflected on the page
  - Notes: **follow the page's own `dotnet add package` lines, not the sample's project
    references** — that divergence is exactly what this run exists to exercise. Fresh clone,
    empty NuGet cache. Adjust the page to the measurement, never the reverse; the 10-minute
    figure is an estimate until this task replaces it.
  - **Measured, following the page's own commands, against released 10.7.0 packages:**

    | Step | Cold |
    |---|---:|
    | `dotnet new console -n HelloWorld -f net9.0` | 2.8s |
    | three `dotnet add package` | 6.2s |
    | writing the three `.cs` files | 0.1s |
    | `dotnet run` (cold build) | 1.9s |
    | **total machine time** | **10.9s** |

    `0 Error(s)`, exit code **0**, and the process **returns to the prompt** — which is Task
    3.1's decision holding up under the reader's own path rather than the sample's.
  - **`NUGET_PACKAGES` alone does not give you a cold run, and the first measurement was
    wrong because of it.** Redirecting the global packages folder to an empty directory
    produced a plausible **9.0s** total with **4.5s** of restore — and that restore was served
    from the machine's **HTTP cache**, `~/.local/share/NuGet/http-cache`, which is a
    *separate* location `NUGET_PACKAGES` does not touch. The directory really did go from 0
    files to 195 MB, so every cheap check agreed it was cold. Redone with all four locations
    redirected (`NUGET_PACKAGES`, `NUGET_HTTP_CACHE_PATH`, `NUGET_SCRATCH`,
    `NUGET_PLUGINS_CACHE_PATH`), each verified empty by `dotnet nuget locals all --list`
    **before** the run: restore went 4.5s → **6.2s** and the http-cache went 0 → **113 files**,
    which is the proof that bytes crossed the network. **An empty cache is a claim about one
    directory; "cold" is a claim about four.**
  - **What the number means for the page, which is not what design assumed.** 10.9 seconds of
    machine time says the tooling is not the reader's cost — reading and typing is. So the
    page keeps **"about ten minutes"** as the reader figure and, rather than asserting it,
    states the measured 11 seconds beside it and turns it into a **diagnostic**: *if your
    restore takes minutes rather than seconds, the problem is your package feed, not this
    tutorial.* Design's *"time targets are measured, not asserted"* is honoured by publishing
    what was actually measured, and by not passing off a machine timing as a reader timing.

- [x] **Task 3.5:** Write the `## Step N:` deviation into `CLAUDE.md` — **DONE 2026-08-24**
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
  - **Landed as a paragraph inside `CLAUDE.md` § *File Organization Pattern***, immediately
    below the note explaining why headings carry their subject — the nearest place a reader
    meets the skeleton it deviates from. It states what the deviation is, why a sequence
    resists a reference skeleton, that the step headings satisfy heading qualification on
    their own terms, that **no exemption from the ledger is sought**, and it names
    `contents/TutorialFirstCommand.md` as the first page taking it.
  - **The non-obvious half is what the deviation does *not* touch.** Everything else in the
    pattern still applies — H1, banner, introduction, *Further Reading* unqualified from the
    navigation allowlist — so the page is a normal page with steps in the middle, which is
    why `pagelint.py` needed no change and why the ledger acquires no new row. Had this gone
    unwritten, the risk was never that a sweep would break the page: it is that someone would
    read `## Step 4: Wire Up Brighter` as an *unqualified* heading and "fix" it.

---

## Phase 4 — The version gate and the release checklist (Docs PR)

**Goal:** D9 and D11 — the two halves of making the pin hold. See §2.9 for why this sits here
rather than at design's step 6.

**This phase must not:** create a second workflow. D9 adds to `.github/workflows/docs.yml`,
which Spec 011 owns and which already carries the slot. And it must not leave the
`if [ -f tools/versioncheck.py ]` guard in place — inheriting it silently un-gates the check,
which is the same shape of defect as the gate itself.

- [x] **Task 4.1:** Write `tools/versioncheck.py` — **DONE 2026-08-25**
  - Input: design § D9 — patterns, NuGet authority, `--release-notes` fallback, exit codes;
    §2.7 for the vacuity contract; `tools/linkcheck.py` for house shape
  - Output: `tools/versioncheck.py`
  - Notes: stdlib only, single file, paths optional, non-zero exit. `TUTORIAL_PAGES` is an
    explicit list, never a glob — a glob would start policing pages that quote old versions
    deliberately. Prints scope before verdict; missing page skipped and announced; existing
    page with zero pins is exit 1; exit 2 is not a pass. When both authorities are available
    and disagree, report it rather than resolving it.
  - **As shipped: 352 lines, stdlib only**, `TUTORIAL_PAGES` holding all five pages of the
    ladder including `GetStarted.md`. On the tree today: *5 page(s) listed, 1 found, 4 not
    written yet, 4 pin(s) examined* → `0 stale pins of 4 examined across 1 page(s)`. §2.7's
    contract is met on every clause, and the four absent rungs are named individually rather
    than counted.
  - **It resolves per package, not once for `paramore.brighter` — and this is a settlement of
    something design left under-determined, not a reversal of it.** Design § D9 says the
    authority is *"NuGet, queried once for the package the tutorials pin"*, written on
    2026-08-03 when no tutorial page existed. Rung 1 pins **two** packages, so "the package"
    has no referent. Per-package is what design's own stated rationale asks for — *"the pin's
    job is to match what `dotnet add package` gives the reader"*, and that command resolves
    against the id it is given.
  - **Measured rather than assumed, because the two ids are not interchangeable.**
    `Paramore.Brighter` lists **123** versions and
    `Paramore.Brighter.Extensions.DependencyInjection` **111**; twelve 7.x/8.x versions
    (7.1.0 … 8.1.1399) exist only for the first and four 1.x/2.x only for the second. They
    agree at the tip today, at 10.7.0, which is exactly the state in which a single-id query
    looks correct forever. Each id is fetched once however many times it is pinned.
  - **`Microsoft.Extensions.Hosting 9.0.0` is left alone, and that is now demonstrated rather
    than predicted.** Task 3.2 flagged rung 1 as D9's first real test of the
    `Paramore.Brighter`-only restriction. The page carries **six** version-shaped strings
    across lines 47–62; the tool examines **four**, skipping `--version 9.0.0` at line 49 and
    `Version="9.0.0"` at line 60. The match is scoped to a *line*, because both pin forms
    carry the id and the number together — a page-wide match would attribute a version to
    whichever package name happened to sit nearest it.

- [x] **Task 4.2:** Prove it red before trusting it green — **DONE 2026-08-25. 6/6 branches
      fired, and the probe found a defect the exit codes could not.**
  - Input: Task 4.1's tool; `TutorialFirstCommand.md` as a real, non-empty input
  - Output: a red-proof recorded here — a stale pin, a page with no pins, and an unreachable
    authority, each forced separately
  - Notes: **print a baseline first, and assert the mutation landed** — not merely that the
    text changed, but that it produced *the input the branch rejects*. Three of Spec 010's
    rule-7 red-proofs reported SILENT and all three were the probe's fault. Restore from a
    copy taken aside, never `git checkout --`, and assert byte-identity afterwards. When an
    assertion disagrees with the exit code, the assertion is the suspect.
  - **Kept as `spec/009-getting_started_tutorials/redproof_versioncheck.py`**, re-runnable,
    the same precedent as 010's `noloss.py`. Its `REPO` is derived from `__file__` rather
    than hard-coded — `urlmap.py` shipped a `parents[2]` that was right where it was written
    and silently wrong one directory later.

    | # | Branch forced | Precondition asserted before the run | Expect | Got |
    |---|---|---|---:|---:|
    | — | BASELINE, tree unmutated | `pins()` returns 4 pins | 0 | **0** |
    | 1 | stale pin | 2 pins now read 9.0.0 against a live NuGet 10.7.0 | 1 | **1** |
    | 2 | page exists, pins nothing | page still on disk **and** `pins() == []` | 1 | **1** |
    | 3 | authority unreachable, no fallback | `nuget_latest()` returns `None` | 2 | **2** |
    | 4 | unreachable **with** `--release-notes` | NuGet still down, fixture parses 10.7.0 | 0 | **0** |
    | 5 | both authorities, disagreeing | NuGet 10.7.0 vs fixture 10.6.0 | 0 | **0** |

  - **Branch 2 is where a careless probe fails**, and the assertion says so in two parts: the
    branch rejects a page that **exists** and pins **nothing**, so deleting the page would
    have exercised the *skip* path instead and reported a rule broken that is not.
  - **Branch 5 asserts on the output, not only the exit code.** Exit 0 is the correct verdict
    — the pins do match the authority that governs them — but it is only *defensible* if the
    disagreement was reported, so the probe requires the literal `AUTHORITIES DISAGREE` in
    stdout. A silent 0 there would be the tool resolving what design says it must report.
  - **The defect the probe found is not one any exit code could show.** All six branches
    already fired; reading branch 3's *output* showed **four** `could not reach NuGet` lines
    for **two** packages. The memo keyed on `latest`, which an unreachable package never
    enters, so each id was re-queried once per **pin** rather than once per package — on a
    real outage that is N HTTP timeouts at 20s each, not one. Fixed with a separate `asked`
    set; re-run prints two lines for two packages. **This is *read the whole output* arriving
    from a new direction**: the programme has been caught by truncating output and by
    trusting a green check, but not before by a probe that was green on every assertion it
    made and wrong about something it had not thought to assert.
  - **The page survived**: `sha256 de759052…95b92` before, and byte-identical after all five
    mutations, restored from the copy aside each time.

- [x] **Task 4.3:** Remove the guard and wire the gate in — **DONE 2026-08-25**
  - Input: `.github/workflows/docs.yml`, `versions:` job at line 94, guard at 102–106
  - Output: the job runs `python3 tools/versioncheck.py` unconditionally
  - Notes: the job keeps **both** triggers — pull request and the daily `schedule:` — because
    the event this catches happens in another repository. A PR-only trigger leaves a stale
    pin undetected until someone happens to touch the docs.
  - **The guard is gone and the comment that replaces it says why it must stay gone**, so the
    next person to meet a red `versions` job does not reintroduce it. No second workflow was
    created: still one file, still two jobs.
  - **Verified by parsing the file, not by reading it** — malformed YAML disables things
    silently in this repository's experience. `yaml.safe_load` gives jobs `['check',
    'versions']`, triggers `['push', 'pull_request', 'schedule']` with `cron: '17 6 * * *'`
    intact, and `versions`' steps as `checkout@v4`, `setup-python@v5`,
    `'python3 tools/versioncheck.py'` — a bare command, no `|| true`, no shell pipeline to
    swallow exit 2.

- [x] **Task 4.4:** Write `RELEASE_CHECKLIST.md` — **DONE 2026-08-25**
  - Input: design § D11 outline
  - Output: `RELEASE_CHECKLIST.md` at the repository root, ~50 lines
  - Notes: the run table carries a **date column**, so *"when was rung 3 last actually run?"*
    has an answer in the repository rather than in someone's memory. Root, not `contents/`:
    it is a maintainer document, deliberately absent from `SUMMARY.md`. **Confirm
    `linkcheck.py`'s orphan check does not object** — it covers `contents/` only. If it does
    object, the file moves; the check is not loosened.
  - **As shipped: 79 lines**, following design's four-section outline. Longer than the ~50
    estimated, and the excess is one section: the clean-machine definition carries the
    four-cache procedure below, which is the difference between a measurement and a warm run
    reported as cold.
  - **The run table has two columns design's sketch did not**: *Against*, naming the Brighter
    release the run was made against, and the rungs not yet written are listed as rows rather
    than omitted. A missing row and an unverified rung look identical; a row reading *not yet
    written* cannot be mistaken for either.
  - **Rung 1's row is filled from Phase 3's evidence, not re-run**: 10.7.0, 2026-08-25
    (`faeac68`), 11s of machine time. The column is create-restore-build-run, deliberately not
    the ten minutes the page quotes a reader — the page's figure is mostly reading, and this
    one is the part a slow feed or a broken sample moves.
  - **The four-cache lesson now lives in the repository instead of in a session's notes.**
    `NUGET_PACKAGES` moves the global packages folder only; the bytes come from a separate
    HTTP cache the variable does not touch. All four of `NUGET_PACKAGES`,
    `NUGET_HTTP_CACHE_PATH`, `NUGET_SCRATCH`, `NUGET_PLUGINS_CACHE_PATH` are redirected,
    verified empty with `dotnet nuget locals all --list` **before** the run, and the HTTP
    cache is asserted to have **filled** afterwards — an isolated cache that stayed empty and
    a bypassed cache are indistinguishable from the outside.
  - **The orphan check was confirmed by reading its scope, not by inferring it from a green
    run**, and the first attempt at that got it backwards: `'RELEASE_CHECKLIST.md' in
    md_files()` returned `False`, which looks like a finding and is a bug in the assertion —
    `md_files()` yields **absolute** paths. Corrected, both halves hold: the file **is**
    link-checked (146 files, up from 145) and `orphans()` skips it by construction, at
    `if os.path.dirname(rel) != 'contents': continue`. It stays at root; design's contingency
    does not fire. *When an assertion disagrees with the tool, the assertion is the suspect* —
    Task 4.2's own note, earned twice in one phase.

- [x] **Task 4.5:** Confirm the gate's first CI run is not vacuous — **DONE 2026-08-25, PR
      #117. It scanned a real page and examined four real pins.**
  - Input: the PR's own checks
  - Output: evidence that the run scanned `TutorialFirstCommand.md` and examined ≥1 pin
  - Notes: `gh pr checks` lists a `push` row and a `pull_request` row per commit and they
    legitimately disagree. **Read the whole output**, and re-check with
    `gh run list --json conclusion` *after* merging, not only before.
  - **Six checks, all six read, none truncated**: two GitBook, `check` ×2 and `versions` ×2 —
    one of each per event. All pass. The `tail -3` that merged a red build in session 23 is
    the reason the whole list is quoted rather than the last lines of it.
  - **The evidence is the step's own output, taken from the job log** — identical on both the
    `push` and `pull_request` rows, so the two events agree here rather than legitimately
    disagreeing as they do for `pagelint --changed`:

    ```text
    5 page(s) listed, 1 found, 4 not written yet, 4 pin(s) examined.
      skipped (does not exist yet): contents/TutorialFirstMessage.md
      skipped (does not exist yet): contents/TutorialDurableOutbox.md
      skipped (does not exist yet): contents/TutorialStreamingWithKafka.md
      skipped (does not exist yet): contents/GetStarted.md
      NuGet (Paramore.Brighter): 10.7.0
      NuGet (Paramore.Brighter.Extensions.DependencyInjection): 10.7.0

    0 stale pins of 4 examined across 1 page(s).
    ```

    **`1 found` and `4 pin(s) examined` are the two figures that make the pass mean
    something**, and they are why §2.9 moved D9 from design's step 6 to here: landed before
    rung 1, the identical green would have read `0 found, 0 pin(s) examined`. §2.7's rule
    that those two runs must not print the same line is doing its job on its first CI run.
  - **NuGet is reachable from a GitHub Actions runner** — an assumption design made and
    nothing had tested, and the failure mode would have been a daily exit 2. Both queries
    resolved on the runner, so the per-package fetch is not a CI liability.
  - **The step fails the build on a non-zero exit**, not merely reports one: the log shows
    `shell: /usr/bin/bash -e {0}`, and the invocation is bare, so exit 1 and exit 2 both
    reach the runner.

---

## Phase 5 — Rung 2's sample (Brighter PR)

**Goal:** D5 — `samples/Tutorials/02-FirstMessage`, derived from `RMQTaskQueue`.

**This phase must not:** touch anything outside `samples/`, or trim `RMQTaskQueue` itself.
The extras being dropped — Serilog, `CustomPublicationFinder`, the second event type, the
explicit scheduler — are *why that sample exists*; removing them there would destroy its
teaching purpose, which is the whole argument for a new sample rather than an edit in place.

- [x] **Task 5.1:** Build `samples/Tutorials/02-FirstMessage` — **DONE 2026-08-25**
  - Input: `../Brighter/samples/TaskQueue/RMQTaskQueue/`; `docker-compose-rmq.yaml`
  - Output: a sender, a receiver console, and the shared event type, under
    `samples/Tutorials/02-FirstMessage/`
  - Notes: − Serilog, − `CustomPublicationFinder`, − `FarewellEvent`, − explicit scheduler.
    `ProjectReference` into `../../../src/` like every other sample, and no `Version`
    attributes on third-party references — central package management (requirements § Q1).

- [x] **Task 5.2:** Register the new projects in `Brighter.slnx` — **DONE 2026-08-25**
  - Input: §2.4; the existing `<Folder Name="/samples/…">` structure
  - Output: a `<Project Path="samples/Tutorials/02-FirstMessage/…" />` entry per project,
    under a `/samples/Tutorials/` folder
  - Notes: **this is the step that makes AC4 true.** Without it the sample is in `samples/`
    and not in the build, which is the state `TodoApi` is in today.

- [x] **Task 5.3:** Open the PR and confirm CI builds the new projects — **DONE 2026-08-25**
  - Input: a branch off `origin/master` (§2.6)
  - Output: a merged Brighter PR; the CI log naming the new projects
  - Notes: AC4 is *merged*, not *opened*. If it stalls beyond a couple of weeks, **say so on
    #67** rather than letting the thread go quiet — and do not work around it by inlining the
    sample into the page, which fails AC4 rather than satisfying it. **`master` may be red for
    reasons that are not yours** — it was on 2026-08-24, on one `net10.0` test in
    `Paramore.Brighter.Transforms.Adaptors.Tests` — so read *which* check failed before
    concluding anything about your sample.
  - **Merged 2026-08-25 as [Brighter#4275](https://github.com/BrighterCommand/Brighter/pull/4275)**,
    squashed to `0e617177c` on Brighter `master` — matching how that repository's recent content
    PRs land. **AC4 is therefore satisfied on its own terms: merged, not opened.** At the merge
    the PR stood at **24 checks passing, 1 failing, 2 skipped**, and the one failure was read
    rather than counted: `aws-ci`, **190 `TopicLimitExceededException` lines**, an SNS quota in
    the test account that no pull request can fix. **`rabbitmq-async-ci` and `rabbitmq-sync-ci`
    both passed**, which is the transport this rung actually uses.
  - **The squash was verified by content, not assumed.** A squash leaves the four branch commits
    non-ancestors of `master`, so `git log origin/master..<branch>` still lists them and looks
    like an unmerged branch. The check that settles it is
    `git diff origin/master <branch> -- samples/Tutorials Brighter.slnx`, which returned **0
    lines**. Only then were the branch and its worktree removed.
  - **All three projects are named in `Brighter.slnx` on `master`** — `Greetings`,
    `GreetingsSender`, `GreetingsReceiver`, under a `/samples/Tutorials/02-FirstMessage/` folder
    alongside the README.

---

## Phase 6 — Rung 2 (Docs PR)

**Goal:** D2. A message over RabbitMQ from one process, consumed in another.

**This phase must not:** ship before Phase 5 merges (AC4), and must not tell the reader to
fetch a file from a repository they have never cloned.

- [x] **Task 6.1:** Write `contents/TutorialFirstMessage.md` — **DONE 2026-08-25**
  - Input: design § D2 (outline, six code examples); the merged D5 sample
  - Output: `contents/TutorialFirstMessage.md`, ~260 lines
  - Notes: **the compose file is inlined in the page** (~12 lines, `rabbitmq:management`, the
    two ports), because the reader has their own project and no Brighter checkout; the sample
    keeps using the root file. The **`ServiceActivator` collision** gets its one sentence at
    the `using` block: prose says Dispatcher, the API says `ServiceActivator`, and a beginner
    who meets the mismatch unexplained concludes they are on the wrong page. Link
    `#dispatcher` — unblocked, see §2.2.
  - **As shipped: 440 lines, against design's estimated ~260** — measured with
    `wc -l`, after a first note in this very file guessed "293" and was corrected by running it.
    The overshoot is all verbatim material rather than prose: the four code blocks reproduce the
    sample in full (AC3), and two `text` blocks carry both terminals' output (AC2). Neither is
    compressible without breaking the acceptance criterion that requires it. **Rungs 3 and 4
    should expect the same overshoot**, and design's per-page line estimates should be read as
    prose budgets, not page lengths. Compose file inlined at 10 lines as design requires; the
    `ServiceActivator` sentence sits at the receiver's package block, where a reader meets the
    name for the first time — in the `dotnet add package` lines, which is *earlier* than the
    `using` block design anticipated, because the package names carry the old name too.
  - **The four C# blocks are byte-identical to the four sample files** minus the licence region
    (AC3's documented elision), asserted mechanically by extracting the page's ```` ```csharp ````
    blocks and diffing each against the file that ran — not by eye. A first draft had trimmed
    three of the sample's comments into prose, which would have broken the promise
    `samples/Tutorials/02-FirstMessage/README.md` makes explicitly: *"Every code block on the page
    is this code."* The diff is what caught it.

- [x] **Task 6.2:** `SUMMARY.md` entry, `pagetypes.tsv` row, banner with its *Prerequisites*
      segment — **DONE 2026-08-25**
  - Input: §2.1's block
  - Output: the entry, the row, and a banner naming rung 1
  - Notes: the *Prerequisites* link is an ordinary internal link and `linkcheck.py` checks it.
    `apply_banners.py` **preserves** a Prerequisites segment as of `5498cd6` — it used to
    strip them — so a later sweep will not eat it.
  - **As shipped:** the five entries join the existing `## Get Started` section per §2.1, rung 2
    directly under rung 1. `pagetypes.tsv` goes 143 → **144 rows** — *corrected 2026-08-26 at
    Phase 8, which re-derived it from history: 143 data rows at `bbd054a`, 144 at `cc4f9f6`.
    The figure written here was `PROMPT.md`'s, and `PROMPT.md`'s was the count **after** this
    task ran* — appended rather than re-sorted. Banner carries the *Prerequisites* segment naming rung 1, and `linkcheck.py`
    checks that link like any other.

- [x] **Task 6.3:** Clean-machine timed run, two terminals (AC1, AC2) — **DONE 2026-08-25**
  - Input: the page as written
  - Output: both terminals' output captured **verbatim** into the page; a measured duration
  - Notes: also walk step 6 — the exchange and queue visible at `localhost:15672` — since a
    reader who cannot see them has diverged and needs to know it there.
  - **Run as a reader would run it**, against **released 10.7.0 packages** rather than the
    `ProjectReference` the sample uses — which is the point of this task, and the third of the
    three partial guarantees in requirements § Q1. All four NuGet cache locations were
    redirected and **asserted empty by `dotnet nuget locals all --list` before the run**;
    afterwards the HTTP cache held **144 files / 45 MB**, which is the positive evidence that
    bytes crossed the network rather than a directory merely being extracted into.
  - **Measured: 20.4s to create the three projects and add the packages, 2.3s to build both
    apps — 23s of machine work.** Docker image pull is excluded and stated as excluded on the
    page, because it depends on the reader's connection.
  - **Step 6 was walked against the management API, not eyeballed**: exchange
    `paramore.brighter.exchange` is type **`direct`**, queue `greeting.event` is
    `durable: false`, `auto_delete: false`, and the binding carries routing key
    `greeting.event`. The `direct` type is why the page says the routing key must match exactly.
  - **The wire format independently re-confirms the defect behind Brighter#4277**, this time on
    a *released* package rather than tip-of-tree source: the body is
    `{"greeting":…,"correlationId":null,"id":…}` with `mediaType: application/json` — plain JSON
    from `JsonMessageMapper<T>`, **not** a CloudEvents envelope, which is what 13 XML
    doc-comment lines in `src/` still claim.

> **Phase 6's durable results, so nobody re-derives them.**
>
> **`Microsoft.Extensions.Hosting 9.0.0` — rung 1's pin — does not build on rung 2.**
> `Paramore.Brighter.ServiceActivator.Extensions.Hosting 10.7.0` depends on
> `Microsoft.Extensions.Hosting (>= 10.0.10)`, so the receiver fails restore with
> **`error NU1605: Detected package downgrade`**. This was found by *running the page's own
> commands*, not by reading them: the pin was copied forward from rung 1 on the reasonable
> assumption that a host package is a host package. **Rung 2 pins `10.0.10` in both projects**
> — uniform across the two rather than only where it is forced, so a reader is never asked why
> two sibling apps disagree — and the page says why in one sentence. **Rungs 3 and 4 inherit
> this**: they also reference the Dispatcher's hosting package.
>
> **A queue-depth probe run too early reports the wrong number, and the wrong number is 0.**
> Verifying *"stop the receiver, send anyway, the message waits"*, the queue read
> `messages: 0, consumers: 1` immediately after the receiver was killed and **still 0** right
> after the send — which reads as *the message was lost*, the opposite of the truth. RabbitMQ
> had handed it to the not-yet-reaped consumer as unacknowledged; five seconds later it was
> requeued and the depth was **1**. This is the programme's *"wait on the value you want, never
> on the absence of the one you don't"* in a new place, and the tell was that `consumers` still
> said 1 for a process that had already exited. **Re-read a broker's counters after the
> connection is actually gone.**
>
> **All three legs of the ordering warning were re-measured here rather than inherited**, on a
> genuinely fresh broker (`docker compose down -v`): sender-first prints
> `Published greeting.event` and exits **0** with no queue in existence and the message
> discarded; the receiver then starts and creates the queue at **0 messages** having received
> nothing; a second sender run delivers. The page carries **both halves** — that order matters
> the first time, and that it stops mattering afterwards, because `autoDelete: false` keeps the
> queue alive once the receiver has run once.

---

## Phase 7 — Rung 3's sample (Brighter PR)

**Goal:** D6 — `samples/Tutorials/03-DurableOutbox`, one delta from D5.

**This phase must not:** reuse `WebAPI_Dapper`'s shape. Its env-var matrix — four databases ×
two transports — plus migration assemblies, telemetry and a README step that hand-edits an
absolute database path is four forks and a manual step before the first message moves.

- [x] **Task 7.1:** Build `samples/Tutorials/03-DurableOutbox`
  - Input: D5; `AddGreetingHandlerAsync.cs` for the transaction shape (design § item 5,
    verified still present §2.8); `OutboxFactory.cs:75` for registration;
    `docker-compose-postgres.yaml`
  - Output: the sample — Postgres outbox, `UseBoxProvisioning(opts =>
    opts.AddPostgreSqlOutbox(cfg))`, a `Greeting` domain table, the transactional handler,
    the Sweeper hosted in-process, and a deliberate failure switch
  - Notes: **omit `ClearOutboxAsync`** — the delay while the Sweeper picks the row up is the
    feature being taught. The `Greeting` table is created by the sample's own startup code;
    `UseBoxProvisioning` owns the Outbox table and nothing else, and the two must not blur.

- [x] **Task 7.2:** Register the new projects in `Brighter.slnx`
  - Input: §2.4
  - Output: the entries, under `/samples/Tutorials/`
  - Notes: as Task 5.2. The check is presence in the solution, not in the directory.

- [x] **Task 7.3:** Open the PR and confirm CI builds it
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

- [x] **Task 8.1:** Write `contents/TutorialDurableOutbox.md`
  - Input: design § D3 (outline, eight code examples); the merged D6 sample
  - Output: `contents/TutorialDurableOutbox.md`, ~300 lines
  - Notes: state plainly that the database user needs `CREATE TABLE` and `ALTER TABLE` rights
    — true for a Docker Postgres container, not true in many real deployments. Box
    Provisioning is Option A of two; the tutorial takes it without discussion and surfaces
    the choice in *Further Reading* via
    `BoxProvisioning.md#when-to-use-box-provisioning`, whose anchor resolves (§2.2). Link
    `#outbox`, `#sweeper`, `#at-least-once`, **`#boxprovisioning`** — closed form, corrected
    2026-08-24 at Phase 2. The kebab-case spelling this task originally carried resolves to
    nothing; Spec 005 settled the glossary anchor and four links already use it. Note the two
    are genuinely different targets and both are correct here:
    `BoxProvisioning.md#when-to-use-box-provisioning` is a *page section*,
    `Glossary.md#boxprovisioning` is the *term*.
  - **As shipped: 658 lines** (`len(text.splitlines())`, re-derived after the last prose edit
    rather than carried from the draft), against design's ~300. As with rung 2
    the overshoot is verbatim material and none of it is compressible: three C# blocks totalling
    **255 lines** carry the whole of `GreetingsSender`, and AC3 requires them byte-identical.
    Read design's per-page estimates as prose budgets — the prose here is about 300 lines.
  - **Three C# blocks, and all three asserted byte-identical to the merged sample** at
    `10351e970`, minus the licence region: `Program.cs` 139, `AddGreeting.cs` 27,
    `AddGreetingHandlerAsync.cs` 89. Done by extracting the page's ```` ```csharp ```` blocks and
    diffing each against `git show`, not by reading them side by side — Phase 6 established that
    reading does not catch it.
  - **The `Greeting` DDL is a ```` ```sql ```` block, deliberately.** It lives inside a C# raw
    string literal in the sample, so lifting it as SQL de-indents it; a non-C# block is outside
    AC3 by construction, which keeps the byte-identity claim above unqualified.
  - **Step 3 was written before step 4 so the two-owners point lands with the DDL**, not after
    the whole of `Program.cs` has gone past. The alternative — showing the
    `CreateGreetingTableAsync` span in step 3 and again inside the full file in step 4 —
    duplicates twenty lines to say the same thing.
  - **Links as the task specifies:** `#outbox`, `#sweeper`, `#at-least-once`, `#boxprovisioning`
    (closed form), plus `#command` and `#handler`; and out to
    `BoxProvisioning.md#when-to-use-box-provisioning`, `PostgresOutbox.md`,
    `BrighterOutboxSupport.md`, `OutboxPattern.md`,
    `TransactionalMessagingWithTheOutbox.md`. **`PostgresOutbox.md` is the addition design's
    cross-link list does not name and should** — it is the page that answers the
    `IAmARelationalDatabaseConfiguration` registration, which Phase 7 spent an evening
    rediscovering. **`OutboxSweeper.md` does not exist**; the Sweeper's own page is
    `BrighterOutboxSupport.md`, and a first draft linked the imaginary one.

- [x] **Task 8.2:** `SUMMARY.md` entry, `pagetypes.tsv` row, banner naming rung 2
  - Input: §2.1's block
  - Output: the entry, the row, the banner
  - Notes: as Task 6.2.
  - **As shipped:** `* [3. Adding a Durable Outbox](/contents/TutorialDurableOutbox.md)` joins
    `## Get Started` directly under rung 2, per §2.1. `pagetypes.tsv` goes 144 → **145 rows**,
    appended rather than re-sorted. The banner's *Prerequisites* segment names rung 2 and
    `linkcheck.py` checks it like any other link.
  - **Four of the six gates moved, and by the amounts §2.1 predicts**: linkcheck 147 → **148**,
    pagelint 145 → **146**, `--check-shape` 144 → **145**, `versioncheck.py` **10 pins across 2
    pages → 15 across 3**. `--check-redirects` is **unchanged at 77 entries**, which is §2.1's
    "re-ordering inside a section moves no URL" asserted rather than assumed. **The 790
    using-directive warnings did not move** — all three C# blocks carry their `using` lines.

- [x] **Task 8.3:** Clean-machine timed run — **both** paths (AC1, AC2)
  - Input: the page as written
  - Output: verbatim output for the happy path *and* the failure path; a measured duration
  - Notes: the failure path is run, not reasoned about. Query both tables after the throw and
    capture what comes back — a reader who is told "you will find neither" and finds one has
    been misled by the page that was supposed to prove the point.
  - **Run from the page's own code, not the sample's.** The three `.cs` files were extracted
    from the *page* and written into the project, so a transcription error would have failed
    the build rather than shipping. The compose file was extracted from the page the same way.
  - **The starting point is a reader's, not a clean machine's.** All four NuGet cache locations
    were redirected and asserted empty, then rung 2's solution was built — which is the state a
    reader of this page is in. The http-cache went 0 → 127 files building rung 2 and **127 →
    141** adding rung 3's six packages, which is the positive evidence that the new packages
    came over the network.
  - **Measured: 9.2s for the six `dotnet add package` lines, 1.3s to build.** No `NU1605`:
    `Microsoft.Extensions.Hosting 10.0.10`, inherited from rung 2, satisfies everything rung 3
    adds. **`Npgsql` is pinned at 10.0.2**, which is what Brighter's own central package
    management resolves for `net9.0` — 10.0.3 exists but is the `net10.0` row.
  - **Preconditions asserted before either path ran**: `\dt` returned *Did not find any tables*
    and `rabbitmq-diagnostics check_running` reported fully booted. That is what makes the
    results below mean anything.
  - **Happy path, caught mid-flight.** `Greeting` one row, `Outbox` one row with **`dispatched`
    NULL** — the page's best single screenshot — then dispatched and received. Sweep delay read
    off the row's own columns: **10.035179s**, a third observation agreeing with Phase 7's
    10.06s. Still *"about ten seconds"*; three observations are not a distribution.
  - **Failure path.** `--fail` throws after both writes and before the commit: **neither table
    gained a row**, `This greeting will not survive` is in neither, and the receiver stayed
    silent — asserted by counting `^Received:` in its log, still exactly 1.
  - **`Greeting.Id` skips a number, confirmed here rather than inherited.** A third run after
    the failure got id **3**, not 2. Postgres allocates from the sequence before the rollback.
  - **A finding the sample's README does not carry: `Save request` is logged on the failing
    run too.** `DepositPostAsync` really did write the Outbox row, inside a transaction that
    then rolled back, so the log says the message was saved and the table says it was not. The
    page says so, because a reader comparing the two listings will notice and conclude the
    page is wrong.

---

## Phase 9 — The landing page (Docs PR) · P1

**Goal:** D10 — the front door: what the ladder is, what you need, where to start.

**This phase must not:** list a rung that has not shipped. The page states the ladder that
exists; if Kafka slips, it lists three rungs and the ladder still stands, which is why the
numbering lives in the display text and not in the file names.

- [x] **Task 9.1:** Write `contents/GetStarted.md` — **DONE 2026-08-26**
  - Input: design § D10 outline
  - Output: `contents/GetStarted.md`, ~90 lines
  - Notes: the four sections are *The Ladder* (rung, what you add, time, needs Docker), *What
    You Need Installed* (.NET 9 SDK; Docker for rungs 2–4; ports 5672/15672, 5432, 9092),
    *Just Want to See the Code?*, *Where to Go After the Ladder*. The **two-front-doors
    sentence** lives in the third: the ladder is for building something,
    `ShowMeTheCode.md` is the two-minute look at what Brighter code reads like. Times come
    from the measured runs of Tasks 3.4, 6.3 and 8.3 — not from design's estimates.
  - **As shipped: 95 lines, four sections plus *Further Reading*, three rungs.** The first
    estimate in this spec that did not overshoot — rungs 2 and 3 overshot because AC3 obliges
    them to carry a sample verbatim, and this page carries no sample.
  - **The times come from the three pages, which is where the measurements landed**, not from
    this document: 10 / 20 / 25 minutes reader-facing, and **11s, 23s, 9.2s + 1.3s** of machine
    work, each quoted from the rung's own *Before You Start*. So the landing page and the rung
    cannot disagree without one of them being edited.
  - **Port 9092 is not listed, and that is this phase's must-not being honoured rather than an
    omission.** The task note above was written when four rungs were assumed; 9092 is Kafka's
    port and Kafka has not shipped. *What You Need Installed* names **5672/15672** (rung 2) and
    **5432** (rung 3), and Docker for **rungs 2 and 3**, not 2–4.
  - **The page pins a version, and it has to.** `versioncheck.py`'s `TUTORIAL_PAGES` lists
    `contents/GetStarted.md` (Phase 4 put it there), and **a listed page that exists and pins
    nothing is exit 1** — so a landing page with no `dotnet add package` line would have gone
    red the moment it was created. Found by reading the tool before writing the page rather
    than by the gate afterwards. It carries one line,
    `dotnet add package Paramore.Brighter --version 10.7.0`, presented as the shape every
    `Paramore.Brighter*` line on the ladder takes — which is true and is worth a reader
    knowing. **The pin was re-derived at writing time**, not carried: NuGet's highest
    non-prerelease for `paramore.brighter` is **10.7.0** (123 versions), and
    `…extensions.dependencyinjection` agrees.
  - **Design's `## Just Want to See the Code?` shipped as `## Just Want to See Brighter
    Code?`** — the outline's heading is unique but unattributable in a retrieval chunk, which
    is exactly what heading qualification exists to prevent. The other three are
    `## The Brighter Tutorial Ladder`, `## What You Need Installed for the Ladder` and
    `## Where to Go After the Ladder`.
  - **The upward links Phase 3 deferred to this phase are in.** Rung 1's closing paragraph now
    links *the next rung* to `TutorialFirstMessage.md`, and rung 2's to
    `TutorialDurableOutbox.md`. **Rung 3's closing paragraph still names Kafka without linking
    it**, because rung 4 does not exist — the same rule pointing the same way, and Phase 11
    inherits that one link.
  - **The page type is `Tutorial`, per approved design, and the confidence in `pagetypes.tsv`
    is `medium` rather than `high`.** It is a front door rather than something you build, and
    `Reference` is arguable; it is typed with the ladder it opens. Recorded here so the
    judgement is visible rather than implied by a green build.

- [x] **Task 9.2:** Execute §2.1's `SUMMARY.md` block and append the row — **DONE 2026-08-26**
  - Input: §2.1
  - Output: `## Get Started` holding the ladder above the three orientation pages; one
    appended `pagetypes.tsv` row
  - Notes: re-ordering inside a section moves no URL (§2.1), so `--check-redirects` should be
    unchanged at 77 entries — assert that rather than assuming it. S2 goes 3 → 8 of 12.
  - **One entry was added, not five.** §2.1's block is the finished section; rungs 1, 2 and 3
    joined it in Phases 3, 6 and 8, so this task contributed
    `* [Get Started with Brighter](/contents/GetStarted.md)` at the top and nothing else. Rung
    4's line is Task 11.2's.
  - **S2 goes 6 → 7 of 12, not 3 → 8**, and both figures are right about different moments:
    §2.1 measured the section before Phase 3 and counted the finished ladder including Kafka.
    The widest section is still **10 of 12** and unchanged. *(This is the programme's "a total
    needs a ref" arriving in the smallest possible way — §2.1's 3 is stamped 2026-08-23.)*
  - **`--check-redirects` is unchanged at 77 entries and 7858 bytes**, asserted rather than
    assumed, which is §2.1's "re-ordering inside a section moves no URL" holding for a fourth
    time.
  - **`pagetypes.tsv` goes 145 → 146 rows, appended rather than re-sorted.** `PROMPT.md` says
    144; it is stamped before rung 3 shipped and is right about that moment. Re-derived here
    with `awk 'NR>1' | wc -l` rather than incremented.
  - **Four gates moved and two did not.** linkcheck 148 → **149 files**, pagelint 146 → **147
    pages**, `--check-shape` 145 → **146 pages**, `versioncheck.py` **15 pins across 3 pages →
    16 across 4**. Unmoved: `--check-redirects` at 77, and the **790 using-directive warnings**
    — the page's one fenced block is `bash`, so it adds no debt. `--changed origin/master`
    reports **5 files, 5 hunks, 3 pages, 1 code block strict**: non-vacuous, and exit 0.
  - **Predicted publication path, with the tool rather than by guessing:**
    `get-started/getstarted`.

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
