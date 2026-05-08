using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcePlant.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.LedgerDayId)
               .IsRequired();

        builder.Property(x => x.SaleTime)
               .IsRequired();

        builder.Property(x => x.BlocksSold)
               .IsRequired();

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_sales_blocks_positive", "[BlocksSold] > 0"));

        // UnitPrice value object (Money)
        builder.OwnsOne(x => x.UnitPrice, m =>
        {
            m.Property(p => p.Amount)
             .HasColumnName("UnitPrice")
             .HasColumnType("decimal(10,2)")
             .IsRequired();
            m.Ignore(p => p.Currency);
        });

        // TotalAmount value object (Money)
        builder.OwnsOne(x => x.TotalAmount, m =>
        {
            m.Property(p => p.Amount)
             .HasColumnName("TotalAmount")
             .HasColumnType("decimal(12,2)")
             .IsRequired();
            m.Ignore(p => p.Currency);
        });

        builder.Property(x => x.CustomerName)
               .HasMaxLength(100);

        builder.Property(x => x.Notes)
               .HasMaxLength(300);

        // Fast lookup of last sale per day (used by replenishment)
        builder.HasIndex(x => new { x.LedgerDayId, x.SaleTime });
    }
}
