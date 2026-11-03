"""Task 8.4: complete the defect ledger. Every row's recurrence grep, re-run at the commit before its
phase's repair, at 3a79b20, and at the tree, against the row's *Before* and *After* cells.

    python3 spec/018-compile_residual/probe/ledger.py spec/018-compile_residual/tasks.md [--tree DIR] [-v]

Reads § *Defect ledger*'s table. A row's commands are the code spans in its *Recurrence grep* cell
that begin `grep` or `git grep`, with the markdown table escape `\\|` rendered as `|` (as GitHub
shows the cell). A span ends at the first backtick outside single quotes, so a pattern quoting a
code span (`'Always `internal_bus` on pump spans'`) is read whole. Three rows cannot be read that
way and carry an override, printed beside the row: 48 (its `\\|` is a literal pipe in ERE, not a
table escape), 51 (*"`grep -LiE causationid` on the six pages"*: the six are its *Page(s)* cell),
92 (a scan, not a grep: `probe/mapper_members.py`, which takes the tree it runs in).

Each command runs with `bash -c` (so `grep` is the system's, not a shell function), its working
directory a copy of `contents/` and `tools/` at the ref (`git archive`), or the tree itself for
`HEAD`. `<ref>` in a command (row 8, a `git grep`) is replaced by the ref; at the tree, by `HEAD`.
A command's count is its output when that is one integer (`grep -c`, `| wc -l`), else its output's
non-empty lines.

Refs. *pre*: the first parent of the merge of the row's phase PR, the commit before the repair.
*base*: 3a79b20, before spec 018. *tree*: the working tree, or `--tree DIR` (a red-proof copy).
Per row it prints: commands; counts at pre, base, tree; the bold figures of *Before* and *After*,
in cell order; whether *Found by* is present; and flags:
  BEFORE    a pre count differs from the Before cell's figure in the same position
  AFTER     a tree count differs from the After cell's figure in the same position
  NONZERO   a tree count is not 0 (the row's prose must say why it is not a recurrence)
  CELL      a cell is empty, or has fewer bold figures than the row has commands
  BASE      a count at 3a79b20 differs from the count at pre (informational)
A BEFORE row also prints a walk of its phase branch's commits (`git rev-list --first-parent` of the
merge's second parent), the counts where they change, and the last commit reading the Before cell:
the commit before the repair. Rows 6, 8 and 85 also print a shape check at the tree, by block, from
`blockcheck.enumerate_blocks()`, because their grep as written no longer reads the defect (6: the
declaration wraps; 8: every single-type lambda is a hit; 85: `-A1` counts two lines a hit).
For those three the shape's count, not the grep's, is set against the cells (6: classes declared;
8: lambdas returning two factory types; 85: unconstrained bases), and row 84's first grep is read
less the one log line its cell names (`OFFSET`). Every other row is its grep, as written.
Exit 0 when no row is flagged AFTER or CELL; 1 when one is; 2 when an input cannot be read.

    python3 spec/018-compile_residual/probe/ledger.py <tasks.md> --hits N[,N…] --at DIR --classify TSV

names each hit of those rows' greps, run in DIR (a whole tree at a ref, `git archive <ref>`, with its
blockcheck built), by page and block (obligation 23), with the block's `--classify` class from TSV
(that tree's `blockcheck.py --classify` output), or *BUILT/not C#* when TSV has no row for it. This
is how a *Found by* cell naming only a task is resolved to its instrument.
"""
import os, re, subprocess, sys, tarfile, tempfile, io

PHASE_MERGE = {3: '5f2294c', 4: '0fc79a3', 5: 'f4d965f', 6: 'dc09559', 7: 'b7e878c'}   # first-parent, master
PHASE_PRE = {n: m + '^1' for n, m in PHASE_MERGE.items()}
BASE = '3a79b20'
ROOT = os.getcwd()

args = sys.argv[1:]
verbose = '-v' in args
args = [a for a in args if a != '-v']
tree = None
if '--tree' in args:
    i = args.index('--tree')
    tree = os.path.abspath(args[i + 1])
    del args[i:i + 2]
