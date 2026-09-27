// The type TutorialStreamingWithKafka.md names in its blocks and never shows.
//
// `GreetingEvent` is rung 2's, written by the reader in TutorialFirstMessage.md block 1;
// this page keeps it "unchanged" and shows only its uses. The stub carries what those uses
// need and nothing else: the base type, so it is a request, and the constructor block 1
// calls. It has no `Greeting` property because no block on this page names one.

using Paramore.Brighter;

namespace Greetings
{
    // TutorialStreamingWithKafka.md, block 1: `new GreetingEvent($"Hello {recipient} #{i}")`
    public class GreetingEvent : Event
    {
        public GreetingEvent(string greeting) : base(Id.Random()) { }
    }
}
