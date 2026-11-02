
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

    builder.Services.AddControllers(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter()));

    builder.Services.AddHrisServices(builder.Configuration);

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblyContaining<Program>();
        cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    builder.Services.AddMiddleware();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            policy.AllowAnyOrigin();
        });
    });

    builder.Services.AddSingleton<TimeProvider>(
        TimeProvider.System);

    builder.Services.AddSingleton<EmployeePolicy>();

    // Authentication
    // Basic Authentication Handler akan didaftarkan pada Lesson 5.
    builder.Services.AddAuthentication();

    builder.Services.AddOpenApi();

    var app = builder.Build();

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

    // Authentication: mengenali user
    app.UseAuthentication();

    // User context logging
    app.UseMiddleware<UserContextLoggingMiddleware>();

    // Authorization: memeriksa izin user
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                "/openapi/v1.json",
                "HRIS API");
        });
    }

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
