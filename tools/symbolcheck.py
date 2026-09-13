#!/usr/bin/env python3
"""Check the documentation for API names known to be dead.

This is a CLOSED-WORLD check. It reads `tools/symbolwatch.tsv` -- a curated list
of names that do not exist in a released product -- and reports every place one
of them appears under contents/. It does not, and cannot, tell you whether a
name you have not listed is real.

That is a deliberate inversion of what spec 014 set out to build. The open-world
version -- resolve every symbol in the docs against the products' source and
report the ones that do not resolve -- was measured before it was written: 831
unresolved symbols across 127 of 144 fenced pages, dominated by the docs' own
example domain (`CustomerId` on 17 pages, `ProcessOrderCommand` on 10), which no
cheap filter separates from a real API name. A check that cannot go green cannot
be a gate. See spec/014-documentation_workflow/design.md §3-4.

What the closed world buys is that the check is safe in PROSE as well as code.
A census over prose is unusable -- every capitalised English word is a candidate,
and 160 of 161 pages carry one. A *curated* name has no false positives at all,
so this searches the whole page. That matters: of the five entries shipped with
this tool, two appear only in prose, and a code-context check would miss them.

Matching is whole-word, and that is load bearing rather than tidy. The corpus
carries `IMessageScheduler` (dead) and `IMessageSchedulerFactory` (dead, and a
DIFFERENT name with a different replacement). A substring match reports the
second under the first and offers the wrong fix; the two were miscounted as one
symbol for the whole of this spec's design phase. Entries need not be
identifiers -- `.AddPolicies(` is a legitimate row -- so word boundaries are
applied only at ends that actually have one.

Product matters too. A symbol can be dead in Brighter and alive in Darker, so
each row names its product and a row is only reported on pages whose banner
admits that product. The banner is read with pagelint's own vocabulary, not a
second copy of it.

A page that names a dead symbol on purpose -- a migration guide, or a sentence
saying the thing does not exist -- opts out one symbol at a time:

    <!-- symbolcheck: allow IMessageScheduler -->

PER SYMBOL, never per page. A page-wide silence would let an opt-out written for
a name the page discusses deliberately cover a second, unrelated dead name that
arrived on that page later, and nothing would report it. Opt-outs that suppress
nothing, or name a symbol no longer on the list, are printed as warnings; they
do not fail the build, because debt that fails a build gets deleted rather than
understood.

Usage:
    python3 tools/symbolcheck.py                    # gate: the whole of contents/
    python3 tools/symbolcheck.py contents/X.md      # specific pages
    python3 tools/symbolcheck.py --census           # the open-world report, NOT a gate
    python3 tools/symbolcheck.py --verify-list      # is every row still dead?

--census and --verify-list read ../Brighter and ../Darker. Nothing in the
`check` job of .github/workflows/docs.yml checks either of them out -- every
tool in that job reads this repository alone -- so --verify-list belongs to the
scheduled `versions` job, for the reason that job already states: the event
that invalidates a pin is a release in another repository.

Exit code is 1 when any listed symbol is found, 0 when clean, and 2 when the
arguments or the watchlist are unusable -- the same contract linkcheck.py,
pagelint.py and urlmap.py share. A malformed or empty watchlist is exit 2 rather
than 1 on purpose: the tool has nothing to say about the corpus, and exiting 1
would say the corpus is red.

A green run prints the number of entries it checked. A gate that silently
degrades to zero entries passes every corpus ever written, which is the failure
this spec's probe met twice in one session in another disguise.
"""
import os
import re
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

# Not a second copy of the banner vocabulary. pagelint owns APPLIES_TO and
# CLAUDE.md documents that tuple; restating it here is how the two drift.
#
# FENCE_RE is imported for the same reason and one sharper one. 141 C# fences
# across 39 pages are written "``` csharp" with a space, and pagelint's regex is
# the only one in this repository that has always handled them. The 014 probe
# wrote its own without the space, silently skipped every one of those fences,
# and reported a corpus containing a KNOWN dead symbol as clean. Never write a
# second fence regex.
from pagelint import (                                        # noqa: E402
    APPLIES_TO, BANNER_RE, CSHARP_TAGS, FENCE_RE, products_named)

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PAGES_DIR = 'contents'
WATCHLIST = os.path.join(ROOT, 'tools', 'symbolwatch.tsv')

