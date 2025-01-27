using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.Common.HealthChecks;
using Ambev.DeveloperEvaluation.Common.Logging;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.IoC;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Ambev.DeveloperEvaluation.WebApi;

public partial class Program
{
    private static WebApplicationBuilder? _builder;

    public static void Main(string[] args)
    {
        try
        {
            Log.Information("Starting web application");
            var app = CreateApplication(args);
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
    public static WebApplication CreateApplication(string[] args = null)
    {
        args ??= Array.Empty<string>();

        _builder = WebApplication.CreateBuilder(args);
        ConfigureServices(_builder);

        var app = _builder.Build();
        ConfigureMiddleware(app);

        return app;
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.AddDefaultLogging();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.AddBasicHealthChecks();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<DefaultContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM")
            )
        );

        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.RegisterDependencies();

        builder.Services.AddAutoMapper(typeof(Program).Assembly,
                                     typeof(ApplicationLayer).Assembly);

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(
                typeof(ApplicationLayer).Assembly,
                typeof(Program).Assembly
            );
        });

        builder.Services.AddTransient(typeof(IPipelineBehavior<,>),
                                    typeof(ValidationBehavior<,>));
        builder.Services.AddEventPublishing(builder.Configuration);
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseBasicHealthChecks();
        app.MapControllers();
    }

    public static IHostBuilder CreateHostBuilder(string[] args = null)
    {
        args ??= Array.Empty<string>();

        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
