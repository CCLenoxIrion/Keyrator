using ApiKeyValidator.Core.Extensions;
using ApiKeyValidator.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddSingleton<IConfiguration>(configuration); // Registriere IConfiguration als Singleton
        services.AddApiKeyValidator(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // ApiKeyValidatorService holen
        var validatorService = serviceProvider.GetRequiredService<IApiKeyValidatorService>();

        // Beispiel: API-Schlüssel generieren
        string generatedKey = validatorService.GenerateKey();
        Console.WriteLine($"Generierter API-Schlüssel: {generatedKey}");

        // Beispiel: API-Schlüssel validieren
        var validResult = validatorService.ValidateKey(generatedKey);
        Console.WriteLine($"Validierung (gültig): {validResult.Message} (IsValid: {validResult.IsValid})");

        // Beispiel: Ungültigen Schlüssel validieren
        var invalidResult = validatorService.ValidateKey("invalid_key");
        Console.WriteLine($"Validierung (ungültig): {invalidResult.Message} (IsValid: {invalidResult.IsValid})");
    }
}