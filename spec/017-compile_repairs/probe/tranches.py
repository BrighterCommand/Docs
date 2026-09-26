"""Design probe: the tranche table. Joins pages.py's verdicts.tsv with stubs.py's
stubs.tsv by block id and counts, per page, the blocks a using or a stub reaches
and the hard ones (PARSE, DEFECT). Run after pages.py and stubs.py on one dir.
"""
import sys, collections
W = sys.argv[1]
st = {l.split('\t')[0]: l.split('\t')[2] for l in open(f'{W}/stubs.tsv')}
pg = collections.defaultdict(collections.Counter)
for l in open(f'{W}/verdicts.tsv'):
    bid, page, v, _ = l.rstrip('\n').split('\t')
    pg[page][v if v != 'STUB' else 'STUB:' + st[bid]] += 1
hard = lambda c: c['PARSE'] + c['DEFECT']
print('max_hard\tpages\tno_same_page\tusing_or_empty_stub\tstub_members_or_typed\tsame_page\thard')
for lim in (0, 1, 2):
    sel = [c for c in pg.values() if hard(c) <= lim]
    print(lim, len(sel), sum(1 for c in sel if not c['STUB:SAME-PAGE']),
          sum(c['BUILT'] + c['STUB:BUILT'] for c in sel),
          sum(c['STUB:MEMBERS'] + c['STUB:HIDDEN'] for c in sel),
          sum(c['STUB:SAME-PAGE'] for c in sel), sum(hard(c) for c in sel), sep='\t')
