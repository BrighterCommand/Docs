"""Task 2.4 (AC7): join `--classify` against the design probe's verdicts.tsv, block by block.

Run from the repo root after `probe/run.sh <W>`, which writes both sides at one ref:
    python3 spec/018-compile_residual/probe/reconcile.py <W>        # exit 0; 1 if any row is unexplained

The probe's verdicts are PARSE, BUILT, DEFECT and STUB; `--classify`'s six classes fold onto them
(same-page, values and page-type are all STUB to the probe). Each disagreement is given the first
reason that the data shows:
  page evidence   the probe supplied a third-party namespace whose root no block on the page imports (D8)
  page-declared   the probe supplied the namespace of a name the page declares (D1)
  CS0308          the probe's second compile (stageP) left a CS0308 on a generic extension call,
                  which --classify reads as a missing `using` and the probe does not
  unexplained     none of these; read by hand
Then E4's 29 second-compile DEFECTs (hardcat's `wrapper` and `collision?`, first tag by its ORDER) are
cross-tabulated against `--classify`, since those are the blocks the design expected to move.
"""
import collections, re, subprocess, sys
sys.path.insert(0, 'tools')
import blockcheck as bc

W = sys.argv[1]
FOLD = {'parse': 'PARSE', 'built-by-using': 'BUILT', 'defect': 'DEFECT',
        'same-page': 'STUB', 'values': 'STUB', 'page-type': 'STUB'}
blocks = bc.enumerate_blocks()
ident = {b.ident: (b.rel, b.ordinal) for b in blocks}

ours = {}
for line in open(f'{W}/cls.tsv'):
    page, ordinal, cls, first, names = line.rstrip('\n').split('\t')
    ours[(page, int(ordinal))] = (cls, names)
probe = {}
for line in open(f'{W}/verdicts.tsv'):
    bid, page, verdict, added = line.rstrip('\n').split('\t')
    probe[ident[bid]] = (verdict, [n for n in added.split(',') if n])
if set(ours) != set(probe):
    sys.exit(f'the two sides hold different blocks: {len(ours)} against {len(probe)}')

roots, declared = collections.defaultdict(set), collections.defaultdict(set)
for b in blocks:
    for line in b.usings:
        m = bc.USING_ROOT_RE.match(line)
        if m:
            roots[b.rel].add(m.group(1))
    declared[b.rel] |= set(bc.TYPE_DECL_RE.findall(b.text))
table = collections.defaultdict(set)              # the probe's own table: run.sh drops Order from it
for line in open(f'{W}/types.tsv'):
    kind, name, ns = (line.rstrip('\n').split('\t') + ['', ''])[:3]
    if ns:
        table[name].add(ns)


def reason(key, added):
    page = key[0]
    foreign = [ns for ns in added if bc.namespace_rank(ns)[0] >= bc.FIRST_PARTY_TIERS
               and ns.split('.')[0] not in roots[page]]
    if foreign:
        return 'page evidence: ' + ', '.join(foreign)
    mine = [f'{n} ({min(table[n], key=bc.namespace_rank)})' for n in sorted(declared[page])
            if n in table and min(table[n], key=bc.namespace_rank) in added]
    if mine:
        return 'page-declared: ' + ', '.join(mine)
    bid = next(b for b, k in ident.items() if k == key)
    out = subprocess.run(['dotnet', bc.TOOL_DLL, '--explain', f'{W}/stageP', bc.REFS_LIST, bid],
                         capture_output=True, text=True).stdout
    hits = sorted({m.group(1) for line in out.splitlines() if '\tCS0308\t' in line
                   for m in [bc.NON_GENERIC_RE.search(line)] if m})
    if hits:
        return 'CS0308: ' + ', '.join(hits)
    return 'unexplained'


matrix = collections.Counter((probe[k][0], FOLD[ours[k][0]]) for k in ours)
print('probe\tclassify\tblocks')
for (p, c), n in sorted(matrix.items()):
    print(f'{p}\t{c}\t{n}')

rows = []
for key in sorted(ours):
    verdict, added = probe[key]
    cls, names = ours[key]
    if FOLD[cls] != verdict:
        rows.append((key[0].replace('contents/', ''), key[1], verdict, cls, reason(key, added)))
print(f'\n{len(rows)} disagreements')
for r in rows:
    print('\t'.join(map(str, r)))

ORDER = ['forthcoming', 'pin-gap', 'two-snippets', 'collision', 'collision?', 'wrapper', 'fragment', 'api']
e4 = collections.Counter()
for line in open(f'{W}/defcat.tsv'):
    f = line.rstrip('\n').split('\t')
    tags = f[-1].split(',')
    first = next((t for t in ORDER if t in tags), None)
    if first in ('wrapper', 'collision?'):
        e4[(first, ours[ident[f[0]]][0])] += 1
print(f'\nE4\'s second-compile DEFECTs: {sum(e4.values())}')
for (tag, cls), n in sorted(e4.items()):
    print(f'{tag}\t{cls}\t{n}')

unexplained = [r for r in rows if r[4] == 'unexplained']
print(f'{len(rows)} disagreements, {len(unexplained)} unexplained', file=sys.stderr)
sys.exit(1 if unexplained else 0)
