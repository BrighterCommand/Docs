import re,glob,sys
# Spec 017 task 3.2. Every `new …Publication` in a fenced block whose initializer sets no
# RequestType, which FindPublicationByPublicationTopicOrRequestType cannot find when a reader
# posts (ConfigurationException, "No producer found for request type"). Generic publications
# (`KafkaPublication<T>`) set it themselves and are skipped. Run from the Docs root.
pat=re.compile(r'new\s+(\w*Publication)(<[^>]+>)?\s*(\(|\{)')
def span(t,i):
    # consume balanced () then optional {}
    j=i; out=''
    for opener,closer in (('(',')'),('{','}')):
        while j<len(t) and t[j] in ' \t\n': j+=1
        if j<len(t) and t[j]==opener:
            d=0; k=j
            while k<len(t):
                if t[k]==opener: d+=1
                elif t[k]==closer:
                    d-=1
                    if d==0: break
                k+=1
            out+=t[j:k+1]; j=k+1
    return out
n=0
for f in sorted(glob.glob('contents/*.md')):
    t=open(f).read()
    # only inside csharp fences
    for fm in re.finditer(r'^\s*```[^\n]*\n(.*?)^\s*```',t,re.S|re.M):
        b=fm.group(1); base=t[:fm.start()].count('\n')+2
        for m in pat.finditer(b):
            if m.group(2): continue
            body=span(b,m.start(3))
            if 'RequestType' in body: continue
            line=base+b[:m.start()].count('\n')
            n+=1; print(f'{f}:{line}\t{m.group(1)}\t'+' '.join(body.split())[:110])
print(n,file=sys.stderr)
