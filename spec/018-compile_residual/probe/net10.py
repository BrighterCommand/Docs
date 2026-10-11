"""Task 2.11 (friction #76, design D7): what the `net9.0` pins miss against `net10.0`.

Both pins (refs.csproj, refs-v4.csproj) are copied into the scratch directory with their
`<TargetFramework>` changed to net10.0 and the pin's three framework-tied packages raised to what a
net10.0 reader gets (BUMPS), and restored there. The TargetFramework change alone does not restore:
Paramore.Brighter.*.EntityFrameworkCore 10.7.0 needs EF Core >= 10.0.10 on net10.0, and the pin's
9.0.15 is then a downgrade, NU1605. Then:
  packages  every package each framework resolves, from project.assets.json, net9.0 against net10.0
  verdicts  the corpus staged once, by the gate's own stage_by_pin, and compiled by the gate's Roslyn
            tool twice per pin: against the committed net9.0 refs.txt and the scratch net10.0 one
Nothing under tools/ is written. The committed pins must be built first (the gate's own build hint).

    python3 spec/018-compile_residual/probe/net10.py <scratch dir>     # exit 0 when the control moved
"""
import json, os, shutil, subprocess, sys
sys.path.insert(0, 'tools')
import blockcheck as bc

W = os.path.abspath(sys.argv[1])
PINS = {   # pin: (committed project, its net9.0 refs.txt)
    'main': (bc.REFS_PROJECT, bc.REFS_LIST),
    'v4': (os.path.join(bc.ROOT, 'tools', 'blockcheck', 'refs-v4', 'refs-v4.csproj'), bc.REFS_V4_LIST),
}
# Each from the package that sets it: the net10.0 dependency group of the Brighter 10.7.0 nuspec
# (EF Core, TickerQ), or, for the third-party provider, its latest stable release for EF Core 10.
BUMPS = [
    ('<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.15" />',
     '<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.10" />'),
    ('<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />',
     '<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.3" />'),
    ('<PackageReference Include="TickerQ.Dashboard" Version="9.0.2" />',
     '<PackageReference Include="TickerQ.Dashboard" Version="10.4.0" />'),
    ('<PackageReference Include="TickerQ.EntityFrameworkCore" Version="9.0.2" />',
     '<PackageReference Include="TickerQ.EntityFrameworkCore" Version="10.4.0" />'),
]
NET9, NET10 = '<TargetFramework>net9.0</TargetFramework>', '<TargetFramework>net10.0</TargetFramework>'


def packages(project_dir, tfm):
    """{package: version} that `tfm` resolved, from the project's own restore."""
    with open(os.path.join(project_dir, 'obj', 'project.assets.json'), encoding='utf-8') as fh:
        targets = json.load(fh)['targets']
    (key,) = [k for k in targets if k.startswith(tfm)]
    return {name.split('/')[0]: name.split('/')[1]
            for name, lib in targets[key].items() if lib.get('type') == 'package'}


def verdicts(directory, refs):
    run = subprocess.run(['dotnet', bc.TOOL_DLL, directory, refs], capture_output=True, text=True)
    if run.returncode != 0:
        sys.exit(f'the tool failed on {refs}:\n{run.stderr}')
    rows = {}
    for line in run.stdout.split('\n'):
        if line:
            ident, verdict, _, codes = line.split('\t')
            rows[ident] = (verdict, codes)
    scope = [line for line in run.stderr.splitlines() if 'reference assemblies' in line]
    return rows, scope[0] if scope else '?'


