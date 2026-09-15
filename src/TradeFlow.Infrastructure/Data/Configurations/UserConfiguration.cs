namespace TradeFlow.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("Users");

    builder.HasKey(u => u.Id);

    builder.Property(u => u.Id)
        .HasConversion(id => id.Value, value => new UserId(value))
        .ValueGeneratedNever();

    builder.Property(u => u.TenantId)
        .HasConversion(t => t.Value, value => new TenantId(value));

    builder.Property(u => u.FullName)
        .HasMaxLength(150)
        .IsRequired();

    builder.OwnsOne(u => u.Email, email =>
    {
      email.Property(e => e.Value)
          .HasColumnName("Email")
          .HasMaxLength(256)
          .IsRequired();

      email.HasIndex(e => e.Value).IsUnique();
    });

    builder.Property(u => u.PasswordHash).IsRequired();

    builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(50);

    builder.HasMany(u => u.RefreshTokens)
        .WithOne()
        .HasForeignKey(rt => rt.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(u => u.RefreshTokens).UsePropertyAccessMode(PropertyAccessMode.Field);
    builder.Metadata.FindNavigation(nameof(User.RefreshTokens))!
        .SetPropertyAccessMode(PropertyAccessMode.Field);
  }
}
