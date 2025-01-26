using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities
{
    /// <summary>
    /// Unit tests for the Sale entity, focusing on domain logic and business rules
    /// </summary>
    public class SaleTests
    {
        [Fact]
        public void Constructor_ShouldSetInitialProperties()
        {
            var customerId = Guid.NewGuid();
            var customerName = "Test Customer";
            var branchId = Guid.NewGuid();
            var branchName = "Test Branch";
            var saleDate = DateTime.UtcNow;

            var sale = new Sale(customerId, customerName, branchId, branchName, saleDate);

            Assert.Equal(customerId, sale.CustomerId);
            Assert.Equal(customerName, sale.CustomerName);
            Assert.Equal(branchId, sale.BranchId);
            Assert.Equal(branchName, sale.BranchName);
            Assert.Equal(saleDate, sale.SaleDate);
            Assert.Equal(0, sale.TotalAmount);
            Assert.False(sale.IsCancelled);
            Assert.NotEmpty(sale.SaleNumber);
            Assert.Contains("SALE-", sale.SaleNumber);
        }

        [Theory]
        [InlineData(3, 0)] // Below threshold - no discount
        [InlineData(4, 0.10)] // First threshold - 10% discount
        [InlineData(9, 0.10)] // First threshold - 10% discount
        [InlineData(10, 0.20)] // Second threshold - 20% discount
        [InlineData(20, 0.20)] // Second threshold - 20% discount
        public void AddItem_ShouldApplyCorrectDiscount(int quantity, decimal expectedDiscount)
        {
            var sale = CreateTestSale();
            var productId = Guid.NewGuid();
            var unitPrice = 100m;

            sale.AddItem(productId, "Test Product", unitPrice, quantity);

            var item = sale.Items.First();
            Assert.Equal(expectedDiscount, item.Discount);
            Assert.Equal(quantity * unitPrice * (1 - expectedDiscount), item.TotalAmount);
        }

        [Fact]
        public void AddItem_WithQuantityAbove20_ShouldThrowException()
        {
            var sale = CreateTestSale();
            var productId = Guid.NewGuid();

            var exception = Assert.Throws<DomainException>(() =>
                sale.AddItem(productId, "Test Product", 100m, 21));
            Assert.Equal("Cannot add more than 20 items of the same product", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldSetInitialPropertiesAndRaiseEvent()
        {
            var customerId = Guid.NewGuid();
            var customerName = "Test Customer";
            var branchId = Guid.NewGuid();
            var branchName = "Test Branch";
            var saleDate = DateTime.UtcNow;

            var sale = new Sale(customerId, customerName, branchId, branchName, saleDate);

            Assert.Equal(customerId, sale.CustomerId);
            Assert.Equal(customerName, sale.CustomerName);
            Assert.Equal(branchId, sale.BranchId);
            Assert.Equal(branchName, sale.BranchName);
            Assert.Equal(saleDate, sale.SaleDate);

            var createdEvent = Assert.Single(sale.DomainEvents);
            Assert.IsType<SaleCreatedEvent>(createdEvent);
            var saleCreatedEvent = (SaleCreatedEvent)createdEvent;
            Assert.Equal(sale, saleCreatedEvent.Sale);
        }

        private static Sale CreateTestSale()
        {
            return new Sale(
                Guid.NewGuid(),
                "Test Customer",
                Guid.NewGuid(),
                "Test Branch",
                DateTime.UtcNow
            );
        }
    }
}
