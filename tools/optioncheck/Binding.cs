using System.Text.RegularExpressions;

namespace Paramore.Docs.OptionCheck;

/// <summary>
/// Task 2.2 — the marker and its two keys (design §5, §5.1).
///
/// A checker reading markdown has no way to know which .NET type a table
/// describes, and inferring it from the heading fails as *a table nobody
/// checked, reported as a table that passed*. So the binding is explicit:
///
///   &lt;!-- optioncheck: Paramore.Brighter.MessagingGateway.Kafka.KafkaSubscription
///        omit: channelFactory — not reader-set; supplied by AddConsumers
///        manual: sweepInterval — default applied by the Dispatcher, not the type
///   --&gt;
///
/// BOTH ESCAPES DECLARE RATHER THAN SILENCE, AND BOTH ARE COUNTED. That is
/// `pagelint.py` rule 6's `// ...` applied to a second tool. A parser that
/// accepted `omit:` with no reason would let 012 reach a green build by writing
/// `omit:` over the hard half of every table, so a reasonless escape is a fault
/// here rather than an escape.
/// </summary>
internal static class Binding
{
    /// <summary>A member deliberately absent from the table, or one whose default the tool cannot determine.</summary>
    internal sealed record Escape(string Member, string Reason);

    /// <summary>One row of a §7.1 table, as written.</summary>
    internal sealed record Row(string Option, string Type, string Default, string Description, int Line);

    /// <summary>One marker, the table beneath it, and anything malformed about either.</summary>
    internal sealed record Marker(
        string File,
        int Line,
        string TypeName,
        IReadOnlyList<Escape> Omit,
        IReadOnlyList<Escape> Manual,
        IReadOnlyList<Row> Rows,
        IReadOnlyList<string> Faults);

    private static readonly Regex Open = new(@"^\s*<!--\s*optioncheck:\s*(?<type>[^\s>]+)\s*(?<close>-->)?\s*$",
        RegexOptions.Compiled);

    private static readonly Regex Key = new(@"^\s*(?<key>omit|manual):\s*(?<body>.*?)\s*(?<close>-->)?\s*$",
        RegexOptions.Compiled);

    /// <summary>The four columns requirements §7.1 fixes, in one order.</summary>
    private static readonly string[] Columns = ["Option", "Type", "Default", "Description"];

    public static IReadOnlyList<Marker> Parse(string path, string relative)
    {
        var lines = File.ReadAllLines(path);
        var markers = new List<Marker>();

        for (var i = 0; i < lines.Length; i++)
        {
            var open = Open.Match(lines[i]);
            if (!open.Success) continue;

            var faults = new List<string>();
            var omit = new List<Escape>();
            var manual = new List<Escape>();
            var markerLine = i + 1;
            var closed = open.Groups["close"].Success;
            var cursor = i + 1;

            while (!closed && cursor < lines.Length)
            {
                var line = lines[cursor];

                if (line.TrimStart().StartsWith("-->", StringComparison.Ordinal))
                {
                    closed = true;
                    cursor++;
                    break;
                }

                var key = Key.Match(line);
                if (!key.Success)
                {
                    faults.Add($"{relative}:{cursor + 1}: MARKER MALFORMED — "
                               + $"expected `omit:`, `manual:` or `-->`, found `{line.Trim()}`");
                    cursor++;
                    continue;
                }

                var escape = ReadEscape(key.Groups["body"].Value, relative, cursor + 1, faults);
                if (escape is not null)
                    (key.Groups["key"].Value == "omit" ? omit : manual).Add(escape);

                if (key.Groups["close"].Success) closed = true;
                cursor++;
            }

            if (!closed)
            {
                faults.Add($"{relative}:{markerLine}: MARKER UNTERMINATED — no `-->`");
                markers.Add(new Marker(relative, markerLine, open.Groups["type"].Value, omit, manual, [], faults));
                continue;
            }

            var rows = ReadTable(lines, ref cursor, relative, markerLine, faults);
            markers.Add(new Marker(relative, markerLine, open.Groups["type"].Value, omit, manual, rows, faults));
            i = cursor - 1;
        }

        return markers;
    }

    /// <summary>
    /// `member — reason`. The reason is required: an escape with no reason is a
    /// silence wearing a declaration's clothes, and §5.1 is explicit that both
    /// keys declare and are counted.
    /// </summary>
    private static Escape? ReadEscape(string body, string file, int line, List<string> faults)
    {
        var parts = body.Split(['—', '-', ':'], 2);
        var member = parts[0].Trim();
        var reason = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        if (member.Length == 0)
        {
            faults.Add($"{file}:{line}: MARKER MALFORMED — an escape with no member name");
            return null;
        }

        if (reason.Length == 0)
        {
            faults.Add($"{file}:{line}: ESCAPE WITHOUT REASON — `{member}` declares nothing. "
                       + $"Write `{member} — <why>`; an escape with no reason is a silent exemption");
            return null;
        }

        return new Escape(member, reason);
    }

    /// <summary>
    /// The table must be the next thing after the marker. Design §5 puts the
    /// comment "immediately above the table", and a marker floating above prose
    /// is a table nobody checks.
    /// </summary>
    private static IReadOnlyList<Row> ReadTable(
        string[] lines, ref int cursor, string file, int markerLine, List<string> faults)
    {
        while (cursor < lines.Length && lines[cursor].Trim().Length == 0) cursor++;

        if (cursor >= lines.Length || !lines[cursor].TrimStart().StartsWith('|'))
        {
            faults.Add($"{file}:{markerLine}: NO TABLE — the marker names a type but nothing follows it. "
                       + "The marker binds the table immediately below it");
            return [];
        }

        var header = Cells(lines[cursor]);
        if (!header.SequenceEqual(Columns))
        {
            faults.Add($"{file}:{cursor + 1}: WRONG COLUMNS — expected "
                       + $"`{string.Join(" | ", Columns)}`, found `{string.Join(" | ", header)}`");
        }

        cursor++;
        if (cursor < lines.Length && IsSeparator(lines[cursor])) cursor++;

        var rows = new List<Row>();
        while (cursor < lines.Length && lines[cursor].TrimStart().StartsWith('|'))
        {
            var cells = Cells(lines[cursor]);
            if (cells.Count < 4)
            {
                faults.Add($"{file}:{cursor + 1}: SHORT ROW — {cells.Count} cells, expected 4");
                cursor++;
                continue;
            }

            rows.Add(new Row(cells[0], cells[1], cells[2], cells[3], cursor + 1));
            cursor++;
        }

        if (rows.Count == 0)
            faults.Add($"{file}:{markerLine}: EMPTY TABLE — a header and no rows");

        return rows;
    }

    private static bool IsSeparator(string line) =>
        line.TrimStart().StartsWith('|') && line.Replace("|", "").Replace("-", "").Replace(":", "").Trim().Length == 0;

    /// <summary>
    /// The cells as a reader sees them: `**CooldownCount**` and `` `CooldownCount` ``
    /// are the same option, written by two authors. Presentation is not the
    /// subject — the spelling is.
    /// </summary>
    private static List<string> Cells(string line)
    {
        var trimmed = line.Trim();
        if (trimmed.StartsWith('|')) trimmed = trimmed[1..];
        if (trimmed.EndsWith('|')) trimmed = trimmed[..^1];

        return trimmed.Split('|').Select(Clean).ToList();
    }

    public static string Clean(string cell) =>
        cell.Replace("`", "").Replace("**", "").Replace("*", "").Trim();
}
