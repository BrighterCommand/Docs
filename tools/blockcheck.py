#!/usr/bin/env python3
"""Enumerate every C# code block in the documentation, and extract it verbatim.

This is the Python half of the compile gate. It finds the blocks, classifies
each one by the wrapper it needs, and hands the text to `tools/blockcheck`, the
Roslyn tool that compiles them one at a time. Nothing here compiles anything.

WHY IT ENUMERATES THROUGH pagelint.Page, AND NOT THROUGH A GREP
--------------------------------------------------------------
`grep -rc '^```csharp$' contents/` finds 835 blocks across 117 pages. The parser
this repository already ships finds 985 across 145. The 150-block gap is fence
spelling -- "``` csharp" with a space (140), indented (8), "``` c#" (1), and one
with a trailing space -- and 28 of those pages hold NOTHING BUT irregular
fences, so a grep-shaped extractor reports them as having no C# at all.
`KafkaConfiguration.md` is 20 blocks that a grep cannot see; `ShowMeTheCode.md`
is 6 more.

That is spec 016's friction 53, and spec 014's probe met the same defect in a
harder form: it wrote its own fence regex, silently skipped every spaced fence,
and reported a corpus containing a KNOWN dead symbol as clean. So there is one
fence parser in this repository and this tool does not own it. Never write a
second one.

WHAT THE EXTRACTION GUARANTEES
------------------------------
The body of a block is copied line for line, unchanged. A wrapper may be added
AROUND it (see `classify`), never inside it: what the compiler reads is what the
page tells a reader to type. Task 1.7's `--verify-extraction` is what proves
that claim rather than asserting it.

ENUMERATIONS END IN A NEWLINE
-----------------------------
Every listing mode prints one record per line, each terminated. An enumeration
written without a trailing newline loses its last member to `while read` -- 60
emitted, 59 measured, which is friction 55 and was found in this programme's own
instruments. Row output goes to stdout and nothing else does, so `wc -l` of a
listing is the count of the thing listed.

Usage:
    python3 tools/blockcheck.py --list             # page<TAB>ordinal<TAB>shape
    python3 tools/blockcheck.py --show <page> <n>  # block n of that page, verbatim
    python3 tools/blockcheck.py --stage <dir>      # one .cs per block, plus index.tsv
    python3 tools/blockcheck.py --list-scaffold    # what the pages were given
    python3 tools/blockcheck.py --report [file]    # compile everything, one row per block
    python3 tools/blockcheck.py --verify-extraction [dir]   # is it byte-identical?

Exit code is 0 when the run has something to say, 1 when the corpus is wrong,
and 2 when NOTHING WAS CHECKED -- the contract in `tools/README.md`, shared with
linkcheck, pagelint, symbolcheck and urlmap. An empty enumeration is exit 2, not
exit 0: a tool that silently degrades to zero blocks passes every corpus ever
written, and it does it in the direction that looks like success.

THE OPT-OUT, AND WHY IT DEMANDS A REASON
----------------------------------------
A block excuses itself from the gate with a comment on its own line, above the
block it excuses:

    <!-- blockcheck: skip V9 form, required by CLAUDE.md -->

It binds the NEXT C# block, never the page, and the reason is part of the
syntax: a marker without one binds nothing and is reported as an error. The two
opt-outs this repository already has -- pagelint's `allow-serviceactivator` and
symbolcheck's `allow <name>` -- need no reason because each names what it
silences and the page discusses that name. A block that fails to compile can
fail for a dozen reasons, so here the reason is the only thing a later reader
can check. Every skip is printed, with its reason, on green runs too.

`--report` IS THE GATE, and `tools/blockcheck/baseline.tsv` is what it holds
the corpus to. The baseline must EQUAL the set of blocks that build, in both
directions, and each disagreement is a finding: a listed block that no longer
builds, a listed block that no longer exists, a block that builds and is not
listed, and a listed block now compiled with a different scaffold. The first is
the regression the gate exists for. The second stops a deleted page shrinking
the corpus while the run still says `0 findings`. The third is the ratchet: a
repair that makes a block build brings its row in the same PR, so the bar can
only rise. A failing block with NO row is not a finding -- that is the debt the
baseline exists to make bearable. A MALFORMED MARKER IS A FINDING regardless: it
is a claim about the corpus that is wrong on its own terms. The no-argument form
is not the gate -- it exits 2, because a run that checked nothing must not look
green.

Every state that exits 2 is listed in one place, above `mode_report`.
"""
import glob
import hashlib
import os
import re
import shutil
import subprocess
import sys
import tempfile

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

# One fence parser, one tag set, one `using` rule. CSHARP_TAGS is
# {'csharp', 'cs', 'c#'} and USING_RE is the regex rule 6 already reads C#
# blocks with -- it admits `using static X;` and `using Alias = X.Y;` while
# excluding the `using (...)` statement and the `using var x = ...;`
# declaration, which a hand-rolled version of it gets wrong.
from pagelint import CSHARP_TAGS, USING_RE, load_pages     # noqa: E402

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# --------------------------------------------------------------------------
# Shape -- the test half of the four wrapper rules
# --------------------------------------------------------------------------
# Applied in order; every block matches exactly one, and over all 985 blocks
# every block is wrapped (0 NOT COMPILABLE). The wrapper each shape implies is
# task 1.2's half; this is the test that chooses it.
#
# These read the block with its `using` lines set aside, because a block that is
# nothing but directives is a statement block with no statements, not a type.

# `namespace Foo;` or `namespace Foo { ... }` -- the block is already a
# compilation unit and gets no wrapper at all.
NAMESPACE_RE = re.compile(r'^\s*namespace\s')

