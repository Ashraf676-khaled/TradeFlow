namespace TradeFlow.Application.Purchasing.Suppliers.Commands.ChangeSupplierPhone;

using FluentValidation;

public sealed class ChangeSupplierPhoneCommandValidator : AbstractValidator<ChangeSupplierPhoneCommand>
{
  public ChangeSupplierPhoneCommandValidator()
  {
    RuleFor(x => x.SupplierId).NotEmpty();
    RuleFor(x => x.NewPhone).NotEmpty();
  }
}
