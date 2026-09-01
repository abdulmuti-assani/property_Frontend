using RealEstate.Domain.Common;

namespace RealEstate.Domain.Entities;

public class Favorite : AuditableEntity
{
    public Favorite()
    {
    }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
}
