namespace IcePlant.Domain.ValueObjects;

/// <summary>
/// Represents a profit-split percentage (1–100).
/// </summary>
public sealed record SplitPercentage
{
    public decimal Value { get; }

    private SplitPercentage(decimal value) => Value = value;

    public static SplitPercentage Of(decimal value)
    {
        if (value <= 0 || value > 100)
            throw new Common.DomainException(
                $"Split percentage must be between 1 and 100. Received: {value}");

        return new SplitPercentage(Math.Round(value, 2));
    }

    public static SplitPercentage Fifty  => Of(50m);
    public static SplitPercentage Forty  => Of(40m);
    public static SplitPercentage Sixty  => Of(60m);

    /// <summary>
    /// Validates that a list of percentages sums to exactly 100.
    /// </summary>
    public static bool ValidateSum(IEnumerable<SplitPercentage> percentages)
        => Math.Abs(percentages.Sum(p => p.Value) - 100m) < 0.01m;

    public override string ToString() => $"{Value:N2}%";
}
