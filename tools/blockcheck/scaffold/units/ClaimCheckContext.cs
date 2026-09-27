// The type ClaimCheck.md names in its blocks and never shows.
//
// Both mapper methods take or return a `GreetingEvent`, the request the page's mapper maps;
// the page shows only the mapper. The stub is a request and nothing more: block 1 reads
// `request.Id`, which `Event` supplies, and no block names a member of its own.

using Paramore.Brighter;

// blocks 1 and 2: `MapToMessage(GreetingEvent request, …)`, `GreetingEvent MapToRequest(…)`
public class GreetingEvent() : Event(Id.Random());
