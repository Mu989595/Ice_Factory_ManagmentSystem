using IcePlant.Domain.Aggregates.Basin;
using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Aggregates.HR;
using IcePlant.Domain.Aggregates.Monthly;
using IcePlant.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcePlant.Infrastructure.Persistence.Configurations;

public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.ToTable("ExpenseCategory");

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

        // â”€â”€ Seed Data â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        builder.HasData(
            // Utility Bills â€” tracked individually per requirement
            new ExpenseCategory { Id = 1, ParentType = "UtilityBill",    Name = "Water",       IsUtility = true  },
            new ExpenseCategory { Id = 2, ParentType = "UtilityBill",    Name = "Electricity", IsUtility = true  },
            new ExpenseCategory { Id = 3, ParentType = "UtilityBill",    Name = "Ammonia",     IsUtility = true  },
            new ExpenseCategory { Id = 4, ParentType = "UtilityBill",    Name = "Salt",        IsUtility = true  },

            // General Expenses
            new ExpenseCategory { Id = 5, ParentType = "GeneralExpense", Name = "Wages",       IsUtility = false },
            new ExpenseCategory { Id = 6, ParentType = "GeneralExpense", Name = "Maintenance", IsUtility = false },
            new ExpenseCategory { Id = 7, ParentType = "GeneralExpense", Name = "Petty Cash",  IsUtility = false },
            new ExpenseCategory { Id = 8, ParentType = "GeneralExpense", Name = "Other",       IsUtility = false }
        );
    }
}

