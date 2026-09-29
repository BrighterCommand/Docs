import re,glob,sys
# Spec 017 task 6.5. The recurrence scan for two async defects no one-line grep can see, because
# the method's signature and the offending line are on different lines of a C# block:
#   CS1997  a value returned from a method declared `async Task` (ledger #24)
#   CS4032  `await` in a method not marked `async` (ledger #61)
# A method is its signature line plus the brace-matched body after it. Awaits and returns inside a
# lambda in that body are the lambda's, not the method's, so a hit is printed for reading, not
# counted as a defect unread. Run from the Docs root; `--root DIR` reads pages under DIR instead.
MODS=r'(?:(?:public|private|protected|internal|static|override|virtual|sealed|new|partial)\s+)*'
SIG=re.compile(r'^[ \t]*'+MODS+r'(async\s+)?'+MODS+r'([\w.]+(?:<[^()=;]*>)?\??)\s+(\w+)\s*\(',re.M)
KEYWORDS={'if','for','foreach','while','switch','using','catch','lock','return','new','await','else','nameof','typeof','class','record','struct','interface'}
root=sys.argv[sys.argv.index('--root')+1] if '--root' in sys.argv else '.'
n=0
for f in sorted(glob.glob(root+'/contents/*.md')):
    t=open(f).read(); rel=f[len(root)+1:]
    for fm in re.finditer(r'^``` ?(?:csharp|cs|c#)[^\n]*\n(.*?)^```',t,re.S|re.M):
        b=fm.group(1); base=t[:fm.start()].count('\n')+2
        for m in SIG.finditer(b):
            is_async=bool(m.group(1)); ret=m.group(2); name=m.group(3)
            if name in KEYWORDS or ret in KEYWORDS: continue
            depth=1; c=m.end()                              # the parameter list's closing paren
            while c<len(b) and depth:
                depth+={'(':1,')':-1}.get(b[c],0); c+=1
            tail=re.match(r'\s*(?:where [^{;]*)?\{',b[c:])  # a block body, not `;`, `=>` or a call
            if not tail: continue
            o=c+tail.end()-1
            depth=0; end=None
            for i in range(o,len(b)):
                if b[i]=='{': depth+=1
                elif b[i]=='}':
                    depth-=1
                    if depth==0: end=i; break
            if end is None: continue
            body=b[o+1:end]; line=base+b[:m.start()].count('\n')
            if is_async and ret in('Task','ValueTask','void'):
                for r in re.finditer(r'\breturn\s+[^;\s]',body):
                    print(f"{rel}:{line+b[m.start():o+1+r.start()].count(chr(10))}\tCS1997\t{name}: a value returned from async {ret}"); n+=1
            if not is_async:
                for a in re.finditer(r'\bawait\b',body):
                    print(f"{rel}:{line+b[m.start():o+1+a.start()].count(chr(10))}\tCS4032\t{name}: await in a method not marked async"); n+=1
print(n,file=sys.stderr)
