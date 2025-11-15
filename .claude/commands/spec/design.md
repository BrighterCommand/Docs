---
allowed-tools: Bash(cat:*), Bash(test:*), Bash(ls:*), Write
description: Create technical design specification
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null`
Requirements approved: !`test -f spec/$(cat spec/.current-spec)/.requirements-approved && echo "Yes" || echo "No"`
Current directory: !`ls -la spec/$(cat spec/.current-spec)/ 2>/dev/null`

## Your Task

1. Verify requirements are approved (look for .requirements-approved file)
2. If not approved, inform user to complete requirements first
3. If approved, create/update design.md with:
   - Architecture overview (with diagrams)
   - Technology stack decisions
   - Data model and schema
   - API design
   - Security considerations
   - Performance considerations
   - Deployment architecture
   - Technical risks and mitigations
4. Use ASCII art or mermaid diagrams where helpful

Use the Write tool to create the design document.