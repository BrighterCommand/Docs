#!/usr/bin/env python3
"""Check authoring conventions across the documentation pages.

Where linkcheck.py asks "does this link resolve?", this asks "is this page
well-formed?" — for a reader deciding whether they are in the right place, and
for a retrieval system holding one chunk with no surrounding context.

Rules, and what each is for:

  NO H1               a page with no title for a banner to sit under
  EXTRA H1            a second H1: a section heading written a level too high
  BANNER MISSING      no page banner on the first non-blank line after the H1
  BANNER MALFORMED    a banner is there, but not in the fixed grammar
  HEADING NOT UNIQUE  a `##` text that also appears on another page
  HEADING REPEATED    a heading text repeated within one page (H2-H4)
  LANGUAGE TAG        a fenced block with no language
  SERVICEACTIVATOR    "ServiceActivator" in prose where "Dispatcher" is meant
  USING DIRECTIVES    a C# block with no `using` lines (warning, counted;
                      stays a warning under --changed if marked `// ...`)
  SUMMARY MISSING     no prose after the banner to summarise the page with
  SUMMARY TOO LONG    an opening sentence over 200 characters, rendered
  SUMMARY ENDS IN COLON   an opening sentence promising a list an index drops
  SUMMARY NOT A SENTENCE  an opening sentence with no terminal punctuation
  SUMMARY NOT UNIQUE  two pages that introduce themselves identically
  DESCRIPTION MISMATCH    `description:` front matter that is not that sentence
  DESCRIPTION UNREADABLE  front matter this tool will not guess the meaning of

The banner states the page type, the Brighter/Darker version it applies to and
what to read first. It is a visible blockquote rather than front matter because
a retrieval chunker strips front matter but keeps body text.

  One of that sentence's two reasons used to be "because GitBook renders front
  matter literally into the page body", and it was wrong -- or has stopped
  being true, which from here is the same thing. Measured on a live preview
  revision 2026-08-12 (spec 010 Phase 10): front matter is consumed, the H1
  survives, and `description:` becomes the page's meta, og and twitter
  description. There is even a switch, layout.description.visible, for whether
  it also renders as a subtitle. The conclusion is untouched -- the banner
  stays a blockquote, on the surviving reason, and it demonstrably reaches the
  .md variant -- but a premise nobody rechecked for a year was false, so it is
  corrected here rather than left to be inherited again.

Rule 7 is the opening sentence, and it is the one rule here that is not about
form. The other six ask whether a page is well-formed; this asks whether its
first sentence is true and specific enough to stand alone, because that
sentence is what a reader meets as a search snippet and what /llms.txt prints
after the title. It cannot check truth, so it checks the three things that
correlate with a sentence nobody reread -- too long, ends in a colon, or
identical to another page's -- and the third is the one that earns its keep.

Heading uniqueness is two rules with deliberately different scopes. Across
pages it applies to `##` only: an H3 is read under its H2, so
`### Basic Configuration` beneath `## Hangfire Scheduler Configuration` is
perfectly attributable, and 39 such H3 texts repeat legitimately. Within one
page it applies to H2-H4, where a repeat is always a defect — it produces
`#when-to-use`, `#when-to-use-1`, `#when-to-use-2` on chunks that sit next to
each other, anchors no author would choose and no reader can interpret.

Both compare slug(), not the markdown as written, because GitBook strips
emphasis when it builds the anchor: `## **Configuration**` and
`## Configuration` collide in the published URL even though the source differs.
slug() is imported from linkcheck.py, so this tool compares exactly what the
link checker resolves anchors against.

Two strictness levels, and only the using-directive rule uses both. A missing
language tag is an error everywhere: the backfill landed with spec 011 Task 7.2,
so there is no debt for the softer level to protect, and an untagged fence today
is a new one.

Repo-wide, missing `using` directives are a warning with a count, so existing
debt is visible without blocking unrelated work. Under --changed they are an
error — but only for code blocks that overlap the diff.
Block granularity, not file: a file-level rule would mean fixing a typo on a
700-line page obliges backfilling every block on it, which penalises exactly the
small corrections worth encouraging.

A --changed run opens by reporting what it examined -- files, hunks, pages and,
the figure that matters, how many code blocks the diff reached. `0 errors` from
a run that made nothing strict is the same line as `0 errors` from one that did,
and the difference is not recoverable afterwards. See describe_scope().

A block that marks its omission with `// ...` stays a warning even when strict.
That is the remedy this rule's own message offers and CLAUDE.md's *Complete code
blocks* prescribes — an incomplete block should be visibly incomplete. It is a
declaration, not a fix, so the finding is downgraded and never silenced: the
block still counts towards the debt and still says so, in its own words. Without
it, moving a block verbatim between pages is indistinguishable from writing a
new one, and a page split cannot honour "move text, do not improve it".

--fix repairs the three rules that have exactly one correct answer: a banner
whose version segment is stale against APPLIES_TO, an untagged fence whose body
shows no evidence of being code, and a missing `description:` front matter,
which is the page's own opening sentence with its markdown stripped. It never
decides a page type. See the --fix section below for why that boundary is where
it is.

The description repair refuses whenever the sentence it would copy fails rule 7,
so --fix cannot manufacture a description the linter then rejects -- nor, worse,
one it accepts. It also refuses when the H1 is not on line 1: GitBook reads front
matter only at the very first byte, and a block written under a leading blank
line is not front matter at all. That failure is silent, which is why it is a
refusal rather than a best effort.

Usage:
    python3 tools/pagelint.py                          # whole repo
    python3 tools/pagelint.py contents/Glossary.md     # specific pages
    python3 tools/pagelint.py --changed origin/master  # strict on changed blocks
    python3 tools/pagelint.py --fix                    # repair, then report what is left

Cross-page uniqueness is a property of the corpus, so when given explicit paths
the tool still loads every page for context and only reports on the ones asked
for — exactly how linkcheck.py handles orphans.

Exit code is 1 when anything is an error, 0 when clean or warnings only, and 2
on bad arguments or an unusable git history, so it can gate CI.
"""
import os
import re
import subprocess
import sys
from collections import defaultdict, namedtuple

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from linkcheck import ROOT, HEADING_RE, md_files, slug  # noqa: E402  reuse, do not duplicate

# Only pages under contents/ are reader-facing. md_files() walks the whole repo
# minus its skip lists, so it also yields README.md and SUMMARY.md, which are
# not pages and carry no banner.
PAGES_DIR = 'contents'

