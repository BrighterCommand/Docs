---
allowed-tools: Bash(ls:*), Bash(echo:*), Bash(test:*), Bash(cat:*)
description: Switch to a different specification
argument-hint: <spec-id-or-name>
---

## Available Specifications

!`ls -d spec/*/ 2>/dev/null | sort`

Current spec: !`cat spec/.current-spec 2>/dev/null || echo "None"`

## Your Task

Switch the active specification to: $ARGUMENTS

1. Verify the spec directory exists (match by ID number or name)
2. Update `spec/.current-spec` with the spec directory name
3. Show the status of the newly active spec (phase, progress)
4. Display the recommended next action

If no argument provided, list all available specs and ask the user which to switch to.
