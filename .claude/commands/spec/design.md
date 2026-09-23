---
allowed-tools: Bash(cat:*), Bash(echo:*), Bash(test:spec/*), Bash(grep:*), Bash(wc:*), Bash(git grep:*), Bash(git -C ../Brighter grep:*), Bash(git -C ../Darker grep:*), Bash(python3 tools/symbolcheck.py:*), Bash(python3 tools/urlmap.py:*), Write, Edit, Read, Glob, Grep
description: Create documentation outline and structure
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null || echo "No active spec"`

## Your Task

1. Verify the requirements are approved — a `.requirements-approved` file in the spec directory.
   If it is missing, send the user to `/spec:review` and stop
2. **Re-verify the requirements; do not quote them** (below). Say what you changed
3. Write or update design.md, per the sections below
4. Point the user at `/spec:review` when it is ready for approval

The **subject declared in the requirements** — feature, reader problem, or process — decides which
sections below apply. Name the ones it makes N/A and mark them N/A; on a process spec *SUMMARY.md
changes* is a category error, not an empty section.

### Re-verify the requirements, do not quote them

*"Reference the requirements for traceability"* reads as *quote them*, and a design that quotes
cannot discover anything. Re-read each deliverable against what is now known and record the deltas
in the design: one that shrank, one that changed shape, one that turned out to be already done.
**Re-derive every figure you carry forward** — `grep -c`, `wc -l`, beside the number — rather than
inheriting it. In this spec's own history a probe run at design time **inverted one deliverable and
halved another**.

### A design may run an experiment — and says so before approval

**A design resting on an unmeasured number says which number, and how it will be measured, before
the design is approved.** Not after, and not at implementation time: by then the thing the number
would have changed is already built. If the answer needs a probe, run it, and put its method, its
controls and its output in the design.

Controls are two-way — a case that should be found and a case that should not. A probe with no
known-absent control cannot tell a real zero from a broken instrument.

### The outline, file by file

For each page to be created or updated:

- **File name and path**, and **which of the four page types** it is — `Tutorial`, `How-to`,
  `Reference`, `Explanation`. The type decides the page's shape, so it is a design decision, not
  something the writer picks later
- **The banner line** it will carry, and **the opening sentence** it will lead with — that sentence
  is what `/llms.txt`, the search snippet and the `.md` variant all print, and it becomes the page's
  `description:` front matter verbatim
- **Purpose** in one sentence, and an approximate **target length**
- **The `##` headings**, each already qualified by its subject — `## Kafka Configuration`, never
  `## Configuration`. A design that outlines unqualified headings hands the writer a page that
  fails the linter
- **Code examples**, each with its source — and see the next section
- **Cross-links**, and any glossary terms to define or reference

`CLAUDE.md` §§ *Page banner*, *The opening sentence*, *Page descriptions*, *Heading qualification*
and *File Organization Pattern* carry all of this. Cite them; do not restate them.

### Verify the API the design will print

*"Note the source file or sample"* asks where an example came from. It never asks whether it is
right. Before the design names a type, a method or an option, **resolve it in the product**:

```bash
git -C ../Brighter grep -n 'class CommandProcessor' -- src
git -C ../Darker grep -n 'IQueryProcessor' -- src
```

Three states, and only the middle one needs prose: **live**; **forthcoming, and the page will say
so**; or **dead — a defect**, which the design records rather than the writer discovering later.

Two traps, both live:

- **`git grep -w` tests the characters *next to* the match, not the ends of your pattern.** So
  `.Handle(` matches `>.Handle()` in prose and misses `.Handle(command)` in code: in `../Brighter`,
  `git grep -lwF '.Handle('` finds **0** files where the same search without `-w` finds **179**.
  A zero from a flag that cannot match the shape you are looking for is not evidence
- **`python3 tools/symbolcheck.py` only knows the names on its watchlist.** A green run is not a
  statement that an API is live. `--census` reports what the corpus asserts, and is never a gate

### SUMMARY.md — which nesting, and what it costs

Say exactly where each new page goes, with the before/after entries. Then say **which of the two
nestings** it is: **sub-topic** nesting puts detail under the page it elaborates; **family**
nesting puts peers under an overview page, and is the only one that absorbs growth. `CLAUDE.md`
§ *The two kinds of nesting* has the distinction and when each is wrong.

**Nesting a page moves its published URL**, so it owes a redirect in `.gitbook.yaml` —
`python3 tools/urlmap.py --redirects OLD_SUMMARY` generates the block. A move without its redirect
is a 404 for everyone holding the old link, and it is the deliverable that gets forgotten.

### Predict which gates the work will move — including "none"

Name each gate the work will move, the direction, and the cause. `tools/README.md` has the nine,
what each checks, and **the corpus each one walks** — the part that catches people out. Cite it for
the expected numbers; do not paste them here.

**"None" is a prediction, not the absence of one, and it is the case worth writing down**: a
vacuous pass is invisible exactly when no movement was expected. **A gate's scope is not the site's
scope** — a phase that touched no published page still moved `linkcheck` by one, because `tools/`
is inside its walk.

### Design quality checklist

- Readable by a person or an LLM with no prior context, and self-contained
- Every file has a page type, a banner, an opening sentence and qualified headings
- Every API it names has been resolved, with the command shown
- Every number carries the command that produced it, or the design says how it will be measured
- Gate movement predicted, including "none"
- Structure obvious at a glance — tree diagrams over prose
- Detailed enough that each file could be written by someone who was not in the room

Use Write to create or update design.md.
