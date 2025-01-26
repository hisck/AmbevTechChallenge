using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.Common;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.ListSales
{
    /// <summary>
    /// Profile for mapping ListSales feature requests to queries
    /// </summary>
    public class ListSalesProfile : Profile
    {
        /// <summary>
        /// Initializes the mappings for ListSales feature
        /// </summary>
        public ListSalesProfile()
        {
            CreateMap<ListSalesRequest, ListSalesCommand>();
            CreateMap<ListSalesResult, PaginatedResponse<SaleResponse>>();
        }
    }
}
