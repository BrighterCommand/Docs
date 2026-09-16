---
allowed-tools: Bash(ls:*), Bash(cat:*), Bash(echo:*), Bash(grep:*), Bash(test:*), Read
description: Show all specifications and their status
---

## Gather Status Information

All specs: !`ls -d spec/*/ 2>/dev/null`
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
- `.accepted` exists → **acceptance walked and approved; the spec is closed**

**Report `.accepted` explicitly, including when it is absent from a spec at 100%.** It is the only
marker no other command gates on, so nothing forces it to be written — and a spec sitting at N/N
with no `.accepted` either never had its acceptance walk or had one nobody recorded. Those are
different states and this is the only place the difference shows.

### Output Format

```
## Documentation Specs Status

| ID  | Topic                | Phase        | Progress | Closed |
|-----|----------------------|--------------|----------|--------|
| 001 | darker_docs          | Writing      | 5/12     | —      |
| 002 | error_handling       | Requirements | -        | —      |
| 003 | some_closed_spec     | Accepted     | 14/14    | ✅     |

**Active:** 001-darker_docs
**Next action:** Run `/spec:implement` to continue writing
```
