using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Common.Events
{
    /// <summary>
    /// Implements event publishing using Rebus as the message broker
    /// </summary>
    public class RebusEventPublisher : IEventPublisher
    {
        private readonly IBus _bus;
        private readonly ILogger<RebusEventPublisher> _logger;

        public RebusEventPublisher(IBus bus, ILogger<RebusEventPublisher> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public async Task PublishAsync<T>(T @event) where T : DomainEvent
        {
            try
            {
                _logger.LogInformation(
                    "Publishing {EventType} event. Event details: {@Event}",
                    typeof(T).Name,
                    @event);

                await _bus.Publish(@event);

                _logger.LogInformation(
                    "Successfully published {EventType} event",
                    typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish {EventType} event. Event details: {@Event}",
                    typeof(T).Name,
                    @event);
                throw;
            }
        }
    }
}