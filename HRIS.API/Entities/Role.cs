namespace HRIS.API.Entities;

public class Role
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public ICollection<UserRole> UserRoles { get; set; } = [];
}