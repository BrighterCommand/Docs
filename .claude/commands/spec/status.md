---
allowed-tools: Bash(ls:*), Bash(cat:*), Bash(grep:*), Bash(test:*), Bash(find:*)
description: Show all specifications and their status
---

## Gather Status Information

All specs: !`ls -d spec/*/ 2>/dev/null | sort`
Current spec: !`cat spec/.current-spec 2>/dev/null || echo "None"`

For each spec directory, check:
!`for dir in spec/*/; do
    if [ -d "$dir" ]; then
        echo "=== $dir ==="
        ls -la "$dir" | grep -E "(requirements|design|tasks)\.md|\..*-approved"
        if [ -f "$dir/tasks.md" ]; then
            echo "Task progress:"
            grep "^- \[" "$dir/tasks.md" | head -5
            echo "Total tasks: $(grep -c "^- \[" "$dir/tasks.md" 2>/dev/null || echo 0)"
            echo "Completed: $(grep -c "^- \[x\]" "$dir/tasks.md" 2>/dev/null || echo 0)"
        fi
        echo ""
    fi
done`

## Your Task

Present a clear status report showing:
1. All specifications with their IDs and names
2. Current active spec (highlighted)
3. Phase completion status for each spec
4. Task progress percentage if applicable
5. Recommended next action for active spec