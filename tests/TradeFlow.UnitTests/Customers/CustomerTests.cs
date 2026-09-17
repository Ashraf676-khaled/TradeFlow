namespace TradeFlow.Domain.Tests.Customers;

using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Customers;
using Xunit;

public class CustomerTests
{
  private static readonly TenantId TestTenant = TenantId.New();

  private static Customer CreateValidCustomer(decimal creditLimit = 5000)
  {
    var phone = PhoneNumber.Create("01012345678").Value;
    var limit = Money.EGP(creditLimit).Value;

    return Customer.Create(TestTenant, "Ahmed Trading Co.", phone, limit).Value;
  }

  [Fact]
  public void Create_WithValidData_ShouldSucceed()
  {
    var phone = PhoneNumber.Create("01012345678").Value;
    var limit = Money.EGP(5000).Value;

    var result = Customer.Create(TestTenant, "Ahmed Trading Co.", phone, limit);

    Assert.True(result.IsSuccess);
    Assert.True(result.Value.IsActive);
    Assert.Equal(0, result.Value.CurrentBalance.Amount);
    Assert.Null(result.Value.Email);
  }

  [Fact]
  public void Create_WithEmptyName_ShouldFail()
  {
    var phone = PhoneNumber.Create("01012345678").Value;
    var limit = Money.EGP(5000).Value;

    var result = Customer.Create(TestTenant, "", phone, limit);

    Assert.True(result.IsError);
    Assert.Equal(CustomerErrors.NameRequired, result.TopError);
  }

  [Fact]
  public void Create_WithOptionalEmail_ShouldSucceed()
  {
    var phone = PhoneNumber.Create("01012345678").Value;
    var email = Email.Create("ahmed@example.com").Value;
    var limit = Money.EGP(5000).Value;

    var result = Customer.Create(TestTenant, "Ahmed Trading Co.", phone, limit, email);

    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value.Email);
  }

  [Fact]
  public void ValidateCredit_WithinLimit_ShouldSucceed()
  {
    var customer = CreateValidCustomer(creditLimit: 5000);
    var orderAmount = Money.EGP(3000).Value;

    var result = customer.ValidateCredit(orderAmount);

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void ValidateCredit_ExceedingLimit_ShouldFail()
  {
    var customer = CreateValidCustomer(creditLimit: 5000);
    var orderAmount = Money.EGP(6000).Value;

    var result = customer.ValidateCredit(orderAmount);

    Assert.True(result.IsError);
    Assert.Equal(CustomerErrors.CreditLimitExceeded, result.TopError);
  }

  [Fact]
  public void ValidateCredit_ExactlyAtLimit_ShouldSucceed()
  {
    var customer = CreateValidCustomer(creditLimit: 5000);
    var orderAmount = Money.EGP(5000).Value;

    var result = customer.ValidateCredit(orderAmount);

    Assert.True(result.IsSuccess);
  }

  [Fact]
  public void ValidateCredit_ConsidersExistingBalance()
  {
    var customer = CreateValidCustomer(creditLimit: 5000);
    customer.IncreaseBalance(Money.EGP(4000).Value);

    var result = customer.ValidateCredit(Money.EGP(2000).Value);

    Assert.True(result.IsError);
    Assert.Equal(CustomerErrors.CreditLimitExceeded, result.TopError);
  }

  [Fact]
  public void ValidateCredit_WhenInactive_ShouldFail()
  {
    var customer = CreateValidCustomer();
    customer.Deactivate();

    var result = customer.ValidateCredit(Money.EGP(100).Value);

    Assert.True(result.IsError);
    Assert.Equal(CustomerErrors.InactiveCustomer, result.TopError);
  }

  [Fact]
  public void IncreaseBalance_ShouldAddToCurrentBalance()
  {
    var customer = CreateValidCustomer();

    var result = customer.IncreaseBalance(Money.EGP(1000).Value);

    Assert.True(result.IsSuccess);
    Assert.Equal(1000, customer.CurrentBalance.Amount);
  }

  [Fact]
  public void DecreaseBalance_WithValidAmount_ShouldSucceed()
  {
    var customer = CreateValidCustomer();
    customer.IncreaseBalance(Money.EGP(1000).Value);

    var result = customer.DecreaseBalance(Money.EGP(400).Value);

    Assert.True(result.IsSuccess);
    Assert.Equal(600, customer.CurrentBalance.Amount);
  }

  [Fact]
  public void DecreaseBalance_MoreThanCurrentBalance_ShouldFail()
  {
    var customer = CreateValidCustomer();
    customer.IncreaseBalance(Money.EGP(100).Value);

    var result = customer.DecreaseBalance(Money.EGP(500).Value);

    Assert.True(result.IsError);
  }

  [Fact]
  public void Deactivate_WhenActive_ShouldSucceed()
  {
    var customer = CreateValidCustomer();

    var result = customer.Deactivate();

    Assert.True(result.IsSuccess);
    Assert.False(customer.IsActive);
  }

  [Fact]
  public void Deactivate_WhenAlreadyInactive_ShouldFail()
  {
    var customer = CreateValidCustomer();
    customer.Deactivate();

    var result = customer.Deactivate();

    Assert.True(result.IsError);
    Assert.Equal(CustomerErrors.AlreadyInactive, result.TopError);
  }

  [Fact]
  public void UpdateContactInfo_ShouldUpdatePhoneAndEmail()
  {
    var customer = CreateValidCustomer();
    var newPhone = PhoneNumber.Create("01112345678").Value;
    var newEmail = Email.Create("new@example.com").Value;

    var result = customer.UpdateContactInfo(newPhone, newEmail);

    Assert.True(result.IsSuccess);
    Assert.Equal(newPhone, customer.Phone);
    Assert.Equal(newEmail, customer.Email);
  }
}
