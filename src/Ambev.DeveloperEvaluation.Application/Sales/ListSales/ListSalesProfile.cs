using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales
{
    public class GetSalesProfile : Profile
    {
        public GetSalesProfile()
        {
            CreateMap<Sale, SaleDto>();
            CreateMap<SaleItem, SaleItemDto>();
        }
    }
}
