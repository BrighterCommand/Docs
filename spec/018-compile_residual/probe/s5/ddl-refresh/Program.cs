// Task 4.5b (D20). Claim: the DDL on MSSQLOutbox.md, MySQLOutbox.md, PostgresOutbox.md,
// SpannerOutbox.md, SqliteOutbox.md and SpannerInbox.md is what GetDDL generates. Case: 10.8.0's
// output, created on its database, has a CausationId column. Control: 10.7.0's, which the pages
// show, created the same way, has none. `gen` writes the text the pages are compared against.
using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using Google.Cloud.Spanner.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using Paramore.Brighter.Inbox.Spanner;
using Paramore.Brighter.Outbox.MsSql;
using Paramore.Brighter.Outbox.MySql;
using Paramore.Brighter.Outbox.PostgreSql;
using Paramore.Brighter.Outbox.Spanner;
using Paramore.Brighter.Outbox.Sqlite;

var version = typeof(SqlOutboxBuilder).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
    .Cast<System.Reflection.AssemblyInformationalVersionAttribute>().First().InformationalVersion.Split('+')[0];

if (args[0] == "gen")
{
    Directory.CreateDirectory(args[1]);
    File.WriteAllText(Path.Combine(args[1], $"mssql-{version}.sql"), SqlOutboxBuilder.GetDDL("Outbox"));
    File.WriteAllText(Path.Combine(args[1], $"mysql-{version}.sql"), MySqlOutboxBuilder.GetDDL("Outbox"));
    File.WriteAllText(Path.Combine(args[1], $"postgres-{version}.sql"), PostgreSqlOutboxBuilder.GetDDL("Outbox"));
    File.WriteAllText(Path.Combine(args[1], $"sqlite-{version}.sql"), SqliteOutboxBuilder.GetDDL("Outbox"));
    File.WriteAllText(Path.Combine(args[1], $"spanner-outbox-{version}.sql"), SpannerOutboxBuilder.GetDDL("Outbox"));
    File.WriteAllText(Path.Combine(args[1], $"spanner-inbox-{version}.sql"), SpannerInboxBuilder.GetDDL("Inbox"));
    if (typeof(SpannerOutboxBuilder).GetMethod("GetCausationIndexDDL")?.Invoke(null, ["Outbox"]) is string ix)
        File.WriteAllText(Path.Combine(args[1], $"spanner-outbox-index-{version}.sql"), ix);
    Console.WriteLine($"{version}: written to {args[1]}");
    return 0;
}

var t = "ddl" + Guid.NewGuid().ToString("N")[..8];
Environment.SetEnvironmentVariable("SPANNER_EMULATOR_HOST", "localhost:9010");
const string spanner = "Data Source=projects/brighter-tests/instances/brighter-spanner/databases/brightertests;EmulatorDetection=EmulatorOnly";
var sqlitePath = Path.Combine(Path.GetTempPath(), t + ".db");
var failed = 0;

Run("mssql", () => new SqlConnection("Server=127.0.0.1,11433;Database=BrighterTests;User Id=sa;Password=Password123!;Encrypt=false"),
    SqlOutboxBuilder.GetDDL(t), $"SELECT CausationId FROM {t} WHERE 1=0", $"DROP TABLE {t}");
Run("mysql", () => new MySqlConnection("Server=localhost;Uid=root;Pwd=root;Database=BrighterTests"),
    MySqlOutboxBuilder.GetDDL(t), $"SELECT CausationId FROM {t} WHERE 1=0", $"DROP TABLE {t}");
Run("postgres", () => new NpgsqlConnection("Host=localhost;Username=postgres;Password=password;Database=brightertests"),
    PostgreSqlOutboxBuilder.GetDDL(t), $"SELECT causationid FROM \"{t}\" WHERE 1=0", $"DROP TABLE \"{t}\"");
Run("sqlite", () => new SqliteConnection($"Data Source={sqlitePath}"),
    SqliteOutboxBuilder.GetDDL(t), $"SELECT CausationId FROM {t} WHERE 1=0", $"DROP TABLE {t}");
// The index is a separate builder method at 10.8.0; read by reflection so 10.7.0 still builds
var indexDdl = typeof(SpannerOutboxBuilder).GetMethod("GetCausationIndexDDL")?.Invoke(null, [t]) as string;
Console.WriteLine($"{version} spanner-outbox: GetCausationIndexDDL {(indexDdl is null ? "absent" : "present")}");
RunSpanner("spanner-outbox", SpannerOutboxBuilder.GetDDL(t) + (indexDdl is null ? "" : ";" + indexDdl), t);
RunSpanner("spanner-inbox", SpannerInboxBuilder.GetDDL(t + "i"), t + "i");
File.Delete(sqlitePath);
return failed == 0 ? 0 : 1;

void Run(string name, Func<DbConnection> open, string ddl, string probe, string drop)
{
    using var c = open(); c.Open();
    try
    {
        Exec(c, ddl);
        Console.WriteLine($"{version} {name}: created; CausationId {(Has(c, probe) ? "present" : "absent")}");
    }
    catch (Exception e) { failed++; Console.WriteLine($"FAIL {version} {name}: {e.GetType().Name} {e.Message.Split('\n')[0]}"); }
    finally { try { Exec(c, drop); } catch { } }
}

void RunSpanner(string name, string ddl, string table)
{
    using var c = new SpannerConnection(spanner); c.Open();
    try
    {
        foreach (var s in ddl.Split(';').Select(s => s.Trim()).Where(s => s.Length > 0))
            c.CreateDdlCommand(s).ExecuteNonQuery();
        Console.WriteLine($"{version} {name}: created; CausationId {(Has(c, $"SELECT CausationId FROM `{table}` WHERE 1=0") ? "present" : "absent")}");
    }
    catch (Exception e) { failed++; Console.WriteLine($"FAIL {version} {name}: {e.GetType().Name} {e.Message.Split('\n')[0]}"); }
    finally
    {
        foreach (var idx in new[] { $"idx_{table}_CausationId", $"idx_{table.ToLowerInvariant()}_causationid" })
            try { c.CreateDdlCommand($"DROP INDEX `{idx}`").ExecuteNonQuery(); } catch { }
        try { c.CreateDdlCommand($"DROP TABLE `{table}`").ExecuteNonQuery(); } catch { }
    }
}

static void Exec(DbConnection c, string sql) { using var cmd = c.CreateCommand(); cmd.CommandText = sql; cmd.ExecuteNonQuery(); }
static bool Has(DbConnection c, string sql)
{
    try { using var cmd = c.CreateCommand(); cmd.CommandText = sql; using var r = cmd.ExecuteReader(); r.Read(); return true; }  // Spanner reads lazily: an absent column throws only at Read()
    catch { return false; }
}
