"""Design probe: after pages.py, give every STUB block mechanical stubs -- an
empty class per missing type (with its generic arity), a `dynamic` per missing
value -- and recompile. What surfaces is either a member the real stub must
carry (the error names a stubbed type) or a defect the missing name was hiding.
Names another block on the same page declares are NOT stubbed: that would be
016's cross-block leak. Scratch only.
"""
import sys, os, re, collections, subprocess, shutil, csv
W = sys.argv[1]
DLL = 'tools/blockcheck/bin/Release/net9.0/blockcheck.dll'
REFS = os.environ.get('REFS', 'tools/blockcheck/refs/bin/Release/net9.0/refs.txt')
idx = {r[0]: r[1] for r in csv.reader(open(f'{W}/stage/index.tsv'), delimiter='\t')}
decl = re.compile(r'\b(?:class|record|interface|struct|enum)\s+([A-Za-z_]\w*)')
pagedecl = collections.defaultdict(lambda: collections.defaultdict(set))
for bid, page in idx.items():
    for n in decl.findall(open(f'{W}/stage/{bid}.cs').read()): pagedecl[page][n].add(bid)
stub = [l.split('\t')[0] for l in open(f'{W}/verdicts.tsv') if l.split('\t')[2] == 'STUB']
def explain(d, ids):
    out = subprocess.run(['dotnet', DLL, '--explain', d, REFS] + ids, capture_output=True, text=True).stdout
    r = collections.defaultdict(list)
    for l in out.splitlines():
        p = l.split('\t')
        if len(p) >= 4: r[p[0]].append((p[1], p[3]))
    return r
before = explain(f'{W}/stageP', stub)
shutil.rmtree(f'{W}/stageS', ignore_errors=True); shutil.copytree(f'{W}/stageP', f'{W}/stageS')
nm = re.compile(r"name '([A-Za-z_]\w*)(<([^>]*)>)?'")
same, stubbed = {}, {}
for bid in stub:
    types, values, own = {}, set(), set()
    for c, m in before[bid]:
        x = nm.search(m)
        if not x: continue
        n = x.group(1)
        if any(b != bid for b in pagedecl[idx[bid]].get(n, ())): own.add(n); continue
        if n[0].islower() or n[0] == '_': values.add(n)
        else: types[n] = (x.group(3) or '').count(',') + 1 if x.group(2) else 0
    same[bid], stubbed[bid] = own, set(types) | values
    extra = []
    for n, a in types.items():
        g = '' if not a else '<' + ','.join(f'T{i}' for i in range(a)) + '>'
        extra.append(f'public class {n}{g} {{ }}')
    if values:
        extra.append(f'public static class ProbeStubs_{bid} {{ ' +
                     ' '.join(f'public static dynamic {v};' for v in sorted(values)) + ' }')
    p = f'{W}/stageS/{bid}.cs'; t = open(p).read()
    if values: t = f'using static ProbeStubs_{bid};\n' + t
    open(p, 'w').write(t + '\n' + '\n'.join(extra) + '\n')
after = explain(f'{W}/stageS', stub)
out = collections.Counter(); why = collections.Counter()
with open(f'{W}/stubs.tsv', 'w') as f:
    for bid in stub:
        if same[bid]: k = 'SAME-PAGE'
        elif not after[bid]: k = 'BUILT'
        elif all(any(s in m for s in stubbed[bid]) for c, m in after[bid]): k = 'MEMBERS'
        else:
            k = 'HIDDEN'
            for c, m in after[bid]:
                if not any(s in m for s in stubbed[bid]): why[c] += 1
        out[k] += 1
        f.write(f'{bid}\t{idx[bid]}\t{k}\t{",".join(sorted(same[bid] or stubbed[bid]))}\n')
print(len(stub), 'STUB blocks:', dict(out))
print('HIDDEN diagnostics:', why.most_common(8))
