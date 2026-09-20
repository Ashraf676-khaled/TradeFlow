namespace TradeFlow.Application.Purchasing.Suppliers.Commands.CreateSupplier;

using FluentValidation;

public sealed class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
  public CreateSupplierCommandValidator()
  {
    RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    RuleFor(x => x.Phone).NotEmpty();
  }
}
