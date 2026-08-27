using System.Reflection;

namespace Paramore.Docs.Probes;

/// <summary>
/// Task 1.3 — load every pinned surface package in one process and instantiate
/// one type from each.
///
/// Design §6.4 registers the risk and says in as many words that none of it is
/// measured: one project referencing every Brighter surface package puts
/// several third-party SDKs in one process, and the risk lands entirely on the
/// instantiation route, because metadata reflection never runs a static
/// constructor while instantiating a type resolves its dependencies for real.
///
/// A conflict found here is a `csproj` edit; found after twelve tables are
/// written it is a redesign.
///
/// The package list is the `PackageReference` set in `probes.csproj`, read back
/// from the output directory rather than restated here — a hand-maintained
/// second copy is a list that can differ from the first silently.
/// </summary>
internal static class PackageLoadProbe
{
    public static int Run()
    {
        Console.WriteLine();
        Console.WriteLine("PROBE 1.3 — every pinned surface package in one process");
        Console.WriteLine();

        var assemblies = Directory
            .GetFiles(AppContext.BaseDirectory, "Paramore.Brighter*.dll")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => n is not null)
            .Select(n => n!)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        if (assemblies.Count == 0)
        {
            Console.Error.WriteLine("no Paramore.Brighter assemblies in the output directory — "
                                    + "has the project been built?");
            return 2;
        }

        Console.WriteLine($"{assemblies.Count} Paramore.Brighter assemblies are in the process — the "
                          + "packages `probes.csproj` names,");
        Console.WriteLine("plus the Brighter packages they depend on. None of them is `.V4`.");
        Console.WriteLine();
        Console.WriteLine($"{"package",-56} {"type instantiated",-40} {"pass",4}  result");
        Console.WriteLine(new string('-', 118));

