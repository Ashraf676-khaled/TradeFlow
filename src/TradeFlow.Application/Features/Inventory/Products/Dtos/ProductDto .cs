namespace TradeFlow.Application.Inventory.Products;

using TradeFlow.Application.Common.Mappings;
using TradeFlow.Domain.Inventory;

public sealed class ProductDto : IMapFrom<Product>
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Sku { get; set; } = string.Empty;
  public decimal SellingPrice { get; set; }
  public string SellingPriceCurrency { get; set; } = string.Empty;
  public decimal Cost { get; set; }
  public int MinimumStock { get; set; }
  public bool IsActive { get; set; }

  public void Mapping(AutoMapper.Profile profile)
      => profile.CreateMap<Product, ProductDto>()
          .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.Value))
          .ForMember(d => d.Sku, o => o.MapFrom(s => s.Sku.Value))
          .ForMember(d => d.SellingPrice, o => o.MapFrom(s => s.SellingPrice.Amount))
          .ForMember(d => d.SellingPriceCurrency, o => o.MapFrom(s => s.SellingPrice.Currency))
          .ForMember(d => d.Cost, o => o.MapFrom(s => s.Cost.Amount));
}
