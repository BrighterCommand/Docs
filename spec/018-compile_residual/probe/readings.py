#!/usr/bin/env python3
"""Obligation 19's reading at a phase close: which C# blocks the phase changed, two ways.

    python3 spec/018-compile_residual/probe/readings.py <base-ref> [<head-ref>]

Method 1, the hunks: `git diff -U0 <base> <head> -- contents`, each hunk's new-side
lines against the fence ranges of the page at <head>. A block is changed when a hunk
touches a line from its opening fence to its closing fence, or a pure deletion falls
inside it.

Method 2, the text: a C# block at <head> whose body is not the body of any C# block
on the same page at <base>.

Both use pagelint's fence parser (`load_pages`'s `Page`), never a second one. Each
changed block is printed as `page<TAB>ordinal<TAB>method(s)<TAB>+N`, N the `// ...` lines
the hunks add inside it; lines removed are read against <base>'s fences and totalled. The last lines are the
totals, and the net `// ...` count over C# blocks at both refs as a check on the hunks.
"""
import os
import re
import subprocess
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))))
sys.path.insert(0, os.path.join(ROOT, 'tools'))
from pagelint import CSHARP_TAGS, Page  # noqa: E402

OMIT = re.compile(r'^\s*//\s*\.\.\.')


def git(*args):
    return subprocess.run(['git', '-C', ROOT, *args], check=True,
                          capture_output=True, text=True).stdout


def page_at(ref, rel):
    try:
        text = git('show', f'{ref}:{rel}')
    except subprocess.CalledProcessError:
        return None
    return Page(os.path.join(ROOT, rel), rel, text.splitlines())


def csharp(page):
    if page is None:
        return []
    return [f for f in page.blocks if (f['info'] or '').strip().lower() in CSHARP_TAGS]


def body(fence):
    return '\n'.join(t for _, t in fence['body'])


def main():
    base = sys.argv[1]
    head = sys.argv[2] if len(sys.argv) > 2 else 'HEAD'
    rels = git('diff', '--name-only', base, head, '--', 'contents').split()
    changed = {}
    removed = []
    omit_net = 0
    for rel in rels:
        new, old = page_at(head, rel), page_at(base, rel)
        fences = csharp(new)
        old_fences = csharp(old)
        omit_net += sum(1 for f in fences for _, t in f['body'] if OMIT.match(t))
        omit_net -= sum(1 for f in csharp(old) for _, t in f['body'] if OMIT.match(t))
        diff = git('diff', '-U0', base, head, '--', rel).splitlines()
        new_line = old_line = 0
        for line in diff:
            m = re.match(r'^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@', line)
            if m:
                old_line, new_line = int(m.group(1)), int(m.group(3))
                count = int(m.group(4)) if m.group(4) is not None else 1
                anchor = new_line if count else new_line + 1   # a pure deletion sits after line +N
                for o, f in enumerate(fences, 1):
                    lo, hi = f['start'], f['end']
                    span = range(new_line, new_line + count) if count else [anchor]
                    if any(lo <= n <= hi for n in span):
                        changed.setdefault((rel, o), {'m': set(), 'add': 0})['m'].add('hunks')
                continue
            if line.startswith('+++') or line.startswith('---'):
                continue
            if line.startswith('+'):
                for o, f in enumerate(fences, 1):
                    if f['start'] <= new_line <= f['end'] and OMIT.match(line[1:]):
                        changed[(rel, o)]['add'] += 1
                new_line += 1
            elif line.startswith('-'):
                # read on the old side, against <base>'s fences: a removed line has no new-side line
                if OMIT.match(line[1:]) and any(f['start'] <= old_line <= f['end'] for f in old_fences):
                    removed.append((rel, old_line))
                old_line += 1
        old_bodies = {body(f) for f in csharp(old)}
        for o, f in enumerate(fences, 1):
            if body(f) not in old_bodies:
                changed.setdefault((rel, o), {'m': set(), 'add': 0})['m'].add('text')
    for (rel, o), v in sorted(changed.items()):
        print(f"{rel}\t{o}\t{','.join(sorted(v['m']))}\t+{v['add']}")
    both = [k for k, v in changed.items() if v['m'] == {'hunks', 'text'}]
    hunks = [k for k, v in changed.items() if 'hunks' in v['m']]
    text = [k for k, v in changed.items() if 'text' in v['m']]
    print(f'# pages changed: {len(rels)}; C# blocks changed: hunks {len(hunks)} on '
          f'{len({r for r, _ in hunks})} pages, text {len(text)} on {len({r for r, _ in text})} pages, '
          f'both {len(both)}')
    add = sum(v['add'] for v in changed.values())
    rem = len(removed)
    blocks = sum(1 for v in changed.values() if v['add'])
    print(f'# `// ...` lines: +{add} in {blocks} blocks; -{rem} on {len({r for r, _ in removed})} pages; net by hunks {add - rem:+d}, '
          f'net by count at both refs {omit_net:+d}')


if __name__ == '__main__':
    main()
