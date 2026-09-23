// Values FAQ.md names in its blocks and never declares.
//
// Identifiers only: every member is typed from a pinned package or the BCL, returns a default,
// and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static FAQContext;

using System;
using Paramore.Brighter;

public static class FAQContext
{
    public static IAmACommandProcessor commandProcessor => null!;
    public static IRequest command => null!;
    public static TimeSpan delay => default;
    public static IAmARequestSchedulerAsync scheduler => null!;
    public static TimeSpan newDelay => default;
}
