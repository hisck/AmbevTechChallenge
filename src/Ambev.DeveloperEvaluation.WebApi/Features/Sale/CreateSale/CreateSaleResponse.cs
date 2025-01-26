using Ambev.DeveloperEvaluation.WebApi.Features.Sale.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.CreateSale
{
    /// <summary>
    /// API response model for CreateSale operation
    /// </summary>
    public class CreateSaleResponse
    {
        /// <summary>
        /// The unique sale number generated for this sale
        /// </summary>
        public string SaleNumber { get; set; } = string.Empty;

        /// <summary>
        /// The complete sale information
        /// </summary>
        public SaleResponse Sale { get; set; } = null!;
    }
}
