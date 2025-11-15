---
allowed-tools: Bash(ls:*), Bash(echo:*), Bash(test:*)
description: Switch to a different specification
argument-hint: <spec-id>
---

## Available Specifications

!`ls -d spec/*/ 2>/dev/null | sort`

## Your Task

Switch the active specification to: $ARGUMENTS

1. Verify the spec directory exists
2. Update spec/.current-spec with the new spec directory name ([ID]-$ARGUMENTS)
3. Show the status of the newly active spec
4. Display next recommended action

If no argument provided, list all available specs.