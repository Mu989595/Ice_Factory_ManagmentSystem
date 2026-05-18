using IcePlant.Application.DTOs;
using IcePlant.Domain.Common;
using IcePlant.Domain.Interfaces;
using IcePlant.Domain.Interfaces.Repositories;

namespace IcePlant.Application.Services;

/// <summary>
/// Generates reports by aggregating data from multiple repositories.
/// 
/// Three report types:
///   1. MonthlySummaryReport — total sales, expenses, net profit, expense breakdown, partner splits
///   2. InventoryReport — daily basin levels, replenishment count
///   3. HRReport — attendance per worker, total hours, calculated salary
/// </summary>
public class ReportService
{
    private readonly ILedgerDayRepository       _ledgerDayRepo;
    private readonly ISaleRepository            _saleRepo;
    private readonly IExpenseRepository         _expenseRepo;
    private readonly IExpenseCategoryRepository _categoryRepo;
    private readonly IBasinRepository           _basinRepo;
    private readonly IProductionCycleRepository _productionRepo;
    private readonly IWorkerRepository          _workerRepo;
    private readonly IAttendanceRepository      _attendanceRepo;
    private readonly IMonthlySummaryRepository  _monthlyRepo;

    public ReportService(
        ILedgerDayRepository       ledgerDayRepo,
        ISaleRepository            saleRepo,
        IExpenseRepository         expenseRepo,
        IExpenseCategoryRepository categoryRepo,
        IBasinRepository           basinRepo,
        IProductionCycleRepository productionRepo,
        IWorkerRepository          workerRepo,
        IAttendanceRepository      attendanceRepo,
        IMonthlySummaryRepository  monthlyRepo)
    {
        _ledgerDayRepo  = ledgerDayRepo;
        _saleRepo       = saleRepo;
        _expenseRepo    = expenseRepo;
        _categoryRepo   = categoryRepo;
        _basinRepo      = basinRepo;
        _productionRepo = productionRepo;
        _workerRepo     = workerRepo;
        _attendanceRepo = attendanceRepo;
        _monthlyRepo    = monthlyRepo;
    }

    // ── Monthly Summary Report ──────────────────────────────────────────────

    public async Task<Result<MonthlySummaryReportDto>> GetMonthlySummaryAsync(
        int year, int month, CancellationToken ct = default)
    {
        if (month < 1 || month > 12)
            return Result.Failure<MonthlySummaryReportDto>("Month must be between 1 and 12.");
        if (year < 2000 || year > 2100)
            return Result.Failure<MonthlySummaryReportDto>("Invalid year.");

        var from = new DateOnly(year, month, 1);
        var to   = from.AddMonths(1).AddDays(-1);

        // Get all ledger days in the month
        var ledgerDays = await _ledgerDayRepo.GetRangeAsync(from, to, ct);

        decimal totalSales    = ledgerDays.Sum(l => l.Sales.Sum(s => s.TotalAmount.Amount));
        decimal totalExpenses = ledgerDays.Sum(l => l.Expenses.Sum(e => e.Amount.Amount));
        int totalBlocksSold   = ledgerDays.Sum(l => l.Sales.Sum(s => s.BlocksSold));

        // Expense breakdown by category
        var allExpenses = ledgerDays.SelectMany(l => l.Expenses).ToList();
        var expenseBreakdown = allExpenses
            .GroupBy(e => new { e.CategoryId, CategoryName = e.Category?.Name ?? "Unknown", CategoryType = e.Category?.CategoryType.ToString() ?? "Unknown" })
            .Select(g => new ExpenseBreakdownDto(
                CategoryName:     g.Key.CategoryName,
                CategoryType:     g.Key.CategoryType,
                TotalAmount:      g.Sum(e => e.Amount.Amount),
                TransactionCount: g.Count()))
            .OrderByDescending(e => e.TotalAmount)
            .ToList();

        // Partner shares from closed monthly summary (if exists)
        var partnerShares = new List<PartnerShareDto>();
        var monthlySummary = await _monthlyRepo.GetByMonthAsync(year, month, ct);
        if (monthlySummary != null)
        {
            partnerShares = monthlySummary.ProfitSplits
                .Select(ps => new PartnerShareDto(
                    PartnerName: ps.PartnerName,
                    Percentage:  ps.SplitPercentage.Value,
                    Amount:      ps.AmountReceived.Amount))
                .ToList();
        }

        return Result.Success(new MonthlySummaryReportDto(
            Year:             year,
            Month:            month,
            TotalSales:       totalSales,
            TotalExpenses:    totalExpenses,
            NetProfit:        totalSales - totalExpenses,
            TotalBlocksSold:  totalBlocksSold,
            TotalDaysActive:  ledgerDays.Count,
            ExpenseBreakdown: expenseBreakdown,
            PartnerShares:    partnerShares));
    }

