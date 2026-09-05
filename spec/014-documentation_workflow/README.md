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

**Found while running 013's requirements phase, 2026-09-04/05 — these are measured, not
predicted, and items 6 to 8 are the ones no command's absence had been noticed before:**

6. **Neither `acceptance criteria` nor `open questions` appears anywhere in the nine commands.**
   Measured across the same 451 lines: `grep -ril 'acceptance criteri'` → **0 files**;
   `grep -ril 'open question'` → **0**. Every spec in the programme has both sections, and they
   exist only because each spec copied the last one's document. **Both criteria found unmet at a
   close were criteria the workflow never asked for.** This is sharper than defect 5: the
   workflow does not ask a spec what it will be judged by, and then has no pass to judge it in.
7. **`/spec:requirements`' Research Steps are feature-shaped.** They name ADRs, release notes,
   source code and samples — right for a spec documenting a new feature, and wrong for 013,
   whose demand lived in **GitHub discussions, issues and `FAQ.md`**. None of the three is
   mentioned by any command. Run literally, the phase would have produced an intuition-ordered
   guide list; the measured one reordered every priority and closed half the candidates.
8. **Nothing in the workflow asks whether the API a page names still exists — and this is the
   defect class with the highest blast radius.** 013's requirements phase found **ten call sites
   across five pages naming Brighter builder methods that have never existed in any released
   version** (`.ResiliencePipelines(`, `.ConfigureResiliencePipelines(`) or that died at V10
   (`.Policies(`), while the real API — `.Resilience(…)` / `.DefaultResilience()` — appears on
   **zero pages**. One of them is presented as the ✅ *"New: Polly v8"* form on the migration
   page. **Every gate is green on all ten**, because a bare method name in a fence is invisible
   to `linkcheck.py`, `pagelint.py` and `optioncheck` alike — `optioncheck` binds only what a
   marker names, and no marker names a method. `/spec:implement`'s quality checklist says
   *"code examples are correct"* and supplies no way to find out. **The instrument that works is
   the compiler**, it is 009's method, and no command mentions it.
9. **No command warns that an inherited list rots.** 013's README named six content gaps and
   **five were already closed** by Specs 010 and 012 — three of them by page splits nobody
   revisited. The re-derivation happened only because the README and `PROMPT.md` say so in prose
   that no command reads. A spec that starts by executing its own README ships work that is
   already done.

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
- Make `/spec:requirements` ask for **acceptance criteria and open questions by name**, and make
  each criterion **name its instrument or be marked as having none** — the marked ones are where
  every close-time failure has been (defect 6).
- Give `/spec:requirements` a **demand step** for specs whose subject is reader problems rather
  than a feature: discussions, issues, `FAQ.md` (defect 7). Dedupe a cross-source census **by
  thread, not by row** — a tracker that moves an issue to a discussion leaves the same asking in
  both enumerations — and **paginate before quoting a count**.
- Add an **API-liveness obligation** to `/spec:implement` and `/spec:review` (defect 8): before a
  page names a type, method or namespace, `git grep -w <name> <ref> -- src/` **with a control**,
  and compile the page's own fences. **Consider whether it should be a gate** — the corpus has ten
  live instances and nothing that can see them, which is exactly the argument that produced
  `optioncheck`. Note the counter-example before designing one: `.AddPolicies(` is dead in
  Brighter and **alive in Darker 4.1.1**, so a checker must know which product a page is about.
- Require a phase to **re-derive its own spec's README** before executing it (defect 9).
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