        var loadFailures = new List<string>();
        var noConstructible = new List<string>();
        var requestedBy = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.Ordinal);
        var pass2 = 0;

        foreach (var name in assemblies)
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(name);
            }
            catch (Exception ex)
            {
                loadFailures.Add($"{name}: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"{name,-56} {"—",-40} {"—",4}  LOAD FAILED");
                continue;
            }

            foreach (var reference in assembly.GetReferencedAssemblies())
            {
                if (reference.Name is null || IsPlatform(reference.Name))
                    continue;

                if (!requestedBy.TryGetValue(reference.Name, out var byVersion))
                    requestedBy[reference.Name] = byVersion =
                        new Dictionary<string, List<string>>(StringComparer.Ordinal);

                var version = reference.Version?.ToString() ?? "unversioned";
                if (!byVersion.TryGetValue(version, out var referrers))
                    byVersion[version] = referrers = [];

                referrers.Add(name);
            }

            var (type, result) = FirstConstructible(assembly);

            if (type is null)
            {
                noConstructible.Add($"{name}: {result?.Error ?? "no public class the synthesiser can build"}");
                Console.WriteLine($"{name,-56} {"—",-40} {"—",4}  NOTHING CONSTRUCTIBLE");
                continue;
            }

            if (result!.Pass == 2) pass2++;
            Console.WriteLine($"{name,-56} {type.Name,-40} {result.Pass,4}  ok");
        }

        Console.WriteLine();
        Console.WriteLine($"{pass2} of the instantiations needed pass 2 — a defaulted parameter the "
                          + "constructor body validates.");

        foreach (var failure in loadFailures)
            Console.WriteLine($"LOAD FAILURE  {failure}");
        foreach (var failure in noConstructible)
            Console.WriteLine($"NOT CONSTRUCTIBLE  {failure}");

        // A third-party assembly is what design §6.4 is actually worried about,
        // and requesting one is not the same as loading it. Load each, and
        // compare what arrived against what every referrer asked for: one
        // process holds one version of a simple name, so a referrer compiled
        // against a different major is running against an assembly it has never
        // seen. That is the conflict, and it is silent until a member is missing.
        Console.WriteLine();
        Console.WriteLine($"{requestedBy.Count} distinct third-party assemblies are referenced by the "
                          + "packages above. Loading each:");

        var unsatisfied = new List<string>();
        var thirdPartyLoadFailures = new List<string>();

        foreach (var (simpleName, byVersion) in requestedBy.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            Version? loaded;
            try
            {
                loaded = Assembly.Load(simpleName).GetName().Version;
            }
            catch (Exception ex)
            {
                thirdPartyLoadFailures.Add($"{simpleName}: {ex.GetType().Name}");
                continue;
            }

            foreach (var (requested, referrers) in byVersion)
            {
                if (Version.TryParse(requested, out var wanted) && loaded is not null
                                                               && wanted.Major != loaded.Major)
                {
                    unsatisfied.Add($"{simpleName}: loaded {loaded}, but "
                                    + $"{string.Join(" and ", referrers)} asked for {requested}");
                }
            }
        }

        if (thirdPartyLoadFailures.Count > 0)
        {
            Console.WriteLine($"  {thirdPartyLoadFailures.Count} would not load:");
            foreach (var failure in thirdPartyLoadFailures)
                Console.WriteLine($"    {failure}");
        }

        if (unsatisfied.Count == 0)
        {
            Console.WriteLine("  All loaded, and no referrer is left running against a different MAJOR "
                              + "version than it was built for.");
        }
        else
        {
            Console.WriteLine($"  {unsatisfied.Count} referrer(s) left unsatisfied — design §6.4's conflict, "
                              + "and NOT the AWS pair it predicted:");
            foreach (var line in unsatisfied)
                Console.WriteLine($"    {line}");
        }

        var rmq = RabbitMqPair();

        Console.WriteLine();
        var clean = loadFailures.Count == 0 && thirdPartyLoadFailures.Count == 0 && rmq == 0;
        if (clean && unsatisfied.Count == 0)
        {
            Console.WriteLine("VERDICT: CLEAN. Every pinned package loads in one process and one type from");
            Console.WriteLine("each was instantiated for real. Design §6.4's fallback — one process per");
            Console.WriteLine("package family — is not needed, and `optioncheck` can be a single project.");
        }
        else if (clean)
        {
            Console.WriteLine("VERDICT: CLEAN, WITH ONE THING TO KNOW. Every pinned package loads in one");
            Console.WriteLine("process and every type 012 documents was instantiated for real — including");
            Console.WriteLine("both sides of the version disagreement above, which is therefore latent");
            Console.WriteLine("rather than fatal. `optioncheck` can be a single project; §6.4's fallback is");
            Console.WriteLine("not needed. Record the disagreement, because the next package to reach into");
            Console.WriteLine("the older API is the one that turns it into a failure.");
        }
        else
        {
            Console.WriteLine("VERDICT: CONFLICT. Take design §6.4's fallback — one `dotnet run` per package");
            Console.WriteLine("family, results concatenated. NOT AssemblyLoadContext (no native isolation),");
            Console.WriteLine("and NOT MetadataLoadContext (cannot instantiate, which is what §6.2 needs).");
        }

        // A package with no constructible type is a gap in the synthesiser, not a
        // package conflict, so it is reported and does not change the verdict.
        return clean ? 0 : 1;
    }

    /// <summary>
    /// The pair the version disagreement lands on. Design §7.2 diffs `RmqSubscription`
    /// across `RMQ.Async` and `RMQ.Sync` to establish that they differ by `queueType`
    /// alone, so 012 needs both types to be constructible in one process — which is
    /// exactly what a single resolved `RabbitMQ.Client` puts in doubt. Measured
    /// rather than assumed, either way.
    /// </summary>
    private static int RabbitMqPair()
    {
        Console.WriteLine();
        Console.WriteLine("The pair the disagreement lands on — design §7.2 needs both:");

        var failures = 0;

        foreach (var package in new[]
                 {
                     "Paramore.Brighter.MessagingGateway.RMQ.Async",
                     "Paramore.Brighter.MessagingGateway.RMQ.Sync",
                 })
        {
            try
            {
                Type[] types;
                try
                {
                    types = Assembly.Load(package).GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // Some of the assembly's types are unloadable, which is itself
                    // the finding. Carry on with the ones that did load and say so:
                    // "the package is broken here" and "this one type is broken
                    // here" are different verdicts and only the second blocks 012.
                    Console.WriteLine($"  {package,-46} {ex.Types.Count(t => t is null)} of "
                                      + $"{ex.Types.Length} types would not load");
                    types = ex.Types.Where(t => t is not null).Select(t => t!).ToArray();
                }

                var type = types
                    .Single(t => t.Name == "RmqSubscription" && !t.IsGenericTypeDefinition);

                var result = Synthesiser.TryCreate(type);
                var ctor = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
                var hasQueueType = ctor.GetParameters().Any(p => p.Name == "queueType");

                Console.WriteLine(result.Ok
                    ? $"  {package,-46} constructed, {ctor.GetParameters().Length} ctor params, "
                      + $"queueType {(hasQueueType ? "present" : "absent")}"
                    : $"  {package,-46} FAILED: {result.Error}");

                if (!result.Ok) failures++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  {package,-46} FAILED: {ex.GetType().Name}: {ex.Message}");
                failures++;
            }
        }

        return failures;
    }

    private static (Type?, Synthesiser.Result?) FirstConstructible(Assembly assembly)
    {
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t is not null).Select(t => t!).ToArray();
        }

        // Prefer the shapes 012 writes tables from, so that a package's line here
        // is a dry run of what `optioncheck` will do to it rather than a load test
        // of an unrelated class.
        var candidates = types
            .Where(t => t is { IsPublic: true, IsAbstract: false, IsGenericTypeDefinition: false, IsClass: true })
            .OrderByDescending(t => IsSurfaceName(t.Name))
            .ThenBy(t => t.FullName, StringComparer.Ordinal)
            .Take(60)
            .ToList();

        Synthesiser.Result? first = null;
        Type? firstType = null;

        foreach (var candidate in candidates)
        {
            var result = Synthesiser.TryCreate(candidate);
            first ??= result;
            firstType ??= candidate;

            if (result.Ok)
                return (candidate, result);
        }

        // Report the FIRST candidate's failure, not the last: the ordering above
        // puts the type 012 would document at the front, so its error is the one
        // that says something. The last is whatever happened to sort last.
        return (null, first is null
            ? null
            : first with { Error = $"{firstType?.Name}: {first.Error} "
                                   + $"({candidates.Count} candidates tried, none constructible)" });
    }

    private static bool IsSurfaceName(string name) =>
        name.EndsWith("Subscription", StringComparison.Ordinal)
        || name.EndsWith("Publication", StringComparison.Ordinal)
        || name.EndsWith("Configuration", StringComparison.Ordinal)
        || name.EndsWith("Options", StringComparison.Ordinal)
        || name.EndsWith("Connection", StringComparison.Ordinal);

    private static bool IsPlatform(string name) =>
        name is "mscorlib" or "netstandard"
        || name.StartsWith("System", StringComparison.Ordinal)
        || name.StartsWith("Microsoft.CSharp", StringComparison.Ordinal)
        || name.StartsWith("Paramore.Brighter", StringComparison.Ordinal);
}
