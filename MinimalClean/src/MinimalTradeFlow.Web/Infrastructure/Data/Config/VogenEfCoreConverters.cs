using MinimalTradeFlow.Web.Domain.CartAggregate;
using MinimalTradeFlow.Web.Domain.GuestUserAggregate;
using MinimalTradeFlow.Web.Domain.OrderAggregate;
using MinimalTradeFlow.Web.Domain.ProductAggregate;
using Vogen;

namespace MinimalTradeFlow.Web.Infrastructure.Data.Config;

[EfCoreConverter<ProductId>]
[EfCoreConverter<CartId>]
[EfCoreConverter<CartItemId>]
[EfCoreConverter<GuestUserId>]
[EfCoreConverter<OrderId>]
[EfCoreConverter<OrderItemId>]
[EfCoreConverter<Quantity>]
[EfCoreConverter<Price>]
internal partial class VogenEfCoreConverters;
