namespace HRIS.API.Common.Security;

public static class RoleIds
{
    public static readonly Guid Admin =
        Guid.Parse("0199f000-0000-7000-8000-0000000000a1");

    public static readonly Guid Hr =
        Guid.Parse("0199f000-0000-7000-8000-0000000000a2");

    public static readonly Guid Manager =
        Guid.Parse("0199f000-0000-7000-8000-0000000000a3");

    public static readonly Guid Employee =
        Guid.Parse("0199f000-0000-7000-8000-0000000000a4");
}