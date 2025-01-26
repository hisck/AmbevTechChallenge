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