#!/usr/bin/env python3
"""Check authoring conventions across the documentation pages.

Where linkcheck.py asks "does this link resolve?", this asks "is this page
well-formed?" — for a reader deciding whether they are in the right place, and
for a retrieval system holding one chunk with no surrounding context.

Rules, and what each is for:

  BANNER MISSING      no page banner on the first non-blank line after the H1
  BANNER MALFORMED    a banner is there, but not in the fixed grammar
  HEADING NOT UNIQUE  a `##` text that also appears on another page
  HEADING REPEATED    a heading text repeated within one page (H2-H4)
  LANGUAGE TAG        a fenced block with no language (warning)
  SERVICEACTIVATOR    "ServiceActivator" in prose where "Dispatcher" is meant
  USING DIRECTIVES    a C# block with no `using` lines (warning, counted;
                      stays a warning under --changed if marked `// ...`)

The banner states the page type, the Brighter/Darker version it applies to and
what to read first. It is a visible blockquote rather than front matter because
GitBook renders front matter literally into the page body, and because a
retrieval chunker strips front matter but keeps body text.

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

Two strictness levels. Repo-wide, missing `using` directives are a warning with
a count, so existing debt is visible without blocking unrelated work. Under
--changed they are an error — but only for code blocks that overlap the diff.
Block granularity, not file: a file-level rule would mean fixing a typo on a
700-line page obliges backfilling every block on it, which penalises exactly the
small corrections worth encouraging.

A block that marks its omission with `// ...` stays a warning even when strict.
That is the remedy this rule's own message offers and CLAUDE.md's *Complete code
blocks* prescribes — an incomplete block should be visibly incomplete. It is a
declaration, not a fix, so the finding is downgraded and never silenced: the
block still counts towards the debt and still says so, in its own words. Without
it, moving a block verbatim between pages is indistinguishable from writing a
new one, and a page split cannot honour "move text, do not improve it".

Usage:
    python3 tools/pagelint.py                          # whole repo
    python3 tools/pagelint.py contents/Glossary.md     # specific pages
    python3 tools/pagelint.py --changed origin/master  # strict on changed blocks

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

# Rule 4 is a warning until the 185 untagged fences are backfilled, then an
# error repo-wide. Flip this in the same commit as the backfill — spec 011
# Task 7.2 — or the tags decay like everything else unenforced.
LANGUAGE_TAG_IS_ERROR = False

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
        if os.path.dirname(rel) != PAGES_DIR:
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

    banner_line = None
    for lineno in range(page.h1_line + 1, len(page.lines) + 1):
        if page.lines[lineno - 1].strip():
            banner_line = lineno
            break

    if banner_line is None:
        return [error(page.rel, page.h1_line, 'BANNER MISSING',
                      f'nothing follows the H1; add a banner below it, e.g.\n    {BANNER_EXAMPLE}')]

    text = page.lines[banner_line - 1].rstrip()
    if not text.startswith('>'):
        return [error(page.rel, banner_line, 'BANNER MISSING',
                      f'add a banner below the H1, e.g.\n    {BANNER_EXAMPLE}')]
    if not BANNER_RE.match(text):
        return [error(page.rel, banner_line, 'BANNER MALFORMED',
                      'banner must read `> **<type>** · Applies to **<product> V10**'
                      '[ · Prerequisites: <links>]`, where <type> is one of '
                      f'{", ".join(PAGE_TYPES)} and the separator is " · " (U+00B7).'
                      f'\n    e.g. {BANNER_EXAMPLE}')]
    return []


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

    findings = []
    for lineno, line in page.prose:
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


# --------------------------------------------------------------------------

def main(argv):
    merge_base = None
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
        elif arg.startswith('-'):
            print(f'unknown option: {arg}', file=sys.stderr)
            return 2
        else:
            paths.append(arg)

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

    findings = []
    for rel in reported:
        page = pages[rel]
        findings += check_banner(page)
        findings += check_code_blocks(page, strict.get(rel, []))
        findings += check_terminology(page)
    findings += check_headings(pages, reported)

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
