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
on another agent's branch and well past the release.

It is a *source* survey and it says so. The real checker (`optioncheck`, spec
012's D-tool) reflects over restored NuGet packages; this exists to size the work
and to record the two shapes, not to be that tool.

    python3 spec/012-configuration_reference/survey.py            # default ref 10.7.0
    python3 spec/012-configuration_reference/survey.py --ref 10.8.0
    python3 spec/012-configuration_reference/survey.py --repo ../Brighter
    python3 spec/012-configuration_reference/survey.py --tsv      # one row per type

TASK 1.5 REBUILT THE PARSER, and the totals moved. Three defects, all of the
same kind -- the script was reading something adjacent to the type it named:

1. **C# primary constructors were invisible.** `widest_ctor` matched
   `public TypeName(` *inside* a class body, and a primary constructor is on the
   declaration line. Design §12.5 measured the cost at 13 surface types and 40
   parameters, two of them absent from the count altogether.
2. **The class was assumed to be the file.** `RocketMqSubscription.cs` declares
   `RocketSubscription`; `AWSMessagingGatewayConfiguration.cs` declares
   `AWSMessagingGatewayConnection`; `MQTTMessagingGatewayConfiguration.cs`
   declares `MqttMessagingGatewayConfiguration` and two more beside it. The old
   script looked for a constructor named after the *file* and, finding none,
   silently counted properties only.
3. **Properties were counted per FILE, not per class**, so a file holding three
   classes gave all three's properties to one of them.

The population is now every public non-generic class whose *own name* says it is
a configuration surface, which is the population `optioncheck` will reflect over.
`probes/CountsProbe.cs` prints the same quantity from the assemblies; the two
instruments agree type by type, and the reconciliation is in probes/README.md.
"""

import argparse
import re
import subprocess
import sys
from collections import Counter

DEFAULT_REF = "10.7.0"
DEFAULT_REPO = "../Brighter"

# A type name that says "I am a configuration surface". Applied to the CLASS,
# not to the file: task 1.5 found four files whose name is not the name of the
# type inside them.
SURFACE_RE = re.compile(r"(Subscription|Publication|Configuration|Options|Connection)$")

# A class or record declaration, capturing the name and the position from which
# a primary-constructor parameter list would start.
DECL_RE = re.compile(
    r"^[ \t]*public\s+"
    r"(?:(?:sealed|abstract|partial|static|unsafe|readonly)\s+)*"
    r"(?:class|record\s+class|record|struct)\s+"
    r"(\w+)",
    re.M,
)

# A public INSTANCE property declaration, up to the `{` that opens its accessor
# block. Three things in this pattern were each worth a wrong number:
#
#   * the type may carry a generic argument list, and that list contains a SPACE
#     -- `Dictionary<string, object>?`. The pattern that did not allow one
#     silently dropped four properties, across `Publication`,
#     `InboxConfiguration`, `AsyncApiOptions` and `AzureBlobLuggageOptions`;
#   * `static` is excluded. `RedisMessagingGatewayConfiguration` declares a
#     static property, which is not an option on an instance and which
#     reflection does not return for one;
#   * the accessor block is taken by BRACE MATCHING rather than by `[^}]*`,
#     because a full property spanning several lines -- `EntryLimit` on
#     `InMemoryBoxConfiguration`, `BucketName` on two luggage options -- has
#     braces inside it.
PROP_RE = re.compile(
    r"^[ \t]*public\s+(?!class|record|struct|interface|enum|delegate|const\b|static\b|event\b)"
    r"(?:virtual\s+|override\s+|new\s+|required\s+|sealed\s+)*"
    r"([\w\.\?\[\]]+(?:<[^<>]*(?:<[^<>]*>)?[^<>]*>)?[\?\[\]]*)\s+(\w+)\s*\{",
    re.M,
)

# A setter the READER can reach. `{ get; private set; }` and
# `{ get; internal set; }` are not options a reader sets, and reflection agrees:
# `PropertyInfo.SetMethod.IsPublic` is false for both.
PUBLIC_SET_RE = re.compile(
    r"(?<!private )(?<!protected )(?<!internal )\b(?:set|init)\s*(?:;|=>|\{)")
GET_RE = re.compile(r"\bget\s*(?:;|=>|\{)")


def git_show(repo, ref, path):
    r = subprocess.run(
        ["git", "-C", repo, "show", f"{ref}:{path}"],
        capture_output=True, text=True,
    )
    return r.stdout if r.returncode == 0 else ""


def candidate_files(repo, ref):
    """Files declaring a surface-named type, found by content rather than by name.

    `git grep` over the ref costs one process; `git show` on every .cs file under
    src/ costs two thousand. The pattern is deliberately loose -- it selects
    files to parse, and the parser decides what counts. No `\\b` in it: git's ERE
    does not support one, and it fails by matching NOTHING rather than by erroring.
    """
    r = subprocess.run(
        ["git", "-C", repo, "grep", "-l", "-E",
         r"public [A-Za-z ]*(class|record|struct) [A-Za-z0-9_]*"
         r"(Subscription|Publication|Configuration|Options|Connection)",
         ref, "--", "src/"],
        capture_output=True, text=True,
    )
    if r.returncode not in (0, 1):
        sys.exit(f"cannot search tree at {ref}: {r.stderr.strip()}")
    # `git grep <ref>` prefixes each path with "<ref>:".
    return sorted(line.split(":", 1)[1] for line in r.stdout.splitlines() if ":" in line)


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


def matched(source, i, opener, closer):
    """From the opening delimiter at `i`, the index just past its partner."""
    depth = 0
    while i < len(source):
        if source[i] == opener:
            depth += 1
        elif source[i] == closer:
            depth -= 1
            if depth == 0:
                return i + 1
        i += 1
    return len(source)


def declarations(source):
    """Every public class in `source`: name, primary-ctor params, and body.

    Generic types are skipped. They are not a surface a reader configures --
    `RocketMqSubscription<T>` and `Subscription<T>` both exist only to type a
    non-generic base -- and reflection would report them separately, which would
    make the two instruments disagree by construction.
    """
    out = []
    for m in DECL_RE.finditer(source):
        name = m.group(1)
        i = m.end()

        # Generic parameter list, if any -- and if there is one, skip the type.
        while i < len(source) and source[i] in " \t":
            i += 1
        if i < len(source) and source[i] == "<":
            continue

        primary = []
        if i < len(source) and source[i] == "(":
            end = matched(source, i, "(", ")")
            primary = split_params(source[i + 1:end - 1])
            i = end

        # Skip a base list, then take the body by brace matching. A record with
        # no body ends in `;` and has no members beyond its primary constructor.
        brace = source.find("{", i)
        semi = source.find(";", i)
        if brace == -1 or (semi != -1 and semi < brace):
            out.append((name, primary, ""))
            continue

        out.append((name, primary, source[brace:matched(source, brace, "{", "}")]))
    return out


def properties(body):
    """A class body's public instance properties, split settable / get-only.

    A get-only property is ctor-assigned, so its default lives in the
    constructor parameter rather than on the property -- which is the whole
    reason this script counts two shapes.
    """
    settable, readonly = [], []
    for m in PROP_RE.finditer(body):
        block = body[m.end() - 1:matched(body, m.end() - 1, "{", "}")]
        if PUBLIC_SET_RE.search(block):
            settable.append((m.group(1), m.group(2)))
        elif GET_RE.search(block):
            readonly.append((m.group(1), m.group(2)))
    return settable, readonly


def widest_ctor(body, cls):
    """The widest classic constructor's parameters -- Brighter's subscriptions
    offer a narrow convenience ctor and a wide one; the wide one is the surface."""
    best = []
    for m in re.finditer(rf"\bpublic\s+{re.escape(cls)}\s*\(", body):
        params = split_params(body[m.end():matched(body, m.end() - 1, "(", ")") - 1])
        if len(params) > len(best):
            best = params
    return best


def survey(repo, ref):
    rows = []
    for path in candidate_files(repo, ref):
        source = git_show(repo, ref, path)
        if not source:
            continue
        asm = path.split("/")[1]
        for name, primary, body in declarations(source):
            if not SURFACE_RE.search(name):
                continue
            props, getonly = properties(body)
            classic = widest_ctor(body, name)
            ctor = primary if len(primary) > len(classic) else classic
            if not (props or ctor):
                continue
            defaulted = [p for p in ctor if "=" in p]
            # Of the defaulted params, those defaulting to `null`: their real
            # default, if any, is applied in the constructor body by a `??` and
            # is NOT recoverable from ParameterInfo.DefaultValue. Requirements
            # §5.1, and probe 1.2 measured it.
            nulls = [p for p in defaulted if re.search(r"=\s*null\s*$", p)]
            rows.append((asm, name, len(props), len(getonly),
                         len(ctor), len(defaulted), len(nulls)))
    return rows


def main():
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--ref", default=DEFAULT_REF, help=f"git ref to read (default {DEFAULT_REF})")
    ap.add_argument("--repo", default=DEFAULT_REPO, help=f"Brighter checkout (default {DEFAULT_REPO})")
    ap.add_argument("--tsv", action="store_true",
                    help="one row per type, for diffing against probes/CountsProbe.cs")
    args = ap.parse_args()

    rows = survey(args.repo, args.ref)
    if not rows:
        sys.exit(f"no configuration surfaces found at {args.ref} -- is the ref right?")

    if args.tsv:
        print("assembly\ttype\tprops\tctor\tmax")
        for asm, cls, np_, _ng, nc, _nd, _nn in sorted(rows, key=lambda r: (r[0], r[1])):
            print(f"{asm}\t{cls}\t{np_}\t{nc}\t{max(np_, nc)}")
        return

    by_asm = Counter()
    for asm, _cls, np_, _ng, nc, _nd, _nn in rows:
        # A type's reader-facing option count is whichever shape it actually uses.
        by_asm[asm] += max(np_, nc)

    print(f"Brighter configuration surfaces at ref {args.ref}\n")
    print("Counting TWO shapes, because the surface uses both:")
    print("  props  = public properties with a set/init accessor (publications, options classes)")
    print("  ctor   = widest constructor's parameters, primary or classic (subscriptions)")
    print("  deflt  = of those ctor params, how many carry a default value\n")

    print(f"{'assembly':<52} {'type':<38} {'props':>5} {'get-only':>8} {'ctor':>5} {'deflt':>5} {'=null':>5}")
    print("-" * 123)
    for asm, cls, np_, ng, nc, nd, nn in sorted(rows, key=lambda r: (-max(r[2], r[4]), r[0])):
        print(f"{asm:<52} {cls:<38} {np_:>5} {ng:>8} {nc:>5} {nd:>5} {nn:>5}")

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
              f"MEASURED by probe\n1.2: of Subscription's 6 `null` defaults, 4 come back as a "
              f"value and 2 really are null.\nThe only route that recovers them is instantiating "
              f"the type and reading the property\nback. See requirements.md 5.1.")


if __name__ == "__main__":
    main()
