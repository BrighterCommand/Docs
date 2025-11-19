---
allowed-tools: Bash(ls:*), Bash(cat:*), Bash(grep:*), Bash(test:*), Bash(find:*)
description: Show all specifications and their status
---

## Gather Status Information

All specs: !`ls -d spec/*/ 2>/dev/null | sort`
Current spec: !`cat spec/.current-spec 2>/dev/null || echo "None"`

## Your Task

Present a clear status report showing:
1. All specifications with their IDs and names
2. Current active spec (highlighted)
3. Phase completion status for each spec
4. Task progress percentage if applicable
5. Recommended next action for active spec