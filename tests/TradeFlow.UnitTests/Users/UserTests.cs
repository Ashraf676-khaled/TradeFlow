namespace TradeFlow.Domain.Tests.Users;

using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Common.ValueObjects;
using TradeFlow.Domain.Users;
using Xunit;

public class UserTests
{
  private static readonly TenantId TestTenant = TenantId.New();

  private static User CreateValidUser()
  {
    var email = Email.Create("ahmed@example.com").Value;
    return User.Create(TestTenant, "Ahmed Ali", email, "hashed_password", UserRole.SalesRepresentative).Value;
  }

  [Fact]
  public void Create_WithValidData_ShouldSucceed()
  {
    var email = Email.Create("ahmed@example.com").Value;

    var result = User.Create(TestTenant, "Ahmed Ali", email, "hashed_password", UserRole.Admin);

    Assert.True(result.IsSuccess);
    Assert.True(result.Value.IsActive);
    Assert.Empty(result.Value.RefreshTokens);
  }

  [Fact]
  public void Create_WithEmptyFullName_ShouldFail()
  {
    var email = Email.Create("ahmed@example.com").Value;

    var result = User.Create(TestTenant, "", email, "hashed_password", UserRole.Admin);

    Assert.True(result.IsError);
    Assert.Equal(UserErrors.FullNameRequired, result.TopError);
  }

  [Fact]
  public void Create_WithEmptyPasswordHash_ShouldFail()
  {
    var email = Email.Create("ahmed@example.com").Value;

    var result = User.Create(TestTenant, "Ahmed Ali", email, "", UserRole.Admin);

    Assert.True(result.IsError);
    Assert.Equal(UserErrors.PasswordHashRequired, result.TopError);
  }

  [Fact]
  public void IssueRefreshToken_ForActiveUser_ShouldSucceed()
  {
    var user = CreateValidUser();
    var expiry = DateTimeOffset.UtcNow.AddDays(7);

    var result = user.IssueRefreshToken("token-abc-123", expiry);

    Assert.True(result.IsSuccess);
    Assert.Single(user.RefreshTokens);
    Assert.True(result.Value.IsActive);
  }

  [Fact]
  public void IssueRefreshToken_ForInactiveUser_ShouldFail()
  {
    var user = CreateValidUser();
    user.Deactivate();

    var result = user.IssueRefreshToken("token-abc-123", DateTimeOffset.UtcNow.AddDays(7));

    Assert.True(result.IsError);
    Assert.Equal(UserErrors.InactiveUser, result.TopError);
  }

  [Fact]
  public void IssueRefreshToken_WithPastExpiry_ShouldFail()
  {
    var user = CreateValidUser();

    var result = user.IssueRefreshToken("token-abc-123", DateTimeOffset.UtcNow.AddDays(-1));

    Assert.True(result.IsError);
    Assert.Equal(UserErrors.ExpiryMustBeInFuture, result.TopError);
  }

  [Fact]
  public void RevokeRefreshToken_ExistingToken_ShouldSucceed()
  {
    var user = CreateValidUser();
    user.IssueRefreshToken("token-abc-123", DateTimeOffset.UtcNow.AddDays(7));

    var result = user.RevokeRefreshToken("token-abc-123");

    Assert.True(result.IsSuccess);

    var validateResult = user.ValidateRefreshToken("token-abc-123");
    Assert.True(validateResult.IsSuccess);
    Assert.False(validateResult.Value); // مش Active دلوقتي
  }

  [Fact]
  public void RevokeRefreshToken_NonExistingToken_ShouldFail()
  {
    var user = CreateValidUser();

    var result = user.RevokeRefreshToken("nonexistent-token");

    Assert.True(result.IsError);
    Assert.Equal(UserErrors.RefreshTokenNotFound, result.TopError);
  }

  [Fact]
  public void RevokeRefreshToken_AlreadyRevoked_ShouldFail()
  {
    var user = CreateValidUser();
    user.IssueRefreshToken("token-abc-123", DateTimeOffset.UtcNow.AddDays(7));
    user.RevokeRefreshToken("token-abc-123");

    var result = user.RevokeRefreshToken("token-abc-123");

    Assert.True(result.IsError);
    Assert.Equal(UserErrors.TokenAlreadyRevoked, result.TopError);
  }

  [Fact]
  public void ValidateRefreshToken_ActiveToken_ShouldReturnTrue()
  {
    var user = CreateValidUser();
    user.IssueRefreshToken("token-abc-123", DateTimeOffset.UtcNow.AddDays(7));

    var result = user.ValidateRefreshToken("token-abc-123");

    Assert.True(result.IsSuccess);
    Assert.True(result.Value);
  }

  [Fact]
  public void ChangePasswordHash_WithValidHash_ShouldSucceed()
  {
    var user = CreateValidUser();

    var result = user.ChangePasswordHash("new_hashed_password");

    Assert.True(result.IsSuccess);
    Assert.Equal("new_hashed_password", user.PasswordHash);
  }

  [Fact]
  public void ChangeRole_ShouldUpdateRole()
  {
    var user = CreateValidUser();

    var result = user.ChangeRole(UserRole.Admin);

    Assert.True(result.IsSuccess);
    Assert.Equal(UserRole.Admin, user.Role);
  }

  [Fact]
  public void Deactivate_WhenActive_ShouldSucceed()
  {
    var user = CreateValidUser();

    var result = user.Deactivate();

    Assert.True(result.IsSuccess);
    Assert.False(user.IsActive);
  }

  [Fact]
  public void Deactivate_WhenAlreadyInactive_ShouldFail()
  {
    var user = CreateValidUser();
    user.Deactivate();

    var result = user.Deactivate();

    Assert.True(result.IsError);
    Assert.Equal(UserErrors.AlreadyInactive, result.TopError);
  }
}
