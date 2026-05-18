using IcePlant.Domain.Aggregates.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IcePlant.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(a => a.EntityId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(a => a.Action)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(a => a.OldValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.NewValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.ChangedColumns)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.UserId)
            .HasMaxLength(256)
            .HasDefaultValue("System");

        builder.Property(a => a.Timestamp)
            .IsRequired();

        // Indexes for common queries
        builder.HasIndex(a => a.EntityName);
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
    }
}