# Repo-root files that GitBook nevertheless publishes as pages. README.md is the
# site root and the 143rd entry in /llms.txt, so its `description:` reaches a
# reader exactly like any other page's -- and until spec 010 Task 11.9 nothing
# checked it, because this tool read contents/ and stopped.
#
# They take rule 7 and nothing else. A root page carries no banner (rules 1 and
# 2 would be asking it to declare a page type it does not have) and stands
# outside the heading-qualification convention, whose whole purpose is to
# attribute a chunk to the page it came from -- which for the site root is
# circular. Everything about the opening sentence applies unchanged.
ROOT_PAGES = frozenset({'README.md'})

# Canonical. CLAUDE.md quotes this list verbatim; the two must not drift.
# Their repetition is a feature: it makes the end of every page predictable.
NAV_ALLOWLIST = frozenset({
    'Further Reading', 'Related Documentation', 'See Also',
    'Next Steps', 'References',
})

PAGE_TYPES = ('Tutorial', 'How-to', 'Reference', 'Explanation')

# Brighter and Darker version independently: Brighter is on V10, Darker's latest
# release is 4.1.1. An earlier draft of this vocabulary said "Darker V10", which
# is a version that has never existed -- exactly the kind of confident-but-wrong
# version claim the banner is here to prevent.
#
# Deliberately a closed vocabulary rather than a version pattern. When Brighter
# goes to V11, or Darker ships the release currently in flight, every unbumped
# page fails loudly instead of quietly asserting last year's version. That is
# 105 mechanical edits, which is what `--fix` exists for. Change the versions
# HERE and nowhere else -- CLAUDE.md documents this tuple, and
# apply_banners.py imports it.
APPLIES_TO = ('Brighter V10 and Darker V4', 'Brighter V10', 'Darker V4')

BANNER_RE = re.compile(
    r'^> \*\*(Tutorial|How-to|Reference|Explanation)\*\*'      # page type
    r' · Applies to \*\*(' + '|'.join(APPLIES_TO) + r')\*\*'   # longest first
    r'( · Prerequisites: .+)?$'                                 # optional
)

# Structural match only: is this line one of our banners *at all*, whatever it
# says? BANNER_RE answers "is this banner valid"; this answers "is this banner
# ours to rewrite". The two differ exactly when a banner was written under a
# superseded vocabulary -- which happened the first time the Darker version was
# corrected, and will happen again at V11. Without this, a tool replacing
# banners cannot tell its own stale output from a page's content blockquote,
# and correctly refuses to touch either.
BANNER_SHAPE_RE = re.compile(r'^> \*\*[^*]+\*\* · Applies to \*\*[^*]+\*\*')

BANNER_EXAMPLE = '> **Reference** · Applies to **Brighter V10**'

# Rule 4 is a repo-wide error as of spec 011 Task 7.2, flipped in the same
# commit that tagged the last 34 untagged fences. It was a warning while the
# backfill was outstanding; there is no backfill left, so an untagged fence is
# now a new one, and a rule left unenforced is a rule that decays.
LANGUAGE_TAG_IS_ERROR = True

# Opening fence: up to three spaces of indent, then a run of >=3 backticks or
# tildes, then an optional info string. A closing fence repeats the character at
# least as many times and carries no info string.
FENCE_RE = re.compile(r'^ {0,3}(`{3,}|~{3,})[ \t]*(\S*)')

CSHARP_TAGS = frozenset({'csharp', 'cs', 'c#'})

# `using System;`, `using static X.Y;`, `using Alias = X.Y;` — but not the using
# statement `using (var conn = ...)` or the declaration `using var conn = ...;`.
USING_RE = re.compile(
    r'^\s*(?:global\s+)?using\s+(?:static\s+)?'
    r'[A-Za-z_][\w.]*(?:\s*=\s*[\w.<>,\[\]\s]+)?\s*;')

# `// ...` marking a deliberate omission, per CLAUDE.md's *Complete code blocks*.
# Leading indent and trailing prose are both fine — `// ... other configuration`
# is the form the repo already uses. Block comments open the same way.
ELISION_RE = re.compile(r'^\s*/[/*]\s*\.\.\.')

INLINE_CODE_RE = re.compile(r'`[^`]*`')
LINK_TARGET_RE = re.compile(r'\]\([^)]*\)')

# Rule 7's ceiling, measured on what a reader sees rather than what the author
# typed -- see rendered(). Two hundred is where design 010 §9.2 put it and it
# has held: a sentence longer than this stops being a summary and starts being
# the first half of a paragraph, which is no use as a search snippet.
SUMMARY_LIMIT = 200
OPT_OUT = '<!-- pagelint: allow-serviceactivator -->'

# Both spellings. The API surface uses the closed form, but prose in this repo
# uses the open one just as often -- 19 instances across 11 pages when this was
# first measured -- and CLAUDE.md's pitfall list treats them as one violation.
# Matching only the closed form would leave the commoner prose spelling of the
# V9 term unenforced, which is the failure this rule exists to prevent.
SERVICEACTIVATOR_RE = re.compile(r'Service\s*Activator')

Finding = namedtuple('Finding', 'path line rule message severity')


def error(path, line, rule, message):
    return Finding(path, line, rule, message, 'error')


def warning(path, line, rule, message):
    return Finding(path, line, rule, message, 'warning')


class Page:
    """One documentation page, parsed once into headings and fenced blocks.

    Fences are tracked properly so that a `# Install packages` comment inside a
    bash block is not mistaken for an H1, and so that an untagged fence closing
    a tagged block is not reported as a missing language tag.
    """

    def __init__(self, path, rel, lines):
        self.path = path
        self.rel = rel
        self.lines = lines
        self.is_root = rel in ROOT_PAGES
        self.headings = []      # (level, text, lineno)
        self.blocks = []        # dicts: info, start, end, body
        self.prose = []         # (lineno, text) outside fences
        self._parse()

    def _parse(self):
        fence = None
        for lineno, line in enumerate(self.lines, 1):
            match = FENCE_RE.match(line)
            if fence is None:
                if match:
                    fence = {
                        'marker': match.group(1),
                        'info': match.group(2),
                        'start': lineno,
                        'body': [],
                    }
                    continue
                heading = HEADING_RE.match(line)
                if heading:
                    self.headings.append(
                        (len(heading.group(1)), heading.group(2).strip(), lineno))
                else:
                    self.prose.append((lineno, line))
                continue
            closes = (match
                      and match.group(1)[0] == fence['marker'][0]
                      and len(match.group(1)) >= len(fence['marker'])
                      and not match.group(2))
            if closes:
                fence['end'] = lineno
                self.blocks.append(fence)
                fence = None
            else:
                fence['body'].append((lineno, line))
        if fence is not None:            # unterminated fence, EOF closes it
            fence['end'] = len(self.lines)
            self.blocks.append(fence)

    @property
    def h1(self):
        for level, text, _ in self.headings:
            if level == 1:
                return text
        return None

    @property
    def h1_line(self):
        for level, _, lineno in self.headings:
            if level == 1:
                return lineno
        return None


