"""Generate `triage.md` 5 -- the record -- from the checkpoint and the rulings.

    python3 spec/015-census_triage/probe/triagetable.py --check     # the counts only
    python3 spec/015-census_triage/probe/triagetable.py > /tmp/5.md # the section

Phase 3 writes this. The record is GENERATED rather than typed, for one reason:
a table of 819 hand-copied rows is a table nobody can re-derive, and standing
obligation 1 asks for the command beside the figure. Re-run this after any
re-run of `triagerun.py` and the section rebuilds from the rows that were
actually measured.

TWO INPUTS, AND THEY ARE DELIBERATELY DIFFERENT KINDS OF THING.

  * `triage-checkpoint.jsonl` -- the machine's output. Stages 1, 2, 2.5 and the
    live query, one row per name, written by `triagerun.py`.
  * `stage3.tsv` -- the PERSON's output. One row per name the machine could not
    finish: the stage-3 ruling, its quoted evidence line, and its control.

Stage 3 is a person by construction (`triage.md` 2.4), so its verdicts cannot be
regenerated -- which is exactly why they live in a file of their own rather than
being edited into generated output. `--check` fails when the two disagree about
which names need a ruling, in BOTH directions: a survivor with no ruling is an
unfinished triage, and a ruling for a name that is not a survivor is a ruling
about a world the run does not describe.
"""
import argparse
import json
import os
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..'))
SPEC_DIR = os.path.join(ROOT, 'spec', '015-census_triage')
CHECKPOINT = os.path.join(SPEC_DIR, 'triage-checkpoint.jsonl')
RULINGS = os.path.join(SPEC_DIR, 'stage3.tsv')

NEVER = 'NEVER EXISTED'
REMOVED = 'EXISTED, REMOVED'
LIVE = 'LIVE'

# What a person may conclude at stage 3. `triage.md` 2.4: the boundary is the
# product's own public surface against a name the product's source merely
# contained.
RULINGS_ALLOWED = ('SURFACE', 'NOT SURFACE', 'INSTRUMENT')

PRODUCTS = ('brighter', 'darker')


class TableError(Exception):
    """The record would misstate the run."""


def load_rows(path):
    rows = []
    with open(path, encoding='utf-8') as fh:
        for lineno, line in enumerate(fh, 1):
            line = line.strip()
            if not line:
                continue
            try:
                rows.append(json.loads(line))
            except json.JSONDecodeError as exc:
                raise TableError(f'{path}:{lineno}: {exc}')
    names = [r['name'] for r in rows]
    if len(set(names)) != len(names):
        raise TableError('the checkpoint holds a name twice; a duplicate row '
                         'would be counted twice by AC5')
    return rows


def load_rulings(path):
    """name -> (ruling, evidence, control). Tab-separated, `#` comments."""
    out = {}
    if not os.path.isfile(path):
        return out
    with open(path, encoding='utf-8') as fh:
        for lineno, line in enumerate(fh, 1):
            if not line.strip() or line.lstrip().startswith('#'):
                continue
            parts = line.rstrip('\n').split('\t')
            if len(parts) != 4:
                raise TableError(f'{path}:{lineno}: expected 4 tab-separated '
                                 f'fields (name, ruling, evidence, control), '
                                 f'got {len(parts)}')
            name, ruling, evidence, control = (p.strip() for p in parts)
            if ruling not in RULINGS_ALLOWED:
                raise TableError(f'{path}:{lineno}: ruling {ruling!r} is not '
                                 f'one of {", ".join(RULINGS_ALLOWED)}')
            if not evidence or not control:
                raise TableError(f'{path}:{lineno}: {name} has no '
                                 f'{"evidence" if not evidence else "control"}; '
                                 f'obligation 8 asks for both on every verdict')
            out[name] = (ruling, evidence, control)
    return out


def per(row, key):
    return '/'.join(str(row[key].get(p, 0)) for p in PRODUCTS)


def needs_ruling(row):
    """Stage 3 reads everything the machine did not finish.

    NEVER EXISTED is finished: stage 1's zero is conclusive, and a stage-2 zero
    on a stage-1 survivor says the hits were substrings. Everything else is a
    provisional verdict by `triage.md` 1.1 and a person owns it.
    """
    return row['verdict'] != NEVER


