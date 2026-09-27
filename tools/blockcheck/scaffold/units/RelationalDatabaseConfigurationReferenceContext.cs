// Values RelationalDatabaseConfigurationReference.md names in its blocks and never declares.
//
// Identifiers only: the member is typed from the BCL and does nothing. It supplies the
// connection string block 1 passes to `new RelationalDatabaseConfiguration(...)`, which the
// page leaves to the reader's own configuration.
//
// blockcheck: using static RelationalDatabaseConfigurationReferenceContext;

public static class RelationalDatabaseConfigurationReferenceContext
{
    // block 1: `connectionString: DbConnectionString(),`
    public static string DbConnectionString() => "Host=localhost;Database=Greetings;";
}
