using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MinimalTradeFlow.Web.Domain.CartAggregate;

namespace MinimalTradeFlow.Web.Infrastructure.Data.Config;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
  public void Configure(EntityTypeBuilder<Cart> builder)
  {
    builder.Property(entity => entity.Id)
      .HasValueGenerator<VogenGuidIdValueGenerator<AppDbContext, Cart, CartId>>()
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.CreatedOn)
      .IsRequired();

    builder.Property(entity => entity.Deleted)
      .IsRequired();

    // CartItems relationship
    builder.HasMany(c => c.Items);
  }
}
