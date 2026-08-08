#!/usr/bin/env python3
"""Predict GitBook published paths from SUMMARY.md, and check the shape of the tree.

This is deliverable D3 of spec 010, and it began as the evidence behind that spec's
requirements.md §2.  D3 is packaging, not invention: the URL model below was verified
110/110 against the live sitemap before this file moved into tools/.

The model, verified 110/110 against the live sitemap on 2026-08-06:

    <section-slug>/[<ancestor-page-slug>/]*<page-slug>

where the section slug comes from the SUMMARY.md H2 the entry sits under, the
ancestor slugs come from SUMMARY.md list nesting, and every slug is
slug(filename-without-.md).  The path on disk plays no part: contents/ is flat
and every page lives directly in it.

Usage, from the repo root:

    python3 tools/urlmap.py                       # print the tree
    python3 tools/urlmap.py --verify              # check vs the live site
    python3 tools/urlmap.py --redirects OLD_SUMMARY
    python3 tools/urlmap.py --check-shape         # S1/S2/S3, and no leading-space headings
    python3 tools/urlmap.py --check-redirects     # .gitbook.yaml's block, and its bytes

--redirects diffs a previous SUMMARY.md against the current one and emits the
.gitbook.yaml block for every page whose published path moved.  Targets are
repository paths, per GitBook's syntax, which for this repo is always
contents/<FileName>.md.

--check-shape and --check-redirects read only the repository, so they gate CI.
--verify reads the live site and deliberately does not: it would make the build
flaky, and an unreachable authority exits 2 rather than passing.

Exit codes: 0 clean, 1 a check failed, 2 bad arguments or an unreachable authority.
Same contract as linkcheck.py and pagelint.py.
"""

import re
import sys
import urllib.parse
import urllib.request
from pathlib import Path

# tools/urlmap.py, so the repo root is one level up — it was parents[2] while this
# lived in spec/010-information_architecture/.  Getting this wrong is silent: every
# path still resolves, just against the wrong tree.
REPO = Path(__file__).resolve().parents[1]
SITEMAP = "https://brightercommand.gitbook.io/paramore-brighter-documentation/sitemap-pages.xml"
BASE = "https://brightercommand.gitbook.io/paramore-brighter-documentation"

H2_RE = re.compile(r"^\s*##\s+(.+?)\s*$")
LINK_RE = re.compile(r"^(\s*)\*\s*\[(.+?)\]\((/contents/[^)]+)\)")

# Spec 010 design §4's shape rules, Q8's answer.  They count *entries*, not pages:
# a section may hold any number of pages so long as its top level stays readable,
# which is how Outbox and Inbox carries 39 pages behind 9 entries.
MIN_PAGES_PER_SECTION = 2      # S1
MAX_TOP_LEVEL_ENTRIES = 12     # S2
MAX_URL_SEGMENTS = 4           # S3 — MEASURED 2026-08-08, design §17, not assumed.
# S3 was 3 until it was measured, on a rationale that was the absence of evidence:
# "three is the deepest the live site is known to work at".  That is a fact about our
# own SUMMARY.md, not about GitBook.  PRs #83/#84 published a new page at four
# segments and it worked; GitBook's own docs publish 30 pages that deep.  Five is
# still untested, which is why the ceiling is 4 and not simply removed.

GITBOOK_YAML = ".gitbook.yaml"


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


def cmd_check_shape():
    """S1, S2, S3, and the leading-whitespace hazard SUMMARY.md:154 used to carry.

    Every assertion is derived from published_paths(), the function verified 110/110
    against the live sitemap, rather than from a second parse of the same file: a
    top-level entry is exactly a path with two segments, and a section is a path's
    first segment.  Two parsers of one file eventually disagree.
    """
    summary = read_summary(REPO / "SUMMARY.md")
    paths = published_paths(summary)
    failures = []

    # P0-1.  SUMMARY.md:154 read " ## Under the Hood" with a leading space no other
    # heading had.  CommonMark tolerates three; at four it stops being a heading and
    # GitBook would silently fold three pages into the section above, moving their
    # URLs.  H2_RE is deliberately tolerant because it must model GitBook, so this is
    # the assertion that the file does not rely on that tolerance.  The instance is
    # gone from the target tree; the hazard is the class, so the check stays.
    for n, line in enumerate(summary.splitlines(), 1):
        if line.startswith((" ", "\t")) and H2_RE.match(line):
            failures.append(f"SUMMARY.md:{n}: heading carries leading whitespace: {line!r}")

    sections = {}
    for path in paths:
        sections.setdefault(path.split("/")[0], []).append(path)

    for section, members in sorted(sections.items()):
        if len(members) < MIN_PAGES_PER_SECTION:
            failures.append(
                f"S1: section {section!r} holds {len(members)} page(s), "
                f"minimum {MIN_PAGES_PER_SECTION} — a one-page section should be a page")
        entries = [p for p in members if p.count("/") == 1]
        if len(entries) > MAX_TOP_LEVEL_ENTRIES:
            failures.append(
                f"S2: section {section!r} shows {len(entries)} top-level entries, "
                f"maximum {MAX_TOP_LEVEL_ENTRIES} — nest some under a parent page")

    for path in sorted(paths):
        segments = path.count("/") + 1
        if segments > MAX_URL_SEGMENTS:
            failures.append(
                f"S3: {path} publishes at {segments} segments, maximum "
                f"{MAX_URL_SEGMENTS} — four is measured (design §17), five is not")

    for f in failures:
        print(f)

    deepest = max((p.count("/") + 1 for p in paths), default=0)
    widest = max((len([p for p in m if p.count("/") == 1]) for m in sections.values()),
                 default=0)
    print(f"\n{len(failures)} shape failures — {len(paths)} pages, {len(sections)} sections, "
          f"deepest {deepest} of {MAX_URL_SEGMENTS} segments, "
          f"widest {widest} of {MAX_TOP_LEVEL_ENTRIES} top-level entries")
    return 1 if failures else 0


