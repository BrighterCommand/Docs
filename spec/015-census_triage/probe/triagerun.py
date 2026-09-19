"""Run the triage method of `triage.md` over the census, one row per name.

Run from the repository root:

    python3 spec/015-census_triage/probe/triagerun.py --controls-only   # 30s
    python3 spec/015-census_triage/probe/triagerun.py --limit 20        # a pilot
    python3 spec/015-census_triage/probe/triagerun.py                   # all 819

Phase 2 writes this; phase 3 runs it. It emits, for every census candidate, the
evidence `triage.md` §2 defines and the verdict §1 defines: page-spread, stage-1
counts per product, stage-2 counts per product, live counts at both refs of both
products, and the stage-2.5 dot evidence.

THREE THINGS ABOUT THIS SCRIPT ARE LOAD-BEARING.

1. IT QUERIES `CENSUS_PINS`, NOT `origin/master`. `design.md` §4.3's queries all
   name `origin/master`; they were written when the pin and the branch were the
   same SHA, and by 2026-09-18 they were not. A nomination taken at one SHA and a
   verdict taken at another are two claims about two worlds -- standing
   obligation 9. The pin lives in `tools/symbolcheck.py` and is imported here.

2. NOTHING GOES THROUGH A SHELL. Every query is a subprocess argument list.
   `triage.md` §3.2 has the measurement: under zsh, `$name[` is an ARRAY
   SUBSCRIPT, so the parameterised bracket-class form dies with a math error,
   writes nothing to stdout, and a loop reading only the count sees 0 for every
   name including the positive control. Plausible zero number nine, and the
   literal form in `design.md` is immune to it, which is why it survived review.

3. A FAILED GIT INVOCATION IS AN ERROR, NOT A ZERO. `git()` below checks the
   return code. The whole method rests on `0 commits == never existed`, so a
   query that could not run must never be readable as a query that found nothing.

CHECKPOINTED, because stage 1 over the census is ~7 minutes and stage 2 another
~10, and a run that loses its output to a timeout is a run nobody repeats. Rows
are appended to a JSONL file and fsynced one at a time; re-running skips every
name already in it. `--restart` truncates it, and says so.

CONTROLS, per standing obligation 3, printed on every run and asserted before any
row is written:

    positive   IAmACommandStoreAsync  -> EXISTED, REMOVED   (planted)
    negative   IAmAMessageScheduler   -> LIVE
    negative   OrderId                -> NEVER EXISTED
    positive   UseExternalInbox       -> EXISTED, REMOVED   (planted)

and friction 44 -- CAN EACH CONTROL PASS AT ALL -- is checked two ways rather
than assumed. The three verdicts must come out DISTINCT, so a classifier stuck on
one value cannot pass all three; and both planted names must be ABSENT from the
census, because a plant that has become a candidate is no longer a case from
outside the corpus and stops testing the mechanism.
"""
import argparse
import json
import os
import re
import subprocess
import sys
import time

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'tools'))

import symbolcheck as sc  # noqa: E402

SPEC_DIR = os.path.join(ROOT, 'spec', '015-census_triage')
CHECKPOINT = os.path.join(SPEC_DIR, 'triage-checkpoint.jsonl')

LIVE = 'LIVE'
REMOVED = 'EXISTED, REMOVED'
NEVER = 'NEVER EXISTED'

# Planted into the input, from outside the corpus. Both are watchlist rows: 014
# repaired every site, so neither is a census candidate -- which is what makes
# them a test of the instrument rather than a sample of the corpus. See
# triage.md 4.2.
PLANTED = ('IAmACommandStoreAsync', 'UseExternalInbox')

# (label, name, required verdict). The first is also a plant; it appears in both
# lists deliberately, because it is doing two jobs -- it is the positive half of
# the two-way control AND one of the two names that show the run could classify
# anything at all.
CONTROLS = (
    ('positive', 'IAmACommandStoreAsync', REMOVED),
    ('negative', 'IAmAMessageScheduler', LIVE),
    ('negative', 'OrderId', NEVER),
    ('positive', 'UseExternalInbox', REMOVED),
)