hits = at = cls_tsv = None
for opt in ('--hits', '--at', '--classify'):
    if opt in args:
        i = args.index(opt)
        val = args[i + 1]
        del args[i:i + 2]
        if opt == '--hits':
            hits = [int(x) for x in val.split(',')]
        elif opt == '--at':
            at = os.path.abspath(val)
        else:
            cls_tsv = os.path.abspath(val)
if len(args) != 1:
    sys.exit('usage: ledger.py <tasks.md> [--tree DIR] [-v]')
try:
    text = open(args[0], encoding='utf-8').read()
except OSError as e:
    print(e, file=sys.stderr)
    sys.exit(2)

m = re.search(r'^## Defect ledger\n(.*?)(?=^## )', text, re.S | re.M)
if not m:
    print('no § Defect ledger', file=sys.stderr)
    sys.exit(2)


def cells(line):
    parts = re.split(r'(?<!\\)\|', line.strip())
    return [p.strip() for p in parts[1:-1]]


rows = []
for line in m.group(1).splitlines():
    if re.match(r'^\| *\d+ *\|', line):
        c = cells(line)
        if len(c) != 8:
            print('row with %d cells: %s' % (len(c), line[:60]), file=sys.stderr)
            sys.exit(2)
        rows.append(dict(n=int(c[0]), defect=c[1], pages=c[2], grep=c[3], before=c[4], after=c[5],
                         found=c[6], phase=int(c[7]) if c[7].isdigit() else None))


def spans(cell):
    """Code spans beginning `grep`/`git grep`, each ending at a backtick outside single quotes."""
    out, i = [], 0
    while True:
        j = cell.find('`', i)
        if j < 0:
            return out
        if not re.match(r'(git )?grep ', cell[j + 1:]):
            k = cell.find('`', j + 1)
            if k < 0:
                return out
            i = k + 1
            continue
        q, k = False, j + 1
        while k < len(cell) and not (cell[k] == '`' and not q):
            if cell[k] == "'":
                q = not q
            k += 1
        out.append(cell[j + 1:k].replace('\\|', '|'))
        i = k + 1


def page_list(cell):
    return re.findall(r'`([A-Za-z0-9]+\.md)`', cell)


OVERRIDE = {
    48: lambda r: [r"grep -cE '^\| `CircuitBreaker` \|' contents/Telemetry.md"],
    51: lambda r: ['grep -LiE causationid ' + ' '.join('contents/' + p for p in page_list(r['pages']))],
    92: lambda r: ['python3 %s' % os.path.join(ROOT, 'spec/018-compile_residual/probe/mapper_members.py')],
}

_trees = {}


def checkout(ref):
    if ref in _trees:
        return _trees[ref]
    d = tempfile.mkdtemp(prefix='ledger-%s-' % ref)
    data = subprocess.run(['git', 'archive', ref, 'contents', 'tools'], check=True,
                          capture_output=True).stdout
    tarfile.open(fileobj=io.BytesIO(data)).extractall(d)
    _trees[ref] = d
    return d


def count(cmd, ref):
    cwd = tree or ROOT if ref == 'HEAD' else checkout(ref)
    if '<ref>' in cmd and ref == 'HEAD' and tree:     # a copy has no history: grep it as files
        cmd = re.sub(r'^git grep (.*?) <ref> -- ', r'grep -r \1 ', cmd)
    elif '<ref>' in cmd:
        cmd, cwd = cmd.replace('<ref>', ref), ROOT
    p = subprocess.run(['bash', '-c', cmd], cwd=cwd, capture_output=True, text=True)
    out = p.stdout.strip()
    if p.returncode > 1 and not out:
        return 'ERR(%d: %s)' % (p.returncode, p.stderr.strip()[:80])
    if re.fullmatch(r'\d+', out):
        return int(out)
    return len([l for l in out.splitlines() if l.strip()])


