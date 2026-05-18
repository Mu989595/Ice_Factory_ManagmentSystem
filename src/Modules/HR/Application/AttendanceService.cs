using IcePlant.Application.DTOs;
using IcePlant.Domain.Aggregates.HR;
using IcePlant.Domain.Common;
using IcePlant.Domain.Interfaces;
using IcePlant.Domain.Interfaces.Repositories;

namespace IcePlant.Application.Services;

/// <summary>
/// Handles all business logic for recording daily worker attendance.
/// 
/// What this service does:
///   1. Validates that the LedgerDay exists (so we know WHICH day we're recording).
///   2. For each worker entry, checks if attendance was already recorded (no duplicates).
///   3. If the worker attended → pays them their daily wage.
///   4. If the worker was absent → wage = 0.
///   5. Saves everything to the database in one transaction.
/// </summary>
public class AttendanceService
{
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly IWorkerRepository     _workerRepo;
    private readonly ILedgerDayRepository  _ledgerDayRepo;
    private readonly IUnitOfWork           _unitOfWork;

    public AttendanceService(
        IAttendanceRepository attendanceRepo,
        IWorkerRepository     workerRepo,
        ILedgerDayRepository  ledgerDayRepo,
        IUnitOfWork           unitOfWork)
    {
        _attendanceRepo = attendanceRepo;
        _workerRepo     = workerRepo;
        _ledgerDayRepo  = ledgerDayRepo;
        _unitOfWork     = unitOfWork;
    }

    /// <summary>
    /// Records attendance for multiple workers on a specific LedgerDay.
    /// Returns a list of results showing each worker's name and wage paid.
    /// </summary>
    public async Task<Result<List<AttendanceResultDto>>> RecordDailyAttendanceAsync(
        int ledgerDayId,
        List<AttendanceEntryDto> entries,
        CancellationToken ct = default)
    {
        // ── Guard: must have at least one entry ──────────────────────────
        if (entries is null || entries.Count == 0)
            return Result.Failure<List<AttendanceResultDto>>("No attendance entries provided.");

        var results = new List<AttendanceResultDto>();

        // ── Begin a transaction so all records save (or none do) ─────────
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            foreach (var entry in entries)
            {
                // 1. Check if this worker already has attendance for this day
                var alreadyExists = await _attendanceRepo.ExistsAsync(ledgerDayId, entry.WorkerId, ct);
                if (alreadyExists)
                {
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result.Failure<List<AttendanceResultDto>>(
                        $"Attendance already recorded for worker ID {entry.WorkerId} on this day.");
                }

                // 2. Get the worker so we know their daily wage
                var worker = await _workerRepo.GetByIdAsync(entry.WorkerId, ct);
                if (worker is null)
                {
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result.Failure<List<AttendanceResultDto>>(
                        $"Worker with ID {entry.WorkerId} not found.");
                }

                // 3. Calculate wage: if they attended → pay daily wage, else → 0
                var wageToPay = entry.Attended ? worker.DailyWage.Amount : 0m;

                // 4. Create the DailyAttendance domain object (this runs validation inside)
                var attendanceResult = DailyAttendance.Create(
                    ledgerDayId,
                    entry.WorkerId,
                    entry.Attended,
                    wageToPay,
                    entry.Notes);

                if (!attendanceResult.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return Result.Failure<List<AttendanceResultDto>>(attendanceResult.Error!);
                }

                // 5. Save to repository
                await _attendanceRepo.AddAsync(attendanceResult.Value!, ct);

                // 6. Build result DTO
                results.Add(new AttendanceResultDto(
                    AttendanceId: attendanceResult.Value!.Id,
                    WorkerId:     worker.Id,
                    WorkerName:   worker.FullName,
                    Attended:     entry.Attended,
                    WagePaid:     wageToPay,
                    Notes:        entry.Notes));
            }

            // ── All good → commit everything ────────────────────────────
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            return Result.Success(results);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// Gets all attendance records for a specific date.
    /// Useful for showing "Today's Attendance" on the frontend.
    /// </summary>
    public async Task<Result<List<AttendanceResultDto>>> GetAttendanceByDateAsync(
        DateOnly date,
        CancellationToken ct = default)
    {
        var records = await _attendanceRepo.GetByDateAsync(date, ct);

        var results = new List<AttendanceResultDto>();
        foreach (var record in records)
        {
            var worker = await _workerRepo.GetByIdAsync(record.WorkerId, ct);
            results.Add(new AttendanceResultDto(
                AttendanceId: record.Id,
                WorkerId:     record.WorkerId,
                WorkerName:   worker?.FullName ?? "Unknown",
                Attended:     record.Attended,
                WagePaid:     record.WagePaid.Amount,
                Notes:        record.Notes));
        }

        return Result.Success(results);
    }
}
