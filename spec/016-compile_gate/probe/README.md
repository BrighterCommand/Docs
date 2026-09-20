# The 016 design probe

**Run 2026-09-20 against Docs `master` `feea788`.** Committed so that `design.md`'s figures can be
reproduced rather than believed. Nothing here is a deliverable — the tool this spec ships is
`tools/blockcheck`, and this is the experiment that decided its architecture.

`gen.py` hardcodes a path to `tools/`, as the rescued harness does. **That is acceptable in a probe
and is a defect in a tool**, and it is one of the things P0-1 fixes.

## What is here

| File | What |
|---|---|
| `gen.py` | emits all 985 C# blocks through `pagelint.Page`, classifying and wrapping each. **No scaffold, no prelude** — so every figure it produces is a floor |
| `probe.csproj` | the package union: `optioncheck`'s 62 pins plus five non-conflicting extras from the rescued harness. **67 packages, one project, restores in 1.16s** |
| `refs.csproj` | **the same 67 packages with no sources**, and `CopyLocalLockFileAssemblies=true`. This is what assembles the reference set — see the warning below |
| `roslyn/Program.cs` | 59 lines: one `CSharpCompilation` per block, all sharing one reference set |
| `verdicts.tsv` | **985 rows** — the output. `id · verdict · error count · first six distinct error codes` |
| `solo-results.txt` | the 59-block MSBuild-one-at-a-time run, kept because it is the **control** that the Roslyn verdicts agree with |

## Reproducing it

> **The first version of this section documented a recipe nobody had run, and it does not work.**
> It said to build `probe.csproj` and take the references from its output. That project **compiles
> the blocks**, so its build fails — and a failed build leaves `bin/` with **0 DLLs**, after which
> the probe runs against no references and calls every block broken. The run that produced
> `verdicts.tsv` used a **separate references-only project** that was neither committed nor
> mentioned. It is `refs.csproj`, it is committed now, and **this recipe has been executed from a
> clean directory**.

```bash
python3 gen.py <outdir>                                  # 985 .cs files
dotnet build refs.csproj                                 # exit 0 — assembles the reference set
cp bin/Debug/net8.0/*.dll <refdir>/
cp $(dirname $(which dotnet))/packs/Microsoft.NETCore.App.Ref/8.0.0/ref/net8.0/*.dll <refdir>/
cd roslyn && dotnet build && dotnet run --no-build -- <outdir> <refdir>
```

**Verified end to end 2026-09-20 from an empty directory**, using only the files committed here:

```text
1. emitted: 985 blocks
2. refs build exit (bare): 0
3. reference assemblies: 391
   985 blocks, 60 clean, 925 failing
```

**Do not read the wall-clock as a constant.** Four runs over identical inputs gave **6.5s, 22.9s,
39.5s and 26.2s** — 7 to 40ms a block, varying with machine load.

## The headline, and the control that licenses it

```text
985 blocks, 60 clean, 925 failing       6.5s total, 7ms per block
```

**Two methods agree.** The same 59 sampled blocks were compiled one at a time by MSBuild
(`solo-results.txt`, 6 clean / 53 failing) and by the Roslyn harness: **59 of 59 verdicts identical,
0 disagreements.** Different code path, same answer.

**Two planted controls, both outside the corpus** — `NoSuchTypeXyz123 f;` must come back `FAIL`
(it does, `CS0246`) and `public class C { public int F; }` must come back `CLEAN` (it does). Without
the second, a harness that had silently stopped compiling would report every block `FAIL` and read
as thorough.

## The finding that decided the architecture

Batching subjects into one compilation makes a clean verdict meaningless — `design.md`
§ *The finding nobody was looking for* has the measurements. The short form:

```text
two blocks that fail alone, 20 and 8 errors
  batch of 2  ->  20 and 8     reported
  batch of 5  ->   0 and 0     both silent, three unrelated files added
```

One `Compilation` per block is not an optimisation. It is the only arrangement in which a verdict
means anything.
