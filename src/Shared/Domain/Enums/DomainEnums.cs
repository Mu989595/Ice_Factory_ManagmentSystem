namespace IcePlant.Domain.Enums;

/// <summary>
/// The three fixed worker roles in the ice factory.
/// </summary>
public enum WorkerRole
{
    /// <summary>وناش — Winch Operator</summary>
    WinchOperator = 1,

    /// <summary>بيزق التلج — Ice Pusher</summary>
    IcePusher = 2,

    /// <summary>بيرص التلج — Ice Stacker</summary>
    IceStacker = 3
}

/// <summary>
/// Top-level expense category type.
/// </summary>
public enum ExpenseCategoryType
{
    GeneralExpense = 1,
    UtilityBill    = 2
}

/// <summary>
/// Specific utility bill types that must be tracked individually.
/// </summary>
public enum UtilityBillType
{
    Water       = 1,
    Electricity = 2,
    Ammonia     = 3,
    Salt        = 4
}

/// <summary>
/// Reason a production/replenishment cycle was triggered.
/// </summary>
public enum ReplenishmentTrigger
{
    AutoTimer = 1,
    Manual    = 2,
    Rollover  = 3
}
