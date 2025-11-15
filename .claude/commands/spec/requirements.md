---
allowed-tools: Bash(cat:*), Bash(test:*), Bash(touch:*), Write
description: Create or review requirements specification
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null || echo "No active spec"`
Spec directory contents: !`ls -la spec/$(cat spec/.current-spec 2>/dev/null)/ 2>/dev/null || echo "Spec not found"`

## Your Task

For the current active specification:

1. Check if requirements.md exists
2. If not, create a comprehensive requirements.md with:
   - Feature overview
   - User stories with acceptance criteria
   - Functional requirements (P0, P1, P2)
   - Non-functional requirements
   - Constraints and assumptions
   - Out of scope items
   - Success metrics
3. If it exists, display current content and suggest improvements
4. Remind user to use `/spec:approve requirements` when ready

Use the Write tool to create/update the requirements.md file.
