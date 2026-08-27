// Spec 012 phase 1 — the three probes, kept and re-runnable.
//
//   dotnet run --project spec/012-configuration_reference/probes            # all three
//   dotnet run --project spec/012-configuration_reference/probes -- default
//   dotnet run --project spec/012-configuration_reference/probes -- packages
//   dotnet run --project spec/012-configuration_reference/probes -- synthesis
//   dotnet run --project spec/012-configuration_reference/probes -- counts     # TSV, task 1.5's oracle
//
// Exit code is the family contract the tools in `tools/` already run under:
// 0 clean, 1 a real finding. A probe that cannot reach its subject exits 2.

using Paramore.Docs.Probes;

var which = args.Length > 0 ? args[0].ToLowerInvariant() : "all";

var codes = new List<int>();

if (which is "all" or "default")
    codes.Add(DefaultProbe.Run());

if (which is "all" or "packages")
    codes.Add(PackageLoadProbe.Run());

if (which is "all" or "synthesis")
    codes.Add(SynthesisProbe.Run());

// Not part of `all`: it prints TSV for `survey.py` to be diffed against, and
// mixing that into the three probes' prose would make both harder to read.
if (which is "counts")
    codes.Add(CountsProbe.Run());

if (codes.Count == 0)
{
    Console.Error.WriteLine($"unknown probe '{which}' — expected default, packages, synthesis or all");
    return 2;
}

return codes.Max();
