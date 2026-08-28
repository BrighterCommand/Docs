using System.Reflection;

namespace Paramore.Docs.OptionCheck;

/// <summary>
/// Exit 2 — the authority is unreachable, which is NOT a pass.
///
/// The family contract `linkcheck.py`, `pagelint.py` and `versioncheck.py`
/// already run under: 0 clean, 1 a real finding, 2 the subject could not be
/// consulted. For this tool the subject is the pinned packages, and the failure
/// has to be reachable as a verdict rather than only as a `dotnet restore`
/// error — a failed restore never runs this program at all, and a run that
/// checked nothing because its assemblies were missing must not be able to
/// print `0 mismatches`.
///
/// So `optioncheck.csproj` writes its own `PackageReference` list into the
/// assembly at build time, and this class holds what loaded against what was
/// pinned. Both halves of the pin are then in one place: the version the tables
/// are checked against, and the list of packages that version applies to.
/// </summary>
internal static class Authority
{
    internal sealed record Package(string Id, string Version);

    private static List<Assembly>? _loaded;

    /// <summary>The pinned list, as the build recorded it.</summary>
    public static IReadOnlyList<Package> Pinned()
    {
        using var stream = typeof(Authority).Assembly.GetManifestResourceStream("pinned-packages.txt");
        if (stream is null) return [];

        using var reader = new StreamReader(stream);
        var packages = new List<Package>();

        while (reader.ReadLine() is { } line)
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2) packages.Add(new Package(parts[0], parts[1]));
        }

        return packages;
    }

    /// <summary>The one version every table in spec 012 is checked against.</summary>
    public static string Pin() =>
        Pinned().Select(p => p.Version).Distinct(StringComparer.Ordinal).SingleOrDefault() ?? "mixed";

    /// <summary>
    /// Every Brighter assembly in the process, for <see cref="Reflect.Resolve"/>
    /// to search. Loaded from the output directory rather than from
    /// `AppDomain.CurrentDomain.GetAssemblies()`, which sees only what has
    /// already been touched — the trap 009 met in `AutoFromAssemblies`.
    /// </summary>
    public static IReadOnlyList<Assembly> Loaded()
    {
        if (_loaded is not null) return _loaded;

        _loaded = [];

        foreach (var path in Directory
                     .GetFiles(AppContext.BaseDirectory, "Paramore.Brighter*.dll")
                     .OrderBy(p => p, StringComparer.Ordinal))
        {
            try { _loaded.Add(Assembly.Load(Path.GetFileNameWithoutExtension(path))); }
            catch { /* reported by Missing() below, where it is a verdict rather than a silence */ }
        }

        return _loaded;
    }

    /// <summary>
    /// The pinned packages whose assembly is not in this process. Any at all is
    /// exit 2: the checker cannot say a table is right against an assembly it
    /// never read.
    /// </summary>
    public static IReadOnlyList<string> Missing()
    {
        var loaded = Loaded()
            .Select(a => a.GetName().Name)
            .Where(n => n is not null)
            .Select(n => n!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return Pinned()
            .Where(p => !loaded.Contains(AssemblyNameOf(p.Id)))
            .Select(p => $"{p.Id} {p.Version}")
            .ToList();
    }

    /// <summary>
    /// Every Brighter package ships an assembly of its own name. Kept as a
    /// method rather than inlined so that the day one does not, the exception
    /// has somewhere to live and a comment beside it.
    /// </summary>
    private static string AssemblyNameOf(string packageId) => packageId;
}
