using IcePlant.Domain.Aggregates.Basin;
using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Aggregates.HR;
using IcePlant.Domain.Aggregates.Monthly;
using IcePlant.Domain.Enums;

namespace IcePlant.Domain.Interfaces.Repositories;

// ── Basin ─────────────────────────────────────────────────────────────────────

public interface IBasinRepository
{
    /// <summary>Gets the singleton basin (Id = 1). Always exists after seeding.</summary>
    Task<BasinAggregate> GetSingletonAsync(CancellationToken ct = default);

    Task UpdateAsync(BasinAggregate basin, CancellationToken ct = default);
}

public interface IProductionCycleRepository
{
    Task AddAsync(ProductionCycle cycle, CancellationToken ct = default);

    /// <summary>Returns true if a replenishment already fired after the given timestamp today.</summary>
    Task<bool> ExistsAfterAsync(DateTime timestamp, CancellationToken ct = default);

    Task<IReadOnlyList<ProductionCycle>> GetByDateAsync(DateOnly date, CancellationToken ct = default);

    Task<IReadOnlyList<ProductionCycle>> GetByDateRangeAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default);
}

// ── Finance ───────────────────────────────────────────────────────────────────

public interface ILedgerDayRepository
{
    Task<LedgerDay?>        GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<LedgerDay>         GetOrCreateAsync(DateOnly date, int openingStock, CancellationToken ct = default);
    Task<IReadOnlyList<LedgerDay>> GetRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);

    /// <summary>Returns total income for the given month across all ledger days.</summary>
    Task<decimal> GetTotalIncomeAsync(int year, int month, CancellationToken ct = default);

    /// <summary>Returns total expenses for the given month across all ledger days.</summary>
    Task<decimal> GetTotalExpensesAsync(int year, int month, CancellationToken ct = default);

    Task AddAsync(LedgerDay ledgerDay, CancellationToken ct = default);
    Task UpdateAsync(LedgerDay ledgerDay, CancellationToken ct = default);
}

public interface ISaleRepository
{
    Task<Sale?>             GetByIdAsync(int id, CancellationToken ct = default);
    Task<Sale?>             GetLastSaleForDayAsync(DateOnly date, CancellationToken ct = default);

    /// <summary>
    /// Returns the total blocks sold since the last replenishment event today.
    /// Used by the auto-replenishment service to know how many blocks to add back.
    /// </summary>
    Task<int> GetBlocksSoldSinceLastReplenishAsync(DateOnly date, CancellationToken ct = default);

    Task<IReadOnlyList<Sale>> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task AddAsync(Sale sale, CancellationToken ct = default);
}

public interface IExpenseRepository
{
    Task<IReadOnlyList<Expense>> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<Expense>> GetByMonthAsync(int year, int month, CancellationToken ct = default);
    Task<IReadOnlyList<Expense>> GetByCategoryTypeAsync(ExpenseCategoryType type, int year, int month, CancellationToken ct = default);
    Task AddAsync(Expense expense, CancellationToken ct = default);
}

public interface IExpenseCategoryRepository
{
    Task<ExpenseCategory?>             GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ExpenseCategory>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ExpenseCategory>> GetByTypeAsync(ExpenseCategoryType type, CancellationToken ct = default);
}

// ── HR ────────────────────────────────────────────────────────────────────────

public interface IWorkerRepository
{
    Task<Worker?>                    GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Worker>>      GetAllActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Worker>>      GetByRoleAsync(WorkerRole role, CancellationToken ct = default);
    Task AddAsync(Worker worker, CancellationToken ct = default);
    Task UpdateAsync(Worker worker, CancellationToken ct = default);
}

public interface IAttendanceRepository
{
    Task<IReadOnlyList<DailyAttendance>> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task AddAsync(DailyAttendance attendance, CancellationToken ct = default);
    Task UpdateAsync(DailyAttendance attendance, CancellationToken ct = default);
    Task<bool> ExistsAsync(int ledgerDayId, int workerId, CancellationToken ct = default);
}

// ── Monthly ───────────────────────────────────────────────────────────────────

public interface IMonthlySummaryRepository
{
    Task<MonthlySummary?>             GetByMonthAsync(int year, int month, CancellationToken ct = default);
    Task<IReadOnlyList<MonthlySummary>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(MonthlySummary summary, CancellationToken ct = default);
    Task UpdateAsync(MonthlySummary summary, CancellationToken ct = default);
    Task<bool> IsMonthClosedAsync(int year, int month, CancellationToken ct = default);
}
