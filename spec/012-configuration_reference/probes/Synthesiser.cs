using System.Reflection;

namespace Paramore.Docs.Probes;

/// <summary>
/// The argument synthesiser design §6.3 sizes: strings, enums, and the three
/// subscription arguments. `optioncheck` needs one because reading a default
/// off an instance means building the instance first (design §6.2).
///
/// Two passes, and the difference between them is a measurement rather than an
/// implementation detail:
///
///   pass 1 — required parameters synthesised, optional ones left at their own
///            declared default. This is what a reader of the signature would
///            predict is enough.
///   pass 2 — optional parameters synthesised too, where the synthesiser has a
///            value for them. Needed only where the constructor body VALIDATES
///            a defaulted parameter, which no parse of the signature can see.
/// </summary>
internal static class Synthesiser
{
    internal sealed record Result(object? Instance, int Pass, string? Error, ConstructorInfo? Ctor)
    {
        public bool Ok => Instance is not null;
    }

    public static Result TryCreate(Type type) => TryCreate(type, depth: 0);

    private static Result TryCreate(Type type, int depth)
    {
        var ctor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (ctor is null)
            return new Result(null, 0, "no public constructor", null);

        string? firstError = null;

        for (var pass = 1; pass <= 2; pass++)
        {
            var (args, unsynthesisable) = BuildArgs(ctor, pass, depth, null);

            if (unsynthesisable is not null)
                return new Result(null, pass,
                    $"cannot synthesise {Pretty(unsynthesisable.ParameterType)} {unsynthesisable.Name}", ctor);

            try
            {
                return new Result(ctor.Invoke(args), pass, null, ctor);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException ?? ex;
                firstError ??= $"{inner.GetType().Name}: {inner.Message}";
            }
        }

        return new Result(null, 2, firstError, ctor);
    }

    /// <summary>
    /// Can this type be constructed with pass-2 arguments, except for the named
    /// parameters, which keep their own declared default?
    ///
    /// This is what turns "pass 2 supplied these five parameters" into "the
    /// constructor requires these two" — supplying a value is not evidence that
    /// it was needed, and the difference is a claim about the product.
    /// </summary>
    public static bool CanConstructKeeping(Type type, IReadOnlySet<string> keepOwnDefault)
    {
        var ctor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (ctor is null) return false;

        var (args, unsynthesisable) = BuildArgs(ctor, pass: 2, depth: 0, keepOwnDefault);
        if (unsynthesisable is not null) return false;

        try { return ctor.Invoke(args) is not null; }
        catch { return false; }
    }

    private static (object?[] Args, ParameterInfo? Unsynthesisable) BuildArgs(
        ConstructorInfo ctor, int pass, int depth, IReadOnlySet<string>? keepOwnDefault)
    {
        var args = new object?[ctor.GetParameters().Length];

        foreach (var p in ctor.GetParameters())
        {
            var keep = p.HasDefaultValue
                       && (pass == 1 || (p.Name is not null && keepOwnDefault?.Contains(p.Name) == true));

            if (keep)
            {
                args[p.Position] = p.DefaultValue;
                continue;
            }

            var value = Synthesise(p.ParameterType, depth);
            if (value is null && !IsNullable(p))
            {
                if (p.HasDefaultValue)
                {
                    // pass 2 has nothing better to offer than the declared default
                    args[p.Position] = p.DefaultValue;
                    continue;
                }

                return (args, p);
            }

            args[p.Position] = value ?? (p.HasDefaultValue ? p.DefaultValue : null);
        }

        return (args, null);
    }

    /// <summary>A value for a parameter type, or null where the synthesiser has none.</summary>
    private static object? Synthesise(Type t, int depth)
    {
        var underlying = Nullable.GetUnderlyingType(t) ?? t;

        if (underlying == typeof(string)) return "probe";
        if (underlying == typeof(Type)) return typeof(ProbeRequest);
        if (underlying == typeof(bool)) return false;
        if (underlying == typeof(int)) return 1;
        if (underlying == typeof(long)) return 1L;
        if (underlying == typeof(double)) return 1d;
        if (underlying == typeof(TimeSpan)) return TimeSpan.FromMilliseconds(1);
        if (underlying == typeof(Uri)) return new Uri("https://probe.invalid");

        if (underlying.IsEnum)
        {
            // `MessagePumpType.Unknown` is 0 and the Subscription constructor
            // throws on it, so a zero-valued enum member named Unknown or None
            // is exactly the wrong choice.
            var names = Enum.GetNames(underlying);
            var usable = names.FirstOrDefault(n => n is not ("Unknown" or "None"));
            return usable is null ? Enum.GetValues(underlying).GetValue(0) : Enum.Parse(underlying, usable);
        }

        // Brighter's own types — the single-string value types first
        // (SubscriptionName, ChannelName, RoutingKey, Id; design §6.3's "three
        // subscription arguments" are all of this shape), then, one level down,
        // any Brighter class the synthesiser can build from those same rules.
        // `RelationalDatabaseConfiguration` is the case that earns the recursion:
        // thirteen pages link its table (design §12.4) and every component that
        // takes it is a type `optioncheck` will have to construct.
        if (underlying.Namespace?.StartsWith("Paramore.Brighter") != true
            || underlying.IsAbstract || underlying.IsInterface || depth >= 2)
            return null;

        var single = underlying.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(c => c.GetParameters().Length == 1
                                 && c.GetParameters()[0].ParameterType == typeof(string));

        if (single is not null)
        {
            try { return single.Invoke(["probe"]); }
            catch { return null; }
        }

        return TryCreate(underlying, depth + 1).Instance;
    }

    private static bool IsNullable(ParameterInfo p) =>
        !p.ParameterType.IsValueType || Nullable.GetUnderlyingType(p.ParameterType) is not null;

    public static string Pretty(Type t)
    {
        if (!t.IsGenericType) return t.Name;
        var name = t.Name[..t.Name.IndexOf('`')];
        return $"{name}<{string.Join(", ", t.GetGenericArguments().Select(Pretty))}>";
    }
}
