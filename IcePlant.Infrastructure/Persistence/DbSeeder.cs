using IcePlant.Domain.Aggregates.Basin;
using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Enums;
using IcePlant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IcePlant.Infrastructure.Persistence;

/// <summary>
/// Seeds mandatory reference data that must exist before the app can run.
/// Called once on startup after migrations are applied.
/// </summary>
public class DbSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(AppDbContext context, ILogger<DbSeeder> logger)
    {
        _context = context;
        _logger  = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedBasinAsync(ct);
        await SeedExpenseCategoriesAsync(ct);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Database seeding complete.");
    }

    // ── Basin (Singleton) ─────────────────────────────────────────────────────

    private async Task SeedBasinAsync(CancellationToken ct)
    {
        if (await _context.Basins.AnyAsync(ct)) return;

        var result = BasinAggregate.Create(
            maxCapacity:  500,
            freezeHours:  8.0,
            initialStock: 0);

        if (result.IsFailure)
        {
            _logger.LogError("Failed to seed basin: {Error}", result.Error);
            return;
        }

        await _context.Basins.AddAsync(result.Value, ct);
        _logger.LogInformation("Basin seeded (capacity=500, freeze=8h).");
    }

    // ── Expense Categories ────────────────────────────────────────────────────

    private async Task SeedExpenseCategoriesAsync(CancellationToken ct)
    {
        if (await _context.ExpenseCategories.AnyAsync(ct)) return;

        var categories = new[]
        {
            // Utility Bills
            ExpenseCategory.CreateUtilityBill("Water",       UtilityBillType.Water),
            ExpenseCategory.CreateUtilityBill("Electricity", UtilityBillType.Electricity),
            ExpenseCategory.CreateUtilityBill("Ammonia",     UtilityBillType.Ammonia),
            ExpenseCategory.CreateUtilityBill("Salt",        UtilityBillType.Salt),

            // General Expenses
            ExpenseCategory.CreateGeneralExpense("Wages"),
            ExpenseCategory.CreateGeneralExpense("Maintenance"),
            ExpenseCategory.CreateGeneralExpense("Petty Cash"),
            ExpenseCategory.CreateGeneralExpense("Other"),
        };

        await _context.ExpenseCategories.AddRangeAsync(categories, ct);
        _logger.LogInformation("Expense categories seeded ({Count} records).", categories.Length);
    }
}
