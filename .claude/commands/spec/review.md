---
allowed-tools: Bash(cat:*), Bash(test:spec/*), Bash(ls:spec/*), Bash(touch:spec/*), Bash(grep:*), Bash(python3 tools/linkcheck.py:*), Read
description: Review current specification phase
---

## Current Spec Status

Current spec: !`cat spec/.current-spec 2>/dev/null || echo "No active spec"`

## Internal Link Check

!`python3 tools/linkcheck.py`

## Your Task

1. Identify which phase is currently active (the earliest phase not yet approved)
2. Display the content of that phase's document
3. Provide a review against the checklist for that phase (see below)
4. Report the Internal Link Check result (see below)
5. Ask the user if they want to approve. If approved, create the approval marker file.

### Internal Link Check

The `## Internal Link Check` section above is the output of `tools/linkcheck.py`,
which walks every published markdown file and verifies that each internal link
resolves — both that the target file exists and that the anchor matches a real
heading.

How to treat the result depends on the phase:

- **Requirements and Design phases** — no documentation has been written yet, so
  any breakage is pre-existing. Mention it in one line so it is on the record,
  but do not block approval on it. Offer to fix it as a separate change.
- **Tasks phase, and any review after writing has begun** — breakage may be
  *yours*. Compare against what was already broken; anything introduced by this
  spec's work is **blocking** and must be fixed before approval.

If the check reports "No broken internal links", say so in one line and move on.
Do not re-run the tool — the output above is current. To check a single file
while fixing, run `python3 tools/linkcheck.py contents/SomePage.md`.

### Phase Detection

Check for these files in the spec directory to determine the current phase:

| Phase | Document | Approval Marker |
|-------|----------|-----------------|
| Requirements | requirements.md | .requirements-approved |
| Design/Outline | design.md | .design-approved |
| Tasks | tasks.md | .tasks-approved |

The active phase is the first one where the document exists but the approval marker does not.

### Review Checklists

**Requirements Review:**
- Is the topic overview clear to someone with no prior context?
- Are current state and target state well defined?
- Is the audience identified?
- Are source materials listed (ADRs, code, samples)?
- Are deliverables specific (file names, not vague descriptions)?
- Is scope clear with P0/P1/P2 priorities?
- Are SUMMARY.md changes specified?

**Design/Outline Review:**
- Does the file structure match the requirements deliverables?
- Is the reading order logical for newcomers?
- Are section outlines detailed enough to write from?
- Are code examples identified with source references?
- Are SUMMARY.md changes shown as a before/after?
- Is the document self-contained and readable on its own?

**Tasks Review:**
- Are tasks small and specific (one section or file each)?
- Do tasks have clear inputs and outputs?
- Are dependencies between tasks noted?
- Is there a final task for SUMMARY.md and link verification (running `python3 tools/linkcheck.py`)?
- Are tasks ordered so foundational content comes first?

### Approval

When the user approves, create the approval marker file:
- `touch spec/[spec-dir]/.requirements-approved`
- `touch spec/[spec-dir]/.design-approved`
- `touch spec/[spec-dir]/.tasks-approved`

Then inform the user of the next step.
