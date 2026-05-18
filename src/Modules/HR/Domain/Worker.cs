using IcePlant.Domain.Common;
using IcePlant.Domain.Enums;
using IcePlant.Domain.ValueObjects;

namespace IcePlant.Domain.Aggregates.HR;

/// <summary>
/// Represents a factory worker with a fixed daily role.
/// </summary>
public sealed class Worker : AggregateRoot
{
    public string     FullName   { get; private set; } = string.Empty;
    public WorkerRole Role       { get; private set; }
    public Money      DailyWage  { get; private set; } = Money.Zero();
    public bool       IsActive   { get; private set; }
    public DateOnly   HiredAt    { get; private set; }

    private Worker() { }

    public static Result<Worker> Create(
        string     fullName,
        WorkerRole role,
        decimal    dailyWage,
        DateOnly   hiredAt)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure<Worker>("Worker name cannot be empty.");
        if (dailyWage < 0)
            return Result.Failure<Worker>("Daily wage cannot be negative.");

        return Result.Success(new Worker
        {
            FullName  = fullName.Trim(),
            Role      = role,
            DailyWage = Money.Of(dailyWage),
            IsActive  = true,
            HiredAt   = hiredAt
        });
    }

    public Result UpdateDailyWage(decimal newWage)
    {
        if (newWage < 0)
            return Result.Failure("Daily wage cannot be negative.");
        DailyWage = Money.Of(newWage);
        return Result.Success();
    }

    public Result UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure("Worker name cannot be empty.");
        FullName = newName.Trim();
        return Result.Success();
    }

    public void Deactivate() => IsActive = false;
    public void Activate()   => IsActive = true;

    /// <summary>
    /// Arabic display name for the role.
    /// </summary>
    public string RoleArabic => Role switch
    {
        WorkerRole.WinchOperator => "وناش",
        WorkerRole.IcePusher     => "بيزق التلج",
        WorkerRole.IceStacker    => "بيرص التلج",
        _                        => "Unknown"
    };
}
