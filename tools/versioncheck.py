#!/usr/bin/env python3
"""Check the package versions pinned in tutorial prose against NuGet.

A tutorial tells the reader to run `dotnet add package Paramore.Brighter
--version 10.7.0`, and that number is a promise the reader can check in one
command. It goes stale because of an event in *another* repository -- a
Brighter release -- so nothing in this repository's history marks the moment it
stops being true. That is why this is a gate and not a checklist line, and why
the workflow runs it on a daily schedule as well as on pull requests.

What it scans
-------------
Only the pages named in TUTORIAL_PAGES, listed explicitly and never globbed. A
glob would silently start policing pages that quote an old version on purpose
-- V10MigrationGuide.md exists to show V9 code.

In each page, every match of:

    --version 10.7.0                                 (a dotnet add package line)
    Version="10.7.0"                                 (a PackageReference)

restricted to lines that also name a `Paramore.Brighter*` package. Anything
else on the page is left alone.

What it compares against
------------------------
NuGet, per package: the highest non-prerelease in

    https://api.nuget.org/v3-flatcontainer/<id>/index.json

Per package rather than once for `paramore.brighter`, because that is the
question `dotnet add package` actually asks. The two are not interchangeable:
`Paramore.Brighter` has shipped twelve 7.x/8.x versions that
`Paramore.Brighter.Extensions.DependencyInjection` never shipped, so the
package lines have diverged before even though they agree at the tip today.
Each id is fetched once however many times it is pinned.

`--release-notes PATH` reads the latest released heading from a local
release_notes.md instead, so the tool still runs with no network. When both
authorities are available they are compared and a disagreement is *reported*,
not resolved: it means a release shipped without notes, or notes shipped
without a release, and either is a fact about Brighter that a docs tool should
not paper over.

Vacuity
-------
The failure this tool is most likely to have is passing without looking at
anything, so:

  * a listed page that does not exist is skipped, and the skip is printed --
    the ladder ships one rung at a time, so absence is expected but never
    silent;
  * a page that exists and contributes no pins is an error, because a tutorial
    whose prose pins no version is one the reader cannot reproduce;
  * the scope -- pages listed, found, skipped, and pins examined -- is printed
    before the verdict, so `0 stale pins` out of 0 and out of 7 do not print
    the same line.

Usage:
    python3 tools/versioncheck.py
    python3 tools/versioncheck.py --release-notes ../Brighter/release_notes.md
    python3 tools/versioncheck.py contents/TutorialFirstCommand.md

Exit code is 0 when every pin is current, 1 when a pin is stale or a listed
page that exists pins nothing, and 2 when the current version could not be
determined. **2 is not a pass**: an unreachable authority is an unchecked pin,
so CI must treat it as a failure.
"""
import json
import os
import re
import sys
import urllib.error
import urllib.request

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# Listed, never globbed. A page joins this list when its rung ships; a page
# that is not here yet is reported as skipped rather than passing invisibly.
TUTORIAL_PAGES = [
    'contents/TutorialFirstCommand.md',
    'contents/TutorialFirstMessage.md',
    'contents/TutorialDurableOutbox.md',
    'contents/TutorialStreamingWithKafka.md',
    'contents/GetStarted.md',
]

NUGET_INDEX = 'https://api.nuget.org/v3-flatcontainer/{id}/index.json'

TIMEOUT = 20

# The package whose version a line pins. Both forms below carry the id on the
# same line as the number, which is what makes a line-scoped match safe:
#
#   dotnet add package Paramore.Brighter.Extensions.DependencyInjection --version 10.7.0
#   <PackageReference Include="Paramore.Brighter" Version="10.7.0" />
#
# The trailing (?![.\w]) stops `Paramore.Brighter` matching the prefix of a
# longer id and reporting the wrong package.
PACKAGE_RE = re.compile(r'\bParamore\.Brighter(?:\.[A-Za-z0-9]+)*(?![.\w])')

