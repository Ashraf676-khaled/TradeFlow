namespace TradeFlow.Application.Inventory.StockItems;

using TradeFlow.Application.Common.Mappings;
using TradeFlow.Domain.Inventory;

public sealed class StockItemDto : IMapFrom<StockItem>
{
  public Guid Id { get; set; }
  public Guid WarehouseId { get; set; }
  public Guid ProductId { get; set; }
  public int AvailableQuantity { get; set; }
  public int ReservedQuantity { get; set; }

  public void Mapping(AutoMapper.Profile profile)
      => profile.CreateMap<StockItem, StockItemDto>()
          .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.Value))
          .ForMember(d => d.WarehouseId, o => o.MapFrom(s => s.WarehouseId.Value))
          .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId.Value))
          .ForMember(d => d.AvailableQuantity, o => o.MapFrom(s => s.AvailableQuantity.Value))
          .ForMember(d => d.ReservedQuantity, o => o.MapFrom(s => s.ReservedQuantity.Value));
}
