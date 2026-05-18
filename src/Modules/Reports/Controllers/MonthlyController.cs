using Microsoft.AspNetCore.Authorization;
using IcePlant.Domain.Aggregates.Monthly;
using IcePlant.Domain.Interfaces;
using IcePlant.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace IceFactoryManagmentSystem.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MonthlyController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMonthlySummaryRepository _monthlyRepo;
    private readonly ILedgerDayRepository _ledgerRepo;

    public MonthlyController(IUnitOfWork uow, IMonthlySummaryRepository monthlyRepo, ILedgerDayRepository ledgerRepo)
    {
        _uow = uow;
        _monthlyRepo = monthlyRepo;
        _ledgerRepo = ledgerRepo;
    }

    public record ProfitSplitDto(string PartnerName, decimal Percentage);
    public record CloseMonthDto(int Year, int Month, List<ProfitSplitDto> Splits);

    /// <summary>
    /// Closes a month, calculating total income and expenses, and splitting profit.
    /// </summary>
    [HttpPost("close")]
    public async Task<IActionResult> CloseMonth([FromBody] CloseMonthDto dto, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            if (await _monthlyRepo.IsMonthClosedAsync(dto.Year, dto.Month, ct))
                return BadRequest(new { Error = "Month is already closed." });

            var totalIncome = await _ledgerRepo.GetTotalIncomeAsync(dto.Year, dto.Month, ct);
            var totalExpenses = await _ledgerRepo.GetTotalExpensesAsync(dto.Year, dto.Month, ct);

            var summaryResult = MonthlySummary.Create(dto.Year, dto.Month, totalIncome, totalExpenses);
            if (summaryResult.IsFailure)
                return BadRequest(new { Error = summaryResult.Error });

            var summary = summaryResult.Value;

            var splits = dto.Splits.Select(s => (s.PartnerName, s.Percentage)).ToList();
            var splitsResult = summary.AddProfitSplits(splits);
            if (splitsResult.IsFailure)
                return BadRequest(new { Error = splitsResult.Error });

            var closeResult = summary.Close();
            if (closeResult.IsFailure)
                return BadRequest(new { Error = closeResult.Error });

            await _monthlyRepo.AddAsync(summary, ct);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            return Ok(new
            {
                summary.Id,
                summary.Year,
                summary.Month,
                TotalIncome = summary.TotalIncome.Amount,
                TotalExpenses = summary.TotalExpenses.Amount,
                NetProfit = summary.NetProfit.Amount,
                Splits = summary.ProfitSplits.Select(s => new { s.PartnerName, s.SplitPercentage.Value, s.AmountReceived.Amount })
            });
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// Gets the financial summary for a specific month.
    /// </summary>
    [HttpGet("{year}/{month}")]
    public async Task<IActionResult> GetSummary([FromRoute] int year, [FromRoute] int month, CancellationToken ct)
    {
        var summary = await _monthlyRepo.GetByMonthAsync(year, month, ct);
        if (summary == null)
            return NotFound(new { Error = "Summary not found for the specified month." });

        return Ok(new
        {
            summary.Id,
            summary.Year,
            summary.Month,
            TotalIncome = summary.TotalIncome.Amount,
            TotalExpenses = summary.TotalExpenses.Amount,
            NetProfit = summary.NetProfit.Amount,
            summary.IsClosed,
            summary.ClosedAt,
            Splits = summary.ProfitSplits.Select(s => new { s.PartnerName, Percentage = s.SplitPercentage.Value, Amount = s.AmountReceived.Amount })
        });
    }
}

