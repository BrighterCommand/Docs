// Types QueryObjectValidation.md names in its blocks and never shows.
//
// The page validates queries whose results, repository and exception belong to a domain it never
// shows. Each stub carries only the members a block names. `Order` is read as a type the page never
// shows (spec 017 task 1.10), never as `StackExchange.Redis.Order`.

using System;
using System.Threading;
using System.Threading.Tasks;

// block 1: `IQuery<PagedResult<Order>>`
public class PagedResult<T> { }

// block 1: `PagedResult<Order>`
public class Order { }

// block 2: `IQuery<IReadOnlyList<Product>>`
public class Product { }

// block 3: `IQuery<User>`, `Task<User>`
public class User { }

// block 3: `_repository.FindByEmailAsync(query.Email, cancellationToken)`
public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
}

// block 3: `new UserNotFoundException($"User with email {query.Email} not found")`
public class UserNotFoundException(string message) : Exception(message);