# A commit boundary in the -p output. \x01 cannot appear at the start of a line
# of C# diff text, which `@@@` and a bare SHA both can -- a combined diff opens
# with `@@@` and `index 1234567..89abcde` starts with lowercase hex.
MARK = '\x01'

# Distinct evidence lines kept per name per product. Generous on purpose: the
# `dotted`/`bare` counts beside it are taken over ALL matches, so a reader can
# always tell whether the lines shown are the whole set. Four was the first
# value and it was wrong for the reason stage2() records.
EVIDENCE_CAP = 40


class TriageError(Exception):
    """The instrument is wrong, so its rows mean nothing."""


def git(repo, args, allow=(0,)):
    """`git -C repo args`, or TriageError. NEVER returns empty on failure.

    The method's whole asymmetry is that zero commits means never-existed. A
    query that could not run has to be distinguishable from a query that ran and
    found nothing, and the only thing that distinguishes them is this check.

    `allow` exists because the two commands disagree about what 1 means, and the
    first run of this script found it: `git log` exits 0 whether or not the
    pickaxe matched, so 1 is a real failure; `git grep` exits 1 for NO MATCHES,
    which is the live query's commonest and most informative answer. One set of
    accepted codes for both would either swallow a broken `log` or refuse every
    absent name -- and an absent name is what this method is looking for.

    `errors='replace'` IS NOT TIDINESS, and phase 3 added it 597 names into the
    first full run. `git log -p` emits the bytes that are in the tree, and
    src/Paramore.Brighter.MessagingGateway.RESTMS/RestMsMessageConsumer.cs
    carried a Latin-1 (c) in its 2014 licence header before it was deleted, so
    `text=True` raised UnicodeDecodeError mid-corpus and took the process with
    it. Replacing the undecodable byte cannot move a verdict: TOKEN_RE nominates
    ASCII identifiers only, and word_re() matches ASCII only, so every byte this
    touches is one no query could ever have matched. The control is in tasks.md
    -- the crashing name against a byte-level count, and a clean name against
    the row the pre-fix run had already written for it.
    """
    path = os.path.join(ROOT, repo)
    proc = subprocess.run(['git', '-C', path] + args,
                          capture_output=True, text=True, errors='replace')
    if proc.returncode not in allow:
        raise TriageError(f'git -C {repo} {" ".join(args[:3])}...: '
                          f'exit {proc.returncode}: '
                          f'{proc.stderr.strip()[:160] or "no stderr"}')
    return proc.stdout


def word_re(name):
    """`name` as a whole identifier, for reading a diff line."""
    return re.compile(r'(?<![A-Za-z0-9_])' + re.escape(name) + r'(?![A-Za-z0-9_])')


def bracket_pattern(name):
    """Stage 2's query form. NOT `\\b` -- see triage.md 3."""
    return f'[^A-Za-z0-9_]{name}[^A-Za-z0-9_]'


def refs():
    """{product: (repo, pin_sha, tag_sha)}, every SHA resolved here and printed.

    The pin is CENSUS_PINS -- census-scoped, so this reads the same world the
    candidate list was nominated from. The tag comes from PRODUCT_REFS and is
    used for the LIVE test only: triage.md 1 defines LIVE as resolving at EITHER
    ref of EITHER product, which is the same four refs the census cleared the
    name against. History is queried at the pin alone, because the release tag is
    an ancestor of it and its history is a subset.
    """
    out = {}
    for product, (repo, (tag, _head)) in sorted(sc.PRODUCT_REFS.items()):
        out[product] = (repo,
                        sc.resolve_sha(repo, sc.CENSUS_PINS[product]),
                        sc.resolve_sha(repo, tag))
    return out


def stage1(repo, sha, name):
    """Plain -S. A zero is conclusive; a non-zero means only "go to stage 2"."""
    out = git(repo, ['log', '-S', name, '--oneline', sha, '--', 'src/*.cs'])
    return len([l for l in out.splitlines() if l.strip()])


