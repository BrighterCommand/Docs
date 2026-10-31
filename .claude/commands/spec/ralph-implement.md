---
allowed-tools: Bash(cat:*), Bash(test:*), Bash(grep:*), Bash(git status:*), Bash(git diff:*), Bash(git add:*), Bash(git commit:*), Bash(python3 tools/*.py:*), Bash(dotnet build:*), Bash(dotnet run --project tools/optioncheck:*), Read, Write, Edit, Glob, Grep, Agent, AskUserQuestion
description: Implement a batch of approved tasks sequentially, each delegated to a sub-agent on a planned model
argument-hint: next N | end of phase | X.Y to X.Z
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null || echo "No active spec"`

Bound: $ARGUMENTS

**What this is.** `/spec:implement` run in a loop over a bounded batch of tasks from the same
approved `tasks.md`, in the same format. Named after Brighter's `/spec:ralph-implement`, whose shape
it follows: strictly sequential, a sub-agent per task, and the main agent (the *orchestrator*)
owning task selection, verification, the checkbox and the commit.

**What it changes is when the expensive gates run, and nothing else.** Every rule
`.claude/commands/spec/implement.md` states binds each task here exactly as it does there: the
page conventions, *before the page names a type*, compiled against the released packages, a
behavioural claim run with a control, and the spec's standing obligations. This file cites that
one and does not restate it.

**Runtime.** Run the orchestrator on **Opus**, in auto mode, so the loop does not stop for each
permission prompt. Advise if either is not so, then proceed — do not block.

## Step 0: Parse the bound and plan the batch

This runs once, before any task.

1. Verify `spec/{current-spec}/.tasks-approved` exists. If not, stop and say so.
2. **Parse the bound** from `$ARGUMENTS`:

   | Form | Means |
   |---|---|
   | `next N`, or a bare `N` | the next N unchecked tasks |
   | `end of phase` | every unchecked task from the next one to the last task of its phase, inclusive |
   | `X.Y to X.Z`, `X.Y-X.Z` | those tasks, inclusive; each must be unchecked, and none before X.Y in the same phase may be |
   | empty | ask, with `AskUserQuestion`, which of the three |

   A bound never crosses a phase boundary: phases are separate PRs with a sign-off between them.
   If it would, stop the batch at the phase's last task and say so.
3. **Plan a model for each task** by its title, and print the plan as a table — task, title,
   model, why — before starting:

   | Task shape (by title) | Model | Why |
   |---|---|---|
   | *Predict …* | **opus** | reading defects against source, and bands with mechanisms |
   | *Repair … reachable blocks* | **sonnet** | mechanical: `using`s, stubs, renamed arguments; the compiler checks the work |
   | *Repair … hard blocks* | **sonnet**, diff reviewed by the orchestrator | the edits are mechanical, but what a fragment teaches and whether a block may be skipped are judgements |
   | *Run … behavioural claims* | **opus** | a probe that cannot fail looks the same as one that passes; four compiling examples lied |
   | *Second pass …* | **opus** | rulings and recurrences |
   | *Close …* | **opus** | reconciliation against the prediction, and the merge ask |
   | anything else | **opus** | when unsure, the stronger model |

   Ask the user, once, whether to proceed with the plan or change a model. That is the last
   question the loop asks unless it pauses.

## Step 1: Before each task

1. **STOP file.** If `RALPH_STOP` exists at the repository root, stop with status `STOPPED`.
2. **Select** the next unchecked task within the bound. None left: status `BOUND_REACHED`.
3. **Baseline**: `git status --porcelain` into the scratchpad, to compare after.

## Step 2: Delegate the task

Launch `Agent` with `subagent_type: "general-purpose"` and the planned `model`. The prompt
carries:

1. The task's full text from `tasks.md`, verbatim, with its phase heading.
2. **What to read first**, by path, since the sub-agent has none of this conversation:
   `CLAUDE.md`; `.claude/commands/spec/implement.md` (its rules are the task's rules);
   `spec/{current-spec}/tasks.md` § 1 *Standing obligations* and the phase's *as executed*
   section so far; the `design.md` sections the task's Input names; `tools/README.md`; and
   `PROMPT.md`'s *START HERE* block for the recipes and probe notes.
3. **Its constraints:**
   - **Never** `git commit`, `git push`, or flip a checkbox. It **may** write the task's Output into
     `tasks.md` — that is where this spec's tasks put it.
   - Never modify `../Brighter` or `../Darker`. A sample change is a pull request, asked per PR.
   - Do not ask the user anything. Where a task would stop to ask, **return** instead.
4. **Its return**, as text:

   ```text
   STATUS: DONE | PAUSE | FAILED
   COMPLETE: yes | no          (PAUSE only: is the task's Output finished?)
   FILES:
     <one path per line>
   SUMMARY: <one line for the commit subject, after "docs: <spec> task X.Y — ">
   DETAIL: <the commit body's bullets>
   CHECKS: <each check run and its result line>
   PAUSE_REASON: <empty unless PAUSE>
   ```

   **`PAUSE`** is for what the batch never decides on its own, or should not pass unseen:
   - **a scope question** — a ruling the maintainer must make: a page rewritten, a feature removed,
     a new skip reason, a package added to a pin, an issue filed upstream, anything the obligations
     reserve to them. A change in the world the gates measure against — a new release, a gate gone
     red that the task did not touch — is one too
   - **a new defect** — any the phase's prediction and ledger do not already name, **including one
     found inside a block the task is already repairing**. A defect in the task's own blocks is
     repaired and recorded as the obligations require, and the task finished (`COMPLETE: yes`); one
     outside them is recorded and left. Either way the sub-agent returns `PAUSE`, so the maintainer
     can spot-check it before the batch goes on. It is a checkpoint, not a failure

   A defect the prediction named is the task's work, not a pause. **An earlier task's prediction is
   never edited** — it is reconciled at the close (obligation 6); a figure that moved is recorded in
   the task's own output.

## Step 3: Verify, commit, mark (orchestrator)

1. **Scope.** `git status --porcelain` against the baseline: every new or changed path is in
   `FILES`, and every one is in this repository. Anything else is not committed; the task is marked
   `FAILED` with what was found.
2. **Cheap checks**, on the paths in `FILES` — the ones `implement.md` § *Quality check* lists:
   `git add` them, then `pagelint` on the paths and `--changed origin/master`, `linkcheck`,
   `symbolcheck`, and `optioncheck` if a marked table was touched. A red check is `FAILED`.
3. **Review**, where Step 0 planned it: read the diff of a *hard blocks* task before committing,
   against `implement.md` § *What you decide*. A judgement you would not defend is `FAILED`, with
   the reason.
4. **On `DONE`**, flip the task's `- [ ]` to `- [x]` and make **one** commit, the repository's
   shape:

   ```text
   docs: <spec> task X.Y — <SUMMARY>

   <DETAIL>

   Co-Authored-By: <the orchestrator's model>
   Co-Authored-By: <the sub-agent's model, if different>
   ```

   with the session's attribution lines.
5. **On `PAUSE`**, verify and review as for `DONE`, then commit the task's work. Flip its checkbox
   only if `COMPLETE: yes`. Stop with status `PAUSED`, printing `PAUSE_REASON`: the question for the
   maintainer, or the new defects to spot-check, each by page and block with its ledger row.
   `/spec:ralph-implement` resumes from the next unchecked task.
6. **On `FAILED`**, commit nothing for the task, leave its changes in the working tree, and stop
   with status `FAILED`. The next task depends on this one.

## Step 4: Gates, once per batch

After the last task, or before any *Close* task in the batch, the orchestrator runs the full gates
`tools/README.md` row by row: rebuild the pins and `blockcheck`, then `--report`, `pagelint`,
`--plant`, `linkcheck`, `symbolcheck`, `versioncheck`, `optioncheck`. Any non-zero exit stops the
batch with status `GATE_RED` and the output. A *Close* task needs these green before it starts,
and it ends at the merge ask (obligation 7): the loop never merges.

## Step 5: Report

```text
=== RALPH SUMMARY ===
Bound: <as given>
Tasks done this run: <list, with model>
Total: X of Y          (grep -c '^- \[x\] \*\*Task' against grep -c '^- \[.\] \*\*Task')
Gates: <Step 4's figures, or "not run">
Status: BOUND_REACHED | PAUSED | FAILED | GATE_RED | STOPPED
Next: <the next task, or the question that paused the run>
=== END RALPH ===
```

Then update `PROMPT.md`'s *START HERE* block as an end of session does: the head, the task count,
what is waiting on the maintainer, and the first steps.