# Where a product's source lives and which refs a symbol is resolved at. Both
# refs, always: a name absent from the pinned release and present on master is
# FORTHCOMING, which is a different thing from dead and must not be listed as
# dead. The tag is the release the documentation is written against, so it moves
# with pagelint's APPLIES_TO -- when that says Brighter V11, this says the V11
# tag. It lives here and nowhere else.
#
# The paths are siblings of this repository, checked out by whoever runs the
# command. Nothing in .github/workflows/docs.yml's `check` job has them, which
# is why neither --census nor --verify-list runs there.
PRODUCT_REFS = {
    'brighter': ('../Brighter', ('10.7.0', 'origin/master')),
    'darker': ('../Darker', ('4.1.1', 'origin/master')),
}

# A PascalCase-ish word, four characters or more. Three would admit `Add`, `Get`
# and every acronym in the corpus; the probe measured this threshold and kept it.
TOKEN_RE = re.compile(r'[A-Z][A-Za-z0-9_]{3,}')

COLUMNS = ('symbol', 'product', 'replacement', 'evidence', 'first_seen')
PRODUCTS = ('brighter', 'darker', 'both')

# The banner names products as `Brighter` / `Darker`; the watchlist names them in
# lower case. One mapping, in one place.
BANNER_PRODUCT = {'brighter': 'Brighter', 'darker': 'Darker'}

IDENT = re.compile(r'[A-Za-z0-9_]')

# One symbol per comment, on its own line -- see opt_outs().
OPT_OUT_RE = re.compile(r'^<!--\s*symbolcheck:\s*allow\s+(\S.*?)\s*-->$')
OPT_OUT_EXAMPLE = '<!-- symbolcheck: allow IMessageScheduler -->'


class Entry:
    """One watchlist row."""

    def __init__(self, symbol, product, replacement, evidence, first_seen, lineno):
        self.symbol = symbol
        self.product = product
        self.replacement = replacement
        self.evidence = evidence
        self.first_seen = first_seen
        self.lineno = lineno
        self.pattern = word_pattern(symbol)

    def applies_to(self, products):
        """Is this row in scope for a page whose banner names `products`?

        An unreadable or missing banner means the page declares no product, and
        a page that declares nothing is checked against everything. Silence is
        not a claim to be exempt -- rules 1 and 2 of pagelint are what make a
        page say what it applies to, and this tool does not get to let a page
        escape one check by failing another.
        """
        if self.product == 'both' or not products:
            return True
        return BANNER_PRODUCT[self.product] in products


def word_pattern(symbol):
    """Match `symbol` as a whole word, at the ends where "word" means anything.

    `\\b` cannot be used unconditionally: a row may be a call fragment such as
    `.AddPolicies(`, whose first and last characters are not word characters, so
    a leading or trailing `\\b` would require a word character next to them and
    the row would never match anything.
    """
    prefix = r'(?<![A-Za-z0-9_])' if IDENT.match(symbol[0]) else ''
    suffix = r'(?![A-Za-z0-9_])' if IDENT.match(symbol[-1]) else ''
    return re.compile(prefix + re.escape(symbol) + suffix)


