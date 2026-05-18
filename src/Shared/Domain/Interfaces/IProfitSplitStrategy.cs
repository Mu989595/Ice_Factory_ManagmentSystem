namespace IcePlant.Domain.Interfaces;

/// <summary>
/// Strategy pattern interface for profit splitting.
/// Two concrete implementations: EvenSplit (50/50) and CustomPercentageSplit.
/// </summary>
public interface IProfitSplitStrategy
{
    /// <summary>
    /// Calculates each partner's share given the net profit and partner list.
    /// </summary>
    /// <param name="netProfit">Total net profit to split.</param>
    /// <param name="partners">List of (PartnerName, Percentage) tuples. Percentages must sum to 100.</param>
    List<(string PartnerName, decimal Percentage, decimal Amount)> Calculate(
        decimal netProfit,
        List<(string Name, decimal Percentage)> partners);
}
