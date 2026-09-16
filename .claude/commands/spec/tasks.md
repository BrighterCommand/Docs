---
allowed-tools: Bash(cat:*), Bash(echo:*), Bash(test:spec/*), Bash(grep:*), Bash(wc:*), Write, Edit, Read
description: Create writing task list
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null || echo "No active spec"`

## Your Task

1. Verify the design is approved — a `.design-approved` file in the spec directory. If it is
   missing, send the user to `/spec:review` and stop
2. Write tasks.md: the phases, the standing obligations, and the tasks
3. Point the user at `/spec:review` when it is ready for approval

### One phase is one pull request

A phase is a coherent unit that **merges before the next branch starts**. `tools/README.md`
§ *One phase is one pull request* carries the contract and what it obliges — green gates before the
PR opens, predicted movement reconciled, a gate and the corpus that satisfies it merging together,
a red-proof for every new check, and a sign-off for anything that changes the published site. Cite
it; do not restate it, and do not paste its numbers.

So the phases decide the pull requests. **Say in the phase heading how many tasks it carries and
that it is one PR**, and order the phases so that nothing merges depending on something that has
not.

### Phases are deliverable-shaped

**Research → Core → Supporting → Polish is a default, and it is labelled one here so that you can
leave it.** An unlabelled default invites a task list that fights its own design: the design named
deliverables, and a phase that splits one deliverable across three phases hides the moment it is
finishable.

Derive the phases from the design's deliverables. Where the default fits, say that it fits; where
a deliverable needs a probe first, or a tool and the corpus it checks in one PR, the phase is
shaped by that instead. **The last phase is always acceptance** (below).

### The standing obligations — stated once, binding every task

Give the list its own section at the top and **do not restate it per task**; restating invites the
reader to treat the unrestated ones as optional. What belongs there, at minimum:

1. **Re-derive any count before quoting it**, with the command beside the figure and **two methods
   that agree**. A number inherited from the requirements, the README, or an earlier phase has
   already begun to rot — 013's README named six gaps of which five were closed
2. **Record the mismatch before fixing it.** The corrected state is the only thing left afterwards,
   so a spec that does not write down what was wrong cannot show the corpus was ever wrong. This is
   the whole evidential product of a repair phase
3. **A check that has never failed has not been shown to work.** Every new check gets a red-proof,
   with its output recorded, and **every control is two-way** — a known-present case and a
   known-absent one
4. **Prose and permission ship together**, and the check runs **in both directions**: a command
   whose prose names a tool its `allowed-tools` forbids is a broken instruction, and a grant no
   reading of the prose reaches is a permission nobody asked for
5. **Cite `CLAUDE.md` and `tools/README.md`; never restate them.** A restatement is a copy that
   drifts, and the copy is the one people read
6. **Predict gate movement before the work, including "none"** — then reconcile
7. Whatever this spec's own subject adds

### The task format

```markdown
- [ ] **Task X.Y:** [Action verb] [specific deliverable]
  - Input: [what to read before writing — files, sections, samples, by name]
  - Output: [the file or section produced, specifically enough to check]
  - Notes: [guidance that is true of this task only]
```

**The Output line is a promise `/spec:update-task` will check**, so write it as something that
either exists or does not. *"Update the docs"* cannot be checked; *"two rows in
`tools/symbolwatch.tsv`"* can — and a task whose Output could not be checked was ticked in 013 with
its two rows unwritten.

Keep tasks small — one file or one section, completable in one sitting. Note dependencies between
them, and group the independent ones so they can run in parallel.

### The acceptance phase, last and always

The final phase walks the acceptance criteria and writes the ledgers. It owns:

- **The walk**, one criterion at a time, each naming the command that decides it and that command's
  output. **Start with the criteria marked as having no instrument** — both criteria ever found
  unmet at a close were unmarked ones
- **A backwards check**: what changed that should not have. Diff the page set against the ref the
  spec named, because *"while I'm here"* is how a spec quietly widens
- **The defect ledger** — every defect the spec found, with a *found by* column that distinguishes
  the instrument's wins from the re-derivation's
- **The friction ledger** — where the workflow itself got in the way. It is what the next spec
  inherits, and it is as much a product of the spec as the pages are
- **Closing**: the README checklist, and the residual gap named in one sentence, since that is the
  line the next spec starts from

### Task quality checklist

- Is each phase one pull request, and are the phases deliverable-shaped rather than the default
  applied unchanged?
- Is there a standing-obligations section, and is it *not* repeated per task?
- Does every new check have a red-proof task, with a two-way control?
- Is every inherited count re-derived, by two methods, with the commands shown?
- Does every task's Output name something that can be seen to exist?
- Is there an acceptance phase last, owning the walk and both ledgers?
- Does the totals line match the list? `grep -c '^- \[.\] \*\*Task'` against the number you wrote

Use Write to create tasks.md.
