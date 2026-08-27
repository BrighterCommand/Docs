using System.Reflection;

namespace Paramore.Docs.Probes;

/// <summary>
/// Task 1.4 — re-derive design §6.3's synthesis table BY CONSTRUCTION.
///
/// §6.3 is parsed from source with `survey.py`'s own parser and says about
/// itself that it is not a measurement of the running tool. This constructs
/// every type in that population and reports what actually happened, which is
/// the only route that can see a constructor body rejecting its own defaults.
///
/// The population is the one `survey.py` counts: a type whose name says it is a
/// configuration surface and whose widest public constructor takes at least one
/// parameter. `.V4` packages are out of scope (requirements §8) and are not
/// referenced by this project, so they cannot appear.
/// </summary>
internal static class SynthesisProbe
{
    public static int Run()
    {
        Console.WriteLine();
        Console.WriteLine("PROBE 1.4 — design §6.3's synthesis table, by construction");
        Console.WriteLine();

        var types = SurfaceTypes().ToList();
        if (types.Count == 0)
        {
            Console.Error.WriteLine("no surface types found — has the project been built?");
            return 2;
        }

        Console.WriteLine($"{"type",-38} {"package",-46} {"req",3} {"pass",4}  outcome");
        Console.WriteLine(new string('-', 118));

        var required = 0;
        var pass1 = new List<Type>();
        var pass2 = new List<(Type Type, string Why)>();
        var failed = new List<(Type Type, string Why)>();

        foreach (var type in types)
        {
            var ctor = WidestCtor(type)!;
            var need = ctor.GetParameters().Count(p => !p.HasDefaultValue);
            required += need;

            var result = Synthesiser.TryCreate(type);

            if (!result.Ok)
            {
                failed.Add((type, result.Error ?? "unknown"));
                Console.WriteLine($"{type.Name,-38} {Package(type),-46} {need,3} {"—",4}  NEEDS A FACTORY");
                continue;
            }

            if (result.Pass == 1)
            {
                pass1.Add(type);
                Console.WriteLine($"{type.Name,-38} {Package(type),-46} {need,3} {1,4}  constructed");
            }
            else
            {
                var why = ValidatedDefaults(ctor);
                pass2.Add((type, why));
                Console.WriteLine($"{type.Name,-38} {Package(type),-46} {need,3} {2,4}  constructed, "
                                  + "defaults overridden");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{types.Count} types with a parameterful public constructor, "
                          + $"{required} parameters carrying no default.");
        Console.WriteLine();
        Console.WriteLine($"  {pass1.Count,3} built from their required parameters alone");
        Console.WriteLine($"  {pass2.Count,3} built only after a DEFAULTED parameter was overridden");
        Console.WriteLine($"  {failed.Count,3} need a hand-written factory or a `manual:` declaration");

        if (pass2.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("The pass-2 types — a constructor body that rejects its own defaults, which no");
            Console.WriteLine("parse of a signature can see:");
            foreach (var (type, why) in pass2)
                Console.WriteLine($"  {type.Name}: {why}");
        }

        if (failed.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("The types needing a factory:");
            foreach (var (type, why) in failed)
                Console.WriteLine($"  {type.Name}: {why}");
        }

        Console.WriteLine();
        Console.WriteLine("Against design §6.3, which claims 24 types and 48 required parameters:");
        Console.WriteLine($"  types              measured {types.Count}, §6.3 says 24");
        Console.WriteLine($"  required params    measured {required}, §6.3 says 48");
        Console.WriteLine($"  needing a factory  measured {failed.Count}, §6.3 says 4");

        // A synthesiser that covers all but the hand-written cases is the claim
        // §6.3 makes and the one phase 2 builds on. The probe reports the shape;
        // the arithmetic reconciliation lives in probes/README.md, where it can
        // say WHY a figure moved.
        return 0;
    }

    private static IEnumerable<Type> SurfaceTypes()
    {
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

            foreach (var type in types.OrderBy(t => t.FullName, StringComparer.Ordinal))
            {
                if (type is not { IsPublic: true, IsAbstract: false, IsClass: true }
                    || type.IsGenericTypeDefinition
                    || !IsSurfaceName(type.Name))
                    continue;

                if (WidestCtor(type) is { } ctor && ctor.GetParameters().Length > 0)
                    yield return type;
            }
        }
    }

    private static ConstructorInfo? WidestCtor(Type type) =>
        type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

    /// <summary>The defaulted parameters pass 2 had to supply a value for.</summary>
    private static string ValidatedDefaults(ConstructorInfo ctor)
    {
        var names = ctor.GetParameters()
            .Where(p => p.HasDefaultValue)
            .Where(p => p.ParameterType.IsEnum || p.ParameterType == typeof(Type))
            .Select(p => p.Name)
            .Where(n => n is not null)
            .ToList();

        return names.Count == 0 ? "a defaulted parameter" : string.Join(", ", names);
    }

    private static string Package(Type type) => type.Assembly.GetName().Name ?? "?";

    private static bool IsSurfaceName(string name) =>
        name.EndsWith("Subscription", StringComparison.Ordinal)
        || name.EndsWith("Publication", StringComparison.Ordinal)
        || name.EndsWith("Configuration", StringComparison.Ordinal)
        || name.EndsWith("Options", StringComparison.Ordinal)
        || name.EndsWith("Connection", StringComparison.Ordinal);
}