# A type declaration, with or without attributes and modifiers.
DECL_RE = re.compile(
    r'^\s*(?:\[.*\]\s*)?'
    r'(?:public |internal |sealed |abstract |static |partial |record |)*'
    r'(?:class|record|interface|struct|enum)\s+[A-Za-z_]')

# A member: a line opening with an access or member modifier. Weaker than
# DECL_RE on purpose -- it catches `public override X Handle(...)` and
# `public string Name { get; set; }`, which need a holder class around them.
MEMBER_RE = re.compile(
    r'^\s*(?:\[.*\]\s*)?'
    r'(?:public|private|protected|internal|static|async|override|virtual)\s')

# A line that carries no code: blank, a comment, an attribute on its own line,
# a preprocessor directive, or a lone brace. `toplevel` asks what comes BEFORE
# a block's first declaration, and a comment before a class is not a statement.
NOT_CODE_RE = re.compile(r'^\s*(?://|/\*|\*|\*/|\[|#|\{|\}|$)')

SHAPES = ('namespaced', 'toplevel', 'types', 'members', 'statements')

# The verdict vocabulary, in full, and it is printed in full even where a
# verdict has no members. `requirements.md` AC1 asks that the four counts SUM
# TO THE CORPUS COUNT, and a summary that lists only the verdicts it happened
# to see cannot be added up: a run printing `61 BUILT, 924 FAILED` is
# indistinguishable from a tool that has no SKIPPED verdict at all. The two
# that read 0 today do so for DIFFERENT reasons, and the difference is the
# point: NOT_COMPILABLE is empty BY CONSTRUCTION, because `classify` cannot
# return *no* -- phase 2 recorded that as friction 59 rather than as coverage.
# SKIPPED is empty only because no page carries a marker yet; it acquired its
# opt-out in phase 3 and is whatever the corpus says from here on.
VERDICTS = ('BUILT', 'FAILED', 'SKIPPED', 'NOT_COMPILABLE')

# THE OPT-OUT. Q5, ruled in phase 3 task 3.1, and it is deliberately stricter
# than the two opt-outs already in this repository: it REQUIRES A REASON.
#
# `pagelint`'s `<!-- pagelint: allow-serviceactivator -->` and `symbolcheck`'s
# `<!-- symbolcheck: allow IMessageScheduler -->` each name what they silence
# and are reported on every run. Neither carries a reason, because for both the
# reason is recoverable: the page discusses the name, and the name is in the
# marker. Here it is not. A block that does not compile can fail to compile for
# a dozen reasons, and "somebody decided this one was fine" is not a claim a
# later reader can check against anything. So the reason is part of the syntax
# and a marker without one binds nothing and is reported as an error.
#
# It binds THE NEXT C# BLOCK on the page, never the page. That is
# `symbolcheck`'s argument transplanted: a page-wide skip written for one V9
# example would silently absorb a second block that arrived two years later,
# and nothing would ever say so.
SKIP_RE = re.compile(r'^<!--\s*blockcheck:\s*skip\s+(\S.*?)\s*-->$')
SKIP_ANY_RE = re.compile(r'^<!--\s*blockcheck:\s*skip\b')
SKIP_EXAMPLE = '<!-- blockcheck: skip V9 form, required by CLAUDE.md -->'

# Markers that bind nothing, accumulated by `enumerate_blocks` and reported by
# `mode_report`. Reset on every call, because a tool that accumulates across
# calls reports the second run's problems twice.
SKIP_PROBLEMS = []


class Block:
    """One C# fenced block, located and classified.

    `body` is the page's lines, verbatim and unmodified. `ident` names the page
    and the block's ordinal within it, which is how a verdict tells a reader
    where to look.
    """

    def __init__(self, rel, ordinal, start, end, body):
        self.rel = rel                  # repo-relative page path
        self.ordinal = ordinal          # 1-based, among that page's C# blocks
        self.start = start              # line of the opening fence
        self.end = end                  # line of the closing fence
        self.body = body                # list of str, verbatim
        stem = re.sub(r'\W', '_', os.path.splitext(os.path.basename(rel))[0])
        self.ident = f'{stem}_{ordinal}'
        self.usings, self.rest = hoist(body)
        self.shape = classify(self.rest)
        # Set by `enumerate_blocks` from the page's markers; None means the
        # gate judges this block. A reason here means it does not.
        self.skip_reason = None

    @property
    def text(self):
        """The block exactly as the page carries it, newline-terminated."""
        return ''.join(line + '\n' for line in self.body)

    @property
    def declares_omission(self):
        """Does the block say, in its own words, that something is elided?"""
        return any('// ...' in line for line in self.body)

    def __repr__(self):
        return f'<Block {self.ident} {self.shape} {self.rel}:{self.start}>'


def hoist(body):
    """Split a block into its `using` directives and everything else.

    Directives are lifted out because three of the four wrappers put the block
    inside a namespace or a class, where a `using` is illegal. Hoisting moves
    lines; it never edits one.
    """
    usings = [line for line in body if USING_RE.match(line)]
    rest = [line for line in body if not USING_RE.match(line)]
    return usings, rest


