// The type Routing.md names in its blocks and never shows.
//
// Block 1's mapper maps a `TaskCompletedEvent`, the reader's own event. The stub is a request and
// nothing more: the block reads `request.Id`, which `Event` supplies, and no block names a member
// of its own.

using Paramore.Brighter;

// block 1: `IAmAMessageMapper<TaskCompletedEvent>`, `request.Id`
public class TaskCompletedEvent() : Event(Id.Random());
