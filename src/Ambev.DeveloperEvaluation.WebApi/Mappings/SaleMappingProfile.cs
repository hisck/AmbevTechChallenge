using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.Common;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Mappings
{
    /// <summary>
    /// Global mapping profile for Sale-related DTOs and responses
    /// </summary>
    public class SaleMappingProfile : Profile
    {
        /// <summary>
        /// Initializes the common mappings for Sale entities and responses
        /// </summary>
        public SaleMappingProfile()
        {
            // Common mappings for sale responses
            CreateMap<SaleDto, SaleResponse>();
            CreateMap<SaleItemDto, SaleItemResponse>();
            CreateMap<PaginatedList<SaleDto>, PaginatedResponse<SaleResponse>>();
        }
    }
}
