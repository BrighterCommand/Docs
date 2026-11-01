// Task 7.5. Page code is copied as the page shows it; the probe's own lines are marked // probe.
//  OutboxArchiver.md #4: Archive(TimeSpan.FromDays(1)) moves a message dispatched two days ago into ArchivedMessages,
//      keyed by its Id, and removes it from the Outbox. Control: a message dispatched an hour ago, and one never
//      dispatched, which stay in the Outbox and are not archived.
//  OutboxArchiver.md #3: CommittableTransaction is the InMemory Outbox's transaction type, so the archiver
//      UseOutboxArchiver registers works over it. Case: #3's registration, the archiver resolved and run. Control: the
//      same with UseOutboxArchiver<System.Data.IDbTransaction>, which the InMemory Outbox does not implement.
//  SweeperCircuitBreaking.md #1: CooldownCount defaults to 10. Case: new OutboxCircuitBreakerOptions(). Control: set to 3.
using System;
using System.Linq;
using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;
using Paramore.Brighter.CircuitBreaker;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.Observability;
using Paramore.Brighter.Outbox.Hosting;

public static class Program
{
    static bool failed;
    static void Row(string name, bool ok, string detail) { failed |= !ok; Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {detail}"); }

    static Message NewMessage() => new(new MessageHeader(Id.Random(), new RoutingKey("orders"), MessageType.MT_EVENT),
        new MessageBody("{\"OrderId\":\"ORD-1\"}"));   // probe

    public static int Main()
    {
        // #4
        {
            var outbox = new InMemoryOutbox(TimeProvider.System);
            var archiveProvider = new InMemoryArchiveProvider();
            var archiver = new OutboxArchiver<Message, CommittableTransaction>(outbox, archiveProvider);

            // A message that was dispatched two days ago
            var message = new Message(
                new MessageHeader(Id.Random(), new RoutingKey("orders"), MessageType.MT_EVENT),
                new MessageBody("{\"OrderId\":\"ORD-1\"}"));
            var context = new RequestContext();
            outbox.Add(message, context);
            outbox.MarkDispatched(message.Id, context, DateTimeOffset.UtcNow.AddDays(-2));

            var recent = NewMessage(); outbox.Add(recent, context); outbox.MarkDispatched(recent.Id, context, DateTimeOffset.UtcNow.AddHours(-1));   // probe: control
            var never = NewMessage(); outbox.Add(never, context);   // probe: control

            // Archive every message dispatched more than a day ago
            archiver.Archive(TimeSpan.FromDays(1));

            // The Archiver moved the message out of the Outbox and into the archive provider
            bool archived = archiveProvider.ArchivedMessages.ContainsKey(message.Id.Value);   // true

            var stillInOutbox = outbox.Get(message.Id, context).Header.MessageType != MessageType.MT_NONE;
            var recentArchived = archiveProvider.ArchivedMessages.ContainsKey(recent.Id.Value);
            var neverArchived = archiveProvider.ArchivedMessages.ContainsKey(never.Id.Value);
            var recentKept = outbox.Get(recent.Id, context).Header.MessageType != MessageType.MT_NONE;
            var neverKept = outbox.Get(never.Id, context).Header.MessageType != MessageType.MT_NONE;
            Row("OutboxArchiver.md #4, dispatched two days ago", archived && !stillInOutbox,
                $"archived {archived}, keyed by Id; still in the Outbox {stillInOutbox}; {archiveProvider.ArchivedMessages.Count} archived in all");
            Console.WriteLine($"     control, dispatched an hour ago: archived {recentArchived}, kept {recentKept}; never dispatched: archived {neverArchived}, kept {neverKept}");
            if (recentArchived || neverArchived || !recentKept || !neverKept) failed = true;
        }

        // #3
        foreach (var right in new[] { true, false })
        {
            var services = new ServiceCollection();   // probe
            var internalBus = new InternalBus();   // probe
            var builder = services.AddBrighter(options =>
            {
                options.HandlerLifetime = ServiceLifetime.Scoped;
            });
            builder = right
                ? builder.UseOutboxArchiver<CommittableTransaction>(new InMemoryArchiveProvider())  // the InMemory Outbox's transaction type
                : builder.UseOutboxArchiver<System.Data.IDbTransaction>(new InMemoryArchiveProvider());   // probe: control
            builder.AddProducers(options =>
            {
                // ... the producer registry for your bus
                options.ProducerRegistry = new InMemoryProducerRegistryFactory(internalBus,
                    [new Publication { Topic = new RoutingKey("orders") }], InstrumentationOptions.All).Create();   // probe
                options.Outbox = new InMemoryOutbox(TimeProvider.System);
            });

            var sp = services.BuildServiceProvider();
            var outbox = (InMemoryOutbox)sp.GetRequiredService<IAmAnOutbox>();
            var m = NewMessage(); var ctx = new RequestContext();
            outbox.Add(m, ctx); outbox.MarkDispatched(m.Id, ctx, DateTimeOffset.UtcNow.AddDays(-2));
            string outcome;
            int archivedCount;
            try
            {
                if (right)
                    sp.GetRequiredService<OutboxArchiver<Message, CommittableTransaction>>().Archive(TimeSpan.FromDays(1));
                else
                    sp.GetRequiredService<OutboxArchiver<Message, System.Data.IDbTransaction>>().Archive(TimeSpan.FromDays(1));
                outcome = "archived without error";
            }
            catch (Exception e) { outcome = $"{e.GetType().Name}: {e.Message}"; }
            archivedCount = ((InMemoryArchiveProvider)sp.GetRequiredService<IAmAnArchiveProvider>()).ArchivedMessages.Count;
            if (right) Row("OutboxArchiver.md #3, UseOutboxArchiver<CommittableTransaction>", archivedCount == 1, $"{outcome}; {archivedCount} archived");
            else Console.WriteLine($"     control, UseOutboxArchiver<IDbTransaction>: {outcome}; {archivedCount} archived");
        }

        // SweeperCircuitBreaking.md #1
        {
            var d = new OutboxCircuitBreakerOptions().CooldownCount;
            var c = new OutboxCircuitBreakerOptions { CooldownCount = 3 }.CooldownCount;
            Row("SweeperCircuitBreaking.md #1, CooldownCount default", d == 10, $"{d}; control, set to 3: {c}");
        }
        return failed ? 1 : 0;
    }
}
