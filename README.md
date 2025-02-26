# RansauSysteme.Toolbox
Methods for .NET Core/.NET 8 applications, for common development tasks.

## RansauSysteme.Database

A flexible .NET data access library implementing the Repository pattern for database operations.

### Features

- Generic repository implementation with full CRUD operations
- Support for MySQL databases with extensible design for other providers
- Both synchronous and asynchronous API
- Efficient batch operations with transaction support
- Built-in caching repository with configurable refresh intervals
- Factory pattern for creating repositories with dependency injection support
- Comprehensive error handling with custom exceptions
- Thread-safe implementation for concurrent applications

### Installation

```bash
dotnet add package RansauSysteme.Database
```

## RansauSysteme.Utils

A comprehensive collection of utility methods for .NET Core/.NET 8 applications, providing robust solutions for common development tasks.


### Features

- The `StringUtils` class provides various string manipulation methods
- I/O methods : the `IOUtils` class makes file and directory operations more robust
- The `JsonUtils` class simplifies working with JSON
- The `DatabaseUtils` class helps with database-related operations
- Result Pattern : the `Result<T>` class provides a clean way to handle operation results
- Build Utils : the `BuildUtils` class provides information about the current build

### Installation

```bash
dotnet add package RansauSysteme.Utils
```


## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.