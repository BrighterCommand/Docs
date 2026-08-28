using Paramore.Docs.OptionCheck;

/// <summary>
/// Task 2.5 — D1's entry point. Design §6.1's seven steps, §6.5's exit codes.
///
///   dotnet run --project tools/optioncheck              # every page under contents/
///   dotnet run --project tools/optioncheck -- <paths>   # just these files or directories
///
/// The optional path arguments are `linkcheck.py`'s and `pagelint.py`'s
/// contract (tasks.md §2.5), and they are what lets phase 2's red-proofs run
/// against fixtures outside `contents/` rather than waiting for a page phase 3
/// has not written yet.
///
/// IT PRINTS ITS SCOPE BEFORE ITS VERDICT. `0 mismatches` across 0 tables and
/// `0 mismatches` across 628 must not print the same line — that is the family
/// contract `versioncheck.py` set, and it is the whole defence against a
/// vacuous green.
/// </summary>
internal static class Program
{
    private const int Clean = 0;
    private const int Findings = 1;
    private const int Unreachable = 2;

    private static int Main(string[] args)
    {
        // Step 1 of design §6.1, and it runs before anything else so that its
        // message is the one a reader gets. The packages are the authority, and
        // a checker that cannot read them has not passed.
        var missing = Authority.Missing();
        if (missing.Count > 0)
        {
            Console.Error.WriteLine($"optioncheck: AUTHORITY UNREACHABLE — {missing.Count} of "
                                    + $"{Authority.Pinned().Count} pinned packages "
                                    + $"{(missing.Count == 1 ? "is" : "are")} not in this process:");
            foreach (var package in missing) Console.Error.WriteLine($"  {package}");
            Console.Error.WriteLine("The pin is tools/optioncheck/optioncheck.csproj. Restore and build the "
                                    + "project, then run it again.");
            Console.Error.WriteLine("This is exit 2, not exit 1: nothing was checked, which is not the same "
                                    + "as nothing being wrong.");
            return Unreachable;
        }

        if (args.Length > 0 && args[0] == "--describe")
            return Describe(args.Skip(1).ToList());

        var root = RepositoryRoot();
        if (root is null && args.Length == 0)
        {
            Console.Error.WriteLine("optioncheck: cannot find the repository root (a directory holding "
                                    + "SUMMARY.md and contents/) above this assembly, and no paths were "
                                    + "given, so there is nothing to check.");
            return Unreachable;
        }

        var files = Files(args, root);
        if (files.Count == 0)
        {
            Console.Error.WriteLine("optioncheck: no markdown files to check in "
                                    + $"{(args.Length == 0 ? "contents/" : string.Join(", ", args))}");
            return Unreachable;
        }

        var markers = files
            .SelectMany(f => Binding.Parse(f, Report(root, f)))
            .ToList();

        var findings = new List<string>();
        var omitted = new List<string>();
        var manual = new List<string>();

        foreach (var marker in markers)
            Check(marker, findings, omitted, manual);

        Scope(files.Count, markers, omitted, manual);

        Console.WriteLine();
        foreach (var finding in findings) Console.WriteLine(finding);

        if (findings.Count > 0) Console.WriteLine();
        Console.WriteLine(findings.Count == 0
            ? $"0 mismatches across {Count(markers.Count, "table")} and "
              + $"{Count(markers.Sum(m => m.Rows.Count), "row")}."
            : $"{Count(findings.Count, "mismatch", "mismatches")} across "
              + $"{Count(markers.Count, "table")}.");

        return findings.Count == 0 ? Clean : Findings;
    }

