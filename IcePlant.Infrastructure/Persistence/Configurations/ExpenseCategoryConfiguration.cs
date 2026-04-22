using IceFactoryManagmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceFactoryManagmentSystem.Infrastructure.Persistence.Configurations;

public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<expense_categories>
{
    public void Configure(EntityTypeBuilder<expense_categories> builder)
    {
        builder.ToTable("expense_categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.ParentType)
               .IsRequired()
               .HasMaxLength(30);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(60);

        builder.HasIndex(x => x.Name)
               .IsUnique();

        builder.Property(x => x.IsUtility)
               .IsRequired()
               .HasDefaultValue(false);

        // Navigation to expenses
        builder.HasMany(x => x.Expenses)
               .WithOne(e => e.Category)
               .HasForeignKey(e => e.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        // ── Seed Data ───────────────────────────────────────────────────────
        builder.HasData(
            // Utility Bills — tracked individually per requirement
            new expense_categories { Id = 1, ParentType = "UtilityBill",    Name = "Water",       IsUtility = true  },
            new expense_categories { Id = 2, ParentType = "UtilityBill",    Name = "Electricity", IsUtility = true  },
            new expense_categories { Id = 3, ParentType = "UtilityBill",    Name = "Ammonia",     IsUtility = true  },
            new expense_categories { Id = 4, ParentType = "UtilityBill",    Name = "Salt",        IsUtility = true  },

            // General Expenses
            new expense_categories { Id = 5, ParentType = "GeneralExpense", Name = "Wages",       IsUtility = false },
            new expense_categories { Id = 6, ParentType = "GeneralExpense", Name = "Maintenance", IsUtility = false },
            new expense_categories { Id = 7, ParentType = "GeneralExpense", Name = "Petty Cash",  IsUtility = false },
            new expense_categories { Id = 8, ParentType = "GeneralExpense", Name = "Other",       IsUtility = false }
        );
    }
}
