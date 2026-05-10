using IcePlant.Application.DTOs;
using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Common;
using IcePlant.Domain.Enums;
using IcePlant.Domain.Interfaces;
using IcePlant.Domain.Interfaces.Repositories;

namespace IcePlant.Application.Services;

/// <summary>
/// Handles the business logic for recording and querying expenses.
/// </summary>
public class ExpenseService
{
    private readonly ILedgerDayRepository      _ledgerDayRepo;
    private readonly IExpenseRepository        _expenseRepo;
    private readonly IExpenseCategoryRepository _categoryRepo;
    private readonly IBasinRepository          _basinRepo;
    private readonly IEventDispatcher          _eventDispatcher;
    private readonly IUnitOfWork               _unitOfWork;

    public ExpenseService(
        ILedgerDayRepository       ledgerDayRepo,
        IExpenseRepository         expenseRepo,
        IExpenseCategoryRepository  categoryRepo,
        IBasinRepository           basinRepo,
        IEventDispatcher           eventDispatcher,
        IUnitOfWork                unitOfWork)
    {
        _ledgerDayRepo   = ledgerDayRepo;
        _expenseRepo     = expenseRepo;
        _categoryRepo    = categoryRepo;
        _basinRepo       = basinRepo;
        _eventDispatcher = eventDispatcher;
        _unitOfWork      = unitOfWork;
    }

    /// <summary>
    /// Records a new expense on the specified LedgerDay.
    /// </summary>
    public async Task<Result<ExpenseResultDto>> RecordExpenseAsync(
        RecordExpenseDto dto,
        CancellationToken ct = default)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // 1. Ensure the ledger day exists for today
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var ledger = await _ledgerDayRepo.GetByDateAsync(today, ct);

            if (ledger is null)
            {
                // Get current basin stock to use as opening stock for the new ledger
                var basin = await _basinRepo.GetSingletonAsync(ct);
                ledger = await _ledgerDayRepo.GetOrCreateAsync(today, basin.CurrentStock, ct);
                await _unitOfWork.SaveChangesAsync(ct); // Ensure ID is populated
            }


            // 2. Verify category exists
            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId, ct);
            if (category is null)
                return Result.Failure<ExpenseResultDto>(
                    $"Expense category with ID {dto.CategoryId} not found.");

            // 3. Create the Expense domain object
            var expenseResult = Expense.Create(
                ledger.Id,
                dto.CategoryId,
                dto.Amount,
                DateTime.UtcNow,
                dto.Supplier,
                dto.InvoiceRef,
                dto.Notes);

            if (expenseResult.IsFailure)
                return Result.Failure<ExpenseResultDto>(expenseResult.Error);

            var expense = expenseResult.Value;

            // 4. Register the expense on the ledger (raises ExpenseRecordedEvent)
            var recordResult = ledger.RecordExpense(expense);
            if (recordResult.IsFailure)
                return Result.Failure<ExpenseResultDto>(recordResult.Error);

            // 5. Persist
            await _expenseRepo.AddAsync(expense, ct);
            await _ledgerDayRepo.UpdateAsync(ledger, ct);

            // 6. Dispatch domain events
            foreach (var domainEvent in ledger.DomainEvents)
                await _eventDispatcher.DispatchAsync(domainEvent, ct);

            ledger.ClearDomainEvents();

            // 7. Commit
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            return Result.Success(new ExpenseResultDto(
                ExpenseId:    expense.Id,
                LedgerDayId:  expense.LedgerDayId,
                CategoryId:   expense.CategoryId,
                CategoryName: category.Name,
                CategoryType: category.CategoryType.ToString(),
                Amount:       expense.Amount.Amount,
                ExpenseTime:  expense.ExpenseTime,
                Supplier:     expense.Supplier,
                InvoiceRef:   expense.InvoiceRef,
                Notes:        expense.Notes));
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    /// <summary>Returns all expenses for a given date.</summary>
    public async Task<Result<List<ExpenseResultDto>>> GetExpensesByDateAsync(
        DateOnly date,
        CancellationToken ct = default)
    {
        var expenses = await _expenseRepo.GetByDateAsync(date, ct);

        var results = expenses.Select(e => new ExpenseResultDto(
            ExpenseId:    e.Id,
            LedgerDayId:  e.LedgerDayId,
            CategoryId:   e.CategoryId,
            CategoryName: e.Category?.Name ?? "Unknown",
            CategoryType: e.Category?.CategoryType.ToString() ?? "Unknown",
            Amount:       e.Amount.Amount,
            ExpenseTime:  e.ExpenseTime,
            Supplier:     e.Supplier,
            InvoiceRef:   e.InvoiceRef,
            Notes:        e.Notes)).ToList();

        return Result.Success(results);
    }

    /// <summary>Returns all expenses for a given month.</summary>
    public async Task<Result<List<ExpenseResultDto>>> GetExpensesByMonthAsync(
        int year, int month,
        CancellationToken ct = default)
    {
        var expenses = await _expenseRepo.GetByMonthAsync(year, month, ct);

        var results = expenses.Select(e => new ExpenseResultDto(
            ExpenseId:    e.Id,
            LedgerDayId:  e.LedgerDayId,
            CategoryId:   e.CategoryId,
            CategoryName: e.Category?.Name ?? "Unknown",
            CategoryType: e.Category?.CategoryType.ToString() ?? "Unknown",
            Amount:       e.Amount.Amount,
            ExpenseTime:  e.ExpenseTime,
            Supplier:     e.Supplier,
            InvoiceRef:   e.InvoiceRef,
            Notes:        e.Notes)).ToList();

        return Result.Success(results);
    }

    /// <summary>Returns all active expense categories.</summary>
    public async Task<Result<List<ExpenseCategoryDto>>> GetCategoriesAsync(
        CancellationToken ct = default)
    {
        var categories = await _categoryRepo.GetAllActiveAsync(ct);

        var results = categories.Select(c => new ExpenseCategoryDto(
            Id:           c.Id,
            Name:         c.Name,
            CategoryType: c.CategoryType.ToString(),
            UtilityType:  c.UtilityType?.ToString(),
            IsActive:     c.IsActive)).ToList();

        return Result.Success(results);
    }
}
