// Values HealthChecks.md names in its blocks and never declares.
//
// Identifiers only: the member is typed from a pinned package and returns a default. Block 1
// elides the `WebApplicationBuilder` as *"Web Application Builder code goes here"*, and every
// call it makes on `builder` is checked against the real type. Block 5, the migration's
// exclusion predicate, elides the `WebApplication` as *"app is the WebApplication you built"*.
//
// blockcheck: using static HealthChecksContext;

using Microsoft.AspNetCore.Builder;

public static class HealthChecksContext
{
    // block 1: `builder.Services.AddHealthChecks()`, `builder.Build()`
    public static WebApplicationBuilder builder => null!;

    // block 5: `app.MapHealthChecks(...)`
    public static WebApplication app => null!;
}
