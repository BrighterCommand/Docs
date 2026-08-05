#!/usr/bin/env python3
"""One-shot: insert the page banner into every page, from reviewed verdicts.

Task 3.3 of spec 011. Reads pagetypes.tsv and writes one banner line, plus one
blank line, immediately below each page's H1.

THIS SCRIPT READS THE `verdict` COLUMN, NEVER `proposed`.

That is the whole point of it. The proposal came from four mechanical signals
and is wrong often enough that shipping it unreviewed would tell readers that
105 pages are things they are not. A blank verdict is a hard stop: the script
prints the offending rows and exits non-zero WITHOUT WRITING TO A SINGLE PAGE.
It does not sweep the reviewed rows and skip the rest -- half a sweep produces a
60-file diff that looks deliberate, and the missing banners then surface as
linter errors with no sign that a human decision was skipped rather than a page
missed.

Deliberately not folded into `pagelint.py --fix`. The sweep is P0 and --fix is
P1, and a design that makes a P0 step depend on a P1 tool is wrong.

The design says to delete this after Task 3.4, on the grounds that it is used
once. It has since been used three more times -- to correct two page types, and
to retarget ten Darker version markers -- so it is kept until the verdicts are
final. Delete it then; `pagelint.py --fix` is the durable tool.

Prerequisites are never authored here, but an existing one is preserved. The
segment is optional by design, and choosing prerequisites is a per-page judgement
that cannot be made mechanically -- so the sweep stays a sweep, and prerequisites
get added as pages
are edited for other reasons.

Re-running is safe: a first line shaped like one of our banners is REPLACED, not
duplicated, so a corrected verdict can be re-applied without unpicking anything.
The test is BANNER_SHAPE_RE rather than BANNER_RE, so a banner left stale by a
vocabulary change is still recognised as ours. Any other blockquote in that
position is left alone and reported, because it is content this script did not
write and must not eat.

Usage:
    python3 spec/011-authoring_conventions/apply_banners.py --dry-run
    python3 spec/011-authoring_conventions/apply_banners.py
"""
import csv
import os
import pathlib
import sys

HERE = pathlib.Path(__file__).resolve().parent
ROOT = HERE.parents[1]
TSV = HERE / 'pagetypes.tsv'

sys.path.insert(0, str(ROOT / 'tools'))
from pagelint import (APPLIES_TO, BANNER_SHAPE_RE,  # noqa: E402  one source of truth
                      PAGE_TYPES)


def banner(verdict, applies):
    return f'> **{verdict}** · Applies to **{applies}**'


def load_rows():
    with TSV.open(encoding='utf-8') as fh:
        return list(csv.DictReader(fh, delimiter='\t'))


def validate(rows):
    """Every row must carry a verdict and an Applies-to the linter will accept."""
    problems = []
    for r in rows:
        verdict, applies = r['verdict'].strip(), r.get('applies', '').strip()
        if not verdict:
            problems.append((r['path'], 'verdict is blank -- not yet reviewed'))
        elif verdict not in PAGE_TYPES:
            problems.append((r['path'], f'verdict {verdict!r} is not one of {PAGE_TYPES}'))
        if not applies:
            problems.append((r['path'], 'applies is blank'))
        elif applies not in APPLIES_TO:
            problems.append((r['path'], f'applies {applies!r} is not one of {APPLIES_TO}'))
        if not (ROOT / r['path']).is_file():
            problems.append((r['path'], 'no such file'))
    return problems


def apply_to(path, text, line, dry_run):
    """Insert or replace the banner below the H1. Returns a status string."""
    lines = text.splitlines()
    h1 = next((i for i, l in enumerate(lines) if l.startswith('# ')), None)
    if h1 is None:
        return 'NO H1'

    nxt = next((i for i in range(h1 + 1, len(lines)) if lines[i].strip()), None)

    # BANNER_SHAPE_RE, not BANNER_RE: a banner written under a superseded
    # vocabulary is still ours to rewrite, and is precisely what needs
    # rewriting. BANNER_RE would call it foreign and refuse.
    if nxt is not None and BANNER_SHAPE_RE.match(lines[nxt].rstrip()):
        # Carry any hand-authored Prerequisites segment across. This script does
        # not author prerequisites -- that is a per-page judgement -- but it must
        # not eat them either. Until Phase 6 no page exercised the segment, so a
        # re-run was harmless; the two splits then added five pages that do, and
        # the next re-run silently deleted all five.
        existing = lines[nxt].rstrip()
        cut = existing.find(' · Prerequisites: ')
        keep = existing[cut:] if cut != -1 else ''
        if existing == line + keep:
            return 'unchanged'
        lines[nxt] = line + keep
        status = 'replaced'
    elif nxt is not None and lines[nxt].lstrip().startswith('>'):
        # A blockquote we did not write. Measured as 0 of 105 before the sweep;
        # if one appears, it is content and this script must not overwrite it.
        return 'FOREIGN BLOCKQUOTE'
    else:
        # Blank line either side, so the banner renders as its own callout and
        # does not run into the section that follows it.
        lines[h1 + 1:h1 + 1] = ['', line, '']
        # The H1 was usually already followed by a blank line; that one is now
        # surplus. Drop it rather than leaving a double blank.
        if len(lines) > h1 + 4 and lines[h1 + 4].strip() == '':
            del lines[h1 + 4]
        status = 'inserted'

    if not dry_run:
        # Preserve whether the file ended with a newline. 18 of the 105 do not,
        # and normalising them here would put 18 unrelated changes into a commit
        # whose whole claim is that it is nothing but banner insertions.
        # Tidying those is a separate change if it is worth making at all.
        out = '\n'.join(lines) + ('\n' if text.endswith('\n') else '')
        (ROOT / path).write_text(out, encoding='utf-8')
    return status


def main(argv):
    dry_run = '--dry-run' in argv
    rows = load_rows()

    problems = validate(rows)
    if problems:
        print(f'REFUSING TO WRITE: {len(problems)} row(s) not ready.\n', file=sys.stderr)
        for path, why in problems:
            print(f'  {path}: {why}', file=sys.stderr)
        print('\nNo page was modified. Fill the verdict column and re-run.',
              file=sys.stderr)
        return 1

    counts = {}
    failures = []
    for r in rows:
        line = banner(r['verdict'].strip(), r['applies'].strip())
        text = (ROOT / r['path']).read_text(encoding='utf-8')
        status = apply_to(r['path'], text, line, dry_run)
        counts[status] = counts.get(status, 0) + 1
        if status in ('NO H1', 'FOREIGN BLOCKQUOTE'):
            failures.append((r['path'], status))

    print(('DRY RUN -- nothing written\n' if dry_run else '') +
          f'{len(rows)} pages: ' +
          ', '.join(f'{n} {s}' for s, n in sorted(counts.items())))
    if failures:
        print(f'\n{len(failures)} page(s) need a human:', file=sys.stderr)
        for path, status in failures:
            print(f'  {path}: {status}', file=sys.stderr)
        return 1
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
