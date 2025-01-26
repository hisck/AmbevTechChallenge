namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.UpdateSale
{
    /// <summary>
    /// Request model for updating an existing sale
    /// </summary>
    public class UpdateSaleRequest
    {

        /// <summary>
        /// Gets or sets the customer's global identifier
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer's name for denormalization
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the branch's global identifier
        /// </summary>
        public Guid BranchId { get; set; }

        /// <summary>
        /// Gets or sets the branch name for denormalization
        /// </summary>
        public string BranchName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of items in the sale
        /// </summary>
        public List<UpdateSaleItemRequest> Items { get; set; } = [];

        /// <summary>
        /// Represents an item in the sale update request
        /// </summary>
        public class UpdateSaleItemRequest
        {
            /// <summary>
            /// Gets or sets the product's global identifier
            /// </summary>
            public Guid ProductId { get; set; }

            /// <summary>
            /// Gets or sets the product name for denormalization
            /// </summary>
            public string ProductName { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the unit price of the product
            /// </summary>
            public decimal UnitPrice { get; set; }

            /// <summary>
            /// Gets or sets the quantity being purchased.
            /// Must be between 1 and 20 items
            /// </summary>
            public int Quantity { get; set; }
        }
    }

}
