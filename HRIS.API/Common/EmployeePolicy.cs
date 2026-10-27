using HRIS.API.Common.Exceptions;
using HRIS.API.Entities;

namespace HRIS.API.Common;

public class EmployeePolicy(TimeProvider timeProvider)
{
    public const int ProbationMonths = 3;
    public const int MaxFutureHireDays = 90;

    public DateOnly Today =>
        DateOnly.FromDateTime(
            timeProvider.GetUtcNow().UtcDateTime);

    public bool IsOnProbation(DateOnly hireDate)
    {
        return Today < hireDate.AddMonths(ProbationMonths);
    }

    public int GetTenureMonths(DateOnly hireDate)
    {
        var months =
            (Today.Year - hireDate.Year) * 12
            + Today.Month - hireDate.Month;

        if (Today.Day < hireDate.Day)
        {
            months--;
        }

        return Math.Max(0, months);
    }

    public bool IsValidHireDate(DateOnly hireDate)
    {
        return hireDate <= Today.AddDays(MaxFutureHireDays);
    }

    public void EnsureCanManage(Employee manager)
    {
        if (manager.EmployeeStatus == EmployeeStatus.Terminated)
        {
            throw new BusinessRuleException(
                "A terminated employee cannot manage other employees.");
        }

        if (IsOnProbation(manager.HireDate))
        {
            throw new BusinessRuleException(
                "An employee on probation cannot manage other employees.");
        }
    }
}