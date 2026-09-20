// Probe: one CSharpCompilation per block, all sharing one reference set.
// Separate Compilation objects cannot share a diagnostic bag, so the
// saturation that hid errors in the MSBuild runs cannot occur by construction.
using System; using System.Collections.Generic; using System.Diagnostics;
using System.IO; using System.Linq;
using Microsoft.CodeAnalysis; using Microsoft.CodeAnalysis.CSharp;

public static class Program {
  public static int Main(string[] argv) {
    var blocks = argv[0]; var refDir = argv[1];
    var refs = Directory.GetFiles(refDir, "*.dll")
        .Select(f => { try { return MetadataReference.CreateFromFile(f); } catch { return null; } })
        .Where(r => r != null).Cast<MetadataReference>().ToList();
    Console.WriteLine($"# {refs.Count} reference assemblies");
    var files = Directory.GetFiles(blocks, "*.cs").OrderBy(f => f).ToList();
    var sw = Stopwatch.StartNew(); int clean = 0;
    var opts = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
    using var w = new StreamWriter("verdicts.tsv");
    foreach (var f in files) {
      var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(f));
      var comp = CSharpCompilation.Create("B" + Path.GetFileNameWithoutExtension(f),
                                          new[] { tree }, refs, opts);
      var errs = comp.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
      if (errs.Count == 0) clean++;
      w.WriteLine($"{Path.GetFileNameWithoutExtension(f)}\t{(errs.Count == 0 ? "CLEAN" : "FAIL")}\t{errs.Count}\t{string.Join(",", errs.Select(e => e.Id).Distinct().Take(6))}");
    }
    sw.Stop();
    Console.WriteLine($"{files.Count} blocks, {clean} clean, {files.Count - clean} failing");
    Console.WriteLine($"{sw.Elapsed.TotalSeconds:F1}s total, {sw.Elapsed.TotalMilliseconds / files.Count:F0}ms per block");
    return 0;
  }
}
