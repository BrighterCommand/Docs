"""The 75 pages 017's tranches held: every `*.md` cell in its phase 2-5 tables."""
import re, sys
text = open(sys.argv[1]).read()
start = text.index('### Phase 2 — tranche 1a'); end = text.index('## Blocks that stay FAILED')
pages = sorted(set(re.findall(r'^\| [^|]+ \| `([A-Za-z0-9]+\.md)`', text[start:end], re.M)))
print('\n'.join(pages)); print(f'{len(pages)} tranche pages', file=sys.stderr)