EXTRA_PY = r'''
import re, sys
sys.path.insert(0, 'tools')
import blockcheck as bc
built = {(l.split('\t')[0], l.split('\t')[1]) for l in open('tools/blockcheck/baseline.tsv') if not l.startswith('#')}
row = sys.argv[1]
for b in bc.enumerate_blocks():
    body = b.body if isinstance(b.body, str) else '\n'.join(b.body)
    name = '%s #%d%s' % (b.rel.replace('contents/', ''), b.ordinal, ' BUILT' if (b.rel, str(b.ordinal)) in built else '')
    if row == '6':
        for m in re.finditer(r'class\s+(\w+)(?:\([^)]*\))?\s*:\s*IAmA(Message|Request)SchedulerAsync', body):
            members = sorted(set(re.findall(r'public (?:async )?[\w<>?]+ (\w+)(?:<[^>]*>)?\(', body)))
            print('declares', m.group(1), name, 'public members:', ' '.join(members))
    if row == '8':
        for m in re.finditer(r'UseScheduler\(provider =>', body):
            i, depth = body.index('{', m.end()) if '{' in body[m.end():m.end() + 40] else -1, 0
            if i < 0:
                continue
            for j in range(i, len(body)):
                depth += {'{': 1, '}': -1}.get(body[j], 0)
                if depth == 0:
                    break
            types = sorted(set(re.findall(r'return new (\w+SchedulerFactory)', body[i:j])))
            print('MIXED' if len(types) > 1 else 'single', '+'.join(types) or '-', name)
    if row == '85':
        for m in re.finditer(r'class \w+<T>\s*:\s*RequestHandler(Async)?<T>[ \t]*\n(.*)', body):
            print('constrained' if m.group(2).strip().startswith('where T') else 'UNCONSTRAINED', name)
'''
EXTRA_ROWS = {6: 'the declaration across its wrap, each class and its block',
              8: 'each `UseScheduler(provider => { … })` lambda by the factory types it returns',
              85: 'hits, not lines: each generic handler base and whether its next line constrains T'}


def extra(n, ref):
    cwd = tree or ROOT if ref == 'HEAD' else checkout(ref)
    p = subprocess.run(['python3', '-c', EXTRA_PY, str(n)], cwd=cwd, capture_output=True, text=True)
    return [l for l in p.stdout.splitlines() if l.strip()] or ['(none)' + (' ' + p.stderr.strip()[-80:] if p.returncode else '')]


def walk(cmds, phase, before):
    """The phase branch's commits, oldest first, with each command's counts where they change; and the
    last commit whose counts equal the Before cell's, the commit before the repair."""
    m = PHASE_MERGE[phase]
    revs = subprocess.run(['git', 'rev-list', '--reverse', '--first-parent', '%s^1..%s^2' % (m, m)],
                          check=True, capture_output=True, text=True).stdout.split()
    seq, last, prev = [], None, None
    for rev in [PHASE_PRE[phase]] + revs:
        c = [count(x, rev) for x in cmds]
        if c != prev:
            seq.append('%s=%s' % (rev[:7] if '^' not in rev else rev, c))
        if c == before[:len(c)]:
            last = rev[:7] if '^' not in rev else rev
        prev = c
    return seq, last


def bold(cell):
    return [int(x) for x in re.findall(r'\*\*(\d+)\*\*', cell)]


if hits:
    cls = {}
    for l in open(cls_tsv):
        f = l.rstrip('\n').split('\t')
        if len(f) >= 3 and f[1].isdigit():
            cls[(f[0], int(f[1]))] = f[2]
    blocks = subprocess.run(['python3', '-c', 'import sys; sys.path.insert(0, "tools"); import blockcheck as bc\n'
                             'for b in bc.enumerate_blocks(): print(b.rel, b.ordinal, b.start, b.end)'],
                            cwd=at, capture_output=True, text=True, check=True).stdout.split('\n')
    blocks = [l.split() for l in blocks if l]
    for r in rows:
        if r['n'] not in hits:
            continue
        cmds = OVERRIDE[r['n']](r) if r['n'] in OVERRIDE else spans(r['grep'])
        for c in cmds:
            first = re.sub(r' \| .*$', '', c)                     # the grep before any pipe names the sites
            out = subprocess.run(['bash', '-c', first], cwd=at, capture_output=True, text=True).stdout
            single = re.search(r'(contents/\S+\.md)\s*$', first)
            seen = []
            for l in out.splitlines():
                m = re.match(r'(contents/[^:]+?\.md)[:-](\d+)[:-]', l) or (
                    single and re.match(r'()(\d+)[:-]', l))
                if not m:
                    continue
                page, line = m.group(1) or single.group(1), int(m.group(2))
                b = next((x for x in blocks if x[0] == page and int(x[2]) <= line <= int(x[3])), None)
                name = '%s #%s %s' % (page.replace('contents/', ''), b[1], cls.get((page, int(b[1])), 'BUILT/not C#')) \
                    if b else '%s prose' % page.replace('contents/', '')
                if name not in seen:
                    seen.append(name)
            print('%3d  %s\n       %s' % (r['n'], c, '; '.join(seen) or '(no hit)'))
    sys.exit(0)

