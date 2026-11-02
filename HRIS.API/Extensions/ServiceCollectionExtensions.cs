
using HRIS.API.Common.ExceptionHandlers;
using HRIS.API.Data;
using HRIS.API.Data.Repositories;
using HRIS.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using HRIS.API.Services.Email;
namespace HRIS.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHrisServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("HrisDB");

        services.AddDbContext<HrisDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton(TimeProvider.System);

        services.AddTransient<ITransientOperation, Operation>();
        services.AddScoped<IScopedOperation, Operation>();
        services.AddSingleton<ISingletonOperation, Operation>();
        services.AddScoped<OperationReport>();

        services.AddScoped(
            typeof(IRepository<>),
            typeof(Repository<>));

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Module 9: Email Configuration
        services.Configure<MailSettings>(
            configuration.GetSection(MailSettings.SectionName)
        );

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                var problem = context.ProblemDetails;

                problem.Instance ??=
                    context.HttpContext.Request.Path.Value;

                if (context.HttpContext.Items.TryGetValue(
                    "X-Correlation-Id",
                    out var correlationId))
                {
                    problem.Extensions["correlationId"] =
                        correlationId?.ToString();
                }

                if (problem is ValidationProblemDetails &&
    !problem.Extensions.ContainsKey(ProblemDetailsWriter.ErrorCodeKey))
                {
                    problem.Extensions[ProblemDetailsWriter.ErrorCodeKey] =
                        "MALFORMED_REQUEST";
                }
            };
        });

        // Module 9: Email Service
        services.AddTransient<IEmailService, MailKitEmailService>();
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        return services;
    }
}