def is_toplevel_unit(rest):
    """Does the block put statements ABOVE a declaration, as a Program.cs does?

    C# allows top-level statements followed by type declarations, and 17 blocks
    in this corpus are written that way -- a couple of `services.Add…` calls,
    then the handler class they register. Such a block is already a compilation
    unit and needs no wrapper.

    Wrapped as `types` it cannot parse AT ALL: the leading statements land at
    namespace level and Roslyn reports CS0116. Phase 2's parse triage found
    those blocks by staging all 187 parse failures under all four rules and
    asking which parsed -- 7 parsed under a rule `classify()` had not chosen,
    and every one of them is this shape.

    ORDER IS THE WHOLE RULE, and it is what keeps this from over-reaching. A
    statement AFTER a declaration is CS8803 and cannot parse unwrapped either,
    so `UsingTheContextBag.md` block 16 -- a class, then a line of usage -- is
    NOT this shape and is a page defect. The two cases look alike in a diff and
    the compiler separates them.
    """
    first_decl = next((i for i, line in enumerate(rest)
                       if DECL_RE.match(line) or MEMBER_RE.match(line)), None)
    if first_decl is None:
        return False
    return any(not NOT_CODE_RE.match(line) for line in rest[:first_decl])


def classify(rest):
    """Which of the wrapper rules applies. `rest` is the hoisted body."""
    if any(NAMESPACE_RE.match(line) for line in rest):
        return 'namespaced'
    if is_toplevel_unit(rest):
        return 'toplevel'
    if any(DECL_RE.match(line) for line in rest):
        return 'types'
    if any(MEMBER_RE.match(line) for line in rest):
        return 'members'
    return 'statements'


# --------------------------------------------------------------------------
# Enumeration
# --------------------------------------------------------------------------
def scan_skips(lines, csharp_starts):
    """Bind each skip marker on a page to the block it precedes.

    Returns `(bindings, problems)` -- `{fence start lineno: reason}` and a list
    of `(lineno, kind, text)` for every marker that binds nothing.

    A marker binds THE NEXT C# FENCE THAT OPENS AFTER IT. That is the reading a
    human gives it, and it is the only rule that needs no second thought: the
    marker sits above the block it excuses, the way the ❌ label already does in
    `CLAUDE.md` § *Version markers on code*.

    THREE WAYS A MARKER BINDS NOTHING, AND ALL THREE ARE REPORTED. It carries no
    reason, so it is malformed and the block it appears to excuse is still
    judged. It follows the page's last C# block, so there is nothing after it to
    bind. Or a block already has one, and a second marker would leave a reader
    with two reasons and no way to tell which the tool used.
    """
    bindings, problems = {}, []
    starts = sorted(csharp_starts)
    for lineno, line in enumerate(lines, 1):
        stripped = line.strip()
        if not SKIP_ANY_RE.match(stripped):
            continue
        match = SKIP_RE.match(stripped)
        if not match:
            problems.append((lineno, 'no reason given', stripped))
            continue
        target = next((start for start in starts if start > lineno), None)
        if target is None:
            problems.append((lineno, 'no C# block follows it', stripped))
        elif target in bindings:
            problems.append((lineno, 'that block already has a marker', stripped))
        else:
            bindings[target] = match.group(1)
    return bindings, problems


def enumerate_blocks(pages=None):
    """Every C# block in the corpus, in page order then block order.

    The corpus is whatever `pagelint.load_pages()` lints -- contents/ plus
    README.md, which is the site root and carries 0 C# blocks today, so "the
    pages pagelint lints" and "the pages under contents/" are the same 985
    blocks. Defining it as the former means a C# block arriving on the site root
    is compiled rather than quietly exempt.
    """
    if pages is None:
        pages = load_pages()
    blocks = []
    del SKIP_PROBLEMS[:]
    for rel, page in sorted(pages.items()):
        csharp = [fence for fence in page.blocks
                  if (fence['info'] or '').strip().lower() in CSHARP_TAGS]
        bindings, problems = scan_skips(page.lines,
                                        [fence['start'] for fence in csharp])
        for lineno, kind, text in problems:
            SKIP_PROBLEMS.append((rel, lineno, kind, text))
        for ordinal, fence in enumerate(csharp, 1):
            block = Block(rel, ordinal, fence['start'], fence['end'],
                          [text for _, text in fence['body']])
            block.skip_reason = bindings.get(fence['start'])
            blocks.append(block)
    return blocks


def find_block(blocks, page, ordinal):
    """The nth C# block of a page, addressed the way `--list` prints it."""
    rel = os.path.relpath(os.path.abspath(page), ROOT)
    for block in blocks:
        if block.rel == rel and block.ordinal == ordinal:
            return block
    return None


# --------------------------------------------------------------------------
# Wrapping -- the emission half of the four rules
# --------------------------------------------------------------------------
# Each shape's wrapper, as the lines that open it. The closing lines are one
# `}` per opening line, which is why they are not written out: a wrapper whose
# two halves are maintained separately is a wrapper that will one day not
# balance.
#
# `__NS__` is the block's own namespace, `B_<ident>`. Every block gets one, so
# no two blocks can collide on a type name even though each is compiled alone
# -- and a verdict line names the page and the ordinal that produced it.
WRAPPERS = {
    'namespaced': [],
    'toplevel': [],
    'types': [
        'namespace __NS__',
        '{',
    ],
    'members': [
        'namespace __NS__',
        '{',
        'public class Holder',
        '{',
    ],
    'statements': [
        'namespace __NS__',
        '{',
        'public class Holder',
        '{',
        'public async System.Threading.Tasks.Task Run()',
        '{',
    ],
}