def stage2(repo, sha, name):
    """The bracket-class form, with stage 2.5's diff lines from the same query.

    Returns (commits, dotted, bare, samples). `-p` costs no more than `--oneline`
    -- measured 5.4s either way -- so the evidence is free where the count is not.

    KEEP THE WHOLE EVIDENCE SET, deduplicated, up to a generous cap. The design's
    stage-3 reading of the seven head survivors took `head -1` of this output,
    and on `Date` the first line is `Get<T>(DateTime date, …)` -- a parameter
    name -- while the full set also holds `public DateTime Date { get; set; }`,
    a public property of `DynamoDbMessage` removed in 2019. Same name, same
    query, opposite readings, and only the truncation decided which one a person
    saw. A provisional verdict hardens into a wrong one exactly here.
    """
    out = git(repo, ['log', '-S', bracket_pattern(name), '--pickaxe-regex',
                     f'--format={MARK}%H', '-p', sha, '--', 'src/*.cs'])
    commits = 0
    dotted = bare = 0
    samples = []
    rx = word_re(name)
    for line in out.splitlines():
        if line.startswith(MARK):
            commits += 1
            continue
        if line[:1] not in '+-' or line.startswith(('+++', '---')):
            continue
        for match in rx.finditer(line):
            # Stage 2.5, evidence and never a filter: a dot in front is a call on
            # somebody else's type. A Brighter extension method is called with a
            # dot too, which is exactly why this is printed, not applied.
            if match.start() and line[match.start() - 1] == '.':
                dotted += 1
            else:
                bare += 1
            # Window the sample ON THE MATCH, not on the start of the line. The
            # first run truncated at 110 characters and printed a line whose
            # only visible occurrence was `IAmAMessageSchedulerAsync` -- a
            # different name -- while the match that produced the row sat off
            # the right-hand edge. Evidence that does not show the thing it is
            # evidence of invites exactly the wrong reading.
            start = max(0, match.start() - 30)
            clip = ('…' if start else '') + line[start:start + 110].rstrip()
            if clip not in samples:
                samples.append(clip)
    return commits, dotted, bare, samples[:EVIDENCE_CAP]


def live(repo, sha, name):
    """Does it resolve at this ref today? -lwF, safe because name is an identifier.

    `-w` tests the characters adjacent to the match, so it is right exactly when
    the pattern begins and ends with word characters. Every census candidate is
    an identifier; a watchlist row such as `.AddPolicies(` is not, and that is
    the case symbolcheck.word_pattern() exists for.
    """
    out = git(repo, ['grep', '-lwF', name, sha, '--', 'src/*.cs'], allow=(0, 1))
    return len([l for l in out.splitlines() if l.strip()])


def classify(row):
    """triage.md 1's three values, in the order the evidence forecloses them."""
    if any(row['live'][p] for p in row['live']):
        return LIVE
    if any(row['s2'].get(p, 0) for p in row['s1']):
        return REMOVED
    return NEVER


def triage_name(name, hits, resolved):
    """One row: every count, both products, with the seconds it took."""
    started = time.time()
    row = {'name': name,
           'pages': len(hits or {}),
           'sites': sum((hits or {}).values()),
           's1': {}, 's2': {}, 'live': {},
           'dotted': {}, 'bare': {}, 'evidence': {}}
    for product, (repo, pin, tag) in resolved.items():
        row['live'][product] = live(repo, pin, name) + live(repo, tag, name)
        row['s1'][product] = stage1(repo, pin, name)
    for product, (repo, pin, _tag) in resolved.items():
        if not row['s1'][product]:
            continue                      # stage 1's zero is conclusive
        commits, dotted, bare, samples = stage2(repo, pin, name)
        row['s2'][product] = commits
        row['dotted'][product] = dotted
        row['bare'][product] = bare
        if samples:
            row['evidence'][product] = samples
    row['verdict'] = classify(row)
    row['seconds'] = round(time.time() - started, 2)
    return row


