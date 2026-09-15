namespace TradeFlow.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Appliction.Common.Interfaces;
using TradeFlow.Domain.Common.Events;
using TradeFlow.Domain.Customers;
using TradeFlow.Domain.Inventory;
using TradeFlow.Domain.Purchasing;
using TradeFlow.Domain.Sales;
using TradeFlow.Domain.Users;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IApplicationDbContext
{
  public DbSet<User> Users => Set<User>();
  public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

  public DbSet<Product> Products => Set<Product>();
  public DbSet<Warehouse> Warehouses => Set<Warehouse>();
  public DbSet<StockItem> StockItems => Set<StockItem>();

  public DbSet<Customer> Customers => Set<Customer>();

  public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
  public DbSet<Invoice> Invoices => Set<Invoice>();

  public DbSet<Supplier> Suppliers => Set<Supplier>();
  public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    modelBuilder.Ignore<DomainEvent>();
    base.OnModelCreating(modelBuilder);
  }
}
