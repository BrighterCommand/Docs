---
allowed-tools: Bash(cat:*), Bash(echo:*), Bash(test:spec/*), Bash(ls:spec/*), Bash(touch:spec/*), Bash(grep:*), Bash(tail:*), Bash(git diff:*), Bash(git log:*), Bash(python3 tools/linkcheck.py:*), Bash(python3 tools/pagelint.py:*), Bash(python3 tools/urlmap.py:*), Bash(python3 tools/versioncheck.py:*), Bash(python3 tools/symbolcheck.py:*), Bash(dotnet run --project tools/optioncheck:*), Read
description: Review current specification phase
---

## Current Spec Status

Current spec: !`cat spec/.current-spec 2>/dev/null || echo "No active spec"`

## Gates

**All eight run here.** `pagelint` is filtered to its errors and its summary line: the
using-directive warnings are counted debt rather than findings, and there are enough of them to
bury everything else. The filter keeps every error — proved by planting one and watching it
survive.

Two of the eight reach the network and one needs a build, so any of the three may report
**exit 2 — nothing was checked**. That is not a pass and not a corpus failure; read it as
unchecked and say so.

### linkcheck
!`python3 tools/linkcheck.py`

### pagelint
!`python3 tools/pagelint.py | grep -v '(warning)' | tail -20`

### shape
!`python3 tools/urlmap.py --check-shape`

### redirects
!`python3 tools/urlmap.py --check-redirects`

### symbolcheck
!`python3 tools/symbolcheck.py`

### versioncheck
!`python3 tools/versioncheck.py | tail -5`

### optioncheck
!`dotnet run --project tools/optioncheck 2>&1 | tail -5`

### verify (live sitemap)
!`python3 tools/urlmap.py --verify | tail -5`

## Your Task

1. Identify the active phase — the earliest one whose document exists and whose approval marker
   does not
2. Display that phase's document
3. Review it against its checklist below
4. Report every gate result, using the discipline below. **Compare against `tools/README.md`**,
   which carries the expected number for each gate at a named ref — do not paste numbers here, and
   do not trust a number in this file over that one
5. Re-run any gate that reported **exit 2** above, once, before treating it as unchecked
6. Ask the user whether to approve. If they approve, create the approval marker

### How to read a gate result

**The rule is the same for all eight, and `linkcheck` is where it came from: is this breakage
pre-existing, or is it yours?**

- **Requirements and design phases** — nothing has been written yet, so any breakage is
  pre-existing. Put it on the record in one line, **do not block approval**, and offer to fix it
  as a separate change
- **Tasks phase, and every review after writing has begun** — breakage may be yours. Establish
  what was already broken with `git log` for the spec's base ref and `git diff --stat <ref>` for
  what it has touched; anything this spec introduced is **blocking**
- **A number that moved when the spec predicted it would not is a finding**, even when it moved in
  a direction that looks like an improvement. Diff the two runs and name the cause. Do not adopt
  the new number by arithmetic
- **Exit 2 is not a pass.** It means nothing was checked — bad arguments, an unusable watchlist, an
  unreachable authority. Report it as unchecked, never as clean
- **Read the scope line, not just the verdict.** `0 stale pins` out of 0 and out of 18 are
  different claims, and so are `0 findings` and `0 findings, 1 silenced`

Do not re-run a gate that answered; the output above is current. To check one file while fixing,
pass it a path — `python3 tools/linkcheck.py contents/SomePage.md`,
`python3 tools/pagelint.py contents/SomePage.md`,
`dotnet run --project tools/optioncheck -- contents/SomePage.md`.

**`--verify` matters most after a `SUMMARY.md` change**, because nesting a page **moves its
published URL** and the redirect is the deliverable that gets forgotten. It is the one gate CI
never runs, so this command is the only place it is seen.

### Phase Detection

| Phase | Document | Approval Marker |
|-------|----------|-----------------|
| Requirements | requirements.md | .requirements-approved |
| Design/Outline | design.md | .design-approved |
| Tasks | tasks.md | .tasks-approved |
| Writing | tasks.md approved, boxes still unticked | — none; it ends when they are all ticked |
| Acceptance | tasks.md, fully ticked | .accepted |

The active phase is the first one whose document exists and whose marker does not. **Writing is the
one phase with no marker**, so it is detected by the boxes rather than by a file: tasks approved and
at least one `- [ ]` left. Reviewing a writing phase means the *Writing Review* checklist below, on
the pages that phase touched.

### Review Checklists

**Requirements Review:**
- Is the subject declared — feature, reader problem, or process — and are the sections it makes
  N/A named and marked, rather than left empty?
- Is the topic overview clear to someone with no prior context?
- Are current state and target state well defined, and the audience identified?
- Are source materials named specifically (ADRs, files, samples), not gestured at?
- Are deliverables file names, each with the page type it will be?
- Is scope clear with P0/P1/P2, and is out-of-scope explicit?
- **Does every acceptance criterion name its instrument, or say it has none?** An unmarked
  criterion is the one that goes unmet — both that ever have were unmarked
- Are open questions listed by name, each with a recommendation?
- Does every number carry the command that produced it?

**Design/Outline Review:**
- Does the file structure match the requirements deliverables?
- Does every new page have a page type, a banner, qualified `##` headings and an opening sentence?
- Is the reading order logical, and are outlines detailed enough to write from?
- Are code examples identified with source references, and **has the API they name been verified
  live** rather than copied?
- Are `SUMMARY.md` changes shown before/after, and does any **nesting** carry its redirect?
- Does it **re-verify the requirements rather than quoting them**, and say what it changed?
- **Does it predict which gates the work will move, including "none"?**
- Is the document self-contained and readable on its own?

**Tasks Review:**
- Are tasks small and specific — one section or file each — with clear inputs and outputs?
- Is each phase one pull request, and are the phases deliverable-shaped rather than a default
  template applied unchanged?
- Is there a standing-obligations section, so obligations are stated once and not per task?
- **Does every new check get a red-proof, with a two-way control?**
- Are inherited counts re-derived by two methods that agree?
- Is the mismatch recorded before it is fixed?
- Is there a final acceptance phase owning the walk and the ledgers?

**Writing Review — the three things no gate decides:**

`CLAUDE.md` § *The ledger* carries three conventions marked **review only**, and this is the review
they mean. No linter will ever report them, so a page that breaks one is green everywhere.

- Are the ❌/✅ **version markers** right on any block that shows a V9 and a V10 form?
- Does every new or edited C# block **compile against the released packages** — not a
  `ProjectReference` into `src/` — and does a step that legitimately does not build say so?
- Does any block asserting **behaviour** — an exception type, an ordering, a precedence — get
  **run, with a control**? Four compiling, reviewed examples in this programme were wrong

**Acceptance Review:**
- Walk the criteria **in order of the ones marked as having no instrument first** — they are the
  ones no run has been checking
- For each, name the command and show its output, or say who read it and what they found
- **What was found wrong is recorded before it was fixed.** A spec that can only show the corpus
  is right now cannot show it was ever wrong
- Did anything change that should not have? Diff the page set against the ref the spec named
- Are the ledgers written — every defect found, and the workflow friction met on the way?

### Approval

When the user approves, create the marker: `touch spec/[spec-dir]/.requirements-approved`,
`.design-approved`, `.tasks-approved`, or `.accepted`. Then tell them the next step.
