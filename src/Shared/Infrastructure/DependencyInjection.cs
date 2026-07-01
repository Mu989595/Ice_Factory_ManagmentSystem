using IcePlant.Domain.Interfaces;
using IcePlant.Infrastructure.BackgroundJobs;
using IcePlant.Infrastructure.Events;
using IcePlant.Infrastructure.Interceptors;
using IcePlant.Infrastructure.Persistence;
using IcePlant.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
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
        // ── Audit Interceptor ─────────────────────────────────────────────────
        services.AddSingleton<AuditSaveChangesInterceptor>();

        // ── Database ───────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(
                        typeof(AppDbContext).Assembly.FullName);
                });
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        // ── Identity & JWT ─────────────────────────────────────────────────────
        services.AddIdentity<IcePlant.Domain.Identity.ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        var jwtSettings = configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"]!;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret))
            };
        });

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
        services.AddScoped<IcePlant.Domain.Interfaces.Repositories.IMonthlySummaryRepository, Repositories.MonthlySummaryRepository>();
        services.AddScoped<IcePlant.Domain.Interfaces.Repositories.IExpenseCategoryRepository, Repositories.ExpenseCategoryRepository>();
        services.AddScoped<IcePlant.Domain.Interfaces.Repositories.IProductionCycleRepository, Repositories.ProductionCycleRepository>();
        services.AddScoped<IcePlant.Domain.Interfaces.Repositories.ITransactionHistoryRepository, Repositories.TransactionHistoryRepository>();
        services.AddScoped<IcePlant.Domain.Interfaces.Repositories.IAttendanceRepository, Repositories.AttendanceRepository>();
        
        // ── Domain Events ─────────────────────────────────────────────────────
        services.AddScoped<IEventDispatcher, EventDispatcher>();
        services.AddTransient<IDomainEventHandler<IcePlant.Domain.Events.SaleRecordedEvent>, IcePlant.Application.EventHandlers.OnSaleRecorded_DeductBasinStock>();
        services.AddTransient<IDomainEventHandler<IcePlant.Domain.Events.SaleRecordedEvent>, IcePlant.Application.EventHandlers.OnSaleRecorded_AddTransactionHistory>();
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