def wrap(block, scaffold=None):
    """The staged compilation unit for a block: usings, wrapper, body, close.

    THE BODY IS NEVER EDITED. Lines are moved -- `using` directives are hoisted
    above the wrapper, because three of the four wrappers put the block inside a
    namespace or a class where a directive is illegal -- and lines are added
    around the block. Nothing between the fences is rewritten, reindented or
    dropped, which is the claim `--verify-extraction` exists to test.

    A scaffolded page replaces the generic wrapper with its prelude and adds the
    `using` lines its unit declares. Both are declared in `scaffold/pages.tsv`
    and both are printed by `--list-scaffold`: a verdict that does not say what
    the block was given is not a verdict.

    Returns (text, n_usings, n_open, n_rest) so that a later pass can find the
    block's own lines in the staged file without guessing.
    """
    usings = list(block.usings)
    if scaffold is not None:
        usings += scaffold.usings
    if scaffold is not None and scaffold.prelude_lines:
        # The prelude's own brace convention, carried over from the harness:
        # `{@` is a brace this wrapper must close, a bare `{` is the prelude's
        # own and is already closed in the file.
        opens = [line.replace('__NAME__', f'Block_{block.ident}').replace('{@', '{')
                 for line in scaffold.prelude_lines]
        closes = ['}' for line in scaffold.prelude_lines if '{@' in line]
    else:
        opens = [line.replace('__NS__', f'B_{block.ident}')
                 for line in WRAPPERS[block.shape]]
        closes = ['}' for line in opens if line.strip() == '{']
    lines = usings + opens + block.rest + closes
    return ('\n'.join(lines) + '\n',
            len(usings), len(opens), len(block.rest))


# --------------------------------------------------------------------------
# The scaffold
# --------------------------------------------------------------------------
SCAFFOLD_DIR = os.path.join(ROOT, 'tools', 'blockcheck', 'scaffold')
SCAFFOLD_MAP = os.path.join(SCAFFOLD_DIR, 'pages.tsv')

# A unit declares the `using` lines it wants injected into every block it is
# given to. `using static PageContext;` is the whole mechanism by which a page's
# named-but-undefined values reach a block, and writing it in the unit keeps the
# declaration next to the thing it is about.
INJECT_RE = re.compile(r'^\s*//\s*blockcheck:\s*(using .+;)\s*$')


class Scaffold:
    """What one page's blocks are compiled with, beyond the page itself."""

    def __init__(self, unit=None, prelude=None):
        self.unit = unit                # file under scaffold/units/, or None
        self.prelude = prelude          # file under scaffold/preludes/, or None
        self.usings = []
        self.prelude_lines = []
        if unit:
            with open(unit, encoding='utf-8') as fh:
                for line in fh:
                    match = INJECT_RE.match(line)
                    if match:
                        self.usings.append(match.group(1))
        if prelude:
            with open(prelude, encoding='utf-8') as fh:
                self.prelude_lines = fh.read().split('\n')
            while self.prelude_lines and not self.prelude_lines[-1].strip():
                self.prelude_lines.pop()

    @property
    def name(self):
        parts = [os.path.basename(p) for p in (self.unit, self.prelude) if p]
        return '+'.join(parts) if parts else '-'


class ScaffoldError(Exception):
    """A scaffold map that names something that is not there: nothing checked."""


def load_scaffold():
    """The page -> scaffold map. Absent is legal and means no page is scaffolded.

    A row naming a file that does not exist is NOT legal, and is exit 2 rather
    than a silently unscaffolded page -- a page that quietly loses its scaffold
    fails every one of its blocks, believably.
    """
    scaffolds = {}
    if not os.path.exists(SCAFFOLD_MAP):
        return scaffolds
    with open(SCAFFOLD_MAP, encoding='utf-8') as fh:
        for lineno, line in enumerate(fh, 1):
            if not line.strip() or line.lstrip().startswith('#'):
                continue
            fields = line.rstrip('\n').split('\t')
            if len(fields) != 3:
                raise ScaffoldError(
                    f'{SCAFFOLD_MAP}:{lineno}: expected 3 tab-separated fields, '
                    f'got {len(fields)}')
            rel, unit, prelude = fields
            paths = []
            for kind, value, ext in (('units', unit, '.cs'),
                                     ('preludes', prelude, '.txt')):
                if value == '-':
                    paths.append(None)
                    continue
                path = os.path.join(SCAFFOLD_DIR, kind, value + ext)
                if not os.path.exists(path):
                    raise ScaffoldError(
                        f'{SCAFFOLD_MAP}:{lineno}: no such {kind[:-1]}: {path}')
                paths.append(path)
            scaffolds[rel] = Scaffold(*paths)
    return scaffolds


# --------------------------------------------------------------------------
# Modes
# --------------------------------------------------------------------------
def mode_list(blocks):
    for block in blocks:
        print(f'{block.rel}\t{block.ordinal}\t{block.shape}')
    counts = {shape: 0 for shape in SHAPES}
    for block in blocks:
        counts[block.shape] += 1
    pages = len({block.rel for block in blocks})
    print(f'{len(blocks)} C# blocks across {pages} pages: '
          + ', '.join(f'{counts[shape]} {shape}' for shape in SHAPES),
          file=sys.stderr)
    return 0


