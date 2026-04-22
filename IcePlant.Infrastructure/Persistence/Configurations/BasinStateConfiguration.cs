using IceFactoryManagmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceFactoryManagmentSystem.Infrastructure.Persistence.Configurations;

public class BasinStateConfiguration : IEntityTypeConfiguration<BasinState>
{
    public void Configure(EntityTypeBuilder<BasinState> builder)
    {
        builder.ToTable("basin_states");

        builder.HasKey(x => x.Id);

        // Enforce singleton — only Id = 1 is ever allowed
        builder.Property(x => x.Id)
               .ValueGeneratedNever(); // we control the Id manually

        builder.Property(x => x.CurrentStock)
               .IsRequired();

        builder.Property(x => x.MaxCapacity)
               .IsRequired();

        builder.Property(x => x.FreezeHours)
               .IsRequired()
               .HasColumnType("decimal(4,1)");

        builder.Property(x => x.LastUpdatedAt)
               .IsRequired()
               .HasDefaultValueSql("GETUTCDATE()");

        // Database-level constraint: only one row with Id = 1 can exist
        builder.ToTable(t => t.HasCheckConstraint("CK_basin_singleton", "[Id] = 1"));

        // Seed the default basin (1000 blocks capacity, 8-hour freeze cycle)
        builder.HasData(new BasinState
        {
            Id           = 1,
            CurrentStock = 1000,
            MaxCapacity  = 1000,
            FreezeHours  = 8.0m,
            LastUpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
