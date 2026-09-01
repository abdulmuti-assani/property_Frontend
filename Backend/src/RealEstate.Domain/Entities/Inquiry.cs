using RealEstate.Domain.Common;

namespace RealEstate.Domain.Entities;

public class Inquiry : AuditableEntity
{
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    public int BuyerId { get; set; }
    public User Buyer { get; set; } = null!;

    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }

    public Inquiry() { }
}
