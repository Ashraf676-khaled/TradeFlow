namespace TradeFlow.Application.UnitTests.Purchasing.Suppliers.Commands;

using TradeFlow.Application.Purchasing.Suppliers.Commands.CreateSupplier;
using TradeFlow.Application.Purchasing.Suppliers.Commands.DeactivateSupplier;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Purchasing;
using Xunit;

public class DeactivateSupplierCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_ForActiveSupplier_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var supplierId = (await new CreateSupplierCommandHandler(context, _currentUser)
        .Handle(new CreateSupplierCommand("شركة الأمل", "01012345678"), CancellationToken.None)).Value;

    var handler = new DeactivateSupplierCommandHandler(context);
    var result = await handler.Handle(new DeactivateSupplierCommand(supplierId), CancellationToken.None);

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public async Task Handle_ForAlreadyInactiveSupplier_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);

    var supplierId = (await new CreateSupplierCommandHandler(context, _currentUser)
        .Handle(new CreateSupplierCommand("شركة الأمل", "01012345678"), CancellationToken.None)).Value;

    var handler = new DeactivateSupplierCommandHandler(context);
    await handler.Handle(new DeactivateSupplierCommand(supplierId), CancellationToken.None);
    var result = await handler.Handle(new DeactivateSupplierCommand(supplierId), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(SupplierErrors.AlreadyInactive, result.TopError);
  }

  [Fact]
  public async Task Handle_ForNonExistingSupplier_ShouldReturnNotFound()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new DeactivateSupplierCommandHandler(context);

    var result = await handler.Handle(new DeactivateSupplierCommand(Guid.NewGuid()), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(SupplierErrors.NotFound, result.TopError);
  }
}
