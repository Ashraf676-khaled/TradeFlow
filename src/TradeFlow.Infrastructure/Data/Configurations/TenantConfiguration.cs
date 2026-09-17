namespace TradeFlow.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Tenants;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
  public void Configure(EntityTypeBuilder<Tenant> builder)
  {
    builder.ToTable("Tenants");
    builder.HasKey(t => t.Id);
    builder.Property(t => t.Id)
        .HasConversion(id => id.Value, v => new TenantId(v))
        .ValueGeneratedNever();

    builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
  }
}
