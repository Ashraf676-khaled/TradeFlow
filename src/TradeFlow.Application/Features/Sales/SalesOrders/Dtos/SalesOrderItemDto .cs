namespace TradeFlow.Application.Sales.SalesOrders;

using TradeFlow.Application.Common.Mappings;
using TradeFlow.Domain.Sales;

public sealed class SalesOrderItemDto : IMapFrom<SalesOrderItem>
{
  public Guid ProductId { get; set; }
  public int Quantity { get; set; }
  public decimal UnitPrice { get; set; }
  public decimal ItemDiscountPercentage { get; set; }

  public void Mapping(AutoMapper.Profile profile)
      => profile.CreateMap<SalesOrderItem, SalesOrderItemDto>()
          .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId.Value))
          .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Quantity.Value))
          .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.UnitPrice.Amount))
          .ForMember(d => d.ItemDiscountPercentage, o => o.MapFrom(s => s.ItemDiscount.Percentage));
}

public sealed class SalesOrderDto : IMapFrom<SalesOrder>
{
  public Guid Id { get; set; }
  public Guid CustomerId { get; set; }
  public Guid SalesRepresentativeId { get; set; }
  public Guid WarehouseId { get; set; }
  public DateTimeOffset OrderDate { get; set; }
  public string Status { get; set; } = string.Empty;
  public decimal OrderDiscountPercentage { get; set; }
  public List<SalesOrderItemDto> Items { get; set; } = [];

  public void Mapping(AutoMapper.Profile profile)
      => profile.CreateMap<SalesOrder, SalesOrderDto>()
          .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.Value))
          .ForMember(d => d.CustomerId, o => o.MapFrom(s => s.CustomerId.Value))
          .ForMember(d => d.SalesRepresentativeId, o => o.MapFrom(s => s.SalesRepresentativeId.Value))
          .ForMember(d => d.WarehouseId, o => o.MapFrom(s => s.WarehouseId.Value))
          .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
          .ForMember(d => d.OrderDiscountPercentage, o => o.MapFrom(s => s.OrderDiscount.Percentage));
}
