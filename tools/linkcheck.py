#!/usr/bin/env python3
"""Check internal markdown links across the documentation.

Reports four kinds of breakage:

  MISSING FILE    the link target file does not exist
  MISSING ANCHOR  the file exists, but no heading in it slugifies to the anchor
  WRONG CASE      the target exists only under different capitalisation
  ORPHAN          a published page nothing in SUMMARY.md links to

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

LINK_RE = re.compile(r'\[([^\]]*)\]\(([^)]+)\)')
HEADING_RE = re.compile(r'^(#{1,6})\s+(.*?)\s*#*$', re.M)


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


def check(paths):
    anchor_cache = {}
    on_disk = real_paths()
    problems = []
    for src in paths:
        with open(src, encoding='utf-8') as fh:
            content = fh.read()
        rel_src = os.path.relpath(src, ROOT)
        for lineno, line in enumerate(content.splitlines(), 1):
            for label, raw in LINK_RE.findall(line):
                target = unquote(raw.strip())
                if target.startswith(('http://', 'https://', 'mailto:')):
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

    for kind in ('MISSING FILE', 'WRONG CASE', 'MISSING ANCHOR'):
        rows = [p for p in problems if p[0] == kind]
        if not rows:
            continue
        print(f"\n===== {kind} ({len(rows)}) =====")
        for _, src, lineno, target, label in rows:
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
