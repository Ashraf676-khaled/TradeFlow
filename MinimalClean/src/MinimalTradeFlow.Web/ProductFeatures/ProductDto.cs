using MinimalTradeFlow.Web.Domain.ProductAggregate;

namespace MinimalTradeFlow.Web.ProductFeatures;
public record ProductDto(ProductId Id, string Name, decimal UnitPrice);