def pages():
    out = []
    for base, _, files in os.walk(os.path.join(ROOT, 'contents')):
        for name in sorted(files):
            if name.endswith('.md'):
                out.append(os.path.relpath(os.path.join(base, name), ROOT))
    return sorted(out)


def census_candidates():
    """The census's own unresolved set, reusing its code rather than copying it.

    perpageprobe.py copies census() on purpose -- it had to measure a change
    before the change shipped. This does not: P0-1 is merged, so the shipped tool
    IS the specification, and a copy here could only drift away from it.
    """
    known = set().union(*sc.universe().values())
    candidates, _counts = sc.census(pages())
    out = {}
    for symbol, hits in candidates.items():
        if symbol in known:
            continue
        if symbol.endswith('Attribute') or (symbol + 'Attribute') in known:
            continue
        out[symbol] = hits
    return out


def fmt(row):
    def per(key):
        return '/'.join(str(row[key].get(p, 0)) for p in ('brighter', 'darker'))
    dots = f"{per('dotted')} dotted, {per('bare')} bare" if row['s2'] else ''
    return (f"{row['name']:<34} {row['pages']:>3}p {row['sites']:>4}s  "
            f"s1 {per('s1'):>8}  s2 {per('s2'):>6}  live {per('live'):>6}  "
            f"{row['verdict']:<17} {dots}")


def run_controls(resolved, candidates):
    """Print the controls with their raw evidence, and check they CAN fail.

    Two checks, not one. A control that passes tells you nothing unless the run
    could have come out otherwise -- friction 44 -- and here there are two ways
    it could not have:

      * a classifier stuck on a single value would pass one control and fail the
        other two, so the three required verdicts must be DISTINCT; and
      * a planted name that had drifted into the census is no longer a case from
        outside the corpus, so it is checked absent before it is trusted.
    """
    print('controls -- triage.md 4.2, run before any row is written:')
    failures = []
    for label, name, want in CONTROLS:
        # The real page-spread, so the printed row says whether the control is
        # in the corpus as well as what it classified as. OrderId is (31 pages);
        # the two plants are not, which is the point of them.
        row = triage_name(name, candidates.get(name), resolved)
        got = row['verdict']
        mark = 'OK  ' if got == want else 'FAIL'
        print(f'  {mark} {label:<9}{name:<24} want {want:<17} got {got:<17} '
              f'[{fmt(row).split("  ", 1)[1].strip()}]')
        for product, lines in sorted(row['evidence'].items()):
            print(f'         {product} history: {lines[0].strip()[:96]}')
        if got != want:
            failures.append(f'{name}: wanted {want}, got {got}')

    wanted = {want for _l, _n, want in CONTROLS}
    print(f'  can they fail? the {len(CONTROLS)} controls require '
          f'{len(wanted)} DISTINCT verdicts: {", ".join(sorted(wanted))}')
    if len(wanted) < 3:
        failures.append('the controls do not require three distinct verdicts, '
                        'so a stuck classifier would pass them')

    for name in PLANTED:
        planted = name not in candidates
        print(f'  {"OK  " if planted else "FAIL"} plant      {name:<24} '
              f'absent from the census, so it is a case from outside the '
              f'corpus: {planted}')
        if not planted:
            failures.append(f'{name} is a census candidate, so it is no longer '
                            f'a plant and tests the corpus rather than the '
                            f'instrument')
    if failures:
        raise TriageError('controls failed: ' + '; '.join(failures))
    print()