def is_nav(text):
    """Allowlisted navigation headings, compared normalised like everything else.

    Matching on slug() keeps an emphasised `## **Further Reading**` exempt
    rather than turning it into a false positive.
    """
    return slug(text) in {slug(t) for t in NAV_ALLOWLIST}


def load_pages():
    """Every page under contents/, keyed by repo-relative path."""
    pages = {}
    for path in md_files():
        rel = os.path.relpath(path, ROOT)
        if os.path.dirname(rel) != PAGES_DIR and rel not in ROOT_PAGES:
            continue
        with open(path, encoding='utf-8') as fh:
            lines = fh.read().splitlines()
        pages[rel] = Page(path, rel, lines)
    return pages


# --------------------------------------------------------------------------
# Rules 1 and 2 — the page banner
# --------------------------------------------------------------------------

def check_banner(page):
    """Banner present as the first non-blank line after the H1, and well-formed."""
    if page.h1 is None:
        return [error(page.rel, 1, 'NO H1',
                      'page has no H1; every page needs a title before a banner')]

    # NO H1's mirror, and it was invisible for the same reason in reverse: that
    # rule fires only on *none*, and rule 3b starts at `##`, so a second H1 was
    # checked by nothing at all. One page carried one. A second H1 is a section
    # heading written a level too high -- it claims to be the page's title, it
    # is exempt from the uniqueness and qualification rules every real section
    # obeys, and GitBook renders it indistinguishably from the real title.
    extra = [lineno for level, _, lineno in page.headings
             if level == 1 and lineno != page.h1_line]
    findings = [
        error(page.rel, lineno, 'EXTRA H1',
              'a page has exactly one H1, its title. Demote this to `##` and '
              'qualify it by the page\'s subject -- as a `##` it must also be '
              'unique across pages, which is the check it has been escaping')
        for lineno in extra
    ]

    banner_line = None
    for lineno in range(page.h1_line + 1, len(page.lines) + 1):
        if page.lines[lineno - 1].strip():
            banner_line = lineno
            break

    if banner_line is None:
        return findings + [error(page.rel, page.h1_line, 'BANNER MISSING',
                      f'nothing follows the H1; add a banner below it, e.g.\n    {BANNER_EXAMPLE}')]

    text = page.lines[banner_line - 1].rstrip()
    if not text.startswith('>'):
        return findings + [error(page.rel, banner_line, 'BANNER MISSING',
                      f'add a banner below the H1, e.g.\n    {BANNER_EXAMPLE}')]
    if not BANNER_RE.match(text):
        return findings + [error(page.rel, banner_line, 'BANNER MALFORMED',
                      'banner must read `> **<type>** · Applies to **<product> V10**'
                      '[ · Prerequisites: <links>]`, where <type> is one of '
                      f'{", ".join(PAGE_TYPES)} and the separator is " · " (U+00B7).'
                      f'\n    e.g. {BANNER_EXAMPLE}')]
    return findings


# --------------------------------------------------------------------------
# Rules 3a and 3b — heading uniqueness, two scopes
# --------------------------------------------------------------------------

def propose_qualifier(h1, text):
    """Suggest a subject-qualified replacement, from the page's H1.

    A proposal only. `## Hangfire Best Practices` beats
    `## Hangfire Scheduler Best Practices` and no rule can tell you that, so the
    linter suggests and a human edits.
    """
    if not h1:
        return None
    if slug(h1).endswith(slug(text)):
        return h1
    return f'{h1} {text}'


def _also_in(rels, limit=3):
    shown = ', '.join(os.path.basename(r) for r in rels[:limit])
    if len(rels) > limit:
        shown += f', and {len(rels) - limit} more'
    return shown


def check_headings(pages, reported):
    """Rule 3a across pages (H2 only), rule 3b within a page (H2-H4).

    `pages` is always the whole corpus — uniqueness cannot be judged from a
    subset — while `reported` limits which pages produce findings.
    """
    findings = []

    # 3a: which pages carry each normalised H2 text.
    pages_by_slug = defaultdict(set)
    for rel, page in pages.items():
        # Root pages are outside this convention, so they must not be inside its
        # corpus either: a page colliding with README.md would be reported
        # against a rule README.md is exempt from.
        if page.is_root:
            continue
        for level, text, _ in page.headings:
            if level == 2 and not is_nav(text):
                pages_by_slug[slug(text)].add(rel)

    for rel in reported:
        page = pages[rel]
        for level, text, lineno in page.headings:
            if level != 2 or is_nav(text):
                continue
            others = sorted(pages_by_slug[slug(text)] - {rel})
            if not others:
                continue
            proposal = propose_qualifier(page.h1, text)
            fix = f' Qualify it: "## {proposal}"' if proposal else ''
            findings.append(error(
                rel, lineno, 'HEADING NOT UNIQUE',
                f'"## {text}" also appears in {_also_in(others)}.{fix}'))

    # 3b: repeats within one page, H2 through H4. Every occurrence after the
    # first is a finding, which is what makes the count comparable to the
    # surplus-instance measurement.
    for rel in reported:
        page = pages[rel]
        seen = {}
        for level, text, lineno in page.headings:
            if not 2 <= level <= 4 or is_nav(text):
                continue
            key = slug(text)
            if key in seen:
                findings.append(error(
                    rel, lineno, 'HEADING REPEATED',
                    f'"{"#" * level} {text}" already appears on this page at line '
                    f'{seen[key]}. Within a page a repeat is always a defect: it '
                    f'produces #{key}-1, #{key}-2 anchors. Qualify it, or merge '
                    f'the sections if they are the same subject.'))
            else:
                seen[key] = lineno
    return findings


