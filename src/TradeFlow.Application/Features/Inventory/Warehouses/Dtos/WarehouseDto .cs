namespace TradeFlow.Application.Inventory.Warehouses;

using TradeFlow.Application.Common.Mappings;
using TradeFlow.Domain.Inventory;

public sealed class WarehouseDto : IMapFrom<Warehouse>
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Location { get; set; } = string.Empty;
  public bool IsActive { get; set; }

  public void Mapping(AutoMapper.Profile profile)
      => profile.CreateMap<Warehouse, WarehouseDto>()
          .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.Value));
}
