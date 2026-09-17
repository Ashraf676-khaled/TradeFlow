namespace TradeFlow.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using TradeFlow.Domain.Customers;
using TradeFlow.Domain.Inventory;
using TradeFlow.Domain.Purchasing;
using TradeFlow.Domain.Sales;
using TradeFlow.Domain.Tenants;
using TradeFlow.Domain.Users;

public interface IApplicationDbContext
{
  DbSet<User> Users { get; }
  DbSet<RefreshToken> RefreshTokens { get; }
  DbSet<Tenant> Tenants { get; }
  DbSet<Product> Products { get; }
  DbSet<Warehouse> Warehouses { get; }
  DbSet<StockItem> StockItems { get; }

  DbSet<Customer> Customers { get; }

  DbSet<SalesOrder> SalesOrders { get; }
  DbSet<Invoice> Invoices { get; }

  DbSet<Supplier> Suppliers { get; }
  DbSet<PurchaseOrder> PurchaseOrders { get; }

  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