def mode_stage(blocks, args):
    """Write one compilation unit per block, plus the index that describes them.

    The index is what the Roslyn tool reads and what `--verify-extraction`
    reconstructs from: `id`, page, ordinal, shape, and the three line counts
    that say where in the staged file the page's own lines begin and end.
    """
    if len(args) != 1:
        print('usage: blockcheck.py --stage <dir>', file=sys.stderr)
        return 2
    out = args[0]
    os.makedirs(out, exist_ok=True)
    scaffolds = load_scaffold()

    # A unit is copied into the staged directory rather than referenced where it
    # lives, so that what was compiled can be read afterwards without knowing
    # how the run was configured.
    units = {}
    for scaffold in scaffolds.values():
        if scaffold.unit and scaffold.unit not in units:
            staged = 'unit_' + os.path.basename(scaffold.unit)
            with open(scaffold.unit, encoding='utf-8') as src:
                text = src.read()
            with open(os.path.join(out, staged), 'w', encoding='utf-8') as dst:
                dst.write(text)
            units[scaffold.unit] = staged

    index = []
    for block in blocks:
        scaffold = scaffolds.get(block.rel)
        text, n_usings, n_open, n_rest = wrap(block, scaffold)
        with open(os.path.join(out, block.ident + '.cs'), 'w',
                  encoding='utf-8') as fh:
            fh.write(text)
        index.append((block.ident, block.rel, block.ordinal, block.shape,
                      n_usings, n_open, n_rest,
                      units.get(scaffold.unit) if scaffold and scaffold.unit
                      else '-'))
    with open(os.path.join(out, 'index.tsv'), 'w', encoding='utf-8') as fh:
        for row in index:
            fh.write('\t'.join(str(field) for field in row) + '\n')
    scaffolded = sum(1 for row in index if row[7] != '-')
    print(f'{len(index)} blocks staged in {out}, {scaffolded} with a scaffold '
          f'unit, {len(scaffolds)} pages in the map', file=sys.stderr)
    return 0


def mode_verify_extraction(blocks, args):
    """Is what was compiled byte-identical to what the page says?

    The claim this tests is narrow and it is the one the whole gate rests on: a
    verdict about a block is only about the page if the text compiled came from
    the page unedited. Lines are MOVED -- `using` directives are hoisted above
    the wrapper -- and the check accounts for that by reconstructing the same
    split rather than by ignoring it. Nothing else may differ: not a space, not
    an indent, not a line ending.

    With no argument it stages a fresh copy and verifies that. Given a directory
    it verifies what is already there, which is how the red half is run.
    """
    if len(args) > 1:
        print('usage: blockcheck.py --verify-extraction [staged dir]',
              file=sys.stderr)
        return 2
    staged, temporary = (args[0], False) if args else (
        tempfile.mkdtemp(prefix='blockcheck-verify-'), True)
    try:
        if temporary and mode_stage(blocks, [staged]) != 0:
            return 2
        index_path = os.path.join(staged, 'index.tsv')
        if not os.path.exists(index_path):
            print(f'no index at {index_path}: nothing was verified',
                  file=sys.stderr)
            return 2
        index = {}
        with open(index_path, encoding='utf-8') as fh:
            for line in fh:
                if not line.strip():
                    continue
                fields = line.rstrip('\n').split('\t')
                index[fields[0]] = fields

        by_ident = {block.ident: block for block in blocks}
        if set(index) != set(by_ident):
            print(f'staged index holds {len(index)} blocks, the corpus holds '
                  f'{len(by_ident)}: nothing was verified', file=sys.stderr)
            return 2

        mismatches, hoisted = [], 0
        for ident, fields in sorted(index.items()):
            block = by_ident[ident]
            n_usings, n_open, n_rest = (int(fields[4]), int(fields[5]),
                                        int(fields[6]))
            with open(os.path.join(staged, ident + '.cs'), encoding='utf-8') as fh:
                lines = fh.read().split('\n')
            # The tool wrote usings, then the wrapper, then the body's
            # remainder. Injected scaffold usings sit at the end of the first
            # region, which is why the block's own share is taken from its head.
            recovered = (lines[:len(block.usings)]
                         + lines[n_usings + n_open:n_usings + n_open + n_rest])
            expected = block.usings + block.rest
            if recovered != expected:
                mismatches.append((ident, block.rel, block.ordinal))
            elif expected != block.body:
                hoisted += 1

        for ident, rel, ordinal in mismatches:
            print(f'{rel}\t{ordinal}\t{ident}\tNOT IDENTICAL')
        total = len(by_ident)
        print(f'{total - len(mismatches)} of {total} identical'
              + (f', {hoisted} with `using` directives hoisted' if hoisted else ''),
              file=sys.stderr)
        return 1 if mismatches else 0
    finally:
        if temporary:
            shutil.rmtree(staged, ignore_errors=True)


