using IceFactoryManagmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceFactoryManagmentSystem.Infrastructure.Persistence.Configurations;

public class MonthlySummaryConfiguration : IEntityTypeConfiguration<MonthlySummary>
{
    public void Configure(EntityTypeBuilder<MonthlySummary> builder)
    {
        builder.ToTable("monthly_summaries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        // One summary per month
        builder.HasIndex(x => new { x.Year, x.Month })
               .IsUnique();

        builder.Property(x => x.Year).IsRequired();
        builder.Property(x => x.Month).IsRequired();

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_monthly_month_range", "[Month] >= 1 AND [Month] <= 12"));

        builder.Property(x => x.TotalIncome)
               .IsRequired()
               .HasColumnType("decimal(14,2)");

        builder.Property(x => x.TotalExpenses)
               .IsRequired()
               .HasColumnType("decimal(14,2)");

        builder.Property(x => x.NetProfit)
               .IsRequired()
               .HasColumnType("decimal(14,2)");

        builder.Property(x => x.IsClosed)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(x => x.ClosedAt)
               .IsRequired(false);

        // Navigation
        builder.HasMany(x => x.ProfitSplits)
               .WithOne(p => p.MonthlySummary)
               .HasForeignKey(p => p.MonthlySummaryId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProfitSplitConfiguration : IEntityTypeConfiguration<ProfitSplit>
{
    public void Configure(EntityTypeBuilder<ProfitSplit> builder)
    {
        builder.ToTable("profit_splits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.MonthlySummaryId)
               .IsRequired();

        builder.Property(x => x.PartnerName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.SplitPercentage)
               .IsRequired()
               .HasColumnType("decimal(5,2)");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_split_pct_range", "[SplitPercentage] > 0 AND [SplitPercentage] <= 100"));

        builder.Property(x => x.AmountReceived)
               .IsRequired()
               .HasColumnType("decimal(14,2)");
    }
}
