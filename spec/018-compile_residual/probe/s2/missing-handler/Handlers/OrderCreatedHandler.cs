using Contracts;
using Paramore.Brighter;

namespace Handlers;

// The page's handler "in a separate assembly"
public class OrderCreatedHandler : RequestHandlerAsync<OrderCreated>;
