namespace IcePlant.Application.DTOs;

/// <summary>
/// Data Transfer Object — a simple "container" that the frontend sends us.
/// It carries only the raw data needed to record one worker's attendance.
/// No business logic lives here; that stays in the Domain.
/// </summary>
public sealed record AttendanceEntryDto(
    int     WorkerId,
    bool    Attended,
    string? Notes = null);

/// <summary>
/// What we send back to the frontend after recording attendance.
/// </summary>
public sealed record AttendanceResultDto(
    int     AttendanceId,
    int     WorkerId,
    string  WorkerName,
    bool    Attended,
    decimal WagePaid,
    string? Notes);
