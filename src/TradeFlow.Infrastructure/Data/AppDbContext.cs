namespace TradeFlow.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Events;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Customers;
using TradeFlow.Domain.Inventory;
using TradeFlow.Domain.Purchasing;
using TradeFlow.Domain.Sales;
using TradeFlow.Domain.Tenants;
using TradeFlow.Domain.Users;

public class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService)
    : DbContext(options), IApplicationDbContext
{
  // النوع بقى TenantId نفسها (الـstruct) مش Guid خام
  private TenantId CurrentTenantId => new(currentUserService.TenantId ?? Guid.Empty);

  public DbSet<User> Users => Set<User>();
  public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
  public DbSet<Tenant> Tenants => Set<Tenant>();
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

    // مقارنة مباشرة e.TenantId == CurrentTenantId — النوعين TenantId مع بعض،
    // EF بيطبق الـHasConversion بتاعتها تلقائيًا ويترجمها لـSQL عادي
    modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
    modelBuilder.Entity<Product>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
    modelBuilder.Entity<Warehouse>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
    modelBuilder.Entity<StockItem>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
    modelBuilder.Entity<Customer>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
    modelBuilder.Entity<SalesOrder>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
    modelBuilder.Entity<Invoice>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
    modelBuilder.Entity<Supplier>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
    modelBuilder.Entity<PurchaseOrder>().HasQueryFilter(e => e.TenantId == CurrentTenantId);

    base.OnModelCreating(modelBuilder);
  }
}
