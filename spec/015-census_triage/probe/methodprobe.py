"""How many census candidates are methods the page declares itself?

Run from the repository root:  python3 spec/015-census_triage/probe/methodprobe.py

`symbolcheck`'s DECL_RE models type declarations -- class, interface, record,
struct, enum -- and forgives them, because a page defining its own example class
is not using an API. It does not model METHOD declarations, so a page that writes
`private static void ConfigureBrighter(...)` and then calls it is counted as
using an unresolved API 36 times across 14 pages.

Two-way control, per standing obligation 3 -- and note the positive case sits
OUTSIDE the enumeration the filter is built from, per friction 36:

    positive   ConfigureBrighter MUST be caught  (a known page-declared method)
    negative   CommandProcessor MUST NOT be      (a real API; never page-declared)

A control proving only absence proves nothing here: if the regex matched nothing,
every candidate would survive and the report would read as "the filter finds
little", which is indistinguishable from a correct filter on a clean corpus.
"""
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'tools'))

import symbolcheck as sc  # noqa: E402

# A method declaration: a modifier, a return type, the name, an open paren.
# Anchored on the modifier, so a bare call `Foo(bar)` cannot match. `[^;=(\n]*?`
# keeps it on one declaration and stops it running through an argument list.
METHOD_DECL_RE = re.compile(
    r'\b(?:public|private|protected|internal|static|async|override|virtual|'
    r'sealed|partial|extern|new)\b[^;=(\n]*?'
    r'\b([A-Za-z_][A-Za-z0-9_]*)\s*(?:<[^>()]*>)?\s*\(')


def pages():
    out = []
    for base, _, files in os.walk(os.path.join(ROOT, 'contents')):
        for name in sorted(files):
            if name.endswith('.md'):
                out.append(os.path.relpath(os.path.join(base, name), ROOT))
    return sorted(out)


def unresolved_candidates(page_list):
    """The census's own 929, reusing its code rather than reimplementing it."""
    sets = sc.universe()
    known = set().union(*sets.values())
    candidates, _ = sc.census(page_list)
    out = {}
    for symbol, hits in candidates.items():
        if symbol in known:
            continue
        if symbol.endswith('Attribute') or (symbol + 'Attribute') in known:
            continue
        out[symbol] = hits
    return out


def declared_methods(page_list):
    """Per page, the method names that page declares in its own C# fences."""
    out = {}
    for rel in page_list:
        with open(os.path.join(ROOT, rel), encoding='utf-8') as fh:
            lines = fh.read().splitlines()
        names = set()
        for body in sc.csharp_blocks(lines):
            names.update(METHOD_DECL_RE.findall(sc.strip_noncode(body)))
        out[rel] = names
    return out


def main():
    page_list = pages()
    unresolved = unresolved_candidates(page_list)
    declared = declared_methods(page_list)

    fully, partly = [], []
    for symbol, hits in unresolved.items():
        on = [p for p in hits if symbol in declared.get(p, ())]
        if len(on) == len(hits):
            fully.append(symbol)
        elif on:
            partly.append(symbol)

    print(f'unresolved candidates                      : {len(unresolved)}')
    print(f'declared as a method on EVERY page using it: {len(fully)}')
    print(f'declared on SOME pages using it            : {len(partly)}')
    print(f'remaining after the filter                 : '
          f'{len(unresolved) - len(fully)}')

    ok_pos = 'ConfigureBrighter' in fully
    ok_neg = 'CommandProcessor' not in unresolved
    print('\ncontrols:')
    print(f'  positive  ConfigureBrighter caught : {ok_pos}')
    print(f'  negative  CommandProcessor absent  : {ok_neg}')
    if not (ok_pos and ok_neg):
        print('\nCONTROLS FAILED -- the numbers above mean nothing.',
              file=sys.stderr)
        return 2

    print('\nhighest-spread names the filter removes:')
    for s in sorted(fully, key=lambda s: -len(unresolved[s]))[:15]:
        print(f'  {len(unresolved[s]):>3} page(s)  {s}')

    print('\nhighest-spread names it does NOT remove:')
    left = (s for s in unresolved if s not in fully)
    for s in sorted(left, key=lambda s: -len(unresolved[s]))[:15]:
        print(f'  {len(unresolved[s]):>3} page(s)  {s}')
    return 0


if __name__ == '__main__':
    sys.exit(main())
