namespace TradeFlow.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Customers;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
  public void Configure(EntityTypeBuilder<Customer> builder)
  {
    builder.ToTable("Customers");
    builder.HasKey(c => c.Id);
    builder.Property(c => c.Id)
        .HasConversion(id => id.Value, v => new CustomerId(v))
        .ValueGeneratedNever();

    builder.Property(c => c.TenantId).HasConversion(t => t.Value, v => new TenantId(v));
    builder.Property(c => c.Name).HasMaxLength(200).IsRequired();

    builder.OwnsOne(c => c.Phone, p =>
        p.Property(x => x.Value).HasColumnName("Phone").HasMaxLength(20));

    // Email اختيارية (Email?) — لازم IsRequired(false) على الـOwned Navigation نفسها
    builder.OwnsOne(c => c.Email, e =>
    {
      e.Property(x => x.Value).HasColumnName("Email").HasMaxLength(254);
    });
    builder.Navigation(c => c.Email).IsRequired(false);

    builder.OwnsOne(c => c.CreditLimit, m =>
    {
      m.Property(x => x.Amount).HasColumnName("CreditLimit").HasColumnType("decimal(18,2)");
      m.Property(x => x.Currency).HasColumnName("CreditLimitCurrency").HasMaxLength(3);
    });

    builder.OwnsOne(c => c.CurrentBalance, m =>
    {
      m.Property(x => x.Amount).HasColumnName("CurrentBalance").HasColumnType("decimal(18,2)");
      m.Property(x => x.Currency).HasColumnName("CurrentBalanceCurrency").HasMaxLength(3);
    });
  }
}
