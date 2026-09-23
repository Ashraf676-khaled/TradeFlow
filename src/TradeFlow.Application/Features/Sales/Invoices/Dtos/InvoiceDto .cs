namespace TradeFlow.Application.Sales.Invoices;

using TradeFlow.Application.Common.Mappings;
using TradeFlow.Domain.Sales;

public sealed class InvoiceDto : IMapFrom<Invoice>
{
  public Guid Id { get; set; }
  public Guid SalesOrderId { get; set; }
  public Guid CustomerId { get; set; }
  public string InvoiceNumber { get; set; } = string.Empty;
  public DateTimeOffset IssuedAt { get; set; }
  public DateTimeOffset DueDate { get; set; }
  public decimal TotalAmount { get; set; }
  public string Status { get; set; } = string.Empty;

  public void Mapping(AutoMapper.Profile profile)
      => profile.CreateMap<Invoice, InvoiceDto>()
          .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.Value))
          .ForMember(d => d.SalesOrderId, o => o.MapFrom(s => s.SalesOrderId.Value))
          .ForMember(d => d.CustomerId, o => o.MapFrom(s => s.CustomerId.Value))
          .ForMember(d => d.InvoiceNumber, o => o.MapFrom(s => s.InvoiceNumber.Value))
          .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.TotalAmount.Amount))
          .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
}
