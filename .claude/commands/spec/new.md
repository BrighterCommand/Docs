---
allowed-tools: Bash(ls:*), Bash(mkdir:*), Bash(date:*), Write, Read
description: Create a new documentation specification
argument-hint: <topic-name>
---

## Existing specs

!`ls -d spec/*/ 2>/dev/null`

## Your Task

Create a new documentation specification for the topic: $ARGUMENTS

1. Take the next ID from the listing above — three digits, `001`, `002`, and so on
2. `mkdir -p spec/[ID]-$ARGUMENTS`
3. Write the new directory name — `[ID]-$ARGUMENTS` — to `spec/.current-spec`
4. Write `spec/[ID]-$ARGUMENTS/README.md` from the template below, dating it with
   `date +%Y-%m-%d`
5. Tell the user the next step: `/spec:requirements`

### The README is a starting point, not a source of truth

Put this line in the README, at the top, and mean it:

> **Re-derive this README before executing it.** It was written before anyone looked.

A spec that starts by executing a stale README ships work that is already done, or argues from a
count that was true once. Both have happened here: 013's README named **six** gaps and **five** of
them were closed before the spec began; 014's defect count was **wrong by two** before its first
phase started. The repair is cheap — a command beside each figure — and it only works if the
README says out loud that it expects to be wrong.

### README.md Template

```markdown
# Spec [ID]: [Topic Name]

**Created:** [date]
**Status:** Requirements Phase

> **Re-derive this README before executing it.** It was written before anyone looked — check every
> count and every named gap against the tree, with the command beside the figure.

## Topic Overview

[What the documentation topic is, and why it is needed]

## Subject

[One of: **feature** · **reader problem** · **process**. It decides which requirements sections
apply and which research runs — see `/spec:requirements`]

## Current State

[What the docs say today. Name files and give counts, each with the command that produced it]

## Acceptance criteria

[Numbered. For each, **the command that decides it** — or the words **"no instrument — checked by
reading"**, and who reads it. Both criteria ever found unmet at a close were unmarked ones, so an
unmarked criterion is a known risk rather than an oversight]

## Open questions

[Numbered, by name, each with a **recommendation** and what it depends on. A question with no
recommendation arrives at the review unprepared; a question nobody wrote down gets decided by
whoever is typing]

## Status Checklist

- [ ] Requirements gathered
- [ ] Requirements reviewed and approved
- [ ] Documentation outline created
- [ ] Outline reviewed and approved
- [ ] Writing tasks identified
- [ ] Writing complete
- [ ] Documentation reviewed
- [ ] Spec closed

## Next Steps

1. Re-derive the counts above
2. Read `SUMMARY.md` for where this sits, and `contents/` for what already covers it
3. Identify source material — source, ADRs, release notes, samples; or issues and discussions for
   a reader problem; or the commands, `CLAUDE.md` and the closed specs for a process
4. Run `/spec:requirements`, and get the requirements approved before going further

## Notes

- `CLAUDE.md` is the authority on documentation standards; cite it rather than restating it
- `tools/README.md` is the authority on the gates and their expected numbers
- Source code lives in `../Brighter` and `../Darker`, and is **read-only**
- `SUMMARY.md` gets updated whenever a page is added, or the page is an orphan
```

Use `mkdir` for the directory and Write for `spec/.current-spec` and the README.
