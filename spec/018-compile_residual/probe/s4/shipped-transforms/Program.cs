// Task 6.4, DefaultMessageMappers.md § Transform Pipelines. As it stood (commit 254e8a7): "Brighter ships the Claim
// Check and Compression transforms; for anything else, such as encryption or PII removal, you write your own
// transform." As repaired: Claim Check, Compression and CloudEvents in Paramore.Brighter, and the JustSaying and
// MassTransit transformer packages' own.
// Case: every concrete WrapWithAttribute and UnwrapWithAttribute in Paramore.Brighter 10.8.1 and in the two
// Paramore.Brighter.Transformers.* packages that carry transforms, JustSaying and MassTransit (the other
// Transformers.* packages, AWS, Azure, GCP and MongoGridFS, are luggage stores). Control: the same reflection over
// this probe's own assembly, which declares one reader's transform, EncryptAttribute, and must find it.
using System;
using System.Linq;
using System.Reflection;
using Paramore.Brighter;

public class EncryptTransform { }                                                     // probe: the reader's own
public class EncryptAttribute(int step) : WrapWithAttribute(step) { public override Type GetHandlerType() => typeof(EncryptTransform); }

public static class Program
{
    static string[] Transforms(Assembly a) => a.GetTypes()
        .Where(t => !t.IsAbstract && (typeof(WrapWithAttribute).IsAssignableFrom(t) || typeof(UnwrapWithAttribute).IsAssignableFrom(t)))
        .Select(t => $"{(typeof(WrapWithAttribute).IsAssignableFrom(t) ? "wrap" : "unwrap")} {t.Name}")
        .OrderBy(n => n).ToArray();

    public static int Main()
    {
        var assemblies = new[]
        {
            typeof(Message).Assembly,
            Assembly.Load("Paramore.Brighter.Transformers.JustSaying"),
            Assembly.Load("Paramore.Brighter.Transformers.MassTransit"),
        };
        var shipped = assemblies.SelectMany(a => Transforms(a).Select(t => $"{a.GetName().Name} {a.GetName().Version}: {t}")).ToArray();
        foreach (var s in shipped) Console.WriteLine($"case:    {s}");
        var own = Transforms(typeof(Program).Assembly);
        foreach (var s in own) Console.WriteLine($"control: {s}");

        string[] Beyond(params string[] named) => shipped.Where(s => !named.Any(n => s.EndsWith($" {n}Attribute"))).ToArray();
        var asItStood = Beyond("ClaimCheck", "RetrieveClaim", "Compress", "Decompress");
        var asRepaired = Beyond("ClaimCheck", "RetrieveClaim", "Compress", "Decompress", "CloudEvents", "ReadFromCloudEvents",
            "JustSaying", "JustSayingDecompress", "MassTransitWrap", "MassTransitUnwrap");
        Console.WriteLine($"as it stood: {asItStood.Length} shipped transforms beyond the sentence: {string.Join(", ", asItStood.Select(s => s.Split(": ")[1]))}");
        Console.WriteLine($"{(asRepaired.Length == 0 ? "ok  " : "FAIL")} as repaired: {asRepaired.Length} shipped transforms beyond the sentence");
        var found = own.SequenceEqual(new[] { "wrap EncryptAttribute" });
        Console.WriteLine($"{(found ? "ok  " : "FAIL")} the instrument finds a transform it was not told about");
        return asRepaired.Length == 0 && asItStood.Length > 0 && found ? 0 : 1;
    }
}
