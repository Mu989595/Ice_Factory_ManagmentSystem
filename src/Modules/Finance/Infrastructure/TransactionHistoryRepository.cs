using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Interfaces.Repositories;
using IcePlant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcePlant.Infrastructure.Repositories;

public sealed class TransactionHistoryRepository
    : BaseRepository<TransactionHistory>, ITransactionHistoryRepository
{
    public TransactionHistoryRepository(AppDbContext context) : base(context) { }

    public async Task<List<TransactionHistory>> GetByLedgerDayIdAsync(
        int ledgerDayId,
        CancellationToken ct = default)
        => await _dbSet
            .Where(t => t.LedgerDayId == ledgerDayId)
            .OrderBy(t => t.OccurredAt)
            .ToListAsync(ct);
}
