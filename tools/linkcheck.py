#!/usr/bin/env python3
"""Check internal markdown links across the documentation.

Reports two kinds of breakage:

  MISSING FILE    the link target file does not exist
  MISSING ANCHOR  the file exists, but no heading in it slugifies to the anchor

Anchors are slugified with GitHub's rules, which GitBook follows: inline
markdown is stripped, the text is lowercased, punctuation is dropped, and each
space becomes a hyphen. Note that runs of spaces are *not* collapsed, so
"Handlers & Pipelines" becomes "handlers--pipelines" with two hyphens.

Usage:
    python3 tools/linkcheck.py            # check the whole repo
    python3 tools/linkcheck.py contents/Glossary.md   # check specific files

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


def check(paths):
    anchor_cache = {}
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
                if anchor:
                    if path not in anchor_cache:
                        anchor_cache[path] = anchors_for(path)
                    if anchor.lower() not in anchor_cache[path]:
                        problems.append(
                            ('MISSING ANCHOR', rel_src, lineno, raw, label))
    return problems


def main(argv):
    if argv:
        paths = [os.path.abspath(p) for p in argv]
        missing = [p for p in paths if not os.path.isfile(p)]
        if missing:
            print("no such file: " + ", ".join(missing), file=sys.stderr)
            return 2
    else:
        paths = list(md_files())

    problems = check(paths)
    if not problems:
        print(f"No broken internal links ({len(paths)} files checked).")
        return 0

    for kind in ('MISSING FILE', 'MISSING ANCHOR'):
        rows = [p for p in problems if p[0] == kind]
        if not rows:
            continue
        print(f"\n===== {kind} ({len(rows)}) =====")
        for _, src, lineno, target, label in rows:
            print(f"{src}:{lineno}  [{label}]({target})")
    print(f"\n{len(problems)} broken link(s) across {len(paths)} files.")
    return 1


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
