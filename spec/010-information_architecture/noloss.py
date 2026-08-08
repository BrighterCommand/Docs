#!/usr/bin/env python3
"""Prove a split lost no information: D5 of spec 010.

Every split in phases 4-9 moves text out of a page and into new ones, under
`worklist.md` §4 rule 4 — *move the text; do not improve it*. This checks that
mechanically. **"I read the diff" is not this check**, and that is the whole
point of the file: a 900-line page split three ways produces a diff nobody can
hold in their head, and the one dropped paragraph looks exactly like the
several hundred deliberately relocated ones.

The method is the one both Spec 011 demonstrator splits used: take every
substantive line of the original **at the ref before the split**, and test it
for verbatim presence anywhere across the resulting pages. Report the ones that
survive nowhere.

    python3 spec/010-information_architecture/noloss.py --ref REF \\
        --original contents/Page.md \\
        --result contents/Page.md contents/NewPageA.md contents/NewPageB.md

`--original` may be given more than once, and so may the paths after
`--result`. That is not a convenience: **Task 3.3 and Task 4.3 fold material
rather than move it**, so their check is the union of the pages on each side.
Running such a fold page-by-page reports every folded line as lost, which is
noise that trains you to skim the output.

A page is usually on both sides — the core keeps its filename (rule 1), so it
is the original at REF and one of the results in the working tree.

`--result-ref REF` reads the results at a ref instead of the working tree. That
is what lets the tool be pointed at a split that already landed, which is how
this one was calibrated — and the calibration produced a finding.

Spec 011 recorded **24 lines and 4 lines** for its two demonstrators. This tool
returns **46 and 11**, and reproduces both recorded figures exactly as subsets:
the 23 folded `Best Practices` items plus the one repointed anchor on the
first, and the three stranded links plus the fold's lead-in on the second. The
recorded numbers were the **prose** lines. What they did not count:

  * **20 requalified headings** on the first split. A heading moving to a new
    page is requalified under rule 3a, which changes its slug — so these are
    precisely the anchor breaks standing obligation 3 exists to catch, and the
    one class of line you least want a D5 check to be quiet about.
  * **2 lines merged into surviving sentences**, and on the second split **4
    structural labels dropped in the fold plus 3 lines of the two
    `Quick Migration Guide` blocks** — dropped as duplicates of
    `V10MigrationGuide.md`'s richer before/after, and visible in 011's own
    debt figure falling 804 → 802.

So nothing here contradicts 011: every extra line is documented somewhere in
its write-up. It was documented somewhere *other than the D5 count*, which is
the point. **A figure that was never wrong can still not be the whole answer.**

**A clean run is not zero.** On the two demonstrators it returns 46 lines and
11 lines, and every one is a deliberate edit: an anchor repointed in the same
pass that renamed its target, four `Best Practices` sections folded into the
sections they concerned, three links whose "as discussed above" no longer had
an above. So the output is *read*, line by line, and every line accounted for.
The exit code is a prompt to read it, not a verdict:

    0   nothing survives nowhere — unusual, and not the target
    1   lines to account for; read them
    2   bad arguments, or a path missing at the ref or in the working tree

This tool is deliberately **not** in CI. It needs a ref, a split and a human
who can say which removals were meant.

What counts as substantive, and why the definition is narrow:

  * A line is substantive if it holds at least one alphanumeric character.
    Blank lines, `---`, `|---|---|`, a lone `}` or `});` are skipped — they
    carry nothing traceable, they recur identically all over the corpus, and a
    check they can satisfy is a check that reports success for the wrong
    reason. The count of what was skipped is printed, so the exclusion is
    visible rather than assumed.
  * Comparison is on the line's stripped text. Re-indenting a list item as it
    moves under a new heading is not information loss; changing its words is.
  * Presence is set membership, not multiplicity. A line occurring three times
    in the original and once across the results is **not** reported. Counting
    multiplicity would fire on every repeated `services.AddBrighter()` in the
    corpus and drown the signal this exists to carry.
  * **A heading is compared by the anchor it produces**, via linkcheck.py's own
    slug(). A section that becomes a page promotes its headings one level and
    may shed their bold, and slug() ignores both — so the anchor survives, no
    inbound link breaks, and nothing was lost. Requalifying a heading under
    rule 3a *does* change the slug, and that is reported, because it is the
    change that breaks `#automatic-event-logging` in some other page.

    That is not a softening to make runs pass. It is the equivalence relation
    the rest of this repo's tooling already uses — pagelint.py imports the same
    function for the same reason — so what this check calls "the same heading"
    is exactly what linkcheck.py calls the same anchor. Measured on Spec 011's
    first demonstrator it accounts for 7 of 53 reported lines; the other 20
    heading lines are genuine requalifications and are still reported.

Where a missing line has a near neighbour among the surviving lines, that
neighbour is printed beneath it. A requalified heading, a repointed link and a
deleted paragraph all read the same in a bare list; the neighbour is what tells
them apart at a glance. It is a hint for triage and nothing is inferred from
it — a line with a 0.99 neighbour is still a line you have to account for.
"""

import difflib
import re
import subprocess
import sys
from pathlib import Path

# spec/010-information_architecture/noloss.py, so the repo root is two levels up.
# urlmap.py moved and carried this constant wrong for a commit: nothing errors,
# every path simply resolves against the wrong tree.
REPO = Path(__file__).resolve().parents[2]

