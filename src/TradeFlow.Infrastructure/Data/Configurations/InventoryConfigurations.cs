namespace TradeFlow.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Inventory;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
  public void Configure(EntityTypeBuilder<Product> builder)
  {
    builder.ToTable("Products");
    builder.HasKey(p => p.Id);
    builder.Property(p => p.Id)
        .HasConversion(id => id.Value, v => new ProductId(v))
        .ValueGeneratedNever();

    builder.Property(p => p.TenantId).HasConversion(t => t.Value, v => new TenantId(v));
    builder.Property(p => p.Name).HasMaxLength(200).IsRequired();

    builder.OwnsOne(p => p.Sku, sku =>
    {
      sku.Property(s => s.Value).HasColumnName("Sku").HasMaxLength(50).IsRequired();
      sku.HasIndex(s => s.Value).IsUnique();
    });

    builder.OwnsOne(p => p.SellingPrice, m =>
    {
      m.Property(x => x.Amount).HasColumnName("SellingPrice").HasColumnType("decimal(18,2)");
      m.Property(x => x.Currency).HasColumnName("SellingPriceCurrency").HasMaxLength(3);
    });

    builder.OwnsOne(p => p.Cost, m =>
    {
      m.Property(x => x.Amount).HasColumnName("Cost").HasColumnType("decimal(18,2)");
      m.Property(x => x.Currency).HasColumnName("CostCurrency").HasMaxLength(3);
    });
  }
}

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
  public void Configure(EntityTypeBuilder<Warehouse> builder)
  {
    builder.ToTable("Warehouses");
    builder.HasKey(w => w.Id);
    builder.Property(w => w.Id)
        .HasConversion(id => id.Value, v => new WarehouseId(v))
        .ValueGeneratedNever();

    builder.Property(w => w.TenantId).HasConversion(t => t.Value, v => new TenantId(v));
    builder.Property(w => w.Name).HasMaxLength(150).IsRequired();
    builder.Property(w => w.Location).HasMaxLength(300).IsRequired();
  }
}

public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
  public void Configure(EntityTypeBuilder<StockItem> builder)
  {
    builder.ToTable("StockItems");
    builder.HasKey(s => s.Id);
    builder.Property(s => s.Id)
        .HasConversion(id => id.Value, v => new StockItemId(v))
        .ValueGeneratedNever();

    builder.Property(s => s.TenantId).HasConversion(t => t.Value, v => new TenantId(v));
    builder.Property(s => s.WarehouseId).HasConversion(w => w.Value, v => new WarehouseId(v));
    builder.Property(s => s.ProductId).HasConversion(p => p.Value, v => new ProductId(v));
    builder.Property(p => p.Id).ValueGeneratedNever();

    builder.OwnsOne(s => s.AvailableQuantity, q => q.Property(x => x.Value).HasColumnName("AvailableQuantity"));
    builder.OwnsOne(s => s.ReservedQuantity, q => q.Property(x => x.Value).HasColumnName("ReservedQuantity"));
    builder.Property(s => s.RowVersion).IsRowVersion().IsConcurrencyToken();

    builder.HasIndex(s => new { s.WarehouseId, s.ProductId }).IsUnique();
  }
}
