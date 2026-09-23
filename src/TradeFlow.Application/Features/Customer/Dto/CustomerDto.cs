namespace TradeFlow.Application.Customers;

using TradeFlow.Application.Common.Mappings;
using TradeFlow.Domain.Customers;

public sealed class CustomerDto : IMapFrom<Customer>
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Phone { get; set; } = string.Empty;
  public string? Email { get; set; }
  public decimal CreditLimit { get; set; }
  public decimal CurrentBalance { get; set; }
  public bool IsActive { get; set; }

  public void Mapping(AutoMapper.Profile profile)
      => profile.CreateMap<Customer, CustomerDto>()
          .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.Value))
          .ForMember(d => d.Phone, o => o.MapFrom(s => s.Phone.Value))
          .ForMember(d => d.Email, o => o.MapFrom(s => s.Email == null ? null : s.Email.Value))
          .ForMember(d => d.CreditLimit, o => o.MapFrom(s => s.CreditLimit.Amount))
          .ForMember(d => d.CurrentBalance, o => o.MapFrom(s => s.CurrentBalance.Amount));
}
