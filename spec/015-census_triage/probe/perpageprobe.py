"""Does the member filter belong per-page, or all-or-nothing across pages?

Run from the repository root:
    python3 spec/015-census_triage/probe/perpageprobe.py

`methodprobe.py` measured the filter POST-HOC and ALL-OR-NOTHING: a name is
removed only when every page using it also declares it as a method. That is a
property of the whole census, computed after the fact.

`DECL_RE` -- the type filter already in `symbolcheck.census()` -- does something
different. It is applied PER PAGE, inside the loop: a token declared on page X
is not a candidate ON PAGE X, and a page that uses it without declaring it still
contributes. So a name declared on some pages and used bare on others SURVIVES
with a reduced page-spread instead of vanishing or staying whole.

P0-1 says "restore the member filter". It does not say which of these two it is,
and they are not the same filter:

  * all-or-nothing  removes 110 names entirely, leaves 31 "declared on SOME" whole
  * per page        removes the same 110, and SHRINKS the 31 towards their bare uses

The second is what consistency with DECL_RE requires. It also reorders the report,
because page-spread IS the ordering -- so it can move names in and out of P0-4's
>=3-page slice, which is the budget the spec is scoped from.

Controls, two-way:

    positive   ConfigureBrighter MUST be removed   (declared on all 14 of its pages)
    negative   HostBuilderContext MUST survive     (14 pages, never page-declared)
    negative   CommandProcessor MUST NOT appear    (a real API; resolves, so the
                                                    census should never carry it)

The obvious third control was `IMessageScheduler MUST survive` -- 014's one real
discovery, a watchlisted dead name, exactly the thing a precision filter must not
hide. It FAILED, and the filter was innocent: 014 repaired every site, so
`grep -rl IMessageScheduler contents/` is 0 and the name is not in the census to
survive anything. **A control must be present in the corpus to test removal from
it.** An absent name makes the check unsatisfiable whatever the filter does, which
is a plausible zero wearing a control's clothes.

The enumeration's blind spot is measured rather than controlled, below:
METHOD_DECL_RE is anchored on a MODIFIER, so a declaration carrying none --
`void Handle(Order o)` in an interface body, or a local function -- cannot match.
Friction 36 asks for a positive case outside the enumeration; here that case is a
whole syntax, so it gets a count instead of a boolean.
"""
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'tools'))

import symbolcheck as sc  # noqa: E402

from methodprobe import METHOD_DECL_RE, pages, unresolved_candidates  # noqa: E402


def census_per_page(page_list):
    """`sc.census`, with method declarations added to the per-page `declared` set.

    A copy of symbolcheck.census() with three lines changed, so that what P0-1
    would ship is what is measured here. Re-read it against the original before
    trusting the numbers.
    """
    candidates = {}
    for rel in page_list:
        with open(os.path.join(ROOT, rel), encoding='utf-8') as fh:
            lines = fh.read().splitlines()
        blocks = sc.csharp_blocks(lines)
        if not blocks:
            continue
        declared = set()
        for body in blocks:
            declared.update(sc.DECL_RE.findall(body))
            declared.update(METHOD_DECL_RE.findall(sc.strip_noncode(body)))
        for body in blocks:
            for token in sc.TOKEN_RE.findall(sc.strip_noncode(body)):
                if (token in declared or token in sc.NOISE_EXACT
                        or token.startswith(sc.NOISE_PREFIX)):
                    continue
                pages_for = candidates.setdefault(token, {})
                pages_for[rel] = pages_for.get(rel, 0) + 1
    return candidates


def unresolved(candidates, known):
    out = {}
    for symbol, hits in candidates.items():
        if symbol in known:
            continue
        if symbol.endswith('Attribute') or (symbol + 'Attribute') in known:
            continue
        out[symbol] = hits
    return out


def slices(hits_by_symbol):
    return {n: sum(1 for h in hits_by_symbol.values() if len(h) >= n)
            for n in (7, 5, 4, 3, 2)}


# Properties and auto-properties, for the --stages measurement ONLY. This regex
# is deliberately NOT shipped into symbolcheck: P1-1 measures what it would buy,
# and Q1 ruled that the measurement is the deliverable, not the code.
PROP_DECL_RE = re.compile(
    r'\b(?:public|private|protected|internal|static|readonly|const|override|'
    r'virtual)\b[^;=(\n]*?\b([A-Za-z_][A-Za-z0-9_]*)\s*(?:\{\s*(?:get|set|init)|;|=)')


def stage_count(page_list, extra):
    """Candidates remaining with `extra` declaration regexes also forgiven."""
    seen = set()
    for rel in page_list:
        with open(os.path.join(ROOT, rel), encoding='utf-8') as fh:
            blocks = sc.csharp_blocks(fh.read().splitlines())
        if not blocks:
            continue
        declared = set()
        for body in blocks:
            declared.update(sc.DECL_RE.findall(body))
            for rx in extra:
                declared.update(rx.findall(sc.strip_noncode(body)))
        for body in blocks:
            for token in sc.TOKEN_RE.findall(sc.strip_noncode(body)):
                if (token in declared or token in sc.NOISE_EXACT
                        or token.startswith(sc.NOISE_PREFIX)):
                    continue
                seen.add(token)
    return len(seen)


