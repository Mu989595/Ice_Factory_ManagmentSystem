using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Enums;
using IcePlant.Domain.Interfaces.Repositories;
using IcePlant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcePlant.Infrastructure.Repositories;

// ── Expense Category ──────────────────────────────────────────────────────────

public class ExpenseCategoryRepository
    : BaseRepository<ExpenseCategory>, IExpenseCategoryRepository
{
    public ExpenseCategoryRepository(AppDbContext context) : base(context) { }

    public async Task<ExpenseCategory?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.FindAsync([id], ct);

    public async Task<IReadOnlyList<ExpenseCategory>> GetAllActiveAsync(CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CategoryType)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ExpenseCategory>> GetByTypeAsync(
        ExpenseCategoryType type,
        CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(c => c.CategoryType == type && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
}

// ── Expense ───────────────────────────────────────────────────────────────────

public class ExpenseRepository : BaseRepository<Expense>, IExpenseRepository
{
    public ExpenseRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Expense>> GetByDateAsync(
        DateOnly date,
        CancellationToken ct = default)
        => await _context.LedgerDays
            .AsNoTracking()
            .Where(l => l.DayDate == date)
            .SelectMany(l => l.Expenses)
            .Include(e => e.Category)
            .OrderBy(e => e.ExpenseTime)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Expense>> GetByMonthAsync(
        int year, int month,
        CancellationToken ct = default)
        => await _context.LedgerDays
            .AsNoTracking()
            .Where(l => l.DayDate.Year == year && l.DayDate.Month == month)
            .SelectMany(l => l.Expenses)
            .Include(e => e.Category)
            .OrderBy(e => e.ExpenseTime)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Expense>> GetByCategoryTypeAsync(
        ExpenseCategoryType type, int year, int month,
        CancellationToken ct = default)
        => await _context.LedgerDays
            .AsNoTracking()
            .Where(l => l.DayDate.Year == year && l.DayDate.Month == month)
            .SelectMany(l => l.Expenses)
            .Include(e => e.Category)
            .Where(e => e.Category != null && e.Category.CategoryType == type)
            .ToListAsync(ct);

    public async Task AddAsync(Expense expense, CancellationToken ct = default)
        => await _dbSet.AddAsync(expense, ct);
}
