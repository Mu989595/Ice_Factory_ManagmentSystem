using IcePlant.Application.DTOs;
using IcePlant.Domain.Common;
using IcePlant.Domain.Interfaces;
using IcePlant.Domain.Interfaces.Repositories;

namespace IcePlant.Application.Services;

/// <summary>
/// Provides real-time KPI data for the dashboard.
/// Aggregates today's sales, expenses, monthly profit, inventory status, and HR attendance.
/// </summary>
public class DashboardService
{
    private readonly ILedgerDayRepository       _ledgerDayRepo;
    private readonly IBasinRepository           _basinRepo;
    private readonly IWorkerRepository          _workerRepo;
    private readonly IAttendanceRepository      _attendanceRepo;
    private readonly IMonthlySummaryRepository  _monthlyRepo;
    private readonly IProductionCycleRepository _productionRepo;

    public DashboardService(
        ILedgerDayRepository       ledgerDayRepo,
        IBasinRepository           basinRepo,
        IWorkerRepository          workerRepo,
        IAttendanceRepository      attendanceRepo,
        IMonthlySummaryRepository  monthlyRepo,
        IProductionCycleRepository productionRepo)
    {
        _ledgerDayRepo  = ledgerDayRepo;
        _basinRepo      = basinRepo;
        _workerRepo     = workerRepo;
        _attendanceRepo = attendanceRepo;
        _monthlyRepo    = monthlyRepo;
        _productionRepo = productionRepo;
    }

    public async Task<Result<DashboardKpiDto>> GetDashboardAsync(CancellationToken ct = default)
    {
        var now   = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // ── Today's data ─────────────────────────────────────────────────────
        var todayLedger = await _ledgerDayRepo.GetByDateAsync(today, ct);

        decimal todaySalesCount = 0;
        decimal todayRevenue    = 0;
        decimal todayExpenses   = 0;
        int     todayBlocksSold = 0;

        if (todayLedger != null)
        {
            todaySalesCount = todayLedger.Sales.Count;
            todayRevenue    = todayLedger.Sales.Sum(s => s.TotalAmount.Amount);
            todayExpenses   = todayLedger.Expenses.Sum(e => e.Amount.Amount);
            todayBlocksSold = todayLedger.Sales.Sum(s => s.BlocksSold);
        }

        var todayDto = new DashboardTodayDto(
            Sales:      todaySalesCount,
            Revenue:    todayRevenue,
            Expenses:   todayExpenses,
            BlocksSold: todayBlocksSold);

        // ── This month's data ────────────────────────────────────────────────
        int year  = now.Year;
        int month = now.Month;

        decimal monthIncome   = await _ledgerDayRepo.GetTotalIncomeAsync(year, month, ct);
        decimal monthExpenses = await _ledgerDayRepo.GetTotalExpensesAsync(year, month, ct);
        decimal netProfit     = monthIncome - monthExpenses;

        // Check if month is closed with partner splits
        decimal partner1Share = 0;
        decimal partner2Share = 0;
        string  partner1Name  = "Partner 1";
        string  partner2Name  = "Partner 2";

        var monthlySummary = await _monthlyRepo.GetByMonthAsync(year, month, ct);
        if (monthlySummary?.ProfitSplits.Count >= 2)
        {
            var splits = monthlySummary.ProfitSplits.ToList();
            partner1Name  = splits[0].PartnerName;
            partner1Share = splits[0].AmountReceived.Amount;
            partner2Name  = splits[1].PartnerName;
            partner2Share = splits[1].AmountReceived.Amount;
        }
        else
        {
            // Default 50/50 estimate if month not yet closed
            partner1Share = netProfit / 2;
            partner2Share = netProfit / 2;
        }

        var monthDto = new DashboardMonthDto(
            TotalIncome:   monthIncome,
            TotalExpenses: monthExpenses,
            NetProfit:     netProfit,
            Partner1Share: partner1Share,
            Partner2Share: partner2Share,
            Partner1Name:  partner1Name,
            Partner2Name:  partner2Name);

        // ── Inventory ────────────────────────────────────────────────────────
        var basin = await _basinRepo.GetSingletonAsync(ct);
        var cycles = await _productionRepo.GetByDateAsync(today, ct);
        string? lastReplenishment = cycles.Count > 0
            ? cycles.OrderByDescending(c => c.TriggeredAt).First().TriggeredAt.ToString("yyyy-MM-dd HH:mm:ss")
            : null;

        var inventoryDto = new DashboardInventoryDto(
            CurrentBasinLevel:  basin.CurrentStock,
            MaxCapacity:        basin.MaxCapacity,
            FillPercent:        basin.FillPercent,
            LastReplenishment:  lastReplenishment);

        // ── HR ───────────────────────────────────────────────────────────────
        var workers       = await _workerRepo.GetAllActiveAsync(ct);
        var todayAttendance = await _attendanceRepo.GetByDateAsync(today, ct);

        int presentToday    = todayAttendance.Count(a => a.Attended);
        int absentToday     = todayAttendance.Count(a => !a.Attended);
        decimal totalWages  = todayAttendance.Where(a => a.Attended).Sum(a => a.WagePaid.Amount);

        var hrDto = new DashboardHRDto(
            PresentToday:   presentToday,
            AbsentToday:    absentToday,
            TotalWorkers:   workers.Count,
            TotalWagesToday: totalWages);

        return Result.Success(new DashboardKpiDto(
            Today:     todayDto,
            ThisMonth: monthDto,
            Inventory: inventoryDto,
            HR:        hrDto));
    }
}
