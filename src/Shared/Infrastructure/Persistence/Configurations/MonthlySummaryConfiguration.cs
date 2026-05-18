using IcePlant.Domain.Aggregates.Monthly;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcePlant.Infrastructure.Persistence.Configurations;

public class MonthlySummaryConfiguration : IEntityTypeConfiguration<MonthlySummary>
{
    public void Configure(EntityTypeBuilder<MonthlySummary> builder)
    {
        builder.ToTable("MonthlySummaries");

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

        // Money value objects — stored as flat decimal columns
        builder.OwnsOne(x => x.TotalIncome, m =>
        {
            m.Property(p => p.Amount)
             .HasColumnName("TotalIncome")
             .HasColumnType("decimal(14,2)")
             .IsRequired();
            m.Ignore(p => p.Currency); // Currency is always EGP, not stored
        });

        builder.OwnsOne(x => x.TotalExpenses, m =>
        {
            m.Property(p => p.Amount)
             .HasColumnName("TotalExpenses")
             .HasColumnType("decimal(14,2)")
             .IsRequired();
            m.Ignore(p => p.Currency);
        });

        builder.OwnsOne(x => x.NetProfit, m =>
        {
            m.Property(p => p.Amount)
             .HasColumnName("NetProfit")
             .HasColumnType("decimal(14,2)")
             .IsRequired();
            m.Ignore(p => p.Currency);
        });

        builder.Property(x => x.IsClosed)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(x => x.ClosedAt)
               .IsRequired(false);

        // Navigation: MonthlySummary owns a collection of ProfitSplits
        builder.HasMany(x => x.ProfitSplits)
               .WithOne()
               .HasForeignKey(p => p.MonthlySummaryId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProfitSplitConfiguration : IEntityTypeConfiguration<ProfitSplit>
{
    public void Configure(EntityTypeBuilder<ProfitSplit> builder)
    {
        builder.ToTable("ProfitSplits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.MonthlySummaryId)
               .IsRequired();

        builder.Property(x => x.PartnerName)
               .IsRequired()
               .HasMaxLength(100);

        // SplitPercentage value object
        builder.OwnsOne(x => x.SplitPercentage, sp =>
        {
            sp.Property(p => p.Value)
              .HasColumnName("SplitPercentage")
              .HasColumnType("decimal(5,2)")
              .IsRequired();
        });

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_split_pct_range", "[SplitPercentage] > 0 AND [SplitPercentage] <= 100"));

        // AmountReceived value object
        builder.OwnsOne(x => x.AmountReceived, m =>
        {
            m.Property(p => p.Amount)
             .HasColumnName("AmountReceived")
             .HasColumnType("decimal(14,2)")
             .IsRequired();
            m.Ignore(p => p.Currency);
        });
    }
}
