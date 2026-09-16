---
allowed-tools: Bash(cat:*), Bash(echo:*), Bash(test:*), Bash(ls:*), Bash(grep:*), Bash(wc:*), Bash(git diff:*), Edit, Read
description: Mark a task as complete
argument-hint: <task-number>
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null || echo "No active spec"`

## Your Task

Update the task status for: "$ARGUMENTS"

1. Read tasks.md from the current spec directory and find the task — by number, e.g. `2.1.1`, or
   by keyword
2. **Check its stated Output exists before flipping the box** (below). This is the step, not a
   formality
3. Change `- [ ]` to `- [x]`
4. Report progress, **re-derived, never incremented**:
   `grep -c '^- \[x\] \*\*Task' <tasks.md>` against `grep -c '^- \[.\] \*\*Task' <tasks.md>`
5. Suggest the next incomplete task

### The Output must exist before the box flips

A task's **Output** line is a promise. Read it, then look:

```bash
test -f contents/SomePage.md && wc -l contents/SomePage.md   # a file that should exist
grep -n '^## Some Section' contents/SomePage.md              # a section inside a file
git diff --stat                                              # the change is actually there
```

**Where the Output cannot be checked mechanically, the tick says so** — append a short note to the
task line naming what was and was not verified. A tick that quietly means *"I believe I did this"*
is indistinguishable from one that means *"I looked"*, and the difference only surfaces at the
acceptance walk, by which time the work has merged.

This is the one rule here bought with evidence: **013's task 4.8 was ticked with its two
`pagetypes.tsv` rows unwritten, and reached `master` that way.** Nothing read the file, nothing
read the task, and the gap was found phases later.

If the Output does not exist, **do not flip the box**. Say what is missing and stop — an unticked
task is a small problem, and a ticked one that is not done is a false record every later phase
reasons from.

Use Read to find the task and Edit to flip that one line.
