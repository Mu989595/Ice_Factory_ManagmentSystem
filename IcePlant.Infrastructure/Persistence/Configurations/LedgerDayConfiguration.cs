using IcePlant.Domain.Aggregates.Basin;
using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Aggregates.HR;
using IcePlant.Domain.Aggregates.Monthly;
using IcePlant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcePlant.Infrastructure.Persistence.Configurations;

public class LedgerDayConfiguration : IEntityTypeConfiguration<LedgerDay>
{
    public void Configure(EntityTypeBuilder<LedgerDay> builder)
    {
        builder.ToTable("LedgerDay");

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

        // â”€â”€ Relationships â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.HasMany(x => x.Sale)
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

