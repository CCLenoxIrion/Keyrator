using ApiKeyValidator.Core.Extensions;
using ApiKeyValidator.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace ApiKeyValidator.Example;

class Program
{
    static void Main(string[] args)
    {
        // Konfiguration einrichten
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        // Dependency Injection einrichten
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddApiKeyValidator(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Logger holen
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        // ApiKeyValidatorService holen
        var validatorService = serviceProvider.GetRequiredService<IApiKeyValidatorService>();

    }
}
