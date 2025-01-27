namespace Ambev.DeveloperEvaluation.Common.Events
{
    /// <summary>
    /// Defines a contract for publishing domain events across the application
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes a domain event to the configured message broker
        /// </summary>
        /// <typeparam name="T">The type of domain event to publish</typeparam>
        /// <param name="event">The event instance to publish</param>
        Task PublishAsync<T>(T @event) where T : DomainEvent;
    }
}