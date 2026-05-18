using IcePlant.Domain.Common;
using IcePlant.Domain.ValueObjects;

namespace IcePlant.Domain.Aggregates.HR;

/// <summary>
/// Records a worker's attendance for a specific day.
/// Links a Worker to a LedgerDay.
/// </summary>
public sealed class DailyAttendance : Entity
{
    public int     LedgerDayId { get; private set; }
    public int     WorkerId    { get; private set; }
    public bool    Attended    { get; private set; }
    public Money   WagePaid    { get; private set; } = Money.Zero();
    public string? Notes       { get; private set; }

    // Navigation properties (populated by EF Core)
    public Worker? Worker { get; private set; }

    private DailyAttendance() { }

    public static Result<DailyAttendance> Create(
        int     ledgerDayId,
        int     workerId,
        bool    attended,
        decimal wagePaid,
        string? notes = null)
    {
        if (ledgerDayId <= 0)
            return Result.Failure<DailyAttendance>("Invalid ledger day reference.");
        if (workerId <= 0)
            return Result.Failure<DailyAttendance>("Invalid worker reference.");
        if (wagePaid < 0)
            return Result.Failure<DailyAttendance>("Wage paid cannot be negative.");

        return Result.Success(new DailyAttendance
        {
            LedgerDayId = ledgerDayId,
            WorkerId    = workerId,
            Attended    = attended,
            WagePaid    = Money.Of(wagePaid),
            Notes       = notes?.Trim()
        });
    }

    public void MarkAbsent()
    {
        Attended = false;
        WagePaid = Money.Zero();
    }

    public void MarkPresent(decimal wage)
    {
        Attended = true;
        WagePaid = Money.Of(wage);
    }
}
