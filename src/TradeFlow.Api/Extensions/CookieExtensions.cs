namespace TradeFlow.Api.Extensions;

public static class CookieExtensions
{
  private const string RefreshTokenCookieName = "refreshToken";

  public static void SetRefreshTokenCookie(this HttpContext httpContext, string token, DateTimeOffset expiresUtc)
  {
    httpContext.Response.Cookies.Append(RefreshTokenCookieName, token, new CookieOptions
    {
      HttpOnly = true,                          // JS مايقدرش يقرأها خالص — حماية من XSS
      Secure = true,                             // بترسل بس عبر HTTPS
      SameSite = SameSiteMode.None,
      Expires = expiresUtc,
      Path = "/api/auth"                         // الكوكي تتبعت بس مع طلبات الـauth، مش كل الـAPI
    });
  }

  public static string? GetRefreshTokenFromCookie(this HttpContext httpContext)
      => httpContext.Request.Cookies.TryGetValue(RefreshTokenCookieName, out var token) ? token : null;

  public static void ClearRefreshTokenCookie(this HttpContext httpContext)
  {
    httpContext.Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
    {
      HttpOnly = true,
      Secure = true,
      SameSite = SameSiteMode.None,
      Path = "/api/auth"
    });
  }
}
