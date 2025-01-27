using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.WebApi.Middleware;
using Microsoft.AspNetCore.Builder;
using Ambev.DeveloperEvaluation.Common.Events;
using Ambev.DeveloperEvaluation.Domain.Events;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Microsoft.Extensions.Logging;
using Rebus.Transport.InMem;
using Rebus.Persistence.InMem;

namespace Ambev.DeveloperEvaluation.Shared
{
    public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DefaultContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var databaseName = $"TestDB_{Guid.NewGuid()}";
                services.AddDbContext<DefaultContext>(options =>
                {
                    options.UseInMemoryDatabase(databaseName)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
                });

                var mediatrDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (mediatrDescriptor != null)
                {
                    services.Remove(mediatrDescriptor);
                }

                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssemblies(
                        typeof(ApplicationLayer).Assembly,
                        typeof(TProgram).Assembly
                    ));

                services.AddScoped<ISaleRepository, SaleRepository>();
                services.AddAutoMapper(typeof(TProgram).Assembly,
                                     typeof(ApplicationLayer).Assembly);

                services.AddControllers()
                    .AddApplicationPart(typeof(TProgram).Assembly);

                services.AddEndpointsApiExplorer();

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DefaultContext>();
                db.Database.EnsureCreated();

                var rebusDescriptors = services.Where(
                    d => d.ServiceType == typeof(Rebus.Subscriptions.ISubscriptionStorage)
                ).ToList();

                foreach (var descript in rebusDescriptors)
                {
                    services.Remove(descript);
                }

                var network = new InMemNetwork();

                services.AddRebus((configure, sp) => configure
                    .Transport(t => t.UseInMemoryTransport(
                        network,
                        "test_sales_events"
                    ))

                    .Subscriptions(s => { })

                    .Routing(r => r.TypeBased()
                        .Map<SaleCreatedEvent>("test_sales_events")
                        .Map<SaleModifiedEvent>("test_sales_events")
                        .Map<SaleCancelledEvent>("test_sales_events")
                        .Map<ItemCancelledEvent>("test_sales_events"))
                    .Logging(l => l.MicrosoftExtensionsLogging(
                        sp.GetRequiredService<ILoggerFactory>()))
                    .Options(o =>
                    {
                        o.SetNumberOfWorkers(1);
                        o.SetMaxParallelism(1);
                    })
                );

                services.AddSingleton<Rebus.Subscriptions.ISubscriptionStorage>(
                    _ => new InMemorySubscriptionStorage()
                );
                services.AddScoped<IEventPublisher, RebusEventPublisher>();
            });

            builder.Configure(app =>
            {
                app.UseMiddleware<ErrorHandlingMiddleware>();
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });
        }
    }
}