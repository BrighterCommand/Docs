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
    python3 tools/blockcheck.py --classify [page...]  # what stands between FAILED and BUILT
    python3 tools/blockcheck.py --list-skips [page...]  # page<TAB>ordinal<TAB>reason
    python3 tools/blockcheck.py --explain <id>...  # every error of those blocks, in full

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
        # `args` because a statements block is most often a Program.cs body, and
        # C# gives a top-level program `string[] args` without declaring it
        # (spec 017 P1-3). Simulated over the corpus before it was added: it
        # moved one block FAILED -> BUILT and none the other way.
        'public async System.Threading.Tasks.Task Run(string[] args)',
        '{',
    ],
}


# THE HANDLER WRAPPER (spec 018, design D2). A `members` block that overrides a
# handler method is a handler shown without its class, and in a base-less
# Holder its `override` has nothing to override and its `Context` belongs to
# nobody. So the Holder is given the base the method implies, read from the
# signature, written fully qualified so that the wrapper adds no `using`: a
# block that needs `using Paramore.Brighter;` for its own names still fails
# without it, and pagelint rule 6's debt is untouched.
#
#   Handle(TRequest …)         Paramore.Brighter.RequestHandler<TRequest>
#   HandleAsync(TRequest …)    Paramore.Brighter.RequestHandlerAsync<TRequest>
#   Execute(TQuery …)          Paramore.Darker.QueryHandler<TQuery, TResult>
#   ExecuteAsync(TQuery …)     Paramore.Darker.QueryHandlerAsync<TQuery, TResult>,
#                              TResult being T of the returned Task<T>
#   Fallback(T …)              Brighter's sync base when it returns T, Darker's
#                              QueryHandler<T, TResult> when it returns anything else
#   FallbackAsync(T …)         likewise, reading T of the returned Task<T>
#
# The two Fallback rows are D9 (ruled 2026-10-09, task 2.5): five blocks show a
# handler by its Fallback alone. Both products have one, and the return type is
# what tells them apart -- Brighter's returns the request it was given,
# Darker's the query's result.
#
# THE HOLDER IS ABSTRACT when it has a base. A block that shows one method of a
# handler shows PART of a class, and a concrete Holder would claim the members
# the page does not show: a Darker handler shown by its `Fallback` alone fails
# CS0534 for want of the `Execute` its base declares abstract. Abstract, the
# Holder claims only what the block shows. A signature that overrides nothing
# still fails CS0115, so a wrong override is not hidden by this.
#
# IT DOES NOT GUESS. The block keeps the base-less Holder when no signature can
# be read -- `ExecuteAsync(...)` names no query type -- or when its overrides
# imply two different bases, since a class has one. `handler_base` says which,
# and why, so that a block left unwrapped can be named rather than counted.
HANDLER_METHODS = ('HandleAsync', 'Handle', 'ExecuteAsync', 'Execute',
                   'FallbackAsync', 'Fallback')
OVERRIDE_RE = re.compile(
    r'\boverride\s+(?P<ret>[^;{}()]+?)\s+(?P<method>' + '|'.join(HANDLER_METHODS)
    + r')\s*\(\s*(?P<arg>[^()]*?)\s*[,)=]')
PARAM_RE = re.compile(r'^(?P<type>[\w.]+(?:<[\w.<>, ?]*>)?\??)\s+@?\w+$')
TASK_OF_RE = re.compile(r'^(?:System\.Threading\.Tasks\.)?Task<(?P<inner>.+)>$')


def handler_base(block):
    """`(base, None)` for a handler block, `(None, why)` otherwise.

    A block that is not `members`, or overrides none of the four methods, is
    `(None, None)`: there is nothing to wrap and nothing to explain. Lines are
    joined before reading, so a signature broken across lines reads as one.
    """
    if block.shape != 'members':
        return None, None
    text = ' '.join(line.strip() for line in block.rest)
    found = list(OVERRIDE_RE.finditer(text))
    if not found:
        return None, None
    bases = set()
    for match in found:
        ret = re.sub(r'^(?:async\s+|new\s+|sealed\s+)+', '', match.group('ret').strip())
        method, param = match.group('method'), PARAM_RE.match(match.group('arg'))
        if not param:
            return None, (f'{method}({match.group("arg")}) names no '
                          'request type to read')
        arg = param.group('type')
        if method == 'Handle':
            bases.add(f'Paramore.Brighter.RequestHandler<{arg}>')
        elif method == 'HandleAsync':
            bases.add(f'Paramore.Brighter.RequestHandlerAsync<{arg}>')
        elif method == 'Execute':
            bases.add(f'Paramore.Darker.QueryHandler<{arg}, {ret}>')
        elif method == 'Fallback':
            bases.add(f'Paramore.Brighter.RequestHandler<{arg}>' if ret == arg
                      else f'Paramore.Darker.QueryHandler<{arg}, {ret}>')
        else:
            inner = TASK_OF_RE.match(ret)
            if not inner:
                return None, f'{method} returns {ret}, not Task<T>'
            result = inner.group('inner')
            if method == 'FallbackAsync' and result == arg:
                bases.add(f'Paramore.Brighter.RequestHandlerAsync<{arg}>')
            else:
                bases.add(f'Paramore.Darker.QueryHandlerAsync<{arg}, {result}>')
    if len(bases) > 1:
        return None, 'its overrides imply two bases: ' + ' and '.join(sorted(bases))
    return bases.pop(), None


# A `statements` BLOCK THAT NAMES `Program` IS A Program.cs (spec 017 friction
# #75, spec 018 task 2.8). `typeof(Program).Assembly` is how a host's
# registration finds its handlers, and C# synthesises `Program` for a file of
# top-level statements -- so such a block is staged as exactly that file, with
# no wrapper, and compiled as an executable (Program.cs reads the tree and
# sees the global statements). Inside the generic Holder the same text fails
# CS0246, because no `Program` exists. It is read with comments and string
# literals removed, so `// Program.cs` above a block does not make it one.
PROGRAM_RE = re.compile(r'\bProgram\b')
CODE_NOISE_RE = re.compile(r'//.*$|"(?:\\.|[^"\\])*"')


