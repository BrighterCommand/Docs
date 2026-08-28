using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Paramore.Docs.OptionCheck;

/// <summary>
/// Task 2.3 — the two routes, one per column (design §6.2).
///
///   Option (name) → ParameterInfo / PropertyInfo. The parameter carries the
///                   spelling the reader types; the property does not.
///   Type          → the same metadata, no instance required.
///   Default       → AN INSTANTIATED OBJECT, READ BACK. ALWAYS.
///
/// Always, including where the parameter default would have been right.
/// Choosing per parameter would make the tool's correctness depend on a
/// judgement about which of requirements §5.1's three shapes a parameter is in
/// — and shape three (`null` in the signature, 500 ms from the body) is
/// precisely the one that LOOKS like shape two. One route for one column.
///
/// Which members are reader-facing is `survey.py`'s `max(props, ctor)`
/// convention expressed as code: settable properties, or the widest
/// constructor's parameters, whichever is the wider surface. Their UNION would
/// double-count — on a subscription every property has a matching parameter and
/// the two differ only in case, which is the hazard requirements §7.1 names,
/// not two options.
/// </summary>
internal static class Reflect
{
    /// <summary>Where a type's reader-facing members came from.</summary>
    internal enum Route
    {
        Properties,
        Constructor,
    }

    /// <summary>
    /// One reader-facing member. <paramref name="Default"/> is null when the
    /// tool cannot determine it, and <paramref name="Unreadable"/> then says why
    /// — that is the case `manual:` exists for, and it declares and counts
    /// rather than passing.
    /// </summary>
    internal sealed record Member(string Name, string TypeName, string? Default, string? Unreadable);

    internal sealed record Surface(Type Type, Route Route, IReadOnlyList<Member> Members, string? Error);

    private static readonly NullabilityInfoContext Nullability = new();

    /// <summary>
    /// The type a marker names, found across every Brighter assembly in the
    /// process. A marker binds a fully-qualified type, so a renamed type is
    /// `THE TYPE IS GONE` rather than a silent pass — phase 1 found two of those
    /// in design §7 before a table was ever written from them.
    /// </summary>
    public static Type? Resolve(string fullName)
    {
        foreach (var assembly in Authority.Loaded())
        {
            var type = assembly.GetType(fullName, throwOnError: false);
            if (type is not null) return type;
        }

        return Type.GetType(fullName, throwOnError: false);
    }

    public static Surface Describe(Type type)
    {
        var properties = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(p => p.SetMethod is { IsPublic: true } && !p.SetMethod.IsStatic)
            .ToList();

        var ctor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        var parameters = ctor?.GetParameters() ?? [];
        var route = parameters.Length > properties.Count ? Route.Constructor : Route.Properties;

        var built = Synthesise.Instance(type);

        var members = route == Route.Constructor
            ? parameters.Select(p => FromParameter(type, p, built)).ToList()
            : properties.Select(p => FromProperty(p, built)).ToList();

        return new Surface(type, route, members, built.Ok ? null : built.Error);
    }

