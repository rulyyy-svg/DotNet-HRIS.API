namespace HRIS.API.Common.Security;

public interface ICurrentUser
{
    Guid UserId { get; }

    Guid? EmployeeId { get; }

    bool IsInRole(string role);

    bool IsHrOrAdmin =>
        IsInRole(Roles.Hr) || IsInRole(Roles.Admin);
}