using IceFactoryManagmentSystem.Infrastructure.BackgroundJobs;
using IceFactoryManagmentSystem.Infrastructure.Persistence;
using IceFactoryManagmentSystem.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IceFactoryManagmentSystem.Infrastructure;

/// <summary>
/// Single entry point to register the entire Infrastructure layer.
/// Call this from Program.cs:  builder.Services.AddInfrastructure(builder.Configuration);
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration          configuration)
    {
        // ── Database ──────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(
                        typeof(AppDbContext).Assembly.FullName);

                    // Retry on transient SQL Server failures (network blips)
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount:       5,
                        maxRetryDelay:       TimeSpan.FromSeconds(10),
                        errorNumbersToAdd:   null);
                }));

        // ── Unit of Work ──────────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        // ── Background Services ───────────────────────────────────────────────
        services.AddHostedService<ReplenishmentBackgroundService>();
        services.AddHostedService<DayRolloverBackgroundService>();

        return services;
    }

    /// <summary>
    /// Applies EF Core migrations and seeds the database on startup.
    /// Call this from Program.cs after app.Build().
    /// </summary>
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope   = services.CreateScope();
        var       context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();
    }
}
