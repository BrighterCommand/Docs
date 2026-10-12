"""Task 2.1: which FAILED blocks would phase 2's three staging changes make BUILT, before any is made.

Each change is prototyped here rather than in tools/blockcheck.py, and only on its candidates:
  H  2.5  the handler wrapper -- a `members` block overriding Handle/HandleAsync/Execute/ExecuteAsync
          gets `Holder : <fully qualified base>`, the base read from the joined signature (design D2)
  S  2.7  Shouldly 4.3.0 in the pin -- blocks calling `.Should…`, compiled against a scratch pin with
          Shouldly added; once as the page has them, once with `using Shouldly;` (7.4's repair, a
          separate prediction)
  P  2.8  `Program` -- a `statements` block naming `Program`, staged unwrapped as a top-level file

Usage, from the repo root:  python3 spec/018-compile_residual/probe/predict.py <scratch dir>
Needs the main pin built. Writes <scratch>/predicted.tsv: set, page, ordinal, verdict, codes.
"""
import os, re, shutil, subprocess, sys
sys.path.insert(0, 'tools')
import blockcheck as bc

W = os.path.abspath(sys.argv[1]); os.makedirs(W, exist_ok=True)
blocks = bc.enumerate_blocks()
scaffolds = bc.load_scaffold()

# Today's verdicts: the set is FAILED blocks only, since a BUILT one cannot rise and a SKIPPED one is not judged.
rep = os.path.join(W, 'report.tsv')
subprocess.run([sys.executable, 'tools/blockcheck.py', '--report', rep], capture_output=True)
now = {(r[1], int(r[2])): r[0] for r in (l.rstrip('\n').split('\t') for l in open(rep))}
failed = [b for b in blocks if now[(b.rel, b.ordinal)] == 'FAILED']

# --- H: the handler wrapper -------------------------------------------------------------------
OVERRIDE = re.compile(r'\boverride\b[^;{]*?\b(Handle|HandleAsync|Execute|ExecuteAsync)\s*\(')
def signature_base(block):
    """The base for the block's overrides, or (None, why). Lines are joined up to each `{`."""
    text = ' '.join(l.strip() for l in block.rest)
    bases = set()
    for m in re.finditer(r'\boverride\s+(.+?)\b(Handle|HandleAsync|Execute|ExecuteAsync)\s*\(\s*([\w.<>, ?]+?)\s+@?\w+\s*[,)=]', text):
        ret, meth, arg = m.group(1).strip(), m.group(2), m.group(3).strip()
        ret = re.sub(r'^(?:async\s+|public\s+|protected\s+)*', '', ret).strip()
        if meth == 'Handle':
            bases.add(f'Paramore.Brighter.RequestHandler<{arg}>')
        elif meth == 'HandleAsync':
            bases.add(f'Paramore.Brighter.RequestHandlerAsync<{arg}>')
        elif meth == 'Execute':
            bases.add(f'Paramore.Darker.QueryHandler<{arg}, {ret}>')
        else:
            inner = re.match(r'(?:System\.Threading\.Tasks\.)?Task<(.+)>$', ret)
            if not inner:
                return None, f'ExecuteAsync returns {ret!r}, not Task<T>'
            bases.add(f'Paramore.Darker.QueryHandlerAsync<{arg}, {inner.group(1)}>')
    if not bases:
        return None, 'no parameter to read the request type from'
    # Handle + HandleAsync on one request type cannot share one base either; any two bases is "do not wrap"
    if len(bases) > 1:
        return None, 'two bases: ' + ' | '.join(sorted(bases))
    return bases.pop(), None

H = [b for b in failed if b.shape == 'members' and OVERRIDE.search(' '.join(b.rest))]

# --- S: Shouldly ------------------------------------------------------------------------------
S = [b for b in failed if re.search(r'\.Should[A-Z]\w*\s*[(<]', b.text)]

# --- P: Program -------------------------------------------------------------------------------
P = [b for b in failed if b.shape == 'statements' and re.search(r'\bProgram\b', b.text)]