PIN_RES = (
    re.compile(r'--version\s+(\d+\.\d+\.\d+)'),
    re.compile(r'Version="(\d+\.\d+\.\d+)"'),
)

# `## Master` is the unreleased section and `## Release 9.X` is not a version,
# so the latest *release* is the first heading carrying three numbers.
RELEASE_HEADING_RE = re.compile(r'^##\s+(?:Release\s+)?(\d+)\.(\d+)\.(\d+)\s*$')


def parse_version(text):
    """('10.7.0') -> (10, 7, 0), or None if it is not three integers."""
    parts = text.split('.')
    if len(parts) != 3:
        return None
    try:
        return tuple(int(p) for p in parts)
    except ValueError:
        return None


def latest_stable(versions):
    """Highest non-prerelease in a NuGet versions[] array.

    Prereleases carry a `-` suffix (10.8.0-beta.1) and are dropped: the pin's
    job is to match what `dotnet add package` gives a reader by default, and
    that is the highest stable.
    """
    stable = []
    for raw in versions:
        if '-' in raw or '+' in raw:
            continue
        parsed = parse_version(raw)
        if parsed:
            stable.append((parsed, raw))
    if not stable:
        return None
    return max(stable)[1]


def nuget_latest(package_id):
    """Highest non-prerelease of one package, or a reason it is unknown."""
    url = NUGET_INDEX.format(id=package_id.lower())
    try:
        with urllib.request.urlopen(url, timeout=TIMEOUT) as response:
            payload = json.load(response)
    except urllib.error.HTTPError as exc:
        if exc.code == 404:
            return None, f'NuGet has no package {package_id}'
        return None, f'NuGet returned HTTP {exc.code} for {package_id}'
    except (urllib.error.URLError, TimeoutError, OSError) as exc:
        return None, f'could not reach NuGet for {package_id}: {exc}'
    except json.JSONDecodeError as exc:
        return None, f'NuGet returned unreadable JSON for {package_id}: {exc}'

    version = latest_stable(payload.get('versions') or [])
    if not version:
        return None, f'NuGet lists no stable version of {package_id}'
    return version, None


def release_notes_latest(path):
    """Latest released version from a local release_notes.md."""
    try:
        with open(path, encoding='utf-8') as handle:
            text = handle.read()
    except OSError as exc:
        return None, f'could not read {path}: {exc}'
    for line in text.splitlines():
        match = RELEASE_HEADING_RE.match(line)
        if match:
            return '.'.join(match.groups()), None
    return None, f'no release heading in {path}'


def pins(path):
    """Every (lineno, package, version) a page pins.

    Scoped to a line, because both pin forms carry the package id and the
    number together. A page-wide match would attribute a version to whichever
    package name happened to appear nearest it.
    """
    with open(os.path.join(ROOT, path), encoding='utf-8') as handle:
        lines = handle.read().splitlines()

    found = []
    for lineno, line in enumerate(lines, 1):
        package = PACKAGE_RE.search(line)
        if not package:
            continue
        for pattern in PIN_RES:
            for match in pattern.finditer(line):
                found.append((lineno, package.group(0), match.group(1)))
    return found


