using HRIS.API.Common;
using HRIS.API.Common.Behaviors;
using HRIS.API.Extensions;
using HRIS.API.Middlewares;
using MediatR;
using Serilog;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting HRIS API");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog configuration
    builder.AddHrisSerilog();

    // Controllers dan JSON configuration
    builder.Services.AddControllers(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter()));

    // Application services
    builder.Services.AddHrisServices(builder.Configuration);

    // Security: Password hashing dan JWT Bearer
    builder.Services.AddHrisSecurity(builder.Configuration);

    // OpenAPI configuration
    builder.Services.AddHrisOpenApi();

    // MediatR dan pipeline behaviors
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblyContaining<Program>();
        cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    // Custom middleware
    builder.Services.AddMiddleware();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            policy.AllowAnyOrigin();
        });
    });

    // Time provider
    builder.Services.AddSingleton<TimeProvider>(
        TimeProvider.System);

    // Authorization policy
    builder.Services.AddSingleton<EmployeePolicy>();

    var app = builder.Build();

    // Development Admin Seeder
    if (app.Environment.IsDevelopment())
    {
        await app.SeedBootstrapAdminAsync();
    }

    // Global exception handler
    app.UseExceptionHandler();

    // Request monitoring
    app.UseRequestTiming();
    app.UseHrisRequestLogging();

    // Maintenance mode
    app.UseMaintenanceMode();

    // OpenAPI dan Swagger UI
    app.UseHrisOpenApi();

    // HTTPS redirection
    app.UseHttpsRedirection();

    // CORS
    app.UseCors();

    // Authentication
    app.UseAuthentication();

    // User context logging
    app.UseMiddleware<UserContextLoggingMiddleware>();

    // Authorization
    app.UseAuthorization();

    // Map API controllers
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "HRIS API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}