def query_forms(resolved):
    """Task 2.3's red-proof: the two forms that return zero for EVERYTHING.

    `triage.md` 3 says the bracket class is not a stylistic preference over
    `\\b`. This runs all four forms against a name that MUST come back non-zero
    and a name the design measured, so the claim is a run rather than a
    recollection -- and so that a later reader tempted to "simplify" the bracket
    class can see what the simplification costs in one command.

    It asserts only the shipped form. Whether git's regex engine grows `\\b`
    support one day is not this script's business; whether the form the method
    actually uses can still find a known-removed API is.
    """
    repo, pin, _tag = resolved['brighter']
    forms = (
        ('-S<name>                    substring',      lambda n: [n], False),
        (r"-S'\b<name>\b'  --pickaxe-regex",           lambda n: [r'\b' + n + r'\b'], True),
        (r"-S'\<<name>\>'  --pickaxe-regex",           lambda n: [r'\<' + n + r'\>'], True),
        ('-S[^A-Za-z0-9_]<name>[…]  --pickaxe-regex',  lambda n: [bracket_pattern(n)], True),
    )
    names = ('IAmACommandStoreAsync', 'Date')
    print(f'query forms at ../Brighter {pin}, on src/*.cs:\n')
    print(f'  {"form":<46}{names[0]:>22}{names[1]:>8}')
    print(f'  {"":<46}{"MUST STAY > 0":>22}{"":>8}')
    results = {}
    for label, build, regex in forms:
        counts = []
        for name in names:
            args = ['log', '-S'] + build(name)
            if regex:
                args.append('--pickaxe-regex')
            args += ['--oneline', pin, '--', 'src/*.cs']
            counts.append(len([l for l in git(repo, args).splitlines() if l.strip()]))
        results[label] = counts
        note = '  <- BROKEN: reads as "never existed", for every name'
        print(f'  {label:<46}{counts[0]:>22}{counts[1]:>8}'
              f'{note if counts[0] == 0 else ""}')

    shipped = results['-S[^A-Za-z0-9_]<name>[…]  --pickaxe-regex']
    print(f'\n  the shipped form finds the positive control: {shipped[0]} > 0')

    # Plausible zero number nine, and the reason this script never opens a
    # shell. The SAME bracket-class form, parameterised into zsh, is an array
    # subscript -- so it dies, writes nothing to stdout, and a caller that reads
    # only the count sees a tidy zero.
    print('\nthe same form through zsh, with the name in a variable:')
    probe = ('name=IAmACommandStoreAsync; '
             f'git -C {os.path.join(ROOT, repo)} log '
             '-S"[^A-Za-z0-9_]$name[^A-Za-z0-9_]" --pickaxe-regex --oneline '
             f'{pin} -- "src/*.cs" | wc -l')
    proc = subprocess.run(['zsh', '-c', probe], capture_output=True, text=True)
    print(f'  stdout (what a `$(...)` capture reads) : {proc.stdout.strip()!r}')
    print(f'  stderr (what nobody looks at)          : '
          f'{proc.stderr.strip()[:96]!r}')
    print(f'  argument list, no shell                : {shipped[0]}')

    if not shipped[0]:
        raise TriageError('the shipped query form returns 0 for the positive '
                          'control, so every verdict it produces would read as '
                          'NEVER EXISTED')
    return 0


def load_checkpoint(path):
    done = {}
    if not os.path.isfile(path):
        return done
    with open(path, encoding='utf-8') as fh:
        for lineno, line in enumerate(fh, 1):
            line = line.strip()
            if not line:
                continue
            try:
                row = json.loads(line)
            except json.JSONDecodeError as exc:
                raise TriageError(f'{path}:{lineno}: unreadable checkpoint row '
                                  f'({exc}). Fix or --restart; a half-written '
                                  f'row silently skipped is a name with no '
                                  f'verdict that reads as a name with one')
            done[row['name']] = row
    return done


def append(path, row):
    """One row, flushed and fsynced. The next name may be the one that times out."""
    with open(path, 'a', encoding='utf-8') as fh:
        fh.write(json.dumps(row, sort_keys=True) + '\n')
        fh.flush()
        os.fsync(fh.fileno())


def summarise(rows):
    counts = {}
    for row in rows:
        counts[row['verdict']] = counts.get(row['verdict'], 0) + 1
    print('\nverdicts:')
    for verdict in (LIVE, REMOVED, NEVER):
        print(f'  {verdict:<18} {counts.get(verdict, 0):>4}')
    print(f'  {"total":<18} {len(rows):>4}')
    interesting = [r for r in rows if r['verdict'] != NEVER]
    if interesting:
        print('\nnot NEVER EXISTED -- every one of these needs stage 3, a person:')
        for row in sorted(interesting, key=lambda r: (-r['pages'], r['name'])):
            print('  ' + fmt(row))


