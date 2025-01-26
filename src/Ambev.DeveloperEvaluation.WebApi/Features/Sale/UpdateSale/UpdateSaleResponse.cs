using Ambev.DeveloperEvaluation.WebApi.Features.Sale.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.UpdateSale
{
    /// <summary>
    /// API response model for UpdateSale operation
    /// </summary>
    public class UpdateSaleResponse
    {
        /// <summary>
        /// The complete updated sale information
        /// </summary>
        public SaleResponse Sale { get; set; } = null!;
    }
}
