"""AC9's instrument: a handler attribute whose sync/async form does not match the
method it decorates -- a sync attribute on an async handler, or the reverse.
Brighter rejects both when it builds the pipeline; the compiler accepts both.

The paired attributes are read from Brighter 10.7.0 (`git grep 'class \\w+Attribute'`):
every name below exists with and without the Async suffix.

Reads blocks through pagelint.Page, never a fence grep (016 friction 53).
    python3 attr_mismatch.py            the corpus: page:line per hit, exit 1 if any
    python3 attr_mismatch.py --plant    the red-proof: two plants that must hit,
                                        one that must not; exit 0 only if all three do
"""
import sys, re
PAIRED = ('BulkDepositCallSite DeferMessageOnError DepositCallSite DontAckOnError FallbackPolicy '
          'FeatureSwitch Monitor RejectMessageOnError RequestLogging UseInbox UsePolicy '
          'UseResiliencePipeline ValidateRequest').split()
ATTR = re.compile(r'^\s*\[\s*(' + '|'.join(PAIRED) + r')(Async)?\s*\(')

def scan(lines):
    """lines: [(lineno, text)]. Yields (lineno, attribute, method-kind)."""
    for i, (n, text) in enumerate(lines):
        m = ATTR.search(text)
        if not m: continue
        j = i + 1
        while j < len(lines) and (lines[j][1].strip().startswith('[') or not lines[j][1].strip()): j += 1
        if j == len(lines): continue
        target = lines[j][1]
        is_async = bool(re.search(r'\bHandleAsync\b', target))
        is_sync = bool(re.search(r'\bHandle\s*\(', target)) and not is_async
        if (m.group(2) is None and is_async) or (m.group(2) and is_sync):
            yield n, m.group(1) + (m.group(2) or ''), 'async' if is_async else 'sync'

def plant():
    sync_on_async = [(1, '[UsePolicy("retry", step: 1)]'),
                     (2, 'public override async Task<X> HandleAsync(X c, CancellationToken t)')]
    async_on_sync = [(1, '[RequestLoggingAsync(0, HandlerTiming.Before)]'),
                     (2, 'public override X Handle(X c)')]
    matched = [(1, '[UsePolicy("retry", step: 1)]'), (2, '[RequestLogging(0, HandlerTiming.Before)]'),
               (3, 'public override X Handle(X c)')]
    ok = (len(list(scan(sync_on_async))) == 1 and len(list(scan(async_on_sync))) == 1
          and len(list(scan(matched))) == 0)
    print('plants: sync-on-async hit, async-on-sync hit, matched pair silent:', 'OK' if ok else 'FAILED')
    return 0 if ok else 1

if __name__ == '__main__':
    if sys.argv[1:] == ['--plant']: sys.exit(plant())
    sys.path.insert(0, 'tools'); import pagelint
    hits = []
    for rel, pg in sorted(pagelint.load_pages().items()):
        for b in pg.blocks:
            if (b['info'] or '').strip().lower() not in ('csharp', 'c#', 'cs'): continue
            hits += [f'{rel}:{n}\t{a} on a {k} handler' for n, a, k in scan(b['body'])]
    print('\n'.join(hits)) if hits else None
    print(f'{len(hits)} attribute(s) on a handler of the other kind', file=sys.stderr)
    sys.exit(1 if hits else 0)
