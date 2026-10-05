"""Per off-tranche page: FAILED, and what the second compile and the stubs make of them.
Hard = PARSE + DEFECT; reachable = BUILT by a using, BUILT by an empty stub, MEMBERS, HIDDEN."""
import collections, re, sys
W = sys.argv[1]
tr = set(open(f'{W}/tranche.txt').read().split())
st = {l.split('\t')[0]: l.split('\t')[2].strip() for l in open(f'{W}/stubs.tsv')}
pg = collections.defaultdict(collections.Counter)
for l in open(f'{W}/verdicts.tsv'):
    bid, page, v, _ = l.rstrip('\n').split('\t'); p = page.replace('contents/', '')
    if p not in tr: pg[p][v if v != 'STUB' else 'STUB:' + st[bid]] += 1
built = collections.Counter(l.split('\t')[1].replace('contents/', '') for l in open(f'{W}/r.tsv') if l.startswith('BUILT\t'))
sec, secof = None, {}
for l in open('SUMMARY.md'):
    m = re.match(r'^## (.+)', l)
    if m: sec = m.group(1).strip()
    m = re.search(r'\(/contents/([^)#]+\.md)', l)
    if m: secof.setdefault(m.group(1), sec)
print('section\tpage\tfailed\tusing\tempty_stub\tmembers\thidden\tsame_page\tparse\tdefect\thard\treachable\tbuilt_now')
for p, c in sorted(pg.items(), key=lambda x: (secof.get(x[0], '?'), x[0])):
    hard = c['PARSE'] + c['DEFECT']
    reach = c['BUILT'] + c['STUB:BUILT'] + c['STUB:MEMBERS'] + c['STUB:HIDDEN']
    print('\t'.join(map(str, [secof.get(p, '?'), p, sum(c.values()), c['BUILT'], c['STUB:BUILT'], c['STUB:MEMBERS'],
                              c['STUB:HIDDEN'], c['STUB:SAME-PAGE'], c['PARSE'], c['DEFECT'], hard, reach, built[p]])))
