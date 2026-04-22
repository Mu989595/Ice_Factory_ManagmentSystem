using IceFactoryManagmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceFactoryManagmentSystem.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.LedgerDayId)
               .IsRequired();

        builder.Property(x => x.CategoryId)
               .IsRequired();

        builder.Property(x => x.Amount)
               .IsRequired()
               .HasColumnType("decimal(12,2)");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_expenses_amount_positive", "[Amount] > 0"));

        builder.Property(x => x.ExpenseTime)
               .IsRequired();

        builder.Property(x => x.Supplier)
               .HasMaxLength(100);

        builder.Property(x => x.InvoiceRef)
               .HasMaxLength(80);

        builder.Property(x => x.Notes)
               .HasMaxLength(300);

        // ── Index: fast monthly expense aggregation ──────────────────────────
        builder.HasIndex(x => new { x.LedgerDayId, x.CategoryId });
    }
}