# --------------------------------------------------------------------------
# Rules 4 and 6 — fenced code blocks
# --------------------------------------------------------------------------

def check_code_blocks(page, strict_ranges):
    """Language tag on every fence; `using` directives in C# blocks.

    A block is strict only if it overlaps a changed line range, so a small
    correction on a large page obliges nothing beyond itself.
    """
    findings = []
    for block in page.blocks:
        strict = any(start <= block['end'] and block['start'] <= end
                     for start, end in strict_ranges)
        tag = block['info'].split(',')[0].lower()

        if not tag:
            severity = error if (strict or LANGUAGE_TAG_IS_ERROR) else warning
            findings.append(severity(
                page.rel, block['start'], 'LANGUAGE TAG',
                'fenced block has no language; tag it `csharp`, `yaml`, `json`, '
                '`bash`, or `text` for output'))
            continue

        if tag in CSHARP_TAGS:
            if not any(USING_RE.match(line) for _, line in block['body']):
                elided = any(ELISION_RE.match(line) for _, line in block['body'])
                severity = error if (strict and not elided) else warning
                findings.append(severity(
                    page.rel, block['start'], 'USING DIRECTIVES',
                    'C# block has no `using` lines and is marked `// ...`; the '
                    'omission is declared, not fixed, and a reader still cannot '
                    'compile it as shown' if elided else
                    'C# block has no `using` lines; a reader cannot compile it as '
                    'shown. Add them, or mark the omission with `// ...`'))
    return findings


# --------------------------------------------------------------------------
# --fix — the two rules with exactly one correct answer
# --------------------------------------------------------------------------
#
# A version bump is 110 banners. Retyping one segment across 110 pages is the
# kind of trudge that gets abandoned half-done, and a half-bumped corpus is
# worse than an un-bumped one: some pages assert the new version and some the
# old, with nothing to distinguish a page that was considered from one that was
# missed. That is the whole reason this exists.
#
# It is deliberately narrow, and the boundary is not squeamishness. It fixes
# what has exactly one correct answer and refuses everything else out loud. It
# must never decide a page type: that is a judgement about what a page is *for*,
# it cannot be recovered from the text, and a wrong one is invisible — a page
# mislabelled `Reference` reads perfectly and misleads every reader who trusted
# the label. The same reasoning keeps apply_banners.py's TSV lookup out of here.
# That is one-off migration logic keyed to a file recording decisions taken
# years before the next person runs this tool; folding it in would leave a
# durable tool quietly carrying it.

Change = namedtuple('Change', 'path line before after note')
Refusal = namedtuple('Refusal', 'path line reason')

# A vocabulary entry names one or more products, each with its own version:
# `Brighter V10`, `Darker V4`, `Brighter V10 and Darker V4`.
VERSIONED_PRODUCT_RE = re.compile(r'^([A-Za-z][\w.]*) V(\d+)$')

# Only the version segment is ever rewritten. Capturing the text either side of
# it is what keeps the page type and any Prerequisites segment untouched --
# the substitution cannot reach them.
APPLIES_SEGMENT_RE = re.compile(r'(· Applies to \*\*)([^*]+)(\*\*)')


def products_named(applies):
    """The set of product names in a vocabulary value, or None if unparseable."""
    names = set()
    for part in applies.split(' and '):
        match = VERSIONED_PRODUCT_RE.match(part.strip())
        if not match:
            return None
        names.add(match.group(1))
    return frozenset(names)


def version_targets():
    """Product set -> the one APPLIES_TO entry naming exactly those products.

    This is how a stale banner finds its replacement without anyone writing a
    migration map: `Brighter V10` names {Brighter}, and after the V11 bump the
    single entry naming {Brighter} is `Brighter V11`. One edit to APPLIES_TO is
    the whole bump.

    A product set claimed by two entries is dropped rather than guessed at. That
    only happens if the vocabulary is restructured rather than bumped, which is
    an editorial change and wants a human.
    """
    by_products = defaultdict(list)
    for entry in APPLIES_TO:
        products = products_named(entry)
        if products is not None:
            by_products[products].append(entry)
    return {p: found[0] for p, found in by_products.items() if len(found) == 1}


def fix_banner_version(page):
    """Retarget a stale `Applies to` segment. Never touches the type."""
    if page.h1_line is None:
        return [], []
    banner_line = None
    for lineno in range(page.h1_line + 1, len(page.lines) + 1):
        if page.lines[lineno - 1].strip():
            banner_line = lineno
            break
    if banner_line is None:
        return [], []

    text = page.lines[banner_line - 1].rstrip()
    # BANNER_SHAPE_RE, not BANNER_RE: the banner being fixed is by definition
    # one BANNER_RE rejects. Shape is what identifies it as ours to rewrite.
    if not BANNER_SHAPE_RE.match(text):
        return [], []
    match = APPLIES_SEGMENT_RE.search(text)
    if not match or match.group(2) in APPLIES_TO:
        return [], []

    stale = match.group(2)
    products = products_named(stale)
    targets = version_targets()
    if products is None:
        return [], [Refusal(page.rel, banner_line,
                            f'`Applies to **{stale}**` is not `<Product> V<n>`, so '
                            'there is nothing to map it from')]
    if products not in targets:
        named = ' and '.join(sorted(products))
        return [], [Refusal(page.rel, banner_line,
                            f'`Applies to **{stale}**` names {named}, which no single '
                            'entry in APPLIES_TO covers; this is an editorial change, '
                            'not a bump')]

    current = targets[products]
    fixed = APPLIES_SEGMENT_RE.sub(
        lambda m: m.group(1) + current + m.group(3), text, count=1)
    return [Change(page.rel, banner_line, text, fixed,
                   f'{stale} -> {current}')], []


def yaml_quote(value):
    """A YAML scalar for `value`, or None when the answer is not unique.

    Double quotes by default, so the apostrophes in "Brighter's" need no
    thought. Single quotes when the value itself contains a double quote --
    EventCarriedStateTransfer.md opens by naming a paper in quotes. Refuses
    when it contains both, or a backslash, rather than guessing at an escape:
    GitBook's failure mode for malformed front matter is a page that imports
    with no title and no error, so a wrong guess here is invisible.
    """
    if '\\' in value:
        return None
    if '"' not in value:
        return '"' + value + '"'
    if "'" not in value:
        return "'" + value + "'"
    return None