def main(argv):
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument('--checkpoint', default=CHECKPOINT)
    parser.add_argument('--limit', type=int, default=0,
                        help='triage only the first N by page-spread')
    parser.add_argument('--names', default='',
                        help='comma-separated names instead of the census')
    parser.add_argument('--controls-only', action='store_true')
    parser.add_argument('--query-forms', action='store_true',
                        help="task 2.3's red-proof of the two broken forms")
    parser.add_argument('--restart', action='store_true',
                        help='truncate the checkpoint first, loudly')
    parser.add_argument('--report', action='store_true',
                        help='summarise the checkpoint without running anything')
    args = parser.parse_args(argv)

    try:
        resolved = refs()
    except sc.CensusError as exc:
        print(f'triage cannot run: {exc}', file=sys.stderr)
        return 2

    print('history at the pinned master, LIVE at both refs of both products:')
    for product, (repo, pin, tag) in sorted(resolved.items()):
        print(f'  {product:<9} {repo:<12} pin {pin:<12} tag {tag}')
    print()

    if args.query_forms:
        try:
            return query_forms(resolved)
        except TriageError as exc:
            print(f'\nRED-PROOF FAILED: {exc}', file=sys.stderr)
            return 2

    if args.report:
        try:
            done = load_checkpoint(args.checkpoint)
        except TriageError as exc:
            print(f'triage cannot run: {exc}', file=sys.stderr)
            return 2
        print(f'{len(done)} row(s) in {os.path.relpath(args.checkpoint, ROOT)}')
        summarise(list(done.values()))
        return 0

    try:
        candidates = census_candidates()
        print(f'census candidates: {len(candidates)}')
        run_controls(resolved, candidates)
    except (TriageError, sc.CensusError) as exc:
        print(f'\nCONTROLS FAILED -- no row below would mean anything: {exc}',
              file=sys.stderr)
        return 2

    if args.controls_only:
        return 0

    if args.names:
        wanted = [n.strip() for n in args.names.split(',') if n.strip()]
        order = [(n, candidates.get(n)) for n in wanted]
    else:
        order = sorted(candidates.items(),
                       key=lambda kv: (-len(kv[1]), -sum(kv[1].values()), kv[0]))
        if args.limit:
            order = order[:args.limit]

    if args.restart and os.path.isfile(args.checkpoint):
        os.remove(args.checkpoint)
        print(f'--restart: removed {os.path.relpath(args.checkpoint, ROOT)}\n')

    try:
        done = load_checkpoint(args.checkpoint)
    except TriageError as exc:
        print(f'triage cannot run: {exc}', file=sys.stderr)
        return 2
    todo = [(n, h) for n, h in order if n not in done]
    print(f'{len(order)} name(s) to triage, {len(order) - len(todo)} already in '
          f'the checkpoint, {len(todo)} to run\n')

    started = time.time()
    rows = [done[n] for n, _h in order if n in done]
    for index, (name, hits) in enumerate(todo, 1):
        try:
            row = triage_name(name, hits, resolved)
        except TriageError as exc:
            print(f'\nSTOPPED at {name}: {exc}', file=sys.stderr)
            print(f'{len(rows)} row(s) are safe in '
                  f'{os.path.relpath(args.checkpoint, ROOT)}; re-run to resume.',
                  file=sys.stderr)
            return 2
        append(args.checkpoint, row)
        rows.append(row)
        if row['verdict'] != NEVER or index % 25 == 0 or index == len(todo):
            print(f'{index:>4}/{len(todo)}  {fmt(row)}')

    elapsed = time.time() - started
    if todo:
        print(f'\n{len(todo)} name(s) in {elapsed:.0f}s '
              f'= {elapsed / len(todo):.2f}s per name, both products')
    summarise(rows)
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
