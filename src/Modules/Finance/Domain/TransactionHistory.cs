using IcePlant.Domain.Common;
using IcePlant.Domain.Enums;

namespace IcePlant.Domain.Aggregates.Finance;

/// <summary>
/// كل عملية بيعة أو مصروف أو تعبئة تترسم سطر في الجدول ده.
/// Immutable audit trail of every financial transaction.
/// </summary>
public sealed class TransactionHistory : Entity
{
    public int             LedgerDayId { get; private set; }
    public DateTime        OccurredAt  { get; private set; }
    public TransactionType Type        { get; private set; }
    public decimal         Amount      { get; private set; }
    public string?         Notes       { get; private set; }
    public int?            ReferenceId { get; private set; } // SaleId أو ExpenseId

    private TransactionHistory() { } // EF Core

    public static TransactionHistory Create(
        int             ledgerDayId,
        DateTime        occurredAt,
        TransactionType type,
        decimal         amount,
        int?            referenceId = null,
        string?         notes       = null)
    {
        return new TransactionHistory
        {
            LedgerDayId = ledgerDayId,
            OccurredAt  = occurredAt,
            Type        = type,
            Amount      = amount,
            ReferenceId = referenceId,
            Notes       = notes
        };
    }
}
