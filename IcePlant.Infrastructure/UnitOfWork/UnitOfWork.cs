using IcePlant.Domain.Interfaces;
using IcePlant.Domain.Interfaces.Repositories;
using IcePlant.Infrastructure.Persistence;
using IcePlant.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace IcePlant.Infrastructure.UnitOfWork;

/// <summary>
/// Wraps all repositories and the DbContext in one transaction boundary.
/// Implements the Domain IUnitOfWork interface — Application layer only sees the interface.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    // ── Lazy-initialised repositories ─────────────────────────────────────────
    private IBasinRepository?           _basin;
    private ILedgerDayRepository?       _ledgerDays;
    private ISaleRepository?            _sales;
    private IExpenseRepository?         _expenses;
    private IExpenseCategoryRepository? _expenseCategories;
    private IWorkerRepository?          _workers;
    private IAttendanceRepository?      _attendance;
    private IMonthlySummaryRepository?  _monthlySummaries;
    private IProductionCycleRepository? _productionCycles;

    public UnitOfWork(AppDbContext context) => _context = context;

    public IBasinRepository           Basin             => _basin            ??= new BasinRepository(_context);
    public ILedgerDayRepository       LedgerDays        => _ledgerDays       ??= new LedgerDayRepository(_context);
    public ISaleRepository            Sale              => _sales            ??= new SaleRepository(_context);
    public IExpenseRepository         Expenses          => _expenses         ??= new ExpenseRepository(_context);
    public IExpenseCategoryRepository ExpenseCategories => _expenseCategories??= new ExpenseCategoryRepository(_context);
    public IWorkerRepository          Workers           => _workers          ??= new WorkerRepository(_context);
    public IAttendanceRepository      Attendance        => _attendance       ??= new AttendanceRepository(_context);
    public IMonthlySummaryRepository  MonthlySummaries  => _monthlySummaries ??= new MonthlySummaryRepository(_context);
    public IProductionCycleRepository ProductionCycles  => _productionCycles ??= new ProductionCycleRepository(_context);

    // ── Transaction Management ────────────────────────────────────────────────
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction to commit.");
        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;
        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