def load_watchlist(path=WATCHLIST):
    """Read the TSV. Raises ValueError with a line number on anything malformed.

    Blank lines and `#` comments are skipped -- the file carries its own
    contract in a comment header, which pagetypes.tsv does not, because this one
    is hand-edited by whoever finds the next dead name.
    """
    if not os.path.isfile(path):
        shown = os.path.relpath(path, ROOT)
        raise ValueError(f'no watchlist at {path if shown.startswith("..") else shown}')

    entries = []
    seen = {}
    header_done = False
    with open(path, encoding='utf-8') as fh:
        for lineno, raw in enumerate(fh, 1):
            line = raw.rstrip('\n')
            if not line.strip() or line.lstrip().startswith('#'):
                continue
            fields = line.split('\t')
            if not header_done:
                if tuple(f.strip() for f in fields) != COLUMNS:
                    raise ValueError(
                        f'{path}:{lineno}: header must be '
                        + ' · '.join(COLUMNS) + f', got: {line!r}')
                header_done = True
                continue
            if len(fields) != len(COLUMNS):
                raise ValueError(
                    f'{path}:{lineno}: expected {len(COLUMNS)} tab-separated '
                    f'fields, got {len(fields)}: {line!r}')
            symbol, product, replacement, evidence, first_seen = (
                f.strip() for f in fields)
            if not symbol:
                raise ValueError(f'{path}:{lineno}: empty symbol')
            if product not in PRODUCTS:
                raise ValueError(
                    f'{path}:{lineno}: product must be one of '
                    + ', '.join(PRODUCTS) + f', got {product!r}')
            if not replacement:
                raise ValueError(
                    f'{path}:{lineno}: {symbol} has no replacement. Write the '
                    'live name, or say what happened to it -- a reader who hits '
                    'this gate needs somewhere to go')
            if symbol in seen:
                raise ValueError(
                    f'{path}:{lineno}: {symbol} is already listed at line '
                    f'{seen[symbol]}')
            seen[symbol] = lineno
            entries.append(
                Entry(symbol, product, replacement, evidence, first_seen, lineno))

    if not header_done:
        raise ValueError(f'{path}: no header row')
    if not entries:
        raise ValueError(
            f'{path}: the watchlist is empty. A green run over an empty list '
            'passes every corpus ever written; it is not evidence of anything')
    return entries


def md_files():
    """Every page under contents/, repo-relative, sorted."""
    directory = os.path.join(ROOT, PAGES_DIR)
    return sorted(
        os.path.join(PAGES_DIR, name)
        for name in os.listdir(directory)
        if name.endswith('.md') and os.path.isfile(os.path.join(directory, name))
    )


def page_products(lines):
    """The products a page's banner names, or None when it names none.

    None and an empty set are different answers and the caller treats them the
    same way today; the distinction is kept because --census will not.
    """
    for line in lines:
        match = BANNER_RE.match(line.strip())
        if match:
            return products_named(match.group(2))
    return None


def opt_outs(lines):
    """{symbol: lineno} for every `<!-- symbolcheck: allow X -->` on its own line.

    The comment is on the same footing as pagelint's
    `<!-- pagelint: allow-serviceactivator -->`: its own line, matched whole.

    It names ONE symbol. That is the whole design of it. A page-wide "skip
    symbolcheck" would let a page opting out of a name it discusses on purpose
    -- "there is no `MsSqlOutboxBuilder`" -- silently opt out of a second, dead
    name that arrived on that page two years later, and nothing would ever say
    so. The symbol is taken as the rest of the comment rather than a word,
    because a watchlist row need not be an identifier: `.AddPolicies(` is one.
    """
    found = {}
    for lineno, line in enumerate(lines, 1):
        match = OPT_OUT_RE.match(line.strip())
        if match:
            found.setdefault(match.group(1), lineno)
    return found


def scan(rel, entries):
    """One page's hits, and what its opt-outs did.

    Returns (hits, suppressed, allowed) where hits and suppressed are lists of
    (entry, lineno, text) and allowed is {symbol: lineno}. Suppressed hits are
    kept rather than dropped so an opt-out that suppresses nothing can be
    reported as the dead weight it is.
    """
    with open(os.path.join(ROOT, rel), encoding='utf-8') as fh:
        lines = fh.read().splitlines()

    allowed = opt_outs(lines)
    products = page_products(lines)
    in_scope = [e for e in entries if e.applies_to(products)]
    if not in_scope:
        return [], [], allowed

    hits, suppressed = [], []
    for lineno, line in enumerate(lines, 1):
        # The opt-out comment names its symbol, so it matches its own pattern.
        # Counting it would report every opt-out as suppressing one site more
        # than it does -- and on a page whose only mention was the comment, an
        # opt-out that suppresses nothing would look used.
        if OPT_OUT_RE.match(line.strip()):
            continue
        for entry in in_scope:
            if entry.pattern.search(line):
                target = suppressed if entry.symbol in allowed else hits
                target.append((entry, lineno, line.strip()))
    return hits, suppressed, allowed


def _clip(text, width=100):
    return text if len(text) <= width else text[:width - 1] + '…'


