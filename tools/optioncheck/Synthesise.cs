using System.Reflection;
using Paramore.Brighter;

namespace Paramore.Docs.OptionCheck;

/// <summary>
/// Task 2.4 — constructor arguments for the types that need them.
///
/// Reading a default off an instance (design §6.2) means building the instance
/// first, and design §6.3 sizes that burden: strings, enums and the three
/// subscription arguments cover most of it. Probe 1.4 then measured the running
/// version of the same table and found **34 types and 70 required parameters**,
/// not 24 and 48 — and one thing design does not name at all:
///
///   THIRTEEN CONSTRUCTORS REJECT THEIR OWN DEFAULTS. Every subscription type in
///   the product requires a request type, and eleven also require
///   `messagePumpType`, whose declared default of `Unknown` the body refuses.
///
/// That is why this class does not stop at "it constructed". A parameter the
/// checker had to supply a value for is a parameter whose instance value is the
/// checker's own argument — so reading a `Default` back off it would document
/// the checker rather than the product. <see cref="Supplied"/> is that set, and
/// <see cref="Program"/> requires a `manual:` declaration for every member in
/// it rather than printing a value it made up.
///
/// NECESSITY IS MEASURED, NOT READ OFF WHAT WAS SUPPLIED. Probe 1.4's first
/// draft read pass 2's supplied list as the constructors' required list and
/// reported that all thirteen need `makeChannels`; not one of them does. So each
/// supplied candidate is put back to its own declared default, one at a time,
/// and kept only where its removal breaks construction.
/// </summary>
internal static class Synthesise
{
    internal sealed record Result(object? Instance, IReadOnlySet<string> Supplied, string? Error)
    {
        public bool Ok => Instance is not null;
    }

    private static readonly Dictionary<Type, Result> Cache = [];

    public static Result Instance(Type type)
    {
        if (Cache.TryGetValue(type, out var cached)) return cached;
        return Cache[type] = Build(type);
    }

    private static Result Build(Type type)
    {
        // The hand-written factories design §6.3 anticipated. Probe 1.4 measured
        // which types genuinely need one: `AzureBlobArchiveProviderOptions` and
        // `S3LuggageOptions` are the two that fail without one, and both are P2.
        // `HandlerConfiguration` is the one of §6.3's four that is P0 — it is on
        // D4, phase 3 — so it gets a factory here rather than a declaration.
        if (Factory(type) is { } made)
            return new Result(made, new HashSet<string>(StringComparer.Ordinal), null);

        var ctor = Widest(type);
        if (ctor is null)
            return new Result(null, EmptySet, "no public constructor");

        if (ctor.GetParameters().Length == 0)
        {
            try { return new Result(ctor.Invoke([]), EmptySet, null); }
            catch (Exception ex) { return new Result(null, EmptySet, Explain(ex)); }
        }

        // Pass 1 — required parameters only. Every defaulted parameter keeps its
        // own default, so if this works, every default the checker reads back is
        // the product's own.
        var pass1 = TryInvoke(ctor, supply: p => !p.HasDefaultValue);
        if (pass1.Instance is not null)
            return new Result(pass1.Instance, EmptySet, null);

        // Pass 2 — supply defaulted parameters too, where there is a value to
        // supply. Needed only where the constructor body validates a defaulted
        // parameter, which no parse of a signature can see.
        var pass2 = TryInvoke(ctor, supply: _ => true);
        if (pass2.Instance is null)
            return new Result(null, EmptySet, pass1.Error ?? pass2.Error);

        // Necessity: put each defaulted parameter pass 2 supplied back to its own
        // default and keep the removal if the type still builds.
        var needed = new HashSet<string>(pass2.Supplied, StringComparer.Ordinal);
        foreach (var candidate in pass2.Supplied.OrderBy(n => n, StringComparer.Ordinal))
        {
            var trial = new HashSet<string>(needed, StringComparer.Ordinal) { };
            trial.Remove(candidate);

            var attempt = TryInvoke(ctor, p => !p.HasDefaultValue || (p.Name is not null && trial.Contains(p.Name)));
            if (attempt.Instance is not null) needed = trial;
        }

        var final = TryInvoke(ctor, p => !p.HasDefaultValue || (p.Name is not null && needed.Contains(p.Name)));

        // Interaction effects: a set that is minimal one parameter at a time is
        // not guaranteed to be minimal together. Where the minimised set does not
        // build, fall back to pass 2's — a larger `manual:` obligation is honest;
        // a value the checker invented is not.
        return final.Instance is not null
            ? new Result(final.Instance, needed, null)
            : new Result(pass2.Instance, pass2.Supplied, null);
    }

    private static readonly IReadOnlySet<string> EmptySet = new HashSet<string>(StringComparer.Ordinal);

    private sealed record Attempt(object? Instance, IReadOnlySet<string> Supplied, string? Error);

