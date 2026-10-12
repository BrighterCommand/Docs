"""Task 2.9's red-proof: a scaffold unit that does not compile is a finding (friction #77).

Runs the gate's own `--report`, in process, twice:
  control  the real units                                   -> 0 compile errors
  plant    one unit given `using Plant.DoesNotExist;`        -> 1 compile error, 1 more finding, exit 1
The plant is a scratch copy of the unit; `load_scaffold` is patched to point the unit's pages at it,
so nothing under tools/ is edited. The plant is a CS0246 in the unit's own tree that the unit rule
cannot see (it reads declarations and `global using`, not plain `using`s), so the only check that
can turn it into a finding is the one under test. The plant run also asserts the unit rule stayed
at 0, which is what makes that true rather than assumed.

    python3 spec/018-compile_residual/probe/unit_plants.py <scratch dir>     # exit 0 when both behave
"""
import contextlib, io, os, re, shutil, sys
sys.path.insert(0, 'tools')
import blockcheck as bc

W = os.path.abspath(sys.argv[1])
UNIT = 'PageContext.cs'
PLANT = 'using Plant.DoesNotExist;\n'
SUMMARY_RE = re.compile(r'^scaffold rule: (\d+) units checked, (\d+) violations, (\d+) compile errors$', re.M)
FINDINGS_RE = re.compile(r'^(\d+) findings', re.M)
LOAD = bc.load_scaffold   # the real one, bound before any run replaces it


def report(load):
    """(exit code, stderr) of one `--report`, with `load_scaffold` replaced by `load`."""
    bc.load_scaffold = load
    err = io.StringIO()
    try:
        with contextlib.redirect_stderr(err):
            code = bc.mode_report(bc.enumerate_blocks(), [os.path.join(W, 'rows.tsv')])
    finally:
        bc.load_scaffold = LOAD
    return code, err.getvalue()


def planted():
    scaffolds = LOAD()
    original = os.path.join(bc.SCAFFOLD_DIR, 'units', UNIT)
    copy = os.path.join(W, UNIT)   # same basename: the staged name and the baseline's both read it
    with open(original, encoding='utf-8') as src, open(copy, 'w', encoding='utf-8') as dst:
        dst.write(PLANT + src.read())
    for scaffold in scaffolds.values():
        if scaffold.unit == original:
            scaffold.unit = copy
    return scaffolds


os.makedirs(W, exist_ok=True)
runs = {'control': report(LOAD), 'plant': report(planted)}
failed = 0
measured = {}
for name, (code, err) in runs.items():
    units, violations, compile_errors = map(int, SUMMARY_RE.search(err).groups())
    findings = int(FINDINGS_RE.search(err).group(1))
    measured[name] = (code, findings, violations, compile_errors)
    heading = '----- scaffold unit does not compile' in err
    print(f'{name:8} exit {code}  findings {findings}  unit rule {violations}  '
          f'compile errors {compile_errors}  heading {"yes" if heading else "no"}')
    if name == 'plant':
        for line in err.split('\n'):
            if 'CS0246' in line and UNIT in line:
                print(f'         {line}')

c, p = measured['control'], measured['plant']
checks = [
    ('control: 0 compile errors', c[3] == 0),
    ('plant: 1 compile error', p[3] == 1),
    ('plant: the unit rule did not see it', p[2] == 0),
    ('plant: one finding more than the control', p[1] == c[1] + 1),
    ('plant: exit 1', p[0] == 1),
]
for label, ok in checks:
    print(f'{"ok  " if ok else "FAIL"} {label}')
    failed += not ok
shutil.rmtree(W, ignore_errors=True)
sys.exit(1 if failed else 0)
