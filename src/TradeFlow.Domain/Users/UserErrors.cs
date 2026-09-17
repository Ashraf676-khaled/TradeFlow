namespace TradeFlow.Domain.Users;

using TradeFlow.Domain.Common.Results;

public static class UserErrors
{
  public static readonly Error FullNameRequired = Error.Validation(
      "User.FullNameRequired", "Full name is required.");

  public static readonly Error FullNameTooLong = Error.Validation(
      "User.FullNameTooLong", "Full name cannot exceed 150 characters.");

  public static readonly Error PasswordHashRequired = Error.Validation(
      "User.PasswordHashRequired", "Password hash is required.");

  public static readonly Error InactiveUser = Error.Validation(
      "User.InactiveUser", "Cannot issue a refresh token for an inactive user.");

  public static readonly Error RefreshTokenNotFound = Error.NotFound(
      "User.RefreshTokenNotFound", "Refresh token not found.");

  public static readonly Error TokenRequired = Error.Validation(
      "User.TokenRequired", "Token cannot be empty.");

  public static readonly Error ExpiryMustBeInFuture = Error.Validation(
      "User.ExpiryMustBeInFuture", "Expiry date must be in the future.");

  public static readonly Error TokenAlreadyRevoked = Error.Conflict(
      "User.TokenAlreadyRevoked", "Token is already revoked.");

  public static readonly Error AlreadyInactive = Error.Conflict(
      "User.AlreadyInactive", "User is already inactive.");

  public static readonly Error AlreadyActive = Error.Conflict(
      "User.AlreadyActive", "User is already active.");

  public static readonly Error NotFound = Error.NotFound(
      "User.NotFound", "User not found.");

  public static readonly Error EmailAlreadyExists = Error.Conflict(
      "User.EmailAlreadyExists", "A user with this email already exists.");
}
