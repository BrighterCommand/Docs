"""Design probe: give EVERY failing block its using directives, then sort what is
left per block and per page. Scratch only; reads and writes under the work dir.

  BUILT        builds once given usings
  STUB         fails only on CS0246/CS0103 for names no pinned package ships
  PARSE        the block does not parse as staged (a fragment, a signature)
  DEFECT       a binder error that is not a missing name: API the page gets wrong
"""
import sys, os, collections, re, subprocess, shutil, csv
W = sys.argv[1]; report = sys.argv[2]
DLL = 'tools/blockcheck/bin/Release/net9.0/blockcheck.dll'
REFS = os.environ.get('REFS', 'tools/blockcheck/refs/bin/Release/net9.0/refs.txt')
ns = collections.defaultdict(set); ext = collections.defaultdict(set)
for l in open(f'{W}/types.tsv'):
    k, n, s = l.rstrip('\n').split('\t')
    if s: (ns if k == 'type' else ext)[n].add(s)
rank = lambda s: (0 if s.startswith('Paramore') else 1 if s.startswith('System') else
                  2 if s.startswith('Microsoft') else 3, len(s), s)
idx = {r[0]: r[1] for r in csv.reader(open(f'{W}/stage/index.tsv'), delimiter='\t')}
rows = list(csv.reader(open(report), delimiter='\t'))
failed = [r[3] for r in rows if r[0] == 'FAILED']
built0 = collections.Counter(r[1] for r in rows if r[0] == 'BUILT')
skip0 = collections.Counter(r[1] for r in rows if r[0] == 'SKIPPED')
shutil.rmtree(f'{W}/stageP', ignore_errors=True); shutil.copytree(f'{W}/stage', f'{W}/stageP')
parse = {l.split('\t')[0] for l in subprocess.run(['dotnet', DLL, '--parse', f'{W}/stageP'],
         capture_output=True, text=True).stdout.splitlines() if '\tBROKEN\t' in l}
added = collections.defaultdict(set); name_re = re.compile(r"name '([^']+)'")
def explain():
    out = subprocess.run(['dotnet', DLL, '--explain', f'{W}/stageP', REFS] + failed,
                         capture_output=True, text=True).stdout
    d = collections.defaultdict(list)
    for l in out.splitlines():
        p = l.split('\t')
        if len(p) >= 4: d[p[0]].append((p[1], p[3]))
    return d
for rnd in range(4):
    d = explain(); changed = 0
    for bid in failed:
        miss = {name_re.search(m).group(1).split('<')[0] for c, m in d[bid]
                if c in ('CS0246', 'CS0103') and name_re.search(m)}
        exts = {x.group(1) for c, m in d[bid] if c == 'CS1061'
                for x in [re.search(r"definition for '([^']+)'", m)] if x}
        new = ({min(ns[n], key=rank) for n in miss if n in ns} |
               {min(ext[n], key=rank) for n in exts if n in ext}) - added[bid]
        if new:
            added[bid] |= new; changed += 1
            body = open(f'{W}/stage/{bid}.cs').read()
            open(f'{W}/stageP/{bid}.cs', 'w').write(''.join(f'using {u};\n' for u in sorted(added[bid])) + body)
    if not changed: break
d = explain(); verdict = {}
for bid in failed:
    ds = d[bid]
    if not ds: verdict[bid] = 'BUILT'
    elif bid in parse: verdict[bid] = 'PARSE'
    elif all(c in ('CS0246', 'CS0103') for c, _ in ds): verdict[bid] = 'STUB'
    else: verdict[bid] = 'DEFECT'
tot = collections.Counter(verdict.values())
print('blocks:', dict(tot), '| usings added to', sum(1 for b in added if added[b]))
page = collections.defaultdict(collections.Counter)
for bid, v in verdict.items(): page[idx[bid]][v] += 1
with open(f'{W}/pages.tsv', 'w') as f:
    for p in sorted(page):
        c = page[p]
        f.write(f"{p}\t{built0[p]}\t{skip0[p]}\t{c['BUILT']}\t{c['STUB']}\t{c['PARSE']}\t{c['DEFECT']}\n")
with open(f'{W}/verdicts.tsv', 'w') as f:
    for bid, v in sorted(verdict.items()):
        f.write(f"{bid}\t{idx[bid]}\t{v}\t{','.join(sorted(added[bid]))}\n")
shape = collections.Counter()
for p, c in page.items():
    shape['A: usings alone take it whole' if not (c['STUB'] or c['PARSE'] or c['DEFECT']) else
          'B: usings + stubs' if not (c['PARSE'] or c['DEFECT']) else
          'C: has parse or defect blocks'] += 1
for k in sorted(shape): print(f'{shape[k]:4d} pages  {k}')
