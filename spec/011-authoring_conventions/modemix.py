#!/usr/bin/env python3
"""Throwaway: estimate Diataxis mode-mixing per page from heading text.

Navigation sections (Further Reading, Related Documentation, See Also) are
excluded -- they appear on nearly every page and are not a content mode.
"""
import re, pathlib, collections

NAV = re.compile(r'^#{2,3}\s+(further reading|related documentation|see also|next steps|references?)\s*$')

PATTERNS = {
    'reference': r'\b(configuration|options?|parameters?|settings|api|properties|fields|matrix|supported)\b',
    'explanation': r'(\bwhat (is|are)\b|\bwhy\b|\bhow .* works?\b|\btrade-?offs?\b|\bvs\.?\b|\bversus\b|\bunderstanding\b|\bbackground\b|\bconcepts?\b|\bwhen to use\b)',
    'howto': r'(\bhow to\b|\bmigrat(e|ing|ion)\b|\bsetting up\b|\bset up\b|\bstep\b|\bimplement(ing)?\b|\badding\b|\benabling\b|\busing\b|\bcreating\b|\bwriting\b)',
    'guidance': r'(\bbest practices?\b|\brecommend|\bpitfalls?\b|\bgotchas?\b|\btroubleshoot|\bcommon (mistakes|problems|issues)\b|\btips\b)',
}

rows = []
no_h1, banner_collision = [], []
for p in sorted(pathlib.Path('contents').glob('*.md')):
    text = p.read_text(encoding='utf-8', errors='replace')
    lines = text.splitlines()

    # H1 present, and what immediately follows it?
    h1_idx = next((i for i, l in enumerate(lines) if l.startswith('# ')), None)
    if h1_idx is None:
        no_h1.append(p.name)
    else:
        after = [l for l in lines[h1_idx + 1:h1_idx + 4] if l.strip()]
        if after and after[0].startswith('>'):
            banner_collision.append(p.name)

    modes = collections.defaultdict(list)
    heads = 0
    for l in lines:
        if not re.match(r'^#{2,3} ', l):
            continue
        heads += 1
        low = l.strip().lower()
        if NAV.match(low):
            continue
        for m, pat in PATTERNS.items():
            if re.search(pat, low):
                modes[m].append(l.strip())
    rows.append((len(lines), len(modes), sorted(modes), heads, p.name))

rows.sort(key=lambda r: (-r[1], -r[0]))
print(f"{'lines':>6} {'modes':>5}  page")
print('-' * 74)
for ln, nm, ms, nh, name in rows:
    if nm >= 3:
        print(f"{ln:>6} {nm:>5}  {name:<40} {','.join(ms)}")

def n(pred):
    return len([r for r in rows if pred(r)])

print()
print(f"total pages                       : {len(rows)}")
print(f">=4 modes (any size)              : {n(lambda r: r[1] == 4)}")
print(f">=3 modes (any size)              : {n(lambda r: r[1] >= 3)}")
print(f">500 lines AND >=3 modes          : {n(lambda r: r[0] > 500 and r[1] >= 3)}")
print(f">500 lines AND <3 modes           : {n(lambda r: r[0] > 500 and r[1] < 3)}")
print(f"<=500 lines AND >=3 modes         : {n(lambda r: r[0] <= 500 and r[1] >= 3)}")
print(f">800 lines (any modes)            : {n(lambda r: r[0] > 800)}")
print()
print(f"pages with no H1                  : {len(no_h1)} {no_h1[:6]}")
print(f"pages already opening with a quote: {len(banner_collision)} {banner_collision[:8]}")
