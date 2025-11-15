---
allowed-tools: Bash(cat:*), Bash(test:*), Bash(grep:*), Write
description: Start implementation from approved tasks
argument-hint: [phase-number]
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null`
Tasks approved: !`test -f spec/$(cat spec/.current-spec)/.tasks-approved && echo "Yes" || echo "No"`

## Current Tasks

!`if [ -f "spec/$(cat spec/.current-spec)/tasks.md" ]; then
    echo "=== Phase Overview ==="
    grep "^## Phase" "spec/$(cat spec/.current-spec)/tasks.md"
    echo ""
    echo "=== Incomplete Tasks ==="
    grep "^- \[ \]" "spec/$(cat spec/.current-spec)/tasks.md" | head -20
fi`

## Your Task

1. Verify all phases are approved
2. If phase number provided ($ARGUMENTS), focus on that phase
3. Display current incomplete tasks
4. Create an implementation session log
5. Guide user to:
   - Work on tasks sequentially
   - Update task checkboxes as completed
   - Commit changes regularly
6. Remind about using Write tool to update tasks.md

Start implementing based on the task list!