# Imported rather than reimplemented, exactly as pagelint.py does: a private copy
# of slug() would drift, and then this check and the link checker would disagree
# about whether an anchor moved.
sys.path.insert(0, str(REPO / 'tools'))
from linkcheck import slug  # noqa: E402

HEADING_RE = re.compile(r'^(#{1,6})\s+(.*\S)\s*$')

# How alike two lines must be before the second is offered as a neighbour.
# High on purpose: a weak match is worse than none, because it invites the
# reading "close enough, it moved" on a line that was deleted.
NEIGHBOUR_RATIO = 0.75


def substantive(text):
    """Is this line worth tracking? See the docstring's definition."""
    return any(c.isalnum() for c in text)


def read_at_ref(ref, rel):
    """The file's content at a git ref, or None if it is not there."""
    result = subprocess.run(['git', 'show', f'{ref}:{rel}'],
                            cwd=REPO, capture_output=True, text=True)
    return None if result.returncode else result.stdout


def read_worktree(rel):
    path = REPO / rel
    return path.read_text() if path.is_file() else None


def lines_of(text):
    """(line number, stripped text) for every substantive line.

    splitlines(), never split("\\n") — the latter counts a phantom empty final
    line on the 93 pages that end in a newline, and wc -l under-reports the 17
    that do not. Design §16 finding 1; it put five figures wrong across this
    programme before anyone noticed.
    """
    out = []
    for number, raw in enumerate(text.splitlines(), start=1):
        stripped = raw.strip()
        if substantive(stripped):
            out.append((number, stripped))
    return out


def main(argv):
    ref = None
    result_ref = None
    originals = []
    results = []
    bucket = None

    args = list(argv[1:])
    while args:
        arg = args.pop(0)
        if arg in ('--ref', '--result-ref'):
            if not args:
                print(f'{arg} needs a git ref, e.g. {arg} origin/master',
                      file=sys.stderr)
                return 2
            if arg == '--ref':
                ref = args.pop(0)
            else:
                result_ref = args.pop(0)
            bucket = None
        elif arg == '--original':
            bucket = originals
        elif arg == '--result':
            bucket = results
        elif arg.startswith('-'):
            print(f'unknown option: {arg}', file=sys.stderr)
            return 2
        elif bucket is None:
            print(f'{arg}: give paths after --original or --result',
                  file=sys.stderr)
            return 2
        else:
            bucket.append(arg)

    if not ref or not originals or not results:
        print(__doc__, file=sys.stderr)
        return 2

    # Paths are repository-relative, like every other path in this repo's tools
    # and in .gitbook.yaml, but accept an absolute or ./-prefixed one and
    # normalise it rather than failing on a shell completion.
    def relative(raw):
        return str(Path(raw).resolve().relative_to(REPO)) \
            if Path(raw).is_absolute() else str(Path(raw))

    originals = [relative(p) for p in originals]
    results = [relative(p) for p in results]

    # The ref is taken literally: `git show REF:path`, no merge-base. When
    # master has moved on since the branch started, pass the branch point —
    # `--ref $(git merge-base origin/master HEAD)` — rather than trusting this
    # tool to guess which of the two you meant.
    original_text = {}
    for rel in originals:
        text = read_at_ref(ref, rel)
        if text is None:
            print(f'{rel}: not present at {ref}', file=sys.stderr)
            return 2
        original_text[rel] = text

    surviving = set()
    surviving_anchors = set()
    where = f'at {result_ref}' if result_ref else 'in the working tree'
    for rel in results:
        text = read_at_ref(result_ref, rel) if result_ref else read_worktree(rel)
        if text is None:
            print(f'{rel}: not {where}', file=sys.stderr)
            return 2
        for _, stripped in lines_of(text):
            surviving.add(stripped)
            heading = HEADING_RE.match(stripped)
            if heading:
                surviving_anchors.add(slug(heading.group(2)))

    tracked = 0
    skipped = 0
    relevelled = 0
    missing = []
    for rel in originals:
        text = original_text[rel]
        skipped += sum(1 for raw in text.splitlines()
                       if not substantive(raw.strip()))
        for number, stripped in lines_of(text):
            tracked += 1
            if stripped in surviving:
                continue
            heading = HEADING_RE.match(stripped)
            if heading and slug(heading.group(2)) in surviving_anchors:
                # Same anchor, different level or emphasis: the section became a
                # page, or stopped being bold. No link breaks and nothing is lost.
                relevelled += 1
                continue
            missing.append((rel, number, stripped))

    candidates = sorted(surviving)
    for rel, number, stripped in missing:
        print(f'{rel}:{number}: {stripped}')
        near = difflib.get_close_matches(stripped, candidates, n=1,
                                         cutoff=NEIGHBOUR_RATIO)
        if near:
            ratio = difflib.SequenceMatcher(None, stripped, near[0]).ratio()
            print(f'    ~ nearest surviving line ({ratio:.2f}): {near[0]}')

    print(f'\n{len(missing)} of {tracked} substantive lines survive nowhere, '
          f'across {len(originals)} original(s) at {ref} and {len(results)} '
          f'result(s) {where} ({skipped} lines skipped as non-substantive, '
          f'{relevelled} headings matched on their anchor alone).')
    if missing:
        print('Account for every line above. A deliberate edit belongs in that '
              'list; anything you cannot name is the defect this check exists '
              'to find.')
    return 1 if missing else 0


if __name__ == '__main__':
    sys.exit(main(sys.argv))
