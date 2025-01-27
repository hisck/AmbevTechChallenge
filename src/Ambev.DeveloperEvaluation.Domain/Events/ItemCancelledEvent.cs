using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Common.Events;

namespace Ambev.DeveloperEvaluation.Domain.Events
{
    public class ItemCancelledEvent : DomainEvent
    {
        public Sale Sale { get; }
        public SaleItem Item { get; }

        public ItemCancelledEvent(Sale sale, SaleItem item)
        {
            Sale = sale;
            Item = item;
        }
    }
}