    private static Member FromParameter(Type type, ParameterInfo parameter, Synthesise.Result built)
    {
        var name = parameter.Name ?? "?";
        var declared = Annotated(parameter.ParameterType, Nullability.Create(parameter).ReadState);

        // A parameter with no declared default has no default to read: whatever
        // the instance holds is the argument the synthesiser passed in. `none`
        // is standing obligation 1's word for it, and it is a fact about the
        // parameter rather than a limit of the tool — so it is a value, not an
        // `Unreadable`.
        if (!parameter.HasDefaultValue)
            return new Member(name, declared, "none", null);

        if (!built.Ok)
            return new Member(name, declared, null, $"the type could not be constructed: {built.Error}");

        // A parameter the synthesiser had to supply a value for reads back the
        // checker's own argument, not the product's default. Say so rather than
        // print it: phase 1 measured thirteen constructors that reject their own
        // declared defaults, and every one of them lands here.
        if (built.Supplied.Contains(name))
            return new Member(name, declared, null,
                "the constructor rejects its own declared default for this parameter, so the "
                + "checker had to supply a value — the instance would read back that value");

        var property = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)
                                 && p.GetMethod is { IsPublic: true });

        if (property is null)
            return new Member(name, declared, null,
                "no property of that name on the instance, so the default cannot be read back");

        return WithValue(name, declared, () => property.GetValue(built.Instance));
    }

    private static Member FromProperty(PropertyInfo property, Synthesise.Result built)
    {
        var declared = Annotated(property.PropertyType, Nullability.Create(property).ReadState);

        if (!built.Ok)
            return new Member(property.Name, declared, null, $"the type could not be constructed: {built.Error}");

        return WithValue(property.Name, declared, () => property.GetValue(built.Instance));
    }

    private static Member WithValue(string name, string declared, Func<object?> read)
    {
        object? value;
        try
        {
            value = read();
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException ?? ex;
            return new Member(name, declared, null, $"reading it threw {inner.GetType().Name}: {inner.Message}");
        }

        var rendered = Render(value);
        if (rendered is null)
            return new Member(name, declared, null,
                $"the value has no printable form ({Pretty(value!.GetType())})");

        // The property is fed by a constructor argument the synthesiser supplied,
        // so what came back is the checker's own sentinel. Reporting it as a
        // default would be the tool documenting itself — the failure `manual:`
        // exists to declare instead.
        return Sentinel(rendered)
            ? new Member(name, declared, null,
                "the value on the instance is the argument the checker had to supply for a "
                + "required constructor parameter, not a default")
            : new Member(name, declared, rendered, null);
    }

    /// <summary>
    /// The synthesiser's own arguments, deliberately distinctive so that a value
    /// arriving back from an instance can be told apart from the product's.
    /// </summary>
    private static bool Sentinel(string rendered) =>
        rendered.Contains("optioncheck", StringComparison.Ordinal)
        || rendered.Contains(nameof(ProbeRequest), StringComparison.Ordinal);

    /// <summary>
    /// The canonical rendering of a default, and the forms a table may write it
    /// in instead. Presentation is not the subject — a table saying `00:00:00.5`
    /// where the canonical form is `500 ms` documents the same product.
    /// </summary>
    public static IReadOnlyList<string> Accepted(string canonical, string? typeName = null)
    {
        var forms = new List<string> { canonical };

        if (canonical.EndsWith(" ms", StringComparison.Ordinal)
            && double.TryParse(canonical[..^3], NumberStyles.Float, CultureInfo.InvariantCulture, out var ms))
        {
            var span = TimeSpan.FromMilliseconds(ms);
            forms.Add(span.ToString());
            forms.Add($"TimeSpan.FromMilliseconds({ms.ToString("0.###", CultureInfo.InvariantCulture)})");
            if (ms % 1000 == 0)
            {
                var seconds = (ms / 1000).ToString("0.###", CultureInfo.InvariantCulture);
                forms.Add($"{seconds} s");
                forms.Add($"{seconds}s");
                forms.Add($"TimeSpan.FromSeconds({seconds})");
            }
        }

        if (canonical.StartsWith('"') && canonical.EndsWith('"') && canonical.Length >= 2)
            forms.Add(canonical[1..^1]);

        if (typeName is not null && canonical is not "null")
            forms.Add($"{typeName}.{canonical}");

        return forms;
    }

    /// <summary>
    /// A default as a table would write it, or null where there is no honest way
    /// to write it — an object whose `ToString` is its own type name says
    /// nothing to a reader, and inventing a rendering for it would be the tool
    /// documenting itself.
    /// </summary>
    public static string? Render(object? value) => value switch
    {
        null => "null",
        bool b => b ? "true" : "false",
        string s => $"\"{s}\"",
        TimeSpan t => $"{t.TotalMilliseconds.ToString("0.###", CultureInfo.InvariantCulture)} ms",
        Enum e => e.ToString(),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        IEnumerable e => e.Cast<object?>().Any() ? null : "empty",
        _ => Printable(value),
    };

    private static string? Printable(object value)
    {
        var text = value.ToString();
        return text is null || text == value.GetType().FullName || text == value.GetType().ToString()
            ? null
            : text;
    }

    /// <summary>The declared type with its nullable annotation: `TimeSpan?`, never `TimeSpan`.</summary>
    private static string Annotated(Type type, NullabilityState state)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlying)
            return Pretty(underlying) + "?";

        return state == NullabilityState.Nullable ? Pretty(type) + "?" : Pretty(type);
    }

    public static string Pretty(Type type)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlying)
            return Pretty(underlying) + "?";

        if (Keywords.TryGetValue(type, out var keyword)) return keyword;

        if (type.IsArray)
            return Pretty(type.GetElementType()!) + "[]";

        if (!type.IsGenericType) return type.Name;

        var name = type.Name[..type.Name.IndexOf('`')];
        return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(Pretty))}>";
    }

    private static readonly Dictionary<Type, string> Keywords = new()
    {
        [typeof(bool)] = "bool",
        [typeof(byte)] = "byte",
        [typeof(sbyte)] = "sbyte",
        [typeof(char)] = "char",
        [typeof(decimal)] = "decimal",
        [typeof(double)] = "double",
        [typeof(float)] = "float",
        [typeof(int)] = "int",
        [typeof(uint)] = "uint",
        [typeof(long)] = "long",
        [typeof(ulong)] = "ulong",
        [typeof(short)] = "short",
        [typeof(ushort)] = "ushort",
        [typeof(object)] = "object",
        [typeof(string)] = "string",
    };
}
