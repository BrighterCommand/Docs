---
allowed-tools: Bash(cat:*), Bash(test:*), Write
description: Create implementation task list
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null`
Design approved: !`test -f spec/$(cat spec/.current-spec)/.design-approved && echo "Yes" || echo "No"`

## Your Task

1. Verify design is approved
2. Create tasks.md with:
   - Overview with time estimates
   - Phase breakdown (Foundation, Core, Testing, Deployment)
   - Detailed task list with checkboxes
   - Task dependencies
   - Risk mitigation tasks
3. Each task should be specific and actionable
4. Use markdown checkboxes: `- [ ] Task description`

Organize tasks to enable incremental development and testing.