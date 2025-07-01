using ApiKeyValidator.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiKeyValidator.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiKeyValidator(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiKeyValidator.Core.Configuration.ApiKeyConfig>(configuration.GetSection("ApiKey"));
        services.AddScoped<IApiKeyValidatorService, ApiKeyValidatorService>();
        services.AddLogging();
        return services;
    }
}