    /// <summary>
    /// `--describe <type>…` — what a table for this type owes, in the four
    /// columns, with the descriptions left empty.
    ///
    /// Standing obligation 3 is *write the table from the type, never from the
    /// survey and never from our own prose*, and this is the instrument for
    /// obeying it. Before it existed the only way to learn a default was to
    /// write a wrong one and read the mismatch, which is transcription by
    /// trial and error across sixty-odd tables still to come.
    ///
    /// **The obvious objection is that this makes the check circular** — a table
    /// pasted from the tool agrees with the tool by construction, so AC2 proves
    /// nothing. It does not bite, for two reasons worth stating rather than
    /// assuming. The `Option`, `Type` and `Default` columns ARE the assembly's
    /// truth by definition: design §6.2 makes the default readable only from an
    /// instantiated object, so a human writing that column is transcribing this
    /// output or guessing. And AC2's subject is DRIFT — today's table against
    /// tomorrow's assembly — which no amount of agreement today can fake.
    ///
    /// What the tool cannot supply is the column that carries meaning. The
    /// description is left empty on purpose: it is AC8's subject, it has no tool
    /// behind it, and a blank cell is the honest prompt to write one.
    /// </summary>
    private static int Describe(IReadOnlyList<string> names)
    {
        if (names.Count == 0)
        {
            Console.Error.WriteLine("optioncheck --describe <fully.qualified.Type> [<type>…]");
            return Unreachable;
        }

        var missing = 0;

        foreach (var name in names)
        {
            var type = Reflect.Resolve(name);
            if (type is null)
            {
                Console.Error.WriteLine($"optioncheck: THE TYPE IS GONE — `{name}` is in none of the "
                                        + $"{Authority.Pinned().Count} pinned packages at {Authority.Pin()}");
                missing++;
                continue;
            }

            var surface = Reflect.Describe(type);

            Console.WriteLine($"<!-- optioncheck: {type.FullName} -->");
            Console.WriteLine();
            Console.WriteLine("| Option | Type | Default | Description |");
            Console.WriteLine("|---|---|---|---|");

            foreach (var member in surface.Members)
                Console.WriteLine($"| `{member.Name}` | `{member.TypeName}` | "
                                  + $"{(member.Default is null ? "" : $"`{member.Default}`")} |  |");

            Console.WriteLine();
            Console.WriteLine($"{Count(surface.Members.Count, "member")}, read from the "
                              + $"{(surface.Route == Reflect.Route.Constructor
                                  ? "widest constructor's parameters"
                                  : "settable properties")} — "
                              + $"`max(props, ctor)` selects that route.");

            foreach (var member in surface.Members.Where(m => m.Unreadable is not null))
                Console.WriteLine($"  `{member.Name}` has no readable default: {member.Unreadable}. "
                                  + $"The table owes `manual: {member.Name} — <why>`");

            if (surface.Error is not null)
                Console.WriteLine($"  the type did not construct: {surface.Error}");

