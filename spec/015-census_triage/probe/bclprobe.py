"""What a BCL/ASP.NET resolution filter would remove -- and why it must not.

Run from the repository root:  python3 spec/015-census_triage/probe/bclprobe.py

The census resolves candidates against Brighter's and Darker's `src/` and nothing
else, so every .NET type a page legitimately uses is unresolved by construction.
`NOISE_PREFIX` catches only the namespace-qualified spelling -- a bare
`WebApplication` or `HostBuilderContext` sails through.

This probe resolves the candidates against the .NET 8 ref packs' XML
documentation, which lists every public member. It removes 111 of 929.

**It is preserved as evidence AGAINST adopting it**, which is why it lives here
rather than in `tools/`. The token set is built from fully-qualified member
names, so `System.DateTime.Date` contributes the bare token `Date` -- and the
filter then strikes `Date`, `Email`, `Total`, `Product`, `Cancelled` and
`Country`, which are the documentation's own invented domain. It is right about
most of what it removes and wrong about the rest, for a reason no threshold
fixes: it is a NAME filter, and names collide. Only a compiler resolves a
reference.

Controls:
    present   WebApplication  MUST be in the set  (a real ASP.NET type)
    absent    CommandProcessor MUST NOT be        (Brighter's, not .NET's)
    absent    GreetingEvent    MUST NOT be        (the docs' invented domain)
"""
import glob
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'tools'))

import symbolcheck as sc  # noqa: E402
from methodprobe import pages, unresolved_candidates  # noqa: E402

PACKS = ('Microsoft.NETCore.App.Ref', 'Microsoft.AspNetCore.App.Ref')
SDK = '/usr/local/share/dotnet/packs'
VERSION = '8.0.0'
MEMBER_RE = re.compile(r'<member name="([^"]+)"')


def bcl_tokens():
    files = []
    for pack in PACKS:
        files += glob.glob(f'{SDK}/{pack}/{VERSION}/ref/net8.0/*.xml')
    if not files:
        raise SystemExit(
            f'no ref-pack XML under {SDK}/*/{VERSION}/ref/net8.0/. '
            f'A plausible zero: with no files the filter removes nothing and '
            f'reads as "the BCL is not the problem".')
    tokens = set()
    for path in files:
        with open(path, encoding='utf-8', errors='ignore') as fh:
            for match in MEMBER_RE.finditer(fh.read()):
                tokens.update(sc.TOKEN_RE.findall(match.group(1)))
    return files, tokens


def main():
    files, bcl = bcl_tokens()
    print(f'ref-pack xml files read : {len(files)}')
    print(f'BCL/ASP.NET tokens      : {len(bcl)}')

    checks = [('present WebApplication', 'WebApplication' in bcl, True),
              ('absent  CommandProcessor', 'CommandProcessor' in bcl, False),
              ('absent  GreetingEvent', 'GreetingEvent' in bcl, False)]
    print('\ncontrols:')
    ok = True
    for label, got, want in checks:
        ok &= (got == want)
        print(f'  {label:26} {got}   (want {want})')
    if not ok:
        print('\nCONTROLS FAILED -- the numbers below mean nothing.',
              file=sys.stderr)
        return 2

    unresolved = unresolved_candidates(pages())
    hit = [s for s in unresolved if s in bcl]
    print(f'\nunresolved candidates       : {len(unresolved)}')
    print(f'...also a BCL/ASP.NET name  : {len(hit)}')
    print(f'remaining                   : {len(unresolved) - len(hit)}')

    print('\nhighest-spread names this would remove '
          '(read the domain names among them):')
    for s in sorted(hit, key=lambda s: -len(unresolved[s]))[:20]:
        print(f'  {len(unresolved[s]):>3} page(s)  {s}')
    return 0


if __name__ == '__main__':
    sys.exit(main())
