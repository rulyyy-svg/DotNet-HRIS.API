using HRIS.API.Data;
using HRIS.API.Entities;
using HRIS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRIS.API.Services
{
    public class EFEmployeeService(
        HrisDbContext context,
        TimeProvider timeProvider) : IEmployeeService
    {
        public async Task<IReadOnlyCollection<EmployeeResponse>> GetAllAsync(
            CancellationToken ct = default)
        {
            var employees = await context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Manager)
                .OrderBy(e => e.FullName)
                .ToListAsync(ct);

            return employees
                .Select(e => e.ToResponse())
                .ToList();
        }

        public async Task<EmployeeResponse?> GetByIdAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var employee = await context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.Id == id, ct);

            return employee?.ToResponse();
        }

        public async Task<Guid> CreateAsync(
            CreateEmployeeRequest request,
            CancellationToken ct = default)
        {
            var employee = new Employee
            {
                Id = Guid.CreateVersion7(),
                EmployeeNumber = request.EmployeeNumber,
                FullName = request.FullName,
                Email = request.Email,
                HireDate = request.HireDate,
                CreatedAt = timeProvider.GetUtcNow(),
                DepartmentId = request.DepartmentId,
                PositionId = request.PositionId,
                ManagerId = request.ManagerId
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync(ct);

            return employee.Id;
        }

        public async Task<bool> UpdateAsync(
            Guid id,
            UpdateEmployeeRequest request,
            CancellationToken ct = default)
        {
            var employee = await context.Employees.FindAsync([id], ct);

            if (employee is null)
                return false;

            if (request.DepartmentId is not Guid departmentId ||
                request.PositionId is not Guid positionId)
            {
                throw new ArgumentException(
                    "DepartmentId dan PositionId wajib diisi.");
            }

            employee.FullName = request.FullName;
            employee.DepartmentId = departmentId;
            employee.PositionId = positionId;
            employee.ManagerId = request.ManagerId;

            await context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> DeleteAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var employee = await context.Employees.FindAsync([id], ct);

            if (employee is null)
                return false;

            context.Employees.Remove(employee);
            await context.SaveChangesAsync(ct);

            return true;
        }
    }
}