def parse_redirects(text):
    """The flat `redirects:` block, as key -> value, with the line each came from.

    Fifteen lines of parsing rather than a YAML dependency, and the reason is not
    convenience: PyYAML is absent from this environment and `ruby -ryaml` is an
    accident of the machine, but more to the point **a YAML parser would have parsed
    `​structure:` perfectly happily**.  It was a valid key that simply was not the
    key anyone meant, and GitBook therefore never read that block at all.  The byte
    check in cmd_check_redirects is the assertion that actually catches that, and it
    is only possible because nothing here normalises the text first.
    """
    entries = {}
    in_block = False
    for n, line in enumerate(text.splitlines(), 1):
        if not line.strip() or line.lstrip().startswith("#"):
            continue
        if not line.startswith((" ", "\t")):
            in_block = line.rstrip() == "redirects:"
            continue
        if in_block and ":" in line:
            key, _, value = line.partition(":")
            entries[key.strip()] = (value.strip(), n)
    return entries


def cmd_check_redirects():
    """Three assertions, one of which is about bytes rather than meaning."""
    yaml_path = REPO / GITBOOK_YAML
    raw = yaml_path.read_bytes()
    failures = []

    # Assertion 3 first, because it is the one that has actually fired on this repo.
    # Malformed indentation disables redirects silently rather than erroring, and an
    # invisible character in a key is that failure mode at its worst: this file
    # carried U+200B at `structure:` and after `SUMMARY.md` until 2026-08-05, so the
    # block was never read, and it fell back to defaults naming the same two files —
    # which is exactly why nothing ever looked broken.  GitBook's own published
    # example still carries both characters today, with the redirects snippet inside
    # the same code block.  Type config; never paste it.
    for i, byte in enumerate(raw):
        if byte > 126 or (byte < 32 and byte != 10):
            failures.append(
                f"{GITBOOK_YAML}: byte {i} is {hex(byte)}, outside printable ASCII — "
                f"an invisible character in a YAML key fails silently")

    text = raw.decode("utf-8", errors="replace")
    entries = parse_redirects(text)
    published = set(published_paths(read_summary(REPO / "SUMMARY.md")))

    for key, (value, line) in sorted(entries.items()):
        # The value is a REPOSITORY path, not a published one, and GitBook resolves it
        # to wherever that page currently publishes — which is what lets the block be
        # written once instead of re-derived every time a page moves again.
        if not (REPO / value).is_file():
            failures.append(f"{GITBOOK_YAML}:{line}: {key} -> {value} does not exist")
        if key.startswith("/") or value.startswith("/"):
            failures.append(f"{GITBOOK_YAML}:{line}: {key} -> {value} has a leading slash")
        # A key that still publishes is dead weight at best: GitBook resolves a live
        # path before it ever consults this block, so such an entry can never fire.
        if key in published:
            failures.append(
                f"{GITBOOK_YAML}:{line}: {key} still publishes, so this redirect can "
                f"never fire — GitBook resolves a live path before consulting the block")

    for f in failures:
        print(f)

    print(f"\n{len(failures)} redirect failures — {len(entries)} entries, "
          f"{len(raw)} bytes, all printable ASCII"
          if not any("outside printable ASCII" in f for f in failures)
          else f"\n{len(failures)} redirect failures — {len(entries)} entries")
    return 1 if failures else 0


def main(argv):
    if len(argv) == 1:
        return cmd_print()
    if argv[1] == "--verify" and len(argv) == 2:
        return cmd_verify()
    if argv[1] == "--redirects" and len(argv) == 3:
        return cmd_redirects(argv[2])
    if argv[1] == "--check-shape" and len(argv) == 2:
        return cmd_check_shape()
    if argv[1] == "--check-redirects" and len(argv) == 2:
        return cmd_check_redirects()
    print(__doc__, file=sys.stderr)
    return 2


if __name__ == "__main__":
    sys.exit(main(sys.argv))
