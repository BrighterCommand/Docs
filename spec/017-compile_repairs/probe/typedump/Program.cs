// Spec 017 probe: every public type in the pinned references, and every public
// extension method, as  type<TAB>Name<TAB>Namespace  or  ext<TAB>Method<TAB>Namespace.
// Reads refs.txt, skipping its '#' stamp line. Scratch instrument, not a gate.
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
var seen = new HashSet<string>();
foreach (var path in File.ReadAllLines(args[0]).Where(l => l.Trim().Length > 0 && !l.StartsWith("#")))
{
    using var fs = File.OpenRead(path.Trim());
    using var pe = new PEReader(fs);
    if (!pe.HasMetadata) continue;
    var md = pe.GetMetadataReader();
    foreach (var h in md.TypeDefinitions)
    {
        var t = md.GetTypeDefinition(h);
        var vis = t.Attributes & TypeAttributes.VisibilityMask;
        if (vis != TypeAttributes.Public && vis != TypeAttributes.NestedPublic) continue;
        var name = md.GetString(t.Name); var i = name.IndexOf('`'); if (i >= 0) name = name[..i];
        var ns = md.GetString(t.Namespace);
        if (seen.Add("type\t" + name + "\t" + ns)) Console.WriteLine("type\t" + name + "\t" + ns);
        if (vis != TypeAttributes.Public || (t.Attributes & TypeAttributes.Abstract) == 0
            || (t.Attributes & TypeAttributes.Sealed) == 0) continue;          // static class
        foreach (var mh in t.GetMethods())
        {
            var m = md.GetMethodDefinition(mh);
            if ((m.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public) continue;
            if (!m.GetCustomAttributes().Any(ah => IsExtension(md, md.GetCustomAttribute(ah)))) continue;
            var key = "ext\t" + md.GetString(m.Name) + "\t" + ns;
            if (seen.Add(key)) Console.WriteLine(key);
        }
    }
}
static bool IsExtension(MetadataReader md, CustomAttribute a)
{
    StringHandle n = default;
    if (a.Constructor.Kind == HandleKind.MemberReference)
    {
        var p = md.GetMemberReference((MemberReferenceHandle)a.Constructor).Parent;
        if (p.Kind == HandleKind.TypeReference) n = md.GetTypeReference((TypeReferenceHandle)p).Name;
    }
    else if (a.Constructor.Kind == HandleKind.MethodDefinition)
        n = md.GetTypeDefinition(md.GetMethodDefinition((MethodDefinitionHandle)a.Constructor).GetDeclaringType()).Name;
    return !n.IsNil && md.GetString(n) == "ExtensionAttribute";
}
