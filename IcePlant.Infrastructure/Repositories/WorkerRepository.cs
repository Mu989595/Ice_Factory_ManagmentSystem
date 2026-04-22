using IceFactoryManagmentSystem.Domain.Entities;
using IceFactoryManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IceFactoryManagmentSystem.Infrastructure.Repositories;

// ── Worker ────────────────────────────────────────────────────────────────────

public interface IWorkerRepository
{
    Task<Worker?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Worker>> GetAllActiveAsync(CancellationToken ct = default);
    Task AddAsync(Worker worker, CancellationToken ct = default);
    void Update(Worker worker);
}

public class WorkerRepository : BaseRepository<Worker>, IWorkerRepository
{
    public WorkerRepository(AppDbContext context) : base(context) { }

    public async Task<Worker?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.FindAsync([id], ct);

    public async Task<IReadOnlyList<Worker>> GetAllActiveAsync(CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(w => w.IsActive)
            .OrderBy(w => w.Role)
            .ToListAsync(ct);

    public async Task AddAsync(Worker worker, CancellationToken ct = default)
        => await _dbSet.AddAsync(worker, ct);

    public void Update(Worker worker)
        => _context.Entry(worker).State = EntityState.Modified;
}

// ── Daily Attendance ──────────────────────────────────────────────────────────

public interface IAttendanceRepository
{
    Task<IReadOnlyList<DailyAttendance>> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<DailyAttendance?> GetByDayAndWorkerAsync(int ledgerDayId, int workerId, CancellationToken ct = default);
    Task<bool> ExistsAsync(int ledgerDayId, int workerId, CancellationToken ct = default);
    Task AddAsync(DailyAttendance attendance, CancellationToken ct = default);
    void Update(DailyAttendance attendance);
}

public class AttendanceRepository : BaseRepository<DailyAttendance>, IAttendanceRepository
{
    public AttendanceRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<DailyAttendance>> GetByDateAsync(
        DateOnly date,
        CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Include(a => a.Worker)
            .Where(a => a.LedgerDay.DayDate == date)
            .OrderBy(a => a.Worker.Role)
            .ToListAsync(ct);

    public async Task<DailyAttendance?> GetByDayAndWorkerAsync(
        int ledgerDayId, int workerId,
        CancellationToken ct = default)
        => await _dbSet
            .FirstOrDefaultAsync(a => a.LedgerDayId == ledgerDayId
                                   && a.WorkerId    == workerId, ct);

    public async Task<bool> ExistsAsync(
        int ledgerDayId, int workerId,
        CancellationToken ct = default)
        => await _dbSet.AnyAsync(a => a.LedgerDayId == ledgerDayId
                                   && a.WorkerId    == workerId, ct);

    public async Task AddAsync(DailyAttendance attendance, CancellationToken ct = default)
        => await _dbSet.AddAsync(attendance, ct);

    public void Update(DailyAttendance attendance)
        => _context.Entry(attendance).State = EntityState.Modified;
}
