using IcePlant.Domain.Aggregates.Basin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcePlant.Infrastructure.Persistence.Configurations;

public class BasinStateConfiguration : IEntityTypeConfiguration<BasinAggregate>
{
    public void Configure(EntityTypeBuilder<BasinAggregate> builder)
    {
        builder.ToTable("BasinStates");

        builder.HasKey(x => x.Id);

        // Enforce singleton — only Id = 1 is ever allowed
        builder.Property(x => x.Id)
               .ValueGeneratedNever();

        builder.Property(x => x.CurrentStock)
               .IsRequired();

        builder.Property(x => x.MaxCapacity)
               .IsRequired();

        builder.Property(x => x.FreezeHours)
               .IsRequired()
               .HasColumnType("float");

        builder.Property(x => x.LastUpdatedAt)
               .IsRequired()
               .HasDefaultValueSql("GETUTCDATE()");

        // Database-level constraint: only one row with Id = 1 can exist
        builder.ToTable(t => t.HasCheckConstraint("CK_basin_singleton", "[Id] = 1"));
    }
}
