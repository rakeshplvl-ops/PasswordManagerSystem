using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Application.Common.Interfaces;
using PasswordManager.Infrastructure.Persistence;
using PasswordManager.Infrastructure.Persistence.Repositories;
using PasswordManager.Infrastructure.Services;

namespace PasswordManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found in configuration.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, b =>
                b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVaultSettingsRepository, VaultSettingsRepository>();
        services.AddScoped<IVaultEntryRepository, VaultEntryRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
