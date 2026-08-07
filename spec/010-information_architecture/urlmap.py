#!/usr/bin/env python3
"""Predict GitBook published paths from SUMMARY.md.

This is the evidence behind requirements.md §2 and the prototype for deliverable D3.

The model, verified 110/110 against the live sitemap on 2026-08-06:

    <section-slug>/[<ancestor-page-slug>/]*<page-slug>

where the section slug comes from the SUMMARY.md H2 the entry sits under, the
ancestor slugs come from SUMMARY.md list nesting, and every slug is
slug(filename-without-.md).  The path on disk plays no part: contents/ is flat
and every page lives directly in it.

Usage, from the repo root:

    python3 spec/010-information_architecture/urlmap.py            # print the tree
    python3 spec/010-information_architecture/urlmap.py --verify   # check vs the live site
    python3 spec/010-information_architecture/urlmap.py --redirects OLD_SUMMARY

--redirects diffs a previous SUMMARY.md against the current one and emits the
.gitbook.yaml block for every page whose published path moved.  Targets are
repository paths, per GitBook's syntax, which for this repo is always
contents/<FileName>.md.

Exit codes: 0 clean, 1 a check failed, 2 bad arguments.  Same contract as
linkcheck.py and pagelint.py.
"""

import re
import sys
import urllib.parse
import urllib.request
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
SITEMAP = "https://brightercommand.gitbook.io/paramore-brighter-documentation/sitemap-pages.xml"
BASE = "https://brightercommand.gitbook.io/paramore-brighter-documentation"

H2_RE = re.compile(r"^\s*##\s+(.+?)\s*$")
LINK_RE = re.compile(r"^(\s*)\*\s*\[(.+?)\]\((/contents/[^)]+)\)")


def slug(text):
    """GitBook's slug: lowercase, non-alphanumeric runs to one hyphen, trimmed.

    Verified against all 110 published leaf slugs, including the only awkward
    filename in the corpus, `Requests, Commands and Events.md`.
    """
    if text.endswith(".md"):
        text = text[:-3]
    return re.sub(r"[^a-z0-9]+", "-", text.lower()).strip("-")


def published_paths(summary_text):
    """Map published path -> repository path, for every entry in a SUMMARY.md."""
    paths = {}
    section = None
    stack = []  # (indent, slug) of the entries this one could nest under

    for line in summary_text.splitlines():
        heading = H2_RE.match(line)
        if heading and "](" not in line:
            section = slug(heading.group(1))
            stack = []
            continue

        link = LINK_RE.match(line)
        if not link:
            continue
        if section is None:
            # An entry before any H2 has no section to publish under.
            continue

        indent = len(link.group(1).expandtabs(4))
        target = urllib.parse.unquote(link.group(3))
        leaf = slug(target.rsplit("/", 1)[-1])

        while stack and stack[-1][0] >= indent:
            stack.pop()

        path = "/".join([section] + [s for _, s in stack] + [leaf])
        paths[path] = target.lstrip("/")
        stack.append((indent, leaf))

    return paths


def read_summary(path):
    return Path(path).read_text(encoding="utf-8")


def cmd_print():
    paths = published_paths(read_summary(REPO / "SUMMARY.md"))
    for p in sorted(paths):
        print(f"{p}\t{paths[p]}")
    print(f"\n{len(paths)} pages", file=sys.stderr)
    return 0


def cmd_verify():
    paths = published_paths(read_summary(REPO / "SUMMARY.md"))
    # GitBook returns 403 to a bare urllib User-Agent.
    request = urllib.request.Request(SITEMAP, headers={"User-Agent": "curl/8"})
    try:
        with urllib.request.urlopen(request, timeout=30) as fh:
            xml = fh.read().decode("utf-8")
    except Exception as exc:  # noqa: BLE001 - the authority being unreachable is not a pass
        print(f"could not fetch the sitemap: {exc}", file=sys.stderr)
        return 2

    actual = set()
    for loc in re.findall(r"<loc>([^<]+)</loc>", xml):
        trimmed = loc.strip()[len(BASE):].strip("/")
        if trimmed:
            actual.add(trimmed)

    predicted = set(paths)
    only_predicted = sorted(predicted - actual)
    only_published = sorted(actual - predicted)

    for p in only_predicted:
        print(f"PREDICTED BUT NOT PUBLISHED: {p}")
    for p in only_published:
        print(f"PUBLISHED BUT NOT PREDICTED: {p}")

    print(f"\npredicted {len(predicted)}, published {len(actual)}, "
          f"{len(predicted & actual)} agree")
    return 1 if (only_predicted or only_published) else 0


def cmd_redirects(old_summary):
    old = published_paths(read_summary(old_summary))
    new = published_paths(read_summary(REPO / "SUMMARY.md"))

    new_by_file = {repo_path: pub for pub, repo_path in new.items()}

    moved, dropped = [], []
    for old_pub, repo_path in sorted(old.items()):
        if repo_path not in new_by_file:
            dropped.append((old_pub, repo_path))
        elif new_by_file[repo_path] != old_pub:
            moved.append((old_pub, repo_path))

    if dropped:
        # A page that leaves SUMMARY.md loses its URL with nowhere to send readers.
        # Refuse to emit a partial block rather than silently dropping the redirect.
        for old_pub, repo_path in dropped:
            print(f"ERROR: {repo_path} published at {old_pub} but is absent from the new "
                  f"SUMMARY.md; it would 404 with no redirect target", file=sys.stderr)
        return 1

    if not moved:
        print("# no published path changed; no redirects needed", file=sys.stderr)
        return 0

    print("redirects:")
    for old_pub, repo_path in moved:
        print(f"  {old_pub}: {repo_path}")
    print(f"\n# {len(moved)} of {len(old)} pages moved", file=sys.stderr)
    return 0


def main(argv):
    if len(argv) == 1:
        return cmd_print()
    if argv[1] == "--verify" and len(argv) == 2:
        return cmd_verify()
    if argv[1] == "--redirects" and len(argv) == 3:
        return cmd_redirects(argv[2])
    print(__doc__, file=sys.stderr)
    return 2


if __name__ == "__main__":
    sys.exit(main(sys.argv))
