using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TradeFlow.Application.Common.Interfaces;

namespace TradeFlow.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
  private readonly IHttpContextAccessor _httpContextAccessor;

  public CurrentUserService(IHttpContextAccessor httpContextAccessor)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

  public Guid? UserId
  {
    get
    {
      var id = User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
      return string.IsNullOrEmpty(id) ? null : Guid.Parse(id);
    }
  }

  public string? UserName =>
      User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

  public string? Email =>
      User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
  public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

  public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
