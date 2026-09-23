namespace TradeFlow.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Settings;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
  public void Configure(EntityTypeBuilder<SystemSetting> builder)
  {
    builder.ToTable("SystemSettings");
    builder.HasKey(s => s.Id);
    builder.Property(s => s.Id)
        .HasConversion(id => id.Value, v => new SettingId(v))
        .ValueGeneratedNever();

    builder.Property(s => s.TenantId).HasConversion(t => t.Value, v => new TenantId(v));
    builder.Property(s => s.Key).HasMaxLength(SystemSetting.MaxKeyLength).IsRequired();
    builder.Property(s => s.Value).HasMaxLength(SystemSetting.MaxValueLength).IsRequired();

    // One row per key per tenant.
    builder.HasIndex(s => new { s.TenantId, s.Key }).IsUnique();
  }
}