def stage(dirname, items):
    """items: [(block, text)]. Writes the staged dir the Roslyn tool reads, units copied as mode_stage does."""
    d = os.path.join(W, dirname); shutil.rmtree(d, ignore_errors=True); os.makedirs(d)
    units, index = {}, []
    for block, text in items:
        sc = scaffolds.get(block.rel)
        unit = '-'
        if sc and sc.unit:
            unit = 'unit_' + os.path.basename(sc.unit)
            if unit not in units:
                shutil.copy(sc.unit, os.path.join(d, unit)); units[unit] = 1
        open(os.path.join(d, block.ident + '.cs'), 'w').write(text)
        index.append((block.ident, block.rel, block.ordinal, block.shape, 0, 0, 0, unit))
    open(os.path.join(d, 'index.tsv'), 'w').write(''.join('\t'.join(map(str, r)) + '\n' for r in index))
    return d

def compile_(d, refs):
    out = subprocess.run(['dotnet', bc.TOOL_DLL, d, refs], capture_output=True, text=True)
    if out.returncode != 0:
        sys.exit(f'compile failed on {d}: {out.stderr}')
    return {l.split('\t')[0]: (l.split('\t')[1], l.split('\t')[3]) for l in out.stdout.splitlines() if l}

rows = []
# H
staged, unwrapped = [], []
for b in H:
    base, why = signature_base(b)
    if base is None:
        unwrapped.append((b, why)); continue
    text, *_ = bc.wrap(b, scaffolds.get(b.rel))
    if scaffolds.get(b.rel) and scaffolds[b.rel].prelude_lines:
        unwrapped.append((b, 'page has a prelude, not the members wrapper')); continue
    assert text.count('public class Holder\n') == 1, b.ident
    staged.append((b, text.replace('public class Holder\n', f'public class Holder : {base}\n', 1)))
v = compile_(stage('H', staged), bc.REFS_LIST)
for b, _ in staged:
    rows.append(('H', b.rel, b.ordinal) + v[b.ident])
for b, why in unwrapped:
    rows.append(('H-unwrapped', b.rel, b.ordinal, 'FAILED', why))

# S: a scratch pin with Shouldly, built from a copy of refs.csproj
pin = os.path.join(W, 'refs-shouldly'); shutil.rmtree(pin, ignore_errors=True); os.makedirs(pin)
proj = open(bc.REFS_PROJECT).read()
anchor = '<PackageReference Include="Paramore.Brighter"'
assert anchor in proj
proj = proj.replace(anchor, '<PackageReference Include="Shouldly" Version="4.3.0" />\n    ' + anchor, 1)
open(os.path.join(pin, 'refs.csproj'), 'w').write(proj)
b_ = subprocess.run(['dotnet', 'build', os.path.join(pin, 'refs.csproj'), '-c', 'Release'], capture_output=True, text=True)
if b_.returncode != 0:
    sys.exit('Shouldly pin did not build:\n' + b_.stdout[-2000:])
srefs = os.path.join(pin, 'bin', 'Release', 'net9.0', 'refs.txt')
n_refs = sum(1 for l in open(srefs) if not l.startswith('#'))
v = compile_(stage('S', [(b, bc.wrap(b, scaffolds.get(b.rel))[0]) for b in S]), srefs)
for b in S:
    rows.append(('S', b.rel, b.ordinal) + v[b.ident])
v = compile_(stage('S-using', [(b, 'using Shouldly;\n' + bc.wrap(b, scaffolds.get(b.rel))[0]) for b in S]), srefs)
for b in S:
    rows.append(('S+using', b.rel, b.ordinal) + v[b.ident])

# P
staged = []
for b in P:
    sc = scaffolds.get(b.rel)
    staged.append((b, '\n'.join(list(b.usings) + (sc.usings if sc else []) + b.rest) + '\n'))
v = compile_(stage('P', staged), bc.REFS_LIST)
for b in P:
    rows.append(('P', b.rel, b.ordinal) + v[b.ident])

with open(os.path.join(W, 'predicted.tsv'), 'w') as fh:
    for r in rows:
        fh.write('\t'.join(map(str, r)) + '\n')
print(f'FAILED today: {len(failed)}; candidates H {len(H)} (wrapped {len(H) - len(unwrapped)}), S {len(S)}, P {len(P)}; '
      f'Shouldly pin {n_refs} assemblies', file=sys.stderr)
for r in rows:
    print('\t'.join(map(str, r)))