def mode_list_scaffold(args):
    """Every identifier supplied from outside a page, and where it came from.

    AC8. The identifiers are read with Roslyn rather than with a regex here,
    because a listing that under-reports is worse than no listing: it says a
    block was given less help than it was.
    """
    if args:
        print('usage: blockcheck.py --list-scaffold', file=sys.stderr)
        return 2
    try:
        scaffolds = load_scaffold()
    except ScaffoldError as exc:
        print(exc, file=sys.stderr)
        return 2

    units = sorted(glob.glob(os.path.join(SCAFFOLD_DIR, 'units', '*.cs')))
    preludes = sorted(glob.glob(os.path.join(SCAFFOLD_DIR, 'preludes', '*.txt')))
    if not units and not preludes:
        print(f'no scaffold files under {SCAFFOLD_DIR}', file=sys.stderr)
        return 2

    tool = os.path.join(ROOT, 'tools', 'blockcheck', 'bin', 'Release', 'net9.0',
                        'blockcheck.dll')
    if not os.path.exists(tool):
        print(f'{tool} is not built: nothing was listed\n'
              '  dotnet build tools/blockcheck/blockcheck.csproj -c Release',
              file=sys.stderr)
        return 2

    # A prelude is a fragment: it opens braces it does not close. Rendering it
    # the way a block would -- the same substitution, the same closing braces --
    # is what makes it parseable, and it is also a check that the fragment's
    # braces balance under the rule wrap() applies.
    work = tempfile.mkdtemp(prefix='blockcheck-scaffold-')
    sources = list(units)
    for prelude in preludes:
        name = os.path.splitext(os.path.basename(prelude))[0]
        with open(prelude, encoding='utf-8') as fh:
            lines = fh.read().split('\n')
        rendered = [line.replace('__NAME__', f'Prelude_{name}').replace('{@', '{')
                    for line in lines]
        rendered += ['}' for line in lines if '{@' in line]
        path = os.path.join(work, f'prelude_{name}.cs')
        with open(path, 'w', encoding='utf-8') as fh:
            fh.write('\n'.join(rendered) + '\n')
        sources.append(path)

    result = subprocess.run(
        ['dotnet', tool, '--identifiers'] + sources,
        capture_output=True, text=True)
    if result.returncode != 0:
        sys.stderr.write(result.stderr)
        return 2

    rows = [line for line in result.stdout.split('\n') if line]
    for row in rows:
        source, rest = row.split('\t', 1)
        print(f'{os.path.basename(source)}\t{rest}')
    for rel, scaffold in sorted(scaffolds.items()):
        for using in scaffold.usings:
            print(f'{rel}\tinjected\t{using}')

    print(f'{len(rows)} identifiers from {len(units)} unit(s) and '
          f'{len(preludes)} prelude(s); {len(scaffolds)} page(s) scaffolded',
          file=sys.stderr)
    return 0


def mode_show(blocks, args):
    if len(args) != 2 or not args[1].isdigit():
        print('usage: blockcheck.py --show <page> <ordinal>', file=sys.stderr)
        return 2
    block = find_block(blocks, args[0], int(args[1]))
    if block is None:
        print(f'no C# block {args[1]} in {args[0]}', file=sys.stderr)
        return 2
    print(f'{block.rel} block {block.ordinal}: lines {block.start}-{block.end}, '
          f'{block.shape}', file=sys.stderr)
    sys.stdout.write(block.text)
    return 0


# --------------------------------------------------------------------------
# The gate
# --------------------------------------------------------------------------
# EVERY CONDITION THAT EXITS 2, NAMED IN ONE PLACE.
#
# 2 is "nothing was checked", and it is the code this tool is most likely to
# need and least likely to emit. Each of these states produces a run that looks
# like a measurement and is not -- with no reference set every block fails, a
# believable red in the direction that confirms the thesis, and a gate with no
# references has not checked the corpus at all.
#
#   1. tools/pagelint.py cannot be imported     -- no corpus walk, no enumeration
#   2. the enumeration is empty                 -- 0 blocks passes every corpus
#   3. tools/blockcheck/scaffold/ is absent     -- blocks lose declared context
#   4. scaffold/pages.tsv names a file that is not there
#   5. a scaffold does not parse                -- it would suppress binding
#   6. the Roslyn tool is not built
#   7. refs.txt is absent                       -- the reference project is unrestored
#   8. refs.txt names an assembly that is not there
#   9. the staged index is missing or malformed
#  10. refs.txt was written by a DIFFERENT refs.csproj -- a stale pin
#  11. baseline.tsv is absent or malformed     -- nothing to hold the corpus to
#
# 10 is phase 2's, and it is the one that had already happened. Adding Darker's
# packages for Q3 left an XML error in refs.csproj, so the project failed to LOAD
# and no target ran; refs.txt survived from the previous build, and the corpus
# run reported the same 61 built as before while the author read it as the new
# pin. Nothing was missing, which is what made it believable. The stamp is the
# first line of refs.txt and this is where it is checked.
#
# 11 is phase 3's. A missing baseline is not an empty one: an empty file would
# be a baseline with no rows, which holds no block to anything, and the run
# would say `0 findings` about a corpus nobody had admitted. So the file must
# exist, every row must have four fields, and no block may be listed twice.
#
# 3 to 11 are enforced across the two halves: 3, 4, 6, 7, 10 and 11 here, 5, 8
# and 9 in tools/blockcheck/Program.cs, which returns 2 for each and is
# propagated.
TOOL_DLL = os.path.join(ROOT, 'tools', 'blockcheck', 'bin', 'Release', 'net9.0',
                        'blockcheck.dll')
REFS_LIST = os.path.join(ROOT, 'tools', 'blockcheck', 'refs', 'bin', 'Release',
                         'net9.0', 'refs.txt')
REFS_PROJECT = os.path.join(ROOT, 'tools', 'blockcheck', 'refs', 'refs.csproj')
BUILD_HINT = ('  dotnet build tools/blockcheck/refs/refs.csproj -c Release\n'
              '  dotnet build tools/blockcheck/blockcheck.csproj -c Release')
# One path, and no flag to change it: a gate that can be pointed at another
# list can be silenced (design Constraint 6).
BASELINE = os.path.join(ROOT, 'tools', 'blockcheck', 'baseline.tsv')
BASELINE_REL = os.path.relpath(BASELINE, ROOT)


class BaselineError(Exception):
    """A baseline that cannot be read as one: nothing was checked."""


