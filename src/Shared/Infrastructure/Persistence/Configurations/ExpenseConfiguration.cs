using IcePlant.Domain.Aggregates.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcePlant.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.LedgerDayId)
               .IsRequired();

        builder.Property(x => x.CategoryId)
               .IsRequired();

        // Amount value object (Money)
        builder.OwnsOne(x => x.Amount, m =>
        {
            m.Property(p => p.Amount)
             .HasColumnName("Amount")
             .HasColumnType("decimal(12,2)")
             .IsRequired();
            m.Ignore(p => p.Currency);
        });

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_expenses_amount_positive", "[Amount] > 0"));

        builder.Property(x => x.ExpenseTime)
               .IsRequired();

        builder.Property(x => x.Supplier)
               .HasMaxLength(100);

        builder.Property(x => x.InvoiceRef)
               .HasMaxLength(50);

        builder.Property(x => x.Notes)
               .HasMaxLength(300);

        // Navigation back to category
        builder.HasOne(x => x.Category)
               .WithMany()
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
