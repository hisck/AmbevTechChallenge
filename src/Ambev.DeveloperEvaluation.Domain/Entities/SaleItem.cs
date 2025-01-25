using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities
{
    public class SaleItem : BaseEntity
    {
        public Guid SaleId { get; private set; }
        public Guid ProductId { get; private set; }
        public string ProductName { get; private set; }
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }
        public decimal Discount { get; private set; }
        public decimal TotalAmount { get; private set; }
        public bool IsCancelled { get; private set; }

        protected SaleItem() { }

        public SaleItem(Guid saleId, Guid productId, string productName, decimal unitPrice, int quantity)
        {
            SaleId = saleId;
            ProductId = productId;
            ProductName = productName;
            UnitPrice = unitPrice;
            Quantity = quantity;

            CalculateDiscount();
            CalculateTotalAmount();
        }

        private void CalculateDiscount()
        {
            // Apply business rules for discounts
            if (Quantity >= 10 && Quantity <= 20)
                Discount = 0.20m; // 20% discount
            else if (Quantity >= 4)
                Discount = 0.10m; // 10% discount
            else
                Discount = 0m;
        }

        private void CalculateTotalAmount()
        {
            var subtotal = UnitPrice * Quantity;
            TotalAmount = subtotal - (subtotal * Discount);
        }

        public void Cancel()
        {
            if (IsCancelled)
                throw new DomainException("Item is already cancelled");

            IsCancelled = true;
        }
    }
}
