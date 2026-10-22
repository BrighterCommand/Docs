// The type MessageTransforms.md names in its blocks and never shows.
//
// The claim-check blocks decorate the mapper of the reader's own `GreetingEvent`. The stub is a
// request and nothing more: the blocks read `request.Id`, which `Event` supplies, and no block
// names a member of its own.

using Paramore.Brighter;

// blocks 2, 3, 6: `MapToMessage(GreetingEvent request, …)`, `GreetingEvent MapToRequest(…)`
public class GreetingEvent() : Event(Id.Random());
