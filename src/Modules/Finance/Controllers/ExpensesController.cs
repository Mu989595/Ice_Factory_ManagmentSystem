using IcePlant.Application.DTOs;
using IcePlant.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IceFactoryManagmentSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly ExpenseService _expenseService;

    public ExpensesController(ExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    /// <summary>
    /// Records a new expense for today's ledger.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RecordExpense(
        [FromBody] RecordExpenseDto dto,
        CancellationToken ct)
    {
        var result = await _expenseService.RecordExpenseAsync(dto, ct);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetExpensesByDate),
                new { date = DateTime.UtcNow.ToString("yyyy-MM-dd") },
                result.Value);

        return BadRequest(new { Error = result.Error });
    }

    /// <summary>
    /// Gets all expenses for a specific date.
    /// Format for date parameter: YYYY-MM-DD
    /// </summary>
    [HttpGet("date/{date}")]
    public async Task<IActionResult> GetExpensesByDate(
        [FromRoute] DateOnly date,
        CancellationToken ct)
    {
        var result = await _expenseService.GetExpensesByDateAsync(date, ct);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new { Error = result.Error });
    }

    /// <summary>
    /// Gets all expenses for a specific month.
    /// </summary>
    [HttpGet("month/{year}/{month}")]
    public async Task<IActionResult> GetExpensesByMonth(
        [FromRoute] int year,
        [FromRoute] int month,
        CancellationToken ct)
    {
        var result = await _expenseService.GetExpensesByMonthAsync(year, month, ct);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new { Error = result.Error });
    }

    /// <summary>
    /// Returns all active expense categories (for populating dropdowns in the frontend).
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var result = await _expenseService.GetCategoriesAsync(ct);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new { Error = result.Error });
    }
}
