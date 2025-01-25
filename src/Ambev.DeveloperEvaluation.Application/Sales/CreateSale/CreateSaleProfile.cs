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
            .ForMember(dest => dest.Items, opt => opt.Ignore()); // Items are handled separately in handler

            CreateMap<CreateSaleCommand.CreateSaleItemCommand, SaleItem>();
            CreateMap<Sale, CreateSaleResult>();
            CreateMap<Sale, SaleDto>();
            CreateMap<SaleItem, SaleItemDto>();
        }
    }
}
