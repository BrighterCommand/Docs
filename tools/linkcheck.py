#!/usr/bin/env python3
"""Check internal markdown links across the documentation.

Reports six kinds of breakage:

  MISSING FILE    the link target file does not exist
  MISSING ANCHOR  the file exists, but no heading in it slugifies to the anchor
  WRONG CASE      the target exists only under different capitalisation
  LEGACY HTML     a link to a `.html` path, which the published site never serves
  EMPTY TARGET    a link whose target is `#` or empty -- it goes nowhere
  ORPHAN          a published page nothing in SUMMARY.md links to

LEGACY HTML is the fault class this tool used to *create*. Only `.md` targets
were resolved and everything else was skipped, so links left over from the
pre-GitBook site were not merely unchecked but reported as fine -- one of them
naming a page that has never existed. An extension is not a reason to skip a
link; it is the reason to look at it.

WRONG CASE matters because macOS and Windows resolve paths case-insensitively
while GitBook and GitHub do not. A link that works on the machine that wrote it
404s once published, and a plain os.path.isfile() check cannot see it.

ORPHAN catches the opposite failure: the link resolves, but no reader can reach
the page, because it never made it into the table of contents. Anchors and file
existence say nothing about reachability, so this is checked separately by
walking SUMMARY.md. Every page under contents/ must be reachable; there are no
exemptions, so a page that is not navigable has to be either linked or deleted.

Anchors are slugified with GitHub's rules, which GitBook follows: inline
markdown is stripped, the text is lowercased, punctuation is dropped, and each
space becomes a hyphen. Note that runs of spaces are *not* collapsed, so
"Handlers & Pipelines" becomes "handlers--pipelines" with two hyphens.

Usage:
    python3 tools/linkcheck.py            # check the whole repo
    python3 tools/linkcheck.py contents/Glossary.md   # check specific files

Orphans are only reported on a whole-repo run: the check needs every page in
view to know what is missing, so passing explicit paths skips it.

Exit code is 1 when anything is broken, 0 when clean, so it can gate CI.
"""
import os
import re
import sys
from collections import defaultdict
from urllib.parse import unquote

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# Directories that hold no published documentation.
SKIP_DIRS = {'.git', '.github', '.claude', '.repomix', 'spec', 'node_modules'}

# Files whose links are deliberate placeholders, not real targets.
SKIP_FILES = {'CLAUDE.md', 'PROMPT.md'}

TOC = 'SUMMARY.md'

# DOTALL, because a link's *text* may wrap across lines and this corpus hard-wraps
# its prose. Extracted per line, 27 links in the corpus were invisible to this
# tool — including one dead link and the only two `.html` links a grep for
# `](...html` on a single line also missed. The label cannot cross a `]`, so a
# stray bracket in prose still cannot swallow the paragraph after it, and the
# target cannot cross a newline, so an unclosed `](` cannot swallow the rest of
# the file. Spaces stay legal inside a target: one page in this corpus is called
# `Requests, Commands and Events.md`, and SUMMARY.md links it by that name.
LINK_RE = re.compile(r'\[([^\]]*)\]\(([^)\n]+)\)', re.S)
HEADING_RE = re.compile(r'^(#{1,6})\s+(.*?)\s*#*$', re.M)

# The published site serves no `.html` path: GitBook renders every page at an
# extensionless URL. Links carrying the extension are survivals from the
# pre-GitBook site, and they are worse than an ordinary broken link, because
# `.md` is the only extension resolved below -- so they have been skipped, and
# have been green, in every run this checker has ever made.
HTML_RE = re.compile(r'\.html$', re.I)


def slug(text):
    """Convert heading text to its GitHub/GitBook anchor."""
    text = re.sub(r'`([^`]*)`', r'\1', text)
    text = re.sub(r'\*\*([^*]*)\*\*', r'\1', text)
    text = re.sub(r'\*([^*]*)\*', r'\1', text)
    text = re.sub(r'__([^_]*)__', r'\1', text)
    text = re.sub(r'\[([^\]]*)\]\([^)]*\)', r'\1', text)
    text = text.strip().lower()
    text = re.sub(r'[^\w\s-]', '', text)
    # GitHub replaces each space individually; it does not collapse runs
    return text.replace(' ', '-')


def anchors_for(path):
    """All anchors a file offers, including GitHub's -1/-2 duplicate suffixes."""
    try:
        with open(path, encoding='utf-8') as fh:
            body = fh.read()
    except OSError:
        return set()
    seen = defaultdict(int)
    out = set()
    for _, text in HEADING_RE.findall(body):
        s = slug(text)
        n = seen[s]
        seen[s] += 1
        out.add(s if n == 0 else f"{s}-{n}")
    return out


def md_files():
    for dirpath, dirnames, filenames in os.walk(ROOT):
        dirnames[:] = [d for d in dirnames if d not in SKIP_DIRS]
        for fn in sorted(filenames):
            if fn.endswith('.md') and fn not in SKIP_FILES:
                yield os.path.join(dirpath, fn)


def real_paths():
    """Map lowercased repo-relative path -> its true on-disk spelling."""
    out = {}
    for p in md_files():
        rel = os.path.relpath(p, ROOT)
        out[rel.lower()] = rel
    return out


def linked_from_toc():
    """Repo-relative paths that SUMMARY.md links to, in its own spelling."""
    toc = os.path.join(ROOT, TOC)
    if not os.path.isfile(toc):
        return None
    with open(toc, encoding='utf-8') as fh:
        body = fh.read()
    out = set()
    for _, raw in LINK_RE.findall(body):
        target = unquote(raw.strip()).split('#')[0]
        if not target.endswith('.md'):
            continue
        out.add(os.path.normpath(target.lstrip('/')))
    return out


