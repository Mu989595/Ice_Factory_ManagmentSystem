using IceFactoryManagmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IceFactoryManagmentSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── DbSets ───────────────────────────────────────────────────────────────
    public DbSet<admin_users>         AdminUsers         { get; set; }
    public DbSet<BasinState>          BasinStates        { get; set; }
    public DbSet<ledger_days>         LedgerDays         { get; set; }
    public DbSet<Sales>               Sales              { get; set; }
    public DbSet<expense_categories>  ExpenseCategories  { get; set; }
    public DbSet<Expense>             Expenses           { get; set; }
    public DbSet<Worker>              Workers            { get; set; }
    public DbSet<DailyAttendance>     DailyAttendances   { get; set; }
    public DbSet<MonthlySummary>      MonthlySummaries   { get; set; }
    public DbSet<ProfitSplit>         ProfitSplits       { get; set; }
    public DbSet<ProductionCycle>     ProductionCycles   { get; set; }

    // ── Model Configuration ──────────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration classes in this assembly automatically
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
