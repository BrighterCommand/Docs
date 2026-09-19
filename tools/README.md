# The gates

Eight commands check this repository. This file is what they are, what each one checks, and the
number each prints when nothing is wrong — so that a command, a pull request or a person can
**cite** the expected figure instead of pasting a copy of it that starts going stale the day it
is written.

That is the whole reason this file is committed. `optioncheck` exists because a number pasted
into a page drifts from the type it describes; a number pasted into nine command files drifts the
same way, and nothing would be checking it.

> **Every number below carries the ref it was measured at. A number without a ref is not a fact.**

## Scope — what lives here, and what stays in `PROMPT.md`

Only what a command cites moved here. The board, the branch tips, the CI flake census and the
open-question log stay in `PROMPT.md`, which stays untracked. If you find yourself reaching into
`PROMPT.md` from a command file, the thing you reached for either belongs in this file or should
not be in a command.

## The eight gates at `412fd34`

`412fd34` is `master` as of 2026-09-13 — spec 014 phase 4, PR #161. Re-derive these before
quoting them; the command is in the row so that you can.

**`linkcheck` reads 165 here and read 164 at `fc77c42`, one merge earlier.** The file you are
reading is the +1: `tools/` is inside `linkcheck`'s walk, so this file entered its corpus the day
it was written. Both numbers are true at their refs, which is what the refs are for.

| # | Gate | Command | Expected at `412fd34` |
|---:|---|---|---|
| 1 | `linkcheck` | `python3 tools/linkcheck.py` | **165 files, 0 broken** |
| 2 | `pagelint` | `python3 tools/pagelint.py` | **0 errors, 757 warnings, 162 pages** |
| 3 | shape | `python3 tools/urlmap.py --check-shape` | **161 pages, 12 sections, widest 12 of 20, deepest 4 of 4** |
| 4 | redirects | `python3 tools/urlmap.py --check-redirects` | **77 entries, 7858 bytes** |
| 5 | `versioncheck` | `python3 tools/versioncheck.py` | **0 stale pins of 18, across 5 pages** |
| 6 | `optioncheck` | `dotnet run --project tools/optioncheck` | **0 mismatches across 59 tables, 519 rows** |
| 7 | `--verify` | `python3 tools/urlmap.py --verify` | **161 predicted = 161 published** |
| 8 | `symbolcheck` | `python3 tools/symbolcheck.py` | **0 findings — 5 entries, 161 pages, 1 silenced** |

**Three of the eight are not in the `check` job of `.github/workflows/docs.yml`, and each absence
is a decision rather than an oversight:**

- **7, `--verify`, is deliberately not in CI at all.** It fetches the live sitemap, and a check
  whose failure mode is *"the site was slow"* teaches people to ignore red builds. Run it by hand
  after a `SUMMARY.md` change.
- **5 and 8's `--verify-list` run in the `versions` job**, which also runs on a daily
  `schedule:`. What invalidates a pinned version — or a watchlist row — is a release in *another*
  repository, so a push-only trigger would leave it stale until somebody happened to touch the
  docs.
- **6 runs in its own `options` job** and has no schedule, because it reflects over a *pinned*
  package: nothing outside this repository can change its verdict.

### Reading a number before you trust it

Four of these gates print their **scope** before their verdict, and that line is the one to read:
`0 stale pins` out of 0 is not the same claim as `0` out of 18, and `0 findings` is not the same
claim as `0 findings, 1 silenced`. A gate that has silently degraded to checking nothing passes
every corpus ever written.

`pagelint`'s 757 is a **warning count, not an error count** — the using-directive debt, visible on
purpose so it is not forgotten and not blocking so it does not tax unrelated work. The number that
gates is `0 errors`. It moves down when a block earns its `using` lines and up when a page adds a
C# block that lacks them; treat a *rise* as something to explain, not as a failure.

## Exit codes — one contract, all of them

| Code | Means |
|---:|---|
| **0** | clean |
| **1** | something is wrong in the corpus |
| **2** | **nothing was checked** — bad arguments, an unusable watchlist, an unreachable authority |

**2 is not a pass, and no invocation may swallow it.** No `|| true`, and no burying the code in a
shell pipeline. An unreachable NuGet is an *unchecked* pin; a watchlist that failed to load is an
*unchecked* corpus. Both report exit 2 rather than exit 1, because a tool with nothing to say
about the corpus must not say the corpus is red either.

## The other modes

Each gate command above is the whole-repo form. These are the rest:

```bash
python3 tools/linkcheck.py contents/Glossary.md      # specific files; skips the orphan check
python3 tools/pagelint.py contents/Glossary.md       # specific pages
python3 tools/pagelint.py --changed origin/master    # strict on code blocks overlapping the diff
python3 tools/pagelint.py --fix                      # repair banners, fences, descriptions
python3 tools/symbolcheck.py contents/Glossary.md    # specific pages
python3 tools/symbolcheck.py --census                # open-world report, never a gate, always exit 0
python3 tools/symbolcheck.py --verify-list           # is every watchlist row still dead?
python3 tools/urlmap.py                              # print the predicted tree
python3 tools/urlmap.py --redirects OLD_SUMMARY      # the .gitbook.yaml block for moved pages
python3 tools/versioncheck.py --release-notes ../Brighter/release_notes.md
dotnet run --project tools/optioncheck -- <paths>    # just these files or directories
```