def orphans():
    """Published pages that SUMMARY.md never links to."""
    listed = linked_from_toc()
    if listed is None:
        return []
    listed = {p.lower() for p in listed}
    out = []
    for p in md_files():
        rel = os.path.relpath(p, ROOT)
        # Only pages under contents/ are navigable; SUMMARY.md itself is the TOC.
        if os.path.dirname(rel) != 'contents':
            continue
        if rel.lower() not in listed:
            out.append(rel)
    return sorted(out)


def html_advice(target, on_disk):
    """What to replace a `.html` link with, resolved rather than guessed.

    The obvious rewrite is the target with its extension swapped, and for one
    of the targets in this corpus that lands on WRONG CASE:
    `CommandsCommandDispatcherAndProcessor.html` against a file spelled
    `...DispatcherandProcessor.md`. Reporting a fault and then advising a second
    fault would be worse than saying nothing, so the counterpart is looked up by
    its lowercased path and quoted in the spelling that is on disk.

    The anchor is deliberately *not* carried across. These links predate the
    headings they point into -- `#command-processor` names a section now called
    "The Command Processor Pattern" -- so an anchor that survives the rewrite is
    an anchor nobody checked. Once the link ends in `.md` the anchor rules apply
    to it, which is the point of converting them.
    """
    base = target.split('#')[0]
    counterpart = on_disk.get(os.path.join('contents', base[:-5] + '.md').lower())
    if not counterpart:
        return (f'no page serves {base}; the published site has no .html paths, '
                'so this link is dead and has never been checked')
    advice = f'the published site has no .html paths; link /{counterpart}'
    if '#' in target:
        advice += ' and re-derive the anchor against that page\'s headings'
    return advice


def check(paths):
    anchor_cache = {}
    on_disk = real_paths()
    problems = []
    for src in paths:
        with open(src, encoding='utf-8') as fh:
            content = fh.read()
        rel_src = os.path.relpath(src, ROOT)
        for match in LINK_RE.finditer(content):
            label, raw = match.group(1), match.group(2)
            # The line the *target* sits on, not the line the text opens on:
            # a wrapped link is fixed where its URL is.
            lineno = content.count('\n', 0, match.start(2)) + 1
            label = ' '.join(label.split())
            target = unquote(raw.strip())
            if target.startswith(('http://', 'https://', 'mailto:')):
                continue
            # `[text](#)` is a link that goes nowhere. It reaches a reader as a
            # live link, and it used to reach this checker as a same-page link
            # with an empty anchor, which the anchor test below skips because
            # there is nothing to look up. Nothing about it is malformed; it
            # simply points at no heading on any page.
            if not target.strip('#').strip():
                problems.append(('EMPTY TARGET', rel_src, lineno, raw, label))
                continue
            if HTML_RE.search(target.split('#')[0]):
                problems.append(
                    ('LEGACY HTML', rel_src, lineno, raw,
                     html_advice(target, on_disk)))
                continue
            if target.startswith('#'):
                path, anchor = src, target[1:]
            else:
                path, _, anchor = target.partition('#')
                if not path:
                    continue
                if path.startswith('/'):
                    path = os.path.join(ROOT, path.lstrip('/'))
                else:
                    path = os.path.normpath(
                        os.path.join(os.path.dirname(src), path))
            if not path.endswith('.md'):
                continue
            if not os.path.isfile(path):
                problems.append(('MISSING FILE', rel_src, lineno, raw, label))
                continue
            # isfile() is case-insensitive on macOS/Windows, so a link that
            # resolves here can still 404 on GitBook. Compare spellings.
            rel_target = os.path.relpath(path, ROOT)
            actual = on_disk.get(rel_target.lower())
            if actual and actual != rel_target:
                problems.append(
                    ('WRONG CASE', rel_src, lineno, raw, f'file is {actual}'))
                continue
            if anchor:
                if path not in anchor_cache:
                    anchor_cache[path] = anchors_for(path)
                if anchor.lower() not in anchor_cache[path]:
                    problems.append(
                        ('MISSING ANCHOR', rel_src, lineno, raw, label))
    return problems


def main(argv):
    whole_repo = not argv
    if argv:
        paths = [os.path.abspath(p) for p in argv]
        missing = [p for p in paths if not os.path.isfile(p)]
        if missing:
            print("no such file: " + ", ".join(missing), file=sys.stderr)
            return 2
    else:
        paths = list(md_files())

    problems = check(paths)
    # Reachability needs every page in view, so only check it on a full run.
    stranded = orphans() if whole_repo else []

    if not problems and not stranded:
        print(f"No broken internal links ({len(paths)} files checked).")
        return 0

    for kind in ('MISSING FILE', 'WRONG CASE', 'MISSING ANCHOR', 'LEGACY HTML',
                 'EMPTY TARGET'):
        rows = [p for p in problems if p[0] == kind]
        if not rows:
            continue
        print(f"\n===== {kind} ({len(rows)}) =====")
        for _, src, lineno, target, label in rows:
            # LEGACY HTML's fifth field is advice on what to write instead, not
            # the link's text, so it goes after the link rather than inside it.
            if kind == 'LEGACY HTML':
                print(f"{src}:{lineno}  ({target})\n    {label}")
            else:
                print(f"{src}:{lineno}  [{label}]({target})")

    if stranded:
        print(f"\n===== ORPHAN ({len(stranded)}) =====")
        print(f"published, but nothing in {TOC} links to them:")
        for rel in stranded:
            print(f"  {rel}")

    total = len(problems) + len(stranded)
    print(f"\n{total} problem(s) across {len(paths)} files.")
    return 1


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
