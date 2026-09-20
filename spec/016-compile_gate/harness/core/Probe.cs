// Reflection probe: what the RELEASED assemblies actually expose.
// Loads every Paramore.*.dll from the build output, so an assembly the
// compiler pruned for being unused is still examined.
using System;
using System.IO;
using System.Linq;
using System.Reflection;

public static class Probe
{
    public static void Main(string[] args)
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var asms = Directory.GetFiles(dir, "Paramore.*.dll")
            .Select(f => { try { return Assembly.LoadFrom(f); } catch { return null; } })
            .Where(a => a != null)
            .ToList();
        Console.WriteLine($"# {asms.Count} released assemblies loaded from {dir}");

        Func<Assembly, Type[]> types = a =>
        {
            try { return a.GetExportedTypes(); } catch { return new Type[0]; }
        };

        foreach (var arg in args)
        {
            Console.WriteLine("== " + arg);
            if (arg.StartsWith("type:"))
            {
                var want = arg.Substring(5);
                foreach (var asm in asms)
                    foreach (var t in types(asm).Where(t => t.Name == want || t.FullName == want))
                    {
                        Console.WriteLine($"  {t.FullName} : {t.BaseType}  [{asm.GetName().Name}]");
                        foreach (var m in t.GetMembers(BindingFlags.Public | BindingFlags.Instance |
                                                       BindingFlags.Static | BindingFlags.DeclaredOnly)
                                     .OrderBy(m => m.Name))
                        {
                            Console.WriteLine($"      {m.MemberType,-8} {m}");
                            var mb = m as MethodBase;
                            if (mb != null)
                                foreach (var p in mb.GetParameters())
                                    Console.WriteLine($"          param {p.ParameterType.Name} {p.Name}" +
                                        (p.HasDefaultValue ? $" = {(p.DefaultValue == null ? "null" : p.DefaultValue)}" : ""));
                        }
                    }
                continue;
            }
            foreach (var asm in asms)
                foreach (var t in types(asm))
                {
                    if (t.Name == arg || t.FullName == arg)
                        Console.WriteLine($"  TYPE {t.FullName}  [{asm.GetName().Name}]");
                    foreach (var m in t.GetMembers(BindingFlags.Public | BindingFlags.Static |
                                                   BindingFlags.Instance | BindingFlags.DeclaredOnly))
                        if (m.Name == arg)
                            Console.WriteLine($"  {m.MemberType} {t.FullName}.{m} [{asm.GetName().Name}]");
                }
        }
    }
}
