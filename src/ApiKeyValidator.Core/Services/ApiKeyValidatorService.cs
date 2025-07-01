using System.Text.RegularExpressions;
using ApiKeyValidator.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApiKeyValidator.Core.Services;

public interface IApiKeyValidatorService
{
    string GenerateKey(int? length = null, string? prefix = null, string? chars = null);
    ValidationResult ValidateKey(string apiKey);
}

public class ApiKeyValidatorService : IApiKeyValidatorService
{
    private readonly ApiKeyConfig _config;
    private readonly ILogger<ApiKeyValidatorService> _logger;

    public ApiKeyValidatorService(IConfiguration configuration, ILogger<ApiKeyValidatorService> logger)
    {
        _config = configuration.GetSection("ApiKey").Get<ApiKeyConfig>() ?? new ApiKeyConfig();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string GenerateKey(int? length = null, string? prefix = null, string? chars = null)
    {
        try
        {
            var effectivePrefix = prefix ?? _config.Prefix;
            var effectiveLength = length ?? _config.Length ?? 32;
            var effectiveChars = chars ?? _config.GenerationChars;

            if (effectiveLength <= effectivePrefix.Length)
            {
                _logger.LogWarning("Generierung fehlgeschlagen: L�nge ({Length}) muss gr��er als Pr�fixl�nge ({PrefixLength}) sein.", effectiveLength, effectivePrefix.Length);
                throw new ArgumentException("Die L�nge des Schl�ssels muss gr��er als die Pr�fixl�nge sein.");
            }

            var randomLength = effectiveLength - effectivePrefix.Length;
            var random = new Random();
            var randomPart = new char[randomLength];

            for (int i = 0; i < randomLength; i++)
            {
                randomPart[i] = effectiveChars[random.Next(effectiveChars.Length)];
            }

            var key = effectivePrefix + new string(randomPart);
            _logger.LogInformation("Neuer API-Schl�ssel generiert: {Key}", key);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei der Generierung des API-Schl�ssels.");
            throw;
        }
    }

    public ValidationResult ValidateKey(string apiKey)
    {
        try
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Validierung fehlgeschlagen: API-Schl�ssel ist leer.");
                return new ValidationResult(false, "API-Schl�ssel ist leer.");
            }

            if (_config.RequirePrefix && !string.IsNullOrEmpty(_config.Prefix) && !apiKey.StartsWith(_config.Prefix))
            {
                _logger.LogWarning("Validierung fehlgeschlagen: API-Schl�ssel muss mit '{Prefix}' beginnen.", _config.Prefix);
                return new ValidationResult(false, $"API-Schl�ssel muss mit '{_config.Prefix}' beginnen.");
            }

            if (_config.Length.HasValue && apiKey.Length != _config.Length)
            {
                _logger.LogWarning("Validierung fehlgeschlagen: API-Schl�ssel muss {_config.Length} Zeichen lang sein.", _config.Length);
                return new ValidationResult(false, $"API-Schl�ssel muss {_config.Length} Zeichen lang sein.");
            }

            if (!Regex.IsMatch(apiKey, _config.AllowedCharsRegex))
            {
                _logger.LogWarning("Validierung fehlgeschlagen: API-Schl�ssel enth�lt ung�ltige Zeichen.");
                return new ValidationResult(false, "API-Schl�ssel enth�lt ung�ltige Zeichen.");
            }

            if (_config.ExpirationDate.HasValue && DateTime.Now > _config.ExpirationDate.Value)
            {
                _logger.LogWarning("Validierung fehlgeschlagen: API-Schl�ssel ist abgelaufen.");
                return new ValidationResult(false, "API-Schl�ssel ist abgelaufen.");
            }

            _logger.LogInformation("API-Schl�ssel validiert: {ApiKey}", apiKey);
            return new ValidationResult(true, "API-Schl�ssel ist g�ltig.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei der Validierung des API-Schl�ssels: {ApiKey}", apiKey);
            throw;
        }
    }
}

public record ValidationResult(bool IsValid, string Message);