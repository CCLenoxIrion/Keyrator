# Keyrator
A robust .NET library for generating and validating API keys with configurable rules and logging support.

# Installation
To use the ApiKeyValidator.Core library in your project, install it via NuGet:

```bash
dotnet add package ApiKeyValidator.Core
```
Ensure you have the .NET 9.0 SDK or later installed. You can download it from the [official .NET website](https://dotnet.microsoft.com/en-us/download).

# Usage
**Prerequisites**
Add the `Microsoft.Extensions.Configuration.Json` and the `Microsoft.Extensions.Logging` packages if not already included

**Configuration**
Create an `appsettings.json` file in your project to configure the API key settings:
```json
{
  "ApiKey": {
    "Prefix": "api_",
    "Length": 16,
    "AllowedCharsRegex": "^[a-zA-Z0-9-_]+$",
    "GenerationChars": "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_",
    "ExpirationDate": "2025-12-31T23:59:59Z",
    "RequirePrefix": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

**Example Code**
Here's an example of how to use the library in a .NET console application:

```csharp
using ApiKeyValidator.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main(string[] args)
    {
        // Setup configuration
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddApiKeyValidator(configuration);
        services.AddLogging(builder => builder.AddConsole()); // Add console logging
        var serviceProvider = services.BuildServiceProvider();

        // Get the validator service
        var validatorService = serviceProvider.GetRequiredService<IApiKeyValidatorService>();

        // Generate a new API key
        string generatedKey = validatorService.GenerateKey();
        Console.WriteLine($"Generated API Key: {generatedKey}");

        // Validate the generated key
        var validResult = validatorService.ValidateKey(generatedKey);
        Console.WriteLine($"Validation (valid): {validResult.Message} (IsValid: {validResult.IsValid})");

        // Validate an invalid key
        var invalidResult = validatorService.ValidateKey("invalid_key");
        Console.WriteLine($"Validation (invalid): {invalidResult.Message} (IsValid: {invalidResult.IsValid})");
    }
}
```