def names_program(block):
    """Does a `statements` block name `Program` in its code?"""
    return (block.shape == 'statements'
            and any(PROGRAM_RE.search(CODE_NOISE_RE.sub('', line))
                    for line in block.rest))


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
    elif names_program(block):
        opens, closes = [], []
    else:
        opens = [line.replace('__NS__', f'B_{block.ident}')
                 for line in WRAPPERS[block.shape]]
        base, _ = handler_base(block)
        if base:
            opens = [f'public abstract class Holder : {base}'
                     if line == 'public class Holder'
                     else line for line in opens]
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

    def __init__(self, unit=None, prelude=None, pin='-'):
        self.unit = unit                # file under scaffold/units/, or None
        self.prelude = prelude          # file under scaffold/preludes/, or None
        self.pin = pin                  # 'v4' for the V4 pin, '-' for the main one
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
        """What the block was compiled with, as baseline.tsv records it.

        The pin is part of it: a block admitted against one package set and
        later compiled against another is a different claim, and the gate's
        `scaffold changed since admission` heading is how it says so.
        """
        parts = [os.path.basename(p) for p in (self.unit, self.prelude) if p]
        if self.pin == 'v4':
            parts.append('v4')
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
            if len(fields) != 4:
                raise ScaffoldError(
                    f'{SCAFFOLD_MAP}:{lineno}: expected 4 tab-separated fields, '
                    f'got {len(fields)}')
            rel, unit, prelude, pin = fields
            if pin not in ('-', 'v4'):
                raise ScaffoldError(
                    f'{SCAFFOLD_MAP}:{lineno}: pin is `v4` or `-`, not {pin!r}')
            if unit == prelude == pin == '-':
                raise ScaffoldError(
                    f'{SCAFFOLD_MAP}:{lineno}: {rel} is given nothing, so its '
                    'row says nothing')
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
            scaffolds[rel] = Scaffold(*paths, pin=pin)
    return scaffolds


def pin_of(rel, scaffolds):
    """The pin a page's blocks compile against: 'v4' or 'main'."""
    scaffold = scaffolds.get(rel)
    return 'v4' if scaffold is not None and scaffold.pin == 'v4' else 'main'


# --------------------------------------------------------------------------
# The unit rule
# --------------------------------------------------------------------------
# A UNIT MAY SUPPLY ONLY WHAT ITS PAGES' BUILDING BLOCKS USE. Spec 016 wrote
# that in pages.tsv's comment and nothing checked it, so PageContext.cs went
# on declaring nine types and seventeen members that no block on its page
# names. Spec 017 made stubs legal for types a page names and never shows,
# which widens what a unit may hold -- so the rule is enforced here, on every
# `--report`, and a violation is a finding. For a unit mapped to pages P:
#
#   1. every type it declares is named by a BUILT block on P
#   2. no block on P declares a type of that name -- the stub would leak the
#      page's own type into a block that is meant to stand alone
#   3. every member it declares (field, property, method) is named by a BUILT
#      block on P
#   4. it declares no `global using`: a unit's plain `using`s are file-scoped
#      and cannot reach a block, and a global one would supply a namespace the
#      page never gave its reader
#
# "Named" is a token match over the block's text. The holder class a unit's
# own `// blockcheck: using static X;` line brings into scope is exempt from 1
# -- no block names it, by design -- and its members are not exempt from 3.
UNIT_MEMBER_KINDS = ('field', 'property', 'method')
UNIT_IGNORED_KINDS = ('parameter', 'local')
HOLDER_RE = re.compile(r'^using static ([A-Za-z_][\w.]*);$')
GLOBAL_USING_RE = re.compile(r'^\s*global\s+using\b', re.M)
TOKEN_RE = re.compile(r'[A-Za-z_]\w*')


def unit_rule_violations(blocks, verdicts, scaffolds):
    """`(violations, units checked)`, or None when the units cannot be read.

    `verdicts` maps (page, ordinal) to the block's verdict.
    """
    pages_of = {}
    for rel, scaffold in scaffolds.items():
        if scaffold.unit:
            pages_of.setdefault(scaffold.unit, []).append(rel)
    if not pages_of:
        return [], 0
    run = subprocess.run(['dotnet', TOOL_DLL, '--identifiers', *sorted(pages_of)],
                         capture_output=True, text=True)
    if run.returncode != 0:
        sys.stderr.write(run.stderr)
        return None
    declared = {}
    for line in run.stdout.split('\n'):
        if line:
            path, kind, name = line.split('\t')
            declared.setdefault(path, []).append((kind, name))

    violations = []
    for unit, pages in sorted(pages_of.items()):
        label = os.path.basename(unit)
        with open(unit, encoding='utf-8') as fh:
            text = fh.read()
        holders = {match.group(1).split('.')[-1]
                   for scaffold in scaffolds.values() if scaffold.unit == unit
                   for using in scaffold.usings
                   for match in [HOLDER_RE.match(using)] if match}
        on_pages = [block for block in blocks if block.rel in pages]
        named = {token for block in on_pages
                 if verdicts.get((block.rel, block.ordinal)) == 'BUILT'
                 for token in TOKEN_RE.findall(block.text)}
        page_types = {}
        for block in on_pages:
            for name in TYPE_DECL_RE.findall(block.text):
                page_types.setdefault(name, f'{block.rel} block {block.ordinal}')
        where = ', '.join(sorted(pages))

        if GLOBAL_USING_RE.search(text):
            violations.append(f'{label}: declares a global using, which would '
                              'supply a namespace to every block it reaches')
        for kind, name in declared.get(unit, []):
            if kind in UNIT_IGNORED_KINDS:
                continue
            if kind in UNIT_MEMBER_KINDS:
                if name not in named:
                    violations.append(f'{label}: {kind} {name} is named by no '
                                      f'BUILT block on {where}')
                continue
            if name in page_types:
                violations.append(f'{label}: {kind} {name} is declared by '
                                  f'{page_types[name]}, and a stub must not '
                                  'supply it')
            if name not in holders and name not in named:
                violations.append(f'{label}: {kind} {name} is named by no BUILT '
                                  f'block on {where}')
    return violations, len(pages_of)


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


