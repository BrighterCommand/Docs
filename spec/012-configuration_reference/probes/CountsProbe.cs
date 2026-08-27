using System.Reflection;

namespace Paramore.Docs.Probes;

/// <summary>
/// The oracle for task 1.5.
///
/// `survey.py` counts a type's options by parsing source; the checker will count
/// them by reflecting over an assembly. Those are two instruments measuring one
/// quantity, and the only way to know the parser is right is to hold it against
/// the thing it is a proxy for.
///
/// This prints one TSV row per surface type — assembly, type, settable
/// properties, widest constructor parameters, and `max` of the two, which is
/// design §2's convention — so the survey's table can be diffed against it.
///
///   dotnet run --project spec/012-configuration_reference/probes -- counts
///
/// DeclaredOnly, deliberately: the survey counts what a type's own source file
/// declares, so counting inherited members here would make the two instruments
/// disagree by construction and prove nothing.
/// </summary>
internal static class CountsProbe
{
    public static int Run()
    {
        Console.WriteLine("assembly\ttype\tprops\tctor\tmax");

        var assemblies = Directory
            .GetFiles(AppContext.BaseDirectory, "Paramore.Brighter*.dll")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => n is not null)
            .Select(n => n!)
            .OrderBy(n => n, StringComparer.Ordinal);

        foreach (var name in assemblies)
        {
            Assembly assembly;
            try { assembly = Assembly.Load(name); }
            catch { continue; }

            Type[] types;
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t is not null).Select(t => t!).ToArray();
            }

            foreach (var type in types.OrderBy(t => t.Name, StringComparer.Ordinal))
            {
                if (type is not { IsPublic: true, IsClass: true } || type.IsGenericTypeDefinition)
                    continue;
                if (!IsSurfaceName(type.Name))
                    continue;

                var props = type
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Count(p => p.SetMethod is { IsPublic: true });

                var ctor = type
                    .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .Select(c => c.GetParameters().Length)
                    .DefaultIfEmpty(0)
                    .Max();

                Console.WriteLine($"{name}\t{type.Name}\t{props}\t{ctor}\t{Math.Max(props, ctor)}");
            }
        }

        return 0;
    }

    private static bool IsSurfaceName(string name) =>
        name.EndsWith("Subscription", StringComparison.Ordinal)
        || name.EndsWith("Publication", StringComparison.Ordinal)
        || name.EndsWith("Configuration", StringComparison.Ordinal)
        || name.EndsWith("Options", StringComparison.Ordinal)
        || name.EndsWith("Connection", StringComparison.Ordinal);
}