# --------------------------------------------------------------------------
# --census — the open-world report, which is NOT a gate
# --------------------------------------------------------------------------
#
# This is the half of spec 014 that was going to be the whole of it. Resolve
# every symbol the docs use against the products' source and report what does
# not resolve. Measured before it was built: 831 unresolved across 127 of 144
# fenced pages, and the top of the list is the docs' own example domain --
# OrderId, CustomerId, GreetingEvent, ProcessOrderCommand. No cheap filter
# separates those from a real API name; the ones tried moved the count by under
# a third and none of them reaches zero.
#
# So it is a REPORT, read by a person, and its output is how names reach the
# watchlist. It must never be promoted to a gate without a second measurement
# saying the example-domain problem has gone away.
#
# Sorted by page-spread, because that ordering is what found IMessageScheduler:
# a name on seven pages is far likelier to be a real API than a one-page example
# type, and it sat at 7 among CustomerId at 17 and OrderStatus at 11.

# Declarations a page makes itself. A reader's own example class is not an
# unresolved API; it is the page defining its subject.
DECL_RE = re.compile(
    r'\b(?:class|interface|record|struct|enum)\s+([A-Za-z_][A-Za-z0-9_]*)')

LINE_COMMENT_RE = re.compile(r'//[^\n]*')
BLOCK_COMMENT_RE = re.compile(r'/\*.*?\*/', re.S)
STRING_RE = re.compile(r'@?"(?:[^"\\\n]|\\.|"")*"')

# Deliberately short. The probe over-reported rather than hide a real dead name
# behind an over-eager filter, and a report a person reads can afford noise in a
# way a gate cannot.
NOISE_PREFIX = ('System', 'Microsoft', 'Polly', 'Newtonsoft', 'Xunit', 'Amazon',
                'Azure', 'Google', 'Confluent', 'RabbitMQ', 'Npgsql', 'Oracle')
NOISE_EXACT = frozenset("""
Task ValueTask String Int32 Int64 Guid DateTime DateTimeOffset TimeSpan Boolean Double Decimal
List Dictionary IEnumerable IReadOnlyList IReadOnlyDictionary IList ICollection IDictionary
Exception InvalidOperationException ArgumentException ArgumentNullException NotImplementedException
TimeProvider CancellationToken CancellationTokenSource Console Program Startup Main
IServiceCollection IServiceProvider IHostBuilder IHost IConfiguration ILogger ILoggerFactory
HttpClient HttpContext ControllerBase ActionResult IActionResult DbContext DbContextOptions
JsonSerializer JsonSerializerOptions JsonConvert Encoding Environment Assembly Type Object
True False Null This Base New Return Await Async Public Private Static Void Var
""".split())

# The two-way control, run before any number is read. A control proving only
# absence proves nothing: if the token sets were built wrong, EVERYTHING looks
# unresolved and the absent name still looks absent. The present one is what
# catches that. The probe's first attempt built six empty sets through broken
# shell quoting, which without this check reads as "the corpus is clean".
CONTROL_PRESENT = 'CommandProcessor'
CONTROL_ABSENT = 'IAmAnIbox'


class CensusError(Exception):
    """The instrument is wrong, so its numbers mean nothing."""


def source_tokens(repo, ref):
    """Every PascalCase token in `src/` .cs files at `ref`. Never returns empty.

    src/ ONLY. Darker's SampleMauiTestApp/Resources/Fonts/FluentUI.cs is a
    generated glyph table contributing 1,579 distinct PascalCase names by
    itself -- five times the whole of Darker's real source surface. Include
    tests and samples and a font glyph can vouch for a dead API.
    """
    path = os.path.join(ROOT, repo)
    if not os.path.isdir(os.path.join(path, '.git')):
        raise CensusError(
            f'no checkout at {repo}; --census and --verify-list read the '
            f'products\' source and cannot run without it')
    proc = subprocess.run(
        ['git', '-C', path, 'grep', '-h', '-I', '-o', '-E',
         TOKEN_RE.pattern, ref, '--', 'src/*.cs'],
        capture_output=True, text=True)
    tokens = set(proc.stdout.split())
    if not tokens:
        raise CensusError(
            f'empty token set for {repo}@{ref}: '
            f'{proc.stderr.strip()[:200] or "no output"}. '
            f'A plausible zero is the failure this check exists to avoid -- '
            f'every symbol in the corpus would read as unresolved, and the '
            f'report would call a clean corpus clean for the wrong reason')
    return tokens


