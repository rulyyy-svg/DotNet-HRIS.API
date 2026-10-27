
using HRIS.API.Entities;

namespace HRIS.API.Data.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<IReadOnlyCollection<Employee>> GetDirectReportsAsync(
        Guid managerId,
        CancellationToken ct = default);

    Task<bool> EmployeeNumberExistsAsync(
    string employeeNumber,
    CancellationToken ct = default);

    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken ct = default);
}
