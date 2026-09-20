using TradeFlow.Application.Common.Mappings;
using TradeFlow.Domain.Common.Identifiers;
using TradeFlow.Domain.Purchasing;

namespace TradeFlow.Application.Features.Purchasing.Suppliers.Dtos;

public sealed  class SupplierDto : IMapFrom<Supplier>
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Phone { get; set; } = string.Empty;
  public bool IsActive { get; set; }

  public void Mapping(AutoMapper.Profile profile)
  =>
    profile.CreateMap<Supplier, SupplierDto>()
.ForMember(d => d.Id, o => o.MapFrom(s => s.Id.Value))
            .ForMember(d => d.Phone, o => o.MapFrom(s => s.Phone.Value));

}
