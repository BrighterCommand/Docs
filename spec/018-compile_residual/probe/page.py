"""The verdict of every C# block on the pages named, and the full diagnostics of each FAILED one.

The repair loop for phases 3–7: `--report` compiles the whole corpus to answer for one page, and
`--classify` prints only what failed. This stages the pages named, each against its own pin and
with its scaffold unit, exactly as `--report` does (`stage_by_pin`), and asks the gate's Roslyn tool
for verdicts and then for the errors. It is a view, not a gate: the unit rule and the baseline are
`--report`'s, and the phase close runs that.

Usage, from the repo root, after the builds in tools/README.md row 9:
    python3 spec/018-compile_residual/probe/page.py <page>...
Prints  page #n VERDICT  and, under a FAILED block, its  code line:col message  lines.
"""
import os, shutil, subprocess, sys, tempfile
sys.path.insert(0, 'tools')
import blockcheck as bc

wanted = {os.path.relpath(os.path.abspath(p), bc.ROOT) for p in sys.argv[1:]}
if not wanted:
    sys.exit('usage: page.py <page>...')
blocks = [b for b in bc.enumerate_blocks() if b.rel in wanted]
missing = wanted - {b.rel for b in blocks}
if missing:
    sys.exit(f'no C# blocks on {", ".join(sorted(missing))}')

def tool(*argv):
    run = subprocess.run(['dotnet', bc.TOOL_DLL, *argv], capture_output=True, text=True)
    if run.returncode != 0:
        sys.exit(run.stderr)
    return [l for l in run.stdout.split('\n') if l]

staged = tempfile.mkdtemp(prefix='page-')
try:
    groups = bc.stage_by_pin(blocks, staged)
    if groups is None:
        sys.exit('staging failed')
    verdict, errors = {}, {}
    for pin, (directory, refs, group) in groups.items():
        for line in tool(directory, refs):
            ident, v = line.split('\t')[:2]
            verdict[ident] = v
        failed = [b.ident for b in group if verdict.get(b.ident) == 'FAILED']
        if failed:
            for line in tool('--explain', directory, refs, *failed):
                f = line.split('\t', 3)
                errors.setdefault(f[0], []).append(f'{f[1]} {f[2]} {f[3]}')
finally:
    shutil.rmtree(staged, ignore_errors=True)

for b in sorted(blocks, key=lambda b: (b.rel, b.ordinal)):
    v = 'SKIPPED' if b.skip_reason else verdict.get(b.ident, '?')
    print(f'{os.path.basename(b.rel)} #{b.ordinal} {v}' + (f' ({b.skip_reason})' if b.skip_reason else ''))
    for e in errors.get(b.ident, []):
        print(f'    {e}')
