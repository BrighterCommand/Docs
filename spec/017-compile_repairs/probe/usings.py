import sys, collections, re, subprocess, shutil, csv
S=sys.argv[1]; DLL='tools/blockcheck/bin/Release/net9.0/blockcheck.dll'; REFS='tools/blockcheck/refs/bin/Release/net9.0/refs.txt'
ns=collections.defaultdict(set)
for l in open(f'{S}/types.tsv'):
    k,n,s=l.rstrip('\n').split('\t')
    if s and k=='type': ns[n].add(s)
def rank(s): return (0 if s.startswith('Paramore') else 1 if s.startswith('System') else 2 if s.startswith('Microsoft') else 3, len(s), s)
imp=[l.split('\t')[2] for l in open(f'{S}/classes.tsv') if l.startswith('import\t')]
shutil.rmtree(f'{S}/stage3', ignore_errors=True); shutil.copytree(f'{S}/stage', f'{S}/stage3')
added=collections.defaultdict(set); name_re=re.compile(r"name '([^']+)'")
for rnd in range(3):
    ex=subprocess.run(['dotnet',DLL,'--explain',f'{S}/stage3',REFS]+imp,capture_output=True,text=True).stdout
    miss=collections.defaultdict(set)
    for l in ex.splitlines():
        p=l.split('\t')
        if len(p)>=4 and p[1] in('CS0246','CS0103'):
            m=name_re.search(p[3]); 
            if m: miss[p[0]].add(m.group(1).split('<')[0])
    changed=0
    for bid in imp:
        new={min(ns[n],key=rank) for n in miss[bid] if n in ns}-added[bid]
        if new:
            added[bid]|=new; changed+=1
            body=open(f'{S}/stage/{bid}.cs').read()
            open(f'{S}/stage3/{bid}.cs','w').write(''.join(f'using {u};\n' for u in sorted(added[bid]))+body)
    print('round',rnd,'blocks changed',changed)
    if not changed: break
subprocess.run(['dotnet',DLL,f'{S}/stage3',REFS,f'{S}/r3.tsv'],capture_output=True)
st={r[0]:r[1] for r in csv.reader(open(f'{S}/r3.tsv'),delimiter='\t')}
b=sum(1 for v in st.values() if v=='BUILT'); ib=sum(1 for x in imp if st.get(x)=='BUILT')
print('BUILT overall',b,'; import blocks now BUILT',ib,'of',len(imp))
