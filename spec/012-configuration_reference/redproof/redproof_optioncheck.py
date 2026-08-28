#!/usr/bin/env python3
"""Tasks 2.7, 2.8 and 2.9 -- force tools/optioncheck red, one branch at a time.

    python3 spec/012-configuration_reference/redproof/redproof_optioncheck.py

Requirements §12 AC3 asks for "a red-proof that a changed ctor default is
caught -- NOT merely that the tool runs", AC3b for one on a body-coalesced
default, and AC5 for exit 2 being distinguishable from exit 0. This is all
three, in 009's `redproof_versioncheck.py` shape, and the discipline is that
file's:

  * print a BASELINE first and require it GREEN. A probe that starts at its
    first mutation cannot tell a rule that fires from a tool that is broken.
  * assert the mutation produced *the input the branch rejects*, not merely
    that the text changed. Three of 010's rule-7 proofs reported SILENT and all
    three were the probe's fault.
  * assert the fixture is IN SCOPE before reading any verdict. 010's S2
    red-proof was vacuous twice over for skipping that step, and this tool
    prints its scope before its verdict for exactly this reason.
  * restore from a copy taken aside, never `git checkout --`, and assert the
    restored file is byte-identical.
  * when an assertion disagrees with the exit code, the assertion is the
    suspect.
"""
import hashlib
import os
import shutil
import subprocess
import sys
import tempfile

# Derived from __file__, never hard-coded: urlmap.py shipped a `parents[2]`
# that was correct where it was written and silently wrong one directory later.
# This file lives at spec/012-configuration_reference/redproof/, so the repo is
# three up.
HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(os.path.dirname(os.path.dirname(HERE)))

FIXTURE = os.path.join(HERE, 'fixture_subscription.md')
PROJECT = os.path.join(REPO, 'tools', 'optioncheck')
BIN = os.path.join(PROJECT, 'bin', 'Debug', 'net9.0')
DLL = os.path.join(BIN, 'optioncheck.dll')

results = []


def run(args, dll=DLL):
    """Run the built checker, returning (exit code, stdout + stderr)."""
    done = subprocess.run(['dotnet', dll] + args, capture_output=True, text=True)
    return done.returncode, done.stdout + done.stderr


def report(name, expected, code, out, assertion):
    verdict = 'FIRED' if code == expected else 'SILENT'
    results.append((verdict, name, expected, code))
    print(f'\n----- {name}')
    print(f'  precondition asserted: {assertion}')
    print('  ' + out.rstrip().replace('\n', '\n  '))
    print(f'  exit={code} expected={expected}  -> {verdict}')


def digest(path):
    with open(path, 'rb') as handle:
        return hashlib.sha256(handle.read()).hexdigest()


def read():
    with open(FIXTURE, encoding='utf-8') as handle:
        return handle.read()


def write(text):
    with open(FIXTURE, 'w', encoding='utf-8') as handle:
        handle.write(text)


def mutate(original, old, new):
    """Replace `old` with `new`, asserting the substitution actually happened."""
    assert old in original, f'the fixture does not contain {old!r} -- it has moved'
    text = original.replace(old, new, 1)
    assert text != original, 'the mutation changed nothing'
    write(text)
    assert new in read(), f'the mutated fixture does not contain {new!r}'
    return text


