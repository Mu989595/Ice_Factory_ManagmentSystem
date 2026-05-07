using IcePlant.Domain.Aggregates.Basin;
using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Aggregates.HR;
using IcePlant.Domain.Aggregates.Monthly;
using IcePlant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcePlant.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sale");

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

        builder.Property(x => x.UnitPrice)
               .IsRequired()
               .HasColumnType("decimal(10,2)");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_sales_price_positive", "[UnitPrice] > 0"));

        builder.Property(x => x.TotalAmount)
               .IsRequired()
               .HasColumnType("decimal(12,2)");

        builder.Property(x => x.CustomerName)
               .HasMaxLength(100);

        builder.Property(x => x.Notes)
               .HasMaxLength(300);

        // â”€â”€ Index: fast lookup of last sale per day (used by replenishment) â”€
        builder.HasIndex(x => new { x.LedgerDayId, x.SaleTime });
    }
}

