"""Task 8.2 (AC13): join `--report`'s FAILED rows to § *Blocks that stay FAILED*, page by page.

  python3 tools/blockcheck.py --report report.tsv
  python3 spec/018-compile_residual/probe/stays.py spec/018-compile_residual/tasks.md report.tsv

The reached pages are the 67 rows of § *The groups*' tables. A FAILED block on one of them with no
row in § *Blocks that stay FAILED*, and a row there that is not a FAILED block, are each printed.
Exit 0 only when both sides are empty; 1 otherwise; 2 when either input cannot be read.
"""
import re, sys

if len(sys.argv) != 3:
    sys.exit('usage: stays.py <tasks.md> <report.tsv>')
try:
    text = open(sys.argv[1]).read()
    report = [l.rstrip('\n').split('\t') for l in open(sys.argv[2]) if l.strip()]
    groups = text[text.index('\n## The groups'):text.index('\n## Blocks that stay FAILED')]
    stays = text[text.index('\n## Blocks that stay FAILED'):text.index('\n## Splits')]
except (OSError, ValueError) as e:
    print(f'stays.py: {e}', file=sys.stderr); sys.exit(2)

reached = set(re.findall(r'^\| [^*|][^|]* \| `([A-Za-z0-9]+\.md)`', groups, re.M))


def ordinals(cell):
    """'3, 5, 6, 14' and '12–14' (an en dash) and '1, 2' → a set of ints."""
    out = set()
    for part in cell.split(','):
        part = part.strip()
        m = re.fullmatch(r'(\d+)\s*[–-]\s*(\d+)', part)
        out |= set(range(int(m[1]), int(m[2]) + 1)) if m else {int(part)}
    return out


listed = set()
for page, cell in re.findall(r'^\| `([A-Za-z0-9]+\.md)` \| ([0-9,– -]+?) \|', stays, re.M):
    listed |= {(page, n) for n in ordinals(cell)}

failed = {(r[1].removeprefix('contents/'), int(r[2])) for r in report if r[0] == 'FAILED'}
failed_reached = {k for k in failed if k[0] in reached}

only_report = sorted(failed_reached - listed)
only_section = sorted(listed - failed_reached)
for p, n in only_report:
    print(f'FAILED, not listed\t{p}\t{n}')
for p, n in only_section:
    print(f'listed, not FAILED\t{p}\t{n}')
print(f'{len(reached)} reached pages; --report FAILED on them {len(failed_reached)}; '
      f'listed {len(listed)}; {len(only_report)} + {len(only_section)} rows either side',
      file=sys.stderr)
sys.exit(1 if only_report or only_section else 0)