def fix_description(page):
    """Write `description:` front matter from the page's own opening sentence.

    This qualifies for --fix on the same test as the other two repairs: there
    is exactly one correct answer and it is derivable from the page. It refuses
    whenever the sentence itself would fail rule 7, so --fix cannot manufacture
    a description that the linter then rejects -- or worse, accept one.
    """
    got, why = opening_sentence(page)
    if got is None:
        return [], [Refusal(page.rel, 1, f'{why}, so there is no sentence to describe it with')]
    sentence, lineno = got
    shown = rendered(sentence)

    if len(shown) > SUMMARY_LIMIT or shown.endswith(':') \
            or not shown.endswith(('.', '!', '?')):
        return [], [Refusal(page.rel, lineno,
                            'the opening sentence does not satisfy rule 7; fix the '
                            'sentence and the description follows from it')]

    quoted = yaml_quote(shown)
    if quoted is None:
        return [], [Refusal(page.rel, lineno,
                            'the opening sentence contains a backslash, or both quote '
                            'characters, so its YAML scalar is not unique')]

    existing, unreadable = front_matter_description(page)
    if unreadable:
        return [], [Refusal(page.rel, 1, unreadable)]

    if existing is not None:
        if existing == shown:
            return [], []
        for line in range(1, len(page.lines) + 1):
            if page.lines[line - 1].startswith('description:'):
                return [Change(page.rel, line, page.lines[line - 1],
                               'description: ' + quoted,
                               'description retargeted at the opening sentence')], []
        return [], [Refusal(page.rel, 1, 'the description moved under the fix')]

    if page.lines and page.lines[0].strip() == '---':
        return [], [Refusal(page.rel, 1,
                            'the page has front matter but no `description:`; adding a key '
                            'to a block someone else wrote is an editorial change')]

    # GitBook requires front matter at the very first byte of the file. A page
    # with a blank line before its H1 would take the block at line 2, where it
    # is not front matter at all -- and the failure is silent, because the page
    # still renders and simply has no description. Caught on
    # ReturningResultsFromAHandler.md, the one page in the corpus that opens
    # with a newline.
    if page.h1_line != 1:
        return [], [Refusal(page.rel, 1,
                            f'the H1 is on line {page.h1_line}, so front matter written '
                            'above it would not be the first bytes of the file, and '
                            'GitBook would ignore it')]

    # No front matter at all: the block goes above the H1, and carries the
    # layout switch with it. Without `visible: false` GitBook renders the
    # description as a subtitle, and the page then opens with the same sentence
    # twice -- once as the subtitle, once as the paragraph it was taken from.
    h1 = page.lines[page.h1_line - 1]
    block = ('---\n'
             f'description: {quoted}\n'
             'layout:\n'
             '  description:\n'
             '    visible: false\n'
             '---\n'
             '\n') + h1
    return [Change(page.rel, page.h1_line, h1, block,
                   'description written from the opening sentence')], []


# Evidence that a block is code, and so not this tool's to tag. Each entry is
# (what it recognises, pattern); the first to match a line holds the fix back.
CODE_EVIDENCE = (
    ('a C# declaration', re.compile(
        r'^\s*(using [A-Za-z_]|namespace\s|(public|private|protected|internal)\s'
        r'|(var|await|return|throw|new)\s)')),
    ('a statement terminator', re.compile(r'[;{}]\s*$')),
    ('a brace', re.compile(r'^\s*[{}]')),
    ('a shell command', re.compile(
        r'^\s*(\$\s|(dotnet|docker|docker-compose|git|npm|yarn|curl|wget|cd|export'
        r'|mkdir|rm|cp|sudo|apt|apt-get|brew|kubectl|helm|python3?|pip3?|bash|sh'
        r'|make|az|aws)\b)')),
    ('a `key: value` mapping', re.compile(r'^\s*[A-Za-z_][\w.-]*:(\s|$)')),
)


def infer_language(block):
    """`text`, or None when the block looks like code.

    `text` is the only tag this infers, and that is a finding rather than a
    limitation. All 34 fences the Task 7.2 backfill had to tag by hand were
    prose — ASCII flow diagrams, directory trees, validation-message dumps,
    trace output — and 33 of the 34 took `text`. The reason is mechanical: an
    author writing C# reaches for ```csharp because they want the highlighting,
    while an author drawing a box-and-arrow diagram has no language in mind and
    types a bare fence, because until `text` is the convention no tag is the
    honest answer to "what language is this?".

    So the detectors below are not a classifier. They exist to hold the fix back
    on the minority case, where choosing between `csharp`, `bash`, `json` and
    `yaml` is a decision belonging to whoever wrote the block. Being held back is
    the safe failure, and it is reported rather than swallowed.
    """
    for _, line in block['body']:
        for _, pattern in CODE_EVIDENCE:
            if pattern.search(line):
                return None
    return 'text'


def describe_evidence(block):
    for lineno, line in block['body']:
        for what, pattern in CODE_EVIDENCE:
            if pattern.search(line):
                return f'line {lineno} looks like {what}'
    return 'it looks like code'


def fix_language_tags(page):
    """Tag an untagged fence `text` when nothing in it suggests code."""
    changes, refusals = [], []
    for block in page.blocks:
        if block['info']:
            continue
        line = page.lines[block['start'] - 1]
        if infer_language(block) is None:
            refusals.append(Refusal(
                page.rel, block['start'],
                'untagged fence left alone: ' + describe_evidence(block) +
                ', and picking between `csharp`, `bash`, `json` and `yaml` is '
                'the author\'s call'))
            continue
        changes.append(Change(page.rel, block['start'], line.rstrip(),
                              line.rstrip() + 'text', 'tagged `text`'))
    return changes, refusals


def apply_changes(changes):
    """Write the changes, one file at a time, verifying every line first.

    Nothing is written for a file until every line it targets is confirmed to
    hold what the fixer read. A stale line number should abort rather than
    half-rewrite a page — the same contract as dedupe_within.py.
    """
    by_path = defaultdict(list)
    for change in changes:
        by_path[change.path].append(change)

    for rel, items in sorted(by_path.items()):
        path = os.path.join(ROOT, rel)
        with open(path, encoding='utf-8') as fh:
            raw = fh.read()
        lines = raw.splitlines(keepends=True)
        for change in items:
            actual = lines[change.line - 1]
            ending = actual[len(actual.rstrip('\r\n')):]
            if actual.rstrip('\r\n') != change.before:
                raise RuntimeError(
                    f'{rel}:{change.line} moved under the fix; nothing written '
                    f'for this file')
            lines[change.line - 1] = change.after + ending
        with open(path, 'w', encoding='utf-8') as fh:
            fh.write(''.join(lines))


