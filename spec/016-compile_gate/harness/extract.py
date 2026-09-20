"""Extract one C# fenced block from a page into the scratch project, verbatim.

The point is that what gets compiled IS what the page says: the block body is
copied byte for byte, and only a wrapper is added around it. Usage:

    python3 extract.py <page> <1-based csharp block index> <out.cs> <wrapper> [prelude]

wrapper is one of:
  raw      - the block is already a compilation unit (usings + types)
  members  - wrap in `public static class BlockN { ... }`, usings hoisted
  stmts    - wrap in a method body, usings hoisted
  prelude  - usings hoisted, then the given prelude file, then the block, then
             a closing `}` per open brace the prelude left
"""
import re
import sys
sys.path.insert(0, '/Users/ian_hammond_cooper/CSharpProjects/github/BrighterCommand/Docs/tools')
import pagelint

page, index, out = sys.argv[1], int(sys.argv[2]), sys.argv[3]
wrapper = sys.argv[4] if len(sys.argv) > 4 else 'raw'
prelude = sys.argv[5] if len(sys.argv) > 5 else None

lines = open(page, encoding='utf-8').read().split('\n')
p = pagelint.Page(page, page, lines)
blocks = [b for b in p.blocks if (b['info'] or '').strip().lower() in ('csharp', 'c#')]
block = blocks[index - 1]
body = [text for _, text in block['body']]
print(f'{page} block {index} of {len(blocks)}: lines {block["start"]}-{block["end"]}',
      file=sys.stderr)

usings = [l for l in body if l.strip().startswith('using ') and l.strip().endswith(';')
          and '=' not in l and '(' not in l]
rest = [l for l in body if l not in usings] if wrapper != 'raw' else body

name = 'Block_' + re.sub(r'\W', '_', page.split('/')[-1]) + f'_{index}'
with open(out, 'w', encoding='utf-8') as fh:
    if wrapper == 'raw':
        fh.write('\n'.join(body) + '\n')
    else:
        fh.write('\n'.join(usings) + '\n')
        fh.write('using static PageContext;   // page-context helpers the page names but does not define\n\n')
        if wrapper == 'prelude':
            text = open(prelude, encoding='utf-8').read()
            fh.write(text.replace('__NAME__', name).replace('{@', '{'))
            fh.write('\n'.join(rest) + '\n')
            fh.write('}\n' * text.count('{@'))   # one `{@` per brace to close
        else:
            fh.write(f'public class {name}\n{{\n')
            if wrapper == 'stmts':
                fh.write('    public async System.Threading.Tasks.Task Run('
                         'Microsoft.Extensions.DependencyInjection.IServiceCollection services,\n'
                         '        Microsoft.Extensions.DependencyInjection.IServiceCollection serviceCollection)\n    {\n')
            fh.write('\n'.join(rest) + '\n')
            if wrapper == 'stmts':
                fh.write('    }\n')
            fh.write('}\n')
