// The value BrighterControlAPI.md names in its block and never declares.
//
// The page maps the Control API's endpoints on the reader's ASP.NET Core app and never shows the
// app being built. The member is typed from a pinned package and returns a default, so the call
// the block makes on it is checked against the real type.
//
// blockcheck: using static BrighterControlAPIContext;

using Microsoft.AspNetCore.Builder;

public static class BrighterControlAPIContext
{
    // block 1: `app.MapBrighterControlEndpoints();`
    public static WebApplication app => null!;
}
