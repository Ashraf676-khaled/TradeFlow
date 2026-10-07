namespace TradeFlow.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Expenses;

public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
  public void Configure(EntityTypeBuilder<Expense> builder)
  {
    builder.ToTable("Expenses");
    builder.HasKey(expense => expense.Id);
    builder.Property(expense => expense.Id)
        .HasConversion(id => id.Value, value => new ExpenseId(value))
        .ValueGeneratedNever();

    builder.Property(expense => expense.TenantId)
        .HasConversion(tenantId => tenantId.Value, value => new TenantId(value))
        .IsRequired();
    builder.Property(expense => expense.Description).HasMaxLength(200).IsRequired();
    builder.Property(expense => expense.Category).HasMaxLength(60).IsRequired();
    builder.Property(expense => expense.Classification).HasMaxLength(20).IsRequired();
    builder.Property(expense => expense.IncurredAt)
        .HasConversion(
            incurredAt => incurredAt.UtcDateTime,
            storedAt => new DateTimeOffset(DateTime.SpecifyKind(storedAt, DateTimeKind.Utc)))
        .IsRequired();
    builder.OwnsOne(expense => expense.Amount, money =>
    {
      money.Property(value => value.Amount).HasColumnName("Amount").HasColumnType("decimal(18,2)");
      money.Property(value => value.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
    });

    builder.HasIndex(expense => new { expense.TenantId, expense.IncurredAt });
  }
}
