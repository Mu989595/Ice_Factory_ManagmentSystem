using IcePlant.Domain.Common;
using IcePlant.Domain.ValueObjects;

namespace IcePlant.Domain.Aggregates.Monthly;

/// <summary>
/// Represents one partner's share in a month's profit split.
/// Child entity of MonthlySummary.
/// </summary>
public sealed class ProfitSplit : Entity
{
    public int             MonthlySummaryId  { get; private set; }
    public string          PartnerName       { get; private set; } = string.Empty;
    public SplitPercentage SplitPercentage   { get; private set; } = null!;
    public Money           AmountReceived    { get; private set; } = Money.Zero();

    private ProfitSplit() { }

    public static Result<ProfitSplit> Create(
        int             monthlySummaryId,
        string          partnerName,
        decimal         splitPercentage,
        decimal         netProfit)
    {
        if (monthlySummaryId <= 0)
            return Result.Failure<ProfitSplit>("Invalid monthly summary reference.");
        if (string.IsNullOrWhiteSpace(partnerName))
            return Result.Failure<ProfitSplit>("Partner name cannot be empty.");

        SplitPercentage pct;
        try   { pct = ValueObjects.SplitPercentage.Of(splitPercentage); }
        catch (DomainException ex)
              { return Result.Failure<ProfitSplit>(ex.Message); }

        var amount = Money.Of(netProfit * pct.Value / 100m);

        return Result.Success(new ProfitSplit
        {
            MonthlySummaryId = monthlySummaryId,
            PartnerName      = partnerName.Trim(),
            SplitPercentage  = pct,
            AmountReceived   = amount
        });
    }
}
