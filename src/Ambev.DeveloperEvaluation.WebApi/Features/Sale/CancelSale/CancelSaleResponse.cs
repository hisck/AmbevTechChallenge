using Ambev.DeveloperEvaluation.WebApi.Features.Sale.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.CancelSale
{
    /// <summary>
    /// API response model for CancelSale operation
    /// </summary>
    public class CancelSaleResponse
    {
        /// <summary>
        /// The complete cancelled sale information
        /// </summary>
        public SaleResponse Sale { get; set; } = null!;
    }
}