def main(argv):
    release_notes = None
    paths = []
    args = list(argv)
    while args:
        arg = args.pop(0)
        if arg == '--release-notes':
            if not args:
                print('--release-notes needs a path, e.g. --release-notes '
                      '../Brighter/release_notes.md', file=sys.stderr)
                return 2
            release_notes = args.pop(0)
        elif arg.startswith('-'):
            print(f'unknown option: {arg}', file=sys.stderr)
            return 2
        else:
            paths.append(arg)

    if paths:
        listed = []
        for raw in paths:
            rel = os.path.relpath(os.path.abspath(raw), ROOT)
            if rel not in TUTORIAL_PAGES:
                print(f'not a tutorial page: {raw}\n'
                      f'add it to TUTORIAL_PAGES in {__file__} if it should be '
                      f'checked', file=sys.stderr)
                return 2
            listed.append(rel)
    else:
        listed = list(TUTORIAL_PAGES)

    present, absent = [], []
    for rel in listed:
        (present if os.path.isfile(os.path.join(ROOT, rel)) else absent).append(rel)

    examined = []
    empty = []
    for rel in present:
        found = pins(rel)
        if found:
            examined += [(rel,) + pin for pin in found]
        else:
            empty.append(rel)

    # Scope before verdict. A run that examined nothing must not be able to
    # print the same summary as one that examined seven pins.
    print(f'{len(listed)} page(s) listed, {len(present)} found, '
          f'{len(absent)} not written yet, {len(examined)} pin(s) examined.')
    for rel in absent:
        print(f'  skipped (does not exist yet): {rel}')

    notes_version = None
    if release_notes:
        notes_version, reason = release_notes_latest(release_notes)
        if notes_version:
            print(f'  release notes ({release_notes}): {notes_version}')
        else:
            print(f'  release notes unusable: {reason}')

    # A page that exists and pins nothing is a defect in the page, and it is
    # reported before any network call so it stands on its own.
    if empty:
        print()
        print(f'===== NO PINS ({len(empty)}) =====')
        for rel in empty:
            print(f'{rel}: exists but pins no Paramore.Brighter version; a '
                  f'reader cannot reproduce it')
        print(f'\n{len(empty)} tutorial page(s) pin nothing.')
        return 1

    if not examined:
        print('\nNo pins to check: no listed tutorial page exists yet.')
        return 0

    latest = {}
    unreachable = []
    # `asked`, not `latest`, is what stops a second query: a package that could
    # not be reached never lands in `latest`, so keying on that would re-ask
    # once per *pin* rather than once per package -- N timeouts on a genuine
    # outage, and N copies of the same line. The red-proof printed four of them
    # for two packages, which is how this was found.
    asked = set()
    for _, _, package, _ in examined:
        if package in asked:
            continue
        asked.add(package)
        version, reason = nuget_latest(package)
        if version:
            latest[package] = version
            print(f'  NuGet ({package}): {version}')
        else:
            unreachable.append(reason)

    if unreachable:
        if notes_version:
            # The fallback is what makes the tool runnable offline. It answers
            # for Brighter as a whole rather than per package, which is the
            # cost of not asking NuGet.
            for _, _, package, _ in examined:
                latest.setdefault(package, notes_version)
            print()
            for reason in unreachable:
                print(f'  {reason}')
            print(f'  falling back to {release_notes} for the rest')
        else:
            print()
            print('===== AUTHORITY UNREACHABLE =====')
            for reason in unreachable:
                print(f'  {reason}')
            print('\nCould not determine the current version. This is not a '
                  'pass: an unchecked pin is not a current pin. Retry, or pass '
                  '--release-notes PATH to check against a local '
                  'release_notes.md.')
            return 2

    # Both authorities available: report a disagreement, never resolve it.
    disagreements = []
    if notes_version:
        for package, version in sorted(latest.items()):
            if version != notes_version:
                disagreements.append(
                    f'{package}: NuGet says {version}, {release_notes} says '
                    f'{notes_version}')

    stale = []
    for rel, lineno, package, version in examined:
        expected = latest[package]
        if version != expected:
            stale.append((rel, lineno, package, version, expected))

    if disagreements:
        print()
        print(f'===== AUTHORITIES DISAGREE ({len(disagreements)}) =====')
        print('reported, not resolved -- a release shipped without notes, or '
              'notes without a release:')
        for row in disagreements:
            print(f'  {row}')

    if stale:
        print()
        print(f'===== STALE PIN ({len(stale)}) =====')
        for rel, lineno, package, version, expected in stale:
            print(f'{rel}:{lineno}  {package} pinned at {version}, '
                  f'current is {expected}')
        print(f'\n{len(stale)} stale pin(s) of {len(examined)} examined across '
              f'{len(present)} page(s).')
        return 1

    print(f'\n0 stale pins of {len(examined)} examined across '
          f'{len(present)} page(s).')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
