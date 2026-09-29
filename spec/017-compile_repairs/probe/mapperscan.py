import re,glob,sys
# Spec 017 task 6.5. The recurrence scan for ledger #83 (and #16, carried into it): a class that
# implements `IAmAMessageMapper<T>` or `IAmAMessageMapperAsync<T>` in a C# block but leaves out a
# member of the interface with no `// ...` in its block to say so (`CS0535`). The members are
# `MapToMessage`, `MapToRequest` and `Context` (sync), and their `Async` forms (async). The marker
# is read per block, as `pagelint` reads it; a block `blockcheck` skips (a labelled V9 form) is not
# read at all, since its members are V9's.
# Run from the Docs root; `--root DIR` reads pages under DIR instead.
MEMBERS={'IAmAMessageMapper':('MapToMessage','MapToRequest','Context'),
         'IAmAMessageMapperAsync':('MapToMessageAsync','MapToRequestAsync','Context')}
HEAD=re.compile(r'\bclass\s+\w+(?:<[^>{]*>)?(?:\([^)]*\))?\s*:[^{]*?\b(IAmAMessageMapper(?:Async)?)\s*<')
root=sys.argv[sys.argv.index('--root')+1] if '--root' in sys.argv else '.'
n=0
for f in sorted(glob.glob(root+'/contents/*.md')):
    t=open(f).read(); rel=f[len(root)+1:]
    for fm in re.finditer(r'^``` ?(?:csharp|cs|c#)[^\n]*\n(.*?)^```',t,re.S|re.M):
        b=fm.group(1); base=t[:fm.start()].count('\n')+2
        if re.search(r'<!-- blockcheck: skip[^\n]*-->\s*$',t[:fm.start()]): continue
        if re.search(r'//\s*\.\.\.|/\*\s*\.\.\.\s*\*/',b): continue
        for m in HEAD.finditer(b):
            o=b.find('{',m.end())
            if o<0: continue
            depth=0; end=None
            for i in range(o,len(b)):
                if b[i]=='{': depth+=1
                elif b[i]=='}':
                    depth-=1
                    if depth==0: end=i; break
            body=b[o:end if end is not None else len(b)]
            missing=[x for x in MEMBERS[m.group(1)] if not re.search(r'\b'+x+r'\b',body)]
            if missing:
                print(f"{rel}:{base+b[:m.start()].count(chr(10))}\t{m.group(1)}\tmissing {', '.join(missing)}"); n+=1
print(n,file=sys.stderr)
