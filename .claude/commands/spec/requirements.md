---
allowed-tools: Bash(cat:*), Bash(echo:*), Bash(test:spec/*), Bash(ls:spec/*), Bash(grep:*), Bash(wc:*), Bash(git log:*), Bash(gh issue list:*), Bash(gh search:*), Bash(gh api:*), Write, Edit, Read, Glob, Grep
description: Create or review requirements specification
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null || echo "No active spec"`

## Your Task

1. **Re-derive the spec's README before executing it.** It was written before anyone looked. A
   spec that starts by executing a stale README ships work that is already done — put the command
   beside every figure you carry forward (`grep -c`, `wc -l`, `git log`), and record any gap it
   named that is already closed
2. **Declare the subject** (below). It decides which sections apply and which research runs
3. If requirements.md does not exist, write it; if it does, show it and propose improvements
4. Point the user at `/spec:review` when it is ready for approval

### Declare the subject — one of three

| Subject | The deliverable is | Sections that do **not** apply |
|---|---|---|
| **Feature** | pages describing something Brighter or Darker does | — |
| **Reader problem** | pages answering a question readers keep asking | — |
| **Process** | tools, commands, conventions — the way the docs get written | *SUMMARY.md changes*, *Target audience*, *Mode mix* |

State the subject in the document, in one line, with the sections it makes N/A **named and marked
N/A rather than left empty**. On a process spec *SUMMARY.md changes* is not an empty section, it is
a category error — and an empty heading reads as an oversight nobody got to.

### Research, per subject

Do the research before writing, and cite what you read.

- **Feature** — ADRs in `../Brighter/docs/adr/`, `../Brighter/release_notes.md`, the source under
  `src/`, the samples, and `../Darker` for query-side work
- **Reader problem** — GitHub discussions and issues (`gh search issues`, `gh issue list`,
  `gh api` for discussions) and `contents/FAQ.md`. **Dedupe by thread, not by row**, and
  **paginate before quoting any count**: a first page of results is not a census
- **Process** — the command files in `.claude/commands/spec/`, `CLAUDE.md`, `tools/README.md`, and
  the closed specs' `tasks.md`, which is where the friction and defect ledgers live

Always: `SUMMARY.md` for where this sits, `contents/` for what already covers it, and
`contents/Glossary.md` and `contents/BasicConcepts.md` for terms already defined.

### What the document contains

**Topic overview** · **Current state** · **Target state** · **Target audience** · **Source
material** · **Scope, P0/P1/P2** · **Out of scope** · **Deliverables** · **SUMMARY.md changes** ·
**Constraints**, plus the three below.

**Deliverables are specific** — file names, not descriptions of file names. For each one, say
**which of the four page types** it is and why. A feature that gets only a `Reference` page is a
decision, and it should be visible as one.

### Acceptance criteria — each names its instrument, or is marked as having none

Number them. For each, name the **command** that decides it. Where no command can, write
**"no instrument — checked by reading"** and say who reads it.

This is the one rule here bought with evidence: **both criteria ever found unmet at a close —
009's AC7 and 012's AC1 — were the unmarked ones.** A criterion whose instrument is missing is
invisible exactly until the acceptance walk, and by then the work has shipped.

Two failures to avoid, both real:

- **Phrase the test as *contains*, not *ends with*,** unless position is genuinely what you mean.
  013's AC7 said a guide *"ends with"* a verification step where it meant *"contains"* one
- **"A file exists" is not an instrument.** 013's AC8 was checked by a row count in
  `pagetypes.tsv`, and nothing reads that file. If nothing consumes the artefact, the criterion is
  unmarked — say so

### Open questions

List them **by name**, numbered, each with a **recommendation** and what it depends on. A question
with no recommendation is a question that arrives at the design review unprepared, and a question
nobody wrote down is one that gets decided by whoever is typing.

### Quality checklist

The document should:

- be readable by a person or an LLM with no prior context
- distinguish P0 / P1 / P2 plainly
- name specific source files and samples rather than gesturing at them
- carry a command beside every number in it
- apply this checklist to itself

Use Write to create or update requirements.md.