# A row whose cell counts something other than the grep's lines says so in the cell; these encode it.
OFFSET = {84: [1, 0]}   # the first grep also reads PipelineValidation.md's `text` log line, the cell's "(+1 …)"
SHAPE_DEFECT = {6: lambda ls: len([l for l in ls if l.startswith('declares')]),   # the cell counts classes
                8: lambda ls: len([l for l in ls if l.startswith('MIXED')]),      # the defect: a mixed lambda
                85: lambda ls: len([l for l in ls if l.startswith('UNCONSTRAINED')])}


def cmp(counts, cell, off):
    return [x - o if isinstance(x, int) else x for x, o in zip(counts, off + [0] * len(counts))] != cell[:len(counts)]


flag_rows = {'BEFORE': [], 'AFTER': [], 'NONZERO': [], 'CELL': [], 'BASE': []}
for r in rows:
    n = r['n']
    over = n in OVERRIDE
    cmds = OVERRIDE[n](r) if over else spans(r['grep'])
    pre = PHASE_PRE.get(r['phase'])
    c_pre = [count(c, pre) for c in cmds] if pre else []
    c_base = [count(c, BASE) for c in cmds]
    c_tree = [count(c, 'HEAD') for c in cmds]
    b, a = bold(r['before']), bold(r['after'])
    if n == 16:              # "1 / 0" and "2 / 1" are figures, not bold per figure
        b = [int(x) for x in re.findall(r'\d+', r['before'])]
        a = [int(x) for x in re.findall(r'\d+', r['after'])[:2]]
    off = OFFSET.get(n, [])
    shape = None
    if n in SHAPE_DEFECT:
        sp, st = extra(n, pre), extra(n, 'HEAD')
        shape = (SHAPE_DEFECT[n](sp), SHAPE_DEFECT[n](st), st)
    flags = []
    if not cmds or not r['found'] or len(b) < len(cmds) or len(a) < len(cmds):
        flags.append('CELL')
    if (cmp(c_pre, b, off) if shape is None else shape[0] != b[0]):
        flags.append('BEFORE')
    if (cmp(c_tree, a, off) if shape is None else shape[1] != a[0]):
        flags.append('AFTER')
    if (any(x != o for x, o in zip(c_tree, off + [0] * len(c_tree))) if shape is None or n == 6 else shape[1]):
        flags.append('NONZERO')   # row 6's cell counts the classes, which stay; its grep is the defect's
    if c_base != c_pre:
        flags.append('BASE')
    for f in flags:
        flag_rows[f].append(n)
    print('%3d  ph%s  pre[%s]=%s  base=%s  tree=%s  Before=%s  After=%s  found=%s  %s%s%s%s' % (
        n, r['phase'], pre, c_pre, c_base, c_tree, b, a, 'yes' if r['found'] else 'NO',
        ' '.join(flags) or 'ok', '  (override)' if over else '',
        '  (offset %s)' % off if off else '',
        '  (shape decides: pre=%d tree=%d)' % shape[:2] if shape else ''))
    if verbose:
        for c in cmds:
            print('       $ ' + c)
    if 'BEFORE' in flags:
        seq, last = walk(cmds, r['phase'], b)
        print('       walk: ' + '  '.join(seq))
        print('       commit before the repair: %s' % (last or 'none reads the Before cell'))
    if shape:
        print('       shape (%s), tree:' % EXTRA_ROWS[n])
        for l in shape[2]:
            print('         ' + l)

print('%d rows; commands read from %d, by override %d' % (
    len(rows), len(rows) - sum(r['n'] in OVERRIDE for r in rows), sum(r['n'] in OVERRIDE for r in rows)))
for f, ns in flag_rows.items():
    print('%-8s %d: %s' % (f, len(ns), ' '.join(map(str, ns)) or '-'))
sys.exit(1 if flag_rows['AFTER'] or flag_rows['CELL'] else 0)
