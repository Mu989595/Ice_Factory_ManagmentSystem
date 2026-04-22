using IceFactoryManagmentSystem.Domain.Entities;
using IceFactoryManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IceFactoryManagmentSystem.Infrastructure.Repositories;

// ── Expense Category ──────────────────────────────────────────────────────────

public interface IExpenseCategoryRepository
{
    Task<expense_categories?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<expense_categories>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<expense_categories>> GetByTypeAsync(string parentType, CancellationToken ct = default);
}

public class ExpenseCategoryRepository
    : BaseRepository<expense_categories>, IExpenseCategoryRepository
{
    public ExpenseCategoryRepository(AppDbContext context) : base(context) { }

    public async Task<expense_categories?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.FindAsync([id], ct);

    public async Task<IReadOnlyList<expense_categories>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking().OrderBy(c => c.ParentType).ThenBy(c => c.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<expense_categories>> GetByTypeAsync(
        string parentType,
        CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(c => c.ParentType == parentType)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
}

// ── Expense ───────────────────────────────────────────────────────────────────

public interface IExpenseRepository
{
    Task<IReadOnlyList<Expense>> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<Expense>> GetByMonthAsync(int year, int month, CancellationToken ct = default);
    Task<IReadOnlyList<Expense>> GetByMonthAndTypeAsync(int year, int month, string parentType, CancellationToken ct = default);
    Task<decimal> GetMonthlyTotalByTypeAsync(int year, int month, string parentType, CancellationToken ct = default);
    Task AddAsync(Expense expense, CancellationToken ct = default);
}

public class ExpenseRepository : BaseRepository<Expense>, IExpenseRepository
{
    public ExpenseRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Expense>> GetByDateAsync(
        DateOnly date,
        CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Include(e => e.Category)
            .Where(e => e.LedgerDay.DayDate == date)
            .OrderBy(e => e.ExpenseTime)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Expense>> GetByMonthAsync(
        int year, int month,
        CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Include(e => e.Category)
            .Where(e => e.LedgerDay.DayDate.Year  == year
                     && e.LedgerDay.DayDate.Month == month)
            .OrderBy(e => e.LedgerDay.DayDate)
            .ThenBy(e => e.ExpenseTime)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Expense>> GetByMonthAndTypeAsync(
        int year, int month, string parentType,
        CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Include(e => e.Category)
            .Where(e => e.LedgerDay.DayDate.Year  == year
                     && e.LedgerDay.DayDate.Month == month
                     && e.Category.ParentType     == parentType)
            .ToListAsync(ct);

    public async Task<decimal> GetMonthlyTotalByTypeAsync(
        int year, int month, string parentType,
        CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(e => e.LedgerDay.DayDate.Year  == year
                     && e.LedgerDay.DayDate.Month == month
                     && e.Category.ParentType     == parentType)
            .SumAsync(e => (decimal?)e.Amount ?? 0m, ct);

    public async Task AddAsync(Expense expense, CancellationToken ct = default)
        => await _dbSet.AddAsync(expense, ct);
}