# --------------------------------------------------------------------------
# Rule 5 — terminology
# --------------------------------------------------------------------------

def check_terminology(page):
    """"ServiceActivator" in prose where "Dispatcher" is meant.

    Legitimate three ways, and a naive ban fires on all of them: inside fenced
    blocks (ServiceActivatorHostedService, the DI namespace), inside inline code
    spans, and on a page discussing the name itself. So this reads prose only,
    with code spans and link targets removed, and honours an opt-out comment
    either on its own line or trailing the line it excuses.
    """
    body = '\n'.join(page.lines)
    if OPT_OUT in body and any(
            line.strip() == OPT_OUT for line in page.lines):
        return []

    # Front matter is not prose, and reading it as prose double-reports. The
    # `description:` value is the opening sentence with its code spans stripped,
    # so an assembly name legitimately in backticks -- `Brighter.ServiceActivator`
    # on HowServiceActivatorWorks.md -- arrives here bare and fires a rule the
    # body it came from already passes. DESCRIPTION MISMATCH keeps the two
    # equal, so checking the body checks both.
    findings = []
    for lineno, line in page.prose:
        if lineno <= front_matter_end(page):
            continue
        if OPT_OUT in line:
            continue
        stripped = LINK_TARGET_RE.sub(']()', INLINE_CODE_RE.sub('``', line))
        found = SERVICEACTIVATOR_RE.search(stripped)
        if found:
            findings.append(error(
                page.rel, lineno, 'SERVICEACTIVATOR',
                f'use "Dispatcher" in prose; "{found.group(0)}" is the V9 name and '
                'survives only in the API surface. If this is an identifier, put it '
                'in backticks rather than bold. If this page discusses the name '
                f'itself, add `{OPT_OUT}`'))
    return findings


# --------------------------------------------------------------------------
# Rule 7 — the opening sentence, which is also the page's summary
# --------------------------------------------------------------------------

def rendered(text):
    """What a reader sees: link text without its target, no emphasis marks.

    The 200-character limit measures this, not the markdown. A sentence can
    carry a 90-character documentation URL that nobody reads, and failing a
    page on the length of a href would be measuring the wrong thing. Measured
    when this rule was written: one sentence came to 199 characters of source
    and about 140 rendered, one character under a limit it was never really
    near.
    """
    text = LINK_TARGET_RE.sub(']', text)          # [text](url) -> [text]
    text = re.sub(r'\[([^\]]*)\]', r'\1', text)   # [text]      -> text
    text = re.sub(r'`([^`]*)`', r'\1', text)
    text = re.sub(r'\*\*([^*]*)\*\*', r'\1', text)
    text = re.sub(r'\*([^*]*)\*', r'\1', text)
    # Markdown backslash escapes are punctuation to a reader and backslashes to
    # everyone else. EventCarriedStateTransfer.md opens with an escaped quote,
    # and without this the description written from it carries the backslashes
    # into the page's meta tags and the published index.
    text = re.sub(r'\\([\\`*_{}\[\]()#+\-.!"<>|\'])', r'\1', text)
    return text.strip()


# Sentence-enders that are not. Kept explicit rather than clever: a page opening
# "Data on the Outside vs. Data on the Inside" was being summarised as
# "In his white paper "Data on the Outside vs." -- accepted by every clause of
# rule 7, because a truncated sentence is short, unique and ends in a period.
ABBREVIATIONS = frozenset({
    'e.g', 'i.e', 'etc', 'vs', 'cf', 'approx', 'viz', 'resp', 'al', 'ca',
    'inc', 'ltd', 'co', 'corp', 'fig', 'no', 'dr', 'mr', 'mrs', 'ms', 'st',
    'jr', 'sr',
})

TRAILING_WORD_RE = re.compile(r'([A-Za-z.]+)$')


def first_sentence(text):
    """Up to the first sentence-ending period, else the whole line.

    A period ends a sentence unless the word before it is an abbreviation or a
    single initial. Guessing the other way -- treating every period as a
    boundary -- produces a summary that passes all of rule 7's checks and is
    nonsense, which is the worst of both: enforced, and wrong.
    """
    for match in re.finditer(r'[.!?](\s|$)', text):
        word = TRAILING_WORD_RE.search(text[:match.start()])
        token = word.group(1) if word else ''
        if token.lower().rstrip('.') in ABBREVIATIONS:
            continue
        if re.fullmatch(r'[A-Za-z]', token):      # an initial: "Pat J. Helland"
            continue
        return text[:match.start() + 1].strip()
    return text.strip()


def opening_sentence(page):
    """(sentence, lineno), or (None, why) — the first sentence of the page body.

    "First non-blank prose line after the banner": blank lines, headings,
    blockquotes and fenced blocks are passed over; anything else is the
    introduction, whether or not it looks like a paragraph.

    A list or table stops the scan rather than being skipped, deliberately. A
    page whose introduction is a bullet list has no opening sentence, and
    should be told so rather than quietly summarised by a paragraph further
    down under some later heading. Both policies were measured across the
    corpus when this was written and both found every page a sentence, so the
    stricter one costs nothing today and says something the day it stops
    being true.
    """
    if page.h1_line is None:
        return None, 'the page has no H1'
    banner = None
    for lineno in range(page.h1_line + 1, len(page.lines) + 1):
        if page.lines[lineno - 1].strip():
            banner = lineno
            break
    if banner is None:
        return None, 'nothing follows the H1'

    # Past the banner, or from the H1 on a root page, which has none. Getting
    # this wrong is silent rather than loud: the extractor would take the
    # opening sentence *for* the banner, skip it, and summarise the page with
    # its second paragraph -- which then mismatches the front matter derived
    # from the first.
    scan_from = page.h1_line if page.is_root else banner
    in_fence = False
    for lineno in range(scan_from + 1, len(page.lines) + 1):
        line = page.lines[lineno - 1]
        match = FENCE_RE.match(line)
        if match:
            in_fence = not in_fence
            continue
        if in_fence:
            continue
        text = line.strip()
        if not text or HEADING_RE.match(line) or text.startswith('>'):
            continue
        # The whole paragraph, not the line. Prose here is hard-wrapped, so a
        # sentence routinely runs past a newline -- and a line-at-a-time reader
        # returns a fragment that passes every clause of rule 7 by being short,
        # unique and punctuation-free. Measured before this was fixed: 20 of
        # 142 summaries were truncated mid-sentence, four of them mid-link.
        para = [text]
        for follow in range(lineno + 1, len(page.lines) + 1):
            nxt = page.lines[follow - 1]
            if not nxt.strip() or HEADING_RE.match(nxt) or FENCE_RE.match(nxt):
                break
            para.append(nxt.strip())
        return (first_sentence(' '.join(para)), lineno), None
    return None, 'the page has no prose after its banner'