def universe():
    """Token sets for every (product, ref), with the controls walked."""
    sets = {}
    for product, (repo, refs) in sorted(PRODUCT_REFS.items()):
        for ref in refs:
            sets[f'{product}@{ref}'] = source_tokens(repo, ref)

    failures = []
    for name, tokens in sorted(sets.items()):
        if name.startswith('brighter') and CONTROL_PRESENT not in tokens:
            failures.append(f'{CONTROL_PRESENT} absent from {name}')
        if CONTROL_ABSENT in tokens:
            failures.append(f'{CONTROL_ABSENT} present in {name}')
    if failures:
        raise CensusError('controls failed: ' + '; '.join(failures))
    return sets


def csharp_blocks(lines):
    """Every C# fenced block body on a page, via pagelint's FENCE_RE."""
    blocks, opener, buf = [], None, []
    for line in lines:
        match = FENCE_RE.match(line)
        if opener is None:
            if match:
                opener = (match.group(1)[0], len(match.group(1)),
                          match.group(2).lower())
                buf = []
            continue
        if (match and match.group(1)[0] == opener[0]
                and len(match.group(1)) >= opener[1] and not match.group(2)):
            if opener[2] in CSHARP_TAGS:
                blocks.append('\n'.join(buf))
            opener = None
            continue
        buf.append(line)
    if opener is not None and opener[2] in CSHARP_TAGS:   # EOF closes it
        blocks.append('\n'.join(buf))
    return blocks


def strip_noncode(body):
    """Comments and string literals out. A name in a comment is not a use."""
    body = BLOCK_COMMENT_RE.sub(' ', body)
    body = STRING_RE.sub(' "" ', body)
    return LINE_COMMENT_RE.sub(' ', body)


def census(pages):
    """{symbol: {page: count}} for candidates, and the stage counts."""
    raw, stripped = set(), set()
    candidates = {}
    fenced = 0
    for rel in pages:
        with open(os.path.join(ROOT, rel), encoding='utf-8') as fh:
            lines = fh.read().splitlines()
        blocks = csharp_blocks(lines)
        if not blocks:
            continue
        fenced += 1
        declared = set()
        for body in blocks:
            declared.update(DECL_RE.findall(body))
        for body in blocks:
            raw.update(TOKEN_RE.findall(body))
            for token in TOKEN_RE.findall(strip_noncode(body)):
                stripped.add(token)
                if (token in declared or token in NOISE_EXACT
                        or token.startswith(NOISE_PREFIX)):
                    continue
                pages_for = candidates.setdefault(token, {})
                pages_for[rel] = pages_for.get(rel, 0) + 1
    return candidates, {'fenced': fenced, 'raw': len(raw),
                        'stripped': len(stripped), 'candidates': len(candidates)}


