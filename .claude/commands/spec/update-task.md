---
allowed-tools: Bash(cat:*), Bash(grep:spec/*), Write, Read
description: Mark a task as complete
argument-hint: <task-number>
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null`

## Your Task

Update the task status for: "$ARGUMENTS"

1. Read tasks.md from the current spec directory
2. Find the matching task (by number, e.g. "2.1.1", or by keyword match)
3. Change `- [ ]` to `- [x]` for that task
4. Show updated progress:
   - Tasks completed in this phase
   - Total tasks completed across all phases
   - Percentage complete
5. Suggest the next incomplete task to work on

Use the Read tool to find the task, then the Write tool to update tasks.md.