    // ── Inventory Report ────────────────────────────────────────────────────

    public async Task<Result<InventoryReportDto>> GetInventoryReportAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        if (from > to)
            return Result.Failure<InventoryReportDto>("From date must be before or equal to To date.");

        var ledgerDays = await _ledgerDayRepo.GetRangeAsync(from, to, ct);

        var dailyLevels = new List<DailyBasinLevelDto>();
        int totalReplenishments = 0;
        int totalBlocksProduced = 0;
        int totalBlocksSold     = 0;

        foreach (var ledger in ledgerDays)
        {
            var cycles = await _productionRepo.GetByDateAsync(ledger.DayDate, ct);
            int dayReplenishCount = cycles.Count;
            int dayBlocksProduced = cycles.Sum(c => c.BlocksAdded);
            int dayBlocksSold     = ledger.Sales.Sum(s => s.BlocksSold);

            dailyLevels.Add(new DailyBasinLevelDto(
                Date:               ledger.DayDate,
                OpeningStock:       ledger.OpeningStock,
                ClosingStock:       ledger.ClosingStock,
                BlocksSold:         dayBlocksSold,
                ReplenishmentCount: dayReplenishCount));

            totalReplenishments += dayReplenishCount;
            totalBlocksProduced += dayBlocksProduced;
            totalBlocksSold     += dayBlocksSold;
        }

        return Result.Success(new InventoryReportDto(
            FromDate:            from,
            ToDate:              to,
            DailyLevels:         dailyLevels,
            TotalReplenishments: totalReplenishments,
            TotalBlocksProduced: totalBlocksProduced,
            TotalBlocksSold:     totalBlocksSold));
    }

    // ── HR Report ───────────────────────────────────────────────────────────

    public async Task<Result<HRReportDto>> GetHRReportAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        if (from > to)
            return Result.Failure<HRReportDto>("From date must be before or equal to To date.");

        var workers = await _workerRepo.GetAllActiveAsync(ct);

        var workerSummaries = new List<WorkerAttendanceSummaryDto>();
        decimal totalWagesPaid = 0;
        int totalAttendanceDays = 0;

        foreach (var worker in workers)
        {
            int daysPresent = 0;
            int daysAbsent  = 0;
            decimal totalWage = 0;

            // Iterate through each day in the range
            var currentDate = from;
            while (currentDate <= to)
            {
                var dayAttendance = await _attendanceRepo.GetByDateAsync(currentDate, ct);
                var workerRecord = dayAttendance.FirstOrDefault(a => a.WorkerId == worker.Id);

                if (workerRecord != null)
                {
                    if (workerRecord.Attended)
                    {
                        daysPresent++;
                        totalWage += workerRecord.WagePaid.Amount;
                    }
                    else
                    {
                        daysAbsent++;
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            workerSummaries.Add(new WorkerAttendanceSummaryDto(
                WorkerId:     worker.Id,
                WorkerName:   worker.FullName,
                Role:         worker.Role.ToString(),
                DaysPresent:  daysPresent,
                DaysAbsent:   daysAbsent,
                TotalWagePaid: totalWage,
                DailyWageRate: worker.DailyWage.Amount));

            totalWagesPaid      += totalWage;
            totalAttendanceDays += daysPresent;
        }

        return Result.Success(new HRReportDto(
            FromDate:            from,
            ToDate:              to,
            Workers:             workerSummaries,
            TotalWagesPaid:      totalWagesPaid,
            TotalAttendanceDays: totalAttendanceDays));
    }
}
