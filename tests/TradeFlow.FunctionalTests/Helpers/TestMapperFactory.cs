namespace TradeFlow.Application.UnitTests.Helpers;

using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using TradeFlow.Application.Common.Mappings;

public static class TestMapperFactory
{
  public static IMapper Create()
  {
    var config = new MapperConfiguration(cfg =>
        cfg.AddMaps(typeof(MappingProfile).Assembly), NullLoggerFactory.Instance);

    return config.CreateMapper();
  }
}
