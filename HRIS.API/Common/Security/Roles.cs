namespace HRIS.API.Common.Security;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Hr = "HR";
    public const string Manager = "Manager";
    public const string Employee = "Employee";

    public const string HrOrAdmin = Admin + "," + Hr;
}