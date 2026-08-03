# Requirements: Spec 011 — Page Type Discipline and Machine-Readable Conventions

**Created:** 2026-08-03
**Status:** Approved 2026-08-03 (reviewed; five findings addressed in this revision)
**Responds to:** [Docs#67](https://github.com/BrighterCommand/Docs/issues/67)

## Topic Overview

[Docs#67](https://github.com/BrighterCommand/Docs/issues/67) observes that *"Brighter
documentation mixes different types of information."* Spec 010 fixes that **between**
pages. This spec fixes it **within** pages — and the same change, viewed from another
angle, makes each page well-formed for machine consumption.

Two audiences, one fix. A page that announces its type and version helps a reader
decide whether they are in the right place, and helps a retrieval system attribute a
chunk it has pulled out of context. A heading that names its subject improves search
results for a human and makes a chunk identifiable to a model. Neither convention is
worth much unless something enforces it, so the linter is part of the deliverable
rather than a follow-up.

## Current State

### Measurements

Measured 2026-08-03 by script over `contents/`. **These figures supersede the earlier
audit** recorded in the programme notes; where they differ, the method is stated so
the difference is checkable rather than mysterious.

| Measure | Value | Note |
|---|---|---|
| Pages under `contents/` | **107** | Earlier notes said 110. `linkcheck.py` reports 109 files = 107 pages + `SUMMARY.md` + `README.md`. No subdirectories. |
| Pages with no H1 | **0** | Every page has one. |
| Pages already opening with a blockquote after the H1 | **0** | Nothing to collide with. |
| C# code blocks (```` ```csharp ````) | **796** | |
| …of those, containing a `using` directive | **133 (16%)** | Earlier notes said ~230 of ~1,050. That count appears to have included all fenced blocks, not just C#-tagged ones. Either way the direction is the same and the compliance is worse than assumed. |
| Fenced blocks with **no language tag** | **185** | Not previously measured. `CLAUDE.md` requires a language on every block. |
| Distinct `##` heading texts | **474** | |
| Heading texts appearing on more than one page | **53** | |
| Total `##` heading instances that are non-unique | **297** | 41 are on the navigation allowlist (`Further Reading` 29, `Related Documentation` 10, `See Also` 2) and stay uniform. **256 instances across 50 texts need qualification.** |
| Internal links carrying an `#anchor` | **299** | |
| …targeting a generic anchor that de-duplication would rename | **8** | 7 × `#provisioning`, 1 × `#configuration`. See risk note below. |

Worst heading collisions: `## Further Reading` 29, `## Best Practices` 26,
`## Configuration` 22, `## Troubleshooting` 14, `## Summary` 14, `## Usage` 13,
`## Related Documentation` 10, `## Overview` 10.

### Mode mixing, measured

Pages were scored by matching H2/H3 heading text against vocabulary for each Diátaxis
mode, excluding navigation sections (`Further Reading`, `Related Documentation`,
`See Also`, `Next Steps`, `References`) which appear almost everywhere and are not a
content mode.

| Cohort | Count |
|---|---|
| Headings hit **all four** modes | **14** |
| Headings hit **three or more** modes | **31** |
| >500 lines **and** ≥3 modes | **21** |
| >800 lines (any mode count) | **9** |

The 14 four-mode pages: `HangfireScheduler` (830), `AwsScheduler` (773),
`AzureScheduler` (715), `NullableReferenceTypes` (709), `PostgreSQLMessageBroker`
(660), `DynamicMessageDeserialization` (595), `Telemetry` (595),
`BrighterSchedulerSupport` (576), `RabbitMQConfiguration` (566), `InMemoryScheduler`
(539), `SweeperCircuitBreaking` (525), `DefaultMessageMappers` (476),
`CloudEventsSupport` (473), `ReactorAndProactor` (440).

**This is a triage signal, not a verdict.** Heading vocabulary is a proxy; a page can
score four modes and still read coherently, and a page can score two and still be a
mess. Every candidate gets human judgement before it is split. The number's job is to
stop us splitting by gut feel and to make the worklist reviewable.

Two things the measurement does establish firmly:

- **Size alone is the wrong criterion.** `Glossary.md` is 589 lines and single-mode
  reference; `KafkaConfiguration.md` is 606 lines and scores one mode. Splitting on
  line count would damage both. `ReactorAndProactor.md` is 440 lines and scores four.
- **`RabbitMQConfiguration.md` and `BrighterBasicConfiguration.md` remain the right
  demonstrators**, being the two the issue and our own reading independently flagged.

## Target State

**For a reader:** every page states, in rendered text below its title, what kind of
page it is, which version it applies to, and what you should have read first. Section
headings name their subject, so a search result reading "Kafka Subscription
Configuration" is useful where "Configuration" was not.

**For a retrieval system:** every chunk carries enough of that context to be
attributable and version-correct. This matters more than it first appears: pre-V10
Brighter is well represented in blog posts and Stack Overflow answers, so a model with
no counter-evidence in context will confidently emit configuration that no longer
compiles. A version marker in body text is the counter-evidence.

**For an author:** the conventions are written down in `CLAUDE.md` and checked by
`tools/pagelint.py` in CI, so the next 100 pages comply without anyone remembering to
ask.

## Target Audience

This spec's output is consumed by three parties, which is unusual and shapes the work:

- **Documentation readers** — get the banner and the qualified headings. They never
  read the conventions document.
- **Documentation authors (human and LLM)** — the `CLAUDE.md` conventions section and
  the linter's error messages are written *for them*. Error messages must say what to
  do, not just what is wrong.
- **Retrieval systems** — consume the banner text and `llms.txt`. They are not a
  reason to make the page worse for humans, and every convention here was chosen
  because it serves both.

## Source Material

- [Diátaxis](https://diataxis.fr/) — the four modes and why mixing them fails
- [llms.txt specification](https://llmstxt.org/) — the index convention defined here,
  generated by Spec 010
- `tools/linkcheck.py` (225 lines) — the pattern to follow and the helpers to reuse:
  `md_files()`, `slug()`, `HEADING_RE`, and its exit-code contract
- `CLAUDE.md` — existing standards, several of which are measurably unenforced
- [GitbookIO/gitbook#1079](https://github.com/GitbookIO/gitbook/issues/1079) — the
  front-matter rendering defect behind the banner decision

## Resolved Questions

**Q1 — the banner goes below the H1.** *(decided 2026-08-03)* Below keeps the H1 as
the first element, which is what GitBook extracts as the page title. The measurement
makes this safe and unambiguous: **0 of 107 pages** currently begin with a blockquote
after their H1, so "first non-blank line after the H1 must match the banner pattern"
is a rule the linter can enforce with no legacy exceptions and no ambiguity about
whether a banner is present.

The README asked for confirmation against a rendered preview. That is worth doing once
the first page ships, but it is not a blocker: a blockquote is ordinary Markdown that
GitBook renders as a callout, and nothing about it is version- or platform-specific.

**Q2 — front matter is ruled out.** *(resolved 2026-08-03, carried from the README)*
GitBook's own documentation points at `.gitbook.yaml` rather than front matter for Git
Sync, and issue #1079 reports front matter rendering literally into the page body.
Shipping it across 107 pages risked a block of raw YAML atop every published page. The
visible banner is better for both audiences anyway — it renders, and it survives the
front-matter stripping retrieval chunkers apply.

**Q3 — 011 proves the split, 010 executes it.** *(decided 2026-08-03)* Splitting a
page is an information-architecture act: it creates new pages that need names,
`SUMMARY.md` placement and redirect entries — all of which Spec 010 owns and is
already doing to those same files.

So this spec ships **two demonstrator splits** (`RabbitMQConfiguration.md` and
`BrighterBasicConfiguration.md`) that establish the pattern, plus the **rule and the
scored worklist** that 010 executes against. 010 splits the remaining candidates while
it re-files them, touching each page once.

This *modifies* the README's scope, which assigned splitting to 011. The reason is
that the README's own justification for ordering 011 first — "so pages are touched
once, not twice" — argues for this: splitting inside 011 and re-filing inside 010
touches the split pages twice and produces two overlapping diffs, exactly the outcome
the ordering was chosen to avoid.

**Q4 — the banner sweep does not trigger the `using`-directive rule.** *(decided
2026-08-03)* Adding one mechanical line to 107 pages is not "touching" a page for the
purposes of the code-completeness rule. Otherwise the sweep drags 796 hand-verified
code-block edits behind it and stops being reviewable.

The linter therefore has **two strictness levels**:

- **Repo-wide run** — banner, heading uniqueness, terminology and language-tag rules
  are errors. Missing `using` directives are **warnings**, reported with a count so
  the debt stays visible.
- **Changed-files run** (`pagelint.py --changed`, from the git merge-base) — missing
  `using` directives are **errors**. A page edited for content complies fully.

This is what makes "define the rule and enforce it going forward" a mechanism rather
than an intention, and it means the debt shrinks monotonically without a big-bang
backfill.

**Q5 — the linter is a sibling tool, not an extension of `linkcheck.py`.** *(decided
2026-08-03)* `tools/pagelint.py` imports `md_files()`, `slug()` and `HEADING_RE` from
`linkcheck.py` rather than duplicating them, but keeps its own rules and output. Link
integrity and authoring conventions fail for different reasons, get fixed by different
people at different times, and a reader of either tool should not have to read the
other.

## The Conventions

### 1. Page banner

Immediately below the H1, one blank line, then a single-line blockquote:

```markdown
# RabbitMQ Subscription Configuration

> **Reference** · Applies to **Brighter V10** · Prerequisites: [Basic Configuration](/contents/BrighterBasicConfiguration.md)
```

- **Page type** — exactly one of `Tutorial`, `How-to`, `Reference`, `Explanation`.
  A fixed vocabulary, because the linter checks membership and because a page that
  cannot pick one is usually a page that needs splitting. That failure to choose is a
  useful signal, and the worklist should record it.

  **`Reference` is also the catch-all.** *(decided 2026-08-03)* Some pages are
  legitimately outside Diátaxis rather than confused about their mode — `FAQ.md` (645
  lines) is the clear case, and `Glossary.md` and `V10MigrationGuide.md` are close
  behind. These are material you *consult* rather than read through, which is what
  Reference means at its widest. They are **not** split candidates, and the worklist
  must not record them as such.

  A fifth vocabulary value was considered and rejected: a bucket for "none of the
  above" collects everything ambiguous and dissolves the discipline this spec exists
  to create. Stretching `Reference` costs less than that. If a page's type is genuinely
  contested, that argument belongs in review, not in the vocabulary.
- **Applies to** — `Brighter V10`, `Darker V10`, or both. Required.

  This is 107 edits at V11. That is acceptable — the docs site publishes one version
  at a time, so the bump is a single mechanical pass — but it must be *mechanical*:
  `pagelint.py --fix` covers the version segment (P1), so the V11 change is one
  command and a diff review rather than a page-by-page trudge.
- **Prerequisites** — zero or more internal links. Optional; omit the segment
  entirely rather than writing "Prerequisites: none".

### 2. Subject-qualified headings

`## Configuration` becomes `## Kafka Subscription Configuration`. The qualifier comes
from the page's subject, so it can be proposed automatically and reviewed by a human.

#### The navigation allowlist (canonical — defined once, referenced everywhere)

Exactly these five heading texts are exempt from uniqueness and **must stay uniform**:

```
Further Reading
Related Documentation
See Also
Next Steps
References
```

Their repetition is a feature: it makes the end of every page predictable. The linter
allowlists them by exact text, and this list is the single source of truth — the
mode-mixing analysis, the linter and `CLAUDE.md` all use it verbatim.

#### This contradicts `CLAUDE.md` today, and `CLAUDE.md` must change with it

`CLAUDE.md`'s *File Organization Pattern* currently prescribes the standard page
skeleton as: Title → Introduction → Key Concepts → How-to/Usage → **Configuration** →
**Best Practices** → **Common Pitfalls** → Further Reading → **Sample Code**.

Five of those are among the worst collisions we are fixing. As written, an author
following the documented standard produces a page the linter rejects. **D1 must amend
that pattern**, not merely append a conventions section beneath it — otherwise
`CLAUDE.md` contradicts itself and authors will reasonably follow whichever half they
read first.

The amended pattern keeps the same skeleton and the same ordering, and states that
every section heading except the navigation allowlist carries its subject:

| Current | Amended |
|---|---|
| `## Configuration` | `## Kafka Subscription Configuration` |
| `## Best Practices` | `## Kafka Best Practices` |
| `## Common Pitfalls` | `## Kafka Common Pitfalls` |
| `## Sample Code` | `## Kafka Sample Code` |
| `## Further Reading` | `## Further Reading` *(allowlisted, unchanged)* |

The skeleton was never the problem — the unqualified headings were.

### 3. Version markers on code

Where V9 and V10 differ, show both, labelled — ❌ for the superseded form, ✅ for
current. Putting the discrimination in the text is more reliable than trusting either
a reader or a model to already know which era they are looking at.

### 4. Complete code blocks

`using` directives present; `// ...` marking genuine omissions; a language tag on
every fence. Already required by `CLAUDE.md`; measurably not complied with.

### 5. `llms.txt`

Defined here, generated by Spec 010 from `SUMMARY.md`: one line per page, title,
canonical path and one-sentence description. This spec owns the format; 010 owns the
generator.

## Scope

### P0 — must have

- **Conventions section in `CLAUDE.md`**, covering all five conventions above with
  the banner vocabulary written out.
- **Banner added to all 107 pages** under `contents/`. Script-generated, human-reviewed
  for the page-type call.
- **`tools/pagelint.py`** implementing the rules below, wired into CI beside
  `linkcheck.py`.
- **Heading de-duplication**: of 53 colliding heading texts / 297 instances, three are
  on the navigation allowlist (`Further Reading`, `Related Documentation`, `See Also` —
  41 instances) and stay as they are. **50 texts / 256 instances need qualification.**
- **Two demonstrator splits** — `RabbitMQConfiguration.md` and
  `BrighterBasicConfiguration.md`.
- **The scored worklist** handed to Spec 010, with each candidate's mode score, size
  and a human verdict.

### P1 — should have

- **`llms.txt` format definition** (the generator ships with 010).
- **Language tags added** to the 185 untagged fenced blocks. Mechanical enough to sit
  alongside the sweep, and it makes the code-block rules enforceable repo-wide.
- **A `--fix` mode on `pagelint.py`** for the mechanical rules (banner insertion,
  language tags), so the same tool that reports the debt can retire it.

### P2 — nice to have

- Backfilling `using` directives beyond changed pages, in batches by section.
- A CI comment summarising the warning count so the debt trend is visible per PR.

## Out of Scope

- **Rewriting prose for style.** This spec is structural.
- **Splitting beyond the two demonstrators** — see Q3; the rest is Spec 010's.
- **Backfilling all 796 code blocks** — see Q4; the mechanism retires this debt as
  pages are edited.
- **Moving or renaming files.** Spec 010 owns placement. This spec edits pages in
  place, with the single exception of the new pages the two demonstrator splits
  create.

## Documentation Deliverables

| ID | File | Description | Priority |
|---|---|---|---|
| D1 | `CLAUDE.md` | New *Page Conventions* section (banner format and vocabulary, heading qualification rule, navigation allowlist, version markers, code completeness, `llms.txt` format) **and an amendment to the existing *File Organization Pattern*** so the prescribed skeleton no longer mandates the unqualified headings the linter rejects. Both edits in one change — shipping the first without the second leaves `CLAUDE.md` self-contradictory. | P0 |
| D2 | `contents/*.md` (107 files) | Banner inserted below each H1. | P0 |
| D3 | `contents/*.md` (subset) | Colliding headings subject-qualified; the 8 inbound anchor links repointed. | P0 |
| D4 | `tools/pagelint.py` | The linter. Rules and exit-code contract below. | P0 |
| D5 | `.github/workflows/docs.yml` | **The repository's first CI workflow.** Runs `linkcheck.py` and `pagelint.py` on push and pull request; both gate the build. | P0 |
| D6 | `contents/RabbitMQConfiguration.md` + new pages | Demonstrator split. Reference stays; explanation, how-to and guidance move out. | P0 |
| D7 | `contents/BrighterBasicConfiguration.md` + new pages | Demonstrator split. | P0 |
| D8 | `spec/011-authoring_conventions/worklist.md` | Scored split candidates with human verdicts, handed to Spec 010. | P0 |
| D9 | `llms.txt` format definition | In `CLAUDE.md`; generator is 010's. | P1 |

### D5 — there is no CI today

Verified 2026-08-03: **`.github/workflows/` does not exist.** The Docs repository has
no automation whatsoever, and `linkcheck.py` — which `CLAUDE.md` instructs authors to
run after every link change — has only ever run when someone remembered to type it.

That reframes this deliverable. It is not "add a step beside the existing check"; it
is standing up the repository's first workflow, and wiring `linkcheck.py` into it is
part of the job rather than an assumption. It also explains the state the audit found:
every convention in `CLAUDE.md` that is not machine-checked has decayed, and the
16%-`using`-directive figure is what that decay looks like after a couple of years.

Requirements for the workflow:

- Triggers on push and on pull request.
- Runs `python3 tools/linkcheck.py` and `python3 tools/pagelint.py` over the whole
  repo; **either failing fails the build**.
- Pull requests additionally run `pagelint.py --changed` against the merge-base, which
  is where the stricter code-block rules apply.
- No network dependencies and no external actions beyond checkout and Python setup —
  the tools are plain standard-library Python, and it should stay that way so the
  build cannot break for reasons unrelated to the docs.

**Land the workflow before the sweeps**, not after. A green baseline on the current
tree, followed by the banner commit, makes it obvious whether a failure came from the
sweep or was already there.

### D4 — linter rules

| Rule | Repo-wide | Changed files |
|---|---|---|
| Banner present as first non-blank line after H1 | error | error |
| Banner page type in the fixed vocabulary | error | error |
| Banner carries an *Applies to* segment | error | error |
| `##` heading text unique across pages (navigation allowlisted) | error | error |
| Fenced block carries a language tag | error *(after P1 lands; warning until then)* | error |
| `ServiceActivator` in prose where `Dispatcher` is meant | error | error |
| C# block carries `using` directives | **warning, with count** | **error** |

- Exit non-zero on any error; print a summary line with the warning count.
- Accept file paths to check a subset, as `linkcheck.py` does.
- **Error messages state the fix**, not just the violation — authors are the audience.

**`--changed` operates on changed *blocks*, not changed files.** A file-level rule
would mean that fixing a typo on a 700-line page obliges you to backfill `using`
directives into every code block on it. That penalises exactly the small, low-risk
corrections we want people to make, and the predictable outcome is that they stop
making them. The tool derives changed line ranges from the git merge-base and applies
the strict code rules only to blocks overlapping those ranges. Everything else on the
page stays a warning.

The terminology rule needs care: `ServiceActivator` is legitimate inside code blocks
and namespaces (`Paramore.Brighter.ServiceActivator.Extensions.*`,
`ServiceActivatorHostedService`) and in the page name `HowServiceActivatorWorks.md`.
The rule applies to **prose outside code fences and outside inline code**, and needs a
line-level allowlist for pages that discuss the name itself.

## SUMMARY.md Changes

The banner sweep and heading de-duplication add no pages, so `SUMMARY.md` is untouched
by D2 and D3.

The two demonstrator splits **do** add pages, and each new page needs an entry.
Placement is provisional and coordinates with Spec 010, which restructures the whole
table of contents immediately afterwards. Add entries beside the page they were split
from rather than inventing new sections — 010 will site them properly.

`.gitbook.yaml` has no `redirects:` block today. The demonstrator splits are the first
change that needs one, since content moves to new URLs. Adding that block is Spec
010's deliverable; if 010 has not landed when the splits do, **the split must add the
redirect entries itself** rather than shipping broken published URLs. Verify
mechanically — malformed indentation disables GitBook redirects silently rather than
erroring.

## Risks

**Heading renames break anchors — but only 8 of them.** Measured: 299 internal links
carry an anchor; exactly 8 point at a generic anchor that de-duplication would rename
(7 × `#provisioning`, 1 × `#configuration`). `linkcheck.py` reports every one as
MISSING ANCHOR, so the failure mode is a red build, not a broken published link.
External inbound links from blog posts and Stack Overflow answers are the real
exposure and cannot be fixed from here; the count is small enough that this is
acceptable, and it is the last moment it will be this small.

**The banner sweep is a 107-file diff.** Reviewable only if it is genuinely
mechanical. Keep it as its own commit, separate from heading de-duplication, and let
the linter rather than the reviewer verify uniformity.

**Page-type assignment is a judgement call at 107 scale.** The script can propose from
the existing structure, but a wrong page type is worse than none — it tells a reader
the page is something it is not. Every proposed type gets human review, and pages that
resist classification go on the worklist as split candidates rather than being forced
into a bucket.

## Acceptance Criteria

1. `python3 tools/pagelint.py` exits 0 across the repo, with the `using`-directive
   warning count printed and recorded as the baseline to shrink.
2. `python3 tools/linkcheck.py` exits 0, including the orphan check.
3. All 107 pages carry a banner whose page type has been reviewed by a human, not just
   generated.
4. No `##` heading text appears on more than one page, except the navigation allowlist.
5. `CLAUDE.md` documents every convention the linter enforces, and the linter enforces
   every convention `CLAUDE.md` documents. Neither has a rule the other lacks — and
   `CLAUDE.md`'s *File Organization Pattern* no longer prescribes headings the linter
   rejects.
6. CI exists, runs both tools on push and pull request, and was **green on the current
   tree before the sweeps landed**, so any later failure is attributable.
6. The two demonstrator splits are single-mode pages that a reader can navigate, with
   redirects in place for the URLs that moved.
7. The worklist is complete enough for Spec 010 to execute against without re-deriving
   the analysis.

## Notes

- **Conventions without enforcement decay.** The evidence is in this repo: `CLAUDE.md`
  has required complete code blocks for as long as it has existed, and compliance is
  16%. The linter is the deliverable that makes the rest of the deliverables durable.
- The mode-mixing script is a triage tool built for this analysis. If it proves useful
  beyond the initial worklist, folding it into `pagelint.py` as an advisory report is
  a natural P2 — but as an *advisory*, never a gate. Diátaxis is an authoring
  discipline, and a build should not fail because a heading matched a regex.
</content>
