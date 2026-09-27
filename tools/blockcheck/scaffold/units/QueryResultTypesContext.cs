// Types QueryResultTypes.md names in its blocks and never shows.
//
// The page is about the shape of a query's result, so its queries return domain types it never
// declares. Each is an empty stub: no block names a member of one. `Order` is read as a type the
// page never shows (spec 017 task 1.10), never as `StackExchange.Redis.Order`.

// blocks 3, 5: `IQuery<List<Customer>>`, `IQuery<Customer?>`
public class Customer { }

// block 3: `IQuery<IReadOnlyList<OrderSummary>>`
public class OrderSummary { }

// block 3: `IQuery<IEnumerable<DataRow>>`
public class DataRow { }

// block 3: `IQuery<Product[]>`
public class Product { }

// block 6: `IQuery<PagedResult<Order>>`
public class Order { }
