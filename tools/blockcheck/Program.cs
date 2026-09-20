// The Roslyn half of the compile gate: give every staged C# block a verdict.
//
// ONE CSharpCompilation PER BLOCK. NEVER A BATCH.
// ----------------------------------------------
// This is the whole architecture, and it is not an implementation detail to be
// tidied into something faster. A batch manufactures clean verdicts for blocks
// that do not compile, and says nothing about having done so. Measured over
// this corpus, 2026-09-20, the same 985 staged blocks both ways:
//
//     one compilation per block   61 built
//     one compilation, all 985    64 built     <- four of them false
//
//     TutorialFirstCommand_2   FAILED  CS0246 alone,  BUILT in the batch
//     TutorialFirstCommand_3   FAILED  CS0246 alone,  BUILT in the batch
//     TutorialFirstMessage_3   FAILED  CS0246 alone,  BUILT in the batch
//     TutorialDurableOutbox_3  FAILED  CS0246 alone,  BUILT in the batch
//
// The mechanism is VISIBILITY, not diagnostics: a tutorial declares a type in
// one block and uses it in the next, and 11 of the 985 blocks carry their own
// `namespace` and are emitted verbatim. Put them in one compilation and block 2
// resolves a name that block 1 declared -- which is exactly the thing a reader
// copying block 2 alone cannot do.
//
// tools/blockcheck/plants holds the control, and Plant_Leak_B IS that control:
// it names a type declared by Plant_Leak_A in a namespace they share, and it
// must come back FAILED. Batch them and it comes back BUILT.
//
// Separate Compilation objects cannot share a symbol table OR a diagnostic bag,
// so the defect cannot recur here by construction rather than by care. That is
// spec 016 friction 54 and standing obligation 8; a proposal to "just compile
// them together" is this defect returning under a better name.
//
// SPEC 016'S DESIGN GIVES A DIFFERENT MECHANISM AND IT DID NOT REPRODUCE.
// Its probe reported that a parse failure suppresses semantic binding across a
// whole compilation -- 1,444 errors and not one semantic error over all 985.
// Re-run here with Roslyn 4.11 on net9.0, one compilation of all 985 reported
// 6,331 errors of which 5,466 were semantic, and a planted unparseable file
// alongside a planted CS0246 hid nothing at all. The design's conclusion holds
// and its stated cause does not; the numbers above are this repository's.
//
// Reference assemblies ARE shared, and that is a different thing: a
// MetadataReference is an immutable input, not a diagnostic sink. Loading 300
// of them once rather than 985 times is what makes the corpus run in seconds.
//
// Usage:
//     blockcheck <blocksDir> <refs.txt> [outFile]
//     blockcheck --identifiers <file>...        what a scaffold supplies
//     blockcheck --parse <blocksDir>            which blocks do not PARSE
//     blockcheck --explain <blocksDir> <refs.txt> <id>...   diagnostics in full
//
// `refs.txt` is written by refs/refs.csproj at build time and holds one
// absolute assembly path per line -- the 67 pinned packages and the framework
// targeting pack, exactly as MSBuild resolved them. Globbing a bin/ directory
// instead silently omits the targeting pack, and the corpus then reports
// CS0518 on 981 of 985 blocks: "predefined type System.Object is not defined".
//
// Rows go to stdout (or outFile), one per block, and nothing else does:
//
//     id<TAB>verdict<TAB>error count<TAB>distinct error codes
//
// The verdict is BUILT or FAILED. SKIPPED and NOT_COMPILABLE are decided
// before a block ever reaches this tool, by tools/blockcheck.py, which also
// owns the 0/1/2 exit contract of the gate as a whole.
//
// This tool's own exit code is 0 when it ran and 2 when it could not -- no
// blocks, no references, a directory that is not there. It is never 1: a
// failing block is data, and the corpus verdict belongs to the Python half.
// Exit 2 matters most where it is least visible. With an empty reference
// directory every block fails, which is a believable red in the direction that
// confirms the thesis, and it is NOT a measurement of the corpus.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal static class Program
{
    private const int ExitRan = 0;
    private const int ExitNothingChecked = 2;

    /// <summary>
    /// Every name a scaffold file supplies, read off the syntax tree.
    /// </summary>
    /// <remarks>
    /// AC8 asks a run to state what it gave a block. A regex over declarations
    /// would under-report -- and a listing that under-reports claims the block
    /// was helped less than it was, which is the one direction that matters.
    /// No references are needed: this is a syntax question, not a semantic one.
    ///
    /// Prints file TAB kind TAB name, and nothing else, so the caller can count
    /// lines.
    /// </remarks>
    private static int Identifiers(string[] files)
    {
        if (files.Length == 0)
        {
            Console.Error.WriteLine("usage: blockcheck --identifiers <file>...");
            return ExitNothingChecked;
        }
        int rows = 0;
        foreach (var file in files)
        {
            if (!File.Exists(file))
            {
                Console.Error.WriteLine($"no such file: {file}");
                return ExitNothingChecked;
            }
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file), Parse, file);
            var broken = tree.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (broken.Count > 0)
            {
                // A scaffold that does not parse cannot be listed and must not
                // be used: in a block's compilation it would suppress binding
                // and hand that block a silent pass.
                Console.Error.WriteLine(
                    $"{file} does not parse: {broken[0].Id} at " +
                    $"{broken[0].Location.GetLineSpan().StartLinePosition}");
                return ExitNothingChecked;
            }
            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                string kind = null, name = null;
                switch (node)
                {
                    case Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeDeclarationSyntax t:
                        kind = t.Kind().ToString().Replace("Declaration", string.Empty).ToLowerInvariant();
                        name = t.Identifier.ValueText; break;
                    case Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax m:
                        kind = "method"; name = m.Identifier.ValueText; break;
                    case Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax p:
                        kind = "property"; name = p.Identifier.ValueText; break;
                    case Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax v:
                        kind = "field"; name = v.Identifier.ValueText; break;
                    case Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax a:
                        kind = "parameter"; name = a.Identifier.ValueText; break;
                }
                if (kind == null) continue;
                Console.WriteLine($"{file}\t{kind}\t{name}");
                rows++;
            }
        }
        Console.Error.WriteLine($"{rows} identifiers from {files.Length} file(s)");
        return ExitRan;
    }

    /// <summary>
    /// Which staged blocks do not PARSE, asked of the parser and not of a list
    /// of error codes.
    /// </summary>
    /// <remarks>
    /// Spec 016 task 2.2 has to split the failures into the ones that never
    /// reached the binder and the ones that did, and its stated filter was
    /// "CS1002/CS1513/CS1519/CS8635 AND FRIENDS" -- an enumeration by
    /// guesswork, over a corpus where being wrong in the generous direction
    /// under-reports the class that matters. Roslyn already knows the answer:
    /// `SyntaxTree.GetDiagnostics()` is the parse, with no references, no
    /// binder and no compilation.
    ///
    /// The code filter is still worth running as the second method, and where
    /// the two disagree the disagreement is the finding.
    ///
    /// Prints id TAB PARSES|BROKEN TAB count TAB codes, and nothing else, so
    /// the caller can count lines. Reads the staged index for the same reason
    /// the compile path does: a glob would sweep in the scaffold units.
    /// </remarks>
    private static int ParseOnly(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("usage: blockcheck --parse <blocksDir>");
            return ExitNothingChecked;
        }
        var blocksDir = args[0];
        var indexPath = Path.Combine(blocksDir, "index.tsv");
        if (!File.Exists(indexPath))
        {
            Console.Error.WriteLine($"no index at {indexPath}: nothing was checked");
            return ExitNothingChecked;
        }
        var ids = new List<string>();
        foreach (var line in File.ReadAllLines(indexPath))
        {
            if (line.Length == 0 || line[0] == '#') continue;
            ids.Add(line.Split('\t')[0]);
        }
        if (ids.Count == 0)
        {
            Console.Error.WriteLine($"no blocks listed in {indexPath}: nothing was checked");
            return ExitNothingChecked;
        }
        int broken = 0;
        foreach (var id in ids)
        {
            var path = Path.Combine(blocksDir, id + ".cs");
            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"staged block missing: {path}");
                return ExitNothingChecked;
            }
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), Parse, path);
            var errors = tree.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errors.Count > 0) broken++;
            var codes = string.Join(",", errors.Select(e => e.Id).Distinct().OrderBy(c => c));
            Console.WriteLine(
                $"{id}\t{(errors.Count == 0 ? "PARSES" : "BROKEN")}\t{errors.Count}\t{codes}");
        }
        Console.Error.WriteLine($"{ids.Count} blocks, {broken} do not parse");
        return ExitRan;
    }

    // Latest, matching what a reader's own project would use; the pin that
    // decides the API surface is the package set in refs/refs.csproj.
    private static readonly CSharpParseOptions Parse =
        new CSharpParseOptions(LanguageVersion.Latest);

    private static readonly CSharpCompilationOptions Options =
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

    // A block written as a Program.cs -- top-level statements, then the types it
    // uses -- is only legal in an EXECUTABLE. Compile it as a library and every
    // such block earns CS8805, "Program using top-level statements must be an
    // executable": a verdict about this tool's output kind, not about the page.
    // Phase 2 met it the moment `toplevel` blocks stopped being wrapped.
    private static readonly CSharpCompilationOptions ExeOptions =
        new CSharpCompilationOptions(OutputKind.ConsoleApplication);

    /// <summary>
    /// Does this block's own tree carry top-level statements?
    /// </summary>
    /// <remarks>
    /// Asked of the syntax tree rather than taken from the staged index. The
    /// index's shape is Python's classification, and a verdict that depends on
    /// it would be trusting the claim instead of the code; `GlobalStatement` is
    /// what the parser actually found.
    /// </remarks>
    private static bool HasTopLevelStatements(SyntaxTree tree) =>
        tree.GetCompilationUnitRoot().Members
            .Any(m => m is Microsoft.CodeAnalysis.CSharp.Syntax.GlobalStatementSyntax);

    public static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--identifiers")
            return Identifiers(args.Skip(1).ToArray());
        if (args.Length > 0 && args[0] == "--parse")
            return ParseOnly(args.Skip(1).ToArray());

        // --explain reads the same staged corpus and the same references, and
        // prints the diagnostics in full rather than a row of codes. A code is
        // enough to count a failure and not enough to triage one: phase 2 had
        // to know whether `CS1729` meant a constructor the page invented or a
        // type its block never imported, and only the message says which.
        var explain = new HashSet<string>(StringComparer.Ordinal);
        if (args.Length > 0 && args[0] == "--explain")
        {
            if (args.Length < 4)
            {
                Console.Error.WriteLine(
                    "usage: blockcheck --explain <blocksDir> <refs.txt> <id>...");
                return ExitNothingChecked;
            }
            foreach (var id in args.Skip(3)) explain.Add(id);
            args = new[] { args[1], args[2] };
        }

        if (args.Length < 2 || args.Length > 3)
        {
            Console.Error.WriteLine("usage: blockcheck <blocksDir> <refs.txt> [outFile]");
            return ExitNothingChecked;
        }

        string blocksDir = args[0], refsList = args[1];
        if (!Directory.Exists(blocksDir))
        {
            Console.Error.WriteLine($"no blocks directory: {blocksDir}");
            return ExitNothingChecked;
        }
        if (!File.Exists(refsList))
        {
            Console.Error.WriteLine(
                $"no reference list at {refsList}: the reference project has not been restored " +
                "and built, so NOTHING WAS CHECKED");
            return ExitNothingChecked;
        }

        var references = new List<MetadataReference>();
        var missing = new List<string>();
        foreach (var path in File.ReadAllLines(refsList))
        {
            // The first line is refs.csproj's SHA256, written by refs.csproj so
            // that a STALE reference list cannot pass for a current one. The
            // comparison is the Python half's; here the line is simply not an
            // assembly, and treating it as one would trip the missing-reference
            // guard below with a message about the wrong thing.
            if (path.Length == 0 || path[0] == '#') continue;
            if (!File.Exists(path)) { missing.Add(path); continue; }
            try
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
            catch (Exception)
            {
                missing.Add(path);
            }
        }
        if (missing.Count > 0)
        {
            // Not a warning. A reference set that is partly there fails blocks
            // for want of a type the reader's own project would have had, and
            // it does it believably -- which is the shape of every plausible
            // zero this programme has met.
            Console.Error.WriteLine(
                $"{missing.Count} of {missing.Count + references.Count} reference assemblies are " +
                $"missing or unreadable, first: {missing[0]}");
            Console.Error.WriteLine("NOTHING WAS CHECKED");
            return ExitNothingChecked;
        }
        if (references.Count == 0)
        {
            Console.Error.WriteLine($"{refsList} lists no assemblies: NOTHING WAS CHECKED");
            return ExitNothingChecked;
        }

        // The staged index, not a glob. A glob would also sweep in the scaffold
        // units staged beside the blocks and compile them as if they were
        // documentation, which is the kind of mistake that inflates a corpus
        // count and passes.
        var indexPath = Path.Combine(blocksDir, "index.tsv");
        if (!File.Exists(indexPath))
        {
            Console.Error.WriteLine($"no index at {indexPath}: nothing was checked");
            return ExitNothingChecked;
        }
        var files = new List<(string Id, string Path, string Scaffold)>();
        foreach (var line in File.ReadAllLines(indexPath))
        {
            if (line.Length == 0 || line[0] == '#') continue;
            var f = line.Split('\t');
            if (f.Length < 8)
            {
                Console.Error.WriteLine($"{indexPath}: expected 8 fields, got {f.Length}");
                return ExitNothingChecked;
            }
            files.Add((f[0], Path.Combine(blocksDir, f[0] + ".cs"), f[7]));
        }
        if (files.Count == 0)
        {
            Console.Error.WriteLine($"no blocks listed in {indexPath}: nothing was checked");
            return ExitNothingChecked;
        }

        // Scaffold units are parsed ONCE, and a unit that does not parse stops
        // the run. A parse error in a shared tree suppresses semantic binding
        // across the compilation it is in -- friction 54's mechanism -- so a
        // broken scaffold would hand every page that uses it a silent CLEAN.
        var scaffoldTrees = new Dictionary<string, SyntaxTree>();
        foreach (var name in files.Select(f => f.Scaffold).Where(s => s != "-").Distinct())
        {
            var path = Path.Combine(blocksDir, name);
            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"staged scaffold missing: {path}");
                return ExitNothingChecked;
            }
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), Parse, path);
            var broken = tree.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (broken.Count > 0)
            {
                Console.Error.WriteLine(
                    $"scaffold {name} does not parse: {broken[0].Id} at " +
                    $"{broken[0].Location.GetLineSpan().StartLinePosition}");
                Console.Error.WriteLine("NOTHING WAS CHECKED");
                return ExitNothingChecked;
            }
            scaffoldTrees[name] = tree;
        }

        Console.Error.WriteLine($"{references.Count} reference assemblies");

        TextWriter rows = args.Length == 3
            ? new StreamWriter(args[2], append: false)
            : Console.Out;
        var clock = Stopwatch.StartNew();
        int built = 0;
        int scaffoldErrors = 0;
        try
        {
            foreach (var (id, path, scaffold) in files)
            {
                if (explain.Count > 0 && !explain.Contains(id)) continue;
                var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), Parse, path);
                var trees = scaffold == "-"
                    ? new[] { tree }
                    : new[] { tree, scaffoldTrees[scaffold] };

                // One BLOCK, one compilation, one diagnostic bag. A page's
                // scaffold rides in that block's own compilation and nowhere
                // else, so it can help this block and cannot silence another.
                var compilation = CSharpCompilation.Create(
                    assemblyName: "B_" + id,
                    syntaxTrees: trees,
                    references: references,
                    options: HasTopLevelStatements(tree) ? ExeOptions : Options);

                var all = compilation.GetDiagnostics()
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .ToList();

                // The verdict is about the BLOCK. An error reported against the
                // scaffold's tree is an instrument fault, not a documentation
                // defect, and counting it as one would fail every page that
                // shares that scaffold for a reason no reader could act on. It
                // is not swallowed: the count is printed at the end of the run.
                var errors = all.Where(d => d.Location.SourceTree == tree).ToList();
                scaffoldErrors += all.Count - errors.Count;
                if (errors.Count == 0) built++;

                if (explain.Count > 0)
                {
                    foreach (var d in errors)
                    {
                        var pos = d.Location.GetLineSpan().StartLinePosition;
                        rows.WriteLine($"{id}\t{d.Id}\t{pos.Line + 1}:{pos.Character + 1}\t" +
                                       d.GetMessage());
                    }
                    continue;
                }

                var codes = string.Join(",", errors.Select(e => e.Id).Distinct().OrderBy(c => c));
                rows.WriteLine($"{id}\t{(errors.Count == 0 ? "BUILT" : "FAILED")}\t{errors.Count}\t{codes}");
            }
        }
        finally
        {
            rows.Flush();
            if (rows != Console.Out) rows.Dispose();
        }
        clock.Stop();

        // The scope line, before the verdict line. `0 failing` out of 0 is not
        // the same claim as `0 failing` out of 985.
        Console.Error.WriteLine(
            $"{files.Count} blocks, {built} built, {files.Count - built} failing" +
            (scaffoldTrees.Count > 0 ? $", {scaffoldTrees.Count} scaffold unit(s)" : string.Empty));
        if (scaffoldErrors > 0)
            Console.Error.WriteLine(
                $"WARNING: {scaffoldErrors} error(s) reported against scaffold trees, " +
                "not counted against any block");
        Console.Error.WriteLine(
            $"{clock.Elapsed.TotalSeconds:F1}s total, " +
            $"{clock.Elapsed.TotalMilliseconds / files.Count:F0}ms per block");
        return ExitRan;
    }
}
