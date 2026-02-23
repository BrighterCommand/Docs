---
allowed-tools: Bash(cat:*), Bash(test:spec/*), Write, Read
description: Create writing task list
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null`

## Your Task

1. Verify design is approved (look for `.design-approved` file in the spec directory)
2. If not approved, inform user to complete design review first (`/spec:review`)
3. If approved, create tasks.md with a detailed writing task list

### What tasks.md Should Contain

**Overview:**
- Total number of tasks
- Phase breakdown with goals

**Task Organization:**

Organize tasks into phases. Typical phases for documentation:

- **Phase 1: Research & Preparation** - Read source code, run samples, verify understanding
- **Phase 2: Core Documentation** - Write the essential (P0) documentation files
- **Phase 3: Supporting Documentation** - Write P1 documentation, update cross-links
- **Phase 4: Polish & Review** - Update SUMMARY.md, verify code examples, check links, final edit

**Task Format:**

Each task should follow this pattern:

```markdown
- [ ] **Task X.Y.Z:** [Action verb] [specific deliverable]
  - Input: [what to read/reference before writing]
  - Output: [what file/section is produced]
  - Notes: [any specific guidance]
```

Example:

```markdown
- [ ] **Task 2.1.1:** Write introduction and prerequisites for DarkerBasicConfiguration.md
  - Input: design.md section for this file, Darker/samples/SampleMinimalApi/
  - Output: DarkerBasicConfiguration.md lines 1-40 (H1 title, intro paragraph, prerequisites list)
  - Notes: Reference NuGet packages from Darker/src/; include .NET version requirements
```

**Task Dependencies:**
- Note which tasks must be completed before others can start
- Group independent tasks so they can be worked on in parallel

### Task Quality Checklist

Good tasks should:
- Be small enough to complete in one sitting (one section or one file)
- Have clear inputs (what to read) and outputs (what to write)
- Be ordered so that foundational content comes first
- Include a final task to update SUMMARY.md and verify all links

Use the Write tool to create the tasks.md file.
