namespace TradeFlow.Domain.Inventory;

using TradeFlow.Domain.Common.Results;

public static class ProductErrors
{
  public static readonly Error NameRequired = Error.Validation(
      "Product.NameRequired", "Product name is required.");

  public static readonly Error NameTooLong = Error.Validation(
      "Product.NameTooLong", "Product name cannot exceed 200 characters.");

  public static readonly Error InvalidSellingPrice = Error.Validation(
      "Product.InvalidSellingPrice", "Selling price must be greater than zero.");

  public static readonly Error InvalidCost = Error.Validation(
      "Product.InvalidCost", "Cost must be greater than zero.");

  public static readonly Error InvalidMinimumStock = Error.Validation(
      "Product.InvalidMinimumStock", "Minimum stock cannot be negative.");

  public static readonly Error AlreadyInactive = Error.Conflict(
      "Product.AlreadyInactive", "Product is already inactive.");

  public static readonly Error AlreadyActive = Error.Conflict(
      "Product.AlreadyActive", "Product is already active.");

  public static readonly Error NotSellable = Error.Validation(
      "Product.NotSellable", "Cannot sell an inactive product.");

  public static readonly Error HasActiveOrders = Error.Conflict(
      "Product.HasActiveOrders", "Cannot deactivate a product with active sales orders.");
  public static readonly Error SkuAlreadyExists = Error.Conflict(
      "Product.SkuAlreadyExists", "A product with the same SKU already exists.");
  public static readonly Error CostExceedsSellingPrice = Error.Validation(
    "Product.CostExceedsSellingPrice", "The cost cannot be greater than or equal to the selling price.");

  public static readonly Error NotFound = Error.NotFound(

      "Product.NotFound", "Product not found.");
}
