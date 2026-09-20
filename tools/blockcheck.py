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

`--report` MEASURES; it does not yet gate. There is no `baseline.tsv` until
phase 3, so no block is required to compile and a failing one is not a finding.
The run says that in those words rather than printing a clean-looking `0
findings` over 924 failures, and the no-argument form is not the gate either --
it exits 2, because a gate that does not exist must not look green.

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
# indistinguishable from a tool that has no SKIPPED verdict at all. Two of the
# four are 0 today for reasons a reader should be told rather than left to infer
# -- SKIPPED has no opt-out to carry it until phase 3, and NOT_COMPILABLE is
# empty by construction because four wrapper rules cover 985 of 985.
VERDICTS = ('BUILT', 'FAILED', 'SKIPPED', 'NOT_COMPILABLE')


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
    for rel, page in sorted(pages.items()):
        ordinal = 0
        for fence in page.blocks:
            if (fence['info'] or '').strip().lower() not in CSHARP_TAGS:
                continue
            ordinal += 1
            blocks.append(Block(rel, ordinal, fence['start'], fence['end'],
                                [text for _, text in fence['body']]))
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
#
# 10 is phase 2's, and it is the one that had already happened. Adding Darker's
# packages for Q3 left an XML error in refs.csproj, so the project failed to LOAD
# and no target ran; refs.txt survived from the previous build, and the corpus
# run reported the same 61 built as before while the author read it as the new
# pin. Nothing was missing, which is what made it believable. The stamp is the
# first line of refs.txt and this is where it is checked.
#
# 3 to 10 are enforced across the two halves: 3, 4, 6, 7 and 10 here, 5, 8 and 9 in
# tools/blockcheck/Program.cs, which returns 2 for each and is propagated.
TOOL_DLL = os.path.join(ROOT, 'tools', 'blockcheck', 'bin', 'Release', 'net9.0',
                        'blockcheck.dll')
REFS_LIST = os.path.join(ROOT, 'tools', 'blockcheck', 'refs', 'bin', 'Release',
                         'net9.0', 'refs.txt')
REFS_PROJECT = os.path.join(ROOT, 'tools', 'blockcheck', 'refs', 'refs.csproj')
BUILD_HINT = ('  dotnet build tools/blockcheck/refs/refs.csproj -c Release\n'
              '  dotnet build tools/blockcheck/blockcheck.csproj -c Release')


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
        for block in blocks:
            verdict, count, codes = verdicts.get(
                block.ident, ('NOT_COMPILABLE', '0', ''))
            counts[verdict] = counts.get(verdict, 0) + 1
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

    # NO BASELINE EXISTS YET, so nothing is REQUIRED to compile and a failing
    # block is not a finding. Phase 3 adds baseline.tsv and the ratchet; until
    # then this tool measures and does not gate, and it says so rather than
    # printing a clean-looking zero.
    findings = 0
    print('no baseline yet: this is a measurement, not a gate', file=sys.stderr)
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
