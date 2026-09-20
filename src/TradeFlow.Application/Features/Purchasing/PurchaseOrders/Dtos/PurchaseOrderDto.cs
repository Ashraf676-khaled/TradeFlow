namespace TradeFlow.Application.Purchasing.PurchaseOrders;

using TradeFlow.Application.Common.Mappings;
using TradeFlow.Domain.Purchasing;

public sealed class PurchaseOrderItemDto : IMapFrom<PurchaseOrderItem>
{
  public Guid ProductId { get; set; }
  public int OrderedQuantity { get; set; }
  public int ReceivedQuantity { get; set; }
  public decimal UnitCost { get; set; }
  public bool IsFullyReceived { get; set; }

  public void Mapping(AutoMapper.Profile profile)
      => profile.CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
          .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId.Value))
          .ForMember(d => d.OrderedQuantity, o => o.MapFrom(s => s.OrderedQuantity.Value))
          .ForMember(d => d.ReceivedQuantity, o => o.MapFrom(s => s.ReceivedQuantity.Value))
          .ForMember(d => d.UnitCost, o => o.MapFrom(s => s.UnitCost.Amount));
}

public sealed class PurchaseOrderDto : IMapFrom<PurchaseOrder>
{
  public Guid Id { get; set; }
  public Guid SupplierId { get; set; }
  public Guid WarehouseId { get; set; }
  public string OrderNumber { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
  public DateTimeOffset CreatedAt { get; set; }
  public List<PurchaseOrderItemDto> Items { get; set; } = [];

  public void Mapping(AutoMapper.Profile profile)
      => profile.CreateMap<PurchaseOrder, PurchaseOrderDto>()
          .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.Value))
          .ForMember(d => d.SupplierId, o => o.MapFrom(s => s.SupplierId.Value))
          .ForMember(d => d.WarehouseId, o => o.MapFrom(s => s.WarehouseId.Value))
          .ForMember(d => d.OrderNumber, o => o.MapFrom(s => s.OrderNumber.Value))
          .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
}
