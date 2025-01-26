using Ambev.DeveloperEvaluation.WebApi.Features.Sale.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.GetSale
{
    /// <summary>
    /// API response model for GetSale operation
    /// </summary>
    public class GetSaleResponse
    {
        /// <summary>
        /// The complete sale information
        /// </summary>
        public SaleResponse Sale { get; set; } = null!;
    }
}
