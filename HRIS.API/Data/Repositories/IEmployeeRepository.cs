using HRIS.API.Entities;

namespace HRIS.API.Data.Repositories
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<IReadOnlyCollection<EmployeeResponse>> GetResponseAsync(CancellationToken ct = default);
        Task<EmployeeResponse?> GetResponseAsync(Guid id, CancellationToken ct = default);

        Task<IReadOnlyCollection<Employee>> GetDirectReportsAsync(Guid managerId, CancellationToken ct = default);
    }
}
