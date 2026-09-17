namespace TradeFlow.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Sales;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
  public void Configure(EntityTypeBuilder<SalesOrder> builder)
  {
    builder.ToTable("SalesOrders");
    builder.HasKey(s => s.Id);
    builder.Property(s => s.Id)
        .HasConversion(id => id.Value, v => new SalesOrderId(v))
        .ValueGeneratedNever();

    builder.Property(s => s.TenantId).HasConversion(t => t.Value, v => new TenantId(v));
    builder.Property(s => s.CustomerId).HasConversion(c => c.Value, v => new CustomerId(v));
    builder.Property(s => s.SalesRepresentativeId).HasConversion(u => u.Value, v => new UserId(v));
    builder.Property(s => s.WarehouseId).HasConversion(w => w.Value, v => new WarehouseId(v));
    builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(50);

    builder.OwnsOne(s => s.OrderDiscount, d =>
        d.Property(x => x.Percentage).HasColumnName("OrderDiscountPercentage").HasColumnType("decimal(5,2)"));

    builder.HasMany(s => s.Items)
        .WithOne()
        .HasForeignKey("SalesOrderId")
        .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(s => s.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}

public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
  public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
  {
    builder.ToTable("SalesOrderItems");
    builder.HasKey(i => i.Id);
    builder.Property(i => i.ProductId).HasConversion(p => p.Value, v => new ProductId(v));

    builder.OwnsOne(i => i.Quantity, q => q.Property(x => x.Value).HasColumnName("Quantity"));
    builder.OwnsOne(i => i.UnitPrice, m =>
    {
      m.Property(x => x.Amount).HasColumnName("UnitPrice").HasColumnType("decimal(18,2)");
      m.Property(x => x.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
    });
    builder.OwnsOne(i => i.ItemDiscount, d =>
        d.Property(x => x.Percentage).HasColumnName("ItemDiscountPercentage").HasColumnType("decimal(5,2)"));
  }
}

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
  public void Configure(EntityTypeBuilder<Invoice> builder)
  {
    builder.ToTable("Invoices");
    builder.HasKey(i => i.Id);
    builder.Property(i => i.Id)
        .HasConversion(id => id.Value, v => new InvoiceId(v))
        .ValueGeneratedNever();

    builder.Property(i => i.TenantId).HasConversion(t => t.Value, v => new TenantId(v));
    builder.Property(i => i.SalesOrderId).HasConversion(s => s.Value, v => new SalesOrderId(v));
    builder.Property(i => i.CustomerId).HasConversion(c => c.Value, v => new CustomerId(v));
    builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(50);

    builder.OwnsOne(i => i.InvoiceNumber, n =>
    {
      n.Property(x => x.Value).HasColumnName("InvoiceNumber").HasMaxLength(20);
      n.HasIndex(x => x.Value).IsUnique();
    });

    builder.OwnsOne(i => i.TotalAmount, m =>
    {
      m.Property(x => x.Amount).HasColumnName("TotalAmount").HasColumnType("decimal(18,2)");
      m.Property(x => x.Currency).HasColumnName("TotalAmountCurrency").HasMaxLength(3);
    });

    builder.HasMany(i => i.Payments)
        .WithOne()
        .HasForeignKey("InvoiceId")
        .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(i => i.Payments).UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
  public void Configure(EntityTypeBuilder<Payment> builder)
  {
    builder.ToTable("Payments");
    builder.HasKey(p => p.Id);

    builder.Property(i => i.Id).ValueGeneratedNever();
    builder.OwnsOne(p => p.Amount, m =>
    {
      m.Property(x => x.Amount).HasColumnName("Amount").HasColumnType("decimal(18,2)");
      m.Property(x => x.Currency).HasColumnName("Currency").HasMaxLength(3);
    });
  }
}
