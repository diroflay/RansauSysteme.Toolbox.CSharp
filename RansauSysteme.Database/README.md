# RansauSysteme.Database

A flexible .NET data access library implementing the Repository pattern for database operations.

## Features

- Generic repository implementation with full CRUD operations
- Support for MySQL databases with extensible design for other providers
- Both synchronous and asynchronous API
- Efficient batch operations with transaction support
- Built-in caching repository with configurable refresh intervals
- Factory pattern for creating repositories with dependency injection support
- Comprehensive error handling with custom exceptions
- Thread-safe implementation for concurrent applications

## Installation

```bash
dotnet add package RansauSysteme.Database
```

Or via the NuGet Package Manager:

```
PM> Install-Package RansauSysteme.Database
```

## Quick Start

### Basic Usage

```csharp
// 1. Create your entity class
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// 2. Set up the database connection
var logger = serviceProvider.GetRequiredService<ILogger<MySqlConnection>>();
var dbConnection = new MySqlConnection(logger)
{
    DatabaseConfiguration = new DatabaseConfiguration
    {
        Server = "localhost",
        Database = "mydatabase",
        Username = "user",
        Password = "password",
        Port = 3306,
        ConnectionTimeout = 30
    }
};

// 3. Create a repository
var repository = new BaseRepository<User>(dbConnection);

// 4. Perform operations
// Add a user
var user = new User { Name = "John Doe", Email = "john@example.com" };
int newId = repository.Add(user);

// Get a user
var retrievedUser = repository.GetById(newId);

// Update a user
retrievedUser.Name = "Jane Doe";
repository.Update(retrievedUser);

// Delete a user
repository.Delete(newId);
```

### Dependency Injection Setup

```csharp
// In Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Register database connection
    services.AddSingleton<IDatabaseConnection>(provider =>
    {
        var logger = provider.GetRequiredService<ILogger<MySqlConnection>>();
        var config = provider.GetRequiredService<IConfiguration>();
        
        return new MySqlConnection(logger)
        {
            DatabaseConfiguration = new DatabaseConfiguration
            {
                Server = config["Database:Server"],
                Database = config["Database:Name"],
                Username = config["Database:Username"],
                Password = config["Database:Password"],
                Port = int.Parse(config["Database:Port"]),
                ConnectionTimeout = int.Parse(config["Database:Timeout"])
            }
        };
    });
    
    // Register repository factory
    services.AddSingleton<IRepositoryFactory, BaseRepositoryFactory>();
    
    // Register repositories
    services.AddScoped<IRepository<User>>(provider =>
    {
        var factory = provider.GetRequiredService<IRepositoryFactory>();
        return factory.CreateRepository<User>();
    });
}
```

### Using Cached Repositories

```csharp
// Create a cached repository with a 5-minute refresh interval
var cachedRepository = new CacheRepository<User>(
    dbConnection, 
    refreshInterval: TimeSpan.FromMinutes(5),
    periodicRefresh: true
);

// Use it just like a regular repository
var users = cachedRepository.GetAll();

// Manually refresh the cache if needed
cachedRepository.RefreshCache();

// Or clear the cache
cachedRepository.InvalidateCache();
```

### Using Repository Factory

```csharp
// Create a factory
var factory = new BaseRepositoryFactory(dbConnection);

// Get a repository for User entity
var userRepository = factory.CreateRepository<User>();

// Get a repository for another entity
var productRepository = factory.CreateRepository<Product>();
```

## Working with Transactions

For operations that need to run in a transaction:

```csharp
// Add multiple items in a transaction
var users = new List<User>
{
    new User { Name = "Alice", Email = "alice@example.com" },
    new User { Name = "Bob", Email = "bob@example.com" }
};

bool success = repository.Add(users);
```

## Advanced Features

### Executing Custom SQL

```csharp
var baseRepo = repository as BaseRepository<User>;

// Execute non-query
int affectedRows = baseRepo.Execute(
    "UPDATE users SET active = @Active WHERE last_login < @Date",
    new { Active = false, Date = DateTime.Now.AddYears(-1) }
);

// Execute scalar query
int count = baseRepo.ExecuteScalar<int>(
    "SELECT COUNT(*) FROM users WHERE active = @Active",
    new { Active = true }
);

// Execute batch commands in a transaction
var commands = new List<(string Sql, object? Parameters)>
{
    ("INSERT INTO audit_log (action, user_id) VALUES (@Action, @UserId)", 
     new { Action = "login", UserId = 1 }),
    ("UPDATE users SET last_login = @Now WHERE id = @UserId", 
     new { Now = DateTime.Now, UserId = 1 })
};

bool batchSuccess = baseRepo.ExecuteBatch(commands);
```

## Extending the Library

### Creating a Custom Repository

```csharp
public class UserRepository : BaseRepository<User>
{
    public UserRepository(IDatabaseConnection databaseConnection, ILogger<UserRepository>? logger = null)
        : base(databaseConnection, logger)
    {
    }
    
    // Add custom methods
    public IEnumerable<User> GetActiveUsers()
    {
        using var connection = DatabaseConnection.CreateConnection();
        var query = $"SELECT * FROM {TableName} WHERE active = @Active";
        return connection.Query<User>(query, new { Active = true });
    }
}
```

### Creating a Custom Database Connection

```csharp
public class PostgreSqlConnection : IDatabaseConnection
{
    // Implement the interface methods for PostgreSQL
}
```

## Handling Exceptions

The library throws specific exceptions:

```csharp
try
{
    var user = repository.GetById(123);
}
catch (RepositoryException ex)
{
    // Handle repository-specific exception
}
catch (DatabaseConnectionException ex)
{
    // Handle connection issues
}
catch (DatabaseConfigurationException ex)
{
    // Handle configuration issues
}
catch (Exception ex)
{
    // Handle other exceptions
}
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.