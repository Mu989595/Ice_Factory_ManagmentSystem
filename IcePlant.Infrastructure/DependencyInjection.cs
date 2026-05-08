using IcePlant.Domain.Interfaces;
using IcePlant.Infrastructure.BackgroundJobs;
using IcePlant.Infrastructure.Events;
using IcePlant.Infrastructure.Persistence;
using IcePlant.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IcePlant.Infrastructure;

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
        // ── Database ───────────────────────────────────────────────────────────
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

        // ── Unit of Work ───────────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        // ── Background Services ────────────────────────────────────────────────
        services.AddHostedService<ReplenishmentBackgroundService>();
        services.AddHostedService<DayRolloverBackgroundService>();

        // ── Repositories ──────────────────────────────────────────────────────
        services.AddScoped<IcePlant.Domain.Interfaces.Repositories.IBasinRepository, Repositories.BasinRepository>();
        services.AddScoped<IcePlant.Domain.Interfaces.Repositories.ILedgerDayRepository, Repositories.LedgerDayRepository>();
        services.AddScoped<IcePlant.Domain.Interfaces.Repositories.ISaleRepository, Repositories.SaleRepository>();
        services.AddScoped<IcePlant.Domain.Interfaces.Repositories.IExpenseRepository, Repositories.ExpenseRepository>();
        services.AddScoped<IcePlant.Domain.Interfaces.Repositories.IWorkerRepository, Repositories.WorkerRepository>();
        services.AddScoped<IcePlant.Domain.Interfaces.Repositories.IAttendanceRepository, Repositories.AttendanceRepository>();
        services.AddScoped<IcePlant.Domain.Interfaces.Repositories.IMonthlySummaryRepository, Repositories.MonthlySummaryRepository>();
        services.AddScoped<IcePlant.Domain.Interfaces.Repositories.IExpenseCategoryRepository, Repositories.ExpenseCategoryRepository>();
        services.AddScoped<IcePlant.Domain.Interfaces.Repositories.IProductionCycleRepository, Repositories.ProductionCycleRepository>();
        
        // ── Domain Events ─────────────────────────────────────────────────────
        services.AddScoped<IEventDispatcher, EventDispatcher>();
        services.AddTransient<IDomainEventHandler<IcePlant.Domain.Events.SaleRecordedEvent>, IcePlant.Application.EventHandlers.OnSaleRecorded_DeductBasinStock>();
        services.AddTransient<IDomainEventHandler<IcePlant.Domain.Events.StockDeductedEvent>, IcePlant.Application.EventHandlers.OnStockDeducted_UpdateLedgerClosingStock>();

        // ── Seeder ────────────────────────────────────────────────────────────
        services.AddScoped<DbSeeder>();

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

        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync();
    }
}

