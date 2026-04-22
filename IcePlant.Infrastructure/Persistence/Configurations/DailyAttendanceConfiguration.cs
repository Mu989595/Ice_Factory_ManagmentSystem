using IcePlant.Domain.Aggregates.HR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcePlant.Infrastructure.Persistence.Configurations;

public class DailyAttendanceConfiguration : IEntityTypeConfiguration<DailyAttendance>
{
    public void Configure(EntityTypeBuilder<DailyAttendance> builder)
    {
        // 1. Table Name
        builder.ToTable("DailyAttendances");

        // 2. Primary Key
        builder.HasKey(a => a.Id);

        // 3. Simple Properties
        builder.Property(a => a.LedgerDayId)
            .IsRequired();

        builder.Property(a => a.WorkerId)
            .IsRequired();

        builder.Property(a => a.Attended)
            .IsRequired();

        builder.Property(a => a.Notes)
            .HasMaxLength(500);

        // 4. Value Objects (Money)
        builder.OwnsOne(a => a.WagePaid, wage =>
        {
            wage.Property(m => m.Amount)
                .HasColumnName("WagePaidAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            wage.Property(m => m.Currency)
                .HasColumnName("WagePaidCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // 5. Relationships
        builder.HasOne(a => a.Worker)
            .WithMany()
            .HasForeignKey(a => a.WorkerId)
            .OnDelete(DeleteBehavior.Restrict); // Don't let deleting a worker delete the attendance history!
    }
}
