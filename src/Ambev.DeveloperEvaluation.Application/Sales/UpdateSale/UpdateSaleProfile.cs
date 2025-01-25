using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale
{
    public class UpdateSaleProfile : Profile
    {
        public UpdateSaleProfile()
        {
            CreateMap<UpdateSaleCommand, Sale>()
                .ForMember(dest => dest.Items, opt => opt.Ignore()); // Items are handled separately in handler

            CreateMap<UpdateSaleCommand.UpdateSaleItemCommand, SaleItem>();
            CreateMap<Sale, UpdateSaleResult>();
            CreateMap<Sale, SaleDto>();
            CreateMap<SaleItem, SaleItemDto>();
        }
    }
}
