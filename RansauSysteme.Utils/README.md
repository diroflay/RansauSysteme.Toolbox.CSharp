# RansauSysteme.Utils

A comprehensive collection of utility methods for .NET Core/.NET 8 applications, providing robust solutions for common development tasks.


## Installation

Install the package from NuGet:

```bash
dotnet add package RansauSysteme.Utils
```

Or using the Package Manager Console:

```powershell
Install-Package RansauSysteme.Utils
```

## Features

### String Utilities

The `StringUtils` class provides various string manipulation methods:

```csharp
// Convert strings to different cases
string snakeCase = "MyVariable".ToSnakeCase(); // "my_variable"
string capitalized = "hello world".Capitalize(); // "Hello World"
string camelCase = "user_first_name".ToCamelCase(); // "userFirstName"

// Other string operations
string truncated = "This is a long text".Truncate(10); // "This is a..."
```

### I/O Utilities

The `IOUtils` class makes file and directory operations more robust:

```csharp
// Create files and directories safely
IOUtils.CreateFile("path/to/new/file.txt");

// Copy directories recursively
IOUtils.CopyDirectory("source", "destination");

// Create timestamped filenames
string filename = IOUtils.DatetimeFilename(DateTime.Now); // "2025-02-26_15-30-00"
```

### JSON Utilities

The `JsonUtils` class simplifies working with JSON:

```csharp
// Serialize objects to JSON
string json = JsonUtils.ToJson(myObject);

// Deserialize JSON to objects
MyClass? obj = JsonUtils.ParseJson<MyClass>(jsonString);

// Load JSON from files
Configuration? config = JsonUtils.ParseFile<Configuration>("config.json");

// Save objects as JSON files
JsonUtils.SaveToFile(myObject, "data.json");
```

### Database Utilities

The `DatabaseUtils` class helps with database-related operations:

```csharp
// Generate secure random passwords
string password = DatabaseUtils.GeneratePassword(length: 12);

// Validate passwords against security requirements
bool isValid = DatabaseUtils.IsValidPassword(
    password, 
    minPasswordLength: 8,
    requireUppercase: true,
    requireLowercase: true,
    requireDigit: true,
    requireSpecialChar: true
);

// Or use predefined security levels
bool isValid = DatabaseUtils.IsValidPassword(
    password, 
    strength: PasswordStrength.VeryStrong
);
```

### Result Pattern

The `Result<T>` class provides a clean way to handle operation results:

```csharp
// Create success results
Result<User> userResult = Result<User>.Success(user);

// Create failure results
Result<User> failedResult = Result<User>.Failure("User not found");

// Use in methods
public Result<User> GetUser(int id)
{
    var user = _repository.FindById(id);
    if (user == null)
        return Result<User>.Failure("User not found");
    
    return Result<User>.Success(user);
}

// Check results
if (result.IsSuccess)
{
    // Use result.Data
}
else
{
    // Handle error using result.Error
}
```

### Build Utils

The `BuildUtils` class provides information about the current build:

```csharp
// Check if running in debug mode
if (BuildUtils.IsDebug)
{
    // Do additional logging
}

// Get environment information
string summary = BuildUtils.GetSummary();
// "Build Configuration: Debug
//  Version: 1.0.0.0
//  OS: Windows
//  Process: 64-bit
//  Build Date: 2025-02-26 15:30:00"
```

## Requirements

- .NET 8.0 or higher

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.