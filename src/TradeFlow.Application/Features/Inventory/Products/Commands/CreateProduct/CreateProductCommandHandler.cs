namespace TradeFlow.Application.Inventory.Products.Commands.CreateProduct;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeFlow.Application.Common.Interfaces;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.Results;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Inventory;

public sealed class CreateProductCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateProductCommand, Result<Guid>>
{
  public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken ct)
  {
    if (currentUser.TenantId is null)
      return Error.Unauthorized("Auth.NoTenant","Unable to determine the current tenant.");

    var skuResult = Sku.Create(request.Sku);
    if (skuResult.IsError)
      return skuResult.Errors;

    // فحص التكرار قبل الحفظ — عشان نرجّع 409 واضح بدل ما الداتابيز ترفض الـInsert
    var skuExists = await context.Products
        .AnyAsync(p => p.Sku.Value == skuResult.Value.Value, ct);

    if (skuExists)
      return ProductErrors.SkuAlreadyExists;

    var sellingPriceResult = Money.EGP(request.SellingPrice);
    if (sellingPriceResult.IsError)
      return sellingPriceResult.Errors;

    var costResult = Money.EGP(request.Cost);
    if (costResult.IsError)
      return costResult.Errors;

    var productResult = Product.Create(
        new TenantId(currentUser.TenantId.Value),
        request.Name,
        skuResult.Value,
        sellingPriceResult.Value,
        costResult.Value,
        request.MinimumStock);

    if (productResult.IsError)
      return productResult.Errors;

    context.Products.Add(productResult.Value);
    await context.SaveChangesAsync(ct);

    return productResult.Value.Id.Value;
  }
}