            Console.WriteLine();
        }

        return missing == 0 ? Clean : Findings;
    }

    /// <summary>
    /// Design §6.1's step 2, printed before anything is diffed. The `omit:` and
    /// `manual:` declarations are named individually, not totalled: §5.1 says
    /// both escapes declare and are counted, and a count with no names is a
    /// number nobody can question.
    /// </summary>
    private static void Scope(
        int filesScanned,
        IReadOnlyList<Binding.Marker> markers,
        IReadOnlyList<string> omitted,
        IReadOnlyList<string> manual)
    {
        var types = markers.Select(m => m.TypeName).Distinct(StringComparer.Ordinal).Count();
        var pages = markers.Select(m => m.File).Distinct(StringComparer.Ordinal).Count();

        Console.WriteLine($"optioncheck — Brighter {Authority.Pin()}, "
                          + $"{Authority.Pinned().Count} pinned packages "
                          + "(tools/optioncheck/optioncheck.csproj)");
        Console.WriteLine();

        if (markers.Count == 0)
        {
            Console.WriteLine($"scope: NOTHING. {Count(filesScanned, "file")} scanned and not one "
                              + "`<!-- optioncheck: -->` marker in any of them.");
            Console.WriteLine("       A run that reaches no table checks no default. This is not a pass "
                              + "with nothing to do;");
            Console.WriteLine("       it is a pass with nothing in scope, and the two look identical in a "
                              + "CI log that prints only a verdict.");
            return;
        }

        Console.WriteLine($"scope: {Count(markers.Count, "table")}, "
                          + $"{Count(markers.Sum(m => m.Rows.Count), "row")}, "
                          + $"{Count(types, "type")}, on {Count(pages, "page")} "
                          + $"of {Count(filesScanned, "file")} scanned.");

        Console.WriteLine(omitted.Count == 0
            ? "       0 omit: declarations."
            : $"       {Count(omitted.Count, "omit: declaration")}, each counted rather than silenced:");
        foreach (var line in omitted) Console.WriteLine($"         {line}");

        Console.WriteLine(manual.Count == 0
            ? "       0 manual: declarations."
            : $"       {Count(manual.Count, "manual: declaration")} — "
              + "the residue requirements §7.3 asks to measure rather than estimate:");
        foreach (var line in manual) Console.WriteLine($"         {line}");
    }

    /// <summary>Design §6.1 steps 3 to 6, for one marker.</summary>
    private static void Check(
        Binding.Marker marker, List<string> findings, List<string> omitted, List<string> manual)
    {
        findings.AddRange(marker.Faults);

        var type = Reflect.Resolve(marker.TypeName);
        if (type is null)
        {
            findings.Add($"{marker.File}:{marker.Line}: THE TYPE IS GONE — `{marker.TypeName}` is in none of "
                         + $"the {Authority.Pinned().Count} pinned packages at {Authority.Pin()}. "
                         + "Either it was renamed, or the marker never named a real type");
            return;
        }

        // First one wins, and none of these throws on a duplicate. A type that
        // declares two members of one name, or a marker that names one twice,
        // is a thing to report — not a stack trace that stops the whole run and
        // leaves every other table unchecked.
        var surface = Reflect.Describe(type);
        var byName = First(surface.Members, m => m.Name);
        var omit = First(marker.Omit, e => e.Member);
        var declared = First(marker.Manual, e => e.Member);

        foreach (var escape in marker.Omit)
        {
            omitted.Add($"{marker.File}:{marker.Line} {type.Name}.{escape.Member} — {escape.Reason}");
            if (!byName.ContainsKey(escape.Member))
                findings.Add($"{marker.File}:{marker.Line}: OMIT NAMES NOTHING — `{escape.Member}` is not a "
                             + $"reader-facing member of `{type.Name}`. A stale escape suppresses a row that "
                             + "no longer exists and hides the one that replaced it");
        }

        foreach (var escape in marker.Manual)
        {
            manual.Add($"{marker.File}:{marker.Line} {type.Name}.{escape.Member} — {escape.Reason}");
            if (!byName.ContainsKey(escape.Member))
                findings.Add($"{marker.File}:{marker.Line}: MANUAL NAMES NOTHING — `{escape.Member}` is not a "
                             + $"reader-facing member of `{type.Name}`");
        }

        var rows = new Dictionary<string, Binding.Row>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in marker.Rows)
        {
            if (row.Option.Length == 0)
            {
                findings.Add($"{marker.File}:{row.Line}: EMPTY OPTION CELL");
                continue;
            }

            if (!rows.TryAdd(row.Option, row))
                findings.Add($"{marker.File}:{row.Line}: DUPLICATE ROW — `{row.Option}` is documented twice");
        }

        foreach (var member in surface.Members)
        {
            if (omit.ContainsKey(member.Name)) continue;

            if (!rows.TryGetValue(member.Name, out var row))
            {
                findings.Add($"{marker.File}:{marker.Line}: UNDOCUMENTED — `{type.Name}.{member.Name}` "
                             + $"({member.TypeName}) is reader-facing and the table has no row for it. "
                             + "Add the row, or declare `omit: " + member.Name + " — <why>`");
                continue;
            }

            if (!string.Equals(row.Option, member.Name, StringComparison.Ordinal))
                findings.Add($"{marker.File}:{row.Line}: WRONG SPELLING — the table writes `{row.Option}`; "
                             + $"the reader types `{member.Name}`. Requirements §7.1: on a "
                             + "constructor-driven type the option is the parameter, not the property");

            if (!string.Equals(row.Type, member.TypeName, StringComparison.Ordinal))
                findings.Add($"{marker.File}:{row.Line}: WRONG TYPE — `{member.Name}` is `{member.TypeName}`, "
                             + $"the table says `{row.Type}`");

            if (row.Description.Length == 0)
                findings.Add($"{marker.File}:{row.Line}: EMPTY DESCRIPTION — `{member.Name}`");

            if (row.Default.Length == 0)
            {
                findings.Add($"{marker.File}:{row.Line}: BLANK DEFAULT — `{member.Name}`. A blank cell is "
                             + "indistinguishable from an unfinished row; write the value, or `none`");
                continue;
            }

            if (declared.ContainsKey(member.Name))
            {
                if (member.Unreadable is null)
                    findings.Add($"{marker.File}:{marker.Line}: MANUAL NOT NEEDED — `{member.Name}` is "
                                 + $"declared `manual:` and the tool reads its default as `{member.Default}`. "
                                 + "A `manual:` over a checkable default is an unchecked row");
                continue;
            }

            if (member.Unreadable is not null)
            {
                findings.Add($"{marker.File}:{row.Line}: DEFAULT NOT DETERMINABLE — `{member.Name}`: "
                             + $"{member.Unreadable}. The table says `{row.Default}` and the tool cannot "
                             + $"confirm it. Declare `manual: {member.Name} — <why>`, which counts");
                continue;
            }

            var accepted = Reflect.Accepted(member.Default!, member.TypeName.TrimEnd('?'));
            if (!accepted.Contains(row.Default, StringComparer.Ordinal))
                findings.Add($"{marker.File}:{row.Line}: WRONG DEFAULT — `{member.Name}` is "
                             + $"`{member.Default}` on a constructed instance; the table says "
                             + $"`{row.Default}`");
        }

        foreach (var row in marker.Rows)
        {
            if (row.Option.Length == 0) continue;
            if (byName.ContainsKey(row.Option)) continue;
            if (byName.Keys.Any(n => string.Equals(n, row.Option, StringComparison.OrdinalIgnoreCase))) continue;

            findings.Add($"{marker.File}:{row.Line}: ROW NAMES NOTHING — `{row.Option}` is not a "
                         + $"reader-facing member of `{type.Name}` at {Authority.Pin()}");
        }

        if (surface.Error is not null && surface.Members.All(m => m.Unreadable is not null))
            findings.Add($"{marker.File}:{marker.Line}: CANNOT CONSTRUCT — `{type.Name}`: {surface.Error}. "
                         + "Every default in this table is unchecked");
    }

    /// <summary>
    /// The files a run reaches: `contents/` with no arguments, and exactly what
    /// was named with them. A directory argument is walked; a file argument is
    /// taken as given, so a fixture outside `contents/` is checkable.
    /// </summary>
    private static List<string> Files(string[] args, string? root)
    {
        if (args.Length == 0)
            return Directory.GetFiles(Path.Combine(root!, "contents"), "*.md", SearchOption.AllDirectories)
                .OrderBy(f => f, StringComparer.Ordinal)
                .ToList();

        var files = new List<string>();

        foreach (var arg in args)
        {
            var path = Path.IsPathRooted(arg) ? arg : Path.Combine(Directory.GetCurrentDirectory(), arg);

            if (Directory.Exists(path))
                files.AddRange(Directory.GetFiles(path, "*.md", SearchOption.AllDirectories));
            else if (File.Exists(path))
                files.Add(path);
            else
                Console.Error.WriteLine($"optioncheck: no such file or directory: {arg}");
        }

        return files.Distinct(StringComparer.Ordinal).OrderBy(f => f, StringComparer.Ordinal).ToList();
    }

    /// <summary>
    /// How a file is named in a finding: repository-relative where it is in the
    /// repository, absolute where it is not. A `../../../..` path to `/tmp` is
    /// not a location anyone can click.
    /// </summary>
    private static string Report(string? root, string file)
    {
        if (root is null) return file;

        var relative = Path.GetRelativePath(root, file);
        return relative.StartsWith("..", StringComparison.Ordinal) ? file : relative;
    }

    private static string? RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "SUMMARY.md"))
                && Directory.Exists(Path.Combine(directory.FullName, "contents")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }

    private static Dictionary<string, T> First<T>(IEnumerable<T> items, Func<T, string> key)
    {
        var map = new Dictionary<string, T>(StringComparer.Ordinal);
        foreach (var item in items) map.TryAdd(key(item), item);
        return map;
    }

    private static string Count(int n, string singular, string? plural = null) =>
        $"{n} {(n == 1 ? singular : plural ?? singular + "s")}";
}
