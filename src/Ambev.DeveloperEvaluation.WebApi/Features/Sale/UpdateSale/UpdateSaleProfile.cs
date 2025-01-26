using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.UpdateSale
{
    /// <summary>
    /// Profile for mapping UpdateSale feature requests to commands
    /// </summary>
    public class UpdateSaleProfile : Profile
    {
        /// <summary>
        /// Initializes the mappings for UpdateSale feature
        /// </summary>
        public UpdateSaleProfile()
        {
            CreateMap<UpdateSaleRequest, UpdateSaleCommand>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<UpdateSaleRequest.UpdateSaleItemRequest, UpdateSaleCommand.UpdateSaleItemCommand>();
            CreateMap<UpdateSaleResult, UpdateSaleResponse>();
        }
    }
}
