namespace TradeFlow.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Purchasing;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
  public void Configure(EntityTypeBuilder<Supplier> builder)
  {
    builder.ToTable("Suppliers");
    builder.HasKey(s => s.Id);
    builder.Property(s => s.Id)
        .HasConversion(id => id.Value, v => new SupplierId(v))
        .ValueGeneratedNever();

    builder.Property(s => s.TenantId).HasConversion(t => t.Value, v => new TenantId(v));
    builder.Property(s => s.Name).HasMaxLength(200).IsRequired();

    builder.OwnsOne(s => s.Phone, p => p.Property(x => x.Value).HasColumnName("Phone").HasMaxLength(20));
  }
}

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
  public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
  {
    builder.ToTable("PurchaseOrders");
    builder.HasKey(p => p.Id);
    builder.Property(p => p.Id)
        .HasConversion(id => id.Value, v => new PurchaseOrderId(v))
        .ValueGeneratedNever();

    builder.Property(p => p.TenantId).HasConversion(t => t.Value, v => new TenantId(v));
    builder.Property(p => p.SupplierId).HasConversion(s => s.Value, v => new SupplierId(v));
    builder.Property(p => p.WarehouseId).HasConversion(w => w.Value, v => new WarehouseId(v));
    builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(50);

    builder.OwnsOne(p => p.OrderNumber, n =>
    {
      n.Property(x => x.Value).HasColumnName("OrderNumber").HasMaxLength(20);
      n.HasIndex(x => x.Value).IsUnique();
    });

    builder.HasMany(p => p.Items)
        .WithOne()
        .HasForeignKey("PurchaseOrderId")
        .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(p => p.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
  public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
  {
    builder.ToTable("PurchaseOrderItems");
    builder.HasKey(i => i.Id);
    builder.Property(i => i.ProductId).HasConversion(p => p.Value, v => new ProductId(v));

    builder.OwnsOne(i => i.OrderedQuantity, q => q.Property(x => x.Value).HasColumnName("OrderedQuantity"));
    builder.OwnsOne(i => i.ReceivedQuantity, q => q.Property(x => x.Value).HasColumnName("ReceivedQuantity"));
    builder.OwnsOne(i => i.UnitCost, m =>
    {
      m.Property(x => x.Amount).HasColumnName("UnitCost").HasColumnType("decimal(18,2)");
      m.Property(x => x.Currency).HasColumnName("UnitCostCurrency").HasMaxLength(3);
    });
  }
}
