namespace TradeFlow.Application.Purchasing.Suppliers.Commands.RenameSupplier;

using FluentValidation;

public sealed class RenameSupplierCommandValidator : AbstractValidator<RenameSupplierCommand>
{
  public RenameSupplierCommandValidator()
  {
    RuleFor(x => x.SupplierId).NotEmpty();
    RuleFor(x => x.NewName).NotEmpty().MaximumLength(200);
  }
}
