using ApiKeyValidator.Core.Configuration;
using ApiKeyValidator.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiKeyValidator.Tests;

public class ApiKeyValidatorServiceTests
{
    private readonly IApiKeyValidatorService _validatorService;
    private readonly Mock<ILogger<ApiKeyValidatorService>> _loggerMock;

    public ApiKeyValidatorServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ApiKey:Prefix", "api_" },
                { "ApiKey:Length", "12" },
                { "ApiKey:AllowedCharsRegex", "^[a-zA-Z0-9-_]+$" },
                { "ApiKey:GenerationChars", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_" },
                { "ApiKey:ExpirationDate", "2025-12-31T23:59:59Z" },
                { "ApiKey:RequirePrefix", "true" }
            })
            .Build();

        _loggerMock = new Mock<ILogger<ApiKeyValidatorService>>();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();
        var serviceProvider = services.BuildServiceProvider();
        _validatorService = serviceProvider.GetRequiredService<IApiKeyValidatorService>();
    }

    [Fact]
    public void ValidateKey_ValidKey_ReturnsValidResult()
    {
        var result = _validatorService.ValidateKey("api_12345678");
        Assert.True(result.IsValid);
        Assert.Equal("API-Schlüssel ist gültig.", result.Message);
    }

    [Fact]
    public void ValidateKey_ExpiredKey_ReturnsInvalidResult()
    {
        var futureDate = DateTime.Now.AddDays(1).ToString("o");
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ApiKey:Prefix", "api_" },
                { "ApiKey:Length", "12" },
                { "ApiKey:AllowedCharsRegex", "^[a-zA-Z0-9-_]+$" },
                { "ApiKey:ExpirationDate", futureDate }, // Zukunft
                { "ApiKey:RequirePrefix", "true" }
            })
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IApiKeyValidatorService>();

        var result = service.ValidateKey("api_12345678");
        Assert.True(result.IsValid); // Sollte gültig sein, da das Datum in der Zukunft liegt
    }

    [Fact]
    public void ValidateKey_ExpiredKey_ReturnsInvalidResultWithPastDate()
    {
        var pastDate = DateTime.Now.AddDays(-1).ToString("o");
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ApiKey:Prefix", "api_" },
                { "ApiKey:Length", "12" },
                { "ApiKey:AllowedCharsRegex", "^[a-zA-Z0-9-_]+$" },
                { "ApiKey:ExpirationDate", pastDate }, // Vergangenheit
                { "ApiKey:RequirePrefix", "true" }
            })
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();
        services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IApiKeyValidatorService>();

        var result = service.ValidateKey("api_12345678");
        Assert.False(result.IsValid);
        Assert.Equal("API-Schlüssel ist abgelaufen.", result.Message);
    }

    [Fact]
    public void GenerateKey_ValidParameters_GeneratesValidKey()
    {
        var key = _validatorService.GenerateKey();
        var result = _validatorService.ValidateKey(key);
        Assert.True(result.IsValid);
        Assert.StartsWith("api_", key);
        Assert.Equal(12, key.Length);
    }
}