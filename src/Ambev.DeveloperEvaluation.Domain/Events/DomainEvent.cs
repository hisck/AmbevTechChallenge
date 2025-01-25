namespace Ambev.DeveloperEvaluation.Domain.Events
{
    public abstract class DomainEvent
    {
        public DateTime OccurredOn { get; }

        protected DomainEvent()
        {
            OccurredOn = DateTime.UtcNow;
        }
    }
}
