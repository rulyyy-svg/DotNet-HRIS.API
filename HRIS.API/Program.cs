
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

    // Security: Password hashing dan Basic Authentication
    builder.Services.AddHrisSecurity();

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

    // OpenAPI
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Development Admin Seeder
    if (app.Environment.IsDevelopment())
    {
        await app.SeedBootstrapAdminAsync();
    }

    // Global exception handler
    app.UseExceptionHandler();

    app.UseHttpsRedirection();

    // CORS
    app.UseCors();

    // Request monitoring
    app.UseRequestTiming();
    app.UseHrisRequestLogging();

    // Maintenance mode
    app.UseMaintenanceMode();

    // Authentication
    app.UseAuthentication();

    // User context logging
    app.UseMiddleware<UserContextLoggingMiddleware>();

    // Authorization
    app.UseAuthorization();

    // OpenAPI dan Swagger UI
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi().AllowAnonymous();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                "/openapi/v1.json",
                "HRIS API");
        });
    }

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