    private static Attempt TryInvoke(ConstructorInfo ctor, Func<ParameterInfo, bool> supply)
    {
        var parameters = ctor.GetParameters();
        var args = new object?[parameters.Length];
        var supplied = new HashSet<string>(StringComparer.Ordinal);

        foreach (var p in parameters)
        {
            if (!supply(p) && p.HasDefaultValue)
            {
                args[p.Position] = p.DefaultValue;
                continue;
            }

            var value = Value(p.ParameterType, depth: 0);

            if (value is null && p.HasDefaultValue)
            {
                // Nothing better to offer than the declared default, so this is
                // not a parameter the checker supplied.
                args[p.Position] = p.DefaultValue;
                continue;
            }

            if (value is null && !IsNullable(p))
                return new Attempt(null, supplied, $"cannot synthesise {Pretty(p.ParameterType)} {p.Name}");

            args[p.Position] = value;
            if (p.Name is not null && p.HasDefaultValue) supplied.Add(p.Name);
        }

        try { return new Attempt(ctor.Invoke(args), supplied, null); }
        catch (Exception ex) { return new Attempt(null, supplied, Explain(ex)); }
    }

    /// <summary>A value for a parameter type, or null where the synthesiser has none.</summary>
    private static object? Value(Type t, int depth)
    {
        var underlying = Nullable.GetUnderlyingType(t) ?? t;

        if (underlying == typeof(string)) return "optioncheck";
        if (underlying == typeof(Type)) return typeof(ProbeRequest);
        if (underlying == typeof(bool)) return false;
        if (underlying == typeof(int)) return 1;
        if (underlying == typeof(long)) return 1L;
        if (underlying == typeof(double)) return 1d;
        if (underlying == typeof(TimeSpan)) return TimeSpan.FromMilliseconds(1);
        if (underlying == typeof(Uri)) return new Uri("https://optioncheck.invalid");

        if (underlying.IsEnum)
        {
            // `MessagePumpType.Unknown` is 0 and the Subscription constructor
            // throws on it, so a zero-valued member named Unknown or None is
            // exactly the wrong choice.
            var usable = Enum.GetNames(underlying).FirstOrDefault(n => n is not ("Unknown" or "None"));
            return usable is null ? Enum.GetValues(underlying).GetValue(0) : Enum.Parse(underlying, usable);
        }

        // Brighter's own types — the single-string value types first
        // (SubscriptionName, ChannelName, RoutingKey; design §6.3's three
        // subscription arguments are all of this shape), then, one level down,
        // any Brighter class these same rules can build.
        if (underlying.Namespace?.StartsWith("Paramore.Brighter") != true
            || underlying.IsAbstract || underlying.IsInterface || depth >= 2)
            return null;

        var single = underlying.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(c => c.GetParameters().Length == 1
                                 && c.GetParameters()[0].ParameterType == typeof(string));

        if (single is not null)
        {
            try { return single.Invoke(["optioncheck"]); }
            catch { return null; }
        }

        var ctor = Widest(underlying);
        if (ctor is null) return null;

        var nested = TryInvoke(ctor, p => !p.HasDefaultValue);
        return nested.Instance ?? TryInvoke(ctor, _ => true).Instance;
    }

    /// <summary>
    /// The hand-written factories. `HandlerConfiguration` is the P0 one and the
    /// reason this method exists: it takes two interfaces, and an interface is
    /// exactly what the generic path cannot invent.
    /// </summary>
    private static object? Factory(Type type) => type.FullName switch
    {
        "Paramore.Brighter.HandlerConfiguration" =>
            new HandlerConfiguration(new SubscriberRegistry(), new NullHandlerFactory()),
        _ => null,
    };

    private static ConstructorInfo? Widest(Type type) =>
        type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

    private static bool IsNullable(ParameterInfo p) =>
        !p.ParameterType.IsValueType || Nullable.GetUnderlyingType(p.ParameterType) is not null;

    private static string Explain(Exception ex)
    {
        var inner = ex.InnerException ?? ex;
        return $"{inner.GetType().Name}: {inner.Message}";
    }

    public static string Pretty(Type t) => Reflect.Pretty(t);
}

/// <summary>A request type with no behaviour, so a subscription has one to name.</summary>
internal sealed class ProbeRequest : IRequest
{
    public Id Id { get; set; } = Id.Random();
    public Id? CorrelationId { get; set; }
}

/// <summary>
/// The handler factory `HandlerConfiguration` requires. It is never asked for a
/// handler — the checker reads properties off the configuration object and
/// dispatches nothing — so returning null is the whole implementation.
/// </summary>
internal sealed class NullHandlerFactory : IAmAHandlerFactorySync
{
    public IHandleRequests? Create(Type handlerType, IAmALifetime lifetime) => null;

    public void Release(IHandleRequests handler, IAmALifetime lifetime) { }
}
