using HRIS.API.Common.Exceptions;
using HRIS.API.Common.Security;

namespace HRIS.API.Features.Employees;

public static class EmployeeAccess
{
    public static void EnsureCanView(
        ICurrentUser currentUser,
        Guid employeeId,
        Guid? employeeManagerId)
    {
        // HR dan Admin dapat melihat semua Employee
        if (currentUser.IsHrOrAdmin)
            return;

        var myEmployeeId = currentUser.EmployeeId;

        // Employee dapat melihat dirinya sendiri.
        // Manager dapat melihat direct reports-nya.
        if (myEmployeeId is not null &&
            (myEmployeeId == employeeId ||
             myEmployeeId == employeeManagerId))
        {
            return;
        }

        throw new ForbiddenAccessException(
            "You can only view your own profile or your direct reports.");
    }
}