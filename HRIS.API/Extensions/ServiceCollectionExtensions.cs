
using HRIS.API.Data;
using HRIS.API.Data.Repositories;
using HRIS.API.Services;
using Microsoft.EntityFrameworkCore;

namespace HRIS.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHrisServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("HrisDB");

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
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();

        return services;
    }
}
