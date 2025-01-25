using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Domain.Entities
{
    public class Sale : BaseEntity
    {
        private readonly List<SaleItem> _items = new();

        public string SaleNumber { get; private set; }
        public DateTime SaleDate { get; private set; }
        public Guid CustomerId { get; private set; }
        public string CustomerName { get; private set; }
        public Guid BranchId { get; private set; }
        public string BranchName { get; private set; }
        public bool IsCancelled { get; private set; }
        public decimal TotalAmount { get; private set; }
        public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

        protected Sale() { }

        public Sale(Guid customerId, string customerName, Guid branchId, string branchName, DateTime saleDate)
        {
            CustomerId = customerId;
            CustomerName = customerName;
            BranchId = branchId;
            BranchName = branchName;
            SaleDate = saleDate;
            SaleNumber = GenerateSaleNumber();
            TotalAmount = 0;

            AddDomainEvent(new SaleCreatedEvent(this));
        }

        public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
        {
            ValidateItemQuantity(quantity);

            var item = new SaleItem(this.Id, productId, productName, unitPrice, quantity);
            _items.Add(item);

            UpdateTotalAmount();

            AddDomainEvent(new SaleModifiedEvent(this));
        }

        public void Cancel()
        {
            if (IsCancelled)
                throw new DomainException("Sale is already cancelled");

            IsCancelled = true;
            TotalAmount = 0;
            AddDomainEvent(new SaleCancelledEvent(this));
        }

        public void CancelItem(Guid itemId)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                throw new DomainException($"Item {itemId} not found in sale {SaleNumber}");

            item.Cancel();

            UpdateTotalAmount();
            AddDomainEvent(new ItemCancelledEvent(this, item));
        }

        private void UpdateTotalAmount()
        {
            // Sum only non-cancelled items
            TotalAmount = _items
                .Where(item => !item.IsCancelled)
                .Sum(item => item.TotalAmount);
        }

        private decimal CalculateTotalAmount()
        {
            return _items
                .Where(item => !item.IsCancelled)
                .Sum(item => item.TotalAmount);
        }


        private void ValidateItemQuantity(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Quantity must be greater than zero");
            if (quantity > 20)
                throw new DomainException("Cannot add more than 20 items of the same product");
        }

        private string GenerateSaleNumber()
        {
            return $"SALE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8)}".ToUpper();
        }
    }
}
