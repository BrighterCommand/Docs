// Values HowConfiguringTheCommandProcessorWorks.md names in its blocks and never declares.
//
// Identifiers only: every member is typed from a pinned package or the BCL, returns a default,
// and does nothing. A block that calls a member of one of these is checked
// against the real type, so a wrong member or argument still fails.
//
// blockcheck: using static HowConfiguringTheCommandProcessorWorksContext;

using Paramore.Brighter;
using Polly.Registry;

public static class HowConfiguringTheCommandProcessorWorksContext
{
    public static IAmAHandlerFactorySync handlerFactory => null!;
    public static SubscriberRegistry subscriberRegistry => null!;
    public static ResiliencePipelineRegistry<string> resiliencePipelineRegistry => null!;
}
