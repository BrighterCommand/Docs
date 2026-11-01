"""Every C# block that declares a message mapper, checked for both of its members (ledger 92).

017's recurring defect: an `IAmAMessageMapper<T>` (or `IAmAMessageMapperAsync<T>`) shown with one of its
two members, under a `// ...` that declares nothing about the other. Used by tasks 7.1, 7.3 and 7.6
(obligation 18).

Usage, from the repo root:
    python3 spec/018-compile_residual/probe/mapper_members.py [report.tsv]
Prints  page #n VERDICT Class missing Member  for each block short of a member; nothing when none is.
The optional report is `blockcheck --report`'s row file, for the verdict column.
"""
import csv, re, sys
sys.path.insert(0, 'tools')
import blockcheck as bc

verdicts = {}
if len(sys.argv) > 1:
    for r in csv.reader(open(sys.argv[1]), delimiter='\t'):
        verdicts[(r[1], int(r[2]))] = r[0]

for b in bc.enumerate_blocks():
    body = b.body if isinstance(b.body, str) else '\n'.join(b.body)
    for m in re.finditer(r'class\s+(\w+)[^{]*?:\s*[^{]*?IAmAMessageMapper(Async)?<', body):
        suffix = m.group(2) or ''
        need = ['MapToMessage' + suffix, 'MapToRequest' + suffix]
        miss = [n for n in need if not re.search(r'\b' + n + r'\s*\(', body)]
        if miss:
            print(b.rel.replace('contents/', ''), '#%d' % b.ordinal, verdicts.get((b.rel, b.ordinal), '-'),
                  m.group(1), 'missing', ','.join(miss))
