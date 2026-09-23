# TradeFlow

> A Domain-Driven, Multi-Tenant ERP System for Trading Companies — built with Clean Architecture & real DDD, not just folder names.

Built on top of [Ardalis' Clean Architecture Template](https://github.com/ashraf676-khaled/CleanArchitecture) (forked from [ardalis/CleanArchitecture](https://github.com/ardalis/CleanArchitecture)), extended with a **Rich Domain Model** approach that actually protects business rules instead of exposing them as public setters.

## 🏗️ Architecture

TradeFlow follows Clean Architecture (Onion/Hexagonal) with strict dependency direction — **Domain has zero dependencies**, everything else depends inward on it.

TradeFlow.Api → Controllers, Endpoints, DI Composition Root
TradeFlow.Application → Use Cases, Command/Query Handlers, DTOs
TradeFlow.Domain → Aggregates, Entities, Value Objects, Business Rules
TradeFlow.Infrastructure → EF Core, Repositories, External Services


Plus dedicated test projects per layer:

TradeFlow.Domain.UnitTests
TradeFlow.Application.UnitTests
TradeFlow.Api.IntegrationTests


## 🎯 What Makes This Different From a CRUD App

### Rich Domain Model — Not Anemic Entities
No `public { get; set; }` anywhere in the Domain. Every state change goes through a method that enforces its own invariants:

```csharp
order.Confirm();        // ✅ Domain protects the rule
order.Status = Confirmed; // ❌ never happens in this codebase
```

### Result Pattern Instead of Exceptions
No `throw new DomainException(...)` for expected business failures. Every operation that can fail returns a `Result<T>`, keeping the Domain fast and explicit:

```csharp
public static Result<Money> EGP(decimal amount)
{
    if (amount < 0)
        return MoneyErrors.NegativeAmount;

    return new Money(amount, "EGP");
}
```

Errors are typed and categorized (`Validation`, `Conflict`, `NotFound`, `Unauthorized`, `Forbidden`, `Failure`, `Unexpected`) via a custom `Error` / `ErrorKind` / `Result<T>` implementation — no external library dependency.

### True Aggregate Boundaries
Aggregates only reference each other by ID, never by direct object reference:

```csharp
public UserId UserId { get; private set; }  // not a User navigation property
```

This keeps each Aggregate independently loadable, testable, and transactionally isolated — critical for scaling to more Bounded Contexts later.

### Strongly-Typed IDs
`ProductId`, `WarehouseId`, `CustomerId`, `SalesOrderId`... implemented as `readonly record struct`, so passing the wrong ID to the wrong method is a compile-time error, not a runtime bug.

### Domain Events for Cross-Aggregate Consistency
Instead of one Aggregate reaching into another's data directly (e.g. `SalesOrder` touching `StockItem` tables), state changes raise Domain Events that are handled after the triggering transaction commits — giving true eventual consistency between Bounded Contexts:

```csharp
Status = OrderStatus.Confirmed;
RaiseDomainEvent(new SalesOrderConfirmedDomainEvent(Id.Value, WarehouseId.Value));
```

### Multi-Tenant by Design
Every entity carries a `TenantId` for full data isolation between companies using the platform, resolved from the JWT context — never trusted from the request body.

## 📦 Bounded Contexts

| Context | Responsibility |
|---|---|
| **Sales** | Orders, Order Items, Invoices, Payments |
| **Inventory** | Products, Warehouses, Stock (Available/Reserved) |
| **Customers** | Customer profiles, credit limits, balances |
| **Purchasing** | Purchase Orders, Suppliers, receiving stock |
| **Users** | Authentication, Roles, Refresh Tokens |

## 🧪 Testing Philosophy

Every Value Object and Aggregate is developed test-first at the unit level — invariants are verified before the next layer is built on top of them, bottom-up: `Value Objects → Entities → Aggregates → Domain Events`.

✔ Add_TwoMoneyValues_ShouldReturnSum
✔ EGP_WithNegativeAmount_ShouldFailWithValidationError
✔ ApplyTo_HundredPercentDiscount_ShouldReturnZero


## 🛠️ Tech Stack

- **.NET 10** / ASP.NET Core
- **EF Core** (Owned Types for Value Objects, private-field mapping for Rich Domain)
- **xUnit** for testing
- **MediatR-style Domain Event dispatching** via SaveChanges interceptor
- **JWT** Authentication + Refresh Token rotation

## 🚧 Roadmap

- [ ] Complete Aggregate implementations (SalesOrder, Product, StockItem, Customer, PurchaseOrder)
- [ ] EF Core Configurations + Migrations
- [ ] Domain Event handlers (Stock Reservation flow)
- [ ] Outbox Pattern for reliable event publishing
- [ ] API Endpoints (Auth, Sales Orders, Inventory Operations, Dashboard)
- [ ] Reporting module
- [ ] AI-assisted querying (`POST /ai/chat`)

---

*A learning-driven project focused on applying real Domain-Driven Design principles — not just the folder structure that looks like it.*