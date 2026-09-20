"""Probe: classify every C# block and emit it into one project, namespace-isolated.

Deliberately gives each block NO page-context help — no scaffold, no prelude.
The number it produces is therefore a FLOOR on what builds, which is the honest
direction for a design decision about how big the repair job is.
"""
import os, re, sys, json
sys.path.insert(0, '/Users/ian_hammond_cooper/CSharpProjects/github/BrighterCommand/Docs/tools')
import pagelint as pl

OUT = sys.argv[1]
DECL = re.compile(r'^\s*(?:\[.*\]\s*)?(?:public |internal |sealed |abstract |static |partial |record |)*'
                  r'(class|record|interface|struct|enum)\s+[A-Za-z_]')
MEMBER = re.compile(r'^\s*(?:\[.*\]\s*)?(?:public|private|protected|internal|static|async|override|virtual)\s')
USING = re.compile(r'^\s*using\s+[A-Za-z_][\w.]*\s*;\s*$')
NS = re.compile(r'^\s*namespace\s')

rows = []
for rel, p in sorted(pl.load_pages().items()):
    n = 0
    for b in p.blocks:
        if (b['info'] or '').strip().lower() not in ('csharp', 'c#', 'cs'):
            continue
        n += 1
        body = [t for _, t in b['body']]
        ident = re.sub(r'\W', '_', rel.replace('contents/', '').replace('.md', '')) + f'_{n}'
        usings = [l for l in body if USING.match(l)]
        rest = [l for l in body if not USING.match(l)]
        has_type = any(DECL.match(l) for l in rest)
        has_ns = any(NS.match(l) for l in rest)
        if has_ns:
            shape = 'namespaced'          # already carries its own namespace
            text = '\n'.join(usings + rest)
        elif has_type:
            shape = 'types'
            text = '\n'.join(usings) + f'\nnamespace B_{ident} {{\n' + '\n'.join(rest) + '\n}\n'
        elif any(MEMBER.match(l) for l in rest):
            shape = 'members'
            text = ('\n'.join(usings) + f'\nnamespace B_{ident} {{\npublic class Holder {{\n'
                    + '\n'.join(rest) + '\n}\n}\n')
        else:
            shape = 'statements'
            text = ('\n'.join(usings) + f'\nnamespace B_{ident} {{\npublic class Holder {{\n'
                    '  public async System.Threading.Tasks.Task Run() {\n'
                    + '\n'.join(rest) + '\n  }\n}\n}\n')
        open(os.path.join(OUT, ident + '.cs'), 'w', encoding='utf-8').write(text)
        rows.append({'id': ident, 'page': rel, 'index': n, 'shape': shape,
                     'lines': len(body), 'declares_omission': any('// ...' in l for l in body)})

json.dump(rows, open(os.path.join(os.path.dirname(OUT), 'blocks.json'), 'w'), indent=0)
from collections import Counter
print(len(rows), 'blocks emitted')
for k, v in Counter(r['shape'] for r in rows).most_common():
    print(f'  {v:5d}  {k}')