def run_census(pages):
    """Print the report. Returns 0 on success, 2 when the instrument is wrong.

    NEVER returns 1. This is a report, not a gate, and an exit code that varies
    with what it finds is the first step towards someone wiring it into CI.
    """
    try:
        sets = universe()
    except CensusError as exc:
        print(f'census cannot run: {exc}', file=sys.stderr)
        return 2

    print('token sets, src/ only, both refs per product:')
    for name, tokens in sorted(sets.items()):
        print(f'  {name:28} {len(tokens):>6} tokens')
    print(f'controls OK: {CONTROL_PRESENT} present in every Brighter set, '
          f'{CONTROL_ABSENT} in none\n')

    known = set().union(*sets.values())
    candidates, counts = census(pages)

    unresolved = {}
    for symbol, hits in candidates.items():
        if symbol in known:
            continue
        # C# lets an attribute drop its suffix, so [Handler] is HandlerAttribute.
        if symbol.endswith('Attribute') or (symbol + 'Attribute') in known:
            continue
        unresolved[symbol] = hits

    listed = set()
    try:
        listed = {e.symbol for e in load_watchlist()}
    except ValueError:
        pass                       # the census does not depend on the watchlist

    spread = sorted(unresolved.items(),
                    key=lambda kv: (-len(kv[1]), -sum(kv[1].values()), kv[0]))
    print(f'pages examined                     : {len(pages)}')
    print(f'...with at least one C# fence      : {counts["fenced"]}')
    print(f'distinct tokens in those fences    : {counts["raw"]}')
    print(f'...after comments and strings      : {counts["stripped"]}')
    print(f'...after page declarations, noise  : {counts["candidates"]}')
    print(f'UNRESOLVED at src/ of both products, both refs : {len(unresolved)}')
    pages_hit = {p for hits in unresolved.values() for p in hits}
    print(f'pages carrying at least one        : {len(pages_hit)} '
          f'of {counts["fenced"]}\n')

    print('by page-spread — the ordering that found IMessageScheduler:')
    for symbol, hits in spread:
        names = sorted(hits)
        shown = ', '.join(os.path.basename(p) for p in names[:4])
        if len(names) > 4:
            shown += f', +{len(names) - 4} more'
        mark = '  <- on the watchlist' if symbol in listed else ''
        print(f'{len(names):>4} page(s) {sum(hits.values()):>4} site(s)  '
              f'{symbol:<40} {shown}{mark}')

    print(f'\n{len(unresolved)} candidate(s), every one printed. This is a '
          f'REPORT, not a gate: most of these are the documentation\'s own '
          f'example domain, and triage is a person\'s job. Exit code is 0 '
          f'whatever it found.')
    return 0


# --------------------------------------------------------------------------
# --verify-list — the half that stops the list rotting
# --------------------------------------------------------------------------
#
# A watchlist is a set of claims about another repository, and claims about
# another repository go stale on that repository's schedule, not ours. So every
# row is re-resolved at BOTH of its product's refs, and the four outcomes are
# not all the same:
#
#   absent at the pin, absent at master   DEAD         the row is still true
#   present at both                       LIVE         the name came back
#   absent at the pin, present at master  FORTHCOMING  it lands next release
#   present at the pin, absent at master  WITHDRAWN    the docs' pin still has it
#
# Only the first is acceptable, and the other three are all "remove the row",
# never "repair the row". A gate policing a name that exists is worse than no
# gate: it tells a writer to replace correct text with something else.
#
# The three-state liveness rule (live / forthcoming-and-said-so / a defect) is
# why both refs are read rather than just the pin. A name absent today and
# present on master is FORTHCOMING, and a list that could not tell those apart
# would keep failing a page that is about to be right.

# Replacements that are prose rather than a name -- "(removed at V10)" -- are
# not resolvable and say so by starting with a bracket.
PROSE_REPLACEMENT = '('


def resolve(repo, ref, symbol):
    """How many src/ files at `ref` contain `symbol`. -1 if the ref is unusable.

    -w ONLY when the symbol begins and ends with a word character, and this is
    not a nicety. `git grep -wF '.Handle('` returns 0 files where the same
    search without -w returns 23: a pattern whose last character is not a word
    character can never satisfy the flag. A row like `.AddPolicies(` verified
    with an unconditional -w would report DEAD forever, whatever the truth --
    the same plausible-zero failure this spec has now met three times, in three
    disguises.
    """
    path = os.path.join(ROOT, repo)
    if not os.path.isdir(os.path.join(path, '.git')):
        raise CensusError(
            f'no checkout at {repo}; --verify-list resolves every row against '
            f'the products\' source and cannot run without it')
    flags = ['-l', '-F']
    if IDENT.match(symbol[0]) and IDENT.match(symbol[-1]):
        flags.append('-w')
    proc = subprocess.run(
        ['git', '-C', path, 'grep'] + flags + [symbol, ref, '--', 'src/*.cs'],
        capture_output=True, text=True)
    # git grep exits 1 for "no matches" and >1 for a real failure, which is how
    # a bad ref is told from an honest zero.
    if proc.returncode > 1:
        raise CensusError(
            f'{repo}@{ref}: {proc.stderr.strip()[:160] or "git grep failed"}')
    return len([line for line in proc.stdout.splitlines() if line.strip()])


def verdict(pin_files, master_files):
    if pin_files == 0 and master_files == 0:
        return 'DEAD'
    if pin_files and master_files:
        return 'LIVE'
    if master_files:
        return 'FORTHCOMING'
    return 'WITHDRAWN'


