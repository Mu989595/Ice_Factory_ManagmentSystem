using IcePlant.Application.DTOs;
using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Common;
using IcePlant.Domain.Interfaces;
using IcePlant.Domain.Interfaces.Repositories;

namespace IcePlant.Application.Services;

/// <summary>
/// Handles the business logic for recording and retrieving sales.
///
/// Flow when a sale is recorded:
///   1. Validate the LedgerDay exists.
///   2. Create a Sale domain object (validates blocksSold > 0, unitPrice > 0).
///   3. Call LedgerDay.RecordSale() — this fires SaleRecordedEvent.
///   4. Event handler picks it up → Basin.DeductStock() is called automatically.
///   5. Everything is saved in one transaction.
/// </summary>
public class SaleService
{
    private readonly ILedgerDayRepository _ledgerDayRepo;
    private readonly ISaleRepository      _saleRepo;
    private readonly IBasinRepository     _basinRepo;
    private readonly IEventDispatcher     _eventDispatcher;
    private readonly IUnitOfWork          _unitOfWork;

    public SaleService(
        ILedgerDayRepository ledgerDayRepo,
        ISaleRepository      saleRepo,
        IBasinRepository     basinRepo,
        IEventDispatcher     eventDispatcher,
        IUnitOfWork          unitOfWork)
    {
        _ledgerDayRepo   = ledgerDayRepo;
        _saleRepo        = saleRepo;
        _basinRepo       = basinRepo;
        _eventDispatcher = eventDispatcher;
        _unitOfWork      = unitOfWork;
    }

    /// <summary>
    /// Records a new sale on the specified LedgerDay.
    /// </summary>
    public async Task<Result<SaleResultDto>> RecordSaleAsync(
        RecordSaleDto dto,
        CancellationToken ct = default)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // 1. Ensure the ledger day exists for today
            var today = DateOnly.FromDateTime(DateTime.Now);
            var ledger = await _ledgerDayRepo.GetByDateAsync(today, ct);

            if (ledger is null)
            {
                // Get current basin stock to use as opening stock for the new ledger
                var basin = await _basinRepo.GetSingletonAsync(ct);
                ledger = await _ledgerDayRepo.GetOrCreateAsync(today, basin.CurrentStock, ct);
                await _unitOfWork.SaveChangesAsync(ct); // Ensure ID is populated
            }


            // 2. Create the Sale domain object
            var saleResult = Sale.Create(
                ledger.Id,
                DateTime.Now,
                dto.BlocksSold,
                dto.UnitPrice,
                dto.CustomerName,
                dto.Notes);

            if (saleResult.IsFailure)
                return Result.Failure<SaleResultDto>(saleResult.Error);

            var sale = saleResult.Value;

            // 3. Register the sale on the ledger (raises SaleRecordedEvent)
            var recordResult = ledger.RecordSale(sale);
            if (recordResult.IsFailure)
                return Result.Failure<SaleResultDto>(recordResult.Error);

            // 4. Persist
            await _saleRepo.AddAsync(sale, ct);
            await _ledgerDayRepo.UpdateAsync(ledger, ct);

            // 5. Dispatch domain events (triggers basin stock deduction)
            foreach (var domainEvent in ledger.DomainEvents)
                await _eventDispatcher.DispatchAsync(domainEvent, ct);

            ledger.ClearDomainEvents();

            // 6. Commit
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            return Result.Success(new SaleResultDto(
                SaleId:       sale.Id,
                LedgerDayId:  sale.LedgerDayId,
                SaleTime:     sale.SaleTime,
                BlocksSold:   sale.BlocksSold,
                UnitPrice:    sale.UnitPrice.Amount,
                TotalAmount:  sale.TotalAmount.Amount,
                CustomerName: sale.CustomerName,
                Notes:        sale.Notes));
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// Returns all sales for a specific date.
    /// </summary>
    public async Task<Result<List<SaleResultDto>>> GetSalesByDateAsync(
        DateOnly date,
        CancellationToken ct = default)
    {
        var sales = await _saleRepo.GetByDateAsync(date, ct);

        var results = sales.Select(s => new SaleResultDto(
            SaleId:       s.Id,
            LedgerDayId:  s.LedgerDayId,
            SaleTime:     s.SaleTime,
            BlocksSold:   s.BlocksSold,
            UnitPrice:    s.UnitPrice.Amount,
            TotalAmount:  s.TotalAmount.Amount,
            CustomerName: s.CustomerName,
            Notes:        s.Notes)).ToList();

        return Result.Success(results);
    }
}