def load_baseline():
    """`{(page, ordinal): (scaffold, ref, lineno)}` from baseline.tsv.

    Absent, a row without four fields, a non-numeric ordinal, or a block listed
    twice is BaselineError -- exit 2, because each makes the file mean
    something other than what it says. A duplicate is the subtle one: two rows
    for one block would let one of them be deleted without the gate noticing.
    """
    if not os.path.exists(BASELINE):
        raise BaselineError(f'no baseline at {BASELINE}')
    rows = {}
    with open(BASELINE, encoding='utf-8') as fh:
        for lineno, line in enumerate(fh, 1):
            if not line.strip() or line.startswith('#'):
                continue
            fields = line.rstrip('\n').split('\t')
            if len(fields) != 4 or not fields[1].isdigit():
                raise BaselineError(
                    f'{BASELINE}:{lineno}: expected page, ordinal, scaffold, '
                    f'ref -- got {line.rstrip()!r}')
            key = (fields[0], int(fields[1]))
            if key in rows:
                raise BaselineError(
                    f'{BASELINE}:{lineno}: {key[0]} block {key[1]} is already '
                    f'listed at line {rows[key][2]}')
            rows[key] = (fields[2], fields[3], lineno)
    return rows


def stale_pin():
    """Was `refs.txt` written by the `refs.csproj` on disk right now?

    Returns a reason, or None when the stamp matches. The stamp is refs.txt's
    first line, written by refs.csproj itself; a list with no stamp is stale by
    definition, because the only thing that writes one is a build of the current
    project.
    """
    with open(REFS_LIST, encoding='utf-8') as handle:
        first = handle.readline().strip()
    want = hashlib.sha256(
        open(REFS_PROJECT, 'rb').read()).hexdigest().lower()
    prefix = '# refs.csproj SHA256 '
    if not first.startswith(prefix):
        return (f'{REFS_LIST} carries no pin stamp on its first line, so it '
                'cannot be told from a stale one')
    got = first[len(prefix):].strip().lower()
    if got != want:
        return (f'{REFS_LIST} was written by a different refs.csproj\n'
                f'  refs.txt   {got}\n  refs.csproj {want}')
    return None


