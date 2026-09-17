namespace TradeFlow.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Users;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
  public void Configure(EntityTypeBuilder<RefreshToken> builder)
  {
    builder.ToTable("RefreshTokens");

    builder.HasKey(rt => rt.Id);

    builder.Property(rt => rt.Id)
           .ValueGeneratedNever();

    builder.Property(rt => rt.UserId)
        .HasConversion(id => id.Value, value => new UserId(value));

    builder.Property(rt => rt.Token)
        .HasMaxLength(500)
        .IsRequired();

    builder.HasIndex(rt => rt.Token).IsUnique();
  }
}
