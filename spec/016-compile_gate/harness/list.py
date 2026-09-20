import sys
sys.path.insert(0, '/Users/ian_hammond_cooper/CSharpProjects/github/BrighterCommand/Docs/tools')
import pagelint

for page in sys.argv[1:]:
    lines = open(page, encoding='utf-8').read().split('\n')
    p = pagelint.Page(page, page, lines)
    blocks = [b for b in p.blocks if (b['info'] or '').strip().lower() in ('csharp', 'c#')]
    print(page)
    for i, b in enumerate(blocks, 1):
        print(f'  {i}: lines {b["start"]}-{b["end"]}')
