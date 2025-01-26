namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.CancelSale
{
    /// <summary>
    /// Request model for cancelling a sale
    /// </summary>
    public class CancelSaleRequest
    {
        /// <summary>
        /// The global identifier of the sale to cancel
        /// </summary>
        public Guid Id { get; set; }
    }
}
