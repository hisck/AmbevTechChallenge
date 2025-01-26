namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.Common
{
    /// <summary>
    /// Common response model for sale item information
    /// </summary>
    public class SaleItemResponse
    {
        /// <summary>
        /// Gets or sets the global identifier of the sale item
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the product's global identifier
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product name
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unit price of the product
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the quantity purchased
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the discount applied to this item
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Gets or sets the total amount for this item
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets whether the item is cancelled
        /// </summary>
        public bool IsCancelled { get; set; }
    }
}
