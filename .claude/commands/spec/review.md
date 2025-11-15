---
allowed-tools: Bash(cat:*), Bash(test:*)
description: Review current specification phase
---

## Current Spec Status

!`current=$(cat spec/.current-spec 2>/dev/null)
if [ -n "$current" ]; then
    echo "Active spec: $current"
    echo "Files present:"
    ls -la "spec/$current/" | grep -E "(requirements|design|tasks)\.md"
    echo ""
    echo "Approval status:"
    ls -la "spec/$current/" | grep "approved"
fi`

## Your Task

1. Identify which phase is currently active (not yet approved)
2. Display the content of that phase's document
3. Provide a review checklist:
   - Does it meet all criteria?
   - Is it complete and clear?
   - Any missing elements?
4. Remind user how to approve when ready.