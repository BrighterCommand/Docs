"""The off-tranche hard blocks, sorted by the 017 repair rule each would need. A heuristic for
sizing the design, not an instrument: P0-2's --classify is what decides. Run after offtable.py.
DEFECT diagnostics are re-read at the second compile (stageP), so a missing using cannot mask them."""
import collections, re, subprocess, sys
W = sys.argv[1]; sys.path.insert(0, 'tools'); import pagelint
DLL = 'tools/blockcheck/bin/Release/net9.0/blockcheck.dll'; REFS = 'tools/blockcheck/refs/bin/Release/net9.0/refs.txt'
tr = set(open(f'{W}/tranche.txt').read().split())
V = [l.rstrip('\n').split('\t') for l in open(f'{W}/verdicts.tsv')]
off = lambda page: page.replace('contents/', '') not in tr
defids = [b for b, p, v, _ in V if v == 'DEFECT' and off(p)]
parids = [b for b, p, v, _ in V if v == 'PARSE' and off(p)]
idx = {l.split('\t')[0]: l.rstrip('\n').split('\t')[1] for l in open(f'{W}/stage/index.tsv')}
decl = re.compile(r'\b(?:class|record|interface|struct|enum)\s+([A-Za-z_]\w*)')
pagedecl = collections.defaultdict(set)
for bid, page in idx.items(): pagedecl[page] |= set(decl.findall(open(f'{W}/stage/{bid}.cs').read()))
out = subprocess.run(['dotnet', DLL, '--explain', f'{W}/stageP', REFS] + defids, capture_output=True, text=True).stdout
diag = collections.defaultdict(list)
for l in out.splitlines():
    p = l.split('\t')
    if len(p) >= 4: diag[p[0]].append((p[1], p[3]))
rx = re.compile(r"^'([^']+)' does not contain a definition for '([^']+)'")
DUP = {'CS0128', 'CS0111', 'CS0101', 'CS0102'}
FRAG = {'CS0161', 'CS0825', 'CS0116', 'CS0106', 'CS1106', 'CS0841', 'CS1520'}
ORDER = ['forthcoming', 'pin-gap', 'two-snippets', 'collision', 'collision?', 'wrapper', 'fragment', 'api']
dc = collections.Counter()
with open(f'{W}/defcat.tsv', 'w') as f:
    for bid in defids:
        page, tags = idx[bid], set()
        for c, m in diag[bid]:
            if c in ('CS0246', 'CS0103', 'CS0234'): continue
            mm = rx.match(m)
            if mm:
                short, mem = mm.group(1).split('.')[-1].split('<')[0], mm.group(2)
                if mem == 'Replay': tags.add('forthcoming')                         # OnceOnlyAction.Replay, after 10.7.0
                elif re.match(r'Should(Be|Not)', mem): tags.add('pin-gap')           # Shouldly is not pinned
                elif short in pagedecl[page]: tags.add('collision')
                elif short in ('object', 'Context') or mem in ('Handle', 'HandleAsync', 'Bag'): tags.add('wrapper')
                elif short in ('Task', 'Tag', 'User', 'Activity', 'Span', 'Key', 'Log'): tags.add('collision?')
                else: tags.add('api')
            elif c in DUP: tags.add('two-snippets')
            elif c in FRAG: tags.add('fragment')
            else: tags.add('api')
        k = next((o for o in ORDER if o in tags), 'none'); dc[k] += 1
        print(bid, page, k, ','.join(sorted(tags)), sep='\t', file=f)
pages = pagelint.load_pages()
rid = {l.split('\t')[3]: (l.split('\t')[1], int(l.split('\t')[2])) for l in open(f'{W}/r.tsv')}
pc = collections.Counter()
with open(f'{W}/parcat.tsv', 'w') as f:
    for bid in parids:
        rel, n = rid[bid]
        b = [b for b in pages[rel].blocks if (b['info'] or '').strip().lower() in ('csharp', 'c#', 'cs')][n - 1]
        txt = '\n'.join(re.sub(r'//.*', '', t) for _, t in b['body']).strip()
        if re.match(r'^\{\s*"', txt) or re.match(r'^\s*<\w', txt) or (re.search(r'^\s*"\w+"\s*:', txt, re.M) and 'var ' not in txt):
            k = 'not-csharp'
        elif '...' in txt: k = 'literal-ellipsis'
        elif re.search(r'^\s*(public |private |internal |protected )?(sealed |static |abstract )*(class|record|interface) \w', txt, re.M) \
             and re.search(r'^(?!\s*(public|private|protected|internal|\[|\}|\{|//))\s*[a-z_]\w*(\.\w+)*\s*(\(|=)', txt, re.M):
            k = 'types-and-statements'
        elif re.match(r'^\s*\.', txt): k = 'leading-dot-chain'
        else: k = 'fragment'
        pc[k] += 1; print(bid, rel, k, sep='\t', file=f)
print(f'{len(defids)} DEFECT:', ', '.join(f'{n} {k}' for k, n in dc.most_common()))
print(f'{len(parids)} PARSE:', ', '.join(f'{n} {k}' for k, n in pc.most_common()))
