---
allowed-tools: Bash(cat:*), Bash(test:*), Bash(touch:*), Bash(ls:spec/*), Write, Read, Glob, Grep
description: Create or review requirements specification
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null || echo "No active spec"`

## Your Task

For the current active specification:

1. Check if requirements.md exists in the spec directory
2. If it does NOT exist, create a comprehensive requirements.md covering:
   - **Topic overview** - What this documentation covers and why it's needed
   - **Current state** - What documentation exists today, what's missing or incomplete
   - **Target state** - What the documentation should look like when done
   - **Target audience** - Who will read this (beginners, intermediate, advanced)
   - **Source material** - ADRs, source code, release notes, samples, and READMEs to draw from
   - **Scope** - What topics to cover (with priorities P0, P1, P2)
   - **Out of scope** - What topics are explicitly excluded
   - **Documentation deliverables** - Specific files to create or update, with descriptions
   - **SUMMARY.md changes** - Where new files should be placed in the table of contents
   - **Constraints** - Style guidelines, terminology, cross-linking requirements
3. If it DOES exist, display current content and suggest improvements
4. Remind user to use `/spec:review` when ready for approval

### Research Steps

Before writing requirements:
- Read SUMMARY.md to understand existing documentation structure
- Check for existing documentation on this topic in `contents/`
- Look for relevant ADRs in `../Brighter/docs/adr/`
- Check release notes in `../Brighter/release_notes.md`
- Review source code and samples for the feature
- Check BasicConcepts.md for existing glossary terms

### Requirements Quality Checklist

A good requirements document should:
- Be readable by a person or LLM without prior context
- Use simple, direct language
- Include examples to clarify scope (e.g., "cover retry policies, for example using Polly")
- Clearly distinguish P0 (must have), P1 (should have), P2 (nice to have)
- Reference specific source files and samples where relevant

Use the Write tool to create/update the requirements.md file.
