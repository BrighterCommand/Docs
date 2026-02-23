---
allowed-tools: Bash(ls:*), Bash(cat:*), Bash(grep:*), Bash(test:*), Read
description: Show all specifications and their status
---

## Gather Status Information

All specs: !`ls -d spec/*/ 2>/dev/null | sort`
Current spec: !`cat spec/.current-spec 2>/dev/null || echo "None"`

## Your Task

Present a clear status report showing:

1. **All specifications** with their IDs, names, and current phase
2. **Current active spec** (highlighted)
3. **Phase completion** for each spec:
   - Requirements: exists? approved?
   - Design: exists? approved?
   - Tasks: exists? approved?
   - Writing: task progress (count completed vs total from tasks.md)
4. **Task progress** for specs in the writing phase (X of Y tasks complete)
5. **Recommended next action** for the active spec

### Phase Detection

For each spec directory, check:
- `requirements.md` exists → requirements gathered
- `.requirements-approved` exists → requirements approved
- `design.md` exists → outline created
- `.design-approved` exists → outline approved
- `tasks.md` exists → tasks identified
- `.tasks-approved` exists → tasks approved
- Count `- [x]` vs `- [ ]` in tasks.md → writing progress

### Output Format

```
## Documentation Specs Status

| ID  | Topic                | Phase        | Progress |
|-----|----------------------|--------------|----------|
| 001 | darker_docs          | Writing      | 5/12     |
| 002 | error_handling       | Requirements | -        |

**Active:** 001-darker_docs
**Next action:** Run `/spec:implement` to continue writing
```
