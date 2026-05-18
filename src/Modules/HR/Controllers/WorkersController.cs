using Microsoft.AspNetCore.Authorization;
using IcePlant.Domain.Aggregates.HR;
using IcePlant.Domain.Enums;
using IcePlant.Domain.Interfaces;
using IcePlant.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace IceFactoryManagmentSystem.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorkersController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IWorkerRepository _workerRepo;

    public WorkersController(IUnitOfWork uow, IWorkerRepository workerRepo)
    {
        _uow = uow;
        _workerRepo = workerRepo;
    }

    /// <summary>
    /// Gets all active workers.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var workers = await _workerRepo.GetAllActiveAsync(ct);
        return Ok(workers.Select(w => new
        {
            w.Id,
            w.FullName,
            w.Role,
            w.RoleArabic,
            DailyWage = w.DailyWage.Amount,
            w.HiredAt
        }));
    }

    public record CreateWorkerDto(string FullName, WorkerRole Role, decimal DailyWage, DateOnly HiredAt);

    /// <summary>
    /// Creates a new worker.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWorker([FromBody] CreateWorkerDto dto, CancellationToken ct)
    {
        var result = Worker.Create(dto.FullName, dto.Role, dto.DailyWage, dto.HiredAt);

        if (result.IsFailure)
            return BadRequest(new { Error = result.Error });

        await _workerRepo.AddAsync(result.Value, ct);
        await _uow.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetAll), null, new { result.Value.Id });
    }

    /// <summary>
    /// Updates a worker's daily wage.
    /// </summary>
    [HttpPatch("{id}/wage")]
    public async Task<IActionResult> UpdateWage([FromRoute] int id, [FromBody] decimal newWage, CancellationToken ct)
    {
        var worker = await _workerRepo.GetByIdAsync(id, ct);
        if (worker == null) return NotFound();

        var result = worker.UpdateDailyWage(newWage);
        if (result.IsFailure)
            return BadRequest(new { Error = result.Error });

        await _workerRepo.UpdateAsync(worker, ct);
        await _uow.SaveChangesAsync(ct);

        return Ok(new { worker.Id, DailyWage = worker.DailyWage.Amount });
    }
}

