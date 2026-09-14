// Infrastructure/Identity/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace TradeFlow.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
  // أي Properties إضافية خاصة بمشروعك تتحط هنا لاحقًا
}
