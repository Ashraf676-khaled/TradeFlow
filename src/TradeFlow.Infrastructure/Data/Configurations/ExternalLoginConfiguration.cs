namespace TradeFlow.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlow.Domain.Users;

public sealed class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
  public void Configure(EntityTypeBuilder<ExternalLogin> builder)
  {
    builder.ToTable("ExternalLogins");
    builder.HasKey(login => login.Id);
    builder.Property(login => login.Id).ValueGeneratedNever();
    builder.Property(login => login.UserId)
        .HasConversion(userId => userId.Value, value => new TradeFlow.Domain.Common.Identifiers.UserId(value));
    builder.Property(login => login.Provider).HasMaxLength(50).IsRequired();
    builder.Property(login => login.ProviderSubject).HasMaxLength(256).IsRequired();
    builder.HasIndex(login => new { login.Provider, login.ProviderSubject }).IsUnique();
    builder.HasOne<User>()
        .WithMany()
        .HasForeignKey(login => login.UserId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}
