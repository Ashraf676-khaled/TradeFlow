namespace TradeFlow.Application.UnitTests.Helpers;

using Microsoft.EntityFrameworkCore;
using TradeFlow.Infrastructure.Data;

public static class TestDbContextFactory
{
  public static AppDbContext Create(FakeCurrentUserService currentUser)
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()) // قاعدة بيانات منفصلة لكل اختبار
        .Options;

    return new AppDbContext(options, currentUser);
  }
}
