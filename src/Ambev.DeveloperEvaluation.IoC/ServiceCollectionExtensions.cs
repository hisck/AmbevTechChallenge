using Ambev.DeveloperEvaluation.Common.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.IoC
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventPublishing(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found in configuration");
            }

            services.AddRebus((configure, serviceProvider) => configure
                .Transport(t => t.UsePostgreSql(
                    connectionString,
                    "sales_events",      
                    "sales_events_error"
                    )
                )

                .Subscriptions(s => s.StoreInPostgres(
                    connectionString,
                    "sales_subscriptions")
                )

                .Routing(r => r.TypeBased()
                    .Map<SaleCreatedEvent>("sales_events")
                    .Map<SaleModifiedEvent>("sales_events")
                    .Map<SaleCancelledEvent>("sales_events")
                    .Map<ItemCancelledEvent>("sales_events"))

                .Logging(l => l.MicrosoftExtensionsLogging(
                    serviceProvider.GetRequiredService<ILoggerFactory>()))

                .Options(o =>
                {
                    o.SetNumberOfWorkers(1);
                    o.SetMaxParallelism(1);
                })
            );

            services.AddScoped<IEventPublisher, RebusEventPublisher>();

            return services;
        }
    }
}
