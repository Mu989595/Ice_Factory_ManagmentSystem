using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcePlant.Infrastructure.Persistence.Configurations;

public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.ToTable("ExpenseCategories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(60);

        builder.HasIndex(x => x.Name)
               .IsUnique();

        // Store enum as its string name so it is human-readable in the DB
        builder.Property(x => x.CategoryType)
               .HasConversion<string>()
               .IsRequired()
               .HasMaxLength(30);

        // Nullable enum stored as string
        builder.Property(x => x.UtilityType)
               .HasConversion<string>()
               .HasMaxLength(30);

        builder.Property(x => x.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        // Navigation to expenses
        builder.HasMany<IcePlant.Domain.Aggregates.Finance.Expense>()
               .WithOne(e => e.Category)
               .HasForeignKey(e => e.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
