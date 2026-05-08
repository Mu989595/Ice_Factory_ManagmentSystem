using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Aggregates.HR;
using IcePlant.Domain.Aggregates.Basin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcePlant.Infrastructure.Persistence.Configurations;

public class LedgerDayConfiguration : IEntityTypeConfiguration<LedgerDay>
{
    public void Configure(EntityTypeBuilder<LedgerDay> builder)
    {
        builder.ToTable("LedgerDays");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        // One row per calendar day
        builder.Property(x => x.DayDate)
               .IsRequired()
               .HasColumnType("date");

        builder.HasIndex(x => x.DayDate)
               .IsUnique();

        builder.Property(x => x.OpeningStock)
               .IsRequired()
               .HasDefaultValue(0);

        builder.Property(x => x.ClosingStock)
               .IsRequired()
               .HasDefaultValue(0);

        builder.Property(x => x.Notes)
               .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
               .IsRequired()
               .HasDefaultValueSql("GETUTCDATE()");

        // ── Relationships ────────────────────────────────────────────────────

        // Sales: LedgerDay owns a collection of Sales (private backing field)
        builder.HasMany(x => x.Sales)
               .WithOne()
               .HasForeignKey(s => s.LedgerDayId)
               .OnDelete(DeleteBehavior.Restrict);

        // Expenses: LedgerDay owns a collection of Expenses (private backing field)
        builder.HasMany(x => x.Expenses)
               .WithOne()
               .HasForeignKey(e => e.LedgerDayId)
               .OnDelete(DeleteBehavior.Restrict);

        // DailyAttendance: references LedgerDayId (no nav property on LedgerDay)
        builder.HasMany<DailyAttendance>()
               .WithOne()
               .HasForeignKey(a => a.LedgerDayId)
               .OnDelete(DeleteBehavior.Restrict);

        // ProductionCycle: references LedgerDayId (no nav property on LedgerDay)
        builder.HasMany<ProductionCycle>()
               .WithOne()
               .HasForeignKey(p => p.LedgerDayId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
