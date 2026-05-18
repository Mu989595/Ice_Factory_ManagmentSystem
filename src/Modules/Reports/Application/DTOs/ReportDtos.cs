namespace IcePlant.Application.DTOs;

// ── Monthly Summary Report ───────────────────────────────────────────────────

public sealed record MonthlySummaryReportDto(
    int     Year,
    int     Month,
    decimal TotalSales,
    decimal TotalExpenses,
    decimal NetProfit,
    int     TotalBlocksSold,
    int     TotalDaysActive,
    List<ExpenseBreakdownDto> ExpenseBreakdown,
    List<PartnerShareDto> PartnerShares);

public sealed record ExpenseBreakdownDto(
    string  CategoryName,
    string  CategoryType,
    decimal TotalAmount,
    int     TransactionCount);

public sealed record PartnerShareDto(
    string  PartnerName,
    decimal Percentage,
    decimal Amount);

// ── Inventory Report ─────────────────────────────────────────────────────────

public sealed record InventoryReportDto(
    DateOnly FromDate,
    DateOnly ToDate,
    List<DailyBasinLevelDto> DailyLevels,
    int TotalReplenishments,
    int TotalBlocksProduced,
    int TotalBlocksSold);

public sealed record DailyBasinLevelDto(
    DateOnly Date,
    int      OpeningStock,
    int      ClosingStock,
    int      BlocksSold,
    int      ReplenishmentCount);

// ── HR Report ────────────────────────────────────────────────────────────────

public sealed record HRReportDto(
    DateOnly FromDate,
    DateOnly ToDate,
    List<WorkerAttendanceSummaryDto> Workers,
    decimal TotalWagesPaid,
    int     TotalAttendanceDays);

public sealed record WorkerAttendanceSummaryDto(
    int     WorkerId,
    string  WorkerName,
    string  Role,
    int     DaysPresent,
    int     DaysAbsent,
    decimal TotalWagePaid,
    decimal DailyWageRate);

// ── Dashboard KPI ────────────────────────────────────────────────────────────

public sealed record DashboardKpiDto(
    DashboardTodayDto    Today,
    DashboardMonthDto    ThisMonth,
    DashboardInventoryDto Inventory,
    DashboardHRDto        HR);

public sealed record DashboardTodayDto(
    decimal Sales,
    decimal Revenue,
    decimal Expenses,
    int     BlocksSold);

public sealed record DashboardMonthDto(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetProfit,
    decimal Partner1Share,
    decimal Partner2Share,
    string  Partner1Name,
    string  Partner2Name);

public sealed record DashboardInventoryDto(
    int      CurrentBasinLevel,
    int      MaxCapacity,
    double   FillPercent,
    string?  LastReplenishment);

public sealed record DashboardHRDto(
    int     PresentToday,
    int     AbsentToday,
    int     TotalWorkers,
    decimal TotalWagesToday);
