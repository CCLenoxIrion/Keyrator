using ApiKeyValidator.Core.Configuration;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ApiKeyValidator.Core.Services;

public interface IApiKeyValidatorService
{
    string GenerateKey(int? length = null, string? prefix = null, string? chars = null);
    ValidationResult ValidateKey(string apiKey);
}

public class ApiKeyValidatorService : IApiKeyValidatorService
{
    private readonly ApiKeyConfig _config;

    public ApiKeyValidatorService(IConfiguration configuration)
    {
        _config = configuration.GetSection("ApiKey").Get<ApiKeyConfig>() ?? new ApiKeyConfig();
    }
    public string GenerateKey(int? length = null, string? prefix = null, string? chars = null)
    {
        var effectivePrefix = prefix ?? _config.Prefix;
        var effectiveLength = length ?? _config.Length ?? 32;
        var effectiveChars = chars ?? _config.GenerationChars;

        if (effectiveLength <= effectivePrefix.Length)
        {
            throw new ArgumentException("Die Länge des Schlüssels muss größer als die Präfixlänge sein.");
        }

        var randomLength = effectiveLength - effectivePrefix.Length;
        var random = new Random();
        var randomPart = new char[randomLength];

        for (int i = 0; i < randomLength; i++)
        {
            randomPart[i] = effectiveChars[random.Next(effectiveChars.Length)];
        }

        return effectivePrefix + new string(randomPart);
    }

    public ValidationResult ValidateKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return new ValidationResult(false, "API-Schlüssel ist leer.");
        }

        if (!string.IsNullOrEmpty(_config.Prefix) && !apiKey.StartsWith(_config.Prefix))
        {
            return new ValidationResult(false, $"API-Schlüssel muss mit '{_config.Prefix}' beginnen.");
        }

        if (_config.Length.HasValue && apiKey.Length != _config.Length)
        {
            return new ValidationResult(false, $"API-Schlüssel muss {_config.Length} Zeichen lang sein.");
        }

        if (!Regex.IsMatch(apiKey, _config.AllowedCharsRegex))
        {
            return new ValidationResult(false, "API-Schlüssel enthält ungültige Zeichen.");
        }

        return new ValidationResult(true, "API-Schlüssel ist gültig.");
    }
}

public record ValidationResult(bool IsValid, string Message);