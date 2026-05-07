using IcePlant.Domain.Aggregates.HR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcePlant.Infrastructure.Persistence.Configurations;

public class WorkerConfiguration : IEntityTypeConfiguration<Worker>
{
    public void Configure(EntityTypeBuilder<Worker> builder)
    {
        // 1. Table Name
        builder.ToTable("Workers");

        // 2. Primary Key
        builder.HasKey(w => w.Id);

        // 3. Simple Properties
        builder.Property(w => w.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(w => w.Role)
            .HasConversion<string>() // Saves enum as "WinchOperator" instead of 0, 1, 2
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.IsActive)
            .IsRequired();

        builder.Property(w => w.HiredAt)
            .IsRequired();

        // 4. Value Objects (Money)
        // Entity Framework will map these into columns: "DailyWageAmount" and "DailyWageCurrency"
        builder.OwnsOne(w => w.DailyWage, wage =>
        {
            wage.Property(m => m.Amount)
                .HasColumnName("DailyWageAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            
            wage.Property(m => m.Currency)
                .HasColumnName("DailyWageCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });
    }
}

