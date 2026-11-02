namespace HRIS.API.Common.Security;

public static class Policies
{
    public const string HrOnly = "HrOnly";
    public const string HasEmployeeProfile = "HasEmployeeProfile";
}

public static class HrisClaimTypes
{
    public const string EmployeeId = "employee_id";
}