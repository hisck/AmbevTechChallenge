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
                b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.WebApi")
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
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();

                // Ensure the database is created and migrations are applied
                var pendingMigrations = context.Database.GetPendingMigrations().ToList();

                if (pendingMigrations.Any())
                {
                    Console.WriteLine("Pending Migrations:");
                    foreach (var migration in pendingMigrations)
                    {
                        Console.WriteLine($" - {migration}");
                    }

                    context.Database.Migrate();

                    Console.WriteLine("Database migrations applied successfully");
                    Log.Information("Database migrations applied successfully");
                }
                else
                {
                    Console.WriteLine("No pending migrations");
                    Log.Information("No pending migrations");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration Error: {ex.Message}");
                Console.WriteLine($"Full Error: {ex}");
                Log.Error(ex, "An error occurred while applying database migrations");
                throw;
            }
        }
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
