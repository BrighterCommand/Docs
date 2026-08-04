#!/usr/bin/env python3
"""Throwaway: propose a Diataxis page type for every page, for human review.

Task 3.1 of spec 011. Writes pagetypes.tsv, one row per page under contents/:

    path <TAB> proposed <TAB> confidence <TAB> signal <TAB> verdict

The verdict column ships blank on purpose. apply_banners.py (Task 3.3) reads
*verdict*, never *proposed*, and hard-stops without writing a single page if any
row is still blank. A wrong page type is worse than a slow one: it tells the
reader the page is something it is not.

Signals, in the order design section 1 sets out. The first to fire wins, except
that conflicts and high mode counts downgrade confidence rather than changing
the proposal:

  1. Pages design section 1 names as needing argument. These already have an
     argued answer, so the answer is proposed -- but at low confidence, because
     "the design suggested it" is not the same as "a human agreed".
  2. Title shape. "How X Works" is Explanation, not How-to, and has to be
     tested before the verb rule or it reads as an imperative.
  3. Title verb: a gerund or imperative indicates How-to.
  4. SUMMARY.md section skew.
  5. modemix.py mode score: a page scoring exactly one mode takes that mode.
  6. Nothing fired -> review queue, with no proposal. Deliberately not a
     default.

Rows sort lowest confidence first, so the pages that need thought come before
the ones that need a glance.

Usage:  python3 spec/011-authoring_conventions/proposetypes.py
"""
import collections
import os
import pathlib
import re

ROOT = pathlib.Path(__file__).resolve().parents[2]
OUT = pathlib.Path(__file__).resolve().parent / 'pagetypes.tsv'

# Copied verbatim from modemix.py so the scores here match the ones quoted in
# requirements. modemix.py is the record of the original analysis and is not
# edited to be importable.
NAV = re.compile(
    r'^#{2,3}\s+(further reading|related documentation|see also|next steps|references?)\s*$')
PATTERNS = {
    'reference': r'\b(configuration|options?|parameters?|settings|api|properties|fields|matrix|supported)\b',
    'explanation': r'(\bwhat (is|are)\b|\bwhy\b|\bhow .* works?\b|\btrade-?offs?\b|\bvs\.?\b|\bversus\b|\bunderstanding\b|\bbackground\b|\bconcepts?\b|\bwhen to use\b)',
    'howto': r'(\bhow to\b|\bmigrat(e|ing|ion)\b|\bsetting up\b|\bset up\b|\bstep\b|\bimplement(ing)?\b|\badding\b|\benabling\b|\busing\b|\bcreating\b|\bwriting\b)',
    'guidance': r'(\bbest practices?\b|\brecommend|\bpitfalls?\b|\bgotchas?\b|\btroubleshoot|\bcommon (mistakes|problems|issues)\b|\btips\b)',
}

# Design section 1, "Pages expected to need argument". The type is the one the
# design argues for; Reference is its catch-all for material you consult rather
# than read through. ShowMeTheCode has no argued answer -- the design says
# Reference fits poorly but it is not a tutorial -- so it goes to the queue.
FLAGGED = {
    'FAQ.md': ('Reference', 'design section 1: outside Diataxis, consulted not read'),
    'Glossary.md': ('Reference', 'design section 1: outside Diataxis, consulted not read'),
    'V10MigrationGuide.md': ('Reference', 'design section 1: consulted, so Reference not How-to'),
    'WhyBrighter.md': ('Explanation', 'design section 1'),
    'ShowMeTheCode.md': ('', 'design section 1: a showcase; Reference fits poorly but it is not a tutorial'),
}

# SUMMARY.md section -> the mode it skews to. Sections whose members genuinely
# split across modes are absent on purpose, so their pages fall through to the
# mode score or the queue rather than inheriting a wrong default.
SECTION_SKEW = {
    'Guaranteed At Least Once': 'Reference',
    'Outbox and Inbox': 'Reference',
    'Scheduler': 'Reference',
    'Reference': 'Reference',
    'FAQ': 'Reference',
    'Event Driven Architectures': 'Explanation',
    'Under the Hood': 'Explanation',
    'CQRS Patterns': 'Explanation',
    'Command, Processors and Dispatchers': 'Explanation',
    'Task Queues': 'Explanation',
}

HOW_IT_WORKS = re.compile(r'^how\b.*\bworks?\b', re.I)
HOWTO_VERB = re.compile(
    r'^(how to|implementing|implement|using|use|building|build|migrating|migrate'
    r'|configuring|configure|upgrading|upgrade|adding|add|enabling|enable'
    r'|creating|create|writing|write|dispatching|returning|passing|supporting'
    r'|setting up|testing|test)\b', re.I)


