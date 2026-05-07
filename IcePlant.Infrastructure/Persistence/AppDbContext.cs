using IcePlant.Domain.Aggregates.Basin;
using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Aggregates.HR;
using IcePlant.Domain.Aggregates.Monthly;
using Microsoft.EntityFrameworkCore;

namespace IcePlant.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── DbSets ───────────────────────────────────────────────────────────────
    public DbSet<BasinAggregate>      Basins             { get; set; }
    public DbSet<LedgerDay>           LedgerDays         { get; set; }
    public DbSet<Sale>                Sales              { get; set; }
    public DbSet<ExpenseCategory>     ExpenseCategories  { get; set; }
    public DbSet<Expense>             Expenses           { get; set; }
    public DbSet<Worker>              Workers            { get; set; }
    public DbSet<DailyAttendance>     DailyAttendances   { get; set; }
    public DbSet<MonthlySummary>      MonthlySummaries   { get; set; }
    public DbSet<ProductionCycle>     ProductionCycles   { get; set; }
    public DbSet<ProfitSplit>         ProfitSplits       { get; set; }

    // ── Model Configuration ──────────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration classes in this assembly automatically
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