def verify_row(symbol, product):
    """[(product, pin, ref_counts, verdict)] for each product in scope."""
    products = PRODUCTS[:2] if product == 'both' else (product,)
    out = []
    for name in products:
        repo, (pin, head) = PRODUCT_REFS[name]
        counts = (resolve(repo, pin, symbol), resolve(repo, head, symbol))
        out.append((name, pin, head, counts, verdict(*counts)))
    return out


def run_verify_list():
    """Re-resolve every watchlist row. 0 when all dead, 1 when any is not."""
    try:
        entries = load_watchlist()
    except ValueError as exc:
        print(f'watchlist unusable: {exc}', file=sys.stderr)
        return 2

    # The controls go through the same code path the rows do. A control that
    # uses a different query proves that query, not this one.
    try:
        for symbol, expected in ((CONTROL_PRESENT, 'LIVE'),
                                 (CONTROL_ABSENT, 'DEAD')):
            got = verify_row(symbol, 'brighter')[0]
            if got[4] != expected:
                print(f'control failed: {symbol} resolves {got[4]}, '
                      f'expected {expected} ({got[3][0]} files at {got[1]}, '
                      f'{got[3][1]} at {got[2]})', file=sys.stderr)
                return 2
            print(f'control: {symbol:<24} {got[4]:<12} '
                  f'{got[3][0]} files at {got[1]}, {got[3][1]} at {got[2]}')
    except CensusError as exc:
        print(f'--verify-list cannot run: {exc}', file=sys.stderr)
        return 2
    print()

    problems = []
    for entry in entries:
        try:
            rows = verify_row(entry.symbol, entry.product)
        except CensusError as exc:
            print(f'--verify-list cannot run: {exc}', file=sys.stderr)
            return 2
        for name, pin, head, counts, state in rows:
            print(f'{entry.symbol:<28} {name:<9} {state:<12} '
                  f'{counts[0]} at {pin}, {counts[1]} at {head}')
            if state != 'DEAD':
                problems.append((entry, name, state, pin, head, counts))

        if entry.replacement.startswith(PROSE_REPLACEMENT):
            continue
        try:
            repl = verify_row(entry.replacement, entry.product)
        except CensusError as exc:
            print(f'--verify-list cannot run: {exc}', file=sys.stderr)
            return 2
        for name, pin, head, counts, state in repl:
            if state != 'LIVE':
                problems.append(
                    (entry, name, f'REPLACEMENT {state}', pin, head, counts))
                print(f'  -> replacement {entry.replacement} is {state} in '
                      f'{name}: {counts[0]} at {pin}, {counts[1]} at {head}')

    if not problems:
        print(f'\nAll {len(entries)} entries still dead at both refs of their '
              f'product, and every named replacement still live.')
        return 0

    plural = 'entry' if len(problems) == 1 else 'entries'
    print(f'\n===== {len(problems)} {plural} need attention =====')
    for entry, name, state, pin, head, counts in problems:
        if state.startswith('REPLACEMENT'):
            print(f'{entry.symbol}: its replacement {entry.replacement} is '
                  f'{state.split()[1]} in {name}. The gate is telling writers '
                  f'to use a name that is not there — fix the replacement.')
            continue
        print(f'{entry.symbol} ({name}, line {entry.lineno}): {state} — '
              f'{counts[0]} files at {pin}, {counts[1]} at {head}.')
        print('    ' + ADVICE[state])
    return 1


# Remove the row in all three cases, but for three different reasons, and the
# reason is what tells the person reading this what to do about the PAGES.
ADVICE = {
    'LIVE': 'REMOVE THE ROW. The name exists at both refs; policing it tells a '
            'writer to replace correct text.',
    'FORTHCOMING': 'REMOVE THE ROW. Absent at the pin and present on master is '
                   'forthcoming, not dead — a page may name it if it says so, '
                   'with `> **Not in a released package yet.**`',
    'WITHDRAWN': 'REMOVE THE ROW. It exists in the release the documentation '
                 'is pinned to, so the corpus is right for that pin. Re-list it '
                 'after the pin moves past the removal, if it is still wrong.',
}


