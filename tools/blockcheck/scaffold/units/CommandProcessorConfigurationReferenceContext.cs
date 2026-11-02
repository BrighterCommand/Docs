// Types and values CommandProcessorConfigurationReference.md names in its blocks and never
// declares.
//
// The page configures Brighter for the reader's own Greetings application: a command, an event,
// two helpers that build subscriptions and a channel factory, and the request context factories
// its tests substitute. Every value member is typed from a pinned package or the BCL and returns a
// default; a stub is a request, a context or a factory with only the members a block names.
//
// blockcheck: using static CommandProcessorConfigurationReferenceContext;

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Paramore.Brighter;

public static class CommandProcessorConfigurationReferenceContext
{
    // block 12: `builder.Services`
    public static WebApplicationBuilder builder => null!;
    // block 18: `new RelationalDatabaseConfiguration(DbConnectionString())`
    public static string DbConnectionString() => "Server=localhost;Database=Greetings;";
    // block 8: `Subscriptions = GetSubscriptions(sp)`
    public static Subscription[] GetSubscriptions(IServiceProvider sp) => null!;
    // block 8: `DefaultChannelFactory = CreateChannelFactory(sp, configuration)`
    public static IAmAChannelFactory CreateChannelFactory(IServiceProvider sp, IConfiguration configuration) => null!;
}

// block 3: `RequestHandler<MyCommand>`
public class MyCommand() : Command(Id.Random());

// blocks 7, 16, 22: `typeof(GreetingMade)`, `JsonSerializer.Deserialize<GreetingMade>(…)`
public class GreetingMade() : Event(Id.Random());

// block 18: `AddDbContext<GreetingsEntityGateway>`, `MySqlEntityFrameworkTransactionProvider<GreetingsEntityGateway>`
public class GreetingsEntityGateway(DbContextOptions<GreetingsEntityGateway> options) : DbContext(options);

// block 6: `AddSingleton<IAmARequestContextFactory, MyCustomContextFactory>()`
public class MyCustomContextFactory : InMemoryRequestContextFactory;

// block 9: `options.RequestContextFactory = new TestRequestContextFactory();`
public class TestRequestContextFactory : InMemoryRequestContextFactory;

// block 11: `new TestContextFactoryA()`, `new TestContextFactoryB()`
public class TestContextFactoryA : InMemoryRequestContextFactory;
public class TestContextFactoryB : InMemoryRequestContextFactory;

// block 14: `typeof(MyRequestHandlerAsync).Assembly`
public class MyRequestHandlerAsync : RequestHandlerAsync<MyCommand>;

// block 10: `new DebugRequestContextFactory()`
public class DebugRequestContextFactory : InMemoryRequestContextFactory;
