"""Task 8.3: check backwards. Every page under `contents/` that changed since a base ref, against
the set the spec allows to change.

    python3 spec/018-compile_residual/probe/backwards.py spec/018-compile_residual/tasks.md 3a79b20

The changed pages are `git diff --name-only <base> HEAD -- contents/`. Each is put in the first
bucket that holds it:
  group    one of the 64 + 3 rows of § *The groups* (the set `probe/stays.py` reads as *reached*)
  ledger   named in the *Page(s)* column of a § *Defect ledger* row (a recurrence); the rows print
  P0-10    `FAQ.md` or `V10MigrationGuide.md` (requirements § P0-10, § *Deliverables*)
  other    none of these
Every page outside *group* prints with the first-parent merges (or direct commits) on HEAD's line
that changed it, so a page a bugfix PR touched between phases is named by that PR.
Exit 0 when every *other* page was changed only by PRs outside spec 018 (a bugfix or docs PR that
landed on master between phases); 1 when a spec 018 PR or a direct commit changed one, which the
task's prose must then explain; 2 when an input cannot be read.
"""
import collections, re, subprocess, sys

if len(sys.argv) != 3:
    sys.exit('usage: backwards.py <tasks.md> <base-ref>')
tasks_path, base = sys.argv[1:3]


def git(*args):
    return subprocess.run(('git',) + args, check=True, capture_output=True, text=True).stdout


try:
    text = open(tasks_path, encoding='utf-8').read()
    groups = text[text.index('\n## The groups'):text.index('\n## Blocks that stay FAILED')]
    ledger = text[text.index('\n## Defect ledger'):text.index('\n## Friction ledger')]
    changed = [p.removeprefix('contents/') for p in
               git('diff', '--name-only', base, 'HEAD', '--', 'contents/').split()]
    line = git('log', '--first-parent', '--format=%h %s', f'{base}..HEAD').splitlines()
except (OSError, ValueError, subprocess.CalledProcessError) as e:
    print(f'backwards.py: {e}', file=sys.stderr); sys.exit(2)

# the same regex `probe/stays.py` uses for its reached pages
reached = set(re.findall(r'^\| [^*|][^|]* \| `([A-Za-z0-9]+\.md)`', groups, re.M))

rows_of = collections.defaultdict(list)
for m in re.finditer(r'^\| *(\d+) *\|(.*)$', ledger, re.M):
    cells = re.split(r'(?<!\\)\|', m.group(2))
    for page in set(re.findall(r'([A-Za-z0-9]+\.md)', cells[1])):   # cells[1] is *Page(s)*
        rows_of[page].append(int(m.group(1)))

P0_10 = {'FAQ.md', 'V10MigrationGuide.md'}

# which first-parent entry on HEAD's line changed each page
by_entry = collections.defaultdict(list)
for entry in line:
    sha, subject = entry.split(' ', 1)
    pr = re.search(r'Merge pull request (#\d+) from [^/\s]+/(\S+)', subject)
    label = f'{pr[1]} ({pr[2]})' if pr else f'{sha} (direct)'
    for p in git('diff', '--name-only', f'{sha}^1', sha, '--', 'contents/').split():
        by_entry[p.removeprefix('contents/')].append(label)

buckets = collections.Counter()
bucket_of = {}
unexplained = 0
print('page\tbucket\tledger rows\tchanged by')
for page in sorted(changed):
    if page in reached:
        bucket = 'group'
    elif page in rows_of:
        bucket = 'ledger'
    elif page in P0_10:
        bucket = 'P0-10'
    else:
        bucket = 'other'
    buckets[bucket] += 1
    bucket_of[page] = bucket
    prs = list(reversed(by_entry.get(page, [])))   # oldest first
    if bucket == 'other' and any(p.endswith('(direct)') or '(spec/018-' in p for p in prs):
        unexplained += 1
    if bucket != 'group':
        rows = ', '.join(str(r) for r in sorted(set(rows_of.get(page, [])))) or '-'
        print(f'{page}\t{bucket}\t{rows}\t{"; ".join(prs) or "?"}')

in_group_changed = len([p for p in changed if p in reached])
print(f'\n{len(changed)} changed pages since {base}: '
      + ', '.join(f'{k} {buckets[k]}' for k in ('group', 'ledger', 'P0-10', 'other')))
print(f'{len(reached)} group pages, {in_group_changed} of them changed; unchanged: '
      + (', '.join(sorted(reached - set(changed))) or 'none'))
outside = collections.Counter(l for p in changed for l in by_entry.get(p, []) if '(spec/018-' not in l)
only = [p for p in changed if all('(spec/018-' not in l for l in by_entry.get(p, []))]
print('PRs outside spec 018: ' + ', '.join(f'{l} {n} pages' for l, n in sorted(outside.items())))
print(f'{len(only)} changed pages touched by no spec 018 PR: '
      + ', '.join(f'{b} {sum(1 for p in only if bucket_of[p] == b)}' for b in ('group', 'ledger', 'P0-10', 'other')))
print(f'{unexplained} other page(s) changed by a spec 018 PR or a direct commit')
sys.exit(1 if unexplained else 0)
