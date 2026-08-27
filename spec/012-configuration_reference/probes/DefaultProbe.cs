using System.Reflection;
using Paramore.Brighter;

namespace Paramore.Docs.Probes;

/// <summary>
/// Task 1.2 — the probe the whole `Default` column rests on.
///
/// Requirements §5.1 reads `Subscription.cs:208` and `:236` and infers that
/// `emptyChannelDelay` defaults to `null` in the signature and to 500 ms in the
/// constructor body. That is a reading of the authority, not a measurement of
/// it. This runs it.
///
/// It prints BOTH routes for every defaulted parameter of the widest
/// constructor, because the finding is the *difference*: where the two columns
/// disagree, a checker reading `ParameterInfo` alone documents the option
/// WRONG rather than merely missing it.
/// </summary>
internal static class DefaultProbe
{
    public static int Run()
    {
        Console.WriteLine("PROBE 1.2 — the body-coalesced default");
        Console.WriteLine("Subject: Paramore.Brighter.Subscription, package Paramore.Brighter 10.7.0");
        Console.WriteLine();

        var ctor = typeof(Subscription)
            .GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .First();

        // The three required arguments, plus the two the constructor body
        // *validates* — see the finding at the foot of this probe.
        var subscription = new Subscription(
            new SubscriptionName("probe"),
            new ChannelName("probe"),
            new RoutingKey("probe"),
            requestType: typeof(ProbeRequest),
            messagePumpType: MessagePumpType.Reactor);

        // The two the probe had to supply itself. They are printed, because a
        // row missing from a table is a row nobody can question, but they are
        // excluded from the arithmetic: their instance value is the probe's own
        // argument and says nothing about the constructor body.
        string[] supplied = ["requestType", "messagePumpType"];

        Console.WriteLine($"{"parameter",-32} {"ParameterInfo.DefaultValue",-28} {"instance property",-28} agree?");
        Console.WriteLine(new string('-', 100));

        var coalesced = new List<string>();
        var nullSignature = 0;

        foreach (var p in ctor.GetParameters().Where(p => p.HasDefaultValue))
        {
            var property = typeof(Subscription)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(x => string.Equals(x.Name, p.Name, StringComparison.OrdinalIgnoreCase));

            if (property is null)
                continue;   // a parameter with no property of its own; §5.1's `Option` column names it anyway

            var signature = p.DefaultValue;
            var instance = property.GetValue(subscription);
            var isSupplied = supplied.Contains(p.Name);
            var agree = Equals(Show(signature), Show(instance));

            if (!isSupplied)
            {
                if (signature is null)
                    nullSignature++;
                if (!agree)
                    coalesced.Add($"{p.Name}: signature {Show(signature)}, instance {Show(instance)}");
            }

            var verdict = isSupplied ? "probe-supplied" : agree ? "yes" : "NO";
            Console.WriteLine($"{p.Name,-32} {Show(signature),-28} {Show(instance),-28} {verdict}");
        }

        Console.WriteLine();
        Console.WriteLine($"Of the parameters the probe left alone, {nullSignature} say `null` in the "
                          + $"signature and {coalesced.Count} of those come back as something else:");
        foreach (var line in coalesced)
            Console.WriteLine($"  {line}");
        Console.WriteLine($"The other {nullSignature - coalesced.Count} really are null on the instance, which "
                          + "is why the shapes cannot be told apart");
        Console.WriteLine("from the signature: `null` there means both \"no default\" and \"the default is "
                          + "assigned below\".");

        // The named case — the one requirements §5.1 and AC3b are written about.
        var emptyChannelDelay = ctor.GetParameters().Single(p => p.Name == "emptyChannelDelay");
        var signatureValue = emptyChannelDelay.DefaultValue;
        var instanceValue = subscription.EmptyChannelDelay;

        Console.WriteLine();
        Console.WriteLine("The named case, requirements §5.1:");
        Console.WriteLine($"  emptyChannelDelay  ParameterInfo.DefaultValue = {Show(signatureValue)}");
        Console.WriteLine($"  EmptyChannelDelay  instance                   = {Show(instanceValue)}");

        var premiseHolds = signatureValue is null && instanceValue == TimeSpan.FromMilliseconds(500);

        Console.WriteLine();
        if (premiseHolds)
        {
            Console.WriteLine("PREMISE HOLDS. The signature says `null`; the instance says 500 ms.");
            Console.WriteLine("A checker reading ParameterInfo alone documents this option as `null` — wrong,");
            Console.WriteLine("not missing. The `Default` column must come from an instantiated object.");
        }
        else
        {
            Console.WriteLine("PREMISE FAILS — the two routes AGREE on emptyChannelDelay.");
            Console.WriteLine("Spec 012's central premise is wrong at this ref and phase 2 changes shape.");
            Console.WriteLine("Do not proceed to phase 2; take this back to the design.");
        }

        Console.WriteLine();
        Console.WriteLine("Also found, by construction rather than by reading (see task 1.4):");
        Console.WriteLine("  `Subscription` cannot be constructed from its required parameters alone. The");
        Console.WriteLine("  constructor body throws ConfigurationException unless `messagePumpType` is set");
        Console.WriteLine("  to something other than its own default of Unknown, and unless one of");
        Console.WriteLine("  `requestType` or `getRequestType` is non-null. Two DEFAULTED parameters are");
        Console.WriteLine("  therefore required in practice, which no parse of the signature can see.");

        return premiseHolds ? 0 : 1;
    }

    internal static string Show(object? value) => value switch
    {
        null => "null",
        TimeSpan t => $"{t.TotalMilliseconds:0.###} ms",
        string s => $"\"{s}\"",
        _ => value.ToString() ?? "null",
    };
}

/// <summary>A request type with no behaviour, so `Subscription` has one to name.</summary>
internal sealed class ProbeRequest : IRequest
{
    public Id Id { get; set; } = Id.Random();
    public Id? CorrelationId { get; set; }
}
