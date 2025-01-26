namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.GetSale
{
    /// <summary>
    /// Request model for getting a sale by ID
    /// </summary>
    public class GetSaleRequest
    {
        /// <summary>
        /// The global identifier of the sale to retrieve
        /// </summary>
        public Guid Id { get; set; }
    }
}
