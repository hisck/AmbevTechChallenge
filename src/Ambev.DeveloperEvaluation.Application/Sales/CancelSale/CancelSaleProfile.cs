using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale
{
    public class CancelSaleProfile : Profile
    {
        public CancelSaleProfile()
        {
            CreateMap<Sale, CancelSaleResult>();
            CreateMap<Sale, SaleDto>();
            CreateMap<SaleItem, SaleItemDto>();
        }
    }
}
