using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Application.Features.Auth;
using PasswordManager.Application.Features.VaultSettings;

namespace PasswordManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVaultSettingsService, VaultSettingsService>();

        return services;
    }
}
