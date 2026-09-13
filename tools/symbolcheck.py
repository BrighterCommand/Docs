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

Usage:
    python3 tools/symbolcheck.py                    # gate: the whole of contents/
    python3 tools/symbolcheck.py contents/X.md      # specific pages

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
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

# Not a second copy of the banner vocabulary. pagelint owns APPLIES_TO and
# CLAUDE.md documents that tuple; restating it here is how the two drift.
from pagelint import APPLIES_TO, BANNER_RE, products_named   # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PAGES_DIR = 'contents'
WATCHLIST = os.path.join(ROOT, 'tools', 'symbolwatch.tsv')

COLUMNS = ('symbol', 'product', 'replacement', 'evidence', 'first_seen')
PRODUCTS = ('brighter', 'darker', 'both')

# The banner names products as `Brighter` / `Darker`; the watchlist names them in
# lower case. One mapping, in one place.
BANNER_PRODUCT = {'brighter': 'Brighter', 'darker': 'Darker'}

IDENT = re.compile(r'[A-Za-z0-9_]')


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


def scan(rel, entries):
    """Every (entry, lineno, text) hit on one page."""
    with open(os.path.join(ROOT, rel), encoding='utf-8') as fh:
        lines = fh.read().splitlines()

    products = page_products(lines)
    in_scope = [e for e in entries if e.applies_to(products)]
    if not in_scope:
        return []

    hits = []
    for lineno, line in enumerate(lines, 1):
        for entry in in_scope:
            if entry.pattern.search(line):
                hits.append((entry, lineno, line.strip()))
    return hits


def _clip(text, width=100):
    return text if len(text) <= width else text[:width - 1] + '…'


def main(argv):
    for arg in argv:
        if arg.startswith('-'):
            print(f'unknown option: {arg}', file=sys.stderr)
            return 2

    try:
        entries = load_watchlist()
    except ValueError as exc:
        print(f'watchlist unusable: {exc}', file=sys.stderr)
        return 2

    everything = md_files()
    if argv:
        pages = []
        missing = []
        for raw in argv:
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

    findings = []
    for rel in pages:
        for entry, lineno, text in scan(rel, entries):
            findings.append((entry, rel, lineno, text))

    if not findings:
        print(f'No watchlisted symbols found '
              f'({len(entries)} entries, {len(pages)} pages checked).')
        return 0

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

    affected = sorted({f[1] for f in findings})
    print(f'\n{len(findings)} site(s) across {len(affected)} page(s), '
          f'from {len(entries)} watchlist entries over {len(pages)} pages.')
    return 1


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