def check(rows, rulings, stream=sys.stdout):
    """The counts, and the two-way ruling check.

    `stream` is stderr when a section is being generated: the checks still run
    on every generation -- a record built from an unfinished triage is the thing
    this guards against -- but their output must not land in the middle of the
    markdown they are checking.
    """
    def print(*args):                                    # noqa: A001
        stream.write(' '.join(str(a) for a in args) + '\n')

    survivors = [r for r in rows if needs_ruling(r)]
    missing = sorted(r['name'] for r in survivors if r['name'] not in rulings)
    extra = sorted(set(rulings) - {r['name'] for r in survivors})
    counts = {}
    for row in rows:
        counts[row['verdict']] = counts.get(row['verdict'], 0) + 1
    ruled = {}
    for name, (ruling, _e, _c) in rulings.items():
        ruled[ruling] = ruled.get(ruling, 0) + 1

    print(f'checkpoint rows                  : {len(rows)}')
    print(f'  {LIVE:<30} : {counts.get(LIVE, 0)}')
    print(f'  {REMOVED:<30} : {counts.get(REMOVED, 0)}')
    print(f'  {NEVER:<30} : {counts.get(NEVER, 0)}')
    print(f'  sum of the three               : {sum(counts.values())}')
    print(f'screened at stage 1 (s1 0 both)  : '
          f'{sum(1 for r in rows if not any(r["s1"].values()))}')
    print(f'stage-1 survivors                : '
          f'{sum(1 for r in rows if any(r["s1"].values()))}')
    print(f'needing a stage-3 ruling         : {len(survivors)}')
    for ruling in RULINGS_ALLOWED:
        print(f'  ruled {ruling:<24} : {ruled.get(ruling, 0)}')
    if missing:
        raise TableError(f'{len(missing)} survivor(s) have no stage-3 ruling, '
                         f'so the triage is unfinished: {", ".join(missing)}')
    if extra:
        raise TableError(f'{len(extra)} ruling(s) name something that is not a '
                         f'survivor of this run: {", ".join(extra)}')
    return 0


def evidence_block(row):
    out = []
    for product in PRODUCTS:
        for line in row['evidence'].get(product, []):
            out.append(f'{product[0].upper()}  {line}')
    return out


def emit(rows, rulings):
    """5.3's evidence, then 5.4's one-row-per-name table."""
    order = sorted(rows, key=lambda r: (-r['pages'], -r['sites'], r['name']))
    survivors = [r for r in order if needs_ruling(r)]

    print('### 5.3 The survivors, with the evidence each verdict was taken from')
    print()
    print(f'**{len(survivors)} of {len(rows)} names reached stage 2 with a '
          f'whole-identifier match in history.** Each one below carries the '
          f'evidence set the run kept, the stage-3 ruling, and the control that '
          f'ruling was checked against.')
    print()
    # The closing clause is generated from the rows rather than asserted in
    # prose: it is only true while every survivor is dead at both refs, and a
    # LIVE row would make it false in exactly the section a reader trusts most.
    all_dead = not any(any(r['live'].values()) for r in survivors)
    print('`B` and `D` are the product the line came from. **A line opening '
          '`…` was clipped to put the match in view, which drops the diff\'s '
          '`+`/`-`** — the window is centred on the match rather than on the '
          'start of the line, because the first run printed lines whose only '
          'visible occurrence was a different name. '
          + ('Nothing turns on the sign here: `live` is 0 for every name in '
             'this section, so every occurrence shown is historical whichever '
             'way the diff ran.'
             if all_dead else
             '**Read the sign carefully in this section: at least one name '
             'below is LIVE**, so an occurrence may be current rather than '
             'historical.'))
    print()
    for row in survivors:
        ruling, evidence, control = rulings[row['name']]
        print(f'#### `{row["name"]}` — {row["verdict"]} · **{ruling}**')
        print()
        print(f'{row["pages"]} page(s), {row["sites"]} site(s) · '
              f'stage 1 `{per(row, "s1")}` · stage 2 `{per(row, "s2")}` · '
              f'live `{per(row, "live")}` · '
              f'`{per(row, "dotted")}` dotted, `{per(row, "bare")}` bare '
              f'(B/D)')
        print()
        lines = evidence_block(row)
        if lines:
            print('```text')
            for line in lines[:12]:
                print(line)
            if len(lines) > 12:
                print(f'… {len(lines) - 12} more distinct line(s) in the '
                      f'checkpoint')
            print('```')
            print()
        print(f'**Ruling:** {evidence}')
        print()
        print(f'**Control:** {control}')
        print()

    print('### 5.4 The record — one row per census name')
    print()
    print('| name | pages | sites | s1 B/D | s2 B/D | live B/D | verdict | '
          'stage 3 |')
    print('|---|---:|---:|---:|---:|---:|---|---|')
    for row in order:
        ruling = rulings.get(row['name'], ('screened at stage 1, 0 history',))[0]
        if row['verdict'] == NEVER and any(row['s1'].values()):
            ruling = 'substring only at stage 2'
        elif row['verdict'] == NEVER:
            ruling = 'screened at stage 1, 0 history'
        print(f'| `{row["name"]}` | {row["pages"]} | {row["sites"]} | '
              f'{per(row, "s1")} | {per(row, "s2")} | {per(row, "live")} | '
              f'{row["verdict"]} | {ruling} |')
    return 0


def main(argv):
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument('--checkpoint', default=CHECKPOINT)
    parser.add_argument('--rulings', default=RULINGS)
    parser.add_argument('--check', action='store_true',
                        help='the counts and the two-way ruling check only')
    args = parser.parse_args(argv)

    try:
        rows = load_rows(args.checkpoint)
        rulings = load_rulings(args.rulings)
        if args.check:
            return check(rows, rulings)
        check(rows, rulings, stream=sys.stderr)
        return emit(rows, rulings)
    except TableError as exc:
        print(f'the record would misstate the run: {exc}', file=sys.stderr)
        return 2


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
