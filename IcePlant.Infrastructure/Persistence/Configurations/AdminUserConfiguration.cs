using IceFactoryManagmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceFactoryManagmentSystem.Infrastructure.Persistence.Configurations;

public class AdminUserConfiguration : IEntityTypeConfiguration<admin_users>
{
    public void Configure(EntityTypeBuilder<admin_users> builder)
    {
        builder.ToTable("admin_users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.username)
               .IsRequired()
               .HasMaxLength(50);

        builder.HasIndex(x => x.username)
               .IsUnique();

        builder.Property(x => x.password_hash)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(x => x.created_at)
               .IsRequired()
               .HasDefaultValueSql("GETUTCDATE()");

        // Seed the single admin user (password should be changed on first login)
        builder.HasData(new admin_users
        {
            Id            = 1,
            username      = "admin",
            // BCrypt hash of "Admin@1234" — change immediately after first login
            password_hash = "$2a$11$placeholder_change_this_on_first_login",
            created_at    = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
