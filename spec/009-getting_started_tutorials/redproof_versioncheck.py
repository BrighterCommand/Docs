#!/usr/bin/env python3
"""Task 4.2 -- force tools/versioncheck.py red, one branch at a time.

The discipline this probe is written to, from spec 010's rule-7 red-proofs:

  * print a BASELINE first, and require it green. A probe that starts at its
    first mutation cannot tell a rule that fires from a tool that is broken.
  * assert the mutation produced *the input the branch rejects*, not merely
    that the text changed. Three of 010's rule-7 proofs reported SILENT and all
    three were the probe's fault.
  * restore from a copy taken aside, never `git checkout --`, and assert the
    restored file is byte-identical.
  * when an assertion disagrees with the exit code, the assertion is the
    suspect.
"""
import contextlib
import hashlib
import io
import os
import shutil
import sys
import tempfile

# Derived from __file__, never hard-coded: urlmap.py shipped a `parents[2]`
# that was correct where it was written and silently wrong one directory later.
# This file lives at spec/009-getting_started_tutorials/, so the repo is two up.
REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
sys.path.insert(0, os.path.join(REPO, 'tools'))
import versioncheck  # noqa: E402

PAGE = os.path.join(REPO, 'contents/TutorialFirstCommand.md')
REL = 'contents/TutorialFirstCommand.md'

results = []


def run(argv):
    """Run main() capturing stdout, returning (exit code, output)."""
    buf = io.StringIO()
    with contextlib.redirect_stdout(buf):
        code = versioncheck.main(argv)
    return code, buf.getvalue()


def report(name, expected, code, out, assertion):
    verdict = 'FIRED' if code == expected else 'SILENT'
    results.append((verdict, name, expected, code))
    print(f'\n----- {name}')
    print(f'  precondition asserted: {assertion}')
    print(out.rstrip())
    print(f'  exit={code} expected={expected}  -> {verdict}')


def digest(path):
    with open(path, 'rb') as handle:
        return hashlib.sha256(handle.read()).hexdigest()


def main():
    original = digest(PAGE)
    keep = os.path.join(tempfile.mkdtemp(), 'TutorialFirstCommand.md')
    shutil.copy2(PAGE, keep)
    assert digest(keep) == original, 'the copy aside is not the file'
    print(f'copy taken aside at {keep}')
    print(f'sha256 {original}')

    # ---------------------------------------------------------------- BASELINE
    found = versioncheck.pins(REL)
    assert found, 'baseline precondition: the page must pin something'
    code, out = run([])
    report('BASELINE (unmutated tree)', 0, code, out,
           f'{len(found)} pin(s) present: '
           + ', '.join(f'{p}@{v} line {n}' for n, p, v in found))
    if code != 0:
        print('\nBASELINE IS RED -- stop. Every result below is untrustworthy.')
        return 1

    text = open(PAGE, encoding='utf-8').read()

    # -------------------------------------------------------------- 1. STALE PIN
    try:
        open(PAGE, 'w', encoding='utf-8').write(
            text.replace('--version 10.7.0', '--version 9.0.0'))
        # The branch rejects a pin whose version differs from the current one.
        # Assert that is what now exists -- not merely that the file changed.
        found = versioncheck.pins(REL)
        current, why = versioncheck.nuget_latest('Paramore.Brighter')
        assert current, f'cannot establish the current version: {why}'
        behind = [(n, p, v) for n, p, v in found if v != current]
        assert behind, ('mutation did not land: no pin differs from '
                        f'{current}')
        code, out = run([])
        report('1. STALE PIN', 1, code, out,
               f'{len(behind)} pin(s) now behind {current}: '
               + ', '.join(f'line {n} {p}@{v}' for n, p, v in behind))
    finally:
        shutil.copy2(keep, PAGE)
        assert digest(PAGE) == original, 'restore after 1 was not byte-identical'

    # ------------------------------------------------------------- 2. NO PINS
    try:
        stripped = text.replace('--version 10.7.0', '')
        stripped = stripped.replace(' Version="10.7.0"', '')
        open(PAGE, 'w', encoding='utf-8').write(stripped)
        # The branch rejects a page that EXISTS and pins NOTHING. Both halves
        # have to hold, and the file-exists half is the one a careless probe
        # would break by deleting the page instead.
        assert os.path.isfile(PAGE), 'mutation deleted the page; wrong branch'
        found = versioncheck.pins(REL)
        assert found == [], f'mutation did not land: {len(found)} pin(s) remain'
        code, out = run([])
        report('2. PAGE EXISTS, PINS NOTHING', 1, code, out,
               'page present on disk and pins() returns []')
    finally:
        shutil.copy2(keep, PAGE)
        assert digest(PAGE) == original, 'restore after 2 was not byte-identical'

    # ------------------------------------------- 3. AUTHORITY UNREACHABLE (no fallback)
    # The tree is untouched here; what is mutated is the authority's address,
    # so urlopen and every error path below it are the shipped ones.
    good_url = versioncheck.NUGET_INDEX
    try:
        versioncheck.NUGET_INDEX = (
            'https://nuget.invalid.localhost.example/{id}/index.json')
        version, why = versioncheck.nuget_latest('Paramore.Brighter')
        assert version is None, ('mutation did not land: the authority is '
                                 'still answering')
        assert versioncheck.pins(REL), ('precondition: there must be a pin to '
                                        'go and check, or the run never asks')
        code, out = run([])
        report('3. AUTHORITY UNREACHABLE, no --release-notes', 2, code, out,
               f'nuget_latest() returns None ({why})')

        # ------------------------------- 4. same, WITH the offline fallback
        notes = os.path.join(os.path.dirname(keep), 'release_notes.md')
        open(notes, 'w', encoding='utf-8').write('## Master\n\n## 10.7.0\n')
        parsed, why = versioncheck.release_notes_latest(notes)
        assert parsed == '10.7.0', f'fixture did not parse: {parsed} / {why}'
        code, out = run(['--release-notes', notes])
        report('4. AUTHORITY UNREACHABLE, --release-notes supplies 10.7.0',
               0, code, out,
               'NuGet still unreachable AND the fallback parses as 10.7.0')

        # ------------------------- 5. fallback disagrees -> reported, not resolved
        open(notes, 'w', encoding='utf-8').write('## Master\n\n## 10.6.0\n')
        parsed, _ = versioncheck.release_notes_latest(notes)
        assert parsed == '10.6.0', f'fixture did not parse: {parsed}'
        versioncheck.NUGET_INDEX = good_url
        current, _ = versioncheck.nuget_latest('Paramore.Brighter')
        assert current != parsed, ('precondition: the two authorities must '
                                   'actually disagree')
        code, out = run(['--release-notes', notes])
        report('5. AUTHORITIES DISAGREE (NuGet reachable)', 0, code, out,
               f'NuGet says {current}, the fixture says {parsed}')
        assert 'AUTHORITIES DISAGREE' in out, (
            'exit 0 is only correct if the disagreement was REPORTED')
    finally:
        versioncheck.NUGET_INDEX = good_url

    assert digest(PAGE) == original, 'the page did not survive the probe'
    print(f'\npage restored byte-identical: sha256 {digest(PAGE)}')

    print('\n===== SUMMARY =====')
    for verdict, name, expected, code in results:
        print(f'  {verdict:6}  expected {expected}, got {code}  {name}')
    silent = [r for r in results if r[0] == 'SILENT']
    print(f'\n{len(results) - len(silent)}/{len(results)} branches fired.')
    return 1 if silent else 0


if __name__ == '__main__':
    sys.exit(main())