shutil.rmtree(W, ignore_errors=True)
os.makedirs(W)
net10_refs = {}
for pin, (project, _) in PINS.items():
    target = os.path.join(W, f'refs10-{pin}')
    os.makedirs(target)
    with open(project, encoding='utf-8') as fh:
        text = fh.read()
    assert text.count(NET9) == 1, f'{project}: expected one {NET9}'
    text = text.replace(NET9, NET10)
    for old, new in BUMPS:
        assert text.count(old) == 1, f'{project}: expected one {old}'
        text = text.replace(old, new)
    copy = os.path.join(target, os.path.basename(project))
    with open(copy, 'w', encoding='utf-8') as fh:
        fh.write(text)
    build = subprocess.run(['dotnet', 'build', copy, '-c', 'Release'], capture_output=True, text=True)
    if build.returncode != 0:
        sys.exit(f'{copy} did not build:\n{build.stdout[-3000:]}')
    for line in sorted({l.split(': warning ')[1] for l in build.stdout.splitlines() if ': warning NU' in l}):
        print(f'  restore ({pin}): warning {line}')
    net10_refs[pin] = os.path.join(target, 'bin', 'Release', 'net10.0', 'refs.txt')

    old = packages(os.path.dirname(project), 'net9.0')
    new = packages(target, 'net10.0')
    moved = sorted(name for name in old.keys() & new.keys() if old[name] != new[name])
    print(f'\n== {pin} pin: {len(old)} packages on net9.0, {len(new)} on net10.0, '
          f'{len(moved)} at a different version ==')
    for name in moved:
        print(f'  {name}\t{old[name]} -> {new[name]}')
    for name in sorted(old.keys() - new.keys()):
        print(f'  {name}\t{old[name]} -> (absent)')
    for name in sorted(new.keys() - old.keys()):
        print(f'  {name}\t(absent) -> {new[name]}')

groups = bc.stage_by_pin(bc.enumerate_blocks(), os.path.join(W, 'staged'))
skipped = {block.ident for block in bc.enumerate_blocks() if block.skip_reason}
print('\n== verdicts, net9.0 against net10.0 (SKIPPED blocks excluded, as --report discards them) ==')
total_moved = 0
for pin, (directory, refs9, group) in groups.items():
    v9, scope9 = verdicts(directory, refs9)
    v10, scope10 = verdicts(directory, net10_refs[pin])
    judged = [block.ident for block in group if block.ident not in skipped]
    built9 = sum(1 for i in judged if v9[i][0] == 'BUILT')
    built10 = sum(1 for i in judged if v10[i][0] == 'BUILT')
    print(f'{pin} pin: {len(judged)} judged; net9.0 {built9} BUILT ({scope9}); '
          f'net10.0 {built10} BUILT ({scope10})')
    for ident in judged:
        if v9[ident][0] != v10[ident][0]:
            total_moved += 1
            print(f'  VERDICT MOVED  {ident}\t{v9[ident][0]} {v9[ident][1]} -> {v10[ident][0]} {v10[ident][1]}')
    codes = [i for i in judged if v9[i][0] == v10[i][0] == 'FAILED' and v9[i][1] != v10[i][1]]
    for ident in codes:
        print(f'  codes moved    {ident}\t{v9[ident][1]} -> {v10[ident][1]}')
print(f'\n{total_moved} verdict(s) moved')

# THE CONTROL: the comparison above must be able to see a move. The main pin's net10.0 list with
# TickerQ's assemblies removed has to move TickerQScheduler.md's BUILT blocks, or `0 moved` is an
# instrument that cannot fail.
directory, refs9, group = groups['main']
cut = os.path.join(W, 'refs10-main-no-tickerq.txt')
with open(net10_refs['main'], encoding='utf-8') as src, open(cut, 'w', encoding='utf-8') as dst:
    dst.writelines(line for line in src if not os.path.basename(line.strip()).startswith('TickerQ'))
v9, _ = verdicts(directory, refs9)
vcut, _ = verdicts(directory, cut)
control = sorted(i for i in v9 if i not in skipped and v9[i][0] != vcut[i][0])
print(f'control, net10.0 without TickerQ: {len(control)} verdict(s) moved: ' + ', '.join(control))
shutil.rmtree(os.path.join(W, 'staged'), ignore_errors=True)
sys.exit(0 if control and all(i.startswith('TickerQ') for i in control) else 1)