def front_matter_end(page):
    """Line number of the closing `---`, or 0 when the page has no front matter."""
    if not page.lines or page.lines[0].strip() != '---':
        return 0
    for lineno in range(2, len(page.lines) + 1):
        if page.lines[lineno - 1].strip() == '---':
            return lineno
    return 0


def front_matter_description(page):
    """(value, None) from `description:` front matter, or (None, why|None).

    Both slots are returned every time and the second is always a *reason*,
    never a line number. An earlier draft returned the line number on success,
    which every caller reads as "something went wrong" -- the check reported
    its own success as a failure, with the line number as the message.

    Deliberately not a YAML parser. Only the single-line, quoted form this
    repository writes is understood, and anything else is reported rather than
    guessed at -- a wrong guess here is worse than no check, because GitBook's
    own failure mode for malformed front matter is silent.
    """
    if not page.lines or page.lines[0].strip() != '---':
        return None, None
    close = None
    for lineno in range(2, len(page.lines) + 1):
        if page.lines[lineno - 1].strip() == '---':
            close = lineno
            break
    if close is None:
        return None, 'the front matter block is never closed'
    for lineno in range(2, close):
        line = page.lines[lineno - 1]
        if not line.startswith('description:'):
            continue
        value = line[len('description:'):].strip()
        if value[:1] in ('>', '|'):
            return None, ('the description uses a YAML block scalar; write it as one '
                          'quoted line so it can be compared with the page')
        if not (len(value) >= 2 and value[0] == value[-1] and value[0] in '"\''):
            return None, ('the description is unquoted; GitBook fails Git Sync '
                          'silently on an unquoted value containing a colon')
        return value[1:-1], None
    return None, None


def check_summaries(pages, reported):
    """Rule 7. Every page opens with a sentence fit to stand alone as its summary.

    That sentence is not decoration. It is what GitBook's `.md` variant leads
    with after the banner, and — once it is carried into `description:` front
    matter — what the canonical /llms.txt prints after the page's title. A
    reader meets it as a search snippet before they ever meet the page.

    Uniqueness is checked across the corpus for the same reason `##` headings
    are: two pages with the same opening sentence are indistinguishable in an
    index, and the duplicate is usually a copied intro somebody forgot to
    rewrite. On this rule's first run that is exactly what it found — a page
    about the Azure Blob archive provider opening with a paragraph about Azure
    Service Bus, wrong since 2023 and invisible to every other check here,
    because nothing about it is malformed.
    """
    findings = []
    by_sentence = defaultdict(list)

    for rel in sorted(pages):
        got, why = opening_sentence(pages[rel])
        if got is None:
            if rel in reported:
                findings.append(error(
                    rel, 1, 'SUMMARY MISSING',
                    f'{why}; add an introductory sentence below the banner, so the '
                    'page has something to be summarised by'))
            continue
        sentence, lineno = got
        by_sentence[rendered(sentence)].append((rel, lineno))
        if rel not in reported:
            continue
        shown = rendered(sentence)
        if len(shown) > SUMMARY_LIMIT:
            findings.append(error(
                rel, lineno, 'SUMMARY TOO LONG',
                f'the opening sentence is {len(shown)} characters rendered; '
                f'the limit is {SUMMARY_LIMIT}. Split it — the first sentence '
                'has to survive being read on its own, in a search result'))
        # Colon first, then the general case. A colon IS a missing terminal
        # stop, so the other order makes this branch unreachable and silently
        # retires a rule -- the specific message has the specific remedy.
        if shown.endswith(':'):
            findings.append(error(
                rel, lineno, 'SUMMARY ENDS IN COLON',
                'the opening sentence ends in a colon, so it promises a list '
                'that an index will not print. Say what the page is about, then '
                'introduce the list'))
        elif not shown.endswith(('.', '!', '?')):
            findings.append(error(
                rel, lineno, 'SUMMARY NOT A SENTENCE',
                f'the page opens with "{shown[:60]}…", which has no terminal '
                'punctuation. An index prints it as the page\'s whole description, '
                'so a fragment reads as a truncation of something better'))

        # Parity with the front matter, on pages that carry one. The whole point
        # of deriving the description from the page is that the two cannot
        # drift; nothing enforces that unless something compares them.
        described, why = front_matter_description(pages[rel])
        if why:
            findings.append(error(rel, 1, 'DESCRIPTION UNREADABLE', why))
        elif described is not None and described != shown:
            findings.append(error(
                rel, lineno, 'DESCRIPTION MISMATCH',
                'the `description:` front matter is not this page\'s opening '
                f'sentence.\n    front matter: {described}\n    opening line: {shown}'))

    for shown, where in sorted(by_sentence.items()):
        if len(where) < 2:
            continue
        others = [rel for rel, _ in where]
        for rel, lineno in where:
            if rel not in reported:
                continue
            elsewhere = [o for o in others if o != rel]
            findings.append(error(
                rel, lineno, 'SUMMARY NOT UNIQUE',
                'this opening sentence also opens '
                f'{_also_in(elsewhere, limit=3)}. Two pages that introduce '
                'themselves identically are indistinguishable in an index — and '
                'one of them is usually about something else'))
    return findings


# --------------------------------------------------------------------------
# --changed
# --------------------------------------------------------------------------

def _git(args):
    return subprocess.run(['git'] + args, cwd=ROOT, capture_output=True,
                          text=True, check=True).stdout


