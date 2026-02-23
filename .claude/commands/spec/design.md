---
allowed-tools: Bash(cat:*), Bash(test:spec/*), Bash(ls:spec/*), Write, Read, Glob, Grep
description: Create documentation outline and structure
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null`

## Your Task

1. Verify requirements are approved (look for `.requirements-approved` file in the spec directory)
2. If not approved, inform user to complete requirements review first (`/spec:review`)
3. If approved, create/update design.md with the documentation outline and structure

### What design.md Should Contain

**Documentation Structure:**
- File hierarchy showing how new and existing files relate
- Reading order for users (what to read first, what links where)

**File-by-File Outline:**
For each documentation file to be created or updated:
- File name and path
- Purpose (one sentence)
- Target length (approximate line count)
- Section outline with headings (H1, H2, H3)
- Key code examples needed (describe each, note source file/sample)
- Cross-links to other documentation files
- Glossary terms to define or reference

**SUMMARY.md Changes:**
- Exact placement in the table of contents
- Show the before/after diff of SUMMARY.md entries

**Code Examples Plan:**
- List each code example needed
- Note the source (sample project, source file, or written from scratch)
- Note whether the example is complete or abbreviated (with `// ...`)

**Style Notes:**
- Terminology decisions (especially if the topic introduces new terms)
- Any deviations from standard documentation patterns and why

### Design Quality Checklist

A good design document should:
- Be readable by a person or LLM as a standalone document
- Use simple language and concrete examples
- Make file and section structure obvious at a glance (use tree diagrams)
- Include enough detail that each file could be written independently
- Reference the requirements for traceability

Use the Write tool to create the design document.
