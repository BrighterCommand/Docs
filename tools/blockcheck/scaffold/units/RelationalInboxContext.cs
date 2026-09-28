// Values the four relational Inbox pages name in their blocks and never declare: MSSQLInbox.md,
// MySQLInbox.md, PostgresInbox.md and SqliteInbox.md, which repeat one block per backend.
//
// Every member is typed from the BCL, returns a default, and does nothing.
//
// blockcheck: using static RelationalInboxContext;

public static class RelationalInboxContext
{
    // block 1: `new RelationalDatabaseConfiguration(connectionString, …)` — the application's own
    public static string connectionString => string.Empty;
}
