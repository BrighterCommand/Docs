"""Task 2.14 (AC8): the S1–S5 page tables, drawn from `--classify`.

Inputs, each the output of a committed command, none of them a probe's verdicts:
  classify  `python3 tools/blockcheck.py --classify > classify.tsv`   every class column, and FAILED
  report    `python3 tools/blockcheck.py --report report.tsv`         BUILT, and the FAILED cross-check
  tranche   `python3 spec/018-compile_residual/probe/tranche_pages.py spec/017-compile_repairs/tasks.md`
            017's 75 tranche pages, which choose the ROWS, never a column
The rows are every page with a FAILED block that is off 017's tranche, plus the tranche pages whose
FAILED blocks are all on the V4 pin (`pages.tsv` pin `v4`). The section is the first `SUMMARY.md`
section that links the page. Prints the five tables in markdown, then the checks; exit 1 if a check fails.

    python3 spec/018-compile_residual/probe/groups.py classify.tsv report.tsv tranche.txt
"""
import collections, re, sys

CLASSES = ('parse', 'built-by-using', 'defect', 'same-page', 'values', 'page-type')
GROUPS = [   # design § The section groups
    ('S1', ['Scheduler']),
    ('S2', ['Commands, Handlers and Pipelines', 'Get Started']),
    ('S3', ['Darker', 'Understanding Brighter']),
    ('S4', ['Using an External Bus', 'Transports']),
    ('S5', ['Brighter Configuration', 'V10 Migration', 'Outbox and Inbox', 'Reference',
            'Health Checks and Observability']),
]
classify_path, report_path, tranche_path = sys.argv[1:4]

classes = collections.defaultdict(collections.Counter)
for line in open(classify_path, encoding='utf-8'):
    page, _, cls, _, _ = line.rstrip('\n').split('\t')
    classes[page][cls] += 1

verdict = collections.defaultdict(collections.Counter)
for line in open(report_path, encoding='utf-8'):
    v, page = line.split('\t')[:2]
    verdict[page][v] += 1

tranche = {'contents/' + p for p in open(tranche_path).read().split()}
v4 = {line.split('\t')[0] for line in open('tools/blockcheck/scaffold/pages.tsv', encoding='utf-8')
      if not line.startswith('#') and line.rstrip('\n').endswith('\tv4')}
v4_failed = collections.Counter()
for line in open(report_path, encoding='utf-8'):
    v, page = line.split('\t')[:2]
    if v == 'FAILED' and page in v4:
        v4_failed[page] += 1

section, section_of = None, {}
for line in open('SUMMARY.md', encoding='utf-8'):
    m = re.match(r'^## (.+)', line)
    if m:
        section = m.group(1).strip()
    m = re.search(r'\(/(contents/[^)#]+\.md)', line)
    if m:
        section_of.setdefault(m.group(1), section)

rows = {p for p in classes if p not in tranche}
v4_tranche = {p for p in classes if p in tranche and p in v4}
rows |= v4_tranche

failures = []
grand = collections.Counter()
placed = set()
for name, sections in GROUPS:
    pages = sorted((p for p in rows if section_of.get(p) in sections),
                   key=lambda p: (sections.index(section_of[p]), p))
    placed |= set(pages)
    print(f'\n#### {name}: {"; ".join(sections)}\n')
    print('| Section | Page | FAILED | ' + ' | '.join(CLASSES) + ' | BUILT |')
    print('|---|---|---:|' + '---:|' * len(CLASSES) + '---:|')
    total = collections.Counter()
    for p in pages:
        c = classes[p]
        failed = sum(c.values())
        built = verdict[p]['BUILT']
        mark = ' (V4)' if p in v4_tranche else ''
        print(f'| {section_of[p]} | `{p[len("contents/"):]}`{mark} | {failed} | '
              + ' | '.join(str(c[k]) if c[k] else '' for k in CLASSES) + f' | {built} |')
        total['FAILED'] += failed
        total['BUILT'] += built
        total['pages'] += 1
        total['nothing built'] += built == 0
        for k in CLASSES:
            total[k] += c[k]
    print(f'| **{name}** | **{total["pages"]} pages** | **{total["FAILED"]}** | '
          + ' | '.join(f'**{total[k]}**' for k in CLASSES) + f' | **{total["BUILT"]}** |')
    print(f'\n{name}: {total["pages"]} pages, {total["FAILED"]} FAILED, '
          f'{total["nothing built"]} with nothing BUILT')
    grand += total

print(f'\nTOTAL: {grand["pages"]} pages ({len(v4_tranche)} tranche pages on the V4 pin), '
      f'{grand["FAILED"]} FAILED, {grand["nothing built"]} with nothing BUILT; '
      + ', '.join(f'{grand[k]} {k}' for k in CLASSES))

# Checks: every row placed in exactly one group; the FAILED column is --report's FAILED on the
# same pages; on the V4 tranche pages, every FAILED block is a V4 block.
unplaced = sorted(rows - placed)
if unplaced:
    failures.append(f'pages in no group: {unplaced}')
report_failed = sum(verdict[p]['FAILED'] for p in rows)
print(f'check: FAILED column {grand["FAILED"]}, --report FAILED on the same pages {report_failed}')
if report_failed != grand['FAILED']:
    failures.append('the FAILED column does not equal --report')
for p in sorted(v4_tranche):
    if v4_failed[p] != sum(classes[p].values()):
        failures.append(f'{p}: a FAILED block that is not on the V4 pin')
for f in failures:
    print('FAIL', f)
sys.exit(1 if failures else 0)
