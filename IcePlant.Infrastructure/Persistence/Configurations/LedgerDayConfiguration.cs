using IceFactoryManagmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceFactoryManagmentSystem.Infrastructure.Persistence.Configurations;

public class LedgerDayConfiguration : IEntityTypeConfiguration<ledger_days>
{
    public void Configure(EntityTypeBuilder<ledger_days> builder)
    {
        builder.ToTable("ledger_days");

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

        // ── Relationships ───────────────────────────────────────────────────
        builder.HasMany(x => x.Sales)
               .WithOne(s => s.LedgerDay)
               .HasForeignKey(s => s.LedgerDayId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Expenses)
               .WithOne(e => e.LedgerDay)
               .HasForeignKey(e => e.LedgerDayId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.DailyAttendances)
               .WithOne(a => a.LedgerDay)
               .HasForeignKey(a => a.LedgerDayId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ProductionCycles)
               .WithOne(p => p.LedgerDay)
               .HasForeignKey(p => p.LedgerDayId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
