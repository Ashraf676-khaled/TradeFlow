using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace TradeFlow.Api.Security;

/// <summary>
/// حماية لوحة Hangfire في الإنتاج — يُسمح فقط للمستخدمين المسجلين دخولهم بالدخول.
/// لو عايز تقيّدها أكثر للمديرين فقط، أضف تحقق من Claim من نوع PlatformAdmin.
/// </summary>
public sealed class HangfireAuthFilter : IDashboardAuthorizationFilter
{
  public bool Authorize(DashboardContext context)
  {
    var httpContext = context.GetHttpContext();
    return httpContext.User?.Identity?.IsAuthenticated == true;
  }
}