def mode_report(blocks, args):
    """Compile every block and report a verdict for each. The whole run.

    Rows go to stdout, or to the file named after `--report`, and the verdict is
    the FIRST field: the criterion that reads this counts verdicts with awk, and
    awk's default split is whitespace.

    `NOT_COMPILABLE` carries an underscore for that reason. `requirements.md`
    AC1 spells it with a space, which would split into two fields and count as
    `NOT` -- invisible today, because it is 0 of 985, and a trap the first time
    it is not.
    """
    if len(args) > 1:
        print('usage: blockcheck.py --report [file]', file=sys.stderr)
        return 2
    if not os.path.isdir(SCAFFOLD_DIR):
        print(f'no scaffold directory at {SCAFFOLD_DIR}: nothing was checked',
              file=sys.stderr)
        return 2
    if not os.path.exists(TOOL_DLL):
        print(f'{TOOL_DLL} is not built: nothing was checked\n' + BUILD_HINT,
              file=sys.stderr)
        return 2
    if not os.path.exists(REFS_LIST):
        print(f'no reference list at {REFS_LIST}: the reference project has not '
              'been restored and built, so nothing was checked\n' + BUILD_HINT,
              file=sys.stderr)
        return 2
    stale = stale_pin()
    if stale is not None:
        print(f'{stale}\nnothing was checked\n' + BUILD_HINT, file=sys.stderr)
        return 2
    try:
        baseline = load_baseline()
        scaffolds = load_scaffold()
    except (BaselineError, ScaffoldError) as exc:
        print(f'{exc}: nothing was checked', file=sys.stderr)
        return 2

    staged = tempfile.mkdtemp(prefix='blockcheck-')
    try:
        if mode_stage(blocks, [staged]) != 0:
            return 2
        result = subprocess.run(
            ['dotnet', TOOL_DLL, staged, REFS_LIST],
            capture_output=True, text=True)
        sys.stderr.write(result.stderr)
        if result.returncode != 0:
            return 2

        verdicts = {}
        for line in result.stdout.split('\n'):
            if not line:
                continue
            ident, verdict, count, codes = line.split('\t')
            verdicts[ident] = (verdict, count, codes)

        rows, counts = [], {verdict: 0 for verdict in VERDICTS}
        judged = {}
        for block in blocks:
            if block.skip_reason:
                # A skipped block IS still staged and compiled -- the cost is
                # 7ms and `--verify-extraction` needs the full 985 -- but its
                # verdict is discarded unread. SKIPPED means the gate did not
                # judge this block, so reporting an error count beside it would
                # be reporting a judgement it just declined to make.
                verdict, count, codes = 'SKIPPED', '0', ''
            else:
                verdict, count, codes = verdicts.get(
                    block.ident, ('NOT_COMPILABLE', '0', ''))
            counts[verdict] = counts.get(verdict, 0) + 1
            judged[(block.rel, block.ordinal)] = (block, verdict, codes)
            rows.append(f'{verdict}\t{block.rel}\t{block.ordinal}\t'
                        f'{block.ident}\t{count}\t{codes}')

        out = open(args[0], 'w', encoding='utf-8') if args else sys.stdout
        try:
            for row in rows:
                print(row, file=out)
        finally:
            if out is not sys.stdout:
                out.close()
    finally:
        shutil.rmtree(staged, ignore_errors=True)

    # Scope first, verdict second. `0 findings` out of 0 blocks and `0 findings`
    # out of 985 are different claims, and only one of them is worth having.
    skipped = counts.get('SKIPPED', 0)
    print(f'{len(rows)} blocks: '
          + ', '.join(f'{counts[v]} {v}' for v in VERDICTS),
          file=sys.stderr)

    # NEVER SILENT. Every skip is printed with the reason its author wrote,
    # on a green run too, because `0 findings` and `0 findings, 8 skipped` are
    # different claims about the corpus and only one of them is checkable.
    # This is `symbolcheck`'s rule, and ruling 4.
    excused = [block for block in blocks if block.skip_reason]
    if excused:
        print(f'\n----- skipped by opt-out ({len(excused)}) -----',
              file=sys.stderr)
        for block in excused:
            print(f'{block.rel}:{block.start}  block {block.ordinal} — '
                  f'{block.skip_reason}', file=sys.stderr)

    # A MARKER THAT BINDS NOTHING IS REPORTED, and the two kinds are not the
    # same defect. One carrying no reason is MALFORMED: it reads as an opt-out,
    # grants none, and the block it appears to excuse is still judged -- so it
    # is an error, and it is the reason the reason is part of the syntax. One
    # that binds no block is DEAD WEIGHT, and that is a warning for
    # `symbolcheck`'s reason: debt that fails the build gets deleted rather
    # than understood.
    malformed = [row for row in SKIP_PROBLEMS if row[2] == 'no reason given']
    dead = [row for row in SKIP_PROBLEMS if row[2] != 'no reason given']
    if dead:
        print(f'\n----- stale opt-out (warning: {len(dead)}) -----',
              file=sys.stderr)
        for rel, lineno, kind, text in dead:
            print(f'{rel}:{lineno}  {kind} — {text}', file=sys.stderr)
    if malformed:
        print(f'\n----- malformed opt-out ({len(malformed)}) -----',
              file=sys.stderr)
        for rel, lineno, kind, text in malformed:
            print(f'{rel}:{lineno}  {kind} — {text}', file=sys.stderr)
        print(f'    A skip states why, on its own line: {SKIP_EXAMPLE}',
              file=sys.stderr)

    # THE RATCHET. The baseline must equal the BUILT set, and each of the four
    # ways it can disagree is printed under its own heading, because they are
    # fixed in different places: a regression on the page, a vanished row and
    # an unlisted block in baseline.tsv, a changed scaffold in either.
    #
    # A baselined block that is SKIPPED counts as no longer building. A skip
    # marker added above a baselined block would otherwise be a way to take a
    # block out of the gate without touching the gate's own file.
    regressed, vanished, unlisted, rescaffolded = [], [], [], []
    for key, (want_scaffold, ref, lineno) in sorted(baseline.items()):
        if key not in judged:
            vanished.append(f'{BASELINE_REL}:{lineno}  {key[0]} block {key[1]} '
                            f'-- admitted at {ref}, and the page has no such block')
            continue
        block, verdict, codes = judged[key]
        if verdict != 'BUILT':
            why = {'FAILED': f'FAILED {codes}',
                   'SKIPPED': 'SKIPPED by an opt-out, which cannot excuse a '
                              'baselined block -- remove the marker or the row'
                   }.get(verdict, verdict)
            regressed.append(f'{block.rel}:{block.start}  block {block.ordinal} '
                             f'-- {why}, admitted BUILT at {ref}')
            continue
        have = scaffolds[block.rel].name if block.rel in scaffolds else '-'
        if have != want_scaffold:
            rescaffolded.append(f'{block.rel}:{block.start}  block '
                                f'{block.ordinal} -- admitted with '
                                f'{want_scaffold}, now compiled with {have}')
    for key, (block, verdict, codes) in sorted(judged.items()):
        if verdict == 'BUILT' and key not in baseline:
            have = scaffolds[block.rel].name if block.rel in scaffolds else '-'
            unlisted.append(f'{block.rel}:{block.start}  block {block.ordinal} '
                            f'-- BUILT, not in the baseline. Add:  '
                            f'{block.rel}\t{block.ordinal}\t{have}\t<ref>')

    for title, items in (('stopped building', regressed),
                         ('baselined block no longer exists', vanished),
                         ('builds and is not baselined', unlisted),
                         ('scaffold changed since admission', rescaffolded)):
        if items:
            print(f'\n----- {title} ({len(items)}) -----', file=sys.stderr)
            for item in items:
                print(item, file=sys.stderr)

    # A malformed marker is a finding REGARDLESS of the baseline, because it
    # is not a claim about whether a block compiles -- it is a claim about the
    # corpus that is wrong on its own terms.
    findings = (len(malformed) + len(regressed) + len(vanished)
                + len(unlisted) + len(rescaffolded))
    print(f'baseline: {len(baseline)} blocks required to build', file=sys.stderr)
    print(f'{findings} findings' + (f', {skipped} skipped' if skipped else ''),
          file=sys.stderr)
    return 1 if findings else 0


def main(argv):
    mode = argv[0] if argv else None
    if mode == '--list-scaffold':
        return mode_list_scaffold(argv[1:])
    if mode not in ('--list', '--show', '--stage', '--report',
                    '--verify-extraction'):
        print(__doc__.split('Usage:')[1].split('Exit code')[0].strip(),
              file=sys.stderr)
        print('\nunknown mode: nothing was checked', file=sys.stderr)
        return 2

    blocks = enumerate_blocks()
    if not blocks:
        # Exit 2, never 0. Zero blocks is a broken corpus walk, and a gate that
        # reports a clean nothing is the failure mode this programme has met
        # twelve times.
        print('no C# blocks found: the corpus walk is broken', file=sys.stderr)
        return 2

    try:
        if mode == '--list':
            return mode_list(blocks)
        if mode == '--stage':
            return mode_stage(blocks, argv[1:])
        if mode == '--report':
            return mode_report(blocks, argv[1:])
        if mode == '--verify-extraction':
            return mode_verify_extraction(blocks, argv[1:])
    except ScaffoldError as exc:
        print(exc, file=sys.stderr)
        return 2
    return mode_show(blocks, argv[1:])


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
