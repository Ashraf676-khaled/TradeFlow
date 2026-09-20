namespace TradeFlow.Application.UnitTests.Purchasing.Suppliers.Commands;

using TradeFlow.Application.Purchasing.Suppliers.Commands.CreateSupplier;
using TradeFlow.Application.UnitTests.Helpers;
using TradeFlow.Domain.Purchasing;
using Xunit;

public class CreateSupplierCommandHandlerTests
{
  private readonly FakeCurrentUserService _currentUser = new() { TenantId = Guid.NewGuid() };

  [Fact]
  public async Task Handle_WithValidData_ShouldSucceed()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new CreateSupplierCommandHandler(context, _currentUser);

    var result = await handler.Handle(new CreateSupplierCommand("شركة الأمل", "01012345678"), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Single(context.Suppliers);
  }

  [Fact]
  public async Task Handle_WithDuplicatePhone_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new CreateSupplierCommandHandler(context, _currentUser);

    await handler.Handle(new CreateSupplierCommand("شركة الأمل", "01012345678"), CancellationToken.None);
    var result = await handler.Handle(new CreateSupplierCommand("شركة تانية", "01012345678"), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(SupplierErrors.PhoneAlreadyExists, result.TopError);
  }

  [Fact]
  public async Task Handle_WithDuplicateName_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new CreateSupplierCommandHandler(context, _currentUser);

    await handler.Handle(new CreateSupplierCommand("شركة الأمل", "01012345678"), CancellationToken.None);
    var result = await handler.Handle(new CreateSupplierCommand("شركة الأمل", "01098765432"), CancellationToken.None);

    Assert.True(result.IsError);
    Assert.Equal(SupplierErrors.NameAlreadyExists, result.TopError);
  }

  [Fact]
  public async Task Handle_WithInvalidPhone_ShouldFail()
  {
    await using var context = TestDbContextFactory.Create(_currentUser);
    var handler = new CreateSupplierCommandHandler(context, _currentUser);

    var result = await handler.Handle(new CreateSupplierCommand("شركة الأمل", "123"), CancellationToken.None);

    Assert.True(result.IsError);
  }

  [Fact]
  public async Task Handle_WithoutTenant_ShouldFail()
  {
    var noTenantUser = new FakeCurrentUserService { TenantId = null };
    await using var context = TestDbContextFactory.Create(noTenantUser);
    var handler = new CreateSupplierCommandHandler(context, noTenantUser);

    var result = await handler.Handle(new CreateSupplierCommand("شركة الأمل", "01012345678"), CancellationToken.None);

    Assert.True(result.IsError);
  }
}
