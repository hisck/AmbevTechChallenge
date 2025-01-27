using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale
{
    public class CreateSaleProfile : Profile
    {
        public CreateSaleProfile()
        {
            CreateMap<CreateSaleCommand, Sale>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());

            CreateMap<CreateSaleCommand.CreateSaleItemCommand, SaleItem>()
            .ConstructUsing((src, ctx) => new SaleItem(
                Guid.Empty,
                src.ProductId,
                src.ProductName,
                src.UnitPrice,
                src.Quantity
            ));
            CreateMap<Sale, CreateSaleResult>();
            CreateMap<Sale, SaleDto>();
            CreateMap<SaleItem, SaleItemDto>();
        }
    }
}