def mode_stage(blocks, args, quiet=False):
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
    if not quiet:
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
        if scaffold.pin == 'v4':
            print(f'{rel}\tpin\tv4')

    pinned = sum(1 for scaffold in scaffolds.values() if scaffold.pin == 'v4')
    given = sum(1 for scaffold in scaffolds.values()
                if scaffold.unit or scaffold.prelude)
    print(f'{len(rows)} identifiers from {len(units)} unit(s) and '
          f'{len(preludes)} prelude(s); {given} page(s) '
          f'scaffolded by a unit or prelude, {pinned} on the v4 pin',
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
# The prefix the Roslyn tool writes before each error it reports against a
# scaffold unit rather than a block. `--report` reads these as findings.
SCAFFOLD_ERROR = 'scaffold error: '
TOOL_DLL = os.path.join(ROOT, 'tools', 'blockcheck', 'bin', 'Release', 'net9.0',
                        'blockcheck.dll')
REFS_LIST = os.path.join(ROOT, 'tools', 'blockcheck', 'refs', 'bin', 'Release',
                         'net9.0', 'refs.txt')
REFS_PROJECT = os.path.join(ROOT, 'tools', 'blockcheck', 'refs', 'refs.csproj')
# THE V4 PIN (spec 018, design § The V4 Pin). The AWS V4 packages need
# AWSSDK.SQS 4.x and the V3 ones cap it below 4.0, so they cannot share a
# project with the main pin. A page marked `v4` in scaffold/pages.tsv is
# compiled against this list instead; every other page against REFS_LIST.
REFS_V4_LIST = os.path.join(ROOT, 'tools', 'blockcheck', 'refs-v4', 'bin',
                            'Release', 'net9.0', 'refs.txt')
REFS_V4_PROJECT = os.path.join(ROOT, 'tools', 'blockcheck', 'refs-v4',
                               'refs-v4.csproj')
PINS = {'main': (REFS_PROJECT, REFS_LIST), 'v4': (REFS_V4_PROJECT, REFS_V4_LIST)}
# The only packages in which the two pins may differ, each by its `.V4` suffix.
V4_SWAPPED = ('Paramore.Brighter.DynamoDb', 'Paramore.Brighter.Inbox.DynamoDB',
              'Paramore.Brighter.Locking.DynamoDB',
              'Paramore.Brighter.MessageScheduler.AWS',
              'Paramore.Brighter.MessagingGateway.AWSSQS',
              'Paramore.Brighter.Outbox.DynamoDB',
              'Paramore.Brighter.Transformers.AWS')
PACKAGE_RE = re.compile(
    r'<PackageReference\s+Include="([^"]+)"\s+Version="([^"]+)"\s*/>')
# A C# block that imports a V4 namespace: the page's evidence that it is a V4
# page, and what the pairing rule reads.
V4_USING_RE = re.compile(r'^\s*using\s+(?:static\s+)?[\w.]+\.V4(?:\.[\w.]+)?\s*;')
BUILD_HINT = ('  dotnet build tools/blockcheck/refs/refs.csproj -c Release\n'
              '  dotnet build tools/blockcheck/refs-v4/refs-v4.csproj -c Release\n'
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


def stale_pin(project=REFS_PROJECT, listing=REFS_LIST):
    """Was this `refs.txt` written by the project on disk right now?

    Returns a reason, or None when the stamp matches. The stamp is refs.txt's
    first line, written by the project itself and naming it; a list with no
    stamp is stale by definition, because the only thing that writes one is a
    build of the current project.
    """
    with open(listing, encoding='utf-8') as handle:
        first = handle.readline().strip()
    want = hashlib.sha256(open(project, 'rb').read()).hexdigest().lower()
    label = os.path.basename(project)
    prefix = f'# {label} SHA256 '
    if not first.startswith(prefix):
        return (f'{listing} carries no {label} stamp on its first line, so it '
                'cannot be told from a stale one')
    got = first[len(prefix):].strip().lower()
    if got != want:
        return (f'{listing} was written by a different {label}\n'
                f'  refs.txt     {got}\n  {label} {want}')
    return None


def pin_drift():
    """Does refs-v4.csproj differ from refs.csproj by anything but the swaps?

    Returns a reason, or None. The V4 pin is a copy of the main one, and a copy
    drifts: a version bump made in one file and not the other would leave the
    V4 pages compiled against last release's packages, quietly. So the two
    package lists are compared on every run, and only V4_SWAPPED may differ,
    each by its `.V4` suffix, at the same version.
    """
    def packages(path):
        with open(path, encoding='utf-8') as fh:
            return set(PACKAGE_RE.findall(fh.read()))
    main, v4 = packages(REFS_PROJECT), packages(REFS_V4_PROJECT)
    want = {(name + '.V4' if name in V4_SWAPPED else name, version)
            for name, version in main}
    if want == v4 and len(main) == len(v4):
        return None
    lines = [f'{os.path.relpath(REFS_V4_PROJECT, ROOT)} has drifted from '
             f'{os.path.relpath(REFS_PROJECT, ROOT)}: the two may differ only '
             'by the seven .V4 swaps']
    lines += [f'  expected, absent: {n} {v}' for n, v in sorted(want - v4)]
    lines += [f'  present, unexpected: {n} {v}' for n, v in sorted(v4 - want)]
    return '\n'.join(lines)


def tool_unready():
    """States 3, 6, 7 and 10 above, as a message, or None when the tools are ready.

    Shared by every mode that compiles, so that `--report` and `--classify`
    cannot disagree about when nothing was checked.
    """
    if not os.path.isdir(SCAFFOLD_DIR):
        return f'no scaffold directory at {SCAFFOLD_DIR}: nothing was checked'
    if not os.path.exists(TOOL_DLL):
        return f'{TOOL_DLL} is not built: nothing was checked\n' + BUILD_HINT
    for project, listing in PINS.values():
        if not os.path.exists(listing):
            return (f'no reference list at {listing}: the reference project has '
                    'not been restored and built, so nothing was checked\n'
                    + BUILD_HINT)
        stale = stale_pin(project, listing)
        if stale is not None:
            return f'{stale}\nnothing was checked\n' + BUILD_HINT
    drift = pin_drift()
    if drift is not None:
        return f'{drift}\nnothing was checked'
    return None


def stage_by_pin(blocks, root):
    """Stage each pin's blocks in a directory of its own, under `root`.

    Returns `{pin: (directory, reference list, blocks)}` for the pins that have
    blocks, or None when staging failed. The Roslyn tool takes one reference
    list per run, so a pin is a run.
    """
    scaffolds = load_scaffold()
    groups = {}
    for block in blocks:
        groups.setdefault(pin_of(block.rel, scaffolds), []).append(block)
    staged = {}
    for pin, group in sorted(groups.items()):
        directory = os.path.join(root, pin)
        if mode_stage(group, [directory], quiet=True) != 0:
            return None
        staged[pin] = (directory, PINS[pin][1], group)
    units = sum(1 for block in blocks
                if block.rel in scaffolds and scaffolds[block.rel].unit)
    print(f'{len(blocks)} blocks staged in {root}, {units} with a scaffold '
          f'unit, {len(scaffolds)} pages in the map'
          + ''.join(f'; {len(group)} on the {pin} pin'
                    for pin, (_, _, group) in staged.items() if pin != 'main'),
          file=sys.stderr)
    return staged


def pin_rule_violations(blocks, scaffolds):
    """A page marked `v4` imports a .V4 namespace, and one that does is marked."""
    importing = {block.rel for block in blocks
                 if any(V4_USING_RE.match(line) for line in block.usings)}
    marked = {rel for rel, scaffold in scaffolds.items() if scaffold.pin == 'v4'}
    violations = [f'{rel}: imports a .V4 namespace and is not marked v4 in '
                  f'{os.path.relpath(SCAFFOLD_MAP, ROOT)}'
                  for rel in sorted(importing - marked)]
    violations += [f'{rel}: marked v4, and no C# block on it imports a .V4 '
                   'namespace' for rel in sorted(marked - importing)]
    return violations, len(marked)


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
    unready = tool_unready()
    if unready is not None:
        print(unready, file=sys.stderr)
        return 2
    try:
        baseline = load_baseline()
        scaffolds = load_scaffold()
    except (BaselineError, ScaffoldError) as exc:
        print(f'{exc}: nothing was checked', file=sys.stderr)
        return 2

    staged = tempfile.mkdtemp(prefix='blockcheck-')
    try:
        groups = stage_by_pin(blocks, staged)
        if groups is None:
            return 2
        verdicts, unit_errors = {}, []
        for pin, (directory, refs, _) in groups.items():
            result = subprocess.run(['dotnet', TOOL_DLL, directory, refs],
                                    capture_output=True, text=True)
            # The V4 pin's scope lines say which pin they count, so that its
            # `N reference assemblies` is never read as the main pin's -- a
            # pin that did not restore must not shrink silently. A `scaffold
            # error:` line is not a scope line: it is kept for the findings.
            for line in result.stderr.splitlines():
                if line.startswith(SCAFFOLD_ERROR):
                    unit_errors.append(line[len(SCAFFOLD_ERROR):].split('\t'))
                    continue
                print(line + (f' ({pin} pin)' if pin != 'main' else ''),
                      file=sys.stderr)
            if result.returncode != 0:
                return 2
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

    # THE UNIT RULE, before the verdict counts: a unit that supplies more than
    # its page's building blocks use is a finding, whatever the blocks did.
    ruled = unit_rule_violations(
        blocks, {key: verdict for key, (_, verdict, _) in judged.items()},
        scaffolds)
    if ruled is None:
        print('a scaffold unit could not be read: nothing was checked',
              file=sys.stderr)
        return 2
    rule_violations, units_ruled = ruled
    for violation in rule_violations:
        print(f'SCAFFOLD RULE: {violation}', file=sys.stderr)
    # THE PAIRING RULE, beside the unit rule: the `v4` cell and the page's own
    # imports must agree, both ways. A V4 page left unmarked fails on CS0234
    # for a namespace the main pin cannot have; a marked page with no V4
    # import is compiled against the wrong AWS SDK for no reason.
    pin_violations, pins_ruled = pin_rule_violations(blocks, scaffolds)
    for violation in pin_violations:
        print(f'PIN RULE: {violation}', file=sys.stderr)
    # A UNIT THAT DOES NOT COMPILE IS A FINDING (friction #77), as a unit-rule
    # violation is. The tool keeps such errors out of every block's verdict,
    # rightly, and until spec 018 that left them a warning on a green run.
    by_ident = {block.ident: block for block in blocks}
    uncompiled = []
    for staged_unit, at, code, message, count, first in unit_errors:
        unit = os.path.relpath(os.path.join(
            SCAFFOLD_DIR, 'units', staged_unit[len('unit_'):]), ROOT)
        block = by_ident[first]
        uncompiled.append(f'{unit}:{at}  {code} {message} -- in {count} '
                          f'block(s), first {block.rel} block {block.ordinal}')

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
                         ('scaffold changed since admission', rescaffolded),
                         ('scaffold unit does not compile', uncompiled)):
        if items:
            print(f'\n----- {title} ({len(items)}) -----', file=sys.stderr)
            for item in items:
                print(item, file=sys.stderr)

    # A malformed marker is a finding REGARDLESS of the baseline, because it
    # is not a claim about whether a block compiles -- it is a claim about the
    # corpus that is wrong on its own terms.
    findings = (len(malformed) + len(regressed) + len(vanished)
                + len(unlisted) + len(rescaffolded) + len(rule_violations)
                + len(pin_violations) + len(uncompiled))
    print(f'baseline: {len(baseline)} blocks required to build', file=sys.stderr)
    print(f'scaffold rule: {units_ruled} units checked, '
          f'{len(rule_violations)} violations, '
          f'{len(uncompiled)} compile errors', file=sys.stderr)
    print(f'pin rule: {pins_ruled} pages on the v4 pin, '
          f'{len(pin_violations)} violations', file=sys.stderr)
    print(f'{findings} findings' + (f', {skipped} skipped' if skipped else ''),
          file=sys.stderr)
    return 1 if findings else 0


def mode_explain(blocks, args):
    """Every compiler error for the blocks named, in full: id, code, line:col, message.

    Ids are the ones `--report` prints in its fourth column. This is the
    Roslyn tool's `--explain`, fronted here so that nobody has to stage the
    corpus and find `refs.txt` by hand -- and so that ids travel as arguments,
    not through a shell variable that zsh declines to word-split.
    """
    if not args:
        print('usage: blockcheck.py --explain <id>...', file=sys.stderr)
        return 2
    unready = tool_unready()
    if unready is not None:
        print(unready, file=sys.stderr)
        return 2
    by_ident = {block.ident: block for block in blocks}
    unknown = sorted(set(args) - set(by_ident))
    if unknown:
        print(f'{len(unknown)} id(s) name no block, first: {unknown[0]}: nothing '
              'was explained', file=sys.stderr)
        return 2
    staged = tempfile.mkdtemp(prefix='blockcheck-explain-')
    try:
        groups = stage_by_pin([by_ident[ident] for ident in dict.fromkeys(args)],
                              staged)
        if groups is None:
            return 2
        worst = 0
        for pin, (directory, refs, group) in groups.items():
            run = subprocess.run(['dotnet', TOOL_DLL, '--explain', directory, refs,
                                  *[block.ident for block in group]],
                                 capture_output=True, text=True)
            sys.stdout.write(run.stdout)
            for line in run.stderr.splitlines():
                print(line + (f' ({pin} pin)' if pin != 'main' else ''),
                      file=sys.stderr)
            worst = max(worst, run.returncode)
        return worst
    finally:
        shutil.rmtree(staged, ignore_errors=True)


def mode_list_skips(blocks, args):
    """Every opt-out, or those on the pages named: page, ordinal, reason.

    Read from the same `skip_reason` that `--report` reads, which
    `enumerate_blocks` sets from `scan_skips` -- so the listing and the gate's
    SKIPPED count cannot disagree. An empty listing is exit 2, as for every
    listing mode: no skips and a broken scan look the same from here.
    """
    if args:
        wanted = {os.path.relpath(os.path.abspath(a), ROOT) for a in args}
        unknown = wanted - {block.rel for block in blocks}
        if unknown:
            print(f'no C# blocks on {", ".join(sorted(unknown))}: nothing was '
                  'listed', file=sys.stderr)
            return 2
        blocks = [block for block in blocks if block.rel in wanted]
    skipped = [block for block in blocks if block.skip_reason]
    for block in skipped:
        print(f'{block.rel}\t{block.ordinal}\t{block.skip_reason}')
    if not skipped:
        print(f'{len(blocks)} blocks, no opt-out: nothing to list', file=sys.stderr)
        return 2
    print(f'{len(skipped)} skipped of {len(blocks)} blocks, across '
          f'{len({block.rel for block in skipped})} pages', file=sys.stderr)
    return 0


# --------------------------------------------------------------------------
# The classification
# --------------------------------------------------------------------------
# WHAT STANDS BETWEEN A FAILED BLOCK AND BUILT -- TWICE.
#
# Spec 016 split its failures by hand, from a type dump nobody committed, and
# its figures could not be re-derived a week later. This is that split as a
# mode, so the next spec starts from a command rather than from a paragraph.
#
# THE FIRST COMPILE is the block as the gate compiles it, and its class is the
# `first` column, unchanged since spec 017:
#
#   parse      the block does not parse -- a fragment, a signature, a `...`
#   import     a missing name is a type the pin ships, or a CS1061 names an
#              extension method it ships: the page needs a `using`
#   other      any diagnostic that is not a missing name: API the page may
#              have wrong, or a block shown without its class
#   same-page  every missing name is a value or a type ANOTHER block on the
#              page declares, and at least one is the latter. Never stubbed
#   values     every missing name is lower-case or _: a variable in scope
#   page-type  the rest: a type the page names and never shows
#
# It classifies by what blocks the block TODAY, not by everything that will:
# the compiler reports the errors it reaches, and an `import` block given its
# `using`s often goes on to need a stub. Spec 017's probe measured 226 of 368.
# So that figure is a lower bound, and it is kept beside the real one.
#
# THE SECOND COMPILE (spec 018, design § The Second Compile) supplies every
# `using` the pinned type table resolves, recompiles, and classes what is left.
# That is the `class` column:
#
#   parse           as above; nothing is supplied to a block that cannot parse
#   built-by-using  builds once given its `using`s
#   defect          after the `using`s, a binder error that is not a missing
#                   name -- or a handler-base member, read below
#   same-page       after the `using`s, as `same-page` above
#   values          after the `using`s, only lower-case missing names
#   page-type       after the `using`s, a capitalised name no pin ships and the
#                   page never shows
#
# RESOLUTION IS RECEIVER-AWARE, AND THAT IS THE WHOLE DIFFERENCE FROM THE PROBE
# IT PROMOTES. Spec 017's probe resolved a name to the first pinned namespace
# that ships it, so `Order` became StackExchange.Redis and `Context` became
# Polly: 39 wrong `import` rows and 29 wrong DEFECT verdicts (design E4). A
# name is resolved only when
#   - the page does not declare it, in any of its blocks: `Order`, `Task`,
#     `Tag` and `User` are the page's own types, and
#   - the staged class's base does not supply it. A `members` block is staged
#     inside a `Holder`; when that Holder has no base, a handler's `Context`,
#     `Name` or `base.Handle…` is an artefact of the staging, not a missing
#     import. Such a name is never resolved, and a block left holding one is
#     `defect`, with the names after `wrapper:`, never `built-by-using`.
#
# THOSE TWO WERE NOT ENOUGH, and task 2.3's red-proof is what showed it. A page
# that uses `Order` without ever declaring it still got StackExchange.Redis --
# 20 rows -- and a block whose handler class derives from a not-yet-imported
# `RequestHandler` got Polly for `Context`, because both names were resolved
# in the same round. Two more rules, ruled by the maintainer 2026-10-05:
#   - TIERED. While any missing name resolves to a Paramore, System or
#     Microsoft namespace, only those are supplied, and the block recompiles.
#     A base class imported this way supplies `Context` before anything else
#     can claim it.
#   - PAGE EVIDENCE. A third-party namespace is supplied only when some block
#     on the page already imports from its root (`using Amazon.Runtime;`
#     admits `Amazon`). A page about Polly gets Polly; a page about orders
#     does not get Redis. Its cost is the other direction: a page that needs a
#     third-party type and imports nothing from that package reads
#     `page-type`, never `built-by-using`.
CLASS_ORDER = ('parse', 'built-by-using', 'defect', 'same-page', 'values',
               'page-type')
FIRST_ORDER = ('parse', 'import', 'other', 'same-page', 'values', 'page-type')
MISSING_NAME_RE = re.compile(r"name '([^']+)'")
MISSING_EXT_RE = re.compile(r"no accessible extension method '([^']+)'")
# A generic extension method whose type has a NON-generic instance overload of
# the same name -- `serviceProvider.GetService<T>()` -- is not CS1061 but
# CS0308, "the non-generic method 'IServiceProvider.GetService(Type)' cannot be
# used with type arguments". It is the same missing `using`, and task 2.4 found
# the second compile reading it as a defect (`DispatchingARequest.md` #1).
NON_GENERIC_RE = re.compile(r"non-generic method '(?:[^'(]*\.)?(\w+)\(")
TYPE_DECL_RE = re.compile(r'\b(?:class|record|interface|struct|enum)\s+([A-Za-z_]\w*)')

# The public members of the four handler bases a staged Holder can carry, read
# from source at the pinned releases -- Brighter 10.7.0 `RequestHandler.cs`,
# `RequestHandlerAsync.cs`; Darker 4.1.1 `QueryHandler.cs`,
# `QueryHandlerAsync.cs` -- with:
#   git -C ../Brighter show 10.7.0:src/Paramore.Brighter/RequestHandler.cs \
#       | grep -nE '^\s*(public|protected) '
# and the same for the other three. Re-derive them at a version bump.
HANDLER_BASE_MEMBERS = frozenset((
    'Context', 'Name', 'ContinueOnCapturedContext', 'SetSuccessor',
    'AddToLifetime', 'DescribePath', 'InitializeFromAttributeParams',
    'Handle', 'HandleAsync', 'Fallback', 'FallbackAsync',
    'Execute', 'ExecuteAsync'))

# How many times `using`s are supplied and the block recompiled. A `using`
# can uncover a name the compiler had not reached, and tiering spends a round
# per tier. Spec 017's probe settled in at most three rounds over the whole
# corpus; tiered, this one settles in three (2.3, at f6a9a7c). Not settling is
# exit 2, since a class read from an unfinished compile is not a class.
USING_ROUNDS = 8
FIRST_PARTY_TIERS = 3       # namespace_rank's tiers 0-2: Paramore, System, Microsoft
USING_ROOT_RE = re.compile(
    r'^\s*(?:global\s+)?using\s+(?:static\s+)?(?:\w+\s*=\s*)?([A-Za-z_]\w*)')


def missing_names(diagnostics):
    """(type-or-value names, extension-method names, any other diagnostic?)."""
    missing, ext_missing, other = [], [], False
    for code, message in diagnostics:
        name = MISSING_NAME_RE.search(message)
        ext = MISSING_EXT_RE.search(message)
        if code in ('CS0246', 'CS0103') and name:
            missing.append(name.group(1).split('<')[0])
        elif code == 'CS1061' and ext:
            ext_missing.append(ext.group(1))
        elif code == 'CS0308' and NON_GENERIC_RE.search(message):
            ext_missing.append(NON_GENERIC_RE.search(message).group(1))
        else:
            other = True
    return missing, ext_missing, other


def classify_failure(diagnostics, broken, types, extensions, declared_elsewhere):
    """One FAILED block's FIRST-compile class and the names that decided it."""
    missing, ext_missing, other_codes = [], [], False
    for code, message in diagnostics:
        name = MISSING_NAME_RE.search(message)
        ext = MISSING_EXT_RE.search(message)
        if code in ('CS0246', 'CS0103') and name:
            missing.append(name.group(1).split('<')[0])
        elif code == 'CS1061' and ext and ext.group(1) in extensions:
            ext_missing.append(ext.group(1))
        else:
            other_codes = True
    names = sorted(set(missing + ext_missing))
    if broken:
        return 'parse', names
    if ext_missing or any(n in types for n in missing):
        return 'import', names
    if other_codes or not missing:
        return 'other', names
    capitalised = [n for n in missing if not (n[0].islower() or n[0] == '_')]
    if capitalised and all(n in declared_elsewhere for n in capitalised):
        return 'same-page', names
    if not capitalised:
        return 'values', names
    return 'page-type', names


def namespace_rank(namespace):
    """Which of several pinned namespaces shipping one name a reader means.

    Brighter's own first, then the framework's, then Microsoft's, then the
    rest; shorter before longer. The probe's order, carried over, so that AC7's
    reconciliation differs only where the receiver-aware rule says it should.
    """
    tier = (0 if namespace.startswith('Paramore') else
            1 if namespace.startswith('System') else
            2 if namespace.startswith('Microsoft') else 3)
    return (tier, len(namespace), namespace)


def resolvable(name, table, declared, artefacts):
    """The namespace a missing name resolves to, or None. Receiver-aware."""
    if name in declared or name in artefacts or name not in table:
        return None
    return min(table[name], key=namespace_rank)


def classify_second(diagnostics, artefacts, declared_elsewhere, supplied):
    """One block's SECOND-compile class, from what is left after its `using`s.

    `artefacts` is the set of handler-base names the block's staging leaves
    unsupplied; `supplied` the namespaces the second compile added.
    """
    if not diagnostics:
        return 'built-by-using', sorted(supplied)
    missing, ext_missing, other = missing_names(diagnostics)
    stranded = sorted({n for n in missing + ext_missing if n in artefacts})
    if other or ext_missing or stranded:
        codes = sorted({code for code, message in diagnostics
                        if not (code in ('CS0246', 'CS0103')
                                and MISSING_NAME_RE.search(message))})
        return 'defect', codes + ([f'wrapper:{"+".join(stranded)}']
                                  if stranded else [])
    names = sorted(set(missing))
    capitalised = [n for n in names if not (n[0].islower() or n[0] == '_')]
    if capitalised and all(n in declared_elsewhere for n in capitalised):
        return 'same-page', names
    if not capitalised:
        return 'values', names
    return 'page-type', names


def mode_classify(blocks, args):
    """Classify every FAILED block, or those of the pages named, twice.

    Rows go to stdout as page, ordinal, class, first, names -- sorted, so two
    runs `diff` clean -- and the counts per class go to stderr. `class` is the
    second compile's and `first` the first's; see the comment above
    CLASS_ORDER. `names` is what decided the class: the namespaces supplied for
    `built-by-using`, the diagnostic codes for `defect` (and `wrapper:` names),
    the missing names otherwise.

    A classification is data, like `--list`, so it exits 0; it exits 2 when
    the tools are not ready or when there is nothing FAILED to classify, since
    an empty listing and a clean corpus are different claims.
    """
    unready = tool_unready()
    if unready is not None:
        print(unready, file=sys.stderr)
        return 2
    page_blocks = blocks
    if args:
        wanted = {os.path.relpath(os.path.abspath(a), ROOT) for a in args}
        unknown = wanted - {block.rel for block in blocks}
        if unknown:
            print(f'no C# blocks on {", ".join(sorted(unknown))}: nothing was '
                  'classified', file=sys.stderr)
            return 2
        blocks = [block for block in blocks if block.rel in wanted]

    def tool(*argv):
        run = subprocess.run(['dotnet', TOOL_DLL, *argv],
                             capture_output=True, text=True)
        if run.returncode != 0:
            sys.stderr.write(run.stderr)
            return None
        return [line for line in run.stdout.split('\n') if line]

    def explain(idents):
        """{ident: [(code, message)]}, or None when the tool failed.

        Each block is explained against its own pin, from the directory it
        was staged into.
        """
        found = {}
        for pin, (directory, refs, _) in groups.items():
            mine = [ident for ident in idents if pin_by_ident[ident] == pin]
            if not mine:
                continue
            lines = tool('--explain', directory, refs, *mine)
            if lines is None:
                return None
            for line in lines:
                fields = line.split('\t', 3)
                found.setdefault(fields[0], []).append((fields[1], fields[3]))
        return found

    # Declarations are read from the page's text, not the staged file, so a
    # wrapper's own class is never mistaken for one the page shows -- and from
    # every block of the page, named or not, since `same-page` and the
    # receiver-aware rule both ask about the page.
    declared = {}
    for block in page_blocks:
        for name in TYPE_DECL_RE.findall(block.text):
            declared.setdefault(block.rel, {}).setdefault(name, set()).add(block.ordinal)
    # The page's evidence for a third-party package: the roots of the
    # namespaces its own blocks import.
    roots = {}
    for block in page_blocks:
        for line in block.usings:
            match = USING_ROOT_RE.match(line)
            if match:
                roots.setdefault(block.rel, set()).add(match.group(1))

    staged = tempfile.mkdtemp(prefix='blockcheck-classify-')
    try:
        groups = stage_by_pin(blocks, staged)
        if groups is None:
            return 2
        pin_by_ident = {block.ident: pin
                        for pin, (_, _, group) in groups.items() for block in group}
        verdicts, parsed, dumps = [], [], {}
        for pin, (directory, refs, _) in groups.items():
            ran = tool(directory, refs)
            parses = tool('--parse', directory)
            dumps[pin] = tool('--types', refs)
            if ran is None or parses is None or dumps[pin] is None:
                return 2
            verdicts += ran
            parsed += parses
        failed_ids = {line.split('\t')[0] for line in verdicts
                      if line.split('\t')[1] == 'FAILED'}
        failed = [block for block in blocks
                  if block.ident in failed_ids and not block.skip_reason]
        if not failed:
            print(f'{len(blocks)} blocks, none FAILED: nothing to classify',
                  file=sys.stderr)
            return 2

        first = explain([block.ident for block in failed])
        if first is None:
            return 2
        broken = {line.split('\t')[0] for line in parsed
                  if line.split('\t')[1] == 'BROKEN'}
        # One type table per pin: a V4 page's names resolve against the V4
        # packages, which is the point of compiling it against them.
        table, ext_table = {}, {}
        for pin, dump in dumps.items():
            table[pin], ext_table[pin] = {}, {}
            for line in dump:
                kind, name, namespace = (line.split('\t') + ['', ''])[:3]
                if namespace and kind in ('type', 'ext'):
                    (table if kind == 'type' else ext_table)[pin].setdefault(
                        name, set()).add(namespace)
        path_of = {block.ident: os.path.join(groups[pin_by_ident[block.ident]][0],
                                             block.ident + '.cs')
                   for block in failed}

        # THE SECOND COMPILE. Each round supplies the `using`s the last one's
        # diagnostics resolve, rewrites the staged file as those directives
        # above the first compile's text, and recompiles. A block that cannot
        # parse is given nothing.
        texts, supplied, artefacts = {}, {}, {}
        for block in failed:
            with open(path_of[block.ident], encoding='utf-8') as fh:
                texts[block.ident] = fh.read()
            supplied[block.ident] = set()
            # The staged Holder has a base only when the wrapper wrote one;
            # read from the staged text, so the rule follows the staging.
            holder_without_base = (block.shape == 'members'
                                   and '\npublic class Holder\n' in
                                   '\n' + texts[block.ident])
            artefacts[block.ident] = (HANDLER_BASE_MEMBERS if holder_without_base
                                      else frozenset())
        second = {ident: diags for ident, diags in first.items()}
        candidates = [block for block in failed if block.ident not in broken]
        settled = 0
        for round_ in range(USING_ROUNDS + 1):
            changed = []
            for block in candidates:
                missing, ext_missing, _ = missing_names(second.get(block.ident, []))
                page_declared = set(declared.get(block.rel, {}))
                pin = pin_by_ident[block.ident]
                new = {namespace for namespace in
                       [resolvable(n, table[pin], page_declared,
                                   artefacts[block.ident])
                        for n in missing]
                       + [resolvable(n, ext_table[pin], page_declared,
                                     artefacts[block.ident])
                          for n in ext_missing]
                       if namespace} - supplied[block.ident]
                first_party = {namespace for namespace in new
                               if namespace_rank(namespace)[0] < FIRST_PARTY_TIERS}
                new = first_party or {
                    namespace for namespace in new
                    if namespace.split('.')[0] in roots.get(block.rel, ())}
                if new:
                    supplied[block.ident] |= new
                    changed.append(block.ident)
                    with open(path_of[block.ident], 'w', encoding='utf-8') as fh:
                        fh.write(''.join(f'using {namespace};\n' for namespace
                                         in sorted(supplied[block.ident]))
                                 + texts[block.ident])
            if not changed:
                settled = round_
                break
            if round_ == USING_ROUNDS:
                print(f'the second compile had not settled after {USING_ROUNDS} '
                      f'rounds ({len(changed)} blocks still changing): nothing '
                      'was classified', file=sys.stderr)
                return 2
            again = explain(changed)
            if again is None:
                return 2
            for ident in changed:
                second[ident] = again.get(ident, [])
    finally:
        shutil.rmtree(staged, ignore_errors=True)

    # The first compile's evidence is every name the dump lists, a type in
    # the global namespace included -- as spec 017 read it. Only the second
    # compile needs a namespace, because it has to write the `using`.
    types = {pin: {line.split('\t')[1] for line in dump if line.startswith('type\t')}
             for pin, dump in dumps.items()}
    extensions = {pin: {line.split('\t')[1] for line in dump
                        if line.startswith('ext\t')}
                  for pin, dump in dumps.items()}
    counts = {name: 0 for name in CLASS_ORDER}
    pages = {name: set() for name in CLASS_ORDER}
    firsts = {name: 0 for name in FIRST_ORDER}
    for block in sorted(failed, key=lambda b: (b.rel, b.ordinal)):
        elsewhere = {name for name, ordinals in declared.get(block.rel, {}).items()
                     if ordinals - {block.ordinal}}
        pin = pin_by_ident[block.ident]
        was, first_names = classify_failure(first.get(block.ident, []),
                                            block.ident in broken, types[pin],
                                            extensions[pin], elsewhere)
        if block.ident in broken:
            cls, names = 'parse', first_names
        else:
            cls, names = classify_second(second.get(block.ident, []),
                                         artefacts[block.ident], elsewhere,
                                         supplied[block.ident])
        counts[cls] += 1
        pages[cls].add(block.rel)
        firsts[was] += 1
        print(f'{block.rel}\t{block.ordinal}\t{cls}\t{was}\t'
              f'{",".join(names) or "-"}')

    print(f'{len(failed)} FAILED blocks classified at the second compile: '
          + ', '.join(f'{counts[c]} {c} ({len(pages[c])} pages)' for c in CLASS_ORDER),
          file=sys.stderr)
    print(f'the second compile settled after {settled} round(s) of `using`s',
          file=sys.stderr)
    print('at the first compile: '
          + ', '.join(f'{firsts[c]} {c}' for c in FIRST_ORDER), file=sys.stderr)
    return 0


def main(argv):
    mode = argv[0] if argv else None
    if mode == '--list-scaffold':
        return mode_list_scaffold(argv[1:])
    if mode not in ('--list', '--show', '--stage', '--report',
                    '--verify-extraction', '--classify', '--list-skips', '--explain'):
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
        if mode == '--classify':
            return mode_classify(blocks, argv[1:])
        if mode == '--list-skips':
            return mode_list_skips(blocks, argv[1:])
        if mode == '--explain':
            return mode_explain(blocks, argv[1:])
    except ScaffoldError as exc:
        print(exc, file=sys.stderr)
        return 2
    return mode_show(blocks, argv[1:])


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
