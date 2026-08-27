#!/usr/bin/env python3
"""Survey Brighter's configuration surfaces at a released ref.

Spec 012 needs to know, before it writes a single table, how many options exist
and what *shape* they come in. That second question is the one that bites: this
programme's own README assumed defaults live in property initialisers and are
readable by reflecting over an instantiated options object. For publications
that is true. For subscriptions -- the surface a reader most needs a table for --
it is false: `KafkaSubscription` takes 30 constructor parameters, 27 of them
defaulted, and exposes get-only properties. A property-only count sees none of
them.

So this script counts both shapes and says which is which. It reads source at a
git ref via `git show`, so it never touches the sibling working tree -- which is
on another agent's branch and 173 commits past the release.

It is a *source* survey and it says so. The real checker (`optioncheck`, spec
012's D-tool) reflects over restored NuGet packages; this exists to size the work
and to record the two shapes, not to be that tool.

    python3 spec/012-configuration_reference/survey.py            # default ref 10.7.0
    python3 spec/012-configuration_reference/survey.py --ref 10.8.0
    python3 spec/012-configuration_reference/survey.py --repo ../Brighter
"""

import argparse
import re
import subprocess
import sys
from collections import Counter

DEFAULT_REF = "10.7.0"
DEFAULT_REPO = "../Brighter"

# Files whose names say "I am a configuration surface".
SURFACE_RE = re.compile(r"(Subscription|Publication|Configuration|Options|Connection)\.cs$")

# A public instance property with an accessible setter or init accessor.
PROP_RE = re.compile(
    r"^\s*public\s+(?!class|record|struct|interface|enum|delegate|const\b)"
    r"(?:virtual\s+|override\s+|new\s+|required\s+|sealed\s+)*"
    r"([\w<>\[\],\?\.]+)\s+(\w+)\s*\{[^}]*\b(?:set|init)\b",
    re.M,
)

# A public instance property with no setter -- ctor-assigned, so its default
# lives in the constructor parameter rather than on the property.
GETONLY_RE = re.compile(
    r"^\s*public\s+(?!class|record|struct|interface|enum|delegate|const\b)"
    r"(?:virtual\s+|override\s+|new\s+|required\s+|sealed\s+)*"
    r"([\w<>\[\],\?\.]+)\s+(\w+)\s*\{\s*get;\s*\}",
    re.M,
)


def git_show(repo, ref, path):
    r = subprocess.run(
        ["git", "-C", repo, "show", f"{ref}:{path}"],
        capture_output=True, text=True,
    )
    return r.stdout if r.returncode == 0 else ""


def list_files(repo, ref):
    r = subprocess.run(
        ["git", "-C", repo, "ls-tree", "-r", "--name-only", ref, "--", "src/"],
        capture_output=True, text=True,
    )
    if r.returncode != 0:
        sys.exit(f"cannot list tree at {ref}: {r.stderr.strip()}")
    return [f for f in r.stdout.split() if f.endswith(".cs")]


def split_params(body):
    """Split a parameter list on top-level commas (generics and defaults nest)."""
    parts, depth, cur = [], 0, ""
    for ch in body:
        if ch in "(<[":
            depth += 1
        elif ch in ")>]":
            depth -= 1
        if ch == "," and depth == 0:
            parts.append(cur)
            cur = ""
        else:
            cur += ch
    if cur.strip():
        parts.append(cur)
    return [p.strip() for p in parts if p.strip()]


def widest_ctor(source, cls):
    """The widest public constructor's parameters -- Brighter's subscriptions
    offer a narrow convenience ctor and a wide one; the wide one is the surface."""
    best = []
    for m in re.finditer(rf"\bpublic\s+{re.escape(cls)}\s*\(", source):
        i, depth, start = m.end(), 1, m.end()
        while i < len(source) and depth:
            if source[i] == "(":
                depth += 1
            elif source[i] == ")":
                depth -= 1
            i += 1
        params = split_params(source[start:i - 1])
        if len(params) > len(best):
            best = params
    return best


