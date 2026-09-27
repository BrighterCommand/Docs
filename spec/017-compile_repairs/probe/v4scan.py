import re,glob,sys
# Spec 017 task 3.3. A page that recommends a Brighter `.V4` AWS package and leaves its reader with
# the V3 namespace: every Brighter V4 package at 10.7.0 declares its types in `<V3 namespace>.V4`
# (`Locking.DynamoDb` -> `Locking.DynamoDB.V4`). A hit is a V3 `using` on such a page whose V4
# namespace the page never names, a V3 `using` in the same block as a `.V4` one, or the claim that
# the namespaces stay the same. Run from the Docs root; `--root DIR` reads pages under DIR instead.
V3={'Paramore.Brighter.MessagingGateway.AWSSQS':'Paramore.Brighter.MessagingGateway.AWSSQS.V4',
    'Paramore.Brighter.Outbox.DynamoDB':'Paramore.Brighter.Outbox.DynamoDB.V4',
    'Paramore.Brighter.Inbox.DynamoDB':'Paramore.Brighter.Inbox.DynamoDB.V4',
    'Paramore.Brighter.DynamoDb':'Paramore.Brighter.DynamoDb.V4',
    'Paramore.Brighter.Locking.DynamoDb':'Paramore.Brighter.Locking.DynamoDB.V4',
    'Paramore.Brighter.Transformers.AWS':'Paramore.Brighter.Transformers.AWS.V4',
    'Paramore.Brighter.MessageScheduler.AWS':'Paramore.Brighter.MessageScheduler.AWS.V4'}
root=sys.argv[sys.argv.index('--root')+1] if '--root' in sys.argv else '.'
n=0
for f in sorted(glob.glob(root+'/contents/*.md')):
    t=open(f).read(); rel=f[len(root)+1:]
    for m in re.finditer(r'(?i)namespace structure remains the same|Same namespaces',t):
        print(f"{rel}:{t[:m.start()].count(chr(10))+1}\tclaims the namespaces are unchanged"); n+=1
    if not re.search(r'\.V4\b',t): continue
    for fm in re.finditer(r'^```[^\n]*\n(.*?)^```',t,re.S|re.M):
        b=fm.group(1); base=t[:fm.start()].count('\n')+2
        mixed=bool(re.search(r'^using Paramore\.Brighter\.[\w.]+\.V4;',b,re.M))
        for um in re.finditer(r'^using (Paramore\.Brighter\.[\w.]+);',b,re.M):
            ns=um.group(1)
            if ns not in V3: continue
            line=base+b[:um.start()].count('\n')
            if mixed:
                print(f"{rel}:{line}\t{ns} beside a .V4 using"); n+=1
            elif V3[ns] not in t:
                print(f"{rel}:{line}\t{ns}, and the page never names {V3[ns]}"); n+=1
print(n,file=sys.stderr)
