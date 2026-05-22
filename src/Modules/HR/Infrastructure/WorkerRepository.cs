using IcePlant.Domain.Aggregates.HR;
using IcePlant.Domain.Interfaces.Repositories;
using IcePlant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcePlant.Infrastructure.Repositories;

// ── Worker ────────────────────────────────────────────────────────────────────

public class WorkerRepository : BaseRepository<Worker>, IWorkerRepository
{
    public WorkerRepository(AppDbContext context) : base(context) { }

    public new async Task<Worker?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.FindAsync([id], ct);

    public async Task<IReadOnlyList<Worker>> GetAllActiveAsync(CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(w => w.IsActive)
            .OrderBy(w => w.Role)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Worker>> GetByRoleAsync(
        IcePlant.Domain.Enums.WorkerRole role,
        CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(w => w.Role == role && w.IsActive)
            .ToListAsync(ct);

    public async Task AddAsync(Worker worker, CancellationToken ct = default)
        => await _dbSet.AddAsync(worker, ct);

    public async Task UpdateAsync(Worker worker, CancellationToken ct = default)
    {
        _context.Entry(worker).State = EntityState.Modified;
        await Task.CompletedTask;
    }
}

// ── Daily Attendance ──────────────────────────────────────────────────────────

public class AttendanceRepository : BaseRepository<DailyAttendance>, IAttendanceRepository
{
    public AttendanceRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<DailyAttendance>> GetByDateAsync(
        DateOnly date,
        CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Include(a => a.Worker)
            .Where(a => _context.LedgerDays
                .Where(l => l.DayDate == date)
                .Select(l => l.Id)
                .Contains(a.LedgerDayId))
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(
        int ledgerDayId, int workerId,
        CancellationToken ct = default)
        => await _dbSet.AnyAsync(a => a.LedgerDayId == ledgerDayId
                                   && a.WorkerId    == workerId, ct);

    public async Task AddAsync(DailyAttendance attendance, CancellationToken ct = default)
        => await _dbSet.AddAsync(attendance, ct);

    public async Task UpdateAsync(DailyAttendance attendance, CancellationToken ct = default)
    {
        _context.Entry(attendance).State = EntityState.Modified;
        await Task.CompletedTask;
    }
}
