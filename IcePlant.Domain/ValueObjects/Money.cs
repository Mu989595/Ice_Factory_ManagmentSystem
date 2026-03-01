namespace IcePlant.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a monetary amount in EGP.
/// </summary>
public sealed record Money
{
    public decimal Amount   { get; }
    public string  Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount   = amount;
        Currency = currency;
    }

    public static Money Of(decimal amount, string currency = "EGP")
    {
        if (amount < 0)
            throw new Common.DomainException("Money amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new Common.DomainException("Currency cannot be empty.");

        return new Money(Math.Round(amount, 2), currency.ToUpper());
    }

    public static Money Zero(string currency = "EGP") => new(0m, currency);

    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money a, decimal multiplier)
        => new(Math.Round(a.Amount * multiplier, 2), a.Currency);

    public bool IsPositive  => Amount > 0;
    public bool IsNegative  => Amount < 0;
    public bool IsZero      => Amount == 0;

    public override string ToString() => $"{Amount:N2} {Currency}";

    private static void EnsureSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new Common.DomainException(
                $"Cannot operate on different currencies: {a.Currency} and {b.Currency}.");
    }
}