Two of them need the sibling repositories checked out beside this one:
`--census` and `--verify-list` read `../Brighter` and `../Darker`, and **refuse rather than
guess** when they are absent. A shallow clone is not enough — rows resolve at a pinned tag *and*
at `origin/master`, so `fetch-depth: 0` is load-bearing.

**They do not read the same refs, and the difference is deliberate.** `--verify-list` follows
`origin/master`, because what invalidates a watchlist row is a removal in another repository and a
gate on a daily schedule exists to notice the world moving. `--census` reads a **recorded SHA**
(`CENSUS_PINS` in `tools/symbolcheck.py`), so that a figure it printed last week can be reproduced
this week; its header prints all four refs it resolved, with the SHA beside each. Refreshing the pin
is a deliberate commit with the new count beside it. A pin the checkout cannot resolve is **exit
2 — nothing was checked**, never a silent fall back to a branch.

**`pagelint --changed` needs `git add` first.** A new file is 100% added lines, but an *untracked*
one has no diff at all, so the strict pass sees nothing and reports a green it did not earn.

**`symbolcheck` has no `--watchlist <path>` flag, and must not get one.** A gate that can be
pointed at another list can be silenced by pointing it at an empty one.

## What each gate actually checks

- **`linkcheck`** — every internal link resolves, in six fault classes: `MISSING FILE`,
  `MISSING ANCHOR`, `WRONG CASE` (macOS resolves these; GitBook does not), `LEGACY HTML`,
  `EMPTY TARGET`, and `ORPHAN` — a page under `contents/` that `SUMMARY.md` never links to.
  Orphans are reported only on a whole-repo run.
  **Its corpus is the repository, not the published tree**: it walks everything except `.git`,
  `.github`, `.claude`, `.repomix`, `spec/` and `node_modules`, which is why its file count (165)
  is higher than `pagelint`'s page count (162) and why *this file* is in it.
  It resolves a link to `CLAUDE.md` but has no index of its headings, so
  **an anchored link into `CLAUDE.md` reports `MISSING ANCHOR` even when the heading exists.**
  Cite `CLAUDE.md` sections as prose here, not as anchored links.
- **`pagelint`** — the authoring conventions, rule by rule, over `contents/` plus the root
  `README.md`. `CLAUDE.md` § *Enforcement* carries the ledger that maps every convention to its
  rule and back; do not restate it here.
- **`urlmap --check-shape`** — the navigation's shape: at least 2 pages per section, at most 20
  top-level entries, at most 4 URL segments, and no `SUMMARY.md` heading with leading whitespace.
- **`urlmap --check-redirects`** — every redirect in `.gitbook.yaml` resolves to a file that
  exists, every key is a path that no longer publishes, and every byte is printable ASCII. That
  last clause is not style: two zero-width spaces in the `structure:` key meant GitBook silently
  ignored that block for months.
- **`versioncheck`** — the package versions pinned in tutorial prose, against NuGet. A page listed
  but absent is skipped *and says so*; a page that exists and pins nothing is an error.
- **`optioncheck`** — every option table marked `<!-- optioncheck: -->` against the type it names,
  by reflection over the pinned packages in `tools/optioncheck/optioncheck.csproj`.
- **`urlmap --verify`** — the predicted URLs against the live sitemap.
- **`symbolcheck`** — no page names a symbol on `tools/symbolwatch.tsv`: a name dead in the
  product at both the pinned release and its `master`. Opt-outs are **per symbol**, written
  `<!-- symbolcheck: allow SymbolName -->`, and always print a count.

> **A green `symbolcheck` does not mean a page is correct.** The watchlist's replacement column is
> a **name**, not a **type**: `IAmAMessageScheduler` is a marker interface with no members, so a
> repair that takes the replacement verbatim can name a live type, satisfy the gate, and still not
> compile. Which type a block should name is a judgement about the code around it.

## One phase is one pull request

A spec phase is a coherent unit that merges before the next branch starts — the contract specs
009, 010, 012, 013 and 014 all ran under. What that obliges:

1. **The phase's own gates are green before the PR opens**, and the ones expected to be *unmoved*
   are checked too. A prediction of "unmoved" is exactly where a vacuous pass hides.
2. **Predict gate movement before the work, including "none"** — then reconcile against what
   actually moved, and explain any difference rather than quietly adopting the new number.
3. **A gate and the corpus that satisfies it merge together, or the gate merges second.** This
   workflow triggers `on: push`, so a gate merged over a corpus that still violates it turns
   `master` red *and* reddens its own pull request.
4. **A check that has never failed has not been shown to work.** Every new gate gets a red-proof,
   recorded with its output in the spec's `tasks.md`; every control is two-way — a known-present
   case and a known-absent one.
5. **Anything that changes the published site needs a merge sign-off**, and the head-ref deletion
   is asked for by name in the same breath. This repository does not auto-delete branches.

A phase that touches no page under `contents/` changes no published URL, and so needs no sign-off
— but it still owes 1 and 2.