def summary_sections():
    """Repo-relative page path -> the SUMMARY.md section heading above it."""
    out = {}
    section = None
    for line in (ROOT / 'SUMMARY.md').read_text(encoding='utf-8').splitlines():
        heading = re.match(r'^\s*#{2,3}\s+(.*?)\s*$', line)
        if heading:
            section = heading.group(1)
            continue
        for target in re.findall(r'\]\(([^)]+)\)', line):
            target = target.split('#')[0].lstrip('/')
            if target.endswith('.md'):
                out[os.path.basename(target)] = section
    return out


def mode_score(lines):
    modes = collections.defaultdict(int)
    for line in lines:
        if not re.match(r'^#{2,3} ', line):
            continue
        low = line.strip().lower()
        if NAV.match(low):
            continue
        for mode, pattern in PATTERNS.items():
            if re.search(pattern, low):
                modes[mode] += 1
    return sorted(modes)


MODE_TO_TYPE = {'reference': 'Reference', 'explanation': 'Explanation',
                'howto': 'How-to'}


def propose(name, h1, section, modes):
    """Return (type, confidence, signal). Empty type means the review queue."""
    if name in FLAGGED:
        proposed, why = FLAGGED[name]
        return proposed, 'low', why

    if h1 and HOW_IT_WORKS.match(h1):
        return 'Explanation', 'high', f'title "{h1}" is How-X-Works, not a how-to'

    if h1 and HOWTO_VERB.match(h1):
        return 'How-to', 'high', f'title "{h1}" opens with a gerund or imperative'

    if section in SECTION_SKEW:
        return SECTION_SKEW[section], 'medium', f'SUMMARY.md section "{section}"'

    if len(modes) == 1 and modes[0] in MODE_TO_TYPE:
        return MODE_TO_TYPE[modes[0]], 'medium', f'single mode scored: {modes[0]}'

    return '', 'review', (
        f'no signal fired (section "{section}", modes: {",".join(modes) or "none"})')


def main():
    sections = summary_sections()
    rows = []
    for path in sorted((ROOT / 'contents').glob('*.md')):
        lines = path.read_text(encoding='utf-8', errors='replace').splitlines()
        h1 = next((l[2:].strip() for l in lines if l.startswith('# ')), '')
        section = sections.get(path.name, '(not in SUMMARY.md)')
        modes = mode_score(lines)
        proposed, confidence, signal = propose(path.name, h1, section, modes)

        # A page scoring three or more modes is one the analysis could not
        # characterise, and design section 1 says that difficulty is itself the
        # worklist signal. Keep the proposal, drop the confidence, and say why
        # -- these are the rows Task 7.1 wants notes from.
        if len(modes) >= 3 and confidence == 'high':
            confidence = 'medium'
            signal += f'; but scores {len(modes)} modes ({",".join(modes)})'
        elif len(modes) >= 3 and confidence == 'medium':
            confidence = 'low'
            signal += f'; but scores {len(modes)} modes ({",".join(modes)})'

        rows.append({
            'path': f'contents/{path.name}',
            'proposed': proposed,
            'confidence': confidence,
            'signal': signal,
            'lines': len(lines),
            'modes': len(modes),
        })

    order = {'review': 0, 'low': 1, 'medium': 2, 'high': 3}
    rows.sort(key=lambda r: (order[r['confidence']], r['path']))

    with OUT.open('w', encoding='utf-8') as fh:
        fh.write('path\tproposed\tconfidence\tsignal\tverdict\n')
        for r in rows:
            fh.write(f"{r['path']}\t{r['proposed']}\t{r['confidence']}\t"
                     f"{r['signal']}\t\n")

    counts = collections.Counter(r['confidence'] for r in rows)
    types = collections.Counter(r['proposed'] or '(queue)' for r in rows)
    print(f'{len(rows)} rows -> {OUT.relative_to(ROOT)}')
    print('confidence:', dict(counts))
    print('proposed  :', dict(types))
    print('\nneeding real thought (review + low), in file order:')
    for r in rows:
        if r['confidence'] in ('review', 'low'):
            print(f"  {r['path']:<48} {r['proposed'] or '-':<12} "
                  f"{r['lines']:>5}L {r['modes']}m  {r['signal'][:60]}")


if __name__ == '__main__':
    main()