def stages():
    """P1-1: how much of finding E's 808-name gap do members actually recover?"""
    page_list = pages()
    types = stage_count(page_list, [])
    methods = stage_count(page_list, [METHOD_DECL_RE])
    members = stage_count(page_list, [METHOD_DECL_RE, PROP_DECL_RE])
    recorded = 1211
    print(f'types only -- shipped today        : {types}')
    print(f'+ methods                          : {methods}   '
          f'(recovers {types - methods})')
    print(f'+ properties and fields            : {members}   '
          f'(recovers {types - members})')
    print(f'design.md 3.2 recorded             : {recorded}')
    print(f'\nunexplained after both             : {members - recorded}'
          f'  of the {types - recorded}-name gap')
    print('\nSo whatever the original probe filtered, it was not members.')
    return 0


def main():
    if '--stages' in sys.argv:
        return stages()
    page_list = pages()
    known = set().union(*sc.universe().values())

    shipped = unresolved_candidates(page_list)          # today's 929
    perpage = unresolved(census_per_page(page_list), known)

    # all-or-nothing, as methodprobe computes it, for the three-way comparison
    declared = {}
    for rel in page_list:
        with open(os.path.join(ROOT, rel), encoding='utf-8') as fh:
            names = set()
            for body in sc.csharp_blocks(fh.read().splitlines()):
                names.update(METHOD_DECL_RE.findall(sc.strip_noncode(body)))
            declared[rel] = names
    allornothing = {s: h for s, h in shipped.items()
                    if not all(s in declared.get(p, ()) for p in h)}

    print(f'{"":<34}{"names":>7}{"  >=7":>7}{"  >=5":>7}'
          f'{"  >=4":>7}{"  >=3":>7}{"  >=2":>7}')
    for label, data in (('shipped today (types only)', shipped),
                        ('all-or-nothing (methodprobe)', allornothing),
                        ('PER PAGE (what P0-1 ships)', perpage)):
        s = slices(data)
        print(f'{label:<34}{len(data):>7}{s[7]:>7}{s[5]:>7}'
              f'{s[4]:>7}{s[3]:>7}{s[2]:>7}')

    ok = {
        'positive  ConfigureBrighter removed ': 'ConfigureBrighter' not in perpage,
        'negative  HostBuilderContext survives': 'HostBuilderContext' in perpage,
        'negative  CommandProcessor absent   ': 'CommandProcessor' not in perpage,
    }
    print('\ncontrols:')
    for label, passed in ok.items():
        print(f'  {label} : {passed}')
    if not all(ok.values()):
        print('\nCONTROLS FAILED -- the numbers above mean nothing.',
              file=sys.stderr)
        return 2

    moved_out = sorted(s for s in allornothing
                       if s in perpage and len(perpage[s]) < len(allornothing[s]))
    print(f'\nnames whose page-spread SHRANK but did not vanish: {len(moved_out)}')
    for s in sorted(moved_out,
                    key=lambda s: -(len(allornothing[s]) - len(perpage[s])))[:12]:
        print(f'  {len(allornothing[s]):>3} -> {len(perpage[s]):<3} pages  {s}')

    in3_a = {s for s, h in allornothing.items() if len(h) >= 3}
    in3_p = {s for s, h in perpage.items() if len(h) >= 3}
    print(f'\n>=3 slice, all-or-nothing : {len(in3_a)}')
    print(f'>=3 slice, per page       : {len(in3_p)}')
    print(f'  dropped out of the slice by per-page: {len(in3_a - in3_p)}')
    for s in sorted(in3_a - in3_p):
        print(f'      {len(allornothing[s])} -> {len(perpage[s])} pages  {s}')
    print(f'  entered the slice by per-page       : {len(in3_p - in3_a)} '
          f'{sorted(in3_p - in3_a)}')

    # The enumeration's blind spot, counted rather than asserted. A method
    # declaration carrying no modifier cannot match METHOD_DECL_RE at all.
    nomod = re.compile(r'^\s{2,}(?!(?:public|private|protected|internal|static|'
                       r'async|override|virtual|sealed|partial|extern|new|return|'
                       r'if|for|foreach|while|switch|using|await|var|catch|lock)\b)'
                       r'[A-Za-z_][A-Za-z0-9_<>,\[\]\?\. ]*\s'
                       r'([A-Z][A-Za-z0-9_]{3,})\s*\([^)]*\)\s*$', re.M)
    missed = {}
    for rel in page_list:
        with open(os.path.join(ROOT, rel), encoding='utf-8') as fh:
            for body in sc.csharp_blocks(fh.read().splitlines()):
                stripped = sc.strip_noncode(body)
                for name in nomod.findall(stripped):
                    if name in perpage and name not in METHOD_DECL_RE.findall(stripped):
                        missed.setdefault(name, set()).add(rel)
    print(f'\nBLIND SPOT -- candidates declared method-shaped with NO modifier, '
          f'so the filter cannot see them: {len(missed)}')
    for name in sorted(missed, key=lambda n: -len(perpage[n]))[:10]:
        print(f'  {len(perpage[name]):>3} page(s) in census  {name}')
    return 0


if __name__ == '__main__':
    sys.exit(main())
