// Values HealthChecks.md names in its blocks and never declares.
//
// Identifiers only: the member is typed from a pinned package and returns a default. Block 1
// elides the `WebApplicationBuilder` as *"Web Application Builder code goes here"*, and every
// call it makes on `builder` is checked against the real type.
//
// blockcheck: using static HealthChecksContext;

using Microsoft.AspNetCore.Builder;

public static class HealthChecksContext
{
    // block 1: `builder.Services.AddHealthChecks()`, `builder.Build()`
    public static WebApplicationBuilder builder => null!;
}
