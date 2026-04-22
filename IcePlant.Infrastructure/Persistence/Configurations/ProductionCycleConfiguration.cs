using IceFactoryManagmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceFactoryManagmentSystem.Infrastructure.Persistence.Configurations;

public class ProductionCycleConfiguration : IEntityTypeConfiguration<ProductionCycle>
{
    public void Configure(EntityTypeBuilder<ProductionCycle> builder)
    {
        builder.ToTable("production_cycles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.LedgerDayId)
               .IsRequired();

        builder.Property(x => x.TriggeredAt)
               .IsRequired();

        builder.Property(x => x.TriggerReason)
               .IsRequired()
               .HasMaxLength(30);

        builder.Property(x => x.BlocksAdded)
               .IsRequired();

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_production_blocks_positive", "[BlocksAdded] > 0"));

        builder.Property(x => x.StockBefore)
               .IsRequired();

        builder.Property(x => x.StockAfter)
               .IsRequired();

        // Fast lookup: "did a replenishment fire after time X today?"
        builder.HasIndex(x => new { x.LedgerDayId, x.TriggeredAt });
    }
}
