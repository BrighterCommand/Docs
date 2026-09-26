import sys, re, collections, csv, os
S = sys.argv[1]; report = sys.argv[2]
types = set(l.split('\t')[1] for l in open(f'{S}/types.tsv') if l.startswith('type\t'))
idx = {}
for row in csv.reader(open(f'{S}/stage/index.tsv'), delimiter='\t'):
    idx[row[0]] = row[1]
decl = re.compile(r'\b(?:class|record|interface|struct|enum)\s+([A-Za-z_]\w*)')
pagedecl = collections.defaultdict(set)
for bid, page in idx.items():
    pagedecl[page] |= set(decl.findall(open(f'{S}/stage/{bid}.cs').read()))
failed = [r[3] for r in csv.reader(open(report), delimiter='\t') if r[0]=='FAILED']
diags = collections.defaultdict(list)
for l in open(f'{S}/explain.txt'):
    p = l.rstrip('\n').split('\t')
    if len(p) >= 4: diags[p[0]].append((p[1], p[3]))
name_re = re.compile(r"name '([^']+)'")
out = collections.Counter(); pages = collections.defaultdict(set); rows = []
for bid in failed:
    page = idx[bid]; missing = []; other = False
    for code, msg in diags[bid]:
        m = name_re.search(msg)
        if code in ('CS0246','CS0103') and m: missing.append(m.group(1).split('<')[0])
        else: other = True
    if other or not missing: k = 'other'
    elif any(n in types for n in missing): k = 'import'
    else:
        own = pagedecl[page]
        caps = [n for n in missing if not (n[0].islower() or n[0]=='_')]
        if not caps: k = 'context:values'
        elif all(n in own for n in caps): k = 'context:same-page-type'
        else: k = 'context:page-type'
    out[k] += 1; pages[k].add(page); rows.append((k, page, bid, ','.join(sorted(set(missing)))))
for k in sorted(out): print(f'{out[k]:4d}  {k:24s} {len(pages[k])} pages')
print(sum(out.values()), 'total')
with open(f'{S}/classes.tsv','w') as f:
    for r in rows: f.write('\t'.join(r)+'\n')
