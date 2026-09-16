---
allowed-tools: Bash(cat:*), Bash(echo:*), Bash(test:spec/*), Bash(grep:*), Bash(git diff:*), Bash(git add:*), Bash(python3 tools/*.py:*), Bash(dotnet run --project tools/optioncheck:*), Write, Edit, Read, Glob, Grep
description: Start writing documentation from approved tasks
argument-hint: [task-number]
---

## Context

Current spec: !`cat spec/.current-spec 2>/dev/null || echo "No active spec"`

## Your Task

1. Verify tasks are approved (a `.tasks-approved` file in the spec directory)
2. Read tasks.md for the next incomplete task, or the one named by $ARGUMENTS
3. Read the spec's **standing obligations**, if it has them — they bind every task and are
   deliberately not restated per task
4. For the selected task: read its **Input** references; read the file it will edit; write it per
   `CLAUDE.md`; run the checks below; then flip `- [ ]` → `- [x]`
5. Report what changed, the progress, and what is next. **Re-derive X of Y, never increment it:**
   `grep -c '^- \[x\] \*\*Task' <tasks.md>` against `grep -c '^- \[.\] \*\*Task' <tasks.md>`

### Structure — two shapes, decided by the page type

A `Tutorial` or a `How-to` is a sequence the reader **executes**, and uses `## Step N: …` headings.
A `Reference` or an `Explanation` is **consulted**, and uses the qualified-section pattern.
`CLAUDE.md` § *File Organization Pattern* carries both and why the split exists. Do not restate it,
and **the conversion is forbidden in both directions on an existing page**: do not tidy a
`## Step N:` page into sections, and do not requalify an existing How-to into steps. The step rule
reached how-tos on 2026-09-06 and **binds pages written after it, not the ones that predate it** —
converting one moves every published anchor on it, for no reader benefit.

Whichever shape, the page owes four things, each with a rule behind it:

- **The banner**, first non-blank line after the H1 — `CLAUDE.md` § *Page banner*. The separator is
  ` · `, not a hyphen. Missing is an error on every page
- **An opening sentence that survives being read alone**, since `/llms.txt`, the search snippet and
  the `.md` variant all print it — § *The opening sentence*
- **`description:` front matter equal to that sentence**, quoted — § *Page descriptions*
- **Every `##` heading qualified by its subject** — `## Kafka Configuration`, never
  `## Configuration` — except the allowlisted navigation headings, which `CLAUDE.md` lists

**Voice:** second person, active, present tense. **`SUMMARY.md`:** update it when you add a page;
never leave an orphan.

### Before the page names a type, a method or an option

Three states, and only the middle one needs prose:

- **Live** — it resolves in the product at the version the banner claims. Confirm it; do not infer
  it from a sample, a blog post or another page
- **Forthcoming, and the page says so** — the corpus's existing form, a blockquote opening
  `> **Not in a released package yet.**`, naming the release it ships after
- **Dead** — a defect, whether or not `symbolcheck` knows the name. Repair it, and repair what you
  find wrong beside it

`symbolcheck` only knows the names on `tools/symbolwatch.tsv`; a green run is **not** a statement
that the page is correct.

### Code blocks

Every new or edited C# block **compiles**: extract the page's blocks into a scratch project
referencing the **released packages**, not `ProjectReference`s into `src/`, and build. A tutorial is
replayed step by step, and a step that legitimately does not build says so on the page.

**Compiling is necessary and not sufficient.** Where a block asserts *behaviour* — an exception
type, an ordering, a precedence, whether an option does anything — **run it, with a control**: the
case that should behave differently. Four compiling, reviewed examples in this programme were wrong.

Mark a genuine omission `// ...`, which downgrades `pagelint` rule 6 honestly. Never use it to
avoid writing the directives.

### Quality check before marking complete

**What a tool decides** — on the paths you touched, `git add` first or `--changed` sees nothing.
`tools/README.md` has the expected numbers and the exit-code contract.

```bash
git add -A
git diff --cached --stat                           # exactly the files you meant to touch?
python3 tools/pagelint.py <paths> && python3 tools/pagelint.py --changed origin/master
python3 tools/linkcheck.py <paths>
python3 tools/symbolcheck.py <paths>
dotnet run --project tools/optioncheck -- <paths>  # only if you touched a marked option table
```

**What you decide**, because nothing here checks it:

- Is the page the type its banner claims? A mislabelled page reads perfectly and misleads everyone
- Does the example teach the thing, or only compile?
- Are the ❌/✅ version markers right? One of the **three** conventions with no rule — the other
  two are the compile and the run obligations above, and `CLAUDE.md` § *The ledger* lists all three
- Is this duplicated from a page that already owns it? Link instead
- Are new terms defined on first use, and consistent with `Glossary.md`?
