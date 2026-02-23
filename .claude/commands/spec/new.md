---
allowed-tools: Bash(mkdir:*), Bash(echo:*), Bash(date:*), Bash(ls:*)
description: Create a new documentation specification
argument-hint: <topic-name>
---

## Current Spec Status

!`ls -la spec/ 2>/dev/null | grep "^d" | wc -l | xargs -I {} echo "Total specs: {}"`

## Your Task

Create a new documentation specification for the topic: $ARGUMENTS

1. Determine the next ID number (format: 001, 002, etc.)
2. Create directory: `spec/[ID]-$ARGUMENTS/`
3. Update `spec/.current-spec` with the new spec directory name ([ID]-$ARGUMENTS)
4. Create a README.md in the new directory with:
   - Topic name and brief description
   - Creation date
   - Status checklist (see template below)
   - Next steps
5. Inform the user about next steps (run `/spec:requirements`)

### README.md Template

```markdown
# Spec [ID]: [Topic Name]

**Created:** [date]
**Status:** Requirements Phase

## Topic Overview

[Brief description of the documentation topic and why it's needed]

## Status Checklist

- [ ] Requirements gathered
- [ ] Requirements reviewed and approved
- [ ] Documentation outline created
- [ ] Outline reviewed and approved
- [ ] Writing tasks identified
- [ ] Writing complete
- [ ] Documentation reviewed
- [ ] Spec closed

## Next Steps

1. Review existing documentation in SUMMARY.md for related content
2. Identify source material (source code, ADRs, release notes, samples)
3. Identify gaps in current documentation
4. Create requirements document
5. Get requirements approved before proceeding

## Notes

- Follow CLAUDE.md guidelines for documentation standards
- Reference source code in ../Brighter and ../Darker as needed
- Ensure SUMMARY.md is updated when new files are created
```

Use the Bash tool to create directories and files as needed.