def changed_ranges(merge_base):
    """Changed line ranges per repo-relative path, against the merge-base.

    Raises RuntimeError rather than returning nothing when git cannot answer.
    A --changed run that silently finds no changed ranges passes vacuously,
    which is the worst outcome: the strict rules would never fire and the build
    would stay green while enforcing nothing.
    """
    try:
        base = _git(['merge-base', 'HEAD', merge_base]).strip()
    except OSError as exc:
        raise RuntimeError(f'cannot run git: {exc}')
    except subprocess.CalledProcessError:
        raise RuntimeError(
            f'no merge-base between HEAD and {merge_base!r}. On CI this usually '
            'means a shallow clone (set fetch-depth: 0) or a ref that was never '
            'fetched')
    try:
        diff = _git(['diff', '--unified=0', base, '--'])
    except subprocess.CalledProcessError as exc:
        raise RuntimeError(f'git diff against {base} failed: {exc}')

    ranges = defaultdict(list)
    current = None
    for line in diff.splitlines():
        if line.startswith('+++ '):
            target = line[4:].strip()
            current = None if target == '/dev/null' else target[2:]
        elif line.startswith('@@ ') and current:
            match = re.search(r'\+(\d+)(?:,(\d+))?', line)
            if not match:
                continue
            start = int(match.group(1))
            count = int(match.group(2)) if match.group(2) else 1
            if count:
                ranges[current].append((start, start + count - 1))
    return ranges


def describe_scope(merge_base, ranges, pages, reported):
    """What a --changed run actually examined, in its own words.

    `0 errors` from --changed is indistinguishable from a run that found no
    ranges to be strict about, and both print the same line. That ambiguity has
    already been read the wrong way twice in this repository: once when a diff
    of five prose lines was expected to make the gate bite, and once when two
    brand-new pages contributed no ranges at all because git diff cannot see an
    unstaged file. Neither run was wrong; both looked exactly like a real one.

    So the run reports its own scope. The load-bearing figure is the last one:
    the strict rules are per *code block*, so a diff spanning a hundred files
    of prose makes them strict about nothing. Files and hunks describe the
    diff; blocks describe what the diff reached.
    """
    files = len(ranges)
    hunks = sum(len(v) for v in ranges.values())
    strict_pages = [rel for rel in reported if ranges.get(rel)]
    blocks = sum(
        1
        for rel in strict_pages
        for block in pages[rel].blocks
        if any(start <= block['end'] and block['start'] <= end
               for start, end in ranges[rel])
    )
    line = (f'--changed {merge_base}: {files} file(s), {hunks} hunk(s) in the '
            f'diff; {len(strict_pages)} documentation page(s), '
            f'{blocks} code block(s) strict.')
    if not blocks:
        line += ('\n  No code block overlaps the diff, so the strict rules are '
                 'vacuous on this run — a pass here says nothing about them. '
                 'That is legitimate for a prose-only change; if you expected '
                 'otherwise, check that new files are staged, since git diff '
                 'cannot see an untracked one.')
    return line


# --------------------------------------------------------------------------

def main(argv):
    merge_base = None
    fix = False
    paths = []
    args = list(argv)
    while args:
        arg = args.pop(0)
        if arg == '--changed':
            if not args:
                print('--changed needs a ref, e.g. --changed origin/master',
                      file=sys.stderr)
                return 2
            merge_base = args.pop(0)
        elif arg == '--fix':
            fix = True
        elif arg.startswith('-'):
            print(f'unknown option: {arg}', file=sys.stderr)
            return 2
        else:
            paths.append(arg)

    # --changed narrows which *blocks* the using-directive rule is strict about.
    # --fix repairs neither that rule nor anything scoped to a diff, so the
    # combination reads as "fix what I changed" and would do something else.
    if fix and merge_base:
        print('--fix and --changed do not combine: --changed only varies the '
              'strictness of a rule --fix does not repair', file=sys.stderr)
        return 2

    pages = load_pages()

    if paths:
        reported = []
        missing = []
        for raw in paths:
            rel = os.path.relpath(os.path.abspath(raw), ROOT)
            if rel in pages:
                reported.append(rel)
            else:
                missing.append(raw)
        if missing:
            print('not a page under contents/: ' + ', '.join(missing),
                  file=sys.stderr)
            return 2
    else:
        reported = sorted(pages)

    strict = {}
    if merge_base:
        try:
            strict = changed_ranges(merge_base)
        except RuntimeError as exc:
            print(f'--changed cannot determine what changed: {exc}',
                  file=sys.stderr)
            return 2
        print(describe_scope(merge_base, strict, pages, reported))
        print()

    if fix:
        changes, refusals = [], []
        for rel in reported:
            fixers = ((fix_description,) if pages[rel].is_root
                      else (fix_banner_version, fix_language_tags, fix_description))
            for fixer in fixers:
                made, held = fixer(pages[rel])
                changes += made
                refusals += held
        try:
            apply_changes(changes)
        except RuntimeError as exc:
            print(f'--fix aborted: {exc}', file=sys.stderr)
            return 2
        for change in sorted(changes):
            print(f'{change.path}:{change.line}: FIXED: {change.note}')
        for refusal in sorted(refusals):
            print(f'{refusal.path}:{refusal.line}: NOT FIXED: {refusal.reason}')
        print(f'\n{len(changes)} fixed, {len(refusals)} left for a human.'
              if changes or refusals else '\nNothing to fix.')
        # Re-read, so what follows reports the tree as it now stands rather than
        # as it was found. A --fix run that printed pre-fix findings would be
        # indistinguishable from one that fixed nothing.
        if changes:
            pages = load_pages()
        print()

    findings = []
    for rel in reported:
        page = pages[rel]
        # A root page takes rule 7 only -- see ROOT_PAGES.
        if page.is_root:
            continue
        findings += check_banner(page)
        findings += check_code_blocks(page, strict.get(rel, []))
        findings += check_terminology(page)
    findings += check_headings(pages, [r for r in reported if not pages[r].is_root])
    findings += check_summaries(pages, reported)

    findings.sort(key=lambda f: (f.path, f.line, f.rule))

    for finding in findings:
        label = finding.rule
        if finding.severity == 'warning':
            label += ' (warning)'
        print(f'{finding.path}:{finding.line}: {label}: {finding.message}')

    errors = sum(1 for f in findings if f.severity == 'error')
    warnings = len(findings) - errors

    debt = [f for f in findings if f.rule == 'USING DIRECTIVES']
    summary = f'\n{errors} errors, {warnings} warnings'
    if debt:
        summary += (f' (using-directive debt: {len(debt)} blocks across '
                    f'{len({f.path for f in debt})} pages)')
    summary += f' across {len(reported)} pages.'
    print(summary)

    return 1 if errors else 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
