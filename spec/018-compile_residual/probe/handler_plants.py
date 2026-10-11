"""Task 2.5's red-proof: one planted handler block per row of the wrapper's table (D2, D9).

Each plant is a `members` block, built as a `blockcheck.Block` and staged by `blockcheck.wrap`, so
the code under test is the wrapper itself, not a hand-written copy of its output. A scaffold unit
declares the plants' request and query types. Each is compiled twice by the gate's Roslyn tool:
  wrapped    as `wrap` stages it                         -> must be BUILT
  base-less  the same text with the Holder's base removed -> must be FAILED (it has nothing to override)
Two more plants must stay unwrapped, with `handler_base` saying why: `ExecuteAsync(...)`, and
overrides that imply two bases. And one is wrapped and must still FAIL: an `ExecuteAsync` with no
CancellationToken overrides nothing, and the Holder being abstract must not hide that (CS0115).

    python3 spec/018-compile_residual/probe/handler_plants.py <scratch dir>     # exit 0 when all behave
"""
import os, re, shutil, subprocess, sys
sys.path.insert(0, 'tools')
import blockcheck as bc

W = os.path.abspath(sys.argv[1])
UNIT = '''using System;
using System.Threading;
using System.Threading.Tasks;
using Paramore.Brighter;
using Paramore.Darker;

public class PlantCommand : Command { public PlantCommand() : base(Guid.NewGuid()) { } }
public class PlantQuery : IQuery<string> { }
'''
USINGS = ['using System;', 'using System.Threading;', 'using System.Threading.Tasks;',
          'using Paramore.Brighter;', 'using Paramore.Darker;']
PLANTS = {   # name: (body, the base wrap must write)
    'Handle': (['public override PlantCommand Handle(PlantCommand command)', '{',
                '    Console.WriteLine(Context);', '    return base.Handle(command);', '}'],
               'Paramore.Brighter.RequestHandler<PlantCommand>'),
    'HandleAsync': (['public override async Task<PlantCommand> HandleAsync(',
                     '    PlantCommand command, CancellationToken cancellationToken = default)', '{',
                     '    Console.WriteLine(Context);',
                     '    return await base.HandleAsync(command, cancellationToken);', '}'],
                    'Paramore.Brighter.RequestHandlerAsync<PlantCommand>'),
    'Execute': (['public override string Execute(PlantQuery query)', '{', '    return "x";', '}'],
                'Paramore.Darker.QueryHandler<PlantQuery, string>'),
    'ExecuteAsync': (['public override Task<string> ExecuteAsync(PlantQuery query,',
                      '    CancellationToken cancellationToken = default)', '{',
                      '    return Task.FromResult("x");', '}'],
                     'Paramore.Darker.QueryHandlerAsync<PlantQuery, string>'),
    'Fallback_Brighter': (['public override PlantCommand Fallback(PlantCommand command)', '{',
                           '    return base.Fallback(command);', '}'],
                          'Paramore.Brighter.RequestHandler<PlantCommand>'),
    'Fallback_Darker': (['public override string Fallback(PlantQuery query)', '{', '    return "x";', '}'],
                        'Paramore.Darker.QueryHandler<PlantQuery, string>'),
    'FallbackAsync_Brighter': (['public override Task<PlantCommand> FallbackAsync(PlantCommand command,',
                                '    CancellationToken cancellationToken = default)', '{',
                                '    return base.FallbackAsync(command, cancellationToken);', '}'],
                               'Paramore.Brighter.RequestHandlerAsync<PlantCommand>'),
    'FallbackAsync_Darker': (['public override Task<string> FallbackAsync(PlantQuery query,',
                              '    CancellationToken cancellationToken = default)', '{',
                              '    return Task.FromResult("x");', '}'],
                             'Paramore.Darker.QueryHandlerAsync<PlantQuery, string>'),
}
UNWRAPPED = {   # name: (body, a phrase handler_base's reason must contain)
    'Ellipsis': (['public override async Task<string> ExecuteAsync(...)', '{', '    return "x";', '}'],
                 'names no request type'),
    'TwoBases': (['public override PlantCommand Handle(PlantCommand command) { return command; }',
                  'public override string Execute(PlantQuery query) { return "x"; }'],
                 'two bases'),
}