def main(argv):
    census_mode = False
    verify_mode = False
    paths = []
    for arg in argv:
        if arg == '--census':
            census_mode = True
        elif arg == '--verify-list':
            verify_mode = True
        elif arg.startswith('-'):
            print(f'unknown option: {arg}', file=sys.stderr)
            return 2
        else:
            paths.append(arg)

    if census_mode and verify_mode:
        print('--census and --verify-list are different questions: one reads '
              'the corpus, the other reads the watchlist', file=sys.stderr)
        return 2

    if verify_mode:
        if paths:
            print('--verify-list takes no paths: it resolves the watchlist '
                  'against the products, not against this repository',
                  file=sys.stderr)
            return 2
        return run_verify_list()

    everything = md_files()
    if paths:
        pages = []
        missing = []
        for raw in paths:
            rel = os.path.relpath(os.path.abspath(raw), ROOT)
            if rel in everything:
                pages.append(rel)
            else:
                missing.append(raw)
        if missing:
            print('not a page under contents/: ' + ', '.join(missing),
                  file=sys.stderr)
            return 2
    else:
        pages = everything

    if census_mode:
        return run_census(pages)

    try:
        entries = load_watchlist()
    except ValueError as exc:
        print(f'watchlist unusable: {exc}', file=sys.stderr)
        return 2

    listed = {e.symbol for e in entries}
    findings, silenced, stale = [], [], []
    for rel in pages:
        hits, suppressed, allowed = scan(rel, entries)
        for entry, lineno, text in hits:
            findings.append((entry, rel, lineno, text))
        for entry, lineno, text in suppressed:
            silenced.append((entry, rel, lineno, text))

        used = {e.symbol for e, _, _ in suppressed}
        for symbol, lineno in sorted(allowed.items(), key=lambda kv: kv[1]):
            if symbol not in listed:
                stale.append((rel, lineno, symbol, 'not on the watchlist'))
            elif symbol not in used:
                stale.append((rel, lineno, symbol, 'suppresses nothing here'))

    for entry in entries:
        rows = [f for f in findings if f[0] is entry]
        if not rows:
            continue
        pages_hit = sorted({r[1] for r in rows})
        print(f'\n===== {entry.symbol} — {len(rows)} site(s) '
              f'across {len(pages_hit)} page(s) =====')
        print(f'    {entry.replacement}   [{entry.evidence}, '
              f'listed {entry.first_seen}]')
        for _, rel, lineno, text in rows:
            print(f'{rel}:{lineno}  {_clip(text)}')
        print(f'    To keep one of these on purpose, put '
              f'`<!-- symbolcheck: allow {entry.symbol} -->` on its own line.')

    # Never silent. An opt-out that nobody can see is the same defect as a dead
    # name nobody can see, one level up: the count is printed on a green run
    # too, because "0 findings" and "0 findings, 14 silenced" are not the same
    # claim about the corpus.
    if silenced:
        by_page = sorted({(rel, e.symbol) for e, rel, _, _ in silenced})
        print(f'\n----- silenced by opt-out ({len(silenced)} site(s)) -----')
        for rel, symbol in by_page:
            n = sum(1 for e, r, _, _ in silenced if r == rel and e.symbol == symbol)
            print(f'{rel}  {symbol} ×{n}')

    # Warnings, not errors. A stale opt-out is debt, and debt that fails the
    # build gets deleted rather than understood -- the same argument that keeps
    # pagelint's using-directive rule a counted warning repo-wide.
    if stale:
        print(f'\n----- stale opt-out (warning: {len(stale)}) -----')
        for rel, lineno, symbol, why in stale:
            print(f'{rel}:{lineno}  allow {symbol} — {why}')

    if not findings:
        extra = ''
        if silenced:
            extra += f', {len(silenced)} silenced'
        if stale:
            extra += f', {len(stale)} stale opt-out(s)'
        print(f'\nNo watchlisted symbols found '
              f'({len(entries)} entries, {len(pages)} pages checked{extra}).')
        return 0

    affected = sorted({f[1] for f in findings})
    print(f'\n{len(findings)} site(s) across {len(affected)} page(s), '
          f'from {len(entries)} watchlist entries over {len(pages)} pages.')
    return 1


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