def main():
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--ref", default=DEFAULT_REF, help=f"git ref to read (default {DEFAULT_REF})")
    ap.add_argument("--repo", default=DEFAULT_REPO, help=f"Brighter checkout (default {DEFAULT_REPO})")
    args = ap.parse_args()

    files = [f for f in list_files(args.repo, args.ref) if SURFACE_RE.search(f)]
    if not files:
        sys.exit(f"no configuration surfaces found at {args.ref} -- is the ref right?")

    print(f"Brighter configuration surfaces at ref {args.ref}\n")
    print("Counting TWO shapes, because the surface uses both:")
    print("  props  = public properties with a set/init accessor (publications, options classes)")
    print("  ctor   = widest public constructor's parameters     (subscriptions)")
    print("  deflt  = of those ctor params, how many carry a default value\n")

    rows, by_asm = [], Counter()
    for path in sorted(files):
        source = git_show(args.repo, args.ref, path)
        if not source:
            continue
        cls = path.rsplit("/", 1)[-1][:-3]
        props = PROP_RE.findall(source)
        getonly = GETONLY_RE.findall(source)
        ctor = widest_ctor(source, cls)
        defaulted = [p for p in ctor if "=" in p]
        if not (props or ctor):
            continue
        asm = path.split("/")[1]
        # Of the defaulted params, those defaulting to `null`: their real default,
        # if any, is applied in the constructor body by a `??` and is NOT recoverable
        # from ParameterInfo.DefaultValue. See requirements.md 5.1.
        nulls = [p for p in defaulted if re.search(r"=\s*null\s*$", p)]
        rows.append((asm, cls, len(props), len(getonly), len(ctor), len(defaulted), len(nulls)))
        # A type's reader-facing option count is whichever shape it actually uses.
        by_asm[asm] += max(len(props), len(ctor))

    print(f"{'assembly':<52} {'type':<34} {'props':>5} {'get-only':>8} {'ctor':>5} {'deflt':>5} {'=null':>5}")
    print("-" * 119)
    for asm, cls, np_, ng, nc, nd, nn in sorted(rows, key=lambda r: (-max(r[2], r[4]), r[0])):
        print(f"{asm:<52} {cls:<34} {np_:>5} {ng:>8} {nc:>5} {nd:>5} {nn:>5}")

    ctor_driven = [r for r in rows if r[4] > r[2]]
    prop_driven = [r for r in rows if r[2] >= r[4] and r[2] > 0]

    print(f"\n{len(rows)} configuration types at {args.ref}")
    print(f"  {len(ctor_driven):>3} are constructor-driven  "
          f"({sum(r[4] for r in ctor_driven)} params, {sum(r[5] for r in ctor_driven)} defaulted)")
    print(f"  {len(prop_driven):>3} are property-driven     "
          f"({sum(r[2] for r in prop_driven)} settable properties)")
    print(f"\nTOTAL reader-facing options: {sum(by_asm.values())}")
    print("\nA property-only reflection pass would see "
          f"{sum(r[2] for r in rows)} of them and miss "
          f"{sum(by_asm.values()) - sum(r[2] for r in rows)}.")

    tot_def = sum(r[5] for r in rows)
    tot_null = sum(r[6] for r in rows)
    if tot_def:
        pct = round(100 * tot_null / tot_def)
        ctor_def = sum(r[5] for r in ctor_driven)
        print(f"\nOf {tot_def} defaulted constructor parameters across ALL {len(rows)} types, "
              f"{tot_null} ({pct}%)\ndefault to `null`. (The {ctor_def} figure above counts only "
              f"the {len(ctor_driven)} constructor-driven\ntypes -- two populations, two totals, "
              f"and they are not the same quantity.)\n"
              f"\nFor a `null` default, ParameterInfo.DefaultValue reports `null` while the real "
              f"default\n-- where there is one -- is applied in the constructor body by a `??`. "
              f"Reading the\nsignature alone would document them WRONG, not merely incompletely. "
              f"The only route\nthat recovers them is instantiating the type and reading the "
              f"property back.\nSee requirements.md 5.1.")


if __name__ == "__main__":
    main()
