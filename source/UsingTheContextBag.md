# Passing information between Handlers in the Pipeline

A key constraint of the Pipes and Filters architectural style is that
Filters do not share state. One reason is that this limits your ability
to recompose the pipeline as steps must follow other steps.

However, when dealing with Handlers that implement orthogonal concerns
it can be useful to pass context along the chain. Given that many
orthogonal concerns have constraints about ordering anyway, we can live
with the ordering constraints imposed by passing context. So how do you
approach passing context from one Handler to another when it is
necessary?

The first thing is to avoid adding extra properties to the Command to
support handling state for these orthogonal Filter steps in your
pipeline. This couples your **Command** to orthogonal concerns and you
really only want to bind it to your **Target Handler**.

Instead we provide a **Context Bag** as part of the Command Dispatcher
which is injected into each Handler in the Pipeline. The lifetime of
this **Context Bag** is the lifetime of the Request (although you will
need to take responsibility for freeing any unmanaged resources you
place into the **Context Bag** for example when code called after the
Handler that inserts the resource into the Bag returns to the Handler).

``` csharp
public class MyContextAwareCommandHandler : RequestHandler<MyCommand>
{
    public static string TestString { get; set; }

    public override MyCommand Handle(MyCommand command)
    {
        LogContext();
        return base.Handle(command);
    }

    private void LogContext()
    {
        TestString = (string)Context.Bag["TestString"];
        Context.Bag["MyContextAwareCommandHandler"] = "I was called and set the context";
    }
}
```

Internally we use the **Context Bag** in a number of the Quality of
Service supporting Attributes we provide. See
[Fallback](PolicyFallback.html) for example.
