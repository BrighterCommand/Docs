using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.JsonConverters;

var before = JsonSerialisationOptions.Options;
Console.WriteLine($"BEFORE: converters={before.Converters.Count} [{string.Join(", ", before.Converters.Select(c => c.GetType().Name))}] WriteIndented={before.WriteIndented}");

var services = new ServiceCollection();
services.AddBrighter().ConfigureJsonSerialisation(options => { options.WriteIndented = true; });

var after = JsonSerialisationOptions.Options;
Console.WriteLine($"AFTER : converters={after.Converters.Count} WriteIndented={after.WriteIndented} sameInstance={ReferenceEquals(before, after)}");
Console.WriteLine($"CONTROL (the instance read before the call) : WriteIndented={before.WriteIndented}");