def main():
    build = subprocess.run(['dotnet', 'build', PROJECT, '-v', 'quiet', '--nologo'],
                           capture_output=True, text=True)
    if build.returncode != 0:
        print(build.stdout + build.stderr)
        print('the checker does not build; nothing below would be measuring it')
        return 2

    original = digest(FIXTURE)
    keep = os.path.join(tempfile.mkdtemp(), 'fixture_subscription.md')
    shutil.copy2(FIXTURE, keep)
    assert digest(keep) == original, 'the copy aside is not the file'
    print(f'copy taken aside at {keep}')
    print(f'sha256 {original}')

    text = read()

    # ---------------------------------------------------------------- BASELINE
    code, out = run([FIXTURE])
    assert 'scope: 1 table, 16 rows, 1 type' in out, (
        'baseline precondition: the fixture must be IN SCOPE. Everything below '
        f'is vacuous if it is not, and the scope line says:\n{out}')
    report('0. BASELINE (must be green)', 0, code, out,
           'the run reaches 1 table, 16 rows, 1 type')
    if code != 0:
        print('\nthe baseline is not green; every branch below is measuring '
              'that instead of the rule it names')
        return 1

    try:
        # ------------------------------------------- 1. AC3, a changed default
        # `bufferSize` is a CONSTRUCTOR PARAMETER with a signature default. A
        # checker reading properties alone never sees it, which is the 43% of
        # the surface AC3 exists to keep in scope.
        mutate(text, '| `bufferSize` | `int` | `1` |', '| `bufferSize` | `int` | `10` |')
        code, out = run([FIXTURE])
        report('1. AC3 -- a changed CONSTRUCTOR default', 1, code, out,
               'the table now says bufferSize defaults to 10; the type says 1')
        assert 'WRONG DEFAULT' in out and 'bufferSize' in out, (
            'exit 1 is only correct if the finding NAMES the row')
        write(text)

        # ------------------------------ 2. AC3b, the body-coalesced default
        # THE ONE THAT MATTERS. The signature says `null`; the constructor body
        # assigns 500 ms. A checker reading ParameterInfo documents this option
        # WRONG rather than missing, and a reader has no reason to doubt a wrong
        # default.
        mutate(text, '| `emptyChannelDelay` | `TimeSpan?` | `500 ms` |',
               '| `emptyChannelDelay` | `TimeSpan?` | `null` |')
        code, out = run([FIXTURE])
        report('2. AC3b -- a body-coalesced default written as `null`', 1, code, out,
               'the table now says `null`, which is exactly what the SIGNATURE says')
        assert '500 ms' in out and 'emptyChannelDelay' in out, (
            'exit 1 is only correct if the finding says the value is 500 ms -- '
            'otherwise the tool caught something else about the same row')
        write(text)

        # ---------------------------------- 3. the spelling the reader types
        mutate(text, '| `bufferSize` | `int` |', '| `BufferSize` | `int` |')
        code, out = run([FIXTURE])
        report('3. WRONG SPELLING -- the property, not the parameter', 1, code, out,
               'the table now writes the PROPERTY spelling, `BufferSize`')
        assert 'WRONG SPELLING' in out, (
            'requirements §7.1 calls this a real hazard: on a constructor-driven '
            'type the reader SETS one spelling and READS BACK the other')
        write(text)

        # ------------------------------------- 4. an escape with no reason
        mutate(text, 'omit: channelFactory — not reader-set; supplied by AddConsumers',
               'omit: channelFactory')
        code, out = run([FIXTURE])
        report('4. ESCAPE WITHOUT REASON', 1, code, out,
               'the marker now carries a bare `omit: channelFactory`')
        assert 'ESCAPE WITHOUT REASON' in out, (
            'both keys DECLARE rather than silence (design §5.1); a parser that '
            'accepts a reasonless `omit:` lets a green build be written over the '
            'hard half of every table')
        write(text)

        # --------------------------------------------- 5. the type is gone
        # Phase 1 found two of these in design §7 -- `RocketMqSubscription` and
        # `MQTTMessagingGatewayConfiguration` name types that do not exist -- and
        # this is the branch that would have caught them.
        mutate(text, '<!-- optioncheck: Paramore.Brighter.Subscription',
               '<!-- optioncheck: Paramore.Brighter.SubscriptionThatWasRenamed')
        code, out = run([FIXTURE])
        report('5. THE TYPE IS GONE', 1, code, out,
               'the marker now names a type no pinned package declares')
        assert 'THE TYPE IS GONE' in out, (
            'a marker binds a fully-qualified type; a renamed type must fail '
            'loudly rather than pass as a table nobody checked')
        write(text)

    finally:
        write(text)

    assert digest(FIXTURE) == original, 'the fixture did not survive the probe'
    print(f'\nfixture restored byte-identical: sha256 {digest(FIXTURE)}')

    # -------------------------------------- 6. AC5, the authority unreachable
    # Exit 2 has to be reachable as a VERDICT rather than as a restore failure:
    # a failed `dotnet restore` never runs the program at all. So the pinned
    # list travels inside the assembly and is checked against what loaded.
    #
    # The control matters as much as the mutation. Copying the output somewhere
    # else is itself a change, and without the control an exit 2 from the copy
    # would prove nothing about the missing package.
    staging = tempfile.mkdtemp()
    control = os.path.join(staging, 'control')
    removed = os.path.join(staging, 'removed')
    shutil.copytree(BIN, control)
    shutil.copytree(BIN, removed)

    code, out = run([FIXTURE], dll=os.path.join(control, 'optioncheck.dll'))
    report('6a. AC5 CONTROL -- the same output directory, nothing removed', 0, code, out,
           f'the checker was copied to {control} and run from there')

    victim = os.path.join(removed, 'Paramore.Brighter.MessagingGateway.Kafka.dll')
    assert os.path.exists(victim), 'the assembly to remove is not there to remove'
    os.remove(victim)
    assert not os.path.exists(victim), 'the assembly is still there'

    code, out = run([FIXTURE], dll=os.path.join(removed, 'optioncheck.dll'))
    report('6b. AC5 -- one pinned package removed', 2, code, out,
           'Paramore.Brighter.MessagingGateway.Kafka.dll deleted from the copy')
    assert 'AUTHORITY UNREACHABLE' in out and 'Kafka' in out, (
        'exit 2 is only correct if the message NAMES THE AUTHORITY rather than '
        'the symptom -- "authority unreachable is not a pass" is the whole point')
    shutil.rmtree(staging)

    print('\n===== SUMMARY =====')
    for verdict, name, expected, code in results:
        print(f'  {verdict:6}  expected {expected}, got {code}  {name}')
    silent = [r for r in results if r[0] == 'SILENT']
    print(f'\n{len(results) - len(silent)}/{len(results)} branches fired.')
    return 1 if silent else 0


if __name__ == '__main__':
    sys.exit(main())