MISFIT = (['public override Task<string> ExecuteAsync(PlantQuery query)', '{',
           '    return Task.FromResult("x");', '}'],
          'Paramore.Darker.QueryHandlerAsync<PlantQuery, string>')

failures = []
staged = {'wrapped': [], 'base-less': []}
for name, (body, want) in list(PLANTS.items()) + [('Misfit', MISFIT)]:
    block = bc.Block(f'planted/{name}.md', 1, 0, 0, USINGS + body)
    if block.shape != 'members':
        failures.append(f'{name}: shaped {block.shape}, not members'); continue
    text = bc.wrap(block)[0]
    holder = next((l for l in text.split('\n') if l.startswith(('public class Holder', 'public abstract class Holder'))), '')
    if holder != f'public abstract class Holder : {want}':
        failures.append(f'{name}: staged {holder!r}, wanted base {want}')
    staged['wrapped'].append((block, text))
    staged['base-less'].append((block, re.sub(r'^public abstract class Holder : .*$', 'public class Holder',
                                              text, flags=re.M)))
for name, (body, why) in UNWRAPPED.items():
    block = bc.Block(f'planted/{name}.md', 1, 0, 0, USINGS + body)
    base, reason = bc.handler_base(block)
    print(f'{name}\tunwrapped\t{reason}')
    if base or why not in (reason or ''):
        failures.append(f'{name}: handler_base gave {base!r}, {reason!r}')

verdicts = {}
for kind, items in staged.items():
    d = os.path.join(W, kind); shutil.rmtree(d, ignore_errors=True); os.makedirs(d)
    open(os.path.join(d, 'unit_plants.cs'), 'w').write(UNIT)
    with open(os.path.join(d, 'index.tsv'), 'w') as index:
        for block, text in items:
            open(os.path.join(d, block.ident + '.cs'), 'w').write(text)
            index.write(f'{block.ident}\t{block.rel}\t1\tmembers\t0\t0\t0\tunit_plants.cs\n')
    run = subprocess.run(['dotnet', bc.TOOL_DLL, d, bc.REFS_LIST], capture_output=True, text=True)
    if run.returncode != 0:
        sys.exit(f'the Roslyn tool failed on {d}:\n{run.stderr}')
    for line in run.stdout.splitlines():
        ident, verdict, count, codes = line.split('\t')
        verdicts[(kind, ident)] = (verdict, codes)

for block, _ in staged['wrapped']:
    if block.rel == 'planted/Misfit.md':
        got = verdicts[('wrapped', block.ident)]
        print(f'{block.ident}\twrapped {got[0]} {got[1]}\t(must stay FAILED, CS0115)')
        if got[0] != 'FAILED' or 'CS0115' not in got[1]:
            failures.append(f'{block.ident}: wrapped, {got}')
        continue
    got = verdicts[('wrapped', block.ident)]
    bare = verdicts[('base-less', block.ident)]
    print(f'{block.ident}\twrapped {got[0]} {got[1]}\tbase-less {bare[0]} {bare[1]}')
    if got[0] != 'BUILT':
        failures.append(f'{block.ident}: wrapped, {got}')
    if bare[0] != 'FAILED':
        failures.append(f'{block.ident}: base-less, {bare}')

for failure in failures:
    print('FAILED ' + failure, file=sys.stderr)
print(f'{len(PLANTS) + len(UNWRAPPED) + 1 - len(failures)} of {len(PLANTS) + len(UNWRAPPED) + 1} plants behave',
      file=sys.stderr)
sys.exit(1 if failures else 0)
