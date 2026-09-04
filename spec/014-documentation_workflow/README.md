# Spec 014: The Documentation Workflow Itself

**Created:** 2026-09-04
**Status:** **README only — requirements phase not started.** `spec/.current-spec` points at
**013**, deliberately: 013 is the next work and this is queued behind it.

## Topic Overview

The nine `/spec:*` commands in `.claude/commands/spec/` are the workflow every spec in this
programme has been run through. **They were written before specs 009 to 012 and none of them
has been updated since.** Four specs' worth of conventions, tools and method now exist that the
workflow does not know about, and in at least one place it instructs a writer to do something
`CLAUDE.md` forbids.

This spec updates the workflow to match what the programme learned. It changes **process**, not
documentation: no page under `contents/` moves.

## Why now, and what triggered it

Specs 009–012 each ended by writing its method into a committed document — 009's compile-and-run
pass, 010's split rules, 011's conventions, 012's ten standing obligations and its drift ledger.
**None of that reaches the next spec through the workflow.** It reaches it only through a person
who happens to have read the previous spec's `tasks.md`, which is exactly the transmission
failure this programme has recorded about every other kind of knowledge.

The specific trigger is the observation that **a new feature usually needs a mix of documentation
modes** — a tutorial to learn it, a how-to for the task it enables, a reference for its options,
an explanation of why it works as it does — and **no command in the workflow asks which mix a
topic needs.** `/spec:requirements` asks for scope and priorities; `/spec:design` asks for a file
outline; neither asks what *kind* of page each file is. The page-type vocabulary exists, is
enforced on every page by `pagelint.py`, and is invisible to the process that plans the pages.

## Measured, 2026-09-04, before anything is claimed

Across the **451 lines** of the nine command files:

| Term | Files mentioning it |
|---|---|
| `banner`, `Applies to`, page-type vocabulary (`Tutorial`/`How-to`/`Reference`/`Explanation`), Diátaxis | **none** |
| `pagelint`, `urlmap`, `versioncheck`, `optioncheck` | **none** |
| `linkcheck` | `review.md` only |
| Front matter / `description:` for a page | **none** |
| Compiling a page's code, `using` directives, rule 6 | **none** |
| Acceptance pass, red-proof, re-deriving a total | **none** |

*(The `description:` field does appear in all nine — it is the commands' own YAML front matter,
not a page's. Counted and discounted, because a grep that matches the wrong thing is this
programme's most-repeated defect.)*

**One of the seven gates is wired into the workflow and six are not.** `/spec:review` runs
`linkcheck.py` and reads its output well — it distinguishes pre-existing breakage from breakage
the spec caused, which is the right shape. Nothing does that for the other six.

## Known defects, not just absences

1. **`/spec:implement` contradicts `CLAUDE.md`.** It prescribes one skeleton for every page —
   *"Title (H1) → Introduction → Key Concepts → Usage → Configuration → Best Practices →
   Pitfalls → Further Reading"* — with no exception. `CLAUDE.md` says tutorial pages use
   `## Step N: …` headings **in place of** that skeleton, and says so as a convention rather
   than an exception. A writer following the command writes the wrong shape for a tutorial.
2. **`/spec:implement`'s "Structure" line omits the banner**, which is rule 1 and an error on
   every page in the corpus.
3. **Its "Quality Check Before Marking Complete" is a by-eye checklist** where five tools exist.
   Every item on it is either mechanically checkable today or is one of the genuinely editorial
   judgements — and it does not distinguish the two.
4. **`/spec:tasks`' phase template predates the phase-is-a-PR contract**, the standing-obligations
   section, red-proofs, and the *record the mismatch before fixing it* rule that produced 012's
   only evidence that the drift was real.
5. **No command mentions an acceptance pass.** Both specs that ran one found a criterion unmet
   at the close — 009's AC7 and 012's AC1 — on corpora green under every gate.

## Scope sketch — for the requirements phase to settle, not settled here

- Add a **mode-mix step** to `/spec:requirements`: for each deliverable, which of the four page
  types, and why. A feature that gets only a reference page is a decision, not a default.
- Point `/spec:design` at the page conventions: banner, qualified `##` headings, opening
  sentence, `description:` front matter, and the two kinds of `SUMMARY.md` nesting.
- Give `/spec:review` the other six gates, with the same pre-existing-versus-yours discipline
  it already applies to `linkcheck.py`.
- Give `/spec:implement` the mode-aware skeleton, the compile obligation, and rule 6's
  placement defence.
- Add an **acceptance-pass phase** — walk the criteria with evidence, and record what was found
  wrong before it was fixed.
- Decide what belongs in `CLAUDE.md` versus in a command. `CLAUDE.md` is the authority on
  conventions and the commands should cite rather than restate it; **restating is how the two
  drifted apart in the first place**, which is 012's premise pointed at ourselves.

## Out of scope, provisionally

- Rewriting `CLAUDE.md`'s conventions. They are enforced and they hold; this spec makes the
  workflow aware of them.
- Any change to a page under `contents/`.

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

1. Finish **013** first — it holds a public commitment on
   [#67](https://github.com/BrighterCommand/Docs/issues/67).
2. Run `/spec:switch 014-documentation_workflow`, then `/spec:requirements`.
3. **Read the four closed specs' `tasks.md` write-ups as the source material** — they are where
   the method actually lives: 009's *acceptance pass as executed*, 010's Phase 6 and 9 split
   rules, 011's conventions, and 012's §1 standing obligations and *Phase 11 as executed*.

## Notes

- **This spec's subject is the workflow, so it is the one spec that should be run under the
  workflow it is changing** — and any friction met while running it is evidence, not an
  annoyance. Record it.
