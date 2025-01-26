namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.ListSales
{
    /// <summary>
    /// Request model for listing sales with pagination and ordering
    /// </summary>
    public class ListSalesRequest
    {
        /// <summary>
        /// Gets or sets the page number for pagination (1-based)
        /// </summary>
        public int? _page { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page
        /// </summary>
        public int? _size { get; set; }

        /// <summary>
        /// Gets or sets the ordering expression (e.g., "saleDate desc")
        /// </summary>
        public string? _order { get; set; }
    }

}
