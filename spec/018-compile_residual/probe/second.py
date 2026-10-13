"""Every error of a page's FAILED blocks at the SECOND compile: after `--classify` has supplied the
`using`s it can, so a missing `using` cannot hide a defect behind it. `--explain` shows the first
compile only, which is why phase 3's defect reading needed this.

`--classify` stages into a temporary directory and deletes it. This keeps it: `tempfile.mkdtemp` is
pointed at <scratch> and `shutil.rmtree` is made a no-op, for this process only. Nothing in tools/
is changed. Then the gate's own Roslyn tool explains the kept stage, one pin directory at a time.

Usage, from the repo root, after the builds in tools/README.md row 9:
    python3 spec/018-compile_residual/probe/second.py <scratch> <page>...
Prints  id <TAB> code <TAB> line:col <TAB> message  for every FAILED block on the pages named.
"""
import contextlib, io, os, shutil, subprocess, sys, tempfile
sys.path.insert(0, 'tools')
import blockcheck as bc

W = os.path.abspath(sys.argv[1]); pages = sys.argv[2:]
if not pages:
    sys.exit('usage: second.py <scratch> <page>...')
shutil.rmtree(W, ignore_errors=True); os.makedirs(W)
tempfile.mkdtemp = lambda prefix='', **_: W            # the stage lands here
bc.shutil.rmtree = lambda *a, **k: None                # and stays

with contextlib.redirect_stdout(io.StringIO()) as rows:
    rc = bc.main(['--classify', *pages])
if rc:
    sys.exit(f'--classify exited {rc}')
failed = [l.split('\t') for l in rows.getvalue().splitlines()]
ident = {(b.rel, b.ordinal): b.ident for b in bc.enumerate_blocks()}
wanted = {ident[(p, int(n))] for p, n, *_ in failed}

for pin, (_, refs) in sorted(bc.PINS.items()):
    stage = os.path.join(W, pin)
    if not os.path.isdir(stage):
        continue
    here = sorted(i for i in wanted if os.path.exists(os.path.join(stage, i + '.cs')))
    if not here:
        continue
    out = subprocess.run(['dotnet', bc.TOOL_DLL, '--explain', stage, refs, *here],
                         capture_output=True, text=True)
    sys.stdout.write(out.stdout)
