using IceFactoryManagmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceFactoryManagmentSystem.Infrastructure.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sales>
{
    public void Configure(EntityTypeBuilder<Sales> builder)
    {
        builder.ToTable("sales");

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

        // ── Index: fast lookup of last sale per day (used by replenishment) ─
        builder.HasIndex(x => new { x.LedgerDayId, x.SaleTime });
    }
}
