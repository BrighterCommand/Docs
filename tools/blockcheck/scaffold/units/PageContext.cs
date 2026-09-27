// Values DapperOutbox.md names in its blocks and never declares.
//
// Identifiers only: every member is typed from the BCL and does nothing. It
// supplies what block 1 passes to `new RelationalDatabaseConfiguration(...)`,
// which the page elides as a connection string the reader already has.
//
// blockcheck: using static PageContext;

public static class PageContext
{
    // DapperOutbox.md block 1: `connectionString,`
    public static string connectionString => "Server=localhost;Database=Greetings;";
}
