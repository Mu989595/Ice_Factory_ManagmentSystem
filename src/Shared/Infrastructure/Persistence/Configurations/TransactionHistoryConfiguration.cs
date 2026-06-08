using IcePlant.Domain.Aggregates.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcePlant.Infrastructure.Persistence.Configurations;

public sealed class TransactionHistoryConfiguration
    : IEntityTypeConfiguration<TransactionHistory>
{
    public void Configure(EntityTypeBuilder<TransactionHistory> builder)
    {
        builder.ToTable("TransactionHistories");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
               .ValueGeneratedOnAdd();

        builder.Property(t => t.LedgerDayId)
               .IsRequired();

        builder.Property(t => t.OccurredAt)
               .IsRequired();

        builder.Property(t => t.Amount)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(t => t.Type)
               .IsRequired();

        builder.Property(t => t.Notes)
               .HasMaxLength(500);

        // Fast lookup by ledger day
        builder.HasIndex(t => t.LedgerDayId);
    }
}
