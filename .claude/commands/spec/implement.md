---
allowed-tools: Bash(cat:*), Bash(test:spec/*), Bash(grep:spec/*), Write, Read, Glob, Grep
description: Start writing documentation from approved tasks
argument-hint: [task-number]
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null`

## Your Task

1. Verify tasks are approved (look for `.tasks-approved` file in the spec directory)
2. Read tasks.md to find the next incomplete task (or the task specified by $ARGUMENTS)
3. For the selected task:
   a. Read the **Input** references (source code, samples, design.md sections)
   b. Read any existing documentation file that will be edited
   c. Write or update the documentation following CLAUDE.md guidelines
   d. Mark the task as complete in tasks.md (`- [ ]` → `- [x]`)
4. After completing the task, show:
   - What was written/updated
   - Progress summary (X of Y tasks complete)
   - Next recommended task

### Writing Guidelines

When writing documentation, follow these principles from CLAUDE.md:

- **Structure:** Title (H1) → Introduction → Key Concepts → Usage → Configuration → Best Practices → Pitfalls → Further Reading
- **Voice:** Second person ("you"), active voice, present tense
- **Code examples:** Complete and runnable, with `csharp` syntax highlighting
- **Cross-links:** Link related concepts on first mention
- **SUMMARY.md:** Update when creating new files (never create orphaned files)

### Quality Check Before Marking Complete

Before marking a task done, verify:
- Code examples have correct syntax highlighting
- Cross-links use relative paths
- No duplicate content (link to authoritative source instead)
- File follows the standard organization pattern from CLAUDE.md
- New terms are defined on first use
