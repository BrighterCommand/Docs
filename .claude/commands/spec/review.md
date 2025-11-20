---
allowed-tools: Bash(cat:*), Bash(test:spec/*), Bash(ls:spec/*), Bash(grep:*), Read
description: Review current specification phase
---

## Current Spec Status

Current spec: !`cat spec/.current-spec 2>/dev/null`

## Your Task

1. Identify which phase is currently active (not yet approved)
2. Display the content of that phase's document
3. Provide a review checklist:
   - Does it meet all criteria?
   - Is it complete and clear?
   - Any missing elements?
4. Remind user how to approve when ready.