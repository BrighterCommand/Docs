using System;
using MySqlConnector;
using Paramore.Brighter.Inbox.MySql;
using Paramore.Brighter.Outbox.MySql;

// usage: dotnet run -- "<connection string with Database=probe>"
var connectionString = args[0];
using var conn = new MySqlConnection(connectionString);
conn.Open();

void Exec(string sql) { using var c = conn.CreateCommand(); c.CommandText = sql; c.ExecuteNonQuery(); }
object? Scalar(string sql) { using var c = conn.CreateCommand(); c.CommandText = sql; return c.ExecuteScalar(); }

foreach (var (kind, table, existsSql, ddl) in new[]
{
    ("Inbox", "ProbeInbox", MySqlInboxBuilder.GetExistsQuery(conn.Database, "ProbeInbox"), MySqlInboxBuilder.GetDDL("ProbeInbox")),
    ("Outbox", "ProbeOutbox", MySqlOutboxBuilder.GetExistsQuery(conn.Database, "ProbeOutbox"), MySqlOutboxBuilder.GetDDL("ProbeOutbox")),
})
{
    Exec($"DROP TABLE IF EXISTS `{table}`");

    var absent = Scalar(existsSql);
    Console.WriteLine($"{kind} table ABSENT : ExecuteScalar() = {absent ?? "null"} ({absent?.GetType().Name}); " +
                      $"page's old check (!= null) says exists = {absent != null}; corrected check (== 1) says exists = {Convert.ToInt32(absent) == 1}");

    Exec(ddl);
    var present = Scalar(existsSql);
    Console.WriteLine($"{kind} table PRESENT: ExecuteScalar() = {present ?? "null"} ({present?.GetType().Name}); " +
                      $"page's old check (!= null) says exists = {present != null}; corrected check (== 1) says exists = {Convert.ToInt32(present) == 1}");
    Exec($"DROP TABLE IF EXISTS `{table}